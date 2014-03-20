#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"
#include "OnBoard.h"
#include "hal_led.h"
#include "hal_key.h"
#include "hal_lcd.h"
#include "gatt.h"
#include "ll.h"
#include "hci.h"
#include "gapgattserver.h"
#include "gattservapp.h"
#include "central.h"
#include "gapbondmgr.h"
#include "simpleGATTprofile.h"
#include "simpleBLECentral.h"

#include "ConnectionManger.h"


#define DEQUEUE_EVENT (1<<1)


static void ConnectionManger_handel(EventQueueItemBase_t* item);
static void EstablishLink(ConnectedDevice_t* conContainor);
static void AcceptLink(uint8* addr, uint16 connHandel);
static EventQueueItemBase_t* Dequeue();
static void Dispose(EventQueueItemBase_t* item);

static ConnectedDevice_t connectedDevices[MAX_HW_SUPPORTED_DEVICES];
static uint8 ConnectedIndex = 0; 
static uint8 ConnectedCount = 0; 

static EventQueueItemBase_t* EventQueue = NULL; 


typedef enum
{
  READY,
  BUSY, 
  ERROR
}ConnectionManger_status;



static ConnectionManger_status status = READY; 
static EventQueueItemBase_t* CurrentEvent = NULL; //event there is working on from queue 


//***********************************************************
//      Main Handler 
//***********************************************************

uint16 ConnectionManger_ProcessEvent( uint8 task_id, uint16 events )
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
  
  if ( events & DEQUEUE_EVENT )
  {
    if(status==READY && EventQueue!=NULL)
    {
      Dispose(CurrentEvent); 
      CurrentEvent = Dequeue(); 
      if(CurrentEvent!=NULL) 
      {
        status = BUSY; 
        ConnectionManger_handel(CurrentEvent);
      }
    }
    
    // return unprocessed events
    return (events ^ DEQUEUE_EVENT);
   }
  
  
}

static void ConnectionManger_handel(EventQueueItemBase_t* item)
{
  switch(item->action)
  {
    case Read:
    case Write:
    case Connect:
    case Disconnect: 
    // do some stuff with the item

  }
}


//***********************************************************
//      static Help Functions  
//***********************************************************

static void Dispose(EventQueueItemBase_t* item)
{
  switch(item->action)
  {
    case Read:
    case Write:
    case Connect:
    case Disconnect: 
    // do some stuff to free memory 

  }
}


bool CreateConnection(uint8* addr)
{
    uint8 i; 
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
      if(connectedDevices[i].status != INUSE)
      {
        //Terminat LINK ??????? why not; 
        
        osal_memcpy(connectedDevices[i].addr,addr,B_ADDR_LEN); 
        EstablishLink(&connectedDevices[i]); 
        return true; 
      }
    }
    return false; 
}

static void EstablishLink(ConnectedDevice_t* conContainor)
{
  conContainor->status = CONNECTING;
  
  if(conContainor->ConnHandel != GAP_CONNHANDLE_INIT)
  {
    GAPCentralRole_TerminateLink(conContainor->ConnHandel);
    conContainor->ConnHandel = GAP_CONNHANDLE_INIT;
  }
  /*
  GAPCentralRole_EstablishLink( DEFAULT_LINK_HIGH_DUTY_CYCLE,
                                      DEFAULT_LINK_WHITE_LIST,
                                      ADDRTYPE_PUBLIC, conContainor->addr );
  */
}

static void AcceptLink(uint8* addr, uint16 connHandel)
{
  uint8 i; 
  for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
  {
      if(osal_memcmp(connectedDevices[i].addr,addr,B_ADDR_LEN))
      {
        connectedDevices[i].ConnHandel = connHandel;
        connectedDevices[i].status = CONNECTED;
        return; 
      }
   }
}


//***********************************************************
//      Static Queue Functions
//***********************************************************

static void Enqueue(EventQueueItemBase_t* item)
{
  if(EventQueue==NULL)
  {
    EventQueue = item; 
  }
  else
  {
    EventQueueItemBase_t* tempItem = EventQueue; 
    
    while(tempItem->next!=NULL)
    {
      tempItem = tempItem->next;
    }
    
    tempItem->next = item;
  }
}

static void EnqueueFront(EventQueueItemBase_t* item)
{
  if(EventQueue==NULL)
  {
    EventQueue = item; 
  }
  else
  {
    EventQueueItemBase_t* tempItem = EventQueue; 
    EventQueue = item;
    EventQueue->next = tempItem;
  }
}


static EventQueueItemBase_t* Dequeue()
{
  if(EventQueue==NULL)
  {
    return NULL; 
  }
  else
  {
    EventQueueItemBase_t* tempItem = EventQueue; 
    EventQueue = tempItem->next;
    return tempItem; 
  }
}

//***********************************************************
//      Public Queue Functions
//***********************************************************


void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel, Callback call)
{
  EventQueueRWItem_t* item = (EventQueueRWItem_t*)osal_mem_alloc(sizeof(EventQueueRWItem_t)); 
  if(item)
  {
    item->base.action = Write;
    item->base.next = NULL;
    
    osal_memcpy(item->base.addr,addr,B_ADDR_LEN);
    item->callback = call; 
    item->handel = handel;
    item->data = osal_mem_alloc(len);
    if(item->data)
    {
      osal_memcpy(item->data,write,len);
      item->length = len;
      addToQueue((EventQueueItemBase_t*)item);
      return;
    }
    else
    {
      osal_mem_free(item);
    }
  }
}
void Queue_addRead(uint8* addr, uint16 handel )
{
  
}

void Queue_addServiceDiscovery(uint8* addr )
{
  
  
}