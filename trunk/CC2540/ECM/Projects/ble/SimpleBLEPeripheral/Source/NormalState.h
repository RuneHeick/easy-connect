#pragma once
#include "State.h"

extern uint16 NormalState_ProcessEvent( uint8 task_id, uint16 events );

extern void NormalState_Enter(uint8); 

extern void NormalState_Exit(); 
