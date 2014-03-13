#pragma once 
#include "OSAL.h"


#define STATE_SWITCH (1<<0)


typedef void (*EnterFunc)(uint8);
typedef void (*ExitFunc)(void);

typedef uint16 (*RunFunc)(uint8, uint16 );

typedef struct 
{
  EnterFunc enter; 
  ExitFunc exit; 
  
  RunFunc run; 
}State;

void SimpleBLEPeripheral_SwitchState(uint8 StateID);