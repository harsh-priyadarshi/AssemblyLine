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
        TRANSPARENT
    }

    public interface IResettable
    {
        void Reset();
    }

    public interface IAssemblyItem
    {
        void AssemblyComplete(float tweenLength);
        void Highlight(HighlightType type);
    }
}
