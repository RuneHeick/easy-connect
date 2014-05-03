#pragma once
#include "State.h"

typedef struct 
{
  uint8 count; 
  uint8* pValue;
}pBuffer_t;


extern uint16 InitState_ProcessEvent( uint8 task_id, uint16 events );

extern void InitState_Enter(uint8); 

extern void InitState_Exit(); 


//MODES:
//GAP_ADTYPE_FLAGS_NON                    
//GAP_ADTYPE_FLAGS_LIMITED
//GAP_ADTYPE_FLAGS_GENERAL
extern void Setup_discoverableMode(uint8 mode, bool hasData,uint16 handel);

extern pBuffer_t GAPManget_GetName();
extern uint8 GAPManget_SetupName(char* DeviceName, uint8 nameSize);