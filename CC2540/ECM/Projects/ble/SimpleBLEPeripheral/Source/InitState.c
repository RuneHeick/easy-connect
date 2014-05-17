/*********************************************************************
 * INCLUDES
 */
#include <stdio.h>
#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"

#include "OnBoard.h"

#include "hal_uart.h"

#include "gatt.h"

#include "hci.h"

#include "gapgattserver.h"
#include "gattservapp.h"
#include "devinfoservice.h"
#include "InitState.h"

#include "peripheral.h"

#include "gapbondmgr.h"
#include "EasyConnectProfile.h"
#include "simpleBLEPeripheral.h"
#include "GenericValueManger.h"
#include "Uart.h"
#include "SmartCommandsManger.h"
#include "SmartCommandsProperties.h"
#include "ECConnect.h"

#include "FileManager.h"

/*********************************************************************
 * CONSTANTS
 */

// How often to perform periodic event
#define SBP_PERIODIC_EVT_PERIOD                   15000

// What is the advertising interval when device is discoverable (units of 625us, 160=100ms)
#define DEFAULT_ADVERTISING_INTERVAL          160

// Limited discoverable mode advertises for 30.72s, and then stops
// General discoverable mode advertises indefinitely

#if defined ( CC2540_MINIDK )
#define DEFAULT_DISCOVERABLE_MODE             GAP_ADTYPE_FLAGS_GENERAL
#else
#define DEFAULT_DISCOVERABLE_MODE             GAP_ADTYPE_FLAGS_GENERAL
#endif  // defined ( CC2540_MINIDK )

// Minimum connection interval (units of 1.25ms, 80=100ms) if automatic parameter update request is enabled
#define DEFAULT_DESIRED_MIN_CONN_INTERVAL     80

// Maximum connection interval (units of 1.25ms, 800=1000ms) if automatic parameter update request is enabled
#define DEFAULT_DESIRED_MAX_CONN_INTERVAL     800

// Slave latency to use if automatic parameter update request is enabled
#define DEFAULT_DESIRED_SLAVE_LATENCY         0

// Supervision timeout value (units of 10ms, 1000=10s) if automatic parameter update request is enabled
#define DEFAULT_DESIRED_CONN_TIMEOUT          1000

// Whether to enable automatic parameter update request when a connection is formed
#define DEFAULT_ENABLE_UPDATE_REQUEST         TRUE

// Connection Pause Peripheral time value (in seconds)
#define DEFAULT_CONN_PAUSE_PERIPHERAL         6

// Company Identifier: Texas Instruments Inc. (13)
#define TI_COMPANY_ID                         0x000D

#define INVALID_CONNHANDLE                    0xFFFF

// Length of bd addr as a string
#define B_ADDR_STR_LEN                        15

#define MAX_SCAN_RSP_SIZE                     31



/*********************************************************************
 * LOCAL VARIABLES
 */

// GAP - SCAN RSP data (max size = 31 bytes)
static GenericValue scanRspData = { .status = NOT_INIT, .pValue = NULL, .size = 0  };
static uint8 TarskID;
static bool IsInit = false; 

static uint8 ScanLen = 0; 
static bool StartDevice(); 

void InitState_HandelUartPacket(osal_event_hdr_t * msg);
uint16 addChar(uint8* buffer, uint8 count);

pBuffer_t GAPManget_GetName()
{
  if(scanRspData.status == READY)
    return (pBuffer_t) {scanRspData.size-9-2, &scanRspData.pValue[2] }; 
  return (pBuffer_t) {0, NULL }; 
}

uint8 GAPManget_SetupName(char* DeviceName, uint8 nameSize)
{
    uint8 size = 0; 
    uint8 buffersize = 2+nameSize+9;
    
    if(buffersize>MAX_SCAN_RSP_SIZE)
    {
      nameSize -= buffersize - MAX_SCAN_RSP_SIZE; 
      buffersize = MAX_SCAN_RSP_SIZE;
    } 
      
    GenericValue_CreateContainer(&scanRspData, buffersize); 
    if(scanRspData.pValue==NULL)
      return 0; 
    scanRspData.pValue[size++] = nameSize+1; // 1 for index field 
    scanRspData.pValue[size++] = GAP_ADTYPE_LOCAL_NAME_COMPLETE; // index field 
    
    osal_memcpy(&scanRspData.pValue[size], DeviceName, nameSize);
    size = size + nameSize;
    
    //Connection Info. 
    scanRspData.pValue[size++] = 0x05;   // length of this data
    scanRspData.pValue[size++] = GAP_ADTYPE_SLAVE_CONN_INTERVAL_RANGE;
    scanRspData.pValue[size++] = LO_UINT16( DEFAULT_DESIRED_MIN_CONN_INTERVAL );   // 100ms
    scanRspData.pValue[size++] = HI_UINT16( DEFAULT_DESIRED_MIN_CONN_INTERVAL );
    scanRspData.pValue[size++] = LO_UINT16( DEFAULT_DESIRED_MAX_CONN_INTERVAL );   // 1s
    scanRspData.pValue[size++] = HI_UINT16( DEFAULT_DESIRED_MAX_CONN_INTERVAL );

    // Tx power level
    scanRspData.pValue[size++] = 0x02;   // length of this data
    scanRspData.pValue[size++] = GAP_ADTYPE_POWER_LEVEL;
    scanRspData.pValue[size++] = 0 ;     // 0dBm
    
    ScanLen = size;
    return size; 
}

