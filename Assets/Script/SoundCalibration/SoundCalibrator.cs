using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace EXP.Sound
{
    public class SoundCalibrator : MonoBehaviour
    {
        public static string calibrationFilePath { 
            get { return mCalibrationFilePath; }
            private set { mCalibrationFilePath = value;}
        }
        private static string mCalibrationFilePath = "./soundCalibration.csv";
        [SerializeField] private List<float> calibrationList = new List<float>();
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private TextMeshProUGUI frequencyInfo;
        [SerializeField] private TMP_InputField inputField;
        private List<float> calibrationResult;
        private bool isCalibrating = false;
        private int countIndex = 0;
        public void StartCalibration()
        {
            calibrationResult = new List<float>(calibrationList.Count);
            countIndex = 0;
            CalibrateOnIndex();
        }
        private void CalibrateOnIndex()
        {
            float currentFrequency = calibrationList[countIndex];
            frequencyInfo.text = "Current frequency : " + currentFrequency.ToString();
            soundPlayer.PlaySoundOnRatio(currentFrequency, float.MaxValue, 1f);
            isCalibrating = true;
        }
        public void SubmitResult()
        {
            if (isCalibrating)
            {
                StoreResult();
                soundPlayer.stopSoundWithSmoothing();
                countIndex++;
                if (countIndex < calibrationList.Count)
                {
                    CalibrateOnIndex();
                }
                else
                {
                    isCalibrating = false;
                    SaveResult();
                    frequencyInfo.text = "Calibration Finished";
                }
            }
        }

        private void SaveResult()
        {
            string[,] fileContent = new string[2, calibrationList.Count + 1];
            fileContent[0, 0] = "frequency(hz)";
            fileContent[1, 0] = "volume(db)";
            for (int i = 0; i < calibrationList.Count; i=i+1)
            {
                fileContent[0, i+1] = calibrationList[i].ToString();
                fileContent[1, i+1] = calibrationResult[i].ToString();
            }
            CsvWriter.WriteCSV(fileContent, calibrationFilePath);
        }

        private void StoreResult()
        {
            float decibel = float.Parse(inputField.text);
            inputField.text = "";
            calibrationResult.Add(decibel);
        }

        // Start is called before the first frame update

    }
}
