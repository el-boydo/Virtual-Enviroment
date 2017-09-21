using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace HandPhysicsExtenstions
{
    [RequireComponent(typeof(HandPhysicsController))]
    public class Tac_Mode_StandaloneScript : MonoBehaviour
    {

        public int CurrentGesture { get; private set; }
        private int motion = 0;
        // control joint movement speed
        private float movementVelocity = 0.5f;
        private bool gestureHeld = false;
        private bool motionSet;
        private int totalInput;
        private int totalBadInput;
        //private char lastInput = 'O';
        private char lastOutput;
        private int[] grabGestures = new int[2] {3, 8};
        
        private int currentGrab = 0; 
        //char commands = [ 'O', 'L', 'R', 'C' ];
        public char[] CommandInputs = new char[4] { 'O', 'L', 'R', 'C' };
        
        Dictionary<char, int> commands = new Dictionary<char, int>();
        List<FingerType> fingerCommands = new List<FingerType> { FingerType.Thumb, FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky};
        public bool EMG = false;
        public bool UDP_EMG = true ;
        public bool mouseCon = false;
        public bool keyboardControl = false;
        public bool staticHand = false; 
        private char command ='O';
        private string controlInput;
        private string lastControlInput;
        private float LastWristRotate = 0;
        private float LastArmRotate = 0;
        public UDPReceive server;
        public Gui_Manager GUI_Manager;
        /// <summary>
        /// Is the hand resetting to the rest position?
        /// </summary>
        public bool IsHandResetting { get; private set; }


        public bool LockCursor = true;
        public bool ControlsEnabled = true;

        [Header("Input Keys")]

        private string m_endComms = "Fire1";
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


        [DllImport("EMG_Read")]
        public static extern char readCommand();
        [DllImport("EMG_Read")]
        public static extern int startEMGRead();
        [DllImport("EMG_Read")]
        public static extern void closeConnection();

        public HandPhysicsController Controller
        {
            get
            {
                if (_controller == null)
                    _controller = GetComponent<HandPhysicsController>();
                return _controller;
            }
        }
        private HandPhysicsController _controller;

        public void OnApplicationFocus(bool focus)
        {
            if (focus && LockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }
        
        void Start()
        {
            CurrentGesture = 1;
            IsHandResetting = false;
            lastControlInput = "";
            if (EMG)
            {
                startEMGRead();
                for (int j = 0; j < 4; j++)
                {
                    commands.Add(CommandInputs[j], j);
                }
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
        bool IsHandNeutral()
        {
            bool fingersNeutral = Controller.CheckFingers();
            bool wristNeutral = Controller.CheckWrist();
            Debug.Log(string.Format("Fingerstate = {0} Wirst state = {1} ", fingersNeutral,wristNeutral));
            if (fingersNeutral && wristNeutral)
            {
                return true;
            }
            else
            {
                return false;
            }

            
        }


        void PerformGesture(int gesture)
        {
//            CurrentGesture = gesture;
// above doesn't work as it is too late in the code logic
            float movementVelocity = 0.6f;
            // controller for "motion" variable commanded control, over mouse+keyboard control.

            //Debug.Log(string.Format("Gesture = {0} ", gesture));


            // hand at rest
            /*            if ((Array.IndexOf(grabGestures, gesture)) == -1)
                        {
                            //if (currentGrab != 0)
                            //{
                            Controller.StopBendFingers();
                            //}
                        }   */


            if (Controller.IsWristRotating)
            {
                Controller.IsWristRotatingToRest = false;
                Controller.IsWristRotating = false;
            }

            if (Controller.IsForearmRotating)
            {


                Controller.IsForearmRotating = false;
                Controller.IsForearmRotatingToRest = false;
            }



            if (gesture == 1)
            {
                //if (currentGrab != 0)
                //{
                if (Controller.IsHandOpening)
                {
                    Controller.StopOpenFingers();
                }
                else {
                    Controller.StopBendFingers();
                }
                //Controller.RotateWristToRest(-LastWristRotate);
                //Controller.RotateForearmToRest(-LastArmRotate);
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToRest = true;
                Controller.IsWristRotatingToPrecision = false;

                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = true;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.wristRotation =  -LastWristRotate;
                Controller.forearmRotation =  -LastArmRotate;
                //}
            }
            /* grab Gestures
            **__________________________________________**
            * 
            - hand closed */
            if ((Array.IndexOf(grabGestures, gesture)) > -1)
            {
                Debug.Log(string.Format("Gesture = {0} ", gesture));
                if (currentGrab != gesture)
                {
                    Controller.StopBendFingers();
                }

                if (gesture == 3)
                {
                    Controller.StartBendFingers();
                    currentGrab = 3;
                }

                // fine pinch 
                else if (gesture == 8)
                {
                    Controller.StartBendFinger(fingerCommands[0]);
                    Controller.StartBendFinger(fingerCommands[1]);
                    currentGrab = 8;
                }
                


            }

            if (gesture == 2)
            {
                Controller.StartOpenFingers();
            }
            // Wrist flexation / extension



            else if (gesture == 4)
            {
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToRest = false;
                Controller.IsWristRotatingToPrecision = false;
                Controller.wristRotation = -movementVelocity;
                LastWristRotate = -movementVelocity;

            }
            else if (gesture == 5)
            {
                Controller.IsWristRotating = true;
                Controller.IsWristRotatingToPrecision = false;
                Controller.IsWristRotatingToRest = false;
                Controller.wristRotation =  movementVelocity;
                LastWristRotate = movementVelocity;
            }

            // Forearm supination / pronation
            else if (gesture == 6)
            {
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = movementVelocity;
                
                LastArmRotate = movementVelocity;
            }
            else if (gesture == 7)
            {

                //Debug.Log(string.Format("Trying to rotate forearm"));
                Controller.IsForearmRotating = true;
                Controller.IsForearmRotatingToRest = false;
                Controller.IsForearmRotatingToPrecision = false;
                Controller.forearmRotation = -movementVelocity;
                 //Controller.RotateForearm(-movementVelocity);
                LastArmRotate = -movementVelocity;
            }

            // individual finger control 


            else if (gesture >= 100000)
            {

                Debug.Log(string.Format("GestFound = {0} ", gesture));
                string fingerGest = gesture.ToString();
                for (int i = 0; i < fingerGest.Length; i++)
                {
                    if (i > 0 && fingerGest[i] == '1')
                    {
                        Controller.StartBendFinger(fingerCommands[i - 1]);
                        //Debug.Log(string.Format("invalid Gesture inputed: = {0} ", Controller.Parts.Fingers));
                        
                        // thumb index middle ring pinky 
                    }
                    else if(i>0 && fingerGest[i] == '0')
                    {
                        Controller.StopBendFinger(fingerCommands[i - 1]);
                    }
                }

            }
            //else
                //Debug.Log(string.Format("invalid Gesture inputed: = {0} ", gesture));


        }
        //

        /* 
         * 
         *  function SendJoints([])
         *  Function SendJoint(float finger, float joint, float x,  float y, float z,  float speed )
         * 
         * Gesture   xxxxxx
         *  Joint fingernum jointNum rotation(x,y,z) _ speed 
         *  Joints fingernum Jointnum(j_1) rotation(x,y,z) _ speed  - Jointnum(j_2) rotation(x,y,z) _ speed  - -
         *  
         *  joints uses the -  identifier to split up the joint commands
         */

        /// <summary>
        /// Manipulates numerous joints, places numerous joints in a predefined angle />
        /// </summary>
        void ControlJoints(String[] controlInput )
        {
            for (int x = 1; x< controlInput.Length;x++)
            {

                var controlInput_S = controlInput[x].Split('_');
                ControlJoint(controlInput_S);
                //ControlJoint() 

            }

        }
        // controls individual joints, places the joint in a pre-defined angle
        /// <summary>
        /// Manipulates a joint based on the input of [Joint_fingernum_jointNum_rotation(x, y, z)_speed]
        /// </summary>

        void ControlJoint(String[] JointCommand)
        {
            int fingerNum = convertJointAngle(JointCommand[1]);
          
            int JointNumber = convertJointAngle(JointCommand[2]);
            Quaternion jointrotations = explodeJointAngles(JointCommand[3]);




                if (JointCommand.Length == 4)
                {
                    Controller.moveJoint(fingerNum, JointNumber, jointrotations);
                }
                else
                {
                    int speed = convertJointAngle(JointCommand[4]);
                    Controller.moveJoint(fingerNum, JointNumber, jointrotations, speed);
                }
                       
            //moveJoint(jointNo, JointAngle);
        }


        Quaternion explodeJointAngles(string inputAngle)
        {

            string [] explodedAngles = inputAngle.Split(',');
            Vector3 parsedAngles = new Vector3();
            
            for (int i =0; i < 3; i++)
            {
                float temp = 0;
                parsedAngles[i] = 0;
                if (i<explodedAngles.Length && float.TryParse(explodedAngles[i],out temp))
                {
                    parsedAngles[i] = temp;
                }
                
                    
            }

            Quaternion outputAngles = Quaternion.Euler(parsedAngles);
            return outputAngles;

        }
        int convertJointAngle(string inputAngle)
        {
            int angle = -1;

            try
            {
                angle = int.Parse(inputAngle);

                return angle; 

            }
            catch
            {
                return angle;
            }

        }


        void Update()
        {
            char tempInput;
            bool handClosed = false;
            //Debug.Log(string.Format("fps (approx) = {0}", (1.0f / Time.deltaTime)));
            //server.setWristRotate(Controller.getWristRotation());
            //server.setJointAngleLoc(Controller.getJointAngle());
            if (!ControlsEnabled)
                return;

            if (UDP_EMG)
            {
                controlInput = server.lastReceivedUDPPacket;

                if (IsHandResetting || controlInput != lastControlInput)
                {

                    lastControlInput = controlInput;
                    //GUI_Manager.setLastInput(lastControlInput);
                    var controlInput_S = controlInput.Split('_');

                    Debug.Log(string.Format("udp Sent = {0} {1} {2}", server.lastReceivedUDPPacket.GetType(), motion, controlInput_S));
                    if (controlInput_S[0] == "Joint")
                    {
                        ControlJoint(controlInput_S);
                    }
                    else if (controlInput_S[0] == "Joints-Joint")
                    {
                        ControlJoints(controlInput.Split('-'));
                    }
                    else if (controlInput_S[0] == "Gesture")
                    {

                        try
                        {
                            motion = int.Parse(controlInput_S[1]);
                            CurrentGesture = motion;
                            //Debug.Log(string.Format("udp Sent = {0} {1} {2}", server.lastReceivedUDPPacket.GetType(), motion,controlInput_S));
                            if (IsHandNeutral())
                            {
                                IsHandResetting = false;
                                PerformGesture(motion);
                            }
                            else
                            {
                                IsHandResetting = true;
                                setHandNeutral();
                            }

                        }
                        catch
                        {

                        }
                    }


                }
            }

            //

            if (EMG)
            {
                tempInput = readCommand();


                Debug.Log(string.Format("temp = {0}", tempInput));
                //}
                /*
                if (tempInput != command)
                {
                    if (tempInput == lastOutput)
                    {
                        totalInput++;
                        if (totalInput >= 2)
                        {
                            command = tempInput;

                        }

                    }
                    else
                    {
                        command = lastInput;
                        totalInput = 0;
                        lastOutput = tempInput;
                        totalBadInput++;
                    }
                }
                Debug.Log(string.Format("command  = {0}, temp = {1} lastout = {2} last in {3}", command, tempInput, lastOutput, lastInput));
        */
                command = tempInput;

                motion = commands[command];
                Debug.Log(string.Format("temp = {0}, if grab = {1}", motion, ((Array.IndexOf(grabGestures, motion)) > -1)));
            }

            // for keyboard control 

            if (keyboardControl)
            {
                if (Input.GetKeyDown(BendAllFingers)/* || motion == 3*/)
                {
                    Controller.StartBendFingers();
                    handClosed = true;

                }
                else if (Input.GetKeyUp(BendAllFingers)/* || (command == 'O' && handClosed) || (motion == 0 && handClosed)*/)
                    Controller.StopBendFingers();

               /* if ((EMG && motion != 3) || (motion != 3))
                    Controller.StopBendFingers();
                    */
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

            }
           



            if (mouseCon) { 
                if (!Input.GetKey(HoldRotation) )
                    Controller.MoveForearm(new Vector3(Input.GetAxis(MoveForearmAxisX), Input.GetAxis(MoveForearmAxisY),
                        Input.GetAxis(MoveForearmAxisZ)));
                else
                {
                    
                    Controller.RotateWrist(Input.GetAxis(RotateWristAxis));
                    Controller.RotateForearm(Input.GetAxis(RotateForearmAxis));
                }
            }
            else if (staticHand)
            {
                if (Input.GetKey(HoldRotation))
                {
                    Controller.RotateWrist(Input.GetAxis(RotateWristAxis));
                    Controller.RotateForearm(Input.GetAxis(RotateForearmAxis));
                }
            }
            if (EMG)
            {
                if (Input.GetButton(m_endComms))
                {
                    closeConnection();
                }
            }
             /*else
            {
               if (Input.GetButton(m_endComms))
                {
                    server.OnDisable();
                    Application.Quit();
                }
            }*/
           // if (Input.GetKey("escape"))
            //    Application.Quit();
        }
    }

}
