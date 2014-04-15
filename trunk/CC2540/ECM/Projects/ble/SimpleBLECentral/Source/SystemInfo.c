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

#include "GenericValueManger.h"
#include "SystemInfo.h"
#include "SmartCommandsProperties.h"

static bStatus_t InfoProfile_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset );

static uint8 InfoProfile_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr, 
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen );


uint8 SystemID[SYSIDSIZE];
GenericValue DeviceName, Password; 
static uint8 DecName[] = "Device Name";
static uint8 GuiPrFor[2] = {0x00};
static uint8 PrFor[7] = {0x00};

static uint8 DecPass[] = "Network Password";

static uint8 Description[] = "Init Room Unit"; 


// Device Info Service Callbacks
CONST gattServiceCBs_t InfoCBs =
{
  InfoProfile_ReadAttrCB,                 // Read callback function pointer
  InfoProfile_WriteAttrCB,               // Write callback function pointer
  NULL                // Authorization callback function pointer
};



static gattAttribute_t devInfoAttrTbl[] =
{
  // Device Information Service
  {
    { ATT_BT_UUID_SIZE, primaryServiceUUID }, /* type */
    GATT_PERMIT_READ,                         /* permissions */
    0,                                        /* handle */
    (uint8 *)&smartConnectService             /* pValue */
  },
  
    // char
    {
      { ATT_BT_UUID_SIZE, characterUUID },
      GATT_PERMIT_READ,
      0,
      &ReadProps
    },

      // Value
      {
        { ATT_BT_UUID_SIZE, descriptionStringCharUUID },
        GATT_PERMIT_READ,
        0,
        Description
      },

    // Char
    {
      { ATT_BT_UUID_SIZE, characterUUID },
      GATT_PERMIT_READ,
      0,
      &ReadWriteProps
    },

      // Update
      {
        { ATT_BT_UUID_SIZE, updateCharUUID },
        GATT_PERMIT_READ,
        0,
        NULL
      },

    // CCC
    {
      { ATT_BT_UUID_SIZE, clientCharCfgUUID },
      GATT_PERMIT_READ,
      0,
      NULL
    },

      // Char
      {
        { ATT_BT_UUID_SIZE, characterUUID },
        GATT_PERMIT_READ,
        0,
        &ReadWriteProps
      },
      // value
      {
        { ATT_BT_UUID_SIZE, genericValuecharUUID },
        GATT_PERMIT_READ|GATT_PERMIT_WRITE,
        0,
        (uint8*)&DeviceName
      },
      // Dec
    {
      { ATT_BT_UUID_SIZE, charUserDescUUID },
      GATT_PERMIT_READ,
      0,
      DecName
    },

      // Gui
      {
        { ATT_BT_UUID_SIZE, guiPresentationDescUUID },
        GATT_PERMIT_READ,
        0,
        GuiPrFor
      },

    // Dec
    {
      { ATT_BT_UUID_SIZE, charFormatUUID },
      GATT_PERMIT_READ,
      0,
      PrFor
    },
    
    
      // Char
      {
        { ATT_BT_UUID_SIZE, characterUUID },
        GATT_PERMIT_READ,
        0,
        &WriteProps
      },
      // value
      {
        { ATT_BT_UUID_SIZE, genericValuecharUUID },
        GATT_PERMIT_WRITE,
        0,
        (uint8*)&Password
      },
      // Dec
    {
      { ATT_BT_UUID_SIZE, charUserDescUUID },
      GATT_PERMIT_READ,
      0,
      DecPass
    },

      // Gui
      {
        { ATT_BT_UUID_SIZE, guiPresentationDescUUID },
        GATT_PERMIT_READ,
        0,
        GuiPrFor
      },

    // Dec
    {
      { ATT_BT_UUID_SIZE, charFormatUUID },
      GATT_PERMIT_READ,
      0,
      PrFor
    }, 
};


void InfoProfile_AddService()
{
  GenericValue_CreateContainer(&DeviceName,23);
  GenericValue_CreateContainer(&Password,100);
  GATTServApp_RegisterService( devInfoAttrTbl, 
                               GATT_NUM_ATTRS( devInfoAttrTbl ),
                               &InfoCBs );
}

