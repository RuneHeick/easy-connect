/*********************************************************************
 * INCLUDES
 */

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
#include "UartManager.h"
#include "OSAL_Timers.h"
#include "BLEparameters.h"
#include "OSAL_Memory.h"

#define PERIODIC_SCAN_PERIOD 5000  // in ms. Minimum 5000 = 5 sec 

#define PERIODIC_SCAN_START   (1<<5)
#define SERVICE_DEVICE (1<<7)

#define MAX_CONNECTED_DEVICES 10

// time before an connection update; 
#define START_UPDATETIME 10000

/*********************************************************************
 * TYPEDEFS
 */

/*********************************************************************
 * GLOBAL VARIABLES
 */

/*********************************************************************
 * EXTERNAL VARIABLES
 */

/*********************************************************************
 * EXTERNAL FUNCTIONS
 */

/*********************************************************************
 * LOCAL VARIABLES
 */

// Task ID for internal task/event processing
static uint8 simpleBLETaskId;

// contains All devices to service 
static List DevicesToService; 

static uint32 SystemClockLastUpdate; 


/*********************************************************************
 * LOCAL FUNCTIONS
 */
static void simpleBLECentral_ProcessOSALMsg( osal_event_hdr_t *pMsg );
static void scanComplete(void* event);
static void ReadUpdateHandelsComplete(void* event);

static void scheduleUpdate();
static void service_addKnownDevice(uint8* addr, uint16 UpdateTimeHandel, uint16 UpdateHandel);
static void service_addUnknownDevice(uint8* addr, uint16 UpdateTimeHandel,uint8* SysID, uint8 sysIDlen,uint16 SysIdHandel , uint16 UpdateHandel);
void service_doService(AcceptedDeviceInfo* device);
static void ReadValueCompleteFail(void* event);

//***** SCHEDULE NEXT DEVICE UPDATETIME UPDATE *******// 
static void scheduleUpdate()
{
  uint32 UpdateTime = 3600000; // update minimum every houer 
  uint8 i;  
  
  for(i = 0;i< DevicesToService.count; i++)
  {
    ListItem* listitem = GenericList_at(&DevicesToService,i);
    AcceptedDeviceInfo* item = (AcceptedDeviceInfo*)listitem->value;
   
     if(item->KeepAliveTimeLeft_ms<UpdateTime)
       UpdateTime = item->KeepAliveTimeLeft_ms; 
  }
  
  osal_start_timerEx( simpleBLETaskId,SERVICE_DEVICE ,UpdateTime);
}

// **** ADD DEVICE TO EC CONNECTION UPDATE SERVICE ****// 
static void service_addUnknownDevice(uint8* addr, uint16 UpdateTimeHandel,uint8* SysID, uint8 sysIDlen,uint16 SysIdHandel, uint16 UpdateHandel )
{
  Queue_addWrite(SysID,sysIDlen,addr,SysIdHandel, NULL, NULL);
  service_addKnownDevice(addr,UpdateTimeHandel, UpdateHandel);
}

static void service_addKnownDevice(uint8* addr, uint16 UpdateTimeHandel, uint16 UpdateHandel)
{
  uint8 i; 
  AcceptedDeviceInfo itemToAdd; 
  if(DevicesToService.count<MAX_CONNECTED_DEVICES)
  {
    
    for(i = 0;i< DevicesToService.count; i++)
    {
      ListItem* listitem = GenericList_at(&DevicesToService,i);
      AcceptedDeviceInfo* item = (AcceptedDeviceInfo*)listitem->value;
      
      if(osal_memcmp(item->addr,addr,B_ADDR_LEN))
         return; 
    }
    
    
    osal_memcpy(itemToAdd.addr,addr,B_ADDR_LEN);
    itemToAdd.KeepAliveTime_ms = START_UPDATETIME; 
    itemToAdd.KeepAliveTimeLeft_ms = START_UPDATETIME;
    itemToAdd.KeppAliveHandel = UpdateTimeHandel; // to the ECConnect time char
    itemToAdd.UpdateHandel = UpdateHandel; // to the update char
    GenericList_add(&DevicesToService,(uint8*)&itemToAdd,sizeof(AcceptedDeviceInfo));
    service_doService(&itemToAdd); 
    
    scheduleUpdate();
  }
}

