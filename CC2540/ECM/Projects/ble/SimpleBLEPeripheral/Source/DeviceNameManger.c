/*********************************************************************
 * INCLUDES
 */

#include "DeviceNameManger.h"
#include <stdio.h>

//This Contains the Device Name. 
static uint8* NamePtr; 
static uint8 NameLength = 0; 
static bool DeviceMangerIsInit = false; 

uint8* deviceName_GetDeviceName()
{
  return NamePtr; 
}


uint8 deviceName_GetNameLen()
{
  return NameLength;
}


//returns len of Name, and puts name in buffer
uint8 deviceName_GetName(uint8* buffer,uint8 bufferSize,uint8 count)
{
  if(DeviceMangerIsInit==true)
  {
   if(bufferSize>=count&&count<=NameLength)
   {
      osal_memcpy(buffer, NamePtr, count);
      return NameLength;
   }
  }
  return 0; 
}


// set name, must have at end \0 
bool deviceName_SetName(uint8* Name)
{
  uint8 len = 0; 
  while(Name[len]!='\0')
  {
    len++; // no end char; 
  }
  
  if(DeviceMangerIsInit==true)
    osal_mem_free(NamePtr);    
  
  NamePtr = osal_mem_alloc(len);
  if(NamePtr==NULL)
  {
    printf("Name Error"); 
    deviceName_Reset();
    return false; 
  }
  osal_memcpy(NamePtr, Name, len);
  NameLength = len; 
  DeviceMangerIsInit = true;
  return true; 
}

void deviceName_Reset()
{
   if(DeviceMangerIsInit==true)
   {
      osal_mem_free(NamePtr); 
      NameLength = 0; 
      DeviceMangerIsInit = false; 
   }
}

