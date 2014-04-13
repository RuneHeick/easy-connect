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
#include "BLEparameters.h"

#define DEQUEUE_EVENT (1<<1)
#define PROCESSQUEUEITEM_EVENT (1<<2)
#define START_DEVICE_EVENT (1<<3)
#define LINKTIMEOUT_EVENT (1<<4)

#define LINKTIMEOUT_TIME 500

static void ConnectionManger_handel(ConnectionEvents_t* item);
static void EstablishLink(ConnectedDevice_t* conContainor);
static void Dequeue(ConnectionEvents_t*);
static bool CreateConnection(uint8* addr);
static bool HasConnection(uint8* addr);
static void BLE_CentralEventCB( gapCentralRoleEvent_t *pEvent );
static void simpleBLECentralRssiCB( uint16 connHandle, int8 rssi );
static void simpleBLECentralPairStateCB( uint16 connHandle, uint8 state, uint8 status );
static void simpleBLECentralPasscodeCB( uint8 *deviceAddr, uint16 connectionHandle,
                                        uint8 uiInputs, uint8 uiOutputs );
static void ConnectionManger_ProcessOSALMsg( osal_event_hdr_t *pMsg );
static void simpleBLECentralProcessGATTMsg( gattMsgEvent_t *pMsg );
static void BLEGATTDiscoveryEvent( gattMsgEvent_t *pMsg );
static uint16 getConnHandel(ConnectionEvents_t* item);
static void cancelLinkEstablishment();
static void discoveryComplete();
static void discoveryCompleteError();
static void RWComplete(Callback call);
static void disposeScanList(List* item);
static void TerminateALLLinks();

static uint8 NULLaddr[B_ADDR_LEN] = {0x00, 0x00, 0x00, 0x00, 0x00 , 0x00}; 
static uint8 ConnectionManger_tarskID;
static ConnectedDevice_t connectedDevices[MAX_HW_SUPPORTED_DEVICES]; 


typedef enum
{
  READY,
  BUSY, 
  ERROR
}ConnectionManger_status;

static ConnectionManger_status status = READY; // is item in queue bein processed 
static ConnectionEvents_t CurrentEvent; //event there is working on from queue 
static List EventQueue; 

/*********************************************************************
 * PROFILE CALLBACKS
 */


// GAP Role Callbacks
static const gapCentralRoleCB_t simpleBLERoleCB =
{
  simpleBLECentralRssiCB,       // RSSI callback
  BLE_CentralEventCB       // Event callback
};

// Bond Manager Callbacks
static const gapBondCBs_t simpleBLEBondCB =
{
  simpleBLECentralPasscodeCB,
  simpleBLECentralPairStateCB
};


//***********************************************************
//      Setup
//***********************************************************

