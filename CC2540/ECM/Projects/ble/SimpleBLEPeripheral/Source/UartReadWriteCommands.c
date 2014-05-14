#include "UartReadWriteCommands.h"
#include "ResetManager.h"
#include "ECConnect.h"
#include "FileManager.h"
#include "InitState.h"
#include "GenericValueManger.h"
#include "devinfoservice.h"
#include "Uart.h"
#include "SmartCommandsManger.h"
#include "GenericList.h"
#include "gattservapp.h"
#include "EasyConnectProfile.h"

#define NAME_ID 1 
#define MANIFACTURE_ID 2 
#define MODELNR_ID 3 
#define SERIALNR_ID 4 

#define UPDATETIMEOUT 2000

#define UPDATEEVENT (1<<2)


static uint8 TaskId; 
static List UpdateItems; 

static void UartReadWrite_HandelUartPacket(osal_event_hdr_t * msg);
static void ReadSystemInfo(uint8 handel);
static void UartReadWrite_UpdateTimeOut();


void UartReadWrite_Init(uint8 taskid)
{
  TaskId = taskid; 
  UpdateItems = GenericList_create();
  Uart_Subscribe(taskid, READ );
  Uart_Subscribe(taskid, WRITE );
}


uint16 UartReadWrite_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;

    if ( (pMsg = osal_msg_receive( task_id )) != NULL )
    {
      osal_event_hdr_t * pHdrMsg = (osal_event_hdr_t *)pMsg;
      if(pHdrMsg->event == UART_RQ)
      {
        UartReadWrite_HandelUartPacket(pHdrMsg);
      }

      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  
  if ( events & UPDATEEVENT )
  {
    UartReadWrite_UpdateTimeOut();
    return (events ^ UPDATEEVENT);
  }
  
  
  // Discard unknown events
  return 0;
}

static void ReadHandle(uint8 service, uint8 chara)
{
  if(service==0)
  {
    ReadSystemInfo(chara);
  }
  else
  {
    GenericValue* c = GetCharacteristic(service,chara); 
    if(c)
    {
       uint8* txbffer = Uart_TxGetDataBuffer();
       if(txbffer != NULL)
       {
          txbffer[0] = 1; //Ack; 
          osal_memcpy(&txbffer[1],c->pValue,c->size); 
          Uart_Send_Response(txbffer,c->size+1); 
       }
    }
  }
}

static void ReadSystemInfo(uint8 handel)
{
  switch(handel)
  {
    case NAME_ID: 
      {
        pBuffer_t nameBuf =  GAPManget_GetName();
        if(nameBuf.count>0)
        {
          uint8* txbffer = Uart_TxGetDataBuffer();
          if(txbffer != NULL)
          {
            txbffer[0] = 1; //Ack; 
            osal_memcpy(&txbffer[1],nameBuf.pValue,nameBuf.count); 
            Uart_Send_Response(txbffer,nameBuf.count+1); 
          }
        }
      }
      break;
        
      case MANIFACTURE_ID: 
      {
         GenericValue mani;
         DevInfo_GetParameter(DEVINFO_MANUFACTURER_NAME, &mani);
         if(mani.status == READY)
         {
           uint8* txbffer = Uart_TxGetDataBuffer();
           if(txbffer != NULL)
           {
             txbffer[0] = 1; //Ack; 
             osal_memcpy(&txbffer[1],mani.pValue,mani.size); 
             Uart_Send_Response(txbffer,mani.size+1);  
           }
         }  
      }
      break;
      
      case MODELNR_ID: 
      {
         GenericValue model;
         DevInfo_GetParameter(DEVINFO_MODEL_NUMBER, &model);
         if(model.status == READY)
         {
           uint8* txbffer = Uart_TxGetDataBuffer();
           if(txbffer != NULL)
           {
             txbffer[0] = 1; //Ack; 
             osal_memcpy(&txbffer[1],model.pValue,model.size); 
             Uart_Send_Response(txbffer,model.size+1);  
           }
         }  
      }
      break;
      
      
      case SERIALNR_ID: 
      {
         GenericValue serial;
         DevInfo_GetParameter(DEVINFO_SERIAL_NUMBER, &serial);
         if(serial.status == READY)
         {
           uint8* txbffer = Uart_TxGetDataBuffer();
           if(txbffer != NULL)
           {
             txbffer[0] = 1; //Ack; 
             osal_memcpy(&txbffer[1],serial.pValue,serial.size); 
             Uart_Send_Response(txbffer,serial.size+1);  
           }
         }  
      }
      break;
  }
}

static bool WriteData(uint8 service, uint8 chara, uint8* data, uint8 count)
{
  if(service!=0)
  {
    if( SUCCESS == SimpleProfile_SetParameter( (service<<8)+chara, count, data))
      return true;
  }
  return false; 
}

static void UartReadWrite_HandelUartPacket(osal_event_hdr_t * msg)
{
  RqMsg* pMsg = (RqMsg*) msg; 
  PayloadBuffer RX = Uart_getRXpayload();
  uint8 ack[1] =
  {
    0x01, // ack
  };
  
  switch(pMsg->command)
  {
    case READ:
    { 
      if(RX.count == 2)
      {
        ReadHandle(RX.bufferPtr[0],RX.bufferPtr[1]); 
      }
    }
    break;
    
    case WRITE:
    { 
      if(RX.count > 2)
      {
        uint16 handle = (RX.bufferPtr[0]<<8)+RX.bufferPtr[1];
        if(WriteData(RX.bufferPtr[0],RX.bufferPtr[1], &RX.bufferPtr[2], RX.count-2))
           Uart_Send_Response(ack,sizeof(ack));
      }
    }
    break;
    
  }
}


void UartReadWrite_UpdateHandle(uint16 handle)
{
    osal_start_timerEx(TaskId,UPDATEEVENT,UPDATETIMEOUT);
    GenericList_add(&UpdateItems,&handle,sizeof(uint16));
}

static void UartReadWrite_UpdateTimeOut()
{
  while(UpdateItems.count != 0)
  {
    ListItem* item = GenericList_at(&UpdateItems,0);
    uint16 handle = *((uint16*)item->value);
    GenericList_remove(&UpdateItems,0);
    
    GenericValue* c = GetCharacteristic((uint8)(handle>>8),(uint8)(handle)); 
    if(c)
    {
      uint8* txbffer = Uart_TxGetDataBuffer();
      if(txbffer != NULL)
      {
        txbffer[0] = (uint8)(handle>>8); 
        txbffer[1] = (uint8)(handle);
        osal_memcpy(&txbffer[2],c->pValue,c->size); 
        Uart_Send(txbffer,c->size+2,UPDATE, NULL);  
       }
     
    }
  }
}