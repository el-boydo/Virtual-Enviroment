using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace HandPhysicsExtenstions
{
    [RequireComponent(typeof(HandPhysicsController))]
    public class Tac_Test_Prompt: MonoBehaviour
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
        private bool  isResting = true;

        private float heldTime = 0;
        public float points = 0;
        public float tempScore { get; private set; }
        private float pointsPerHold = 2.5f;

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
            GestureSequence = RandNumGen();
            IsHandResetting = false;
            GestureStart = timeLeft;
            for (int i = 0; i < 8; i++)
            {
                outputGestureList = outputGestureList + GestureSequence[i];
            }
        }




        void Update()
        {
            timeLeft += Time.deltaTime;
           // UI_manager.updateTransitionTimer(CountdownTimeLeft);
            if (nextGesture < GestureSequence.Length)
            {


                if (GesturesCompleted == 0)
                {
                    CountdownTimeLeft = (int)((GestureTimeLimit + .5f) - (timeLeft - checkPoint));
                    UI_manager.countDownTimeLeft = (int)((GestureTimeLimit+ .5f) - timeLeft);
                    
                }
                // Debug.Log(string.Format("timLeft = {0}", (timeLeft - GestureStart)));
                if (timeLeft - GestureStart > GestureTimeLimit && !IsHandResetting)
                {
                    Debug.Log(string.Format("TRIGGERED"));
                    TargetGesture = 0;
                    points += pointsPerHold * heldTime;
                    heldTime = 0;

                    setHandNeutral();
                    IsHandResetting = true;

                    GesturesCompleted += 1;

                    if (GesturesCompleted == 1){
                        UI_manager.showGamePlay();
                    }
                        if (GesturesCompleted > 1)
                    {


                        nextGesture += 1;

                    }
                    motionLightCon.CurrentState = 0;
                    

                }


                else
                {

                    if (IsHandResetting)
                    {
                        Debug.Log(string.Format("reseting"));
                        if (IsHandNeutral())
                        {
                            if (GesturesCompleted > 0)
                            {

                                TargetGesture = GestureSequence[nextGesture];
                            }
                            if (isResting)
                            {
                                Debug.Log(string.Format("resting "));


                                CountdownTimeLeft = (int)((RestingTimeLimit+.5f) - (timeLeft - checkPoint));
                                if (GesturesCompleted == 1||timeLeft - checkPoint > RestingTimeLimit)
                                {
                                    UI_manager.TransitionTimer.color = Color.red;
                                    GestureStart = timeLeft;
                                    IsHandResetting = false;
                                    isResting = false;
                                    motionLightCon.CurrentState = 0;
                                    handControl();
                                    checkPoint = timeLeft;
                                }
                            }
                            else
                            {
                                checkPoint = timeLeft;
                                isResting = true;
                            }





                        }
                        else
                        {
                            UI_manager.TransitionTimer.color = Color.blue;
                            CountdownTimeLeft = 0;
                        }
                    }
                    else
                    {
                        CountdownTimeLeft = (int)((GestureTimeLimit + .5f) - (timeLeft - checkPoint));




                        UI_manager.score = (int)tempScore;




                        Debug.Log(string.Format("Time Left = {0}, Points = {1}, Gesture_Sequence = {2}, target = Gesture_Sequence = {3}, currentgesture time = {4}; A: {5} E:{6}", timeLeft, tempScore, outputGestureList, TargetGesture, timeLeft - GestureStart, UserHandInput.CurrentGesture, TargetGesture));

                        if (UserHandInput.CurrentGesture == TargetGesture)
                        {

                            heldTime += Time.deltaTime;
                            motionLightCon.CurrentState = 1;
                            tempScore = points + (pointsPerHold * heldTime);
                        }
                        else if (!IsHandResetting && UserHandInput.CurrentGesture != TargetGesture)
                        {
                            motionLightCon.CurrentState = -1;
                        }



                        /*
                        if (!ControlsEnabledPrompt)
                            return;




                        if (Input.GetKeyDown(BendAllFingers))
                        {

                            handHeld = true;
                            Controller.StartBendFingers();
                        }
                        else if (Input.GetKeyUp(BendAllFingers))
                        {
                            handHeld = false;
                            Controller.StopBendFingers();
                        }

                        if (Input.GetKeyDown(BendThumb))
                            Controller.StartBendFinger(FingerType.Thumb);
                        else if (Input.GetKeyUp(BendThumb))
                            Controller.StopBendFinger(FingerType.Thumb);

                        if (Input.GetKeyDown(BendIndex))
                            Controller.StartBendFinger(FingerType.Index);
                        else if (Input.GetKeyUp(BendIndex))
                            Controller.StopBendFinger(FingerType.Index);

                        if (Input.GetKeyDown(BendMiddle))
                            Controller.StartBendFinger(FingerType.Middle);
                        else if (Input.GetKeyUp(BendMiddle))
                            Controller.StopBendFinger(FingerType.Middle);

                        if (Input.GetKeyDown(BendRing))
                            Controller.StartBendFinger(FingerType.Ring);
                        else if (Input.GetKeyUp(BendRing))
                            Controller.StopBendFinger(FingerType.Ring);

                        if (Input.GetKeyDown(BendPinky))
                            Controller.StartBendFinger(FingerType.Pinky);
                        else if (Input.GetKeyUp(BendPinky))
                            Controller.StopBendFinger(FingerType.Pinky);


                        if (!Input.GetKey(HoldRotation))
                            Controller.MoveForearm(new Vector3(Input.GetAxis(MoveForearmAxisX), Input.GetAxis(MoveForearmAxisY),
                                Input.GetAxis(MoveForearmAxisZ)));
                        else
                        {
                            Controller.RotateWrist(Input.GetAxis(RotateWristAxis));
                            Controller.RotateForearm(Input.GetAxis(RotateForearmAxis));
                        }
                        */
                    }
                }
                UI_manager.updateTransitionTimer(CountdownTimeLeft);
            }
            else
            {
                motionLightCon.CurrentState = 0;
                if (IsHandNeutral())
                {

                    GuiControl.gameOver = true;
                } 
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
                Controller.wristRotation =  movementVelocity;
                LastWristRotate = movementVelocity;
            }

            // Forearm supination / pronation
            else if (TargetGesture == 6)
            {
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = movementVelocity;
                
                LastArmRotate = movementVelocity;
            }
            else if (TargetGesture == 7)
            {

                //Debug.Log(string.Format("Trying to rotate forearm"));
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = -movementVelocity;
                 //Controller.RotateForearm(-movementVelocity);
                LastArmRotate = -movementVelocity;
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