void ConnectionManger_Init( uint8 task_id)
{  
  uint8 i; 
  ConnectionManger_tarskID = task_id;
  EventQueue = GenericList_create();
  // Setup Central Profile
  {
    uint8 scanRes = DEFAULT_MAX_SCAN_RES;
    GAPCentralRole_SetParameter ( GAPCENTRALROLE_MAX_SCAN_RES, sizeof( uint8 ), &scanRes );
  }
  
  // Setup GAP
  GAP_SetParamValue( TGAP_GEN_DISC_SCAN, DEFAULT_SCAN_DURATION );
  GAP_SetParamValue( TGAP_LIM_DISC_SCAN, DEFAULT_SCAN_DURATION );
  GGS_SetParameter( GGS_DEVICE_NAME_ATT, 7, (uint8 *) "TEST RU" ); // make So generic name can be used 
  
  // Setup the GAP Bond Manager
  {
    uint32 passkey = DEFAULT_PASSCODE;
    uint8 pairMode = DEFAULT_PAIRING_MODE;
    uint8 mitm = DEFAULT_MITM_MODE;
    uint8 ioCap = DEFAULT_IO_CAPABILITIES;
    uint8 bonding = DEFAULT_BONDING_MODE;
    GAPBondMgr_SetParameter( GAPBOND_DEFAULT_PASSCODE, sizeof( uint32 ), &passkey );
    GAPBondMgr_SetParameter( GAPBOND_PAIRING_MODE, sizeof( uint8 ), &pairMode );
    GAPBondMgr_SetParameter( GAPBOND_MITM_PROTECTION, sizeof( uint8 ), &mitm );
    GAPBondMgr_SetParameter( GAPBOND_IO_CAPABILITIES, sizeof( uint8 ), &ioCap );
    GAPBondMgr_SetParameter( GAPBOND_BONDING_ENABLED, sizeof( uint8 ), &bonding );
  }  
  
  // Initialize GATT Client
  VOID GATT_InitClient();

  // Register to receive incoming ATT Indications/Notifications
  GATT_RegisterForInd( ConnectionManger_tarskID );

  // Initialize GATT attributes
  GGS_AddService( GATT_ALL_SERVICES );         // GAP
  GATTServApp_AddService( GATT_ALL_SERVICES ); // GATT attributes
  
  // Setup a delayed profile startup
  osal_set_event( ConnectionManger_tarskID, START_DEVICE_EVENT );
 
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
      connectedDevices[i].ConnHandel = GAP_CONNHANDLE_INIT;
      connectedDevices[i].status = NOTCONNECTED;
    }
}


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
      
      ConnectionManger_ProcessOSALMsg( (osal_event_hdr_t *)pMsg );
      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  
  if ( events & START_DEVICE_EVENT )
  {
    // Start the Device
    VOID GAPCentralRole_StartDevice( (gapCentralRoleCB_t *) &simpleBLERoleCB );

    // Register with bond manager after starting device
    GAPBondMgr_Register( (gapBondCBs_t *) &simpleBLEBondCB );
    
    return ( events ^ START_DEVICE_EVENT );
  }
  
  if ( events & DEQUEUE_EVENT )
  {
    if(status==READY)
    {
      if(EventQueue.count>0)
      {
        Dequeue(&CurrentEvent);
        if(CurrentEvent.base.action != None) 
        {
          status = BUSY; 
          osal_set_event(ConnectionManger_tarskID,PROCESSQUEUEITEM_EVENT);
        }
      }
      else
      {
        TerminateALLLinks(); 
      }
    }
    
    // return unprocessed events
    return (events ^ DEQUEUE_EVENT);
  }
  
  if ( events & PROCESSQUEUEITEM_EVENT )
  {
    if(CurrentEvent.base.action != None && status==BUSY)
    {
      if(HasConnection(CurrentEvent.base.addr)==false)
      {
        if(CreateConnection(CurrentEvent.base.addr)==false)
        {
          if(CurrentEvent.base.errorcall != NULL)
            CurrentEvent.base.errorcall(&CurrentEvent); // if no connection can be established. 
          status = READY; 
          osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
        }
      }
      else
      {
        ConnectionManger_handel(&CurrentEvent);
      }
    }
    return (events ^ PROCESSQUEUEITEM_EVENT);
  }
  /*   Connection TimeOut   */
  if ( events & LINKTIMEOUT_EVENT )
  {
    cancelLinkEstablishment();
    if(CurrentEvent.base.action != None && status==BUSY && CurrentEvent.base.errorcall != NULL)
    {
      CurrentEvent.base.errorcall(&CurrentEvent); 
    }
    status = READY;
    osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
    return (events ^ LINKTIMEOUT_EVENT);
  }
  
  
  return 0; 
}



