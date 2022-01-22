using System.Collections;
using UnityEngine;
using EVP;
using UnityEngine.AI;

namespace Assets.Scripts.RacerAi
{
    public class RacerAI : MonoBehaviour
    {
        public enum CarBehaviour
        {
            TrafficAI,
            RacerAI
        }
        public Transform target;
       
        private VehicleController vehicleController;
        private VehicleStandardInput standardInput;
        private AIWayPoints wayPoints;
        public NavMeshAgent agent;

        public string RacerName;
        [Header("Car Basic Components")]
        public float currentSpeed;
        public float turnAmount = 0f;
        public float forwardAmount = 0f;
        public float breakAmount = 0f;
        public float minimumReachDistance = 2f;
        public float maximumDistanceToReverse = 20f;
        public float stoppingDistance = 30f;
        public float stoppingSpeed = 50f;
        public float breakSpeed = 5f;
        public float maxSpeed =  100f;

        [Range(0,1)]public float steerChangeRate;
        private float distanceToTarget;


        public CarBehaviour behaviour;
        public bool useAI;
        [Header("Car Obstacle Avoidance Componnets")]
        
        public float MaxSensorLength;
        public float MaxRearSensorLength;
        public Vector3 frontSensorPosition = new Vector3(0.0f,0.2f,0.5f);
        public Vector3 rearSensorPosition = new Vector3(0.0f, 0.2f, -0.5f);
        public float sideSensorPosition;
        public float frontSensorAngle = 30f;
        public float safeObstacleDistanceInFront = 2f;
        public float safeObstacleDistanceOnSide = 2f;
        public float maximumTimeToreverse = 5f;
        public float RequiredSpeedAtCorner;
        public float SpeedCheckLength;
        public LayerMask obstacles;

        private float StaticSensorLength;
        private float StaticTimeToReverse;
        private float StaticSafeObstacleDistance;
        public int currentWayPoint;

        public bool overSpeeding;
        public bool reachedTarget;
        public bool avoidObstacle;
        public bool frontisClear;
        public bool isStuck;
        public bool isOverTurned;
        public bool sharpTurn;
        public bool enteredBreakZone;

        private void OnEnable()
        {
            vehicleController = gameObject.GetComponent<VehicleController>();
            standardInput = gameObject.GetComponent<VehicleStandardInput>();
            wayPoints = GameObject.FindObjectOfType<AIWayPoints>();
        }
        // Use this for initialization
        void Start()
        {
            switch (behaviour)
            {
                case CarBehaviour.TrafficAI:
                    vehicleController.maxSpeedForward = 20f;
                    break;
                case CarBehaviour.RacerAI:
                    vehicleController.maxSpeedForward = maxSpeed;
                    break;
            }
            if(standardInput.enabled && useAI)
            {
                standardInput.enabled = false;
            }

            currentWayPoint = 0;
            target = wayPoints.allWayPoints[currentWayPoint];
            StaticSensorLength = MaxSensorLength;
            StaticTimeToReverse = maximumTimeToreverse;
            StaticSafeObstacleDistance = safeObstacleDistanceInFront;

           
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //target = wayPoints.nextWayPoint;

            Vector3 DirToMove = (target.position - transform.position).normalized;// get the position of the transform to move to 
            float dot = Vector3.Dot(transform.forward, DirToMove);//Evaluate the position in which >= 1 is infront <=-1 is behind

            //if we are sstuck stop car from moving the obstacle avoidance func moves the vehicle
            MoveVehicle(dot);
            ObstacleAvoidance();
            
            //ChangeRayLength();
            //CalculateSafeSpeed();
            ResetVehicle();

            
            
            
            currentSpeed = vehicleController.speed * 18/5;
            vehicleController.steerInput = turnAmount;
            vehicleController.throttleInput = forwardAmount;
            vehicleController.brakeInput = breakAmount;
            
            
        }


