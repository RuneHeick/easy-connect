#pragma once
#include "OSAL.h"

extern uint16 UartReadWrite_ProcessEvent( uint8 task_id, uint16 events );

extern void UartReadWrite_Init(uint8 taskid);

extern void UartReadWrite_UpdateHandle(uint16 handle);