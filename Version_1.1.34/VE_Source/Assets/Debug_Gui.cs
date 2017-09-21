using UnityEngine;
using System.Collections;

public class Debug_Gui : MonoBehaviour {
    //Debug.Log(string.Format("fps (approx) = {0}", (1.0f / Time.deltaTime)));
    float MaxFPS;
    float MinFPS;
    float FPS ;
    // Use this for initialization
    void Start() {


    }

    // Update is called once per frame
    void Update() {

        FPS = 1.0f / Time.deltaTime;
        if (MaxFPS < FPS)
        {
            MaxFPS = FPS;
        }
        else if (MinFPS > FPS || FPS < 30)
        {
            MinFPS = FPS;
        }


    }
    void OnGUI()
    {

        Rect rectObj = new Rect(50, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# Current FPS:" + FPS + "\nMax FPS: "+MaxFPS+ "\nMin FPS:"+MinFPS + "\nMs per update: "+(1000/FPS)

                , style);
    }
}
