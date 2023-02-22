using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace EXP.Sound
{
    public class SineWaveGenerator
    {
        static int sampleRate = 44100;
        static float soundLengthInSeconds = 2f;
        public static AudioClip MakeSound(float frequency)
        {
            AudioClip audioClip = AudioClip.Create("SineWave" + frequency.ToString(), (int)(sampleRate * soundLengthInSeconds), 1, sampleRate, false);
            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);
            for (int i = 0; i < audioClip.samples; i++)
            {
                data[i] = CreateSine(i, frequency, sampleRate);
            }
            audioClip.SetData(data, 0);
            return audioClip;
        }
        private static float CreateSine(int frame, float frequency, float sampleRate)
        {
            return Mathf.Sin(2 * Mathf.PI * frame * frequency / sampleRate);
        }
    }

}
