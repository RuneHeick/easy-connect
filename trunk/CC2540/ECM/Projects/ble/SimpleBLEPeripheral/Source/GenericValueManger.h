#pragma once 

#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"
#include "hal_types.h"


//Contains the Pointer to the Value. 
typedef struct 
{
  bool IsReady = False;
  uint8* pValue,
  uint8 Size;
}GenericValue; 


//Set an GenericValue to the value, need the length; 
GenericValue_SetValue(GenericValue* item, uint8* value, uint8 len);

GenericValue_SetString(GenericValue* item, uint8* value);

GenericValue_GetValue(GenericValue* item, uint8* value, 