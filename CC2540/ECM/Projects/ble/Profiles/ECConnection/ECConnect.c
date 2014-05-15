/*********************************************************************
 * INCLUDES
 */
#include "bcomdef.h"
#include "OSAL.h"
#include "linkdb.h"
#include "att.h"
#include "gatt.h"
#include "gatt_uuid.h"
#include "gatt_profile_uuid.h"
#include "gattservapp.h"
#include "GenericList.h"
#include "GenericValueManger.h"
#include "ECConnect.h"
#include "peripheral.h"

/*********************************************************************
 * MACROS
 */

/*********************************************************************
 * CONSTANTS
 */
#define TIME_TICK (1<<0)
#define NEWLINK_EVENT (1<<1)
#define TERMINATELINK_EVENT (1<<2)
/*********************************************************************
 * TYPEDEFS
 */

typedef struct KnownAddrs_t
{
 uint8 addr[B_ADDR_LEN];
 
}KnownAddrs_t;


/*********************************************************************
 * GLOBAL VARIABLES
 */
// ECConnect service
CONST uint8 ecConnectServUUID[ATT_BT_UUID_SIZE] =
{
  LO_UINT16(ECCONNECT_SERV_UUID), HI_UINT16(ECCONNECT_SERV_UUID)
};

// System ID
CONST uint8 ecConnectSystemIDUUID[ATT_BT_UUID_SIZE] =
{
  LO_UINT16(SYSTEMID_CHARA_UUID), HI_UINT16(SYSTEMID_CHARA_UUID)
};

// UpdateTime ID
CONST uint8 ecConnectUpdateTimeUUID[ATT_BT_UUID_SIZE] =
{
  LO_UINT16(UPDATETIME_CHARA_UUID), HI_UINT16(UPDATETIME_CHARA_UUID)
};


ECC_Status_t ECConnect_AcceptedHost = DISCONNECTED; 

static ECConnect_StatusChange_t CallBackhandler = NULL; 
static ECConnect_GotPassCode   passCodeCallback = NULL;
/*********************************************************************
 * EXTERNAL VARIABLES
 */

/*********************************************************************
 * LOCAL VARIABLES
 */
static uint8 InitialSysID[SYSID_SIZE];

static List KnownAddresList; 
static uint32 LastSysClock;

static uint8 EcConnect_TaskID;
/*********************************************************************
 * Profile Attributes - variables
 */

// Device Information Service attribute
static CONST gattAttrType_t ecConnectService = { ATT_BT_UUID_SIZE, ecConnectServUUID };

// System ID characteristic
static uint8 ecConnectSysIDProps = GATT_PROP_WRITE;
static uint8 SysID[SYSID_SIZE];

// UpdateTime characteristic
static uint8 ecConnectUpdateTimeProps = GATT_PROP_READ|GATT_PROP_WRITE;
static uint32 UpdateTime = ECCONNECT_STARTTIME;

/*********************************************************************
 * Profile Attributes - Table
 */



static gattAttribute_t ECConnectAttrTbl[] =
{
  // ECConnect Service
  {
    { ATT_BT_UUID_SIZE, primaryServiceUUID },  /* type */
    GATT_PERMIT_READ,                         /* permissions */
    0,                                        /* handle */
    (uint8 *)&ecConnectService                /* pValue */
  },
  
    // System ID Declaration
    {
      { ATT_BT_UUID_SIZE, characterUUID },
      GATT_PERMIT_READ,
      0,
      &ecConnectSysIDProps
    },

      // System ID Value
      {
        { ATT_BT_UUID_SIZE, ecConnectSystemIDUUID },
        GATT_PERMIT_WRITE,
        0,
        SysID
      },

    // System ID Declaration
    {
      { ATT_BT_UUID_SIZE, characterUUID },
      GATT_PERMIT_READ,
      0,
      &ecConnectUpdateTimeProps
    },

      // System ID Value
      {
        { ATT_BT_UUID_SIZE, ecConnectUpdateTimeUUID },
        GATT_PERMIT_READ|GATT_PERMIT_WRITE,
        0,
        (uint8*)&UpdateTime
      }
};


/*********************************************************************
 * LOCAL FUNCTIONS
 */
static uint8 ECConnect_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen );


static uint8 ECConnect_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset );

static void ECConnect_HandleConnStatusCB( uint16 connHandle, uint8 changeType );
static void addConnectionToList();

void ECConnect_Reset()
{
  for(uint8 i = 0; i<SYSID_SIZE;i++)
  {
    InitialSysID[i] = 0; 
    SysID[i] = 0; 
  }
}

/*********************************************************************
 * PROFILE CALLBACKS
 */
// Device Info Service Callbacks
CONST gattServiceCBs_t ECConnectCBs =
{
  ECConnect_ReadAttrCB, // Read callback function pointer
  ECConnect_WriteAttrCB, // Write callback function pointer
  NULL                // Authorization callback function pointer
};