static void ConnectionManger_handel(ConnectionEvents_t* item)
{
  switch(item->base.action)
  {
    case Read:
      {
        GATT_ReadLongCharValue(getConnHandel(item), &item->read.item.read, ConnectionManger_tarskID );
      }
      break; 
    case Write:
      {
        GATT_WriteLongCharValue(getConnHandel(item), &item->write.item.write, ConnectionManger_tarskID );
      }
      break;
    case Connect:
    case Disconnect: 
    case Scan:
      {
        GAPCentralRole_CancelDiscovery();
        GAPCentralRole_StartDiscovery( DEFAULT_DISCOVERY_MODE,
                                       DEFAULT_DISCOVERY_ACTIVE_SCAN,
                                       DEFAULT_DISCOVERY_WHITE_LIST );
      }
      break;
    case ServiceDiscovery:
      { 
        switch(item->serviceDir.type)
        {
          case Primary: 
            GATT_DiscAllPrimaryServices(getConnHandel(item),ConnectionManger_tarskID); 
            break; 
          case Characteristic: 
            GATT_DiscAllChars(getConnHandel(item),item->serviceDir.startHandle,item->serviceDir.endHandle,ConnectionManger_tarskID); 
            break;
          case Descriptor: 
            GATT_DiscAllCharDescs(getConnHandel(item),item->serviceDir.startHandle,item->serviceDir.endHandle,ConnectionManger_tarskID); 
            break;
        }
      }
      break; 
    // do some stuff with the item

  }
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
static void ConnectionManger_ProcessOSALMsg( osal_event_hdr_t *pMsg )
{
  switch ( pMsg->event )
  {
    case GATT_MSG_EVENT:
      simpleBLECentralProcessGATTMsg( (gattMsgEvent_t *) pMsg );
      break;
      
  }
}

//***********************************************************
//      static Help Functions  
//***********************************************************


static bool CreateConnection(uint8* addr)
{
    uint8 i; 
    ConnectedDevice_t* containor = NULL; 
    
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
      if(osal_memcmp(connectedDevices[i].addr,addr,B_ADDR_LEN))
      {
          if(connectedDevices[i].status == CONNECTING) //if a establish connection is in progress 
            return true; 
      }
      if(connectedDevices[i].status != INUSE)
      {
        containor = &connectedDevices[i];
      }
    }
    
    if(containor!=NULL)
    {
      osal_memcpy(containor->addr,addr,B_ADDR_LEN); 
      EstablishLink(containor); 
      return true; 
    }
    
    return false; 
}

// check to see if there allready is a connection. 
static bool HasConnection(uint8* addr)
{
    uint8 i; 
    
    if(osal_memcmp(addr,NULLaddr,B_ADDR_LEN))
      return true;
    
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
        if(osal_memcmp(connectedDevices[i].addr,addr,B_ADDR_LEN))
        {
          if(connectedDevices[i].status == CONNECTED)
            return true; 
        }
    }
    return false;
}