// UPDATE THE CONNECTED DEVICE INFO
void service_doService(AcceptedDeviceInfo* device)
{ 
  device->KeepAliveTimeLeft_ms = device->KeepAliveTime_ms; // here you can change the time dynamicaly
  uint32 writevalue = device->KeepAliveTimeLeft_ms*2+100; 
  uint8 write[4] = {(uint8)writevalue,(uint8)(writevalue>>8),(uint8)(writevalue>>16),(uint8)(writevalue>>24)};
  Queue_addWrite(write,sizeof(write),device->addr,device->KeppAliveHandel, NULL, ReadValueCompleteFail);
  
  scheduleUpdate();
}

void DecrimentUpdateWait(uint32 Ticks)
{
  uint8 i;
  for(i = 0;i< DevicesToService.count; i++)
  {   
      ListItem* listitem = GenericList_at(&DevicesToService,i);
      AcceptedDeviceInfo* item = (AcceptedDeviceInfo*)listitem->value;
      if(item->KeepAliveTimeLeft_ms<=Ticks && item->KeepAliveTimeLeft_ms!=0 )
      {
        item->KeepAliveTimeLeft_ms = 0; 
        DoServiceEvent_t* msg = (DoServiceEvent_t*)osal_msg_allocate(sizeof(DoServiceEvent_t));
        if(msg)
        {
          msg->hdr.event = DO_SERVICE_MSG; 
          msg->device = item; 
          osal_msg_send(simpleBLETaskId, (uint8*)msg);
        }
      }
      else
        item->KeepAliveTimeLeft_ms -= Ticks; 
  }  
}


/*********************************************************************
 * PUBLIC FUNCTIONS
 */

/*********************************************************************
 * @fn      SimpleBLECentral_Init
 *
 * @brief   Initialization function for the Simple BLE Central App Task.
 *          This is called during initialization and should contain
 *          any application specific initialization (ie. hardware
 *          initialization/setup, table initialization, power up
 *          notification).
 *
 * @param   task_id - the ID assigned by OSAL.  This ID should be
 *                    used to send messages and set timers.
 *
 * @return  none
 */
void SimpleBLECentral_Init( uint8 task_id )
{
  simpleBLETaskId = task_id;
  DevicesToService = GenericList_create();
  // Setup a delayed profile startup
  osal_set_event( simpleBLETaskId, START_DEVICE_EVT );
  
  osal_start_reload_timer( simpleBLETaskId, PERIODIC_SCAN_START, PERIODIC_SCAN_PERIOD );
  UartManager_Init();
}


/*********************************************************************
 * @fn      SimpleBLECentral_ProcessEvent
 *
 * @brief   Simple BLE Central Application Task event processor.  This function
 *          is called to process all events for the task.  Events
 *          include timers, messages and any other user defined events.
 *
 * @param   task_id  - The OSAL assigned task ID.
 * @param   events - events to process.  This is a bit map and can
 *                   contain more than one event.
 *
 * @return  events not processed
 */
uint16 SimpleBLECentral_ProcessEvent( uint8 task_id, uint16 events )
{
  
  VOID task_id; // OSAL required parameter that isn't used in this function
  
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;

    if ( (pMsg = osal_msg_receive( simpleBLETaskId )) != NULL )
    {
      simpleBLECentral_ProcessOSALMsg( (osal_event_hdr_t *)pMsg );

      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }

  if ( events & START_DEVICE_EVT )
  {
    //test
    //uint8 adress[] = {0x62,0xEE,0xD4,0xF7,0xB1,0x34};
    //uint8 adress[] = {0xF8,0x3A,0x22,0x8C,0xBA,0x1C};
    //char string[] = "There have been several claims for the longest sentence in the English language";
    
    //uint8 adress[] = {0xE6,0x81,0x70,0xE5,0xc5,0x78};
    //service_addUnknownDevice(adress,0x001b,adress,sizeof(adress),0x0019,0x020);
    
    return ( events ^ START_DEVICE_EVT );
  }
  
  if ( events & PERIODIC_SCAN_START )
  {
    uint16 dec = osal_heap_mem_used();
    
    Queue_Scan(scanComplete,NULL); 
    return ( events ^ PERIODIC_SCAN_START );
  }
  
  
  //SERVICE_DEVICE
  if ( events & SERVICE_DEVICE )
  {
    uint32 sysClock = osal_GetSystemClock();
    uint32 Ticks = sysClock<SystemClockLastUpdate ? // has timer overflowed 
      0xFFFFFFFF- SystemClockLastUpdate + sysClock : 
      sysClock-SystemClockLastUpdate ;
    DecrimentUpdateWait(Ticks);
    
    SystemClockLastUpdate = sysClock; 
    
    scheduleUpdate();
    return ( events ^ SERVICE_DEVICE );
  }
  // Discard unknown events
  return 0;
}

