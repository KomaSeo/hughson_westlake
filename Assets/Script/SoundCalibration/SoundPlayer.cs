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
            SetFrequency(frequency);
            SetVolume(dbVolume);
            SetDuration(duration);
            StartSound();
            soundStart.Invoke();
        }
        public void PlaySoundOnRatio(float frequency, float duration, float linearVolume)
        {
            SetFrequency(frequency);
            SetVolume(MaxSoundVolume);
            SetDuration(duration);
            StartSound();
            soundStart.Invoke();
        }
        private void StartSound()
        {
            audioPlayer.Play();
            audioTimer.startTimer();

        }
        private void SetDuration(float duration)
        {
            audioTimer.Initialize(duration);
            audioTimer.AddExpireListener(stopSound);
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
            audioPlayer.clip = SineWaveGenerator.MakeSound(frequency);
            float audio_frequency_ratio = frequency / 500f;
            audioPlayer.pitch = audio_frequency_ratio;
        }

        public void stopSound()
        {
            audioTimer.Initialize(0);
            audioPlayer.Stop();
            soundEnd.Invoke();
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
