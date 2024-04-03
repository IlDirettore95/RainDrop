using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class DataManager
    {
        private static DataManager _instance;
        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataManager();
                }
                return _instance;
            }
        }

        private DataManager() { }

        public void SaveInt(string name, int value)
        {
            PlayerPrefs.SetInt(name, value);
            PlayerPrefs.Save();
        }

        public int LoadInt(string name, int defaultValue)
        {
            return PlayerPrefs.GetInt(name, defaultValue);
        }

        public void SaveFloat(string name, float value)
        {
            PlayerPrefs.SetFloat(name, value);
            PlayerPrefs.Save();
        }

        public float LoadFloat(string name, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(name, defaultValue);
        }

        public void SaveString(string name, string text)
        {
            PlayerPrefs.SetString(name, text);
            PlayerPrefs.Save();
        }

        public string LoadString(string name, string defaultString = "")
        {
            return PlayerPrefs.GetString(name, defaultString);
        }
    }
}