static uint16 getConnHandel(ConnectionEvents_t* item)
{                      
   for(uint8 i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
   {
     if(osal_memcmp(connectedDevices[i].addr,item->base.addr,B_ADDR_LEN))
        {
          if(connectedDevices[i].status == CONNECTED)
            return connectedDevices[i].ConnHandel; 
        }
   }
   return GAP_CONNHANDLE_INIT; 
}

static void EstablishLink(ConnectedDevice_t* conContainor)
{
  conContainor->status = CONNECTING;
  
  
  if(conContainor->ConnHandel != GAP_CONNHANDLE_INIT)
  {
    GAPCentralRole_TerminateLink(conContainor->ConnHandel);
    conContainor->ConnHandel = GAP_CONNHANDLE_INIT;
  }
  
  GAPCentralRole_EstablishLink( DEFAULT_LINK_HIGH_DUTY_CYCLE,
                                      DEFAULT_LINK_WHITE_LIST,
                                      ADDRTYPE_PUBLIC, conContainor->addr );
  
  osal_start_timerEx(ConnectionManger_tarskID ,LINKTIMEOUT_EVENT,LINKTIMEOUT_TIME);
}

static void TerminateALLLinks()
{
  for(uint8 i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
  {
      if(connectedDevices[i].status == CONNECTED)
      {
          GAPCentralRole_TerminateLink(connectedDevices[i].ConnHandel);
      }
  }
}

static void cancelLinkEstablishment()
{
  uint8 i; 
  for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
  {
      if(connectedDevices[i].status == CONNECTING)
      {
          connectedDevices[i].status = NOTCONNECTED;
      }
  }
  GAPCentralRole_TerminateLink(GAP_CONNHANDLE_INIT); // terminate pending Connections. 
}

//***********************************************************
//      Static Queue Functions
//***********************************************************

static void Enqueue(ConnectionEvents_t* item)
{
  if(EventQueue.count<20)
  {
  
  
  if(GenericList_add(&EventQueue,(uint8*)item,sizeof(ConnectionEvents_t))==false &&
     item->base.errorcall != NULL)
        item->base.errorcall(item);
  }
  else
  {
    ResetManager_Reset(false);
  }
  osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
}

static void Dequeue(ConnectionEvents_t* item)
{
  if(EventQueue.count>0)
  {
    ListItem* firstItem = GenericList_at(&EventQueue,0);
    osal_memcpy(item,firstItem->value,sizeof(ConnectionEvents_t));
    GenericList_remove(&EventQueue,0);
  }
  else
  {
    item->base.action = None; 
  }
}

//***********************************************************
//      Public Queue Functions
//***********************************************************


void Queue_addRead(uint8* addr, uint16 handel, Callback call, Callback ecall)
{
  ConnectionEvents_t item;
  item.base.action = Read;
  item.base.errorcall = ecall;
  osal_memcpy(item.base.addr,addr,B_ADDR_LEN);
  item.base.callback = call; 
  item.read.item.read.handle = handel;
  item.read.item.write.offset = 0; 
  item.read.response = GenericList_create();
    
  Enqueue(&item);
}


void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel, Callback call, Callback ecall)
{
    ConnectionEvents_t item;
    item.base.action = Write;
    item.base.errorcall = ecall;
    osal_memcpy(item.base.addr,addr,B_ADDR_LEN);
    item.base.callback = call; 
    item.write.item.write.handle = handel;
    item.write.item.write.pValue = osal_mem_alloc(len);
    item.write.item.write.offset = 0; 
    item.write.response = GenericList_create();
    if(item.write.item.write.pValue)
    {
      osal_memcpy(item.write.item.write.pValue,write,len);
      item.write.item.write.len = len;
      Enqueue(&item);
      return;
    }
    else if(ecall != NULL)
    {
      ecall(&item);
    }
}

void Queue_addServiceDiscovery(uint8* addr, Callback call ,Callback ecall,DiscoveryRange range, uint16 startHandle, uint16 endHandle)
{
  ConnectionEvents_t item;
  item.base.action = ServiceDiscovery;
  item.base.errorcall = ecall;
  osal_memcpy(item.base.addr,addr,B_ADDR_LEN);
  item.base.callback = call; 
  item.serviceDir.type = range; 
  item.serviceDir.result = GenericList_create(); 
  item.serviceDir.startHandle = startHandle;
  item.serviceDir.endHandle = endHandle;
  Enqueue(&item);
}

void Queue_Scan(Callback call, Callback ecall)
{
  ConnectionEvents_t item;
  item.base.action = Scan;
  item.base.errorcall = ecall;
  osal_memcpy(item.base.addr,NULLaddr,B_ADDR_LEN);
  item.base.callback = call; 
  
  item.scan.response = GenericList_create(); 
  
  Enqueue(&item);
}





/*********************************************************************
 * @fn      simpleBLECentralEventCB
 *
 * @brief   Central event callback function.
 *
 * @param   pEvent - pointer to event structure
 *
 * @return  none
 */
static void BLE_CentralEventCB( gapCentralRoleEvent_t *pEvent )
{
  switch ( pEvent->gap.opcode )
  {
     case GAP_DEVICE_INIT_DONE_EVENT:  
      {
        LCD_WRITE_STRING( "BLE Central", HAL_LCD_LINE_1 );
        LCD_WRITE_STRING( bdAddr2Str( pEvent->initDone.devAddr ),  HAL_LCD_LINE_2 );
      }
      break;
    case GAP_DEVICE_INFO_EVENT:
      {
        if(CurrentEvent.base.action == Scan)
        {
          EventQueueScanItem_t* currentevent = &CurrentEvent.scan;
          ScanResponse_t item; 
          item.eventType = pEvent->devceInfo.eventType;
          item.rssi = pEvent->devceInfo.rssi;
          item.dataLen = pEvent->devceInfo.dataLen;
          
          osal_memcpy(item.addr,pEvent->devceInfo.addr,B_ADDR_LEN);
          item.pEvtData = (uint8*)osal_memdup(pEvent->devceInfo.pEvtData,pEvent->devceInfo.dataLen);
          if(item.pEvtData==NULL)
          {
            item.dataLen = 0; 
          }
          
          if(GenericList_add(&currentevent->response,&item,sizeof(ScanResponse_t))==false)
           osal_mem_free(item.pEvtData); 
        }
      }
      break;
    case GAP_DEVICE_DISCOVERY_EVENT:
    {
       if(CurrentEvent.base.action == Scan)
       {
         EventQueueScanItem_t* item = &CurrentEvent.scan;
         if(item->base.callback != NULL)
         {
            item->base.callback(&CurrentEvent); 
         }
         disposeScanList(&item->response);
         status = READY; 
       }
       osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT); 
     }
     break;

    case GAP_LINK_ESTABLISHED_EVENT:
      {
          
          uint8 i; 
          for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
          {
            if(connectedDevices[i].status == CONNECTING && osal_memcmp(connectedDevices[i].addr,pEvent->linkCmpl.devAddr,B_ADDR_LEN))
            {
              if ( pEvent->gap.hdr.status == SUCCESS )
              { 
                connectedDevices[i].status = CONNECTED;
                connectedDevices[i].ConnHandel = pEvent->linkCmpl.connectionHandle;
              }
              else
              {
                connectedDevices[i].status = NOTCONNECTED;
                connectedDevices[i].ConnHandel = GAP_CONNHANDLE_INIT;
              }
              osal_set_event(ConnectionManger_tarskID,PROCESSQUEUEITEM_EVENT);
              osal_stop_timerEx(ConnectionManger_tarskID,LINKTIMEOUT_EVENT);
              return; 
            }
          }
         
          /* if connection was not requested */
          GAPCentralRole_TerminateLink(pEvent->linkCmpl.connectionHandle);
      }
      break;

    case GAP_LINK_TERMINATED_EVENT:
      { 
        uint8 i;
        for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
        {
          if(connectedDevices[i].ConnHandel == pEvent->linkTerminate.connectionHandle)
          {
            connectedDevices[i].status = NOTCONNECTED;
            connectedDevices[i].ConnHandel = GAP_CONNHANDLE_INIT;
            return; 
          }
        }
      }
      break;
      
    default:
      break;
  }
}

