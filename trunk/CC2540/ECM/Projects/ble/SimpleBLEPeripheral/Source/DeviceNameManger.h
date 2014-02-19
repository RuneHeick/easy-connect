#pragma once 

#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"
#include "hal_types.h"

void deviceName_Reset();

uint8 deviceName_GetNameLen();

//returns len of Name, and puts name in buffer
uint8 deviceName_GetName(uint8* buffer,uint8 bufferSize,uint8 startIndex);

// set name, must have at end \0 
bool deviceName_SetName(uint8* Name);

uint8* deviceName_GetDeviceName();