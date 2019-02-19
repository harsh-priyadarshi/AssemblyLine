using AL.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.UI
{
    public class ResultWindow : ModalWindow
    {
        [SerializeField]
        private List<StepResultElement> stepResultElelments;
        public void ShowResult(List<Step> assemblySteps)
        {
            for (int i = 0; i < assemblySteps.Count; i++)
                stepResultElelments[i].Fill(i + 1, assemblySteps[i]);
            Show(WindowType.RESULT);
        }

    }
}