void Setup_discoverableMode(uint8 mode, bool hasData,uint16 handel)
{
  if(mode == GAP_ADTYPE_FLAGS_NON || mode == GAP_ADTYPE_FLAGS_LIMITED || 
     mode == GAP_ADTYPE_FLAGS_GENERAL)
  { 
    uint8 advertlen; 
    
    uint8 advertData[] =
    {
    // Flags; this sets the device to use limited discoverable
    // mode (advertises for 30 seconds at a time) instead of general
    // discoverable mode (advertises indefinitely)
    0x02,   // length of this data
    GAP_ADTYPE_FLAGS,
    mode | GAP_ADTYPE_FLAGS_BREDR_NOT_SUPPORTED,
  
    // service UUID, to notify central devices what services are included
    // in this peripheral
    0x03,   // length of this data
    GAP_ADTYPE_16BIT_MORE,      // some of the UUID's, but not all
    LO_UINT16( SMARTCOMMAND_SERV_UUID ),
    HI_UINT16( SMARTCOMMAND_SERV_UUID ),
    0, // for has data flag
    0,
    0,
    0,
    0,
    0,
    };
    advertlen = sizeof(advertData)-6; 
    
    if(hasData)
    {
      // three-byte broadcast of the data "1 2 3"
      advertData[7] = 0x05;   // length of this data including the data type byte
      advertData[8] = GAP_ADTYPE_MANUFACTURER_SPECIFIC;      // manufacturer specific advertisement data type
      advertData[9] = 0xEC;
      advertData[10] = 0xDA; // EC application has DAta 
      advertData[11] = (uint8)handel;
      advertData[12] = (uint8)(handel>>8);
      
      advertlen += 6;
    }
    
    // Set the GAP Role Parameters
    GAPRole_SetParameter( GAPROLE_ADVERT_DATA, advertlen , advertData );
  }
}

uint16 InitState_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;

    if ( (pMsg = osal_msg_receive( task_id )) != NULL )
    {
      osal_event_hdr_t * pHdrMsg = (osal_event_hdr_t *)pMsg;
      if(pHdrMsg->event == UART_RQ)
      {
        InitState_HandelUartPacket(pHdrMsg);
      }

      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }


  // Discard unknown events
  return 0;
}

/*
DEVINFO_MODEL_NUMBER              1
#define DEVINFO_SERIAL_NUMBER             2
#define DEVINFO_FIRMWARE_REV              3
#define DEVINFO_HARDWARE_REV              4
#define DEVINFO_SOFTWARE_REV              5
#define DEVINFO_MANUFACTURER_NAME         6
*/

