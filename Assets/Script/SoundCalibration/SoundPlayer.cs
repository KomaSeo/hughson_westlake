using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Events;
namespace EXP.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource audioPlayer;
        [SerializeField] float MaxSoundVolume = 60;
        UnityEvent soundStart = new UnityEvent();
        UnityEvent soundEnd = new UnityEvent();
        float smoothingTime = 0.3f;

        Timer audioTimer;
        private void Awake()
        {
            audioTimer = gameObject.AddComponent<Timer>();
            audioPlayer.loop = true;
            audioPlayer.playOnAwake = false;
            audioPlayer.Stop();
        }
        public void AddStartListener(UnityAction action)
        {
            soundStart.AddListener(action);
        }
        public void AddEndListener(UnityAction action)
        {
            soundEnd.AddListener(action);
        }
        public void PlaySound(float frequency, float duration, float dbVolume, bool useCalibration)
        {
            SetMaxVolume(frequency, useCalibration);
            SetFrequency(frequency);
            SetVolume(dbVolume);
            SetDuration(duration);
            StartSoundWithSmoothing();
            soundStart.Invoke();
        }

        private void SetMaxVolume(float frequency, bool useCalibration)
        {
            if (useCalibration)
            {
                string[][] frequencyInfo = CsvReader.ReadCSV(SoundCalibrator.calibrationFilePath);
                int arr = Array.FindIndex<string>(frequencyInfo[0], x => x.Equals(frequency.ToString()));
                if (arr < 0)
                {
                    Debug.LogErrorFormat("Can't find frequency volume info for {0}hz at file : {1}.", frequency, SoundCalibrator.calibrationFilePath);
                }
                else
                {
                    MaxSoundVolume = int.Parse(frequencyInfo[1][arr]);
                }
            }
        }

        public void PlaySoundOnRatio(float frequency, float duration, float linearVolume)
        {
            SetFrequency(frequency);
            SetVolume(MaxSoundVolume);
            SetDuration(duration);
            StartSoundWithSmoothing();
            soundStart.Invoke();
        }
        private IEnumerator SoundFadeIn()
        {
            audioPlayer.Play();
            float targetVolume = audioPlayer.volume;
            for (float time = 0f; time <= smoothingTime;)
            {
                audioPlayer.volume = time / smoothingTime * targetVolume;
                yield return null;
                time += Time.deltaTime;
            }
            audioPlayer.volume = targetVolume;
        }
        private IEnumerator SoundFadeOut()
        {
            float startVolume = audioPlayer.volume;
            for (float time = 0f; time <= smoothingTime;)
            {
                audioPlayer.volume = startVolume - time / smoothingTime * startVolume;
                yield return null;
                time += Time.deltaTime;
            }
            audioPlayer.volume = 0;
            audioPlayer.Stop();
            soundEnd.Invoke();

        }
        private void SetDuration(float duration)
        {
            audioTimer.Initialize(duration - smoothingTime);
            audioTimer.AddExpireListener(stopSoundWithSmoothing);
        }

        private void SetVolume(float dbVolume)
        {
            float linearVolumne = DecibelToLinear(dbVolume);
            float linearStdVolume = DecibelToLinear(MaxSoundVolume);
            float setVolume = linearVolumne / linearStdVolume;
            audioPlayer.volume = setVolume;
        }

        private void SetFrequency(float frequency)
        {
            audioPlayer.clip = SineWaveGenerator.MakeSound(500f);
            float audio_frequency_ratio = frequency / 500f;
            audioPlayer.pitch = audio_frequency_ratio;
        }

        private void StartSoundWithSmoothing()
        {
            audioTimer.startTimer();
            StartCoroutine(SoundFadeIn());

        }
        public void stopSoundWithSmoothing()
        {
            audioTimer.Initialize(0);
            Coroutine fadeoutCoroutine = StartCoroutine(SoundFadeOut());
        }
        private float LinearToDecibel(float linear)
        {
            float db;
            if (linear != 0)
            {
                db = 20.0f * Mathf.Log10(linear);
            }
            else
            {
                db = 144.0f;
            }
            return db;
        }
        private float DecibelToLinear(float dB)
        {
            float linear = Mathf.Pow(10.0f, dB / 20.0f);
            return linear;
        }
    }
}
