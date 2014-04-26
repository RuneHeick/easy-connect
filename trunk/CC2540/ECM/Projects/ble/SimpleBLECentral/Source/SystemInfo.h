#pragma once 
#include "OSAL.h"
#include "GenericValueManger.h"

#define SYSIDSIZE 8

typedef void (*ChangeCallBack)(void); 

extern uint8 SystemID[SYSIDSIZE];
extern GenericValue DeviceName, Password; 

extern void InfoProfile_AddService();
extern void SystemInfo_Change(ChangeCallBack call);