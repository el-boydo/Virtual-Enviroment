using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace HandPhysicsExtenstions
{
    [RequireComponent(typeof(HandPhysicsController))]
    public class Training_Hand_Controller : MonoBehaviour
    {

        private int totalGestures = 8;

        

        public HandPhysicsStandaloneInput UserHandInput;
        public Gui_Manager GuiControl;
        public UI_Manager UI_manager;
        public PauseMenuBehavior pauseMenu;
        public Text PauseText;
        public MotionLight motionLightCon;
        public UDPReceive server;


        private bool TrainingStarted = false;
        private bool Training = false;


        /// <summary>
        /// TIME PER DISPLAYED GESTURE
        /// </summary>
        private float GestureTimeLimit = 10f;
        /// <summary>
        /// TIME UNTIL TRAINING STARTS
        /// </summary>
        private float Starting_Timer = 10f;

        private int SessionTime = 0;



        public int CurrentGesture = 0;
        private float timeLeft = 0;
        private float GestureStart = 0;
        

        private float checkPoint = 0;
        private int CountdownTimeLeft = 0;
        private bool isResting = true;
        
        
        private float movementVelocity = 0.5f;
        private float LastWristRotate = 0;
        private float LastArmRotate = 0;
        private int[] GestureSequence = {1,2,3,4,5,6,7,8};
        private int nextGesture = 0;
        private int GesturesCompleted = 0;
        private bool PrepareNextGesture = false;
        List<FingerType> fingerCommands = new List<FingerType> { FingerType.Thumb, FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky };


        private int TargetGesture;

        private bool handHeld = false;
        private bool transitioning = false;
        public bool IsHandResetting { get; private set; }

        public Text countdownTimer;

        private string outputGestureList = "";



        public HandPhysicsController Controller
        {
            get
            {
                if (controller == null)
                    controller = GetComponent<HandPhysicsController>();
                return controller;
            }
        }
        private HandPhysicsController controller;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            SessionTime = ((GestureSequence.Length )*(int)GestureTimeLimit)+ (int)Starting_Timer;
            IsHandResetting = true;
            GestureStart = timeLeft;
            
        }



        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!TrainingStarted && server.training)
            {
                startTrainingSession();
                TrainingStarted = true;
            }
            Debug.Log(string.Format("Next Gesture conditions = {0},{1},{2}", IsHandResetting,IsHandNeutral(),PrepareNextGesture));
            if (IsHandResetting && IsHandNeutral() && PrepareNextGesture)
            {
                handControl();
                PrepareNextGesture = false;
                IsHandResetting = false;
            }

        }



        void handControl()
        {
            if (TargetGesture == 3)
            {
                Controller.StartBendFingers();

            }

            // fine pinch 
            else if (TargetGesture == 8)
            {
                Controller.StartBendFinger(fingerCommands[0]);
                Controller.StartBendFinger(fingerCommands[1]);

            }





            else if (TargetGesture == 2)
            {
                Controller.StartOpenFingers();
            }
            // Wrist flexation / extension



            else if (TargetGesture == 4)
            {
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToRest = false;
                Controller.IsWristRotatingToPrecision = false;
                Controller.wristRotation = -movementVelocity;
                LastWristRotate = -movementVelocity;

            }
            else if (TargetGesture == 5)
            {
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToPrecision = false;
                Controller.IsWristRotatingToRest = false;
                Controller.wristRotation = movementVelocity;
                LastWristRotate = movementVelocity;
            }

            // Forearm supination / pronation
            else if (TargetGesture == 7)
            {
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = movementVelocity;

                LastArmRotate = movementVelocity;
            }
            else if (TargetGesture == 6)
            {

               
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = -movementVelocity;
               
                LastArmRotate = -movementVelocity;
            }


        }
        bool IsHandNeutral()
        {
            bool fingersNeutral = Controller.CheckFingers();
            bool wristNeutral = Controller.CheckWrist();
          
            if (fingersNeutral && wristNeutral)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        void setHandNeutral()
        {
            if (!Controller.CheckFingers())
            {
                Controller.StopBendFingers();
            }
            else
            {
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToRest = true;
                Controller.IsWristRotatingToPrecision = false;

                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = true;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.wristRotation = -LastWristRotate;
                Controller.forearmRotation = -LastArmRotate;
            }
        }
        void startTrainingSession()
        {
            Debug.Log(string.Format("Starting Training Session"));
            Training = true;
            InvokeRepeating("Change_Gesture", Starting_Timer, GestureTimeLimit);
            InvokeRepeating("Update_Timer", 0f, 1f);


        }
        void Change_Gesture()
        {
            if (Training)
            {
                Debug.Log(string.Format("Next Gesture = {0}", nextGesture));

                setHandNeutral();
                TargetGesture = GestureSequence[nextGesture];
                nextGesture += 1;
                PrepareNextGesture = true;
                IsHandResetting = true;
            }
        }
        void Update_Timer()
        {
            if (SessionTime == 0)
            {
                Training = false;

                PauseText.text = "Finished";
                pauseMenu.togglePause();
                
            }
            if (Training)
            {
                SessionTime -= 1;
                if (SessionTime >= (GestureSequence.Length * GestureTimeLimit))
                {
                    countdownTimer.color = Color.blue;
                    countdownTimer.text = string.Format("{0}", SessionTime % Starting_Timer);
                }
                else
                {
                    countdownTimer.color = Color.red;
                    countdownTimer.text = string.Format("{0}", SessionTime % GestureTimeLimit);
                }


                
            }
        }
    }

}

