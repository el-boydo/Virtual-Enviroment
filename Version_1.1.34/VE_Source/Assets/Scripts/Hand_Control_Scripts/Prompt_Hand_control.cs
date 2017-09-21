using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace HandPhysicsExtenstions
{
    [RequireComponent(typeof(HandPhysicsController))]
    public class Prompt_Hand_control : MonoBehaviour
    {


        private int totalGestures = 8;


        public bool LockCursorPrompt = false;
        public bool ControlsEnabledPrompt = true;


        public HandPhysicsStandaloneInput UserHandInput;
        public Gui_Manager GuiControl;
        public UI_Manager UI_manager;
        public MotionLight motionLightCon;
        
        
        public int CurrentGesture = 0;
        private float timeLeft = 0;
        private float GestureStart = 0;
        private float GestureTimeLimit = 5;
        private float RestingTimeLimit = 3;
        private float checkPoint = 0;
        private int CountdownTimeLeft= 0;
        private bool  isHandResting = true;

        private float heldTime = 0;
        public float points = 0;
        public float tempScore { get; private set; }
        private float pointsPerHold;
        
        private float movementVelocity = 0.5f;
        private float LastWristRotate = 0;
        private float LastArmRotate = 0;
        private int[] GestureSequence;
        private int nextGesture=0;
        private int GesturesCompleted=0;
        List<FingerType> fingerCommands = new List<FingerType> { FingerType.Thumb, FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky };


        private int TargetGesture;

        private bool handHeld = false;
        private bool transitioning = false;
        public bool IsHandResetting { get; private set; }
        public bool isHandPerforming { get; private set; }


        private float debug_Gesture_TimeTotal;

        private bool debugMode;

        [Header("Input Keys")]
        public string MoveForearmAxisX = "Mouse X";
        public string MoveForearmAxisY = "Mouse ScrollWheel";
        public string MoveForearmAxisZ = "Mouse Y";

        [Space]
        public string RotateForearmAxis = "Mouse X";
        public string RotateWristAxis = "Mouse Y";

        [Space]
        public KeyCode BendAllFingers = KeyCode.Mouse0;

        [Space]
        public KeyCode BendThumb = KeyCode.Space;
        public KeyCode BendIndex = KeyCode.F;
        public KeyCode BendMiddle = KeyCode.D;
        public KeyCode BendRing = KeyCode.S;
        public KeyCode BendPinky = KeyCode.A;

        [Space]
        public KeyCode HoldRotation = KeyCode.Mouse1;

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

        public void OnApplicationFocus(bool focus)
        {
            if (focus && LockCursorPrompt)
                Cursor.lockState = CursorLockMode.Locked;
        }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            pointsPerHold = 100 / (totalGestures * GestureTimeLimit);

            GestureSequence = RandNumGen();
            IsHandResetting = false;
            GestureStart = timeLeft;
            for (int i = 0; i < totalGestures; i++)
            {
                outputGestureList = outputGestureList + GestureSequence[i];
            }
            Debug.Log(string.Format(outputGestureList));
            UI_manager.TransitionTimer.color = Color.blue;
            IsHandResetting = true;
        }




        void Update()
        {
            timeLeft += Time.deltaTime;
           // UI_manager.updateTransitionTimer(CountdownTimeLeft);
            if (nextGesture < GestureSequence.Length)
            {
                Prompt_Hand_Manager();   
            }
            else
            {
                setHandNeutral();
                motionLightCon.CurrentState = 0;
                if (IsHandNeutral())
                {
                    Debug.Log(string.Format("Gesture time = {0}, Points = {1}, actual Points = {2}, currentgesture time = {3}", debug_Gesture_TimeTotal, tempScore, points,  heldTime));
                    UI_manager.score =  (float)System.Math.Round(points, 1);
                    GuiControl.gameOver = true;
                } 
            }
        }

        void Prompt_Hand_Manager()
        {

            /*
             * Stage 0 = Prompt Hand Neutral
             * timer 1
             * Stage 1 = User hand resting
             * Stage 2 = Prompt Hand performing
             * timer 2
             * Stage 3 = User hand performing
             * timer 3
             * Stage 4 = Prompt hand resetting
             * 
             * Modes: 
             * Resting
             * Performing
             * Resetting
             */
            
            if (isHandResting)
            {
                RestingActions();
            }
            else if (isHandPerforming)
            {
                PerformingActions();
            }
            else
            {
                ResetingActions();
            }
            HandleTimer();

        }
        /// <summary>
        /// Handle time related functions of the platform
        /// </summary>
        void HandleTimer()
        {
            float timeLimit = 0f;

            if (GesturesCompleted == 0 || isHandPerforming)
            {
                timeLimit = GestureTimeLimit ;

            }
            else
            {
                timeLimit = RestingTimeLimit ;
            }
            

            CountdownTimeLeft = (int)(timeLimit - (timeLeft - checkPoint)+.5f);
            if (IsHandResetting)
            {
                CountdownTimeLeft = 0;
            }


            UI_manager.updateTransitionTimer(CountdownTimeLeft);
            if (GesturesCompleted == 0)
            {
                UI_manager.countDownTimeLeft = (int)((GestureTimeLimit + .5f) - timeLeft);
            }

            // Activate when timer hits 0, move onto next stage of program
            if (!IsHandResetting && timeLeft - checkPoint> timeLimit)
            {
                if (isHandPerforming)
                {
                    adjustTotalScore();
                    motionLightCon.CurrentState = 0;
                    GesturesCompleted += 1;
                    nextGesture += 1;
                    
                    heldTime = 0;
                    if (nextGesture < GestureSequence.Length)
                    {
                        TargetGesture = GestureSequence[nextGesture];
                    }
                    isHandPerforming = false;
                    IsHandResetting = true;

                    

                }
                else if (isHandResting)
                {
                    UI_manager.TransitionTimer.color = Color.red;
                    GestureStart = timeLeft;

                    motionLightCon.CurrentState = -1;

                    

                    isHandResting = false;
                    isHandPerforming = true;

                    if (GesturesCompleted == 0)
                    {
                        UI_manager.showGamePlay();
                    }
                    checkPoint = timeLeft;
                }


            }
        }
        void adjustTotalScore()
        {
            UI_manager.TransitionTimer.color = Color.blue;
            if (heldTime > 5.00000000f)
            {
                heldTime = 5.00000000f;
            }
            points += pointsPerHold * heldTime;
        }
        /// <summary>
        /// Handle the hand when it is presently resting, moves hand to position, does nothing otherwise.
        /// </summary>
        void RestingActions()
        {
            
            if (IsHandNeutral()&& IsHandResetting)
            {

                //Debug.Log(string.Format("{0}", GestureSequence[nextGesture]));

                TargetGesture = GestureSequence[nextGesture];


                IsHandResetting = false;
                checkPoint = timeLeft;
                isHandResting = true;
                handControl();

            }

                // CountdownTimeLeft = (int)(timeLimit - (timeLeft - checkPoint));
/*                if (GesturesCompleted == 1 || timeLeft - checkPoint > timeLimit)
            {

                UI_manager.TransitionTimer.color = Color.red;
                GestureStart = timeLeft;

                motionLightCon.CurrentState = 0;

                checkPoint = timeLeft;

                isHandResting = false;
            }*/
        }
        /// <summary>
        /// Handles reseting the hand to neutral and related gampeplay effects. 
        /// </summary>
        void ResetingActions()
        {

            UI_manager.TransitionTimer.color = Color.blue;
            CountdownTimeLeft = 0;


           // Debug.Log(string.Format("reseting"));
            setHandNeutral();
            isHandResting = true;





            
        }
        /// <summary>
        /// Handles score keeping when the user is performing the prompt gesture.
        /// </summary>
        void PerformingActions()
        {



            

            UI_manager.score = (int)tempScore;




            Debug.Log(string.Format("Gesture time = {8}, Time Left = {0},PPH = {9}, Points = {1}, actual Points = {7}, Gesture_Sequence = {2}, target = Gesture_Sequence = {3}, currentgesture time = {4}; A: {5} E:{6}", timeLeft, tempScore, outputGestureList, TargetGesture, timeLeft - GestureStart, UserHandInput.CurrentGesture, TargetGesture,points, debug_Gesture_TimeTotal,pointsPerHold));

            if (UserHandInput.CurrentGesture == TargetGesture)
            {
                debug_Gesture_TimeTotal += Time.deltaTime;

                heldTime += Time.deltaTime;
                motionLightCon.CurrentState = 1;
                //tempScore = points + (pointsPerHold * heldTime);
                tempScore = pointsPerHold * debug_Gesture_TimeTotal;
            }
            else if (!IsHandResetting && UserHandInput.CurrentGesture != TargetGesture)
            {
                motionLightCon.CurrentState = -1;
            }


        }
        /// <summary>
        /// Controls the robotic hand. 
        /// </summary>
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
                Controller.wristRotation =  movementVelocity;
                LastWristRotate = movementVelocity;
            }

            // Forearm supination / pronation
            else if (TargetGesture == 6)
            {
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = -movementVelocity;
                
                LastArmRotate = -movementVelocity;
            }
            else if (TargetGesture == 7)
            {

                //Debug.Log(string.Format("Trying to rotate forearm"));
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = movementVelocity;
                 //Controller.RotateForearm(-movementVelocity);
                LastArmRotate = movementVelocity;
            }


        }
        bool IsHandNeutral()
        {
            bool fingersNeutral = Controller.CheckFingers();
            bool wristNeutral = Controller.CheckWrist();
            //Debug.Log(string.Format("Fingerstate = {0} Wirst state = {1} ", fingersNeutral, wristNeutral));
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

        int[] RandNumGen()
        {

            var numbers = new List<int> (totalGestures);
            for (int i = 0; i < totalGestures; i++)
            {
                numbers.Add(i);
            }
            var randomNumbers = new int[totalGestures];
            for (int i = 0; i < randomNumbers.Length; i++)
            {
                var thisNumber = UnityEngine.Random.Range(0, numbers.Count);
                randomNumbers[i] = numbers[thisNumber]+1;
                numbers.RemoveAt(thisNumber);
            }
            return randomNumbers;

        }
    }

}
