using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HearingTest : MonoBehaviour
{
    [SerializeField] SoundPlayer soundPlayer;
    [SerializeField] Image soundImage;
    [SerializeField] Image detectImage;
    [SerializeField] Image inputImage;
    [SerializeField] TextMeshProUGUI volumeText;
    [SerializeField] TextMeshProUGUI frequencyText;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] float soundTime = 2f;
    [SerializeField] float inputWaitTime = 2f;
    [SerializeField] float intervalTime = 2f;
    [SerializeField] Hughson_westlake algorithm;
    Timer timer;
    TestPhase testPhase = TestPhase.idle;
    float volumeAtStep0 = 40;
    float stepMultiplier = 5; // change 5db per step
    float currentFrequency = 500;
    float inputWaitTimer = 0f;
    float intervalTimer = 0f;
    bool isDetected = false;
    private enum TestPhase
    {
        idle,
        soundPlay,
        inputWait,
        trialInterval
    }
    private void Awake()
    {
        timer = gameObject.AddComponent<Timer>();
        soundPlayer.AddStartListener(() => soundImage.enabled = true);
        soundPlayer.AddEndListener(() => {
            soundImage.enabled = false;
            ChangeState(TestPhase.inputWait);
            });
    }
    private void Update()
    {
        volumeText.text = "Current Volume : " + getVolume().ToString();
        frequencyText.text = "Current frequency : " + currentFrequency.ToString();
        detectImage.enabled = isDetected;
        switch (testPhase)
        {
            case TestPhase.idle:
                break;
            case TestPhase.soundPlay:
                isDetected = detectInput() || isDetected;
                break;
            case TestPhase.inputWait:
                isDetected = detectInput() || isDetected;
                inputWaitTimer += Time.deltaTime;
                if(inputWaitTimer >= inputWaitTime)
                {
                    ChangeState(TestPhase.trialInterval);
                }
                break;
            case TestPhase.trialInterval:
                intervalTimer += Time.deltaTime;
                if(intervalTimer >= intervalTime)
                {
                    if (algorithm.IsDone())
                    {
                        ChangeState(TestPhase.idle);
                        EndTest();
                    }
                    else
                    {
                        ChangeState(TestPhase.soundPlay);
                    }
                }
                break;

        }
    }
    private void ChangeState(TestPhase phase)
    {
        testPhase = phase;
        switch (phase)
        {
            case TestPhase.idle:
                break;
            case TestPhase.soundPlay:
                soundPlayer.PlaySound(currentFrequency, soundTime, getVolume());
                break;
            case TestPhase.inputWait:
                inputWaitTimer = 0f;
                break;
            case TestPhase.trialInterval:
                intervalTimer = 0f;
                algorithm.SubmitTrialResult(isDetected);
                isDetected = false;
                break;
        }
    }
    public void StartTest()
    {
        algorithm.Initialize(10);
        ChangeState(TestPhase.soundPlay);
    }
    private float getVolume()
    {
        return algorithm.currentStepValue * stepMultiplier + volumeAtStep0;
    }
    private bool detectInput()
    {
        bool hasInput = Input.GetKey(KeyCode.Space);
        inputImage.enabled = hasInput;
        return hasInput;
    }
    private void EndTest()
    {
        if(algorithm.StatusCode != 0)
        {
            resultText.text = "Problem occur : can't find threshold";
        }
        else
        {
            resultText.text = "Threshold step : " + algorithm.GetThreshold().ToString() + ".\nThreshold decibel = " + (algorithm.GetThreshold() * stepMultiplier + volumeAtStep0).ToString();
        }
    }
}
