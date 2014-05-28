#pragma once
#include "OSAL.h"

extern void GPIO_register(uint8 pin, uint8 format );

extern void GPIO_Trig(uint8 pin, uint8 value); 

extern uint16 GPIO_ProcessEvent( uint8 task_id, uint16 events );

extern void GPIO_Init( uint8 task_id );