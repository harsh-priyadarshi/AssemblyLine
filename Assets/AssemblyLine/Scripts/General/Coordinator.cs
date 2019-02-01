using AL.Theme;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public class Coordinator : MonoBehaviour {

        public static Coordinator instance;

        [Header("Scripts")]
        public AppTheme appTheme;

        private void Awake()
        {
            instance = this;
        }

    }
}
