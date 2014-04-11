#pragma once 
#include "OSAL.h"

typedef enum  //from manual p.64
{
  PowerOn = 0x00, // can be Brownout 
  External = 0x01, 
  Watchdog = 0x02,
  ClockLoss = 0x03,  
}ResetType_t; 

typedef void(*ResetCallBack)(ResetType_t);

extern void ResetManager_checkForReset(); 

extern void ResetManager_Reset(bool soft); 

extern void ResetManager_RegistreResetCallBack(ResetCallBack);

extern void ResetManager_ClearWatchDog();

extern void ResetManager_StartWatchDog();