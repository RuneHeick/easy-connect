#include "UartClearCommands.h"
#include "ResetManager.h"
#include "ECConnect.h"
#include "FileManager.h"

#define RESETEVENT (1<<1)

static uint8 TaskId; 

static void UartClearCommands_HandelUartPacket(osal_event_hdr_t * msg);


void UartClearCommands_Init(uint8 taskid)
{
  TaskId = taskid; 
  
  Uart_Subscribe(taskid, SOFTRESET );
  Uart_Subscribe(taskid, PASSCLEARRESET);
  Uart_Subscribe(taskid, FACTORYRESET); 
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

/*
-UartClearCommands_HandleResetButtom():void
-UartClearCommands_ResetButtomISR():void 
*/