        private void MoveVehicle(float dot)
        {
            if (!avoidObstacle)//are we avoiding an obstacle if not run this
            {
                
                distanceToTarget = Vector3.Distance(transform.position, target.position);
                ApplySteering();
                if (distanceToTarget > minimumReachDistance && !overSpeeding)
                {
                    //Move to the target
                    reachedTarget = false;
                    if (dot >= 0f)
                    {
                        if (!sharpTurn)
                        {
                            //the target is infront 
                            if (enteredBreakZone && currentSpeed > RequiredSpeedAtCorner)
                            {
                                ApplyBreak(0.5f);
                            }
                            forwardAmount = 1f;

                        }
                        else
                        {
                            forwardAmount = 0f;
                            
                        }
                        
                    }
                    else
                    {
                        //the target is behind
                        //check if the target is too far to reverse
                        //dont execute when we are stuck

                        if (distanceToTarget > maximumDistanceToReverse)
                        {
                            //too far to reverse
                            forwardAmount = 1f;
                        }
                        else
                        {
                            //forwardAmount = Mathf.MoveTowards(vehicleController.steerInput, -1f, SteerChangeRate);
                            forwardAmount = -0.5f;
                        }

                    }


                    //because it goes from 1f to -1f this simulates input as a player would 
                    //make sure to smooth the input 
                }
                else
                {
                    //we have reached the Target
                    forwardAmount = 0f;
                    reachedTarget = true;

                    
                    GetNextWayPoint();
                }
            }
           
        }

        private void ObstacleAvoidance()
        {
            //create ray casts on both the front and back to see our obstacles
            RaycastHit hit;
            Vector3 _frontRayStartPosition = transform.position;
            _frontRayStartPosition += transform.forward * frontSensorPosition.z;
            _frontRayStartPosition += transform.up * frontSensorPosition.y;



            float avoidMultiplier = 0f;
            avoidObstacle = false;
            frontisClear = true;
            //Front Center Sensor for checking if we have space for moving forward
            if (Physics.Raycast(_frontRayStartPosition, transform.forward, out hit, MaxSensorLength, obstacles))
            {
                if (hit.collider)
                {
                    frontisClear = false;
                    Debug.DrawLine(_frontRayStartPosition, hit.point, Color.red);
                    
                }

                Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
            }
            if (!isStuck && !reachedTarget)
            {

                #region FrontSensors
               
                
                //Front Right Sensor
                _frontRayStartPosition += transform.right * sideSensorPosition;
                if (Physics.Raycast(_frontRayStartPosition, transform.forward, out hit, MaxSensorLength, obstacles))
                {
                    if (hit.collider && !frontisClear)
                    {
                        avoidObstacle = true;

                        if (hit.distance < safeObstacleDistanceInFront)
                        {

                            isStuck = true;
                            return;
                        }
                        else
                        {
                            //Debug.Log("Going left");
                            if (hit.collider.CompareTag("Player"))
                                return;
                            avoidMultiplier = -1f;//negative to go right
                        }

                        Debug.DrawLine(_frontRayStartPosition, hit.point,Color.red);
                    }
                    Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
                }
                //Vector3 _SideRayStartPosition = transform.position;
                //_SideRayStartPosition += transform.right * frontSensorPosition.z;
                //_SideRayStartPosition += transform.up * frontSensorPosition.y;
                ////side Right Sensor
                //if (Physics.Raycast(_SideRayStartPosition, transform.right, out hit, MaxSensorLength, obstacles))
                //{
                //    if (hit.collider)
                //    {
                //        //avoidObstacle = true;

                //        //if (hit.distance <= safeObstacleDistance)
                //        //{
                //        //    isStuck = true;
                //        //    return;
                //        //}
                //        //else
                //        //{

                //        //    avoidMultiplier = -1f;//negative to go right
                //        //}

                        
                //    }
                //    Debug.DrawLine(_frontRayStartPosition, hit.point);
                //}
                //Front Right angled Sensor
                else if (Physics.Raycast(_frontRayStartPosition, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, MaxSensorLength, obstacles))
                {
                    
                    // check the distane to avoid car from consatntly reversing and forwarding even tho space is there
                    if (hit.collider && !frontisClear)
                    {
                        avoidObstacle = true;
                        
                        if (hit.distance < safeObstacleDistanceInFront)
                        {
                            isStuck = true;
                            return;
                        }
                        else
                        {
                            if (hit.collider.CompareTag("Player"))
                                return;
                            Debug.Log("Going extreme left");
                            avoidMultiplier = -0.5f;//negative to go right
                        }

                        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.red);
                    }
                    Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
                }
                //Front Left Sensor
                _frontRayStartPosition -= transform.right * sideSensorPosition * 2;
                if (Physics.Raycast(_frontRayStartPosition, transform.forward, out hit, MaxSensorLength, obstacles))
                {
                    if (hit.collider)
                    {
                        avoidObstacle = true;
                        //avoidMultiplier = 1f;//positve to go left 
                        if (hit.distance <= safeObstacleDistanceInFront)
                        {
                            isStuck = true;
                            
                            return;
                        }
                        else
                        {
                            if (hit.collider.CompareTag("Player"))
                                return;
                            avoidMultiplier = 1f;
                            Debug.Log("Going Right");
                        }

                        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.red);
                    }
                    Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
                }
                //Front left angled Sensor
                else if (Physics.Raycast(_frontRayStartPosition, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, MaxSensorLength, obstacles))
                {
                   
                    // check the distane to avoid car from consatntly reversing and forwarding even tho space is there
                    if (hit.collider && !frontisClear)
                    {
                        avoidObstacle = true;
                        //avoidMultiplier = 0.5f;//negative to go right
                        if (hit.distance <= safeObstacleDistanceInFront)
                        {
                            isStuck = true;
                            return;
                        }
                        else
                        {//change this when other ai cars
                            if (hit.collider.CompareTag("Player"))
                                return;
                            avoidMultiplier = 0.5f;//negative to go right
                            Debug.Log("Going extreme right");
                        }

                        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.red);
                    }
                    Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
                }

