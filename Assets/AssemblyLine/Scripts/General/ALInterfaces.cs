using AL.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AL
{
    public enum HighlightType
    {
        NONE,
        BLILNK,
        RED,
        GREEN,
        YELLOW,
        TRANSPARENT
    }

    public interface IResettable
    {
        void OnReset();
    }

    public interface IAssemblyItem
    {
        void AssemblyComplete(float tweenLength);
        void Highlight(HighlightType type);
        void ShowUpForAssembly(StepType type);
    }
}
