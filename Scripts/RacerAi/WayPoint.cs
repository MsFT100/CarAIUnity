using System.Collections;
using UnityEngine;
using UnityEditor;
namespace Assets.Scripts.RacerAi
{
    public class WayPoint : MonoBehaviour
    {
        public float gizmoRadius = 5f;
        private void OnDrawGizmos()
        {

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
          
        }
    }
}