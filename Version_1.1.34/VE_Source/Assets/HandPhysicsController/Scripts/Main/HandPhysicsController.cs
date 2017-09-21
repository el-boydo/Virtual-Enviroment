//Hand Physics Controller 
//v1.1

using System;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsExtenstions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class HandPhysicsController : MonoBehaviour
{
    /// <summary>
    /// Called when one of any finger part triggers collides with any other <see cref="Rigidbody"/>
    /// </summary>
    public ObjectTouched OnObjectTouched = () => {};
    /// <summary>
    /// Called when <see cref="Rigidbody"/> has been attached to hand
    /// </summary>
    public ObjectAttached OnObjectAttached = rigidbody => { };
    /// <summary>
    /// Called when attached object was dropped
    /// </summary>
    public ObjectDetached OnObjectDetached = () => { };

    #region Public Fields
    public ForearmConfig Forearm;
    public WristConfig Wrist;
    public FingersConfig Fingers;

    public HandParts Parts;

    #if UNITY_EDITOR
    [Header("Editor")]
    public bool DrawGizmos = true;
    #endif
    #endregion

    #region Public Properties

    /// <summary>
    /// Is forearm currently moving in specified direction?
    /// </summary>
    public bool IsForearmMoving { get; private set; }


    public float wristRotation { get; internal set; }
    public float forearmRotation { get; internal set; }
    /// <summary>
    /// Is forearm currently rotating around its base joint axis?
    /// </summary>
    public bool IsForearmRotating { get; internal set; }
    /// <summary>
    /// Is forearm currently rotating around its base joint axis to rest position?
    /// </summary>
    public bool IsForearmRotatingToRest { get; internal set; }
    /// <summary>
    /// Is forearm currently rotating around its base joint axis to a precision position?
    /// </summary>
    public bool IsForearmRotatingToPrecision { get; internal set; }
    /// <summary>
    /// Forearm precision target
    /// </summary>
    public Quaternion ForearmPrecisionAngle { get; internal set; }
    /// <summary>
    /// Is wrist currently rotating around its base joint axis?
    /// </summary>
    public bool IsWristRotating { get; internal set; }
    /// <summary>
    /// Is wrist currently rotating around its base joint axis to rest position?
    /// </summary>
    public bool IsWristRotatingToRest { get; internal set; }
    /// <summary>
    /// Is wrist currently rotating around its base joint axis to rest position?
    /// </summary>
    public bool IsWristRotatingToPrecision { get; internal set; }
    /// <summary>
    ///  Wrist precision target
    /// </summary>
    public Quaternion WristPrecisionAngle { get; internal set; }

    public bool IsHandOpening { get; private set; }




    /// <summary>
    /// Original Wrist rotation for returning to rest
    /// </summary>
    private Quaternion _WristStartRotation;
    /// <summary>
    /// Original Forearm rotation for returning to rest
    /// </summary>
    private Quaternion _ForearmStartRotation;



    public char[] CommandInputs = new char[4] { 'O', 'L', 'R', 'C' };
    public string[] jointNames = new string[18] {"T0", "T1", "T2", "I0", "I1", "I2", "M0", "M1", "M2", "R0", "R1", "R2", "S0", "S1", "S2", "W0", "W1", "W2", };
    /// <summary>
    /// Returns currently attached <see cref="Rigidbody"/>. Otherwise returns null 
    /// </summary>
    public AttachedObject AttachedObject { get; private set; }

    #endregion

    #region Private Fields
    private Vector3 _lastForearmPosition;
    private Vector3 _curForearmDirection;
    private float _curForearmTargetRotation;
    private float _curWristTargetRotation;

    private Vector3 _minForearmPos = Vector3.zero;
    private Vector3 _maxForearmPos = Vector3.zero;
    #endregion


    void Awake()
    {
        Init();
    }

    void Init()
    {
        _ForearmStartRotation = Parts.Forearm.transform.localRotation;
        _WristStartRotation =  Parts.Wrist.transform.localRotation;
        wristRotation = 0f;
        forearmRotation = 0f;
        Parts.SetController(this);
        Parts.DisableAllCollisions();
        UpdatePositionLimits();

        Parts.Forearm.Joint.slerpDrive = GetJointDrive(
            Parts.Forearm.Joint.slerpDrive,
            Mathf.Lerp(0, 10000*Parts.Forearm.Rigidbody.mass, Forearm.RotationHardness));
        Parts.Wrist.Joint.slerpDrive = GetJointDrive(
            Parts.Wrist.Joint.slerpDrive,
            Mathf.Lerp(0, 10000*Parts.Wrist.Rigidbody.mass, Wrist.RotationHardness));
    }

    void OnEnable()
    {
        OnObjectTouched += TryAttachObject;
    }

    void OnDisable()
    {
        OnObjectTouched -= TryAttachObject;
    }

    void Start()
    {
        //Dummy
    }

    void Update()
    {
        ControlForearm();
        ControlWrist();
        TryFixForearmAxes();
        ApplyPositionLimits();
    }

    void ControlForearm()
    {
        if (IsForearmMoving)
        {

            Parts.Forearm.Rigidbody.velocity = Vector3.Lerp(
                Parts.Forearm.Rigidbody.velocity,
                GetForearmTargetVelocity(_curForearmDirection),
                Forearm.MovementHardness);

            Vector3 forearmPos = Parts.Forearm.transform.position;
            if (!Mathf.Approximately(_curForearmDirection.x, 0))
                _lastForearmPosition = new Vector3(forearmPos.x, _lastForearmPosition.y, _lastForearmPosition.z);
            if (!Mathf.Approximately(_curForearmDirection.y, 0))
                _lastForearmPosition = new Vector3(_lastForearmPosition.x, forearmPos.y, _lastForearmPosition.z);
            if (!Mathf.Approximately(_curForearmDirection.z, 0))
                _lastForearmPosition = new Vector3(_lastForearmPosition.x, _lastForearmPosition.y, forearmPos.z);

            IsForearmMoving = false;
        }
        else
        {
            Parts.Forearm.Rigidbody.velocity = Vector3.Lerp(
                Parts.Forearm.Rigidbody.velocity,
                Vector3.zero,
                Forearm.MovementHardness);
        }

        if (IsForearmRotating)
        {
            if (IsForearmRotatingToPrecision)
            {
                //Debug.Log(string.Format("forarm arngle = {0}", Quaternion.Angle(Parts.Forearm.Joint.transform.localRotation, ForearmPrecisionAngle)));
                if (Quaternion.Angle(Parts.Forearm.Joint.transform.localRotation, ForearmPrecisionAngle) < 0.1)
                {


                    IsForearmRotating = false;
                    IsForearmRotatingToPrecision = false;

                }

                else
                {
                    Parts.Forearm.Joint.targetRotation = ForearmPrecisionAngle;

                }
            }
            else
            {
               // Debug.Log(string.Format("else statement is qworking"));
                if (IsForearmRotatingToRest)
                {
                    if (Quaternion.Angle(Parts.Forearm.Joint.transform.localRotation, _ForearmStartRotation) < 0.1)
                    {
                        IsForearmRotating = false;
                        IsForearmRotatingToRest = false;
                    }
                    else
                    {

                        RotateForearmToRest();
                    }
                }
                // update this later to check if keyboard / mouse control
                else
                {
                   // Debug.Log(string.Format("Trying to rotate forearm {0}", _curForearmTargetRotation));
                    RotateForearm(forearmRotation);
                }

                Parts.Forearm.Joint.targetRotation =
                    Quaternion.Euler(_curForearmTargetRotation, 0, 0);
            }
           // IsForearmRotating = false;
        }
    }

    void ControlWrist()
    {






        if (IsWristRotating)
        {
            // Debug.Log(string.Format("Wrist current angle= {0}", Quaternion.Euler(Parts.Wrist.Joint.transform.localRotation.eulerAngles)));

            if (IsWristRotatingToPrecision)
            {
                if (Quaternion.Angle(Parts.Wrist.Joint.transform.localRotation, WristPrecisionAngle) < 0.1)
                {


                    IsWristRotating = false;
                    IsWristRotatingToPrecision = false;

                }

                else
                {
                    Parts.Wrist.Joint.targetRotation = WristPrecisionAngle;
                    
                }
            }
            else
            {


                if (IsWristRotatingToRest)
                {
                    if (Quaternion.Angle(Parts.Wrist.Joint.transform.localRotation, _WristStartRotation) < 0.1)
                    {


                        IsWristRotating = false;
                        IsWristRotatingToRest = false;

                    }

                    else
                    {

                        RotateWristToRest();
                    }
                }
                else
                {

           
                    RotateWrist();
                }
                Parts.Wrist.Joint.targetRotation =
                    Quaternion.Euler(_curWristTargetRotation, 0, 0);
            }

            //Debug.Log(string.Format("temp = {0}", Parts.Wrist.Joint.targetRotation));
            //IsWristRotating = false;
            //return;
        }

        /*
         * 
         * 
         *         if (IsWristRotating)
        {
            Parts.Wrist.Joint.targetRotation =
                Quaternion.Euler(_curWristTargetRotation, 0, 0);

            IsWristRotating = false;
            return;
        }
         * 
         * 
         * 
         * 
         * 
         * 
         */

        if (!Wrist.HoldRotation)
        {
            Parts.Wrist.Joint.targetRotation = Quaternion.Lerp(Parts.Wrist.Joint.targetRotation, Quaternion.identity, Wrist.RotationHardness);
            _curWristTargetRotation = Parts.Wrist.Joint.targetRotation.eulerAngles.x;
        }
    }
    public Vector3 getWristRotation()
    {
        Vector3 rotationAngle = Parts.Wrist.transform.localRotation.eulerAngles;
        if (rotationAngle.x > 180)
            rotationAngle.x -= 360;
        if (rotationAngle.y > 180)
            rotationAngle.y -= 360;
        return rotationAngle; 
    }
    /*    public float getJointAngle()
        {
            //float currentAngle = Quaternion.Angle(Parts.Fingers[1].Parts[0].transform.localRotation, Parts.Fingers[1].Parts[0].StartRotation);
            //float currentAngle = Parts.Fingers[1].Parts[0].transform.localRotation.eulerAngles.x - Parts.Fingers[1].Parts[0].StartRotation.eulerAngles.x;

            Vector3 vectorA = Parts.Fingers[1].Parts[0].transform.localRotation.eulerAngles;
            Vector3 vectorB = Parts.Fingers[1].Parts[0].TargetRotation.eulerAngles;
            float angle = Vector3.Angle(vectorA, vectorB);
            //Vector3 cross = Vector3.Cross(vectorA, vectorB);
            //if (cross.x < 0) angle = -angle;

            return angle;
        }
        //This returns the angle in radians
        public static float AngleInRad(Vector3 vec1, Vector3 vec2)
        {
            return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
        }*/

    public string getJointAngleInformation(string request)
    {
        string jointAngle="";

        if(request == "all")
        {
            jointAngle = getAllJointAngles();
        }
        else
        {
            var jointRequests = request.Split(',');
            for (int JR = 0; JR < jointRequests.Length; JR++)
            {
                var JointInfo = jointRequests[JR].Split(':');
                if (JR == jointRequests.Length - 1)
                {
                    jointAngle += getJointAngle((int.Parse(JointInfo[0])), int.Parse(JointInfo[1]),true);
                }
                else
                {
                    jointAngle += getJointAngle((int.Parse(JointInfo[0])), int.Parse(JointInfo[1]));
                }
            }
        }


        return jointAngle;
    }


    /// <summary>
    /// Retrieves all joints angles of every finger
    /// </summary>
    /// <returns>Formatted string containing all joint commands</returns>
    public string getAllJointAngles()
    {
        Vector3 vectorA = new Vector3(0, 0, 0); 
        Vector3 vectorB = new Vector3(0, 0, 0);
        float angle = 0F;
        string angles = "";
        //float currentAngle = Quaternion.Angle(Parts.Fingers[1].Parts[0].transform.localRotation, Parts.Fingers[1].Parts[0].StartRotation);
        //float currentAngle = Parts.Fingers[1].Parts[0].transform.localRotation.eulerAngles.x - Parts.Fingers[1].Parts[0].StartRotation.eulerAngles.x;
        for (int f = 0; f < 6; f++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (f < 5)
                {

                    vectorA = Parts.Fingers[f].Parts[j].transform.localRotation.eulerAngles;
                    vectorB = Parts.Fingers[f].Parts[j].OpenHandRotation.eulerAngles;
                    //angle = Vector3.Angle(vectorB, vectorA);
                    //angle = AngleInRad(vectorB, vectorA);
                    angle = Quaternion.Angle(Parts.Fingers[f].Parts[j].OpenHandRotation, Parts.Fingers[f].Parts[j].transform.localRotation);
                    //Debug.Log(string.Format("F = {0}, J = {1}, angle = : {2}", vectorA, vectorB, angle));
                    //angles = angles + jointNames[(3 * f)+j] + ":" + Parts.Fingers[f].Parts[j].transform.localRotation.eulerAngles + "\n";
                    angles = angles + jointNames[(3 * f) + j] + ":" + angle + ",";

                }
                else
                {
                    if (j < 2)
                    {
                        if (j == 0)
                        {
                            //angle = Quaternion.Angle(_ForearmStartRotation, Parts.Forearm.transform.localRotation);
                            vectorA = _ForearmStartRotation.eulerAngles;
                            vectorB = Parts.Forearm.transform.localRotation.eulerAngles;
                        }
                        else if (j == 1)
                        {
                            vectorA = _WristStartRotation.eulerAngles;
                            vectorB = Parts.Wrist.transform.localRotation.eulerAngles;
                            //angle = Quaternion.Angle(_WristStartRotation, Parts.Wrist.transform.localRotation);
                        }

                        // float WristAngle = Quaternion.Angle(_WristStartRotation, Parts.Wrist.transform.localRotation);

                        //vectorA = transform.localRotation.eulerAngles;
                        //vectorB = _EndRotation.eulerAngles;
                        angle = Vector3.Angle(vectorA, vectorB);
                       // Debug.Log(string.Format("F = {0}, J = {1}, angle = : {2}", vectorA, vectorB, angle));

                        Vector3 cross = Vector3.Cross(vectorA, vectorB);
                        if (cross.x < 0) angle = -angle;
                        angles = angles + jointNames[(3 * f) + j] + ":" + angle + ",";
                    }
                    else
                    {
                        //angles = angles + jointNames[(3 * f) + j] + ":" + "N/A" + "";
                    }
                    
                }
            }
        }
        
        //Vector3 cross = Vector3.Cross(vectorA, vectorB);
        //if (cross.x < 0) angle = -angle;

        return angles;
    }
    /// <summary>
    /// Returns the current angle of an individual joint
    /// </summary>
    /// <param name="finger">Finger containing the joint, 0-5 (0-4 thumb-small finger, 5 wrist/forearm)</param>
    /// <param name="joint">Individual Joint (0-2) from palm outwards</param>
    /// <returns>Returns formated string of joint angle</returns>
    public string getJointAngle(int finger, int joint, bool final = false)
    {
        Vector3 vectorA = new Vector3(0, 0, 0);
        Vector3 vectorB = new Vector3(0, 0, 0);
        float angle = 0F;
        string angles = "";
        string endl = ",";
        if (final)
        {
            endl = "";
        }

                if (finger <5)
                {

                    vectorA = Parts.Fingers[finger].Parts[joint].transform.localRotation.eulerAngles;
                    vectorB = Parts.Fingers[finger].Parts[joint].OpenHandRotation.eulerAngles;
                    //angle = Vector3.Angle(vectorB, vectorA);
                    //angle = AngleInRad(vectorB, vectorA);
                    angle = Quaternion.Angle(Parts.Fingers[finger].Parts[joint].OpenHandRotation, Parts.Fingers[finger].Parts[joint].transform.localRotation);
                    //Debug.Log(string.Format("F = {0}, J = {1}, angle = : {2}", vectorA, vectorB, angle));
                    //angles = angles + jointNames[(3 * f)+j] + ":" + Parts.Fingers[f].Parts[j].transform.localRotation.eulerAngles + "\n";
                    angles = angles + jointNames[(3 * finger) + joint] + ":" + angle + endl;

                }
                if (finger == 5)
                {
                    if (joint < 2)
                    {
                        if (joint == 0)
                        {
                           
                            vectorA = _ForearmStartRotation.eulerAngles;
                            vectorB = Parts.Forearm.transform.localRotation.eulerAngles;
                        }
                        else if (joint == 1)
                        {
                            vectorA = _WristStartRotation.eulerAngles;
                            vectorB = Parts.Wrist.transform.localRotation.eulerAngles;

                        }


                        angle = Vector3.Angle(vectorA, vectorB);
                      //  Debug.Log(string.Format("F = {0}, J = {1}, angle = : {2}", vectorA, vectorB, angle));

                        Vector3 cross = Vector3.Cross(vectorA, vectorB);
                        if (cross.x < 0) angle = -angle;
                        angles = angles + jointNames[(3 * finger) + joint] + ":" + angle + endl;
                    }
                    else
                    {
                        //angles = angles + jointNames[(3 * f) + j] + ":" + "N/A" + "";
                    }
        }

        return angles;
    }

    /*
     * 
     * 
     * 
     * 
     * BELOW IS THE OBJECT ATTATCHMENT CODE, MODIFYING THIS WILL ALLOW ME TO MODIFY MATERIALS FOR MANIPULATION
     * 
     * 
     * 
     * 
     * 
     */
    void TryAttachObject()
    {
        if (AttachedObject != null || !Parts.Fingers[0].IsBending)
            return;
        
        List<Rigidbody> thumbTouches = Parts.Fingers[0].GetAllTouchingObjects();
        for (int i = 0; i < thumbTouches.Count; i++)
        {
            for (int j = 1; j < Parts.Fingers.Length; j++)
            {
                List<Rigidbody> touches = Parts.Fingers[j].GetAllTouchingObjects();
                for (int k = 0; k < touches.Count; k++)
                {
                    if (thumbTouches.Contains(touches[k]) && Parts.Fingers[j].IsBending)
                    {
                        AttachObject(touches[k], 0, (FingerType) j);
                        return;
                    }
                }
            }
        }
    }

    void AttachObject(Rigidbody objectToAttach, FingerType holdingFinger1, FingerType holdingFinger2)
    {
        _lastForearmPosition = Parts.Forearm.transform.position;

        Parts.Fingers[(int) holdingFinger1].IsHoldingObject = Parts.Fingers[(int)holdingFinger2].IsHoldingObject = true;
        AttachedObject = objectToAttach.gameObject.AddComponent<AttachedObject>();
        AttachedObject.Attach(this, DetachObject);
        Parts.IgnoreCollisions(objectToAttach, true);

        OnObjectAttached(objectToAttach);
    }

    void DetachObject()
    {
        if (Parts.CheckIfNull())
            return;

        for (int i = 0; i < Parts.Fingers.Length; i++)
        {
            Parts.Fingers[i].IsHoldingObject = false;
        }

        if (AttachedObject == null)
            return;

        if (AttachedObject.Rigidbody != null)
            Parts.IgnoreCollisions(AttachedObject.Rigidbody, false);
        AttachedObject.Detach();
        AttachedObject = null;

        OnObjectDetached();
    }

    void TryFixForearmAxes()
    {
        if (AttachedObject == null)
            return;

        Vector3 forearmPos = Parts.Forearm.transform.position;
        float hardness = Forearm.FixAxis.Hardness;

        if (Forearm.FixAxis.X && Mathf.Approximately(_curForearmDirection.x, 0))
            Parts.Forearm.Rigidbody.MovePosition(
                forearmPos = Vector3.Lerp(
                    forearmPos,
                    new Vector3(_lastForearmPosition.x, forearmPos.y, forearmPos.z),
                    hardness));

        if (Forearm.FixAxis.Y && Mathf.Approximately(_curForearmDirection.y, 0))
            Parts.Forearm.Rigidbody.MovePosition(
                forearmPos = Vector3.Lerp(
                    forearmPos,
                    new Vector3(forearmPos.x, _lastForearmPosition.y, forearmPos.z),
                    hardness));


        if (Forearm.FixAxis.Z && Mathf.Approximately(_curForearmDirection.z, 0))
            Parts.Forearm.Rigidbody.MovePosition(
                Vector3.Lerp(
                    forearmPos,
                    new Vector3(forearmPos.x, forearmPos.y, _lastForearmPosition.z),
                    hardness));
    }

    void ApplyPositionLimits()
    {
        if (!Forearm.PositionLimits.UseLimits)
            return;

        Rigidbody forearm = Parts.Forearm.Rigidbody;
        Vector3 forearmPos = forearm.transform.position;

        if (forearm.transform.position.x > _maxForearmPos.x)
            forearm.transform.position = forearmPos = new Vector3(_maxForearmPos.x, forearmPos.y, forearmPos.z);
        if (forearm.transform.position.x < _minForearmPos.x)
            forearm.transform.position = forearmPos = new Vector3(_minForearmPos.x, forearmPos.y, forearmPos.z);

        if (forearm.transform.position.y > _maxForearmPos.y)
            forearm.transform.position = forearmPos = new Vector3(forearmPos.x, _maxForearmPos.y, forearmPos.z);
        if (forearm.transform.position.y < _minForearmPos.y)
            forearm.transform.position = forearmPos = new Vector3(forearmPos.x, _maxForearmPos.y, forearmPos.z);

        if (forearm.transform.position.z > _maxForearmPos.z)
            forearm.transform.position = forearmPos = new Vector3(forearmPos.x, forearmPos.y, _maxForearmPos.z);
        if (forearm.transform.position.z < _minForearmPos.z)
            forearm.transform.position = new Vector3(forearmPos.x, forearmPos.y, _minForearmPos.z);
    }

    JointDrive GetJointDrive(JointDrive jointDrive, float spring)
    {
        jointDrive.positionSpring = spring;
        return jointDrive;
    }

    Vector3 GetForearmTargetVelocity(Vector3 direction)
    {
        if (Forearm.MovementRelativity == Space.Self)
            return Parts.Forearm.transform.TransformDirection(new Vector3(
                direction.x * Forearm.MovementSpeed.x,
                direction.y * Forearm.MovementSpeed.y,
                direction.z * Forearm.MovementSpeed.z));

        return new Vector3(
                direction.x * Forearm.MovementSpeed.x,
                direction.y * Forearm.MovementSpeed.y,
                direction.z * Forearm.MovementSpeed.z);
    }
    /// <summary>
    /// Retrieve Current Forarm angle
    /// </summary>
    /// <returns></returns>
    float GetForearmAngle()
    {
        return 5f;
    }

    /// <summary>
    /// Get current Wrist Angle
    /// </summary>
    /// <returns></returns>
    float GetWristAngle()
    {
        return 5f;
    }
    


    /// <summary>
    /// Updates current position limits represented in <see cref="HandPhysicsExtenstions.ForearmConfig.PositionLimits"/>
    /// </summary>
    public void UpdatePositionLimits()
    {
        var limits = Forearm.PositionLimits;

        Vector3 boundsHalfSize = limits.Bounds * 0.5f;
        _minForearmPos = limits.BoundsOffset - boundsHalfSize;
        _maxForearmPos = limits.BoundsOffset + boundsHalfSize;

        if (limits.Relativity == Space.Self)
        {
            _minForearmPos += transform.position;
            _maxForearmPos += transform.position;
        }
    }

    /// <summary>
    /// Applies velocity to forearm at <see cref="direction"/> based on <see cref="HandPhysicsExtenstions.ForearmConfig.MovementSpeed"/> and <see cref="HandPhysicsExtenstions.ForearmConfig.MovementHardness"/>
    /// </summary>
    /// <param name="direction"></param>
    public void MoveForearm(Vector3 direction)
    {
        if (Mathf.Approximately(direction.magnitude, 0))
        {
            _curForearmDirection = Vector3.zero;
            return;
        }

        _curForearmDirection = direction;
        IsForearmMoving = true;
    }
    #region Precise_Control


    /// <summary>
    /// Loops through fingers and connected joints to check if they are all at their neutral rotation
    /// </summary>
    public bool CheckFingers()
    {
        for (int i = 0; i < Parts.Fingers.Length; i++)
        {
            if (!Parts.Fingers[i].checkJoints())
            {
             //   Debug.Log(string.Format("finger {0} is not at neutral", i));
                return false;
            }
        }
        return true;

        
    }
    /// <summary>
    /// Checks wrist and forearm joint angles against their neutral angles
    /// </summary>
    public bool CheckWrist()
    {
        float ForearmAngle = Quaternion.Angle(_ForearmStartRotation, Parts.Forearm.transform.localRotation);
        float WristAngle = Quaternion.Angle(_WristStartRotation, Parts.Wrist.transform.localRotation);

        if (ForearmAngle == 0 && WristAngle == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }






    // ORIGINAL CODE, KEEP AS REFERENCE 

    /// <summary>
    /// Rotates forearm around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.ForearmConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.ForearmConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateForearm(float value)
    {
        _curForearmTargetRotation += Mathf.Clamp(value, -1, 1) * Forearm.RotationSpeed;
        _curForearmTargetRotation = Mathf.Clamp(
            _curForearmTargetRotation,
            Parts.Forearm.Joint.lowAngularXLimit.limit, 
            Parts.Forearm.Joint.highAngularXLimit.limit);

        IsForearmRotating = true;
    }

    /// <summary>
    /// Rotates wrist around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.WristConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.WristConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateWrist(float value)
    {
        _curWristTargetRotation += Mathf.Clamp(value, -1, 1) * Wrist.RotationSpeed;
        _curWristTargetRotation = Mathf.Clamp(
            _curWristTargetRotation,
            Parts.Wrist.Joint.lowAngularXLimit.limit,
            Parts.Wrist.Joint.highAngularXLimit.limit);
        
        IsWristRotating = true;
    }

    public void moveJoint(int fingerNumber, int JointNumber, Quaternion rotation)
    {
        if (fingerNumber == 5 )
        {
            if (JointNumber == 1)
            {


                IsWristRotating = true;
                IsWristRotatingToRest = false;
                IsWristRotatingToPrecision = true;
                WristPrecisionAngle = rotation;

            }
            else
            {
                IsForearmRotating = true;
                IsForearmRotatingToRest = false;
                IsForearmRotatingToPrecision = true;
                ForearmPrecisionAngle = rotation;
            }
        }
        else
        {

            Parts.Fingers[fingerNumber].PrecisionRotate(JointNumber, rotation);
        }
    }
    public void moveJoint(int fingerNumber, int JointNumber, Quaternion rotation, int speed)
    {
        if (fingerNumber == 5 )
        {
            if (JointNumber == 1)
            {


                IsWristRotating = true;
                IsWristRotatingToRest = false;
                IsWristRotatingToPrecision = true;
                WristPrecisionAngle = rotation;

            }
            else
            {
                IsForearmRotating = true;
                IsForearmRotatingToRest = false;
                IsForearmRotatingToPrecision = true;
                ForearmPrecisionAngle = rotation;
            }
        }
        else
        {
            Parts.Fingers[fingerNumber].PrecisionRotate(JointNumber, rotation, speed);
        }
    }
    
    /*
     * MODIFIED CODE BY PETER, THIS IS NEW STUFF AND EXPERIMENTAL OR POORLY IMPLEMENTED, PLEASE BE MINDFUL
     * PLEASE DO  NOT DO THE NEEDFUL
     * 
     * 
     * 
     * */




    /// <summary>
    /// Rotates forearm around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.ForearmConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.ForearmConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateForearm()
    {
        //Debug.Log(string.Format("Trying to rotate forearm"));
        _curForearmTargetRotation += Mathf.Clamp(forearmRotation, -1, 1) * Forearm.RotationSpeed;
        _curForearmTargetRotation = Mathf.Clamp(
            _curForearmTargetRotation,
            Parts.Forearm.Joint.lowAngularXLimit.limit,
            Parts.Forearm.Joint.highAngularXLimit.limit);

        
    }

    /// <summary>
    /// Rotates wrist around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.WristConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.WristConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateWrist()
    {
        _curWristTargetRotation += Mathf.Clamp(wristRotation, -1, 1) * Wrist.RotationSpeed;
        _curWristTargetRotation = Mathf.Clamp(
            _curWristTargetRotation,
            Parts.Wrist.Joint.lowAngularXLimit.limit,
            Parts.Wrist.Joint.highAngularXLimit.limit);

        
    }


    /// <summary>
    /// Rotates forearm around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.ForearmConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.ForearmConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateForearmToRest()
    {
        float MaxLimit=1;
        float LowLimit=-1;
        if (forearmRotation < 0)
        {
            MaxLimit = Parts.Forearm.Joint.highAngularXLimit.limit;
            LowLimit = 0;

        }
        else
        {


            LowLimit = Parts.Forearm.Joint.lowAngularXLimit.limit;
            MaxLimit = 0;
        }
        _curForearmTargetRotation += Mathf.Clamp(forearmRotation, -1, 1) * Forearm.RotationSpeed;
        _curForearmTargetRotation = Mathf.Clamp(
            _curForearmTargetRotation,
            LowLimit,
            MaxLimit);

        
    }

    /// <summary>
    /// Rotates wrist around base joint axis by <see cref="value"/> between min and max joint limits based on <see cref="HandPhysicsExtenstions.WristConfig.RotationSpeed"/> and <see cref="HandPhysicsExtenstions.WristConfig.RotationHardness"/>
    /// </summary>
    /// <param name="value"></param>
    public void RotateWristToRest()
    {
        float MaxLimit;
        float LowLimit;
        if (wristRotation < 0)
        {
            MaxLimit = Parts.Wrist.Joint.highAngularXLimit.limit;
            LowLimit = 0;
        }
        else
        {

            LowLimit = Parts.Wrist.Joint.lowAngularXLimit.limit;
            MaxLimit = 0;
        }
        _curWristTargetRotation += Mathf.Clamp(wristRotation, -1, 1) * Wrist.RotationSpeed;
        _curWristTargetRotation = Mathf.Clamp(
            _curWristTargetRotation,
            LowLimit,
            MaxLimit);

        //IsWristRotating = true;
    }



    /*
     * END OF CUSTOM CODE
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * */

    /// <summary>
    /// Starts bending all fingers based on <see cref="HandPhysicsExtenstions.FingersConfig.BendSpeed"/>
    /// </summary>
    public void StartBendFingers()
    {
        for (int i = 0; i < Parts.Fingers.Length; i++)
            Parts.Fingers[i].StartBending();
    }

    /// <summary>
    /// Stops bending all fingers
    /// </summary>
    public void StopBendFingers()
    {
        for (int i = 0; i < Parts.Fingers.Length; i++)
            Parts.Fingers[i].StopBending();

        DetachObject();
    }

    /*
     * 
     * 
     * CUSTOM CODE - PETER
     * 
     * 
     * 
     * 
     * 
     * 
     */


    public void StartOpenFingers()
    {
        for (int i = 0; i < Parts.Fingers.Length; i++)
            Parts.Fingers[i].StartOpening();
        
        IsHandOpening = true;
    }
    public void StopOpenFingers()
    {
        for (int i = 0; i < Parts.Fingers.Length; i++)
            Parts.Fingers[i].StopOpening();

        DetachObject();
        IsHandOpening = false;
    }

    /*
     * 
     * 
     * 
     * END OF CUSTOM CODE
     * 
     * 
     * 
     * 
     */
    #endregion
    /// <summary>
    /// Starts bending certain finger based on <see cref="HandPhysicsExtenstions.FingersConfig.BendSpeed"/>
    /// </summary>
    public void StartBendFinger(FingerType fingerType)
    {
        Parts.Fingers[(int)fingerType].StartBending();
    }

    /// <summary>
    /// Stops bending certain finger
    /// </summary>
    public void StopBendFinger(FingerType fingerType)
    {
        Parts.Fingers[(int)fingerType].StopBending();

        int holdersCount = 0;
        for (int i = 1; i < Parts.Fingers.Length; i++)
            if (Parts.Fingers[i].IsHoldingObject)
                holdersCount++;

        if (holdersCount == 0)
            DetachObject();
    }


    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!DrawGizmos)
            return;
        
        DrawPartsLinks();
        DrawPositionLimits();
    }

    void DrawPartsLinks()
    {
        if (!Parts.CheckIfNull())
        {
            Color hc = Color.green;
            hc.a = 0.2f;
            Handles.color = hc;

            float cubeSize = 0.025f * transform.localScale.magnitude;

            Handles.CubeCap(-1, Parts.Forearm.transform.position, Parts.Forearm.transform.rotation, cubeSize);
            Handles.CubeCap(-1, Parts.Wrist.transform.position, Parts.Wrist.transform.rotation, cubeSize);
            Handles.DrawLine(Parts.Forearm.transform.position, Parts.Wrist.transform.position);

            for (int i = 0; i < Parts.Fingers.Length; i++)
            {
                FingerPart[] fParts = Parts.Fingers[i].Parts;
                Handles.CubeCap(-1, fParts[0].transform.position, fParts[0].transform.rotation, cubeSize);
                Handles.DrawLine(Parts.Wrist.transform.position, fParts[0].transform.position);
                for (int j = 1; j < fParts.Length; j++)
                {
                    Handles.CubeCap(-1, fParts[j].transform.position, fParts[j].transform.rotation, cubeSize);
                    Handles.DrawLine(fParts[j - 1].transform.position, fParts[j].transform.position);
                }
            }
        }
    }

    void DrawPositionLimits()
    {
        if (!Forearm.PositionLimits.UseLimits)
            return;

        Color limitsColor = Color.red;
        limitsColor.a = 0.5f;
        Gizmos.color = limitsColor;

        var posLimits = Forearm.PositionLimits;

        Vector3 center = Vector3.zero;
        if (posLimits.Relativity == Space.Self)
            center = transform.position;

        Gizmos.DrawWireCube(center + posLimits.BoundsOffset, Forearm.PositionLimits.Bounds);
    }
    #endif
}

