/*
UDP Control of Virtual enviroment
*/
#include<stdio.h>
#include "VE_Comms.h"
#include<winsock2.h>
#include <iostream>

// for string validation 
#include <string.h>
#include <sstream>
#pragma comment(lib,"ws2_32.lib") //Winsock Library

#define SERVER "127.0.0.1"  //ip address of udp server
#define BUFLEN 512  //Max length of buffer
char buf[BUFLEN];
#define PORT 8888   //The port for outgoing data
#define PORT_S 8887   //The port  for incoming data
struct sockaddr_in si_other;
int s, slen = sizeof(si_other);
//char buf[BUFLEN];
char message[BUFLEN];
WSADATA wsa;
char* returned;

int testing = 5;
namespace VE_Comm
{
	// Simple function to compare strings
	int checkSTring(char* String1, const char* ValidationString, int size)
	{
		for (int i = 0; i < size; i++) {
			if (String1[i] != ValidationString[i])
				return 0;
		}
		return 1;
	}
	//gesture joints joint 
	//Validates if entered command is correct, returns false if a bad command is sent. not presently necessary
	int validate(char* inputString) {
		bool CheckFirst = false;
		int inputStringSize = 0;
		char * pch;
		
		pch = strtok(inputString, "_");
		while (pch != NULL)
		{
			printf("%s \n", pch);
			if (!CheckFirst)
			{
				inputStringSize = (strlen(pch));
				//int i = 0;
				//for (i = 0; pch[i] != '\0'; i++);
				//for (i = 0; isdigit(pch[i]); i++);

				//	inputStringSize = i; //sizeof(pch[0]);
				if (inputStringSize == 5)
				{
					if (!checkSTring(pch, "Joint ", 5))
						return false;
				}
				else if (inputStringSize == 6)
				{
					if (!checkSTring(pch, "Joints", 6))
						return false;
				}
				else if (inputStringSize == 7)
				{
					if (!checkSTring(pch, "Gesture", 7))
						return false;
				}
				else
					return 0;

				CheckFirst = true;
			}
			else
				if (!isdigit(pch[0]))
				{
					return 0;

				}
					//printf("%s\n", pch);
				pch = strtok(NULL, "_");
				
		}
		return 1;

	}
	int init_Variables() {

		//VE_Comms::w0 = { 6, 0 };
		//VE_Comms::w1 = { 6,1 };
		return 1;
	}

	// Initializes the UDP server for usage.
	int VE_Comms::InitializeServer() 
	{
		init_Variables();

		//Initialise winsock
		printf("\nInitialising Winsock...");
		if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		{
			printf("Failed. Error Code : %d", WSAGetLastError());
			exit(EXIT_FAILURE);
		}
		printf("Initialised.\n");

		//create socket
		if ((s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == SOCKET_ERROR)
		{
			printf("socket() failed with error code : %d", WSAGetLastError());
			exit(EXIT_FAILURE);
		}
		//setup address structure
		memset((char *)&si_other, 0, sizeof(si_other));
		si_other.sin_family = AF_INET;
		si_other.sin_port = htons(PORT);
		si_other.sin_addr.S_un.S_addr = inet_addr(SERVER);

		return 1;
	}

	int VE_Comms::CloseServer() 
	{
		closesocket(s);
		WSACleanup();
		
		return 1;
	}
	// Retrieves all cuurrent joint angles
	char* VE_Comms::getAllJointAngles() {
		return SendUDP("all~", true);
	}
	char* VE_Comms::getJointAngles(JointRotation joints[],int size) {
		char FormatedString[100];
		std::ostringstream oss;
		for (int x = 0; x < size; x++) {
			//sprintf(FormatedString, "%s%d%c%d%");
			
			if (x == size - 1) {
				oss << joints[x]._Joint.FingerNumber << ":" << joints[x]._Joint.JointNumber <<'~';
			}
			else {
				oss << joints[x]._Joint.FingerNumber << ":" << joints[x]._Joint.JointNumber << ',';
			}
		}
		std::string completedString= oss.str();
		sprintf(FormatedString, completedString.c_str());
		std::cout << "sending: " << FormatedString<< std::endl;
		return SendUDP(FormatedString, true);
	}

	// Sends a formated UDP packet to the Virtual enviroment
	char* VE_Comms::SendUDP(const char* input, bool getJointPositions)
	{


		
		//start communication

		if (getJointPositions) {


			sprintf(message, "JA~%s",input);
		}
		else {
			sprintf(message, input);
		}
		//if (validate(message))
		//{
		
			if (sendto(s, message, strlen(message), 0, (struct sockaddr *) &si_other, slen) == SOCKET_ERROR)
			{
				printf("sendto() failed with error code : %d", WSAGetLastError());
				exit(EXIT_FAILURE);
			}
			//receive a reply and print it
			//clear the buffer by filling null, it might have previously received data
			memset(buf, '\0', BUFLEN);


			if (getJointPositions) {
				//try to receive some data, this is a blocking call
				if (recvfrom(s, buf, BUFLEN, 0, (struct sockaddr *) &si_other, &slen) == SOCKET_ERROR)
				{
					printf("recvfrom() failed with error code : %d", WSAGetLastError());
					exit(EXIT_FAILURE);
				}

				puts(buf);
				return buf;
			}

			return "1";
		}



	// Takes a single joint command and request for retrieving joint positions before formatting and sending to virtual enviroment.
	// Joint position request defaults to false if not entered.
	char*  VE_Comms::SendJoint(JointRotation JointCommand, bool getJointPositions) {

		
		char FormatedString[256];
		char US = '_';
		sprintf(FormatedString, "Joint_%d%c%d%c%f%c%f", JointCommand._Joint.FingerNumber, '_',JointCommand._Joint.JointNumber,'_',JointCommand.angle1,',', JointCommand.angle2);
		//sprintf(FormatedString, "Joint_%d%c", JointCommand._Joint.FingerNumber,US);
		printf("Jointcommand: : %s", FormatedString);
		returned = SendUDP(FormatedString, getJointPositions);
		return returned;
	}
	// Takes a single joint command, including the speed to move, and a request for joint positions, formats list for virtual enviroment
	char*  VE_Comms::SendJoint(JointRotation JointCommand,int speed, bool getJointPositions) {


		char FormatedString[100];
		sprintf(FormatedString, "Joint_%d%c%d%c%f%c%f%c%d", JointCommand._Joint.FingerNumber, '_', JointCommand._Joint.JointNumber, '_', JointCommand.angle1, ',', JointCommand.angle2,'_',speed);
		returned = SendUDP(FormatedString, getJointPositions);
		return returned;
	}
	// Takes a list of Joint Commands, loops through the list of joint commands given and formats into a string to send the the virtual enviroment
	// includes a request to return all joint positions, defaults to false if not used.
	char*  VE_Comms::SendJoints(JointRotation JointCommand[],int size, bool getJointPositions) {


		char FormatedString[1280];
		sprintf(FormatedString, "Joints;");
		;
		for (int j = 0; j < size; j++)
		{
			if (j == size-1) {


				sprintf(FormatedString, "%sJoint_%d%c%d%c%f%c%f%c", FormatedString, JointCommand[j]._Joint.FingerNumber, '_', JointCommand[j]._Joint.JointNumber, '_', JointCommand[j].angle1, ',', JointCommand[j].angle2);
			}
			else
			{
				sprintf(FormatedString, "%sJoint_%d%c%d%c%f%c%f%c", FormatedString, JointCommand[j]._Joint.FingerNumber, '_', JointCommand[j]._Joint.JointNumber, '_', JointCommand[j].angle1, ',', JointCommand[j].angle2, ';');
			}
		}

		returned = SendUDP(FormatedString, getJointPositions);
		return returned;
	}
	// Takes a list of Joint Commands, loops through the list of joint commands given and formats into a string to send the the virtual enviroment
	// Includes a speed command, alongside request for joint positions to be returned too.
	char*  VE_Comms::SendJoints(JointRotation JointCommand[], int size, int speed, bool getJointPositions) {


		char FormatedString[512];
		sprintf(FormatedString, "Joints-");
		for (int j = 0; j < sizeof(JointCommand); j++)
		{
			if (j == sizeof(JointCommand - 1)) {


				sprintf(FormatedString, FormatedString, "Joint_", JointCommand[j]._Joint.FingerNumber, '_', JointCommand[j]._Joint.JointNumber, '_', JointCommand[j].angle1, ',', JointCommand[j].angle2, '_', speed);
			}
			else
			{
				sprintf(FormatedString, FormatedString, "Joint_", JointCommand[j]._Joint.FingerNumber, '_', JointCommand[j]._Joint.JointNumber, '_', JointCommand[j].angle1, ',', JointCommand[j].angle2, '_', speed, ';');
			}
		}

		returned = SendUDP(FormatedString, getJointPositions);
		return returned;
	}
	// Takes a single gesture command and request for retrieving joint positions before sending to virtual enviroment
	char* VE_Comms::SendGesture(int Gesture, bool getJointPositions)
	{
		
		char FormatedString[32];
		sprintf(FormatedString, "Gesture_",Gesture);
		returned = SendUDP(FormatedString,getJointPositions);
		return returned;
	}

	// Sends a command to the Virtual Enviroment to begin a training session
	int VE_Comms::startTraining() {
		SendUDP("Training");
		return 1;
	}


		//else
		//	return 0;
	//}
	
}