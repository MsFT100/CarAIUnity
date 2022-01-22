using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.RaceUI
{
    public class RaceLine : MonoBehaviour
    {
        public LapCounter lapCounter;
        public GameObject halfPointTrigger;
        public GameObject StartPointTrigger;
        public bool isHalfPointTrigger;
        
        public void OnEnable()
        {
            lapCounter = GameObject.FindObjectOfType<LapCounter>();
           
        }
        public void Start()
        {
            gameObject.SetActive(false);
            halfPointTrigger.SetActive(true);
        }
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Trigger"))
            {
                lapCounter.startTime = true;
                lapCounter.LapCount();
                if (isHalfPointTrigger)
                {
                    StartPointTrigger.SetActive(true);
                    gameObject.SetActive(false);
                }
                else
                {
                    
                    halfPointTrigger.SetActive(true);
                    lapCounter.SaveLapTime();
                    gameObject.SetActive(false);
                }
                
            }
        }
    }
}