static void InitState_HandelUartPacket(osal_event_hdr_t * msg)
{
  RqMsg* pMsg = (RqMsg*) msg; 
  PayloadBuffer RX = Uart_getRXpayload();
  uint8 ack[3] =
  {
    0x01, // ack
    0x00, //handel msb
    0x00 // handel lsb
  };
  
  switch(pMsg->command)
  {
    case COMMAND_DEVICENAME:
    { 
      ack[2]= 0x01; 
      if(scanRspData.status == NOT_INIT)
      {
        ScanLen = GAPManget_SetupName(RX.bufferPtr, RX.count);
        if(ScanLen != 0)
        {
          Uart_Send_Response(ack,sizeof(ack));
        }
      }
      else if(scanRspData.status == READY)
      {
        if(scanRspData.size-9-2 == RX.count && 0==osal_memcmp(&scanRspData.pValue[2], RX.bufferPtr,RX.count))
        { 
          Uart_Send_Response(ack,sizeof(ack));
        }
      }
        
      break;
    }
    case COMMAND_MANIFACTURE:
    {
      if(SUCCESS == DevInfo_SetParameter(DEVINFO_MANUFACTURER_NAME,RX.count,RX.bufferPtr))
      {
        ack[2]= 0x02;
        Uart_Send_Response(ack,sizeof(ack));
      }
       break;
    }
    case COMMAND_MODELNR:
    {
      if(SUCCESS == DevInfo_SetParameter(DEVINFO_MODEL_NUMBER,RX.count,RX.bufferPtr))
      {
        ack[2]= 0x03;
        Uart_Send_Response(ack,sizeof(ack));
      }
       break;
    }
    case COMMAND_SERIALNR:
    {
      if(SUCCESS == DevInfo_SetParameter(DEVINFO_SERIAL_NUMBER,RX.count,RX.bufferPtr))
      {
        ack[2]= 0x04;
        Uart_Send_Response(ack,sizeof(ack));
      }
       break;
    }
    
    // ***** add smartcommand ***** // 
    
    case COMMAND_SMARTFUNCTION:
    {
      if(NULL != SmartCommandsManger_CreateService(RX.bufferPtr,RX.count))
      {
        Uart_Send_Response(ack,1);
      }
       break;
    }
    
    case COMMAND_GENRICVALUE:
    {
      uint16 itemhandle = addChar(RX.bufferPtr,RX.count);
      if(itemhandle != 0)
      {
        ack[1] = (uint8)(itemhandle>>8);
        ack[2] = (uint8)(itemhandle);
        Uart_Send_Response(ack,3);
      }
      break;
    }
    
    case COMMAND_REANGES:
    {
       if(SmartCommandsManger_addRange(RX.bufferPtr,RX.count))
       {
         Uart_Send_Response(ack,1);
       }
       break;
    }
    
    case COMMAND_START:
    {
       if(StartDevice())
       {
         Uart_Send_Response(ack,1);
       }
       break;
    }
    
  }
  
}


static bool StartDevice()
{
  if(ScanLen != 0 && SmartCommandServices_Count != 0)
  {
    SimpleBLEPeripheral_SwitchState(NORMALSTATE_INDEX);
    return true;  
  }
  return false; 
}


uint16 addChar(uint8* buffer, uint8 count)
{
  
  if(count>6)
  {
    bool subscription = (buffer[0]>>2)&0x01; 
    Subscription sub = NONE;
    if(subscription)
      sub = YES;
    
    bool read = (buffer[0]>>1)&0x01; 
    bool write = (buffer[0])&0x01;
    
    uint8 rdwr = 0; 
    if(read)
      rdwr = rdwr| GATT_PERMIT_READ;
    if(write) 
      rdwr = rdwr|GATT_PERMIT_WRITE;
    
    PresentationFormat Format = (PresentationFormat){0,0,0,0,0};
    Format.Format = buffer[1];
    GUIPresentationFormat FormatByteSize = (GUIPresentationFormat){buffer[2],buffer[3]};
    uint8 GPIO = buffer[4];
    uint8 size = buffer[5];
      
    return SmartCommandsManger_addCharacteristic(size,&buffer[6],count-6, FormatByteSize, Format,sub , rdwr,GPIO); 
  }
  
  return false; 
}





void InitState_Enter(uint8 tarskID)
{ 
  osal_pwrmgr_device( PWRMGR_ALWAYS_ON);
  ECConnect_Reset(); // must be before FileManager_Load 
  FileManager_Load(); // has an image in Flash 
  StartDevice(); //Start if Image is Loaded. 
  
  
  TarskID = tarskID;
  Uart_Subscribe(tarskID,COMMAND_DEVICENAME);
  Uart_Subscribe(tarskID,COMMAND_MANIFACTURE);
  Uart_Subscribe(tarskID,COMMAND_MODELNR);
  Uart_Subscribe(tarskID,COMMAND_SERIALNR);
  Uart_Subscribe(tarskID,COMMAND_SMARTFUNCTION);
  Uart_Subscribe(tarskID,COMMAND_GENRICVALUE);
  Uart_Subscribe(tarskID,COMMAND_REANGES);
  Uart_Subscribe(tarskID,COMMAND_START);
 
}

