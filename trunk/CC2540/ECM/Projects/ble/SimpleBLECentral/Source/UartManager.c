#include "UartManager.h"
#include "GenericList.h"

#define COMMANDLENGTH 1
#define HANDELLENGTH 2

static List uartQueue; 

void UartManager_Init()
{
  uartQueue = GenericList_create();
}


static void Send(TXStatus status)
{
  if(uartQueue.count>0 )
  {
    ListItem* item = GenericList_at(&uartQueue,0);
    if(Uart_Send(&item->value[1],item->size-1, item->value[0], Send))
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
    data[0] = 0x11;
    osal_memcpy(&data[COMMANDLENGTH],item->addr,B_ADDR_LEN);
    osal_memcpy(&data[COMMANDLENGTH+B_ADDR_LEN],item->pEvtData,item->dataLen);
    
    GenericList_add(&uartQueue,data,item->dataLen+B_ADDR_LEN+COMMANDLENGTH);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
}

void SendDataCommand(EventQueueRWItem_t* item)
{
  /*
  uint16 len = GenericList_TotalSize(&item->response);
  uint8 index = 0; 
  if(len < UART_BUFFER_SIZE-COMMANDLENGTH-B_ADDR_LEN-HANDELLENGTH)
  {
    uint8* data = osal_mem_alloc(len+COMMANDLENGTH+B_ADDR_LEN+HANDELLENGTH);
    if(data)
    {
      data[0] = 0x11;
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
  */
  
}

void SendDisconnectedCommand(uint8* addr)
{
  uint8* data = osal_mem_alloc(B_ADDR_LEN+COMMANDLENGTH);
  if(data)
  {
    data[0] = 0x11;
    osal_memcpy(&data[COMMANDLENGTH],addr,B_ADDR_LEN);
    
    GenericList_add(&uartQueue,data,B_ADDR_LEN+COMMANDLENGTH);  
    osal_mem_free(data);
    Send(SUCSSES);
  }
  
}


//*****************************************************************************
//     Received command 
//*****************************************************************************