static bStatus_t InfoProfile_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset )
{
  bStatus_t status = SUCCESS;
  
  if ( pAttr->type.len == ATT_BT_UUID_SIZE )
  {
    // 16-bit UUID
    uint16 uuid = BUILD_UINT16( pAttr->type.uuid[0], pAttr->type.uuid[1]);
    switch ( uuid )
    {
      case GENERICVALUE_CHARACTERISTICS_UUID:
        //Validate the value
        // Make sure it's not a blob oper
        {
          GenericValue* value = (GenericValue*) pAttr->pValue;
          if(offset+len<=value->size)
          {
            osal_memcpy(&value->pValue[offset],pValue,len);
            for(uint8 i = offset+len; i<value->size;i++)
              value->pValue[i] = 0; 
            break;
          }
          else
          {
            status = ATT_ERR_INVALID_VALUE_SIZE;
          }
        }
        
        break;

      case GATT_CLIENT_CHAR_CFG_UUID:
        status = GATTServApp_ProcessCCCWriteReq( connHandle, pAttr, pValue, len,
                                                 offset, GATT_CLIENT_CFG_INDICATE);
        break;
        
      default:
        // Should never get here! 
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
  
}




static uint8 InfoProfile_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr, 
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen )
{
  bStatus_t status = SUCCESS;
  
  // If attribute permissions require authorization to read, return error
  if ( gattPermitAuthorRead( pAttr->permissions ) )
  {
    // Insufficient authorization
    return ( ATT_ERR_INSUFFICIENT_AUTHOR );
  }
  
  if ( pAttr->type.len == ATT_BT_UUID_SIZE )
  {
    // 16-bit UUID
    uint16 uuid = BUILD_UINT16( pAttr->type.uuid[0], pAttr->type.uuid[1]);
    switch ( uuid )
    {
      // No need for "GATT_SERVICE_UUID" or "GATT_CLIENT_CHAR_CFG_UUID" cases;
      // gattserverapp handles those reads

      // characteristics 1 and 2 have read permissions
      // characteritisc 3 does not have read permissions; therefore it is not
      //   included here
      // characteristic 4 does not have read permissions, but because it
      //   can be sent as a notification, it is included here
      case GENERICVALUE_CHARACTERISTICS_UUID:
        {
          GenericValue* value = (GenericValue*) pAttr->pValue;
          if(offset<=value->size)
          {
            uint8 len = value->size-offset;
            *pLen = len>maxLen ? maxLen: len;
            
            for(uint8 i = 0;i<*pLen;i++)
            {
              if(value->pValue[offset+i]==0)
                *pLen = i;
            }
            
            osal_memcpy(pValue,&value->pValue[offset],*pLen); 
            break;
          }
          else
          {
            return ( ATT_ERR_INVALID_OFFSET );
          }
        }
        
      case DESCRIPTIONSTR_CHARACTERISTICS_UUID:
        {
          uint8 len = osal_strlen(pAttr->pValue);
          if(offset<=len)
          {
            *pLen = len-offset>maxLen ? maxLen: len-offset;
            osal_memcpy(pValue,&pAttr->pValue[offset],*pLen); 
            break;
          }
          else
          {
            return ( ATT_ERR_INVALID_OFFSET );
          }
        }
        
      case GUIPREFORMAT_DESCRIPTOR_UUID:
        {
          if(offset==0)
          {
            *pLen = sizeof(GuiPrFor);
            osal_memcpy(pValue,GuiPrFor,*pLen); 
            break;
          }
          else
          {
            return ( ATT_ERR_INVALID_OFFSET );
          }
        }
       case GATT_VALID_RANGE_UUID:
        {
          GenericValue* value = (GenericValue*) pAttr->pValue;
          if(offset<=value->size)
          {
            uint8 len = value->size-offset;
            *pLen = len>maxLen ? maxLen: len;
            osal_memcpy(pValue,&value->pValue[offset],*pLen); 
            break;
          }
          else
          {
            return ( ATT_ERR_INVALID_OFFSET );
          }
        }
      case UPDATE_CHARACTERISTICS_UUID:
        *pLen = 0;
        break;
        
      default:
        // Should never get here! (characteristics 3 and 4 do not have read permissions)
        *pLen = 0;
        status = ATT_ERR_ATTR_NOT_FOUND;
        break;
    }
  }
  else
  {
    // 128-bit UUID
    *pLen = 0;
    status = ATT_ERR_INVALID_HANDLE;
  }

  return ( status );
  
  
}