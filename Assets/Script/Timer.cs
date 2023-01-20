using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    float maxTime = 0f;
    float currentTime = 0f;
    bool isOn;
    UnityEvent expireEvent;
    public void Initialize(float time)
    {
        expireEvent = new UnityEvent();
        currentTime = 0f;
        maxTime = time;
    }
    public void startTimer()
    {
        isOn = true;
    }
    public void AddExpireListener(UnityAction action)
    {
        expireEvent.AddListener(action);
    }

    private void FixedUpdate()
    {
        if (isOn)
        {
            currentTime += Time.fixedDeltaTime;
            if(maxTime <= currentTime)
            {
                Expire();
            }
        }
    }
    private void Expire()
    {
        isOn = false;
        expireEvent.Invoke();
    }
}