                if (avoidMultiplier == 0)
                {
                    //front Center Sensor
                    if (Physics.Raycast(_frontRayStartPosition, transform.forward, out hit, MaxSensorLength, obstacles))
                    {
                        if (hit.collider)
                        {
                            avoidObstacle = true;
                            if (hit.normal.x > 0f)
                            {
                                if (hit.collider.CompareTag("Player"))
                                    return;
                                avoidMultiplier = -1f;
                            }
                            else
                            {
                                if (hit.collider.CompareTag("Player"))
                                    return;
                                avoidMultiplier = 1f;
                            }
                            Debug.DrawLine(_frontRayStartPosition, hit.point,Color.red);
                        }
                        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.blue);
                    }
                }

                #endregion
            }
            else
            {
                #region RearSensors
                Vector3 _backRayStartPosition = transform.position;
                _backRayStartPosition += -transform.forward * rearSensorPosition.z;
                _backRayStartPosition += transform.up * rearSensorPosition.y;
                // reverse the vehicle
                ReverseVehicle(-0.2f);
                avoidMultiplier = 0f;//reset it to 0 or we will still have the input from the front sensors
                

                //Rear Right Sensor
                _backRayStartPosition += transform.right * sideSensorPosition;
                if (Physics.Raycast(_backRayStartPosition, -transform.forward, out hit, MaxRearSensorLength, obstacles))
                {
                    if (hit.collider && hit.distance <= safeObstacleDistanceInFront)
                    {
                        avoidObstacle = true;
                        isStuck = false;
                        avoidMultiplier = -1f;//positive to go right
                        Debug.DrawLine(_backRayStartPosition, hit.point,Color.red);
                    }

                }

                
                //Rear Left Sensor
                _backRayStartPosition -= transform.right * sideSensorPosition * 2;
                if (Physics.Raycast(_backRayStartPosition, -transform.forward, out hit, MaxRearSensorLength, obstacles))
                {
                    if (hit.collider && hit.distance <= safeObstacleDistanceInFront)
                    {
                        avoidObstacle = true;
                        isStuck = false;
                        avoidMultiplier = 1f;//negative to go left 
                        Debug.DrawLine(_backRayStartPosition, hit.point,Color.red);
                    }

                }
               
                
                if (avoidMultiplier == 0)
                {
                    //Rear Center Sensor
                    if (Physics.Raycast(_backRayStartPosition, -transform.forward, out hit, MaxRearSensorLength, obstacles))
                    {
                        if (hit.collider)
                        {
                            avoidObstacle = true;
                            if (hit.collider)
                            {
                                avoidObstacle = true;
                                if (hit.normal.x > 0f)
                                {
                                    avoidMultiplier = 1f;
                                }
                                else
                                {
                                    avoidMultiplier = -1f;
                                }

                                Debug.DrawLine(_backRayStartPosition, hit.point,Color.red);
                            }
                           
                        }

                    }


                }

             
                #endregion

            }
        


            if (avoidObstacle)
            {
                turnAmount = avoidMultiplier;
                MoveVehicleThroughObstacles();
            }
        }

        private void MoveVehicleThroughObstacles()
        {
            forwardAmount = 0.5f;
            Debug.Log("Avoiding Obstacles");
        }

        private void ApplySteering()
        {
        
            // get a point on a 2d graph 
            Vector3 turnDir = transform.InverseTransformPoint(target.position);
            turnAmount = (turnDir.x / turnDir.magnitude) * steerChangeRate;
            //TNAMT = Mathf.Clamp(transform.InverseTransformDirection(agent.desiredVelocity).x * 1.5f, -1f, 1f);
            


            //turnDir.x > 5 || turnDir.x < -5
            //this finds out the threshold that we are turning n if we are turn dir is too much 
            //and we are overspeeding we apply breaks
            if (turnAmount > 0.4 || turnAmount < -0.4)
            {
                if (currentSpeed > RequiredSpeedAtCorner)
                {
                    
                    sharpTurn = true;
                    ApplyBreak(1f);
                }
                else
                {
                    sharpTurn = false;
                    ApplyBreak(0f);
                }

            }
            else
            {
                
                sharpTurn = false;
                ApplyBreak(0f);

            }

            //because it goes from 1f to -1f this simulates input as a player would 
            //make sure to smooth the input 
        }

        private void ApplyBreak(float amount)
        {
            if (avoidObstacle)
            {
                #region ApplyingBreakSOnObstacles
                //for easier turn to avoid obstacles
                if (currentSpeed > 3f)
                {
                    breakAmount = 0.5f;
                }
                else
                {
                    breakAmount = 0f;
                }
                #endregion

            }


            breakAmount = amount;
            
        }
        private void ReverseVehicle(float safeReverseSpeed)
        {
            //safereversedspeed must be negative
            maximumTimeToreverse-=1 * Time.fixedDeltaTime;
            if(maximumTimeToreverse <= 0)
            {
                isStuck = false;
                maximumTimeToreverse = StaticTimeToReverse;
            }
            else
            {
                forwardAmount = safeReverseSpeed;
            }
            
            
        }
        private void ChangeRayLength()
        {
            if(currentSpeed > 20f)
            {//lengthen to avoid obstacles
                MaxSensorLength = StaticSensorLength;
                safeObstacleDistanceInFront = StaticSafeObstacleDistance;
            }
            else
            {//change the raylength to pass through small spaces
                MaxSensorLength = StaticSensorLength / 2f;
                safeObstacleDistanceInFront = StaticSafeObstacleDistance / 2f;
            }

            
        }

        private void GetNextWayPoint()
        {
            if (!reachedTarget)
                return;
            CalculateNextWayPoint();
        }

        private void CalculateSafeSpeed()
        {
            //RaycastHit hit;
            //Vector3 _frontRayStartPosition = transform.position;
            //_frontRayStartPosition += transform.forward * frontSensorPosition.z;
            //_frontRayStartPosition += transform.up * frontSensorPosition.y;

            ////front Center Sensor
            //if (Physics.Raycast(_frontRayStartPosition, transform.forward, out hit, MaxSensorLength * 2f, obstacles))
            //{
            //    if (hit.collider && hit.distance > SpeedCheckLength && currentSpeed > RequiredSpeedAtCorner)
            //    {
            //        if (hit.collider.CompareTag("Terrain"))
            //        {
            //            overSpeeding = true;

            //        }
            //        Debug.Log("Unsafe to speed");
            //        overSpeeding = true;
            //        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.green);
            //    }
            //    else
            //    {
            //        Debug.Log("Safe To Speed");
            //        overSpeeding = false;

            //        Debug.DrawLine(_frontRayStartPosition, hit.point, Color.red);
            //    }

            //}

            Vector3 firstPosition = wayPoints.allWayPoints[currentWayPoint].position;
            Vector3 secondposition = wayPoints.allWayPoints[currentWayPoint + 1].position;

            //get the dot product
            float angle = Vector3.Angle(firstPosition, secondposition);
            angle = angle * 180 / Mathf.PI;
            

            if(angle > 40)
            {
                print(angle);
            }
            
        }

        public void CalculateNextWayPoint()
        {
            currentWayPoint++;

            if (currentWayPoint >= wayPoints.allWayPoints.Count)
            {
                currentWayPoint = 0;
                
            }
            else
            {
                target = wayPoints.allWayPoints[currentWayPoint];

            }

            
        }

        private void ResetVehicle()
        {
            if(transform.rotation.x > 90 || transform.rotation.z > 90)
            {
                isOverTurned = true;
            }
            else
            {
                isOverTurned = false;
                return;
            }

            transform.position = wayPoints.allWayPoints[currentWayPoint - 1].position;

        }

        //private void FindPath()
        //{
        //    agent.SetDestination(target.position);

        //}
    }
}