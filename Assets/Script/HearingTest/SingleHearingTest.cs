using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using EXP.Sound;
public class SingleHearingTest : MonoBehaviour
{
    string participantName;
    [SerializeField] SoundPlayer soundPlayer;
    [SerializeField] Image soundImage;
    [SerializeField] Image detectImage;
    [SerializeField] Image inputImage;
    [SerializeField] TextMeshProUGUI interfaceSettingText;
    [SerializeField] TextMeshProUGUI volumeText;
    [SerializeField] TextMeshProUGUI frequencyText;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] float soundTime = 2f;
    [SerializeField] float inputWaitTime = 2f;
    [SerializeField] float intervalTime = 2f;
    [SerializeField] Hughson_westlake algorithm;
    [SerializeField] TestCondition testCondition;
    TestPhase testPhase = TestPhase.idle;
    float volumeAtStep0 = 40;
    float stepMultiplier = 5; // change 5db per step
    [SerializeField]float currentFrequency = 500;
    float inputWaitTimer = 0f;
    float intervalTimer = 0f;
    bool isDetected = false;
    UnityEvent expStart = new UnityEvent();
    UnityEvent expEnd = new UnityEvent();
    private enum TestPhase
    {
        idle,
        soundPlay,
        inputWait,
        trialInterval,
        End
    }
    public void InitializeTest(string participantName, float frequency, TestCondition condition)
    {
        this.participantName = participantName;
        currentFrequency = frequency;
        testCondition = condition;
    }
    public void AddStartListener(UnityAction action)
    {
        expStart.AddListener(action);
    }
    public void AddEndListener(UnityAction action)
    {
        expEnd.AddListener(action);
    }
    private void Awake()
    {
        InitializeSoundPlayer();
    }
    private void InitializeSoundPlayer()
    {
        soundPlayer.AddStartListener(() => soundImage.enabled = true);
        soundPlayer.AddEndListener(() =>
        {
            soundImage.enabled = false;
            ChangeState(TestPhase.inputWait);
        });
    }
    private void UpdateTexts()
    {
        volumeText.text = "Current Volume : " + GetVolume().ToString();
        frequencyText.text = "Current frequency : " + currentFrequency.ToString();
        string settingText = "N/A";
        float volumeDifference;
        switch (testCondition){
            case TestCondition.ACV:
                settingText = "ACV ON\n BCV OFF";
                break;
            case TestCondition.BcvWith2PoleNoise_ACV:
                settingText = "ACV ON\n 2 Pole Noisy BCV ON\n";
                break;
            case TestCondition.BcvWith2PoleNative:
                settingText = "Mute ACV\n 4 Pole Native BCV ON\n";
                volumeDifference = BcvCalibrationFinder.FindCalibrationDifference(participantName, currentFrequency, false);
                settingText += "Set BCV volume difference as " + volumeDifference.ToString() + "dB with ACV";
                break;
            case TestCondition.BcvWith4PoleNative:
                settingText = "Mute ACV\n 4 Pole Native BCV ON\n";
                volumeDifference = BcvCalibrationFinder.FindCalibrationDifference(participantName, currentFrequency, false);
                settingText += "Set BCV volume difference as " + volumeDifference.ToString() + "dB with ACV";
                break;
            case TestCondition.BcvWith2PoleNative_ACV:
                settingText = "ACV ON\n 2 Pole Native BCV ON\n";
                volumeDifference = BcvCalibrationFinder.FindCalibrationDifference(participantName, currentFrequency, true);
                settingText += "Set BCV volume difference as " + volumeDifference.ToString() + "dB with ACV";
                break;
            default:
                Debug.LogWarning("There is unexpected test condition! Please add instruction.");
                break;

        }
        interfaceSettingText.text = settingText;
    }

    private void Update()
    {
        UpdateTexts();
        detectImage.enabled = isDetected;
        switch (testPhase)
        {
            case TestPhase.idle:
                break;
            case TestPhase.soundPlay:
                isDetected = DetectInput() || isDetected;
                break;
            case TestPhase.inputWait:
                isDetected = DetectInput() || isDetected;
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
                        ChangeState(TestPhase.End);
                        EndTest();
                    }
                    else
                    {
                        ChangeState(TestPhase.soundPlay);
                    }
                }
                break;
            case TestPhase.End:
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
                soundPlayer.PlaySound(currentFrequency, soundTime, GetVolume(),true);
                break;
            case TestPhase.inputWait:
                inputWaitTimer = 0f;
                break;
            case TestPhase.trialInterval:
                intervalTimer = 0f;
                algorithm.SubmitTrialResult(isDetected);
                isDetected = false;
                break;
            case TestPhase.End:
                break;
        }
    }
    public void StartTest()
    {
        algorithm.Initialize(10);
        ChangeState(TestPhase.soundPlay);
        expStart.Invoke();
    }
    private float GetVolume()
    {
        float offset = 0f;
        switch (testCondition)
        {
            case TestCondition.ACV:
                offset = 0f;
                break;
            case TestCondition.BcvWith2PoleNoise_ACV:
                offset = 0f;
                break;
            case TestCondition.BcvWith2PoleNative_ACV:
                offset = -10f * Mathf.Log10(2f);//half of sound volume.
                break;
            case TestCondition.BcvWith4PoleNative:
                offset = 0f;
                break;
            case TestCondition.BcvWith2PoleNative:
                offset = 0f;
                break;
            default:
                Debug.LogWarning("Not considered in condition for volume offset");
                break;
        }
        return algorithm.currentStepValue * stepMultiplier + volumeAtStep0 + offset;
    }
    private bool DetectInput()
    {
        bool hasInput = Input.GetKey(KeyCode.Space);
        inputImage.enabled = hasInput;
        return hasInput;
    }
    private void EndTest()
    {
        if (algorithm.StatusCode != 0)
        {
            resultText.text = "Problem occur : can't find threshold";
        }
        else
        {
            resultText.text = "Threshold step : " + algorithm.GetThreshold().ToString() + ".\nThreshold decibel = " + GetThershold().ToString();
        }
        expEnd.Invoke();
    }

    public void GetLog(string fileSuffix)
    {
        var log = algorithm.GetExpLog();
        TableBuilder tableBuilder = new TableBuilder(TableBuilder.AddMode.column);
        tableBuilder.Add("Decibel", "Correct");
        foreach(var logInfo in log)
        {
            tableBuilder.Add(ConvertStepToVolume(logInfo.Item1), logInfo.Item2);
        }
        string dateTimePattern = "yyyy_MM_dd_H_mm";
        CsvWriter.WriteCSV(tableBuilder.GetTable(), "./logs/" + fileSuffix + DateTime.Now.ToString(dateTimePattern) + "_" + currentFrequency + "hz.csv");
    }

    public  bool IsTestEndNomally()
    {
        return algorithm.StatusCode == 0 && algorithm.IsDone();
    }
    public float GetThershold()
    {
        return ConvertStepToVolume(algorithm.GetThreshold());
    }
    private float ConvertStepToVolume(int step)
    {
        return step * stepMultiplier + volumeAtStep0;
    }
}
