

using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{

    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    public HandPhysicsController Controller;

    // public
    // public string IP = "127.0.0.1"; default local
    public int port; // define > init
    
    // infos
    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = "";
    
    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

    private string lastWristRotate = "";
    float presentJointAngle = 0;
    public bool recievedNew = false;
    /// <summary>
    /// Does the server need to send joint angles? 
    /// </summary>
    public bool sendJoints = false;

    /// <summary>
    /// Contains a string of all joints to send the current angles of. If set to "All" then all joints will be returned.
    /// </summary>
    public string JointstoSend = "";
    /// <summary>
    /// signals is a training command has been sent to the VE. 
    /// </summary>
    public bool training = false;
    // start from shell
    private int recievedPackets;
    public string[] allReceivedUDPPacketArray = new string[10];


    public void setWristRotate(Vector3 inputAngle) 
    {
        lastWristRotate = string.Format("Wrist Rotation: {0} ", inputAngle);
    }
    public void setJointAngleLoc(float jointAngle)
    {
        presentJointAngle = jointAngle;
    }
    public float getJointAngleLoc()
    {
        return presentJointAngle ;
    }

    public string getWristRotate()
    {
        return lastWristRotate;
    }

private static void Main()
    {
        UDPReceive receiveObj = new UDPReceive();
        receiveObj.init();

        string text = "";
        do
        {
            text = Console.ReadLine();
        }
        while (!text.Equals("exit"));
    }
    
    public void Start()
    {

        init();
    }




    // init
    private void init()
    {

        print("UDPSend.init()");

        // define port
        port = 8888;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");



        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }


    // receive thread
    private void ReceiveData()
    {

        client = new UdpClient(port);
        while (true)
        {

            try
            {
                // recieve bytes
                
                byte[] data = client.Receive(ref anyIP);
                if (recievedPackets == 100) {
                    recievedPackets = 0;
                }
                else
                {
                    recievedPackets++;
                }
                    

                // envode bytes to UTF8.
                string text = Encoding.UTF8.GetString(data);
                // Check for request for joint information.
                var controlInput_A = text.Split('~');
                if (controlInput_A[0] == "JA")
                {

                    if (controlInput_A.Length > 1)
                        text = controlInput_A[2];
                    JointstoSend = controlInput_A[1];
                    sendJoints = true;
                }
                else if (controlInput_A[0] == "Training")
                {
                    training = true;
                }
                    
                    print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                //allReceivedUDPPackets = allReceivedUDPPackets + "\n" +text;
                allReceivedUDPPacketArray[recievedPackets % 10] = text;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
    public void sendJointAngles(string angles)
    {
        byte[] response = Encoding.UTF8.GetBytes(angles);
        client.Send(response, response.Length, anyIP);
        sendJoints = false;
    }
    public void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }

    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }

    private string CheckJointRequest(string text)
    {

            sendJointAngles(Controller.getAllJointAngles());
        
        return text;
    }
}