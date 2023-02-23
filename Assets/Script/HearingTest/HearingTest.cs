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
        string[] column = new string[frequencyList.Count + 1];
        column[0] = "frequency/condition";
        for(int i = 1; i <= frequencyList.Count; i++)
        {
            column[i] = frequencyList[i-1].ToString();
        }
        builder.Add(column);
        foreach(TestCondition condition in System.Enum.GetValues(typeof(TestCondition)))
        {
            string[] conditionColumn = new string[frequencyList.Count + 1];
            conditionColumn[0] = condition.ToString();
            for (int i = 1; i <= frequencyList.Count; i++)
            {
                conditionColumn[i] = resultList.Find(
                (match) =>
                {
                    return isConditionEqual(match, (frequencyList[i-1], condition));
                }).Item2.ToString();
            }
            builder.Add(conditionColumn);
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
                    return isConditionEqual(match, testTarget);
                }
                ) )
            {
                buttonText.color = Color.black;
            }
            newButton.transform.SetParent(buttonPanel.transform);
        }
    }

    private static bool isConditionEqual(((float, TestCondition), float) condition1, (float, TestCondition) condition2)
    {
        var resultCondition = condition1.Item1;
        var frequency = resultCondition.Item1;
        var condition = resultCondition.Item2;
        return condition == condition2.Item2 && frequency == condition2.Item1;
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
