using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class HearingTest : MonoBehaviour
{
    private static HearingTest instance;
    [SerializeField] string participantName;
    [SerializeField] TextMeshProUGUI participantNameText;
    [SerializeField] GameObject buttonPanel;
    [SerializeField] TextMeshProUGUI experimentInfoText;
    [SerializeField] List<float> frequencyList = new List<float>();
    private List<(float,TestCondition)> experimentList_ = new List<(float, TestCondition)>();
    [SerializeField] string singleHearingTestScenePath;
    [SerializeField] string hearingTestScenePath;
    List<((float, TestCondition),float)> resultList = new List<((float,TestCondition),float)>();//(frequency,condition),result
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            InitializeExperimentList();
            UpdateInfo();
            AddTestButtons();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance.buttonPanel = this.buttonPanel;
            instance.participantNameText = this.participantNameText;
            instance.experimentInfoText = this.experimentInfoText;
            instance.AddTestButtons();
            instance.UpdateInfo();
            instance.SaveTest();
            Destroy(this);
        }
    }
    private void InitializeExperimentList()
    {
        foreach(TestCondition condition in Enum.GetValues(typeof(TestCondition)))
        {
            foreach(float frequency in frequencyList)
            {
                experimentList_.Add((frequency, condition));
            }
        }
    }
    public void LoadTest(float frequency,TestCondition condition)
    {
        StartCoroutine(LoadScene(frequency,condition));
    }
    IEnumerator LoadScene(float frequency,TestCondition condition)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(singleHearingTestScenePath);
        yield return asyncLoad;
        InitializeTest(frequency,condition);
    }

    private void InitializeTest(float frequency,TestCondition condition)
    {
        SingleHearingTest test = FindObjectOfType<SingleHearingTest>();
        test.InitializeTest(participantName,frequency,condition);
        test.AddEndListener(() =>
        {
            test.GetLog(participantName);
            if (test.IsTestEndNomally())
            {
                resultList.Add(((frequency,condition), test.GetThershold()));
            }
            else
            {
                Debug.LogWarning("Cannot find threshold!");
                resultList.Add(((frequency,condition), float.NaN));
            }
            SceneManager.LoadScene(hearingTestScenePath);
            SaveTest();
        });
    }

    public void SaveTest()
    {
        TableBuilder builder = new TableBuilder(TableBuilder.AddMode.column);
        builder.Add("frequency + condition", "decibel");
        foreach(var result in resultList)
        {
            builder.Add(result.Item1, result.Item2);
        }
        string[,] table = builder.GetTable();
        CsvWriter.WriteCSV(table, "./experimentResult/" + participantName + ".csv");
    }
    private void AddTestButtons()
    {
        foreach(var testTarget in experimentList_)
        {
            GameObject newButton = new GameObject();
            Button buttonComponent = newButton.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() => LoadTest(testTarget.Item1,testTarget.Item2));
            TextMeshProUGUI buttonText = newButton.AddComponent<TextMeshProUGUI>();
            buttonText.text = testTarget.Item1.ToString() + "hz_" + testTarget.Item2.ToString();
            if(resultList.Exists(
                (match)=>
                {
                    var resultCondition = match.Item1;
                    var frequency = resultCondition.Item1;
                    var condition = resultCondition.Item2;
                    return condition == testTarget.Item2 && frequency == testTarget.Item1;
                }
                ) )
            {
                buttonText.color = Color.black;
            }
            newButton.transform.SetParent(buttonPanel.transform);
        }
    }
    private void UpdateInfo()
    {
        participantNameText.text = participantName;

        StringBuilder experimentInfo = new StringBuilder();
        experimentInfo.Append("Experiment List :");
        foreach (var testList in experimentList_)
        {
            experimentInfo.AppendFormat(" {0}hz_{1}\n", testList.Item1.ToString(), testList.Item2.ToString());
        }
        experimentInfo.Append("\nCurrent held experiment :");
        foreach(var result in resultList)
        {
            var testCondition = result.Item1;
            experimentInfo.AppendFormat(" {0}hz_{1}\n", testCondition.Item1.ToString(),testCondition.Item2.ToString());
        }
        experimentInfoText.text = experimentInfo.ToString();
    }
}
