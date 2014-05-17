#pragma once
#include "OSAL.h"
#include "Uart.h"

extern void UartClearCommands_Init(uint8 taskid);

extern uint16 UartClearCommands_ProcessEvent( uint8 task_id, uint16 events );