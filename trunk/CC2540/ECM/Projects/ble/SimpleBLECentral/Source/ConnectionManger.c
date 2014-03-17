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


#define MAX_HW_SUPPORTED_DEVICES 3

typedef enum
{
  NOTCONNECTED,
  CONNECTING,
  CONNECTED, 
  INUSE
}ConnectionStatus_t;

typedef enum
{
  Read,
  Write,
  Connect,
  Disconnect,
  
}EventType_t;

typedef struct 
{
  uint8 addr[B_ADDR_LEN];
  uint16 ConnHandel;
  ConnectionStatus_t status; 
}ConnectedDevice_t; 
 

typedef struct EventQueueItemBase_t 
{
  uint8 addr[B_ADDR_LEN];
  EventType_t action;
  
  struct EventQueueItemBase_t* next;
  
}EventQueueItemBase_t;


typedef struct EventQueueRWItem_t 
{
  EventQueueItemBase_t base;
  
  uint16 handel;
  uint8* data;
  uint8 length; 
}EventQueueRWItem_t;

static ConnectedDevice_t connectedDevices[MAX_HW_SUPPORTED_DEVICES];
static uint8 ConnectedIndex = 0; 
static uint8 ConnectedCount = 0; 

static EventQueueItemBase_t* EventQueue = NULL; 

static void EstablishLink(ConnectedDevice_t* conContainor);
static void AcceptLink(uint8* addr, uint16 connHandel);

void CreateConnection(uint8* addr)
{
    uint8 i; 
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
      if(connectedDevices[i].status != INUSE)
      {
        osal_memcpy(connectedDevices[i].addr,addr,B_ADDR_LEN); 
        EstablishLink(&connectedDevices[i]); 
        return; 
      }
    }
    // fix me Some Error or correction.. maybe a queue  
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

static void addToQueue(EventQueueItemBase_t* item)
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

//***********************************************************
//      Public Queue Functions
//***********************************************************


void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel)
{
  EventQueueRWItem_t* item = (EventQueueRWItem_t*)osal_mem_alloc(sizeof(EventQueueRWItem_t)); 
  if(item)
  {
    item->base.action = Write;
    item->base.next = NULL;
    
    osal_memcpy(item->base.addr,addr,B_ADDR_LEN);
    
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