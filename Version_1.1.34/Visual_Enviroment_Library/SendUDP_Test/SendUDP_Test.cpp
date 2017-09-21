// SendUDP_Test.cpp : Defines the entry point for the console application.
//
// Largely a simple test application for string entry, will return joint positions after every command

#include "stdafx.h"
#include <iostream>
#include "VE_Comms.h"
#include<stdlib.h>
#include<stdio.h>

// JOINT NAMES:

VE_Comm::VE_Comms::Joint T0 = { 0,0 };
VE_Comm::VE_Comms::Joint T1 = { 0,1 };
VE_Comm::VE_Comms::Joint T2 = { 0,2 };
VE_Comm::VE_Comms::Joint I0 = { 1,0 };
VE_Comm::VE_Comms::Joint I1 = { 1,1 };
VE_Comm::VE_Comms::Joint I2 = { 1,2 };
VE_Comm::VE_Comms::Joint M0 = { 2,0 };
VE_Comm::VE_Comms::Joint M1 = { 2,1 };
VE_Comm::VE_Comms::Joint M2 = { 2,2 };
VE_Comm::VE_Comms::Joint R0 = { 3,0 };
VE_Comm::VE_Comms::Joint R1 = { 3,1 };
VE_Comm::VE_Comms::Joint R2 = { 3,2 };
VE_Comm::VE_Comms::Joint S0 = { 4,0 };
VE_Comm::VE_Comms::Joint S1 = { 4,1 };
VE_Comm::VE_Comms::Joint S2 = { 4,2 };
VE_Comm::VE_Comms::Joint W0 = { 5,0 };
VE_Comm::VE_Comms::Joint W1 = { 5,1 };
VE_Comm::VE_Comms::Joint W2 = { 5,2 };



// JOINT ROTATIONS

VE_Comm::VE_Comms::JointRotation R_T0 = {T0,0};
VE_Comm::VE_Comms::JointRotation R_T1 = { T1,0 };
VE_Comm::VE_Comms::JointRotation R_T2 = { T2,0 };
VE_Comm::VE_Comms::JointRotation R_I0 = { I0,0 };
VE_Comm::VE_Comms::JointRotation R_I1 = { I1,0 };
VE_Comm::VE_Comms::JointRotation R_I2 = { I2,0 };
VE_Comm::VE_Comms::JointRotation R_M0 = { M0,0 };
VE_Comm::VE_Comms::JointRotation R_M1 = { M1,0 };
VE_Comm::VE_Comms::JointRotation R_M2 = { M2,0 };
VE_Comm::VE_Comms::JointRotation R_R0 = { R0,0 };
VE_Comm::VE_Comms::JointRotation R_R1 = { R1,0 };
VE_Comm::VE_Comms::JointRotation R_R2 = { R2,0 };
VE_Comm::VE_Comms::JointRotation R_S0 = { S0,0 };
VE_Comm::VE_Comms::JointRotation R_S1 = { S1,0 };
VE_Comm::VE_Comms::JointRotation R_S2 = { S2,0 };
VE_Comm::VE_Comms::JointRotation R_W0 = { W0,0,10 };
VE_Comm::VE_Comms::JointRotation R_W1 = { W1,0,10 };
VE_Comm::VE_Comms::JointRotation R_W2 = { W2,0,0 };

// Array of all joint rotations
VE_Comm::VE_Comms::JointRotation JointRotations[] = { R_T0, R_T1, R_T2, R_I0, R_I1, R_I2, R_M0, R_M1, R_M2, R_R0, R_R1, R_R2, R_S0, R_S1, R_S2, R_W0, R_W1, R_W2 };
int size = 17;
using namespace VE_Comm;
int main()
{
	VE_Comms comms;
//	VE_Comms::Joint joint1 = comms.t0;
//	VE_Comms::JointRotation jointr1 = { comms.t0, 0};
	char message[512];

	VE_Comm::VE_Comms::InitializeServer();

	while (1)
	{
		printf("Enter message : ");
		std::cin >> message;
		std::cout << "sending: " << message << std::endl;
		char* returned = VE_Comm::VE_Comms::SendUDP(message,false);
		//VE_Comm::VE_Comms::Joint Joints[] = { {1,0},{2,0},{3,0} };
		//std::cout << "sending: " << Joints << std::endl;

		//comms.T0.angle1 = atof(message);
		//char* returned = VE_Comm::VE_Comms::SendJoint(comms.T0);

		/*for (int x = 0; x < size; x++) {
			JointRotations[x].angle1 = atof(message);
		}*/
		//char* returned = VE_Comm::VE_Comms::SendJoint({ T0 ,30 });
		
		std::cout << "sending: " << JointRotations<< std::endl;
		//char* returned = VE_Comm::VE_Comms::SendJoint(JointRotations[0]);
		//char* returned = VE_Comm::VE_Comms::getJointAngles(Joints, 3);

		//char* returned = VE_Comm::VE_Comms::getAllJointAngles();
		//char* returned = VE_Comm::VE_Comms::SendJoints(JointRotations,size);


		//char* returned = VE_Comm::VE_Comms::SendJoint(JointRotations[0]);
		
		//VE_Comm::VE_Comms::JointRotation *JointRotationPtr = (VE_Comm::VE_Comms::JointRotation*)malloc(sizeof(VE_Comm::VE_Comms::JointRotation));
		//JointRotationPtr->_Joint.FingerNumber = 2;
		//JointRotationPtr->_Joint.JointNumber = 0;
		//JointRotationPtr->X_Rotation = 50;
		//JointRotationPtr->Y_Rotation = 30;
		//JointRotationPtr->Z_Rotation = 30;


		//VE_Comm::VE_Comms::JointRotation myfot{ VE_Comm::VE_Comms::w0, 30, 0, 0};

		//VE_Comm::VE_Comms::SendJoint(JointRotationPtr);

		//std::cout << "sent: " << returned <<  std::endl; 
	}
}

