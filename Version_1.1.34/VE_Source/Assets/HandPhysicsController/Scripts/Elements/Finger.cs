using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HandPhysicsExtenstions
{
    public class Finger : MonoBehaviour
    {
        public FingerPart[] Parts = new FingerPart[3];

        /// <summary>
        /// Is this finger currrently bending?
        /// </summary>
        public bool IsBending { get; private set; }

        /// <summary>
        /// Is currently attached object was being attached by this finger?
        /// </summary>
        public bool IsHoldingObject { get; set; }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].ParentFinger = this;
                if (i > 0)
                    Parts[i].PrevPart = Parts[i - 1];
                if (i < Parts.Length - 1)
                    Parts[i].NextPart = Parts[i + 1];
            }
        }
        /// <summary>
        /// Checks if internal joints are all neutral
        /// </summary>
        public bool checkJoints()
        {
            for(int i = 0; i< Parts.Length; i++)
            {
                if (Parts[i].isNeutral == false)
                {
                    return false;
                }
            }
            return true;
        }

        public void StartBending()
        {
          //  if (IsBending)
           //     return;

            for (int i = 0; i < Parts.Length; i++)
            {

                Parts[i].RotationValue = 0;
                Parts[i].ModifiedSpeed = false;
                Parts[i].IsRotating = true;
                Parts[i]._EndRotation = Parts[i].TargetRotation;
                Parts[i]._InitialRotation = Parts[i].transform.localRotation;
                
            }
            IsBending = true;
        }

        public void StopBending()
        {
        //    if (!IsBending)
        //        return;

            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].ModifiedSpeed = true;
                Parts[i].m_bendSpeed = 11;
                Parts[i].RotationValue = 0;
                Parts[i].IsRotating = true;
                Parts[i]._EndRotation = Parts[i].StartRotation;
                Parts[i]._InitialRotation = Parts[i].transform.localRotation;
                
            }


            IsBending = false;
            IsHoldingObject = false;
        }
        /*
         *
         * 
         *  CUSTOM CODE FROM PETER
         *
         *
         *
         *  ANYTHING THAT BREAKS WILL BE BELOW THIS
         *
         *
         *
         *
         */

        public void PrecisionRotate(int jointNumber, Quaternion rotation)
        {

            Parts[jointNumber].RotationValue = 0;
            Parts[jointNumber].ModifiedSpeed = false;
            Parts[jointNumber].IsRotating = true;
            Parts[jointNumber]._EndRotation = rotation;
            Parts[jointNumber]._InitialRotation = Parts[jointNumber].transform.localRotation;
        
            
        }

        public void PrecisionRotate(int jointNumber, Quaternion rotation,int speed)
        {
            Parts[jointNumber].RotationValue = 0;
            Parts[jointNumber].m_bendSpeed = speed;
            Parts[jointNumber].ModifiedSpeed = true;
            Parts[jointNumber].IsRotating = true;
            Parts[jointNumber]._EndRotation = rotation;
            Parts[jointNumber]._InitialRotation = Parts[jointNumber].transform.localRotation;


        }

        public void StartOpening()
        {
           // if (IsBending)
           //     return;

            for (int i = 0; i < Parts.Length; i++)
            {

                Parts[i].RotationValue = 0;
                Parts[i].IsRotating = true;
                Parts[i]._EndRotation = Parts[i].OpenHandRotation;
                Parts[i]._InitialRotation = Parts[i].transform.localRotation;
            }
            IsBending = true;
        }

        public void StopOpening()
        {
         //   if (!IsBending)
        //        return;

            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].RotationValue = 0;
                Parts[i].IsRotating = true;
                Parts[i]._EndRotation = Parts[i].StartRotation;
                Parts[i]._InitialRotation = Parts[i].transform.localRotation;
            }
            IsBending = false;
            IsHoldingObject = false;
        }
        /*
         * 
         * 
         * 
         * 
         * 
         * END OF CUSTOM CODE 
         * 
         * 
         * 
         * 
         */
        /// <summary>
        /// Returns all rigidbodies which collides with this finger
        /// </summary>
        /// <returns></returns>
        public List<Rigidbody> GetAllTouchingObjects()
        {
            List<Rigidbody> allTouchingObjects = new List<Rigidbody>();
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i].IsTouchingAnyObject)
                    allTouchingObjects.AddRange(Parts[i].TouchingObjects);
            }

            return allTouchingObjects;
        } 
    }
}

