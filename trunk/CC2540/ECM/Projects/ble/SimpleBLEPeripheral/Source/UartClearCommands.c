#include "UartClearCommands.h"
#include "ResetManager.h"
#include "ECConnect.h"
#include "FileManager.h"

#define RESETEVENT (1<<1)
#define HANDLE_RESETKEY (1<<2)
#define BUTTONCHECKTIME 500

#define BUTTONCLEARTIME 10000
#define BUTTONRESETTIME 1000

static uint8 TaskId; 

static void UartClearCommands_HandelUartPacket(osal_event_hdr_t * msg);
static uint8 resetButtonCount = 0; 

void UartClearCommands_Init(uint8 taskid)
{
  TaskId = taskid; 
  
  Uart_Subscribe(taskid, SOFTRESET );
  Uart_Subscribe(taskid, PASSCLEARRESET);
  Uart_Subscribe(taskid, FACTORYRESET); 
  
  /* Reset Button P0.0*/
  
  PICTL |= 0x01; /* Faling */
  IEN1 |= (1<<5); /*enable P0 interupts*/
    
  P0SEL &= 0xFE; /* Set as GPIO */ 
  P0DIR &= 0xFE; /* Set as Input */ 
  P0IEN |= 0x01; /* Enable interupt */
  P0IFG &= 0xFE; /* Clear Interupt */ 
}


uint16 UartClearCommands_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;

    if ( (pMsg = osal_msg_receive( task_id )) != NULL )
    {
      osal_event_hdr_t * pHdrMsg = (osal_event_hdr_t *)pMsg;
      if(pHdrMsg->event == UART_RQ)
      {
        UartClearCommands_HandelUartPacket(pHdrMsg);
      }

      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  
  if ( events & RESETEVENT )
  {
    ResetManager_Reset(false); 
    return (events ^ SYS_EVENT_MSG);
  }
  
  if ( events & HANDLE_RESETKEY )
  {
    resetButtonCount++; 
    
    if(P0&1)
    {
      /* not Pressed */
       uint16 pressedTime = resetButtonCount*BUTTONCHECKTIME; 
       
       if(pressedTime>= BUTTONCLEARTIME)
         ECConnect_ClearPassCode();
      
       if(pressedTime >= BUTTONRESETTIME)
        osal_set_event(TaskId, RESETEVENT);
      
      resetButtonCount = 0; 
      osal_stop_timerEx(TaskId,HANDLE_RESETKEY); 
    }
    
    return (events ^ HANDLE_RESETKEY);
  }

  // Discard unknown events
  return 0;
}


static void UartClearCommands_HandelUartPacket(osal_event_hdr_t * msg)
{
  RqMsg* pMsg = (RqMsg*) msg; 
  PayloadBuffer RX = Uart_getRXpayload();
  uint8 ack[1] =
  {
    0x01, // ack
  };
  
  switch(pMsg->command)
  {
    case SOFTRESET:
    { 
      Uart_Send_Response(ack,sizeof(ack));
      osal_set_event(TaskId, RESETEVENT);
    }
    break; 
    
    case PASSCLEARRESET:
    { 
      Uart_Send_Response(ack,sizeof(ack));
      osal_set_event(TaskId, RESETEVENT);
      ECConnect_ClearPassCode(); 
    }
    break;
    
    case FACTORYRESET:
    { 
      Uart_Send_Response(ack,sizeof(ack));
      FileManager_Clear(); 
      osal_set_event(TaskId, RESETEVENT);
    }
    break;
    
  }
}

HAL_ISR_FUNCTION( halKeyPort0Isr, P0INT_VECTOR )
{
  HAL_ENTER_ISR();
  
  if(P0&1)
  {
    /* not Pressed */
    
  }
  else
  {
    /* Pressed */
    osal_start_reload_timer(TaskId,HANDLE_RESETKEY,BUTTONCHECKTIME);
  }
  
  P0IFG = 0; 
  P0IF = 0; 
  HAL_EXIT_ISR();

  return;
}
