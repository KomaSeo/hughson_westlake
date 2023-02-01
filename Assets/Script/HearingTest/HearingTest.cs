using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
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
    [SerializeField] List<float> experimentList = new List<float>();
    [SerializeField] string singleHearingTestScenePath;
    [SerializeField] string hearingTestScenePath;
    List<(float, float)> resultList = new List<(float, float)>();
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
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
    public void LoadTest(float frequency)
    {
        StartCoroutine(LoadScene(frequency));
    }
    IEnumerator LoadScene(float frequency)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(singleHearingTestScenePath);
        yield return asyncLoad;
        InitializeTest(frequency);
    }

    private void InitializeTest(float frequency)
    {
        SingleHearingTest test = FindObjectOfType<SingleHearingTest>();
        test.InitializeTest(frequency);
        test.AddEndListener(() =>
        {
            test.GetLog(participantName);
            if (test.IsTestEndNomally())
            {
                resultList.Add((frequency, test.GetThershold()));
            }
            else
            {
                Debug.LogWarning("Cannot find threshold!");
                resultList.Add((frequency, float.NaN));
            }
            SceneManager.LoadScene(hearingTestScenePath);
            SaveTest();
        });
    }

    public void SaveTest()
    {
        TableBuilder builder = new TableBuilder(TableBuilder.AddMode.column);
        builder.Add("frequency", "decibel");
        foreach(var result in resultList)
        {
            builder.Add(result.Item1, result.Item2);
        }
        string[,] table = builder.GetTable();
        CsvWriter.WriteCSV(table, "./experimentResult/" + participantName + ".csv");
    }
    private void AddTestButtons()
    {
        foreach(float frequency in experimentList)
        {
            GameObject newButton = new GameObject();
            Button buttonComponent = newButton.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() => LoadTest(frequency));
            newButton.AddComponent<TextMeshProUGUI>().text = frequency + "hz";
            newButton.transform.SetParent(buttonPanel.transform);
        }
    }
    private void UpdateInfo()
    {
        participantNameText.text = participantName;

        StringBuilder experimentInfo = new StringBuilder();
        experimentInfo.Append("Experiemnt List :");
        foreach(float frequency in experimentList)
        {
            experimentInfo.AppendFormat(" {0}hz", frequency.ToString());
        }
        experimentInfo.Append("\nCurrent held experiment :");
        foreach((float,float) result in resultList)
        {
            float frequency = result.Item1;
            experimentInfo.AppendFormat(" {0}hz", frequency.ToString());
        }
        experimentInfoText.text = experimentInfo.ToString();
    }
}