void InitState_Exit()
{
  
  Uart_Unsubscribe(TarskID,COMMAND_DEVICENAME);
  Uart_Unsubscribe(TarskID,COMMAND_MANIFACTURE);
  Uart_Unsubscribe(TarskID,COMMAND_MODELNR);
  Uart_Unsubscribe(TarskID,COMMAND_SERIALNR);
  Uart_Unsubscribe(TarskID,COMMAND_SMARTFUNCTION);
  Uart_Unsubscribe(TarskID,COMMAND_GENRICVALUE);
  Uart_Unsubscribe(TarskID,COMMAND_REANGES);
  Uart_Unsubscribe(TarskID,COMMAND_START);
  
  // Setup the GAP Peripheral Role Profile
  {

    uint8 enable_update_request = DEFAULT_ENABLE_UPDATE_REQUEST;
    uint16 desired_min_interval = DEFAULT_DESIRED_MIN_CONN_INTERVAL;
    uint16 desired_max_interval = DEFAULT_DESIRED_MAX_CONN_INTERVAL;
    uint16 desired_slave_latency = DEFAULT_DESIRED_SLAVE_LATENCY;
    uint16 desired_conn_timeout = DEFAULT_DESIRED_CONN_TIMEOUT;
    
    GAPRole_SetParameter( GAPROLE_SCAN_RSP_DATA, ScanLen*sizeof( uint8 ) , scanRspData.pValue );
    Setup_discoverableMode(DEFAULT_DISCOVERABLE_MODE,false,0);
    
    
    
    GAPRole_SetParameter( GAPROLE_PARAM_UPDATE_ENABLE, sizeof( uint8 ), &enable_update_request );
    GAPRole_SetParameter( GAPROLE_MIN_CONN_INTERVAL, sizeof( uint16 ), &desired_min_interval );
    GAPRole_SetParameter( GAPROLE_MAX_CONN_INTERVAL, sizeof( uint16 ), &desired_max_interval );
    GAPRole_SetParameter( GAPROLE_SLAVE_LATENCY, sizeof( uint16 ), &desired_slave_latency );
    GAPRole_SetParameter( GAPROLE_TIMEOUT_MULTIPLIER, sizeof( uint16 ), &desired_conn_timeout );
  }

  // Set the GAP Characteristics
  GGS_SetParameter( GGS_DEVICE_NAME_ATT, scanRspData.size-11, &scanRspData.pValue[2] ); //to only get the name; 
  
  // Set advertising interval
  {
    uint16 advInt = DEFAULT_ADVERTISING_INTERVAL;

    GAP_SetParamValue( TGAP_LIM_DISC_ADV_INT_MIN, advInt );
    GAP_SetParamValue( TGAP_LIM_DISC_ADV_INT_MAX, advInt );
    GAP_SetParamValue( TGAP_GEN_DISC_ADV_INT_MIN, advInt );
    GAP_SetParamValue( TGAP_GEN_DISC_ADV_INT_MAX, advInt );
  }

  // Setup the GAP Bond Manager
  {
    uint32 passkey = 0; // passkey "000000"
    uint8 pairMode = GAPBOND_PAIRING_MODE_INITIATE;
    uint8 mitm = TRUE;
    uint8 ioCap = GAPBOND_IO_CAP_NO_INPUT_NO_OUTPUT;
    uint8 bonding = TRUE;
    GAPBondMgr_SetParameter( GAPBOND_DEFAULT_PASSCODE, sizeof ( uint32 ), &passkey );
    GAPBondMgr_SetParameter( GAPBOND_PAIRING_MODE, sizeof ( uint8 ), &pairMode );
    GAPBondMgr_SetParameter( GAPBOND_MITM_PROTECTION, sizeof ( uint8 ), &mitm );
    GAPBondMgr_SetParameter( GAPBOND_IO_CAPABILITIES, sizeof ( uint8 ), &ioCap );
    GAPBondMgr_SetParameter( GAPBOND_BONDING_ENABLED, sizeof ( uint8 ), &bonding );
  }

  // Initialize GATT attributes
  GGS_AddService( GATT_ALL_SERVICES );              // GAP
  GATTServApp_AddService( GATT_ALL_SERVICES );      // GATT attributes
  DevInfo_AddService(); // Device Information Service
  ECConnect_AddService(GATT_ALL_SERVICES);
  SimpleProfile_AddService(GATT_ALL_SERVICES);
  
  // Enable clock divide on halt
  // This reduces active current while radio is active and CC254x MCU
  // is halted
  HCI_EXT_ClkDivOnHaltCmd( HCI_EXT_ENABLE_CLK_DIVIDE_ON_HALT );

  #if defined ( DC_DC_P0_7 )

  // Enable stack to toggle bypass control on TPS62730 (DC/DC converter)
  HCI_EXT_MapPmIoPortCmd( HCI_EXT_PM_IO_PORT_P0, HCI_EXT_PM_IO_PORT_PIN7 );

  #endif // defined ( DC_DC_P0_7 )
  
  
  // Enable clock divide on halt
  // This reduces active current while radio is active and CC254x MCU
  // is halted
  HCI_EXT_ClkDivOnHaltCmd( HCI_EXT_ENABLE_CLK_DIVIDE_ON_HALT );
  
  // Start the Device
  VOID GAPRole_StartDevice( &simpleBLEPeripheral_PeripheralCBs );
  
      // Start Bond Manager
  VOID GAPBondMgr_Register( &simpleBLEPeripheral_BondMgrCBs );
  
  if(FileManager_HasLoadedImage == false)
    FileManager_Save(); //Save Image To Flash; 

   osal_pwrmgr_device( PWRMGR_BATTERY);
}