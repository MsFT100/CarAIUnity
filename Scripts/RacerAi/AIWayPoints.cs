using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Assets.Scripts.RacerAi
{
    public class AIWayPoints : MonoBehaviour
    {
        public Color nodeColor;
        public Transform wayPointHolder;
        public List<Transform> allWayPoints = new List<Transform>();
        public Transform nextWayPoint;
        public int currentWayPoint;
        [Range(0,5)]public float waypointRadius;
        public bool shownodes = true;
        private void OnEnable()
        {
            
            //GetAllWayPoints();
        }
        private void Start()
        {
            GetAllWayPoints();
            nextWayPoint = allWayPoints[0];
            //GetNearestWayPointOnStart();
        }

        private void GetAllWayPoints()
        {
            Transform[] path = wayPointHolder.GetComponentsInChildren<Transform>();
            for(int i = 0; i < path.Length; i++)
            {
                allWayPoints.Clear();
                allWayPoints.Add(path[i]);
                
            }
            
        }
        public void CalculateNextWayPoint()
        {
            currentWayPoint++;

            if(currentWayPoint >= allWayPoints.Capacity)
            {
                currentWayPoint = 0;
            }
            else
            {
                nextWayPoint = allWayPoints[currentWayPoint];
                
            }
           
        }
        private void GetNearestWayPointOnStart()
        {
            for(int i = 0; i < allWayPoints.Capacity; i++)
            {
                if(Vector3.Distance(transform.position, allWayPoints[i].position) <= 20f)
                {
                    nextWayPoint = allWayPoints[i];
                    currentWayPoint = i;
                }
            }

            
        }

        private void OnDrawGizmos()
        {
            if(wayPointHolder == null)
            {
                Debug.LogError("Assign awaypoint Holder");
            }
            else
            {
                if (!shownodes)
                    return;
                Transform[] path = wayPointHolder.GetComponentsInChildren<Transform>();
                allWayPoints = new List<Transform>();
                for (int i = 0; i < path.Length; i++)
                {
                    allWayPoints.Add(path[i]);

                }

                for (int i = 0; i < allWayPoints.Count; i++)
                {
                    Vector3 currentWayPointPosition = allWayPoints[i].position;
                    Vector3 previousPointPosition = Vector3.zero;

                    if (i != 0) previousPointPosition = allWayPoints[i - 1].position;
                    else if (i == 0) previousPointPosition = allWayPoints[allWayPoints.Count - 1].position;

                    Gizmos.color = nodeColor;
                    Gizmos.DrawLine(previousPointPosition, currentWayPointPosition);
                    Gizmos.DrawSphere(currentWayPointPosition, waypointRadius);
                }
            }
            
        }
    }
}