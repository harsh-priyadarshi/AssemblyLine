using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AL.Gameplay;

namespace AL.UI
{
    public class StepResultElement : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI stepNumber, Name, timeTaken, status, wrongAttempt;

        public void Fill(int _stepNumber, Step step)
        {
            stepNumber.text = _stepNumber.ToString();
            Name.text = step.Name;
            timeTaken.text = step.Status == StepStatus.COMPLETE ? step.TimeTaken.ToString() : "0";
            status.text = step.Status == StepStatus.COMPLETE ? "Complete" : "Incomplete";
            wrongAttempt.text = step.WrongAttemptCount == 0 ? "0" : step.WrongAttemptCount.ToString().Style(AppManager.errorTextColorStyle);
        }
    }
}
