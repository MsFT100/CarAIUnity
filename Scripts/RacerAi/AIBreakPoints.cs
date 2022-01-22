using System.Collections;
using UnityEngine;

namespace Assets.Scripts.RacerAi
{
    public class AIBreakPoints : MonoBehaviour
    {
        private RacerAI racerAI;
        [Tooltip("Speed Required to take this corner")]public float SafeSpeed;
        private void OnEnable()
        {
            racerAI = GameObject.FindObjectOfType<RacerAI>();
        }


        public void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("AI"))
            {
                
                racerAI.enteredBreakZone = true;
                racerAI.RequiredSpeedAtCorner = SafeSpeed;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("AI"))
            {
                
                racerAI.enteredBreakZone = false;
                racerAI.RequiredSpeedAtCorner = 65f;
            }
        }
    }
}