/*********************************************************************
 * @fn      simpleBLECentral_ProcessOSALMsg
 *
 * @brief   Process an incoming task message.
 *
 * @param   pMsg - message to process
 *
 * @return  none
 */
static void simpleBLECentral_ProcessOSALMsg( osal_event_hdr_t *pMsg )
{
  switch ( pMsg->event )
  { 
    case DO_SERVICE_MSG:
      service_doService(((DoServiceEvent_t*)pMsg)->device); 
    break;
  }
}



/*********************************************************************
*     SCAN CALLBACK 
*********************************************************************/
static uint8 Itemaddr[B_ADDR_LEN]; // for search in list
bool searchForItemaddr(ListItem* listitem)
{
  AcceptedDeviceInfo* item = (AcceptedDeviceInfo*)listitem->value;
  return osal_memcmp(Itemaddr,item->addr,B_ADDR_LEN);
}

//All types found with GAP_ADTYPE_ADV_IND
static void RecivedAdvertisment(ScanResponse_t* item)
{
  ListItem* listitem;
  osal_memcpy(Itemaddr,item->addr,B_ADDR_LEN);
  listitem = GenericList_First(&DevicesToService,searchForItemaddr);
  if(listitem != NULL)
  {
    AcceptedDeviceInfo* dev = (AcceptedDeviceInfo*)listitem->value;
    uint8 i = 0; 
    while(i<item->dataLen)      // se if the ECDA Signal is send to signal a Connection Rq. 
    {
      uint8 len = item->pEvtData[i];
      uint8 command = item->pEvtData[i+1];
      if(command==GAP_ADTYPE_MANUFACTURER_SPECIFIC && len == 3)
      {
        if(item->pEvtData[i+2]==0xEC && item->pEvtData[i+3]==0xDA && dev->UpdateHandel != 0)
        {
          Queue_addRead(dev->addr,dev->UpdateHandel,ReadUpdateHandelsComplete,NULL);
          break; 
        }
      }
      i=i+len+1; 
    }
    
  }
}

//All types found with GAP_ADTYPE_SCAN_RSP_IND
static void DeviceFound(ScanResponse_t* item)
{
  SendDeviceInfo(item); 
}

static void scanComplete(void* event)
{
  EventQueueScanItem_t* scan_event = (EventQueueScanItem_t*)event;
  List* foundDevices = &scan_event->response;
  ScanResponse_t* resp;
  
  for(uint8 i = 0; i<foundDevices->count;i++)
  {
     ListItem* item = GenericList_at(foundDevices,i); 
     resp = (ScanResponse_t*)item->value; 
     if(resp->eventType == GAP_ADTYPE_ADV_IND)
       RecivedAdvertisment(resp); 
     if(resp->eventType == GAP_ADTYPE_SCAN_RSP_IND)
       DeviceFound(resp); 
  }
}

//****************************************************************************
//    Auto Read Update Values 
//****************************************************************************

static void ReadValueComplete(void* event)
{
  EventQueueRWItem_t* item = (EventQueueRWItem_t*)event;
  SendDataCommand(item);
}

static void ReadValueCompleteFail(void* event)
{
  EventQueueRWItem_t* item = (EventQueueRWItem_t*)event;
  SendDisconnectedCommand(item->base.addr);
}

//Takes out the 2 byte handel from multible list items. And queue a read. 

static void ReadUpdateHandelsComplete(void* event)
{
  EventQueueRWItem_t* item = (EventQueueRWItem_t*)event;
  uint16 handel; 
  bool hasleftovers = false; 
  
  while(item->response.count != 0 )
  {
    uint8* value =  item->response.first->value;
    uint8 len = item->response.first->size; 
    
    if(hasleftovers)
    {
      len--; 
      handel = (handel) + (value[0]<<8);
      Queue_addRead(item->base.addr,handel,ReadValueComplete,NULL);
      value = &value[1];
    }
    
    for(uint8 i = 0; i<len; i=i+2)
    {
      handel = (value[i]) + (value[i+1]<<8);
      Queue_addRead(item->base.addr,handel,ReadValueComplete,NULL);
    }
    
    if(len%2!=0)
    {
      handel = value[len-1];
      hasleftovers = true; 
    }
    else
      hasleftovers = false; 
    
    GenericList_remove(&item->response,0);
    
  }  
}