using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AL.Gameplay
{
    public enum HighlightType
    {
        NONE,
        BLILNK,
        RED,
        GREEN,
        TRANSPARENT
    }

    public interface IHighlightable {
        void Highlight(HighlightType type);
    }
}