namespace HandPhysicsExtenstions
{
    #region Classes
    [Serializable]
    public sealed class ForearmConfig
    {
        [Tooltip("Determines coordinate system for movement. Use Space.Self if forearm rotation potentially will be different from Vector3.zero")]
        public Space MovementRelativity = Space.World;
        [Tooltip("Movement speed for each axis in local space")]
        public Vector3 MovementSpeed = new Vector3(8, 120, 8);

        [Range(0f, 1f)]
        [Tooltip("Determines movement responsivity. Set it to 1 for full control")]
        public float MovementHardness = 0.125f;

        [Space]
        [Tooltip("Rotation speed along joint X axis")]
        public float RotationSpeed = 8;
        [Range(0f, 1f)]
        [Tooltip("Determines rotation responsivity. Set it to 1 for full control")]
        public float RotationHardness = 0.6f;

        [Space]
        [Tooltip("Fixing forearm position when holding an object to prevent falling down due to mass differences")]
        public AxesFixation FixAxis;
        [Tooltip("Sets forearm position limits so forearm will never be out of specified bounds")]
        public PositionLimits PositionLimits;
    }

    [Serializable]
    public sealed class WristConfig
    {
        [Tooltip("Rotation speed along joint X axis")]
        public float RotationSpeed = 8;