/*********************************************************************
 * @fn      simpleBLECentralPasscodeCB
 *
 * @brief   Passcode callback.
 *
 * @return  none
 */
static void simpleBLECentralPasscodeCB( uint8 *deviceAddr, uint16 connectionHandle,
                                        uint8 uiInputs, uint8 uiOutputs )
{
#if (HAL_LCD == TRUE)

  uint32  passcode;
  uint8   str[7];

  // Create random passcode
  LL_Rand( ((uint8 *) &passcode), sizeof( uint32 ));
  passcode %= 1000000;
  
  // Display passcode to user
  if ( uiOutputs != 0 )
  {
    LCD_WRITE_STRING( "Passcode:",  HAL_LCD_LINE_1 );
    LCD_WRITE_STRING( (char *) _ltoa(passcode, str, 10),  HAL_LCD_LINE_2 );
  }
  
  // Send passcode response
  GAPBondMgr_PasscodeRsp( connectionHandle, SUCCESS, passcode );
#endif
}


/*********************************************************************
 * @fn      pairStateCB
 *
 * @brief   Pairing state callback.
 *
 * @return  none
 */
static void simpleBLECentralPairStateCB( uint16 connHandle, uint8 state, uint8 status )
{
  if ( state == GAPBOND_PAIRING_STATE_STARTED )
  {
    LCD_WRITE_STRING( "Pairing started", HAL_LCD_LINE_1 );
  }
  else if ( state == GAPBOND_PAIRING_STATE_COMPLETE )
  {
    if ( status == SUCCESS )
    {
      LCD_WRITE_STRING( "Pairing success", HAL_LCD_LINE_1 );
    }
    else
    {
      LCD_WRITE_STRING_VALUE( "Pairing fail", status, 10, HAL_LCD_LINE_1 );
    }
  }
  else if ( state == GAPBOND_PAIRING_STATE_BONDED )
  {
    if ( status == SUCCESS )
    {
      LCD_WRITE_STRING( "Bonding success", HAL_LCD_LINE_1 );
    }
  }
}

