#pragma once 

#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"
#include "hal_types.h"


typedef enum
{
  NOT_INIT = 0, 
  READY, 
  IN_USE,
  ERROR
}GenericValue_Status;

//Contains the Pointer to the Value. 
typedef struct 
{
  GenericValue_Status status;
  uint8* pValue;
  uint8 size;
}GenericValue; 


//Set an GenericValue to the value, need the length, return true for succses 
bool GenericValue_SetValue(GenericValue* item, uint8* value, uint8 len);

// set string in item, return true for succses 
bool GenericValue_SetString(GenericValue* item, uint8* value);

//Get value or first part of value, return true for succses  
uint8 GenericValue_GetValue(GenericValue* item, uint8* buffer, uint8 Length);

//Delete item, use to release heap, return true for succses 
bool GenericValue_DeleteValue(GenericValue* item);

//Alloctaes a array
bool GenericValue_CreateContainer(GenericValue* item, uint8 len);