        [Range(0f, 1f)]
        [Tooltip("Determines rotation responsivity. Set it to 1 for full control")]
        public float RotationHardness = 0.2f;
        [Tooltip("If enabled, wrist will always return to its initial rotation")]
        public bool HoldRotation = true;
    }

    [Serializable]
    public sealed class FingersConfig
    {
        [Tooltip("Sets rotation mode for finger parts")]
        public FingerRotationMode RotationMode = FingerRotationMode.Smooth;
        [Tooltip("Determines rotation speed for all finger parts")]
        public float BendSpeed = 8;
        [Space]
        [Tooltip("If enabled, only objects with PickableObject component attached may be picked")]
        public bool AttachPickableOnly = false;
        [Tooltip("Determines joint break force for attached object")]
        public float ForceToDetachObject = Mathf.Infinity;
    }
    #endregion

    #region Structs
    [Serializable]
    public struct HandParts
    {
        public HandPart Forearm;
        public HandPart Wrist;
        public Finger[] Fingers;

        /// <summary>
        /// Assigns provided controller to all hand parts
        /// </summary>
        /// <param name="controller"></param>
        public void SetController(HandPhysicsController controller)
        {
            var allParts = GetAll();
            for (int i = 0; i < allParts.Length; i++)
            {
                allParts[i].Controller = controller;
            }
        }

