using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EXP.Sound;
namespace EXP.ScriptTest
{
    public class SoundPlayerTest : MonoBehaviour
    {
        [SerializeField] SoundPlayer soundPlayer;
        [SerializeField] float volume;
        [SerializeField] float frequency;
        [SerializeField] float duration;
        public void PlaySound()
        {
            soundPlayer.PlaySound(frequency, duration, volume, true);
        }
    }


}