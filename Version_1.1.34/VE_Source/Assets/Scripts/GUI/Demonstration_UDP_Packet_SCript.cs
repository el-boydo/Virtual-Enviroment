using UnityEngine;
using System.Collections;

public class Demonstration_UDP_Packet_SCript : MonoBehaviour {
    public UDPReceive server;
    private string recievedList;
	// Use this for initialization
	void Start () {
        recievedList = "";

    }
	
	// Update is called once per frame
	void Update () {
        updateRecievedList();
	}
    void OnGUI()
    {
        Rect rectObj = new Rect(Screen.width - 250, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "All hand controls \n"
                    + "\nLast Packet: \n" + server.lastReceivedUDPPacket
                    + "\nAll Messages:" + recievedList
                , style);
        Rect rectObj2 = new Rect(10, 10, 200, 400);
        GUIStyle style1 = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        
    }
    void updateRecievedList()
    {
        recievedList = "\n";
        for (int j=0;j < server.allReceivedUDPPacketArray.Length; j++)
        {

            recievedList = recievedList +server.allReceivedUDPPacketArray[j]+"\n";
        }
    }
}
