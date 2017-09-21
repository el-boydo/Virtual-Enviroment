#pragma once

#include <atlstr.h>
// Namespace of VE_Comms Module for communication between external controllers and Virtual Enviroment
namespace VE_Comm
{
	//Virtual Enviroment Communication class file to communicate between external controller and Virtual Enviroment
	class VE_Comms
	{
	public:
		/**
		Container of a single Joint rotation command
		@param int FingerNumber -  Individual FingerNumber 0-4 start from Thumb, 6 for Wrist articulation and rotation
		@param int JointNumber - Individual Joint values between 0-2 inclusive from palm to fingertip, 0,1 for wrist.
		*/
		typedef struct
		{
			int FingerNumber;
			int JointNumber;
			
		} Joint;
		/**
		Container of a single Joint rotation command
		@param Joint JointName  - Selection joint to perform action upon
		@param float angle1 - Angle for Joint to rotate to
		@param float angle2 - Used for complex joints, such as (thumb angular rotation), defaults to 0
		*/
		typedef struct 
		{
			Joint _Joint;
			float angle1; 
			float angle2;

		} JointRotation;

	private:
		// Example of joint variables
		//thumb,index,middle ring,small, 
		//static Joint w0; // Wrist/forearm rotation
		//static Joint w1; // Wrist Articulations 
		Joint w2 = { 5,2 }; // Wrist Articulations 
		Joint w1 = { 5,1 }; // Wrist Articulations 
		Joint w0 = { 5,0 }; // forearm Articulations 
		Joint t0 = { 0,0 }; // Thumb Metacarpophalangeal (MP) Joint
		Joint t1 = { 0,1 }; // Thumb Proximal Interphalangeal (PIP) Joint
		Joint t2 = { 0,2 }; // Thumb Distal Interphalangeal (DIP) Joint
		Joint i0 = { 1,0 }; // Thumb Metacarpophalangeal (MP) Joint
		Joint i1 = { 1,1 }; // Index Finger Proximal Interphalangeal (PIP) Joint
		Joint i2 = { 1,2 }; // Index Finger Distal Interphalangeal (DIP) Joint
		Joint m0 = { 2,0 }; // Middle Finger Metacarpophalangeal (MP) Joint
		Joint m1 = { 2,1 }; // Middle Finger Proximal Interphalangeal (PIP) Joint
		Joint m2 = { 2,2 }; // Middle Finger Distal Interphalangeal (DIP) Joint
		Joint r0 = { 3,0 }; // Ring Finger Metacarpophalangeal (MP) Joint
		Joint r1 = { 3,1 }; // Ring Finger Proximal Interphalangeal (PIP) Joint
		Joint r2 = { 3,2 }; // Ring Finger Distal Interphalangeal (DIP) Joint
		Joint s0 = { 4,0 }; // Small Finger Metacarpophalangeal (MP) Joint
		Joint s1 = { 4,1 }; // Small Finger Proximal Interphalangeal (PIP) Joint
		Joint s2 = { 4,2 }; // Small Finger Distal Interphalangeal (DIP) Joint

	public:
		JointRotation T0 = { t0,0 };
		JointRotation T1 = { t1,0 };
		JointRotation T2 = { t2,0 };
		JointRotation I0 = { i0,0 };
		JointRotation I1 = { i1,0 };
		JointRotation I2 = { i2,0 };
		JointRotation M0 = { m0,0 };
		JointRotation M1 = { m1,0 };
		JointRotation M2 = { m2,0 };
		JointRotation R0 = { r0,0 };
		JointRotation R1 = { r1,0 };
		JointRotation R2 = { r2,0 };
		JointRotation S0 = { s0,0 };
		JointRotation S1 = { s1,0 };
		JointRotation S2 = { s2,0 };
		JointRotation W0 = { w0,0,0 };
		JointRotation W1 = { w1,0,0 };
		JointRotation W2 = { w2,0,0 };



		/**
		Formats and sends a joint command.

		@param gesture - the individual digit of the motion class to perform
		@param bool getJointPositions - defaults as false, set to true to return joint angles
		@return either 1 or all Joint Angles completion, dependant upon the boolean value of getJointPositions
		*/
		static char*  SendJoint(JointRotation JointCommand, bool getJointPositions = false);
		/**
		Formats and sends a joint command with speed modifier.

		@param gesture - the individual digit of the motion class to perform
		@param speed - The speed to perform the motion (ideally <20)
		@param bool getJointPositions - defaults as false, set to true to return joint angles
		@return either 1 or all Joint Angles completion, dependant upon the boolean value of getJointPositions
		*/
		static char*  SendJoint(JointRotation JointCommand,int speed, bool getJointPositions = false);
		/**
		Formats and sends a series of joint commands.

		@param gesture - the individual digit of the motion class to perform
		@param bool getJointPositions - defaults as false, set to true to return joint angles
		@return either 1 or all Joint Angles completion, dependant upon the boolean value of getJointPositions
		*/
		static char*  SendJoints(JointRotation JointCommand[],int size, bool getJointPositions = false);
		/**
		Formats and sends a series of joint commands with speed modifier.

		@param gesture - the individual digit of the motion class to perform
		@param speed - The speed to perform the motion (ideally <20)
		@param bool getJointPositions - defaults as false, set to true to return joint angles
		@return either 1 or all Joint Angles completion, dependant upon the boolean value of getJointPositions
		*/
		static char*  SendJoints(JointRotation JointCommand[], int size, int speed, bool getJointPositions = false);
		/**
		Formats and sends a single gesture command. 
		Gestures_1XXXXX (5xX)  \n"
		Substitute each X with 0 or 1 to bend or extend each finger 
		E.G. Hand Close - Gestures_111111 | Hand Open = Gestures_100000  
		@param int gesture - the individual digit of the motion class to perform 
		@param bool getJointPositions - defaults as false, set to true to return joint angles
		@returns a char* of either "1" or all Joint Angles, dependant upon the boolean value of getJointPositions

		*/
		static char* SendGesture(int Gesture, bool getJointPositions = false);

		/* Sends a UDP stream to the Virtual enviroment alongside request for retrieving joint positions
		@param char* input - char array to send to enviroment
		@param bool getJointPositions - defaults to false, set to true to return joint positions

		@returns: "1" if getJointPositions is not set, or set to false, otherwise returns joint positions of hand when command is sent. 
		*/
		static char* SendUDP(const char* input,bool getJointPositions = false);


		/* Sends a signal to the Virtual Enviroment to start training.
		@Returns 1 if succesful 
		*/
		static int startTraining();
		// Returns all joint angles at the present time
		static char* getAllJointAngles();

		static char* getJointAngles(JointRotation joints[], int size);

		static JointRotation GetJointPosition(Joint _joint);

		static JointRotation* GetJointPositions(Joint _joints[]);

		// Initializes the server for sending information to the enviroment
		static int InitializeServer();


		static int CloseServer();
	};

}
