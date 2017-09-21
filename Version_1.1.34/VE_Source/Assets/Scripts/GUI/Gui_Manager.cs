
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class Gui_Manager : MonoBehaviour {
    public UDPReceive UDPServer;
    public UI_Manager UI_Control;
    //public PauseMenuBehavior pauseController;
    GameObject[] StartButton;
    public string lastInput;
    private string PauseButton = "Cancel";
    private bool paused = false;
    private bool hasStart = false;
    public bool gameOver = false;
    // Use this for initialization
    void Start () {
        StartButton = GameObject.FindGameObjectsWithTag("StartButton");
        
        paused = togglePause();

    }
	
	// Update is called once per frame
	void Update () {
        if (!hasStart)
        {
            Time.timeScale = 0f;
        }
        if (!gameOver)
        {
          /*  if (Input.GetButtonDown(PauseButton))
            {
                paused = pauseController.togglePause();
            }*/
        }
        else
        {
            
            setGameOver();
        }
	}

    public void onStartButton()
    {
        foreach (GameObject g in StartButton)
        {

            g.SetActive(false);
        }
        hasStart = true;
        togglePause();

    }
    public void setLastInput(string newLast)
    {
        lastInput = newLast;
    }

    // OnGUI
    void OnGUI()
    {
        
        Rect rectObj = new Rect(Screen.width - 250, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# Hand Control Commands: #\n" + UDPServer.lastReceivedUDPPacket
                     
                , style);


        /*if (paused)
        {
            GUILayout.Label("Game is paused!");
            if (GUILayout.Button("Click me to Begin"))
                paused = togglePause();
            else if(GUILayout.Button("Quit"))
                Application.Quit();
            //if (GUILayout.Button("Click me to end socketConnetion"))
            //   UDPServer.OnDisable();
        }*/

        /////////////////////////////////////////

        /*        Rect rectObj2 = new Rect(10, 10, 200, 400);
                GUIStyle style1 = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
                GUI.Box(rectObj2, "Pressing the 'escape' button closes the session \n" + getWristRotate() + "\n" + getJointAngleLoc()
                        , style1);*/
    }
    bool togglePause()
    {
        Debug.Log(string.Format("toggling pause"));
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            return (false);
        }
        else
        {
            Time.timeScale = 0f;
            return (true);
        }
    }
    bool setGameOver()
    {
        
        Time.timeScale =0f;
        UI_Control.showGameOver();
        return true;
    }
}
