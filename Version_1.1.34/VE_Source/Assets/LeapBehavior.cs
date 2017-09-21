using UnityEngine;
using System.Collections.Generic;
using Leap;
using Leap.Unity;

public class LeapBehavior : MonoBehaviour
{
    LeapProvider provider;
    Leap.Vector leapToWorld(Leap.Vector leapPoint, InteractionBox iBox)
    {
        leapPoint.z *= -1.0f; //right-hand to left-hand rule
        Leap.Vector normalized = iBox.NormalizePoint(leapPoint, false);
        normalized += new Leap.Vector(0.5f, 0f, 0.5f); //recenter origin
        return normalized * 100.0f; //scale
    }
    void Start()
    {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
    }

    void Update()
    {
        Frame frame = provider.CurrentFrame;
        List<Hand> hands = frame.Hands;
        for (int h = 0; h < hands.Count; h++)
        {
            Hand hand = hands[h];
            if (hand.IsRight)
            {
                Leap.Vector mappingDiff = new Leap.Vector(-93f,21f,-104f);
                Leap.Vector mappingDiff2 = new Leap.Vector(16f, -31f, 7f);
                Leap.Vector pos = hand.WristPosition; 
                Leap.Vector normalized = frame.InteractionBox.NormalizePoint(pos);
                Leap.Vector center = frame.InteractionBox.Center;
                Leap.Vector testL2W = (leapToWorld(((pos + mappingDiff2) * 50), frame.InteractionBox));
                //transform.position = testL2W.ToVector3();
                //transform.position = hand.PalmPosition.ToVector3();
                /*
                transform.position = hand.PalmPosition.ToVector3() +
                                      hand.PalmNormal.ToVector3() *
                                     ((transform.localScale.y)* .5f + .02f)*10;// */
                Vector3 newHandPos = hand.WristPosition.ToVector3() * 2;
                newHandPos = new Vector3(newHandPos.x, newHandPos.y, newHandPos.z); 
                transform.position = (newHandPos);
                //transform.rotation = hand.Basis.rotation.ToQuaternion();// */
                //
                //transform.rotation = hand.Arm.Basis.rotation.ToQuaternion();//*/
                 //transform.position = hand.PalmPosition.ToVector3();


                 //Debug.Log(string.Format("{0} || {1} || {2} || {3} || {4}", hand.PalmPosition, hand.PalmNormal, hand.PalmPosition.y, (transform.localScale.y) * .5f + .02f, normalized));
                 Debug.Log(string.Format("{0} || {1} || {2} || {3} ", hand.Arm.ElbowPosition.ToVector3(), hand.PalmPosition.ToVector3(),leapToWorld(hand.PalmPosition, frame.InteractionBox), (hand.PalmPosition.ToVector3() +
                                      hand.PalmNormal.ToVector3() *
                                     ((transform.localScale.y) * .5f + .02f) * 10)));
                Debug.DrawLine(hand.Arm.ElbowPosition.ToVector3(), hand.Arm.WristPosition.ToVector3(), Color.red); //Arm
                Debug.DrawLine(hand.WristPosition.ToVector3(), hand.PalmPosition.ToVector3(), Color.white); //Wrist to palm line
                Debug.DrawLine(hand.PalmPosition.ToVector3(), (hand.PalmPosition + hand.PalmNormal * hand.PalmWidth / 2).ToVector3(), Color.black); //Hand Normal
            }
        }
    }
    
}