/*********************************************************************
 * NETWORK LAYER CALLBACKS
 */

/*********************************************************************
 * PUBLIC FUNCTIONS
 */

/*********************************************************************
 * @fn      DevInfo_AddService
 *
 * @brief   Initializes the Device Information service by registering
 *          GATT attributes with the GATT server.
 *
 * @return  Success or Failure
 */
bStatus_t ECConnect_AddService()
{
  KnownAddresList = GenericList_create();
  LastSysClock = osal_GetSystemClock();
  
  VOID linkDB_Register( ECConnect_HandleConnStatusCB );  
  // Register GATT attribute list and CBs with GATT Server App
  return GATTServApp_RegisterService( ECConnectAttrTbl,
                                      GATT_NUM_ATTRS( ECConnectAttrTbl ),
                                      &ECConnectCBs );
}


//Called when changes in the registration. 
static void InvokeCallback(ECC_Status_t newstate)
{
  ECConnect_AcceptedHost = newstate;
  if(CallBackhandler!=NULL)
  {
    CallBackhandler(newstate);
  }
}


/*********************************************************************
 * @fn      DevInfo_SetParameter
 *
 * @brief   Set a Device Information parameter.
 *
 * @param   param - Profile parameter ID
 * @param   len - length of data to write
 * @param   value - pointer to data to write.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate
 *          data type (example: data type of uint16 will be cast to
 *          uint16 pointer).
 *
 * @return  bStatus_t
 */
bStatus_t ECConnect_SetParameter( uint8 param, uint8 len, void *value )
{
  bStatus_t ret = SUCCESS;

  switch ( param )
  {
    
    default:
      ret = INVALIDPARAMETER;
      break;
  }

  return ( ret );
}

/*********************************************************************
 * @fn      DevInfo_GetParameter
 *
 * @brief   Get a Device Information parameter.
 *
 * @param   param - Profile parameter ID
 * @param   value - pointer to data to get.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate
 *          data type (example: data type of uint16 will be cast to
 *          uint16 pointer).
 *
 * @return  bStatus_t
 */
bStatus_t ECConnect_GetParameter( uint8 param, void *value )
{
  bStatus_t ret = SUCCESS;

  switch ( param )
  {

    default:
      ret = INVALIDPARAMETER;
      break;
  }

  return ( ret );
}

/*********************************************************************
 * @fn          ECConnect_ReadAttrCB
 *
 * @brief       Read an attribute.
 *
 * @param       connHandle - connection message was received on
 * @param       pAttr - pointer to attribute
 * @param       pValue - pointer to data to be read
 * @param       pLen - length of data to be read
 * @param       offset - offset of the first octet to be read
 * @param       maxLen - maximum length of data to be read
 *
 * @return      Success or Failure
 */
static uint8 ECConnect_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen )
{
  bStatus_t status = SUCCESS;
  uint16 uuid = BUILD_UINT16( pAttr->type.uuid[0], pAttr->type.uuid[1]);

  switch (uuid)
  {
    case UPDATETIME_CHARA_UUID:
      if (offset != 0)
      {
        status = ATT_ERR_INVALID_OFFSET;
      }
      else
      {
        *pLen = 4;
        osal_memcpy(pValue,&UpdateTime,*pLen);
      }
      break;

    default:
      *pLen = 0;
      status = ATT_ERR_READ_NOT_PERMITTED;
      break;
  }

  return ( status );
}

/*********************************************************************
 * @fn      ECConnect_WriteAttrCB
 *
 * @brief   Validate attribute data prior to a write operation
 *
 * @param   connHandle - connection message was received on
 * @param   pAttr - pointer to attribute
 * @param   pValue - pointer to data to be written
 * @param   len - length of data
 * @param   offset - offset of the first octet to be written
 *
 * @return  Success or Failure
 */

static uint8 ECConnect_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset )
{
  
  bStatus_t status = SUCCESS;
  
  if ( pAttr->type.len == ATT_BT_UUID_SIZE )
  {
   
    // 16-bit UUID
    uint16 uuid = BUILD_UINT16( pAttr->type.uuid[0], pAttr->type.uuid[1]);
    switch ( uuid )
    {
      case UPDATETIME_CHARA_UUID:
        {
            if(offset!=0)
              return ATT_ERR_INVALID_OFFSET;
     
            if(len == 4)
            {
              osal_memcpy(&UpdateTime,pValue,len);
              printf("Up");  
            }
            else
              status = ATT_ERR_INVALID_VALUE_SIZE;
        }
        break;
      case SYSTEMID_CHARA_UUID:
        if(len<=SYSID_SIZE)
        {
          if(osal_memcmp(SysID,InitialSysID,SYSID_SIZE)==TRUE)
          {
              osal_memcpy(SysID,pValue,len);
              if(passCodeCallback)
                passCodeCallback();
              addConnectionToList();
          }
          else if(osal_memcmp(SysID,pValue,len))
          {
            addConnectionToList();
          }
          else
          {
             status = ATT_ERR_UNSUPPORTED_REQ;
          }
        }
        else
          status = ATT_ERR_INVALID_VALUE_SIZE;
        break;
        
      default:
        // Should never get here! (characteristics 2 and 4 do not have write permissions)
        status = ATT_ERR_ATTR_NOT_FOUND;
        break;
    }
  }
  else
  {
    // 128-bit UUID
    status = ATT_ERR_INVALID_HANDLE;
  }

  return ( status );
  
  
  
  return 0;
}

