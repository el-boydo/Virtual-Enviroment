 _   _ _      _               _   _   _                 _                                          
| | | (_)    | |             | | | | | |               | |                                         
| | | |_ _ __| |_ _   _  __ _| | | |_| | __ _ _ __   __| |                                         
| | | | | '__| __| | | |/ _` | | |  _  |/ _` | '_ \ / _` |                                         
\ \_/ / | |  | |_| |_| | (_| | | | | | | (_| | | | | (_| |                                         
 \___/|_|_|   \__|\__,_|\__,_|_| \_| |_/\__,_|_| |_|\__,_|                                         
                                                                                                   
                                                                                                   
______               _  ___  ___                                                                   
| ___ \             | | |  \/  |                                                                   
| |_/ /___  __ _  __| | | .  . | ___                                                               
|    // _ \/ _` |/ _` | | |\/| |/ _ \                                                              
| |\ \  __/ (_| | (_| | | |  | |  __/                                                              
\_| \_\___|\__,_|\__,_| \_|  |_/\___|                                                              
                                                                                                                                                                                              ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ 
|______|______|______|______|______|______|______|______|______|______|______|______|______|______|

Instructions: 

 _____          _        _ _       _   _                                                           
|_   _|        | |      | | |     | | (_)                                                          
  | | _ __  ___| |_ __ _| | | __ _| |_ _  ___  _ __                                                
  | || '_ \/ __| __/ _` | | |/ _` | __| |/ _ \| '_ \                                               
 _| || | | \__ \ || (_| | | | (_| | |_| | (_) | | | |                                              
 \___/_| |_|___/\__\__,_|_|_|\__,_|\__|_|\___/|_| |_|                                              
 ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ 
|______|______|______|______|______|______|______|______|______|______|______|______|______|______|

All C++ was made in Visual studio 2015, yet can be compiled with most other versions of visual Studio. 

Unity Software developed in unity version 5.4.1f1, presently only compiled for Windows systems. Performance on other operating systems may be affected.

Place all unity files and folders into a folder on your computer

The VE comms library, Header, and Object files are kept in the VE_comms library folder. They may be kept with or seperate from the unity files


Included is the console application (SendUDPTest) which can be used for easy testing of gesture control.




 _   _                                                                                             
| | | |                                                                                            
| | | |___  __ _  __ _  ___                                                                        
| | | / __|/ _` |/ _` |/ _ \                                                                       
| |_| \__ \ (_| | (_| |  __/                                                                       
 \___/|___/\__,_|\__, |\___|                                                                       
                  __/ |                                                                            
 ______ ______ __|___/______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ 
|______|______|______|______|______|______|______|______|______|______|______|______|______|______|


Open the Unity project (preferebly in windowed mode) , commands may not always be recognised until the unity window regains focus

The library files may be used as per any other library. 

To send a message or command, the function call is : VE_Comm::VE_Comms::SendUDP(const char* input);

Library returns a "1" on succesful messege sending, or a list of current joint positions if selected. 

All positons are presently held until a new message is sent. 


 _____                                           _                                                 
/  __ \                                         | |                                                
| /  \/ ___  _ __ ___  _ __ ___   __ _ _ __   __| |___                                             
| |    / _ \| '_ ` _ \| '_ ` _ \ / _` | '_ \ / _` / __|                                            
| \__/\ (_) | | | | | | | | | | | (_| | | | | (_| \__ \                                            
 \____/\___/|_| |_| |_|_| |_| |_|\__,_|_| |_|\__,_|___/                                            
 ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ 
|______|______|______|______|______|______|______|______|______|______|______|______|______|______|

All gesture commands are preceeded by "Gesture_" Followed by the gesture number / code


Direct Gestures:
Gestures 1 - Hand at Rest 
Gestures 3 - Closed hand 
Gestures 4 - Flexion 
Gestures 5 - Extension
Gestures 6 - Pronation
Gestures 7 - Supination
Gestures 8 - Fine Pinch


Unique Gestures: 

All fingers on the hand may be controlled by sending the following format: 

Gesture_1XXXXX

Where each X corresponds a finger on the hand starting with the thumb, and moving along until the little finger. 

Substituting the X for a 1 will bend the finger, and a 0 will return the finger to its resting position. 
E.G. Hand Close - Gestures_111111 and Hand Open = Gestures_100000 


           _____ _____                                    
     /\   |  __ \_   _|                                   
    /  \  | |__) || |                                     
   / /\ \ |  ___/ | |                                     
  / ____ \| |    _| |_                                    
 /_/____\_\_|__ |_____|
 ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ ______ 
|______|______|______|______|______|______|______|______|______|______|______|______|______|______|


Accessing namespace: 


After importing library files, all functions can be accessed through:
VE_Comm::VE_Comms::<function name>
Or when using namespace "VE_Comm":
VE_Comms::<function name>  

The class VE_Comms can also be inisitialized as a variable for 

VE_Comms_Var.<function name>



Structs:

VE_Comms::Joint 

Values: 
int FingerNumber; (Denotes fingers from thumb to small finger (0-4) and wrist (5))
int JointNumber;  Denotes individual joints on the finger (0-2), starting from the metacarpal 

VE_Comms::JointRotation 

Joint _Joint; Takes a single joint
float angle1; The main angle of rotation, in degrees. 
float angle2; Alternative angle of rotation (mainly used in thumb or wrist movement)



Functions:


VE_Comms::InitializeServer();

Place this function call early on to activate the UDP server, only needs to be run once. 


char*  VE_Comms::SendJoint(JointRotation JointCommand, bool getJointPositions = false);

Sends a single JointRotation to the Virtual Enviroment. Returns char "1", or joint positions of requested angle is "getJointPositions" set to true. 

static char*  VE_Comms::SendJoint(JointRotation JointCommand,int speed, bool getJointPositions = false);

Sends a single jointRotation to the Virtual enviroment with added functionality to control the speed of the joints rotation. 

Returns either "1" or current joint position if requested.



static char*  VE_Comms::SendJoints(JointRotation JointCommand[],int size, bool getJointPositions = false);

Takes an array of JointRotations, the size of the Array, and a bool for returning joint values. 

Other than taking an Array, this functions exactly like VE_Comms::SendJoint 


static char*  VE_Comms::SendJoints(JointRotation JointCommand[], int size, int speed, bool getJointPositions = false);

Takes an Array of JointRotation, the size of the array, and speed of motion, provides option to also return joint positions. 

Other than the array input, functions identically to VE_Comms::SentJoint with speed modifier



static char* VE_Comms::SendGesture(int Gesture, bool getJointPositions = false);

Takes a single int of gesture input, can return joint angles. 

Presently working gestures are listed in the Commands section. 


static char* VE_Comms::SendUDP(const char* input,bool getJointPositions = false);


Sends a UDP Command to the server , largely used by other functions but can be used directly although it is advised to use the provided functions. 


VE_Comms::startTraining();

Send a command to the Virtual Enviroment to start a training session. Will only start if the Virtual enviroment is in the training mode. 


VE_Comms::getAllJointAngles();

Retrieves all joint angles from the Virtual enviroment and returns them as a char*

VE_Comms::getJointAngles(Joint joints[], int size)

Takes a list of Joint Rotations and Retrieves a char* that contains requested joint angles