        /// <summary>
        /// Enables or disable collisions between all <see cref="HandParts"/> and provided <see cref="rigidbody"/>
        /// </summary>
        /// <param name="rigidbody"></param>
        /// /// <param name="ignore"></param>
        public void IgnoreCollisions(Rigidbody rigidbody, bool ignore)
        {
            Collider rbCollider = rigidbody.GetComponent<Collider>();
            if (rbCollider == null)
                return;

            
            var allParts = GetAll();
            for (int i = 0; i < allParts.Length; i++)
                Physics.IgnoreCollision(rbCollider, allParts[i].Collider, ignore);
        }

        /// <summary>
        /// Turns off all collisions betweel all hand parts
        /// </summary>
        public void DisableAllCollisions()
        {
            var allParts = GetAll();
            for (int i = 0; i < allParts.Length; i++)
                for (int j = 0; j < allParts.Length; j++)
                    Physics.IgnoreCollision(allParts[i].Collider, allParts[j].Collider, true);
        }

        /// <summary>
        /// Returns array of all hand parts
        /// </summary>
        /// <returns></returns>
        public HandPart[] GetAll()
        {
            var allParts = new List<HandPart> { Forearm, Wrist };
            if (Fingers.Length == 0)
            {
                allParts.Add(null);
                return allParts.ToArray();
            }
                
            for (int i = 0; i < Fingers.Length; i++)
            {
                if (Fingers[i] != null)
                    allParts.AddRange(Fingers[i].Parts);
                else allParts.Add(null);
            }

            return allParts.ToArray();
        }

        /// <summary>
        /// Returns true if all parts are linked
        /// </summary>
        /// <returns></returns>
        public bool CheckIfNull()
        {
            HandPart[] allParts = GetAll();
            for (int i = 0; i < allParts.Length; i++)
            {
                if (allParts[i] == null)
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public struct PositionLimits
    {
        public bool UseLimits;
        public Space Relativity;

        public Vector3 Bounds;
        public Vector3 BoundsOffset;
    }

    [Serializable]
    public struct AxesFixation
    {
        [Range(0f, 1f)]
        public float Hardness;

        public bool X;
        public bool Y;
        public bool Z;
    }
    #endregion

    #region Enums
    public enum FingerType
    {
        Thumb = 0, Index, Middle, Ring, Pinky
    }

    public enum FingerRotationMode
    {
        Linear, Smooth
    }
    #endregion

    #region Delegates
    public delegate void ObjectTouched();
    public delegate void ObjectAttached(Rigidbody rigidbody);
    public delegate void ObjectDetached();
    #endregion
}