/*********************************************************************
*********************************************************************/


/*********************************************************************
 * @fn          ECConnect_HandleConnStatusCB
 *
 * @brief        link status change handler function.
 *
 * @param       connHandle - connection handle
 * @param       changeType - type of change
 *
 * @return      none
 */
static void ECConnect_HandleConnStatusCB( uint16 connHandle, uint8 changeType )
{      
  switch(changeType)
  {
    case LINKDB_STATUS_UPDATE_NEW:  // New connection created
      {
        osal_set_event(EcConnect_TaskID,NEWLINK_EVENT);
      }
      break;
    case LINKDB_STATUS_UPDATE_REMOVED: // Connection was removed
      {
        osal_set_event(EcConnect_TaskID,TERMINATELINK_EVENT);
      }
      break;
  default:
    {
      volatile int a = 5;  
    }
    break; 
  }
  
}


static void addConnectionToList()
{
  KnownAddrs_t Address;
  GAPRole_GetParameter(GAPROLE_CONN_BD_ADDR, Address.addr);
  if(GenericList_contains(&KnownAddresList,(uint8*)&Address,sizeof(KnownAddrs_t))==false)
    GenericList_add(&KnownAddresList,(uint8*)&Address,sizeof(KnownAddrs_t));
  InvokeCallback(CONNECTED_ACCEPTED);
}


//***** ProcessEvent from OS *********************//

static void Update_UpdateTime()
{
    if(ECConnect_AcceptedHost == CONNECTED_SLEEPING)
    {
      uint32 time = osal_GetSystemClock();
      uint32 timeDiff = time<LastSysClock ? (0xFFFFFFFF - LastSysClock) + time : time-LastSysClock;
      LastSysClock = time;
      
        UpdateTime = UpdateTime<timeDiff ? 0: UpdateTime-timeDiff;
        if(UpdateTime == 0)
          InvokeCallback(DISCONNECTED);
        else
          osal_start_timerEx(EcConnect_TaskID,TIME_TICK,UpdateTime);
    } 
}

uint16 ECConnect_ProcessEvent( uint8 task_id, uint16 events )
{

  VOID task_id; // OSAL required parameter that isn't used in this function

  if ( events & TIME_TICK )
  {
      Update_UpdateTime();
      
      return ( events ^ TIME_TICK );
  }
  if ( events & NEWLINK_EVENT )
  {
      KnownAddrs_t Address;
      Update_UpdateTime();
      GAPRole_GetParameter(GAPROLE_CONN_BD_ADDR, Address.addr);
      if(GenericList_contains(&KnownAddresList,(uint8*)&Address,sizeof(KnownAddrs_t))==true)
        InvokeCallback(CONNECTED_ACCEPTED);
      else
        InvokeCallback(CONNECTED_NOTACCEPTED);
      
      return ( events ^ NEWLINK_EVENT );
  }
  if ( events & TERMINATELINK_EVENT )
  {
      if(ECConnect_AcceptedHost == CONNECTED_ACCEPTED)
      {
        InvokeCallback(CONNECTED_SLEEPING); 
        LastSysClock = osal_GetSystemClock();
        osal_start_timerEx(EcConnect_TaskID,TIME_TICK,UpdateTime);
      }
      else
        InvokeCallback(DISCONNECTED); 
      
      return ( events ^ TERMINATELINK_EVENT );
  }
  
  if ( events & SYS_EVENT_MSG ) // is not used 
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
  
  // Discard unknown events
  return 0;
}


void ECConnect_Init( uint8 task_id )
{
  EcConnect_TaskID = task_id;
}


void ECConnect_RegistreChangedCallback(ECConnect_StatusChange_t handler)
{
  CallBackhandler = handler;
}

void ECConnect_RegistrePassCodeCallback(ECConnect_GotPassCode handler)
{
  passCodeCallback = handler;
}


uint8* GetSetSystemID()
{
    return SysID;
}

void ECConnect_ClearPassCode()
{
    ECConnect_Reset();
    if(passCodeCallback)
      passCodeCallback();       
}