/*********************************************************************
 * @fn      simpleBLECentralRssiCB
 *
 * @brief   RSSI callback.
 *
 * @param   connHandle - connection handle
 * @param   rssi - RSSI
 *
 * @return  none
 */
static void simpleBLECentralRssiCB( uint16 connHandle, int8 rssi )
{
    LCD_WRITE_STRING_VALUE( "RSSI -dB:", (uint8) (-rssi), 10, HAL_LCD_LINE_1 );
}


uint8 count = 0; 
/*********************************************************************
 * @fn      simpleBLECentralProcessGATTMsg
 *
 * @brief   Process GATT messages
 *
 * @return  none
 */
static void simpleBLECentralProcessGATTMsg( gattMsgEvent_t *pMsg )
{
  
  //********************    read blob ***************//
  if(CurrentEvent.base.action == Read)
  {
    if ( ( pMsg->method == ATT_READ_BLOB_RSP ) || ( pMsg->method == ATT_ERROR_RSP ) )
    {
      
      if (pMsg->hdr.status == bleProcedureComplete)
      {
        RWComplete(CurrentEvent.base.callback);
      }
      else if ( pMsg->method == ATT_READ_BLOB_RSP && pMsg->hdr.status == 0)
      {
         GenericList_add(&CurrentEvent.read.response,pMsg->msg.readBlobRsp.value,pMsg->msg.readBlobRsp.len); 
      }
      else //TIMEOUT AND ERRORS 
      {
        RWComplete(CurrentEvent.base.errorcall);
      }
      
    }
    else
    {
     RWComplete(CurrentEvent.base.errorcall); 
    }
  }
  
  //*************************** Write ********************************
  /*
  ATT_PREPARE_WRITE_RSP,
 *          ATT_EXECUTE_WRITE_RSP or ATT_ERROR_RSP (if an error occurred on
 *          the server).
   
   */
  else if(CurrentEvent.base.action == Write)
  {
    if ( (pMsg->method == ATT_PREPARE_WRITE_RSP) || (pMsg->method == ATT_EXECUTE_WRITE_RSP)||(pMsg->method == ATT_ERROR_RSP))
    {
      if ( pMsg->method == ATT_ERROR_RSP)
      {
         RWComplete(CurrentEvent.base.errorcall);
      }
      else
      {
        RWComplete(CurrentEvent.base.callback);
      }
  
    }
    else
    {
        RWComplete(CurrentEvent.base.errorcall);
    }
  }
  else if(CurrentEvent.base.action == ServiceDiscovery)
  {
    BLEGATTDiscoveryEvent( pMsg );
  }
  
}

/*********************************************************************
 * @fn      simpleBLEGATTDiscoveryEvent
 *
 * @brief   Process GATT discovery event
 *
 * @return  none
 */

static void BLEGATTDiscoveryEvent( gattMsgEvent_t *pMsg )
{ 
     
  if(pMsg->method == ATT_ERROR_RSP )
  {
    discoveryCompleteError();
  }
  // If procedure complete
  else if (pMsg->hdr.status == bleProcedureComplete)
  {
    discoveryComplete(); 
  }
  else if(pMsg->method == ATT_READ_BY_GRP_TYPE_RSP && pMsg->msg.readByGrpTypeRsp.numGrps > 0)
  {
    // Service found, store handles
      uint8 i; 
      EventQueueServiceDirItem_t* extitem = &CurrentEvent.serviceDir;
      
      for(i=0;i<pMsg->msg.readByGrpTypeRsp.numGrps;i++)
      {
        DiscoveryItem item;

        item.service.handle = (pMsg->msg.readByGrpTypeRsp.dataList[0+(i*6)]<<8)+pMsg->msg.readByGrpTypeRsp.dataList[1+(i*6)];
        item.service.endHandle = (pMsg->msg.readByGrpTypeRsp.dataList[2+(i*6)]<<8)+pMsg->msg.readByGrpTypeRsp.dataList[3+(i*6)];
        item.service.ServiceUUID = (pMsg->msg.readByGrpTypeRsp.dataList[4+(i*6)]<<8)+pMsg->msg.readByGrpTypeRsp.dataList[5+(i*6)];
        
        GenericList_add(&extitem->result,&item,sizeof(DiscoveryItem));
       }
    }
  
    /* characerisic */
    else if(pMsg->method == ATT_READ_BY_TYPE_RSP && pMsg->msg.readByTypeRsp.numPairs > 0)
    {
        uint8 i; 
        EventQueueServiceDirItem_t* extitem = &CurrentEvent.serviceDir;
      
        for(i=0;i<pMsg->msg.readByGrpTypeRsp.numGrps;i++)
        {
            DiscoveryItem item;

            item.characteristic.Handle = (pMsg->msg.readByTypeRsp.dataList[3+(i*pMsg->msg.readByTypeRsp.len)]<<8)+pMsg->msg.readByTypeRsp.dataList[4+(i*pMsg->msg.readByTypeRsp.len)];
            item.characteristic.UUID = (pMsg->msg.readByTypeRsp.dataList[5+(i*pMsg->msg.readByTypeRsp.len)]<<8)+pMsg->msg.readByTypeRsp.dataList[6+(i*pMsg->msg.readByTypeRsp.len)];
            GenericList_add(&extitem->result,&item,sizeof(DiscoveryItem));
         }
    }
  
    /* descripors */
    else if(pMsg->method == ATT_FIND_INFO_RSP && pMsg->msg.findInfoRsp.numInfo > 0)
    {
        uint8 i; 
        EventQueueServiceDirItem_t* extitem = &CurrentEvent.serviceDir;
        
        if(pMsg->msg.findInfoRsp.format == 0x01)// 16 bit UUID
        {
          for(i=0;i<pMsg->msg.findInfoRsp.numInfo;i++)
          {
              DiscoveryItem item;

              item.descriptors.Handle = pMsg->msg.findInfoRsp.info.btPair[i].handle;
              item.descriptors.UUID = (pMsg->msg.findInfoRsp.info.btPair[i].uuid[0]<<8)+pMsg->msg.findInfoRsp.info.btPair[i].uuid[1];
              
              GenericList_add(&extitem->result,&item,sizeof(DiscoveryItem));
           }
        }
    }
    else
    {
      discoveryCompleteError();
    }
}

static void discoveryComplete()
{
  EventQueueServiceDirItem_t* item = &CurrentEvent.serviceDir;
  if(item->base.callback != NULL)
    item->base.callback(&CurrentEvent);
  GenericList_dispose(&item->result);
  status = READY; 
  osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
}

static void discoveryCompleteError()
{
  EventQueueServiceDirItem_t* item = &CurrentEvent.serviceDir;
  if(item->base.errorcall != NULL)
    item->base.errorcall(&CurrentEvent);
  GenericList_dispose(&item->result);
  status = READY;
  osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
}

static void RWComplete(Callback call)
{
  EventQueueRWItem_t* item = &CurrentEvent.read;
  if(call != NULL)
    call(&CurrentEvent);
  GenericList_dispose(&item->response);
  if(item->base.action == Write)
  {
    osal_mem_free(item->item.write.pValue);
  }
  status = READY; 
  osal_set_event(ConnectionManger_tarskID,DEQUEUE_EVENT);
}

static void disposeScanList(List* list)
{
  while(list->count != 0)
  { 
     ListItem* listitem = GenericList_at(list,0);
     ScanResponse_t* item = (ScanResponse_t*)listitem->value; 
     osal_mem_free(item->pEvtData);
     GenericList_remove(list,0); 
  }
}