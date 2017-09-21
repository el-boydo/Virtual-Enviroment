using UnityEngine;
using System.Collections.Generic;

namespace HandPhysicsExtenstions
{
    public class FingerPart : HandPart
    {
        #region Public Fields
        [Tooltip("Target rotation quaternion in local space")]
        public Quaternion TargetRotation = new Quaternion(0, 0, 0, 1);
        [Tooltip("Target rotation quaternion in local space")]
        public Quaternion OpenHandRotation = new Quaternion(0, 0, 0, 1);
        [Tooltip("Target rotation vector3 in local space")]
        public Vector3 Vector3_OpenHandRotation = new Vector3(0, 0, 0);
        [Tooltip("Rotation process from start to target quaternion will be exposed by this curve")]
        public AnimationCurve RotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [Space]
        public FingerPartTrigger Trigger;
        #endregion

        #region Public Properties
        /// <summary>
        /// Start local rotation defined in <see cref="Transform"/> component in editor
        /// </summary>
        public Quaternion StartRotation
        {
            get
            {
                if (Application.isPlaying)
                    return _startRotation;
                return transform.localRotation;
            }
            private set { _startRotation = value; }
        }
        private Quaternion _startRotation;

        public Quaternion _InitialRotation;
        public Quaternion _EndRotation;

        /// <summary>
        /// Returns current rotation value clamped between 0 and 1
        /// </summary>
        public float RotationValue
        {
            get { return _rotationValue; }
            internal set{ _rotationValue = Mathf.Clamp01(value); }
        }
        private float _rotationValue;

        /// <summary>
        /// Is this finger part currently rotating?
        /// </summary>
        public bool IsRotating { get; internal set; }
        /// <summary>
        /// Is rotation currently allowed?
        /// </summary>
        public bool IsRotationAllowed { get; internal set; }

        public float m_bendSpeed { get; internal set; }
        public bool ModifiedSpeed { get; internal set; }

        public bool IsOpening { get; internal set; }

        public bool isNeutral { get; private set; }

        /// <summary>
        /// Returns true if <see cref="Trigger"/> collides with any other <see cref="Rigidbody"/>
        /// </summary>
        public bool IsTouchingAnyObject
        {
            get
            {
                for (int i = 0; i < TouchingObjects.Count; i++)
                    if (TouchingObjects[i] == null)
                        TouchingObjects.RemoveAt(i);
                if (TouchingObjects.Count == 0)
                    _isTouchingAnyObject = false;

                return _isTouchingAnyObject;
            }
            internal set
            {
                _isTouchingAnyObject = value;
                if (value && ParentFinger.IsBending)
                    Controller.OnObjectTouched();
            }
        }
        private bool _isTouchingAnyObject;

        /// <summary>
        /// Returns all objects which collides with <see cref="Trigger"/>
        /// </summary>
        public List<Rigidbody> TouchingObjects { get; internal set; }
        
        public Finger ParentFinger { get; internal set; }
        public FingerPart PrevPart { get; internal set; }
        public FingerPart NextPart { get; internal set; }
        #endregion


        void Start()
        {
            Init();
        }

        void Init()
        {
            TouchingObjects = new List<Rigidbody>();
            Trigger.ParentFingerPart = this;
            StartRotation = transform.localRotation;
            OpenHandRotation = Quaternion.Euler(Vector3_OpenHandRotation);
        }

        void Update()
        {
            ControlPart();
                //Debug.Log(string.Format("rotation value= {0}", (RotationValue)));
        }

        void ControlPart()
        {

            /*
          // find a workaround for the rotation recursively
            if (IsTouchingAnyObject)
                DisallowRotationRecursively();

            if (IsRotating )
            {
                if (IsRotationAllowed)
                {
                    if (NextPart == null)
                    {
                        if (PrevPart.IsRotationAllowed)
                            IncreaseRotationValue(GetRotationAmount());
                    }
                    else IncreaseRotationValue(GetRotationAmount());
                }

            }
            else
            {
                DecreaseRotationValue(GetRotationAmount());
                IsRotationAllowed = true;
            }
            */
            float angle = Quaternion.Angle(StartRotation, transform.localRotation);
            if (angle < 1)
            {
                isNeutral = true;
            }
            else
            {
                //Debug.Log(string.Format("rotation angle= {0}", (angle)));
                isNeutral = false;
            }


            if (IsRotating && canRotate())
            {
                

                IncreaseRotationValue(GetRotationAmount());

                transform.localRotation = Quaternion.Lerp(_InitialRotation, _EndRotation, RotationCurve.Evaluate(RotationValue));
            }

           }

        bool canRotate()
        {
            /*
            Vector3 vectorA = transform.localRotation.eulerAngles;
            Vector3 vectorB = _EndRotation.eulerAngles;
            float angle = Vector3.Angle(vectorA, vectorB);
            */
            //Vector3 cross = Vector3.Cross(vectorA, vectorB);
            //if (cross.x < 0) angle = -angle;

            if (RotationValue >.96)
            {
                IsRotating = false;
                ModifiedSpeed = false;
                RotationValue = 0;
                return false;
            }
            
                return true;
        }

        void calcuLateEndRotation()
        {
            if (Quaternion.Angle(transform.localRotation, StartRotation) > 20)
            {
               
            }
            if (IsRotating)
            {
                if (IsRotationAllowed)
                {
                    if (NextPart == null)
                    {
                        if (PrevPart.IsRotationAllowed)
                            IncreaseRotationValue(GetRotationAmount());
                    }
                    else IncreaseRotationValue(GetRotationAmount());
                }

            }
            else
            {
                DecreaseRotationValue(GetRotationAmount());
                IsRotationAllowed = true;
            }

        }

        void IncreaseRotationValue(float amount)
        {
            switch (Controller.Fingers.RotationMode)
            {
                case FingerRotationMode.Linear:
                    RotationValue += amount;
                    break;
                case FingerRotationMode.Smooth:
                    RotationValue = Mathf.SmoothStep(RotationValue, 1, amount);
                    break;
            }
        }

        void DecreaseRotationValue(float amount)
        {
            switch (Controller.Fingers.RotationMode)
            {
                case FingerRotationMode.Linear:
                    RotationValue -= amount;
                    break;
                case FingerRotationMode.Smooth:
                    RotationValue = Mathf.SmoothStep(RotationValue, 0, amount);
                    break;
            }
        }

        float GetRotationAmount()
        {
            if (ModifiedSpeed)
            {
                return Time.deltaTime * m_bendSpeed;
            }
            else
            {
                return Time.deltaTime * Controller.Fingers.BendSpeed;
            }
        }


        internal void DisallowRotationRecursively()
        {
            IsRotationAllowed = false;
            if (PrevPart != null)
                PrevPart.DisallowRotationRecursively();
        }
    }
}

