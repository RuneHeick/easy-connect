#include "GPIOManager.h"
#include "hal_mcu.h"

#define GPIO_PINS 8
#define BOOL_FORMAT 1
#define TrigPuleTime 500

#define PULSE_TIMEOUT (1<<2)

typedef enum 
{
  None = 0,
  Pulse,
  Boolean
}PinMode;

typedef struct
{
  PinMode pinMode;
  uint32 TrigedTime;
}GPIOPinInfo; 

static uint8 TaskId; 
static GPIOPinInfo GPIOInfos[GPIO_PINS] = { {0,0},{0,0},{0,0},{0,0},{0,0},{0,0},{0,0},{0,0}};
static uint32 nextTimeOut = 0xFFFFFFFF;

/*  Setup a pin */
void GPIO_register(uint8 pin, uint8 format )
{
  if(pin<GPIO_PINS)
  {
    if(format == BOOL_FORMAT && GPIOInfos[pin].pinMode == None)
    {
      GPIOInfos[pin].pinMode = Boolean;
    }
    else
    {
      GPIOInfos[pin].pinMode = Pulse;
    }
  }
}

/*  Start an event on a GPIO pin*/
void GPIO_Trig(uint8 pin, uint8 value)
{
    if(GPIOInfos[pin].pinMode == Pulse)
    {
      uint32 clock = osal_GetSystemClock(); 
      uint32 trigTime = GPIOInfos[pin].TrigedTime  >  (0xFFFFFFFF-TrigPuleTime) ? TrigPuleTime - (0xFFFFFFFF-GPIOInfos[pin].TrigedTime) : GPIOInfos[pin].TrigedTime+TrigPuleTime;
      if(clock>trigTime)
      {
        GPIOInfos[pin].TrigedTime = clock; 
        P1 |= (1<<pin);
        //Start Timer
        
        if(nextTimeOut > clock+TrigPuleTime)
        {
          osal_start_timerEx(TaskId,PULSE_TIMEOUT,TrigPuleTime);
          nextTimeOut = clock+TrigPuleTime; 
        }
      }
    }
    else if(GPIOInfos[pin].pinMode == Boolean)
    {
      if(value)
        P1 |= (1<<pin);
      else
        P1 &= !(1<<pin);
    }
}

uint16 GPIO_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;
    
    if ( (pMsg = osal_msg_receive( task_id )) != NULL )
    {
      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }
    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  
  /*  Pulse is ended */
  if ( events & PULSE_TIMEOUT )
  {
    uint8 pin;
    nextTimeOut = 0xFFFFFFFF;
    for(pin=0;pin<GPIO_PINS;pin++)
    {
      uint32 clock = osal_GetSystemClock(); 
      uint32 trigTime = GPIOInfos[pin].TrigedTime  >  (0xFFFFFFFF-TrigPuleTime) ? TrigPuleTime - (0xFFFFFFFF-GPIOInfos[pin].TrigedTime) : GPIOInfos[pin].TrigedTime+TrigPuleTime;
      if(clock>trigTime)
      {
        P1 &= !(1<<pin);
        GPIOInfos[pin].TrigedTime = 0; 
      }
      else
      {
        if(GPIOInfos[pin].TrigedTime != 0 && nextTimeOut > trigTime)
        {
          nextTimeOut = trigTime;
          osal_start_timerEx(TaskId,PULSE_TIMEOUT,trigTime-clock);
        }
      }
    }
    
    // return unprocessed events
    return (events ^ PULSE_TIMEOUT);
  }
  
  return 0; 
}

void GPIO_Init( uint8 task_id )
{
  TaskId = task_id;
  P1SEL = 0; // Configure Port 0 as GPIO
  P1DIR = 0xFF; // Port 1 all as output
  P1 = 0; 
}