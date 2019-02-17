using SQLite4Unity3d;
using UnityEngine;
using AL.Gameplay;
using System;
#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using System.Collections.Generic;

namespace AL.Database
{
    public class DataService
    {

        private SQLiteConnection _connection;

        public DataService(string DatabaseName)
        {

#if UNITY_EDITOR
            var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
            _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            Debug.Log("Final PATH: " + dbPath);

        }

        public void CreateDB()
        {
            _connection.DropTable<Person>();
            _connection.CreateTable<Person>();

            _connection.InsertAll(new[]{
            new Person{
                Id = 1,
                UserName = "harsh",
                Name = "Harsh Priyadarshi",
                Password = "tp@123"
            },
            new Person{
                Id = 2,
                UserName = "aravind",
                Name = "Aravindhan",
                Password = "fa@123"
            },
            new Person{
                Id = 3,
                UserName = "paras",
                Name = "Paras Gupta",
                Password = "jd@123"
            },
            new Person{
                Id = 4,
                UserName = "kuldeep",
                Name = "Kuldeep Joshi",
                Password = "rh@123"
            }
        });
        }

        #region MODIFIED_BY_HARSH
        public void CreateUserActivityDB()
        {
            _connection.DropTable<StepResult>();
            _connection.CreateTable<StepResult>();
        }

        #endregion

        public IEnumerable<Person> GetPersons()
        {
            return _connection.Table<Person>();
        }

        public IEnumerable<Person> GetPersonsNamedHarsh()
        {
            return _connection.Table<Person>().Where(x => x.UserName == "Harsh");
        }

        public Person GetJohnny()
        {
            return _connection.Table<Person>().Where(x => x.UserName == "Johnny").FirstOrDefault();
        }

        public Person CreatePerson()
        {
            var p = new Person
            {
                UserName = "Johnny",
                Name = "Mnemonic",
                Password = "jm@123"
            };
            _connection.Insert(p);
            return p;
        }

        public string StartDate { get; set; }
        public string UserName { get; set; }
        public int StepNumber { get; set; }
        public string Name { get; set; }
        public int TimeTaken { get; set; }
        public string Status { get; set; }
        public int WrongAttempts { get; set; }

        public void InsertStepResult(List<Step> steps)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                Step step = steps[i];
                var stepResult = new StepResult
                {
                    StartDate = DateTime.UtcNow.ToShortTimeString() + ", " + DateTime.UtcNow.ToShortDateString(),
                    UserName = Coordinator.instance.authentication.CurrentUser.UserName,
                    StepNumber = i + 1,
                    Name = step.Name,
                    TimeTaken = step.Status == StepStatus.COMPLETE ? (int)step.TimeTaken : 0,
                    Status = step.Status == StepStatus.COMPLETE ? "Complete" : "Incomplete",
                    WrongAttempts = step.WrongAttemptCount
                };
                _connection.Insert(stepResult);
            }
        }
          
    }
}
