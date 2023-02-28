using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace EXP.Sound
{
    public class BcvCalibrator : MonoBehaviour
    {
        public static string calibrationFileSuffix
        {
            get { return mCalibrationFileSuffix; }
            private set { mCalibrationFileSuffix = value; }
        }
        private static string mCalibrationFileSuffix = "BcvSoundCalibration.csv";
        [SerializeField] string participantName;
        [SerializeField] private List<float> frequencyList = new List<float>();
        [SerializeField] private List<(float, bool)> calibrationList = new List<(float, bool)>();//frequency, isMastoid
        private List<float> calibrationResult;
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private TextMeshProUGUI frequencyInfo;
        [SerializeField] private TextMeshProUGUI participantNameInfo;
        [SerializeField] private TMP_InputField inputField;
        private bool isCalibrating = false;
        private int countIndex = 0;
        public void StartCalibration()
        {
            InitializeCalibrationList();
            calibrationResult = new List<float>(frequencyList.Count);
            countIndex = 0;
            CalibrateOnIndex();
        }

        private void InitializeCalibrationList()
        {
            foreach (float frequency in frequencyList)
            {
                calibrationList.Add((frequency, true));
            }
            foreach (float frequency in frequencyList)
            {
                calibrationList.Add((frequency, false));
            }
        }

        private void CalibrateOnIndex()
        {
            var currentCalibration = calibrationList[countIndex];
            float currentFrequency = currentCalibration.Item1;
            bool isMastoid = currentCalibration.Item2;
            string testTarget = isMastoid ? "mastoid" : "condyle";
            frequencyInfo.text = "Current frequency : " + currentFrequency.ToString() + "  \nTarget : " + testTarget;
            participantNameInfo.text = "Current Participant : " + participantName;
            soundPlayer.PlaySound(currentFrequency, float.MaxValue, 50f, true);
            isCalibrating = true;
        }
        public void SubmitResult()
        {
            if (isCalibrating)
            {
                StoreResult();
                soundPlayer.stopSound();
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
            TableBuilder Table2Pole = new TableBuilder(TableBuilder.AddMode.column);
            TableBuilder Table4Pole = new TableBuilder(TableBuilder.AddMode.column);
            Table2Pole.Add("frequency(hz)", "volumeDifference(db)");
            Table4Pole.Add("frequency(hz)", "volumeDifference(db)");
            for (int i = 0; i < calibrationList.Count; i = i + 1)
            {
                if (calibrationList[i].Item2)
                {
                    Table2Pole.Add(calibrationList[i].Item1.ToString(), calibrationResult[i]);
                }
                else
                {
                    Table4Pole.Add(calibrationList[i].Item1.ToString(), calibrationResult[i]);
                }
            }
            CsvWriter.WriteCSV(Table2Pole.GetTable(), GetCalibrationFilePath(participantName,true));
            CsvWriter.WriteCSV(Table4Pole.GetTable(), GetCalibrationFilePath(participantName,false));
        }
        public static string GetCalibrationFilePath(string participantName, bool isMastoid)
        {
            string pole = isMastoid ? "Mastoid" : "Condyle";
            return "./BcvCalibration/" + participantName + pole + calibrationFileSuffix;
        }

        private void StoreResult()
        {
            float decibel = float.Parse(inputField.text);
            inputField.text = "";
            calibrationResult.Add(decibel);
        }
    }
    public class BcvCalibrationFinder
    {
        public static float FindCalibrationDifference(string participantName, float frequency, bool isMastoid)
        {
            string bcvCalibrationPath = BcvCalibrator.GetCalibrationFilePath(participantName,isMastoid);
            var result = CsvReader.ReadCSV(bcvCalibrationPath);
            int matchingIndex = -1;
             for(int index = 0; index < result[0].Length; index++)
            {
                bool isMatching = result[0][index].Equals(frequency.ToString());
                if (isMatching){
                    matchingIndex = index;
                }
            }
            return float.Parse(result[1][matchingIndex]);
        }
    }


}