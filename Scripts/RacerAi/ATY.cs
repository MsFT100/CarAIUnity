using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.RacerAi;

public class ATY : MonoBehaviour
{
    public Transform Target;
    AIWayPoints aIWayPoints;
    public NavMeshAgent agent;

    private void OnEnable()
    {
        aIWayPoints = GameObject.FindObjectOfType<AIWayPoints>();
        Target = aIWayPoints.nextWayPoint;
    }
    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, Target.position) < 5f)
        {
            
            aIWayPoints.CalculateNextWayPoint();
            
        }
        Target = aIWayPoints.nextWayPoint;
        agent.SetDestination(Target.position);
        
    }
}
