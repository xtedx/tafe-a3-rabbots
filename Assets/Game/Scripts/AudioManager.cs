using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource hitFX;
    [SerializeField] private AudioSource dashFX;
    [SerializeField] private AudioSource runFX;
    [SerializeField] private AudioSource boingFX;
    private bool isRunning = false;
    
    public void playHitFx(bool isPlay)
    {
        if (isPlay)
        {
            hitFX.Play();
        }
        else
        {
            hitFX.Stop();
        }
    }

    public void playRunFx(bool isPlay)
    {
        if (isRunning && isPlay) return;
        if (!isRunning && isPlay)
        {
            runFX.Play();
            isRunning = true;
        }
        if (!isPlay)
        {
            runFX.Stop();
            isRunning = false;
        }
    }
    
    public void playDashFx(bool isPlay)
    {
        if (isPlay)
        {
            isRunning = true;
            dashFX.Play();
        }
        else
        {
            isRunning = false;
            dashFX.Stop();
        }
    }
    
    public void playBoingFx(bool isPlay)
    {
        if (isPlay)
        {
            boingFX.Play();
        }
        else
        {
            boingFX.Stop();
        }
    }
    
}
