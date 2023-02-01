using System;
using System.Collections.Generic;
using UnityEngine;

public class Hughson_westlake : MonoBehaviour
{
    public int InitialJumpStepsUp { get; set; } = 4;
    public int InitialJumpStepsDown { get; set; } = 3;
    public int StepsUp { get; set; } = 1;
    public int StepsDown { get; set; } = 2;
    public int MinimumThresholdTrials { get; set; } = 3;
    public double MinimumPassingThreshold { get; set; } = 2.0/3.0;
    public bool ShortCircuit { get; set; } = true;

    public int MaxTrialsAtLimits { get; set; } = 1000;

    public int StatusCode { get; set; }
    List<(int, bool)> expLog;

    #region Handler

    private Phase phase;
    public int currentStepValue { get; private set; }
    private bool endTriggered;
    private int thresholdStep;
    private int cellingStep;

    private readonly Dictionary<int, ModifiedHughsonWestlakeStep> stepDictionary = new Dictionary<int, ModifiedHughsonWestlakeStep>();

    private enum Phase
    {
        InitialBigAscension = 0,
        InitialAscension,
        Descending,
        Ascending,
    }

    public void Initialize(int cellingStep)
    {
        stepDictionary.Clear();
        phase = Phase.InitialBigAscension;
        currentStepValue = 0;
        this.cellingStep = cellingStep;

        endTriggered = false;

        StatusCode = 0;
        expLog = new List<(int, bool)>();
    }

    public void SubmitTrialResult(bool correct)
    {
        expLog.Add((currentStepValue, correct));
        int stepDiff;

        switch (phase)
        {
            case Phase.InitialBigAscension:
                if (correct)
                {
                    stepDiff = -InitialJumpStepsDown;
                    phase = Phase.Descending;
                }
                else
                {
                    stepDiff = InitialJumpStepsUp;
                    phase = Phase.InitialAscension;
                }
                break;

            case Phase.InitialAscension:
                if (correct)
                {
                    stepDiff = -InitialJumpStepsDown;
                    phase = Phase.Descending;
                }
                else
                {
                    stepDiff = InitialJumpStepsUp;
                    phase = Phase.InitialAscension;
                }
                break;
            case Phase.Descending:
                if (correct)
                {
                    stepDiff = -InitialJumpStepsDown;
                    phase = Phase.Descending;
                }
                else
                {
                    stepDiff = StepsUp;
                    phase = Phase.Ascending;
                }
                break;

            case Phase.Ascending:
                //Add Trial
                if (!stepDictionary.ContainsKey(currentStepValue))
                {
                    stepDictionary.Add(currentStepValue, new ModifiedHughsonWestlakeStep());
                }

                stepDictionary[currentStepValue].AddTrial(correct);

                if (stepDictionary[currentStepValue].IsPassing(ShortCircuit, MinimumThresholdTrials, MinimumPassingThreshold))
                {
                    endTriggered = true;
                    thresholdStep = currentStepValue;
                }

                if (correct)
                {
                    stepDiff = -StepsDown;
                }
                else
                {
                    stepDiff = StepsUp;
                }
                break;

            default:
                UnityEngine.Debug.LogError($"Unexpected Phase: {phase}");
                goto case Phase.Descending;
        }

        bool canStep = currentStepValue + stepDiff<= cellingStep;

        if (canStep)
        {
            currentStepValue += stepDiff;
        }
        else
        {
            //Unable to step
            thresholdStep = currentStepValue;

            if (stepDiff > 0)
            {
                StatusCode = 1;
                endTriggered = true;
            }
            else
            {
                StatusCode = -1;
                endTriggered = true;
            }
        }
    }


    public  bool IsDone() => endTriggered;
    public int GetThreshold() => thresholdStep;
    public List<(int, bool)> GetExpLog() => expLog;

    private class ModifiedHughsonWestlakeStep
    {
        private int trials = 0;
        private int hits = 0;

        public ModifiedHughsonWestlakeStep() { }

        /// <summary>
        /// Adds a trial and conditionally a hit
        /// </summary>
        public void AddTrial(bool hit)
        {
            trials++;

            if (hit)
            {
                hits++;
            }
        }

        public bool IsPassing(bool shortCircuit, int minimumThresholdTrials, double minimumPassingThreshold) =>
            shortCircuit ?
                IsPassingInevitable(minimumThresholdTrials, minimumPassingThreshold) :
                IsPassingCurrently(minimumThresholdTrials, minimumPassingThreshold);


        private bool IsPassingCurrently(int minimumThresholdTrials, double minimumPassingThreshold) =>
            trials >= minimumThresholdTrials &&
            (hits / (double)trials) >= minimumPassingThreshold;

        private bool IsPassingInevitable(int minimumThresholdTrials, double minimumPassingThreshold) =>
            hits / (double)Math.Max(trials, minimumThresholdTrials) >= minimumPassingThreshold;
    }

    #endregion Handler
}