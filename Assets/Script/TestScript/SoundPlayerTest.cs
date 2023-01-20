using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayerTest : MonoBehaviour
{
    [SerializeField] SoundPlayer soundPlayer;
    [SerializeField] float volume;
    [SerializeField] float frequency;
    [SerializeField] float duration;
    public void PlaySound()
    {
        soundPlayer.PlaySound(frequency, duration, volume);
    }
}
