using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using AL.Gameplay;

namespace AL.Database
{
    public class DatabaseManager : MonoBehaviour
    {

        private string UserActivityDataPath;

        // Use this for initialization
        void Start()
        {
            Init();
        }

        private void Init()
        {
            UserActivityDataPath = Application.dataPath + "/StreamingAssets/UserActivity.db";
            if (File.Exists(UserActivityDataPath))
            {
                print("User activity data already exists");
            }
            else
            {
                var ds = new DataService("UserActivity.db");
                ds.CreateUserActivityDB();
            }
        }

        public void SaveUserActivity(List<Step> steps)
        {
            var ds = new DataService("UserActivity.db");
            ds.InsertStepResult(steps);
        }

    }
}
