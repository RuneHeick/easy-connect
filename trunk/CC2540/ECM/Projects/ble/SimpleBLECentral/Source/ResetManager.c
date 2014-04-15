#include "ResetManager.h"
#include "OnBoard.h"

static ResetCallBack ResetManager_callBack = NULL; 

void ResetManager_checkForReset()
{
  ResetType_t reson = ResetReason(); 
  if(ResetManager_callBack)
    ResetManager_callBack(reson);
}

void ResetManager_Reset(bool soft)
{
  if(soft)
  {
    Onboard_soft_reset(); 
  }
  else
  {
    SystemReset();
  }
}

void ResetManager_RegistreResetCallBack(ResetCallBack call)
{
  ResetManager_callBack = call; 
}



void ResetManager_StartWatchDog()
{
   WD_START();
}

void ResetManager_ClearWatchDog()
{
  WD_KICK();
}