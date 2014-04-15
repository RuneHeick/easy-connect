#include "UartManager.h"
#include "GenericList.h"
#include "Uart.h"
#include "ResetManager.h"
#include "SystemInfo.h"

#define COMMANDLENGTH 1
#define HANDELLENGTH 2
#define MAXPACKETSIZE 100

static List uartQueue; 


void UartManager_Init(uint8 TarskId)
{
  uartQueue = GenericList_create();
  Uart_Subscribe(TarskId,SystemInfo);
  Uart_Subscribe(TarskId,Reset);

  Uart_Subscribe(TarskId,ReadEvent);
  Uart_Subscribe(TarskId,WriteEvent);
  Uart_Subscribe(TarskId,DiscoverEvent);
  Uart_Subscribe(TarskId,AddDeviceEvent);
}


static void Send(TXStatus status)
{
  if(uartQueue.count>0 )
  {
    ListItem* item = GenericList_at(&uartQueue,0);
    uint8* dataptr = (uint8*)item->value; 
    if(Uart_Send(&dataptr[1],item->size-1,dataptr[0], Send))
    {
      GenericList_remove(&uartQueue,0);
    }
  }
}



//*****************************************************************************
//      Write Commands 
//*****************************************************************************


void SendDeviceInfo(ScanResponse_t* item)
{
  uint8* data = osal_mem_alloc(item->dataLen+B_ADDR_LEN+COMMANDLENGTH);
  if(data)
  {
    data[0] = DeviceEvent;
    osal_memcpy(&data[COMMANDLENGTH],item->addr,B_ADDR_LEN);
    osal_memcpy(&data[COMMANDLENGTH+B_ADDR_LEN],item->pEvtData,item->dataLen);
    
    GenericList_add(&uartQueue,data,item->dataLen+B_ADDR_LEN+COMMANDLENGTH);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
}

void SendDataCommand(EventQueueRWItem_t* item)
{
  uint16 len = GenericList_TotalSize(&item->response);
  uint8 index = 0; 
  if(len < UART_BUFFER_SIZE-COMMANDLENGTH-B_ADDR_LEN-HANDELLENGTH)
  {
    uint8* data = osal_mem_alloc(len+COMMANDLENGTH+B_ADDR_LEN+HANDELLENGTH);
    if(data)
    {
      data[0] = DataEvent;
      osal_memcpy(&data[COMMANDLENGTH],item->base.addr,B_ADDR_LEN);
      osal_memcpy(&data[COMMANDLENGTH+B_ADDR_LEN],&item->item.read.handle,HANDELLENGTH);
      
      for(uint8 i = 0; i<item->response.count; i++)
      {
        ListItem* listitem = GenericList_at(&item->response,i);
        osal_memcpy(&data[COMMANDLENGTH+B_ADDR_LEN+HANDELLENGTH+index],listitem->value,listitem->size);
        index = index+listitem->size;
      }
      
      GenericList_add(&uartQueue,data,len+B_ADDR_LEN+COMMANDLENGTH+HANDELLENGTH);  
      osal_mem_free(data);
      Send(SUCSSES);
    }
  }
  
}

void SendDisconnectedCommand(uint8* addr)
{
  uint8* data = osal_mem_alloc(B_ADDR_LEN+COMMANDLENGTH);
  if(data)
  {
    data[0] = DisconnectEvent;
    osal_memcpy(&data[COMMANDLENGTH],addr,B_ADDR_LEN);
    
    GenericList_add(&uartQueue,data,B_ADDR_LEN+COMMANDLENGTH);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
}

void SendResetCommand()
{
  uint8* data = osal_mem_alloc(COMMANDLENGTH+1);
  if(data)
  {
    data[0] = Reset;   
    GenericList_add(&uartQueue,data,COMMANDLENGTH+1);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
}


void SendServicesCommand(EventQueueServiceDirItem_t* item)
{
  
  uint8* data = osal_mem_alloc(114);
  if(data)
  {
    data[0] = ServiceEvent | (uint8)item->type;   
    osal_memcpy(&data[COMMANDLENGTH],item->base.addr,B_ADDR_LEN);
    
    uint8 packetCount = COMMANDLENGTH+B_ADDR_LEN; 
    for(uint8 i = 0; i< item->result.count; i++)
    {
      ListItem* res = GenericList_at(&item->result,i);
      DiscoveryItem* val = (DiscoveryItem*)res->value;
      
      if(item->type == Primary)
      { 
        data[packetCount++] = (uint8)(val->service.handle >> 8);
        data[packetCount++] = (uint8)(val->service.handle);
        data[packetCount++] = (uint8)(val->service.endHandle >> 8);
        data[packetCount++] = (uint8)(val->service.endHandle);
        data[packetCount++] = (uint8)(val->service.ServiceUUID >> 8);
        data[packetCount++] = (uint8)(val->service.ServiceUUID);
                 
      }
      else
      {
        data[packetCount++] = (uint8)(val->characteristic.Handle >> 8);
        data[packetCount++] = (uint8)(val->characteristic.Handle);
        data[packetCount++] = (uint8)(val->characteristic.UUID >> 8);
        data[packetCount++] = (uint8)(val->characteristic.UUID);
      }
      
      
      if(packetCount>MAXPACKETSIZE)
      {
       GenericList_add(&uartQueue,data,packetCount); 
       packetCount = COMMANDLENGTH+B_ADDR_LEN; 
      }
    }
    
    if(packetCount != COMMANDLENGTH+B_ADDR_LEN)
      GenericList_add(&uartQueue,data,packetCount);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
}

