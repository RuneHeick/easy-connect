#pragma once 
#include "OSAL.h"
#include "GenericValueManger.h"

#define SYSIDSIZE 20


extern uint8 SystemID[SYSIDSIZE];
extern GenericValue DeviceName, Password; 

extern void InfoProfile_AddService();
