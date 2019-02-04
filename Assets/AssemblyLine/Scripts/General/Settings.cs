using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public class Settings : MonoBehaviour {
        [SerializeField]
        Preferences selectedPreferences;

        public Preferences SelectedPreferences
        {
            get
            {
                return selectedPreferences;
            }
        }
    }
}
