using System.Collections;
using UnityEngine;
using TMPro;
using System.IO;

namespace Assets.Scripts.RaceUI
{
    public class LapCounter : MonoBehaviour
    {

        public TMP_Text lapTimeText;
        public TMP_Text bestLapTimeText;

        
        public TMP_Text LapText;


        public int numberOflaps;
        private int currentLap;

        public int lapMicroTime;
        int Second;
        int Minute;

        public bool startTime;

        string path;
        

        public void OnEnable()
        {

            string Createdpath = Application.dataPath + "/LapManagers.txt";

            if (!File.Exists(Createdpath))
            {
                File.WriteAllText(Createdpath, "Racer Saves \n");
            }
            path = Createdpath;

            startTime = false;
        }
        public void Update()
        {
            if (startTime)
            {
                lapMicroTime++;
                lapTimeText.text = Minute + ":" + Second + ":" + lapMicroTime;
                if (lapMicroTime >= 60)
                {
                    lapMicroTime = 0;
                    Second++;
                    if (Second >= 60)
                    {
                        Minute++;
                        Second = 0;
                    }
                }
            }
            
        }

        public void SaveLapTime()
        {
            bestLapTimeText = lapTimeText;
            
            File.AppendAllText(path, bestLapTimeText.text + "\n");
            lapMicroTime = 0;
            Second = 0;
            Minute = 0;
            lapTimeText.text = Minute + ":" + Second + ":" + lapMicroTime;
        }


        public void LapCount()
        {
            if (currentLap >= numberOflaps)
                return;
            currentLap += 1;
            LapText.text = "Lap " + currentLap + " / " + numberOflaps; 
        }
    }
}