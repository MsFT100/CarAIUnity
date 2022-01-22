using System.Collections;
using UnityEngine;

namespace Assets.Scripts.RacerAi
{
    public class MouseWayPointer : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        public LayerMask Ground;
        public bool follow;

        void Update()
        {
            if (follow)
            {
                targetTransform.position = GetWorldPosition(); 
            }
            if (Input.GetMouseButtonDown(0))
            {
                follow = false;
            }
        }

        private Vector3 GetWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray , out RaycastHit hit, 1000f, Ground))
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
            
        }
    }
}