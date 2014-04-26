/**************************************************************************************************
  Filename:       simpleGATTprofile.c
  Revised:        $Date: 2013-05-06 13:33:47 -0700 (Mon, 06 May 2013) $
  Revision:       $Revision: 34153 $

  Description:    This file contains the Simple GATT profile sample GATT service 
                  profile for use with the BLE sample application.

  Copyright 2010 - 2013 Texas Instruments Incorporated. All rights reserved.

  IMPORTANT: Your use of this Software is limited to those specific rights
  granted under the terms of a software license agreement between the user
  who downloaded the software, his/her employer (which must be your employer)
  and Texas Instruments Incorporated (the "License").  You may not use this
  Software unless you agree to abide by the terms of the License. The License
  limits your use, and you acknowledge, that the Software may not be modified,
  copied or distributed unless embedded on a Texas Instruments microcontroller
  or used solely and exclusively in conjunction with a Texas Instruments radio
  frequency transceiver, which is integrated into your product.  Other than for
  the foregoing purpose, you may not use, reproduce, copy, prepare derivative
  works of, modify, distribute, perform, display or sell this Software and/or
  its documentation for any purpose.

  YOU FURTHER ACKNOWLEDGE AND AGREE THAT THE SOFTWARE AND DOCUMENTATION ARE
  PROVIDED “AS IS” WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
  INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY, TITLE, 
  NON-INFRINGEMENT AND FITNESS FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL
  TEXAS INSTRUMENTS OR ITS LICENSORS BE LIABLE OR OBLIGATED UNDER CONTRACT,
  NEGLIGENCE, STRICT LIABILITY, CONTRIBUTION, BREACH OF WARRANTY, OR OTHER
  LEGAL EQUITABLE THEORY ANY DIRECT OR INDIRECT DAMAGES OR EXPENSES
  INCLUDING BUT NOT LIMITED TO ANY INCIDENTAL, SPECIAL, INDIRECT, PUNITIVE
  OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF PROCUREMENT
  OF SUBSTITUTE GOODS, TECHNOLOGY, SERVICES, OR ANY CLAIMS BY THIRD PARTIES
  (INCLUDING BUT NOT LIMITED TO ANY DEFENSE THEREOF), OR OTHER SIMILAR COSTS.

  Should you have any questions regarding your right to use this Software,
  contact Texas Instruments Incorporated at www.TI.com. 
**************************************************************************************************/

/*********************************************************************
 * INCLUDES
 */
#include "bcomdef.h"
#include "OSAL.h"
#include "linkdb.h"
#include "att.h"
#include "gatt.h"
#include "gatt_uuid.h"
#include "gattservapp.h"
#include "gapbondmgr.h"
#include "SmartCommandsManger.h"
#include "SmartCommandsProperties.h"
#include "EasyConnectProfile.h"

/*********************************************************************
 * MACROS
 */

/*********************************************************************
 * CONSTANTS
 */

#define SERVAPP_NUM_ATTR_SUPPORTED        17




/*********************************************************************
 * EXTERNAL VARIABLES
 */

/*********************************************************************
 * EXTERNAL FUNCTIONS
 */

/*********************************************************************
 * LOCAL VARIABLES
 */


/*********************************************************************
 * Profile Attributes - variables
 */


//  Each client has its own
// instantiation of the Client Characteristic Configuration. Reads of the
// Client Characteristic Configuration only shows the configuration for
// that client and writes only affect the configuration of that client.

gattCharCfg_t UpdateConfig[GATT_MAX_NUM_CONN];

static bool EC_isLocked = TRUE; 
static simpleProfileUnreadValueChange_t  unreadValueChange = NULL;
/*********************************************************************
 * LOCAL FUNCTIONS
 */
static uint8 simpleProfile_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr, 
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen );
static bStatus_t simpleProfile_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset );

static void simpleProfile_HandleConnStatusCB( uint16 connHandle, uint8 changeType );
static void simpleProfile_CCCUpdate(SmartService* service);

/*********************************************************************
 * PROFILE CALLBACKS
 */
// Simple Profile Service Callbacks
CONST gattServiceCBs_t simpleProfileCBs =
{
  simpleProfile_ReadAttrCB,  // Read callback function pointer
  simpleProfile_WriteAttrCB, // Write callback function pointer
  NULL                       // Authorization callback function pointer
};

/*********************************************************************
 * PUBLIC FUNCTIONS
 */

//Lock Read and Write for not ECConnected Devices. 

void SimpleProfile_SetItemLocked(bool isLocked)
{
  EC_isLocked = isLocked;
}



//Deice has Unread updates 
void SimpleProfile_RegistreUnreadCallback(simpleProfileUnreadValueChange_t call)
{
  unreadValueChange = call;
}

/*********************************************************************
 * @fn      SimpleProfile_AddService
 *
 * @brief   Initializes the Simple Profile service by registering
 *          GATT attributes with the GATT server.
 *
 * @param   services - services to add. This is a bit map and can
 *                     contain more than one service.
 *
 * @return  Success or Failure
 */

bStatus_t SimpleProfile_AddService( uint32 services )
{
  uint8 status = SUCCESS;
  uint16 addr;
  // Initialize Client Characteristic Configuration attributes
  GATTServApp_InitCharCfg( INVALID_CONNHANDLE, UpdateConfig );

  // Register with Link DB to receive link status change callback
  VOID linkDB_Register( simpleProfile_HandleConnStatusCB );  
    
  
  
  SmartService* Testservice = SmartCommandsManger_CreateService("Lav Kaffe",10); 
  SmartCommandsManger_addCharacteristic(50,"Antal kopper",13,(GUIPresentationFormat){00,00},(PresentationFormat){1,2,3,4,5},NONE,GATT_PERMIT_READ|GATT_PERMIT_WRITE);
  SmartCommandsManger_addCharacteristic(50,"Bonner",7,(GUIPresentationFormat){00,00},(PresentationFormat){7,4,5,6,5},YES,GATT_PERMIT_READ|GATT_PERMIT_WRITE);
  
  SmartCommandsManger_CompileServices();
  
  status = GATTServApp_RegisterService( Testservice->llReg, 
                                          SmartCommandsManger_ElementsInService(Testservice),
                                          &simpleProfileCBs );
  
  return ( status );
}
  

/*********************************************************************
 * @fn      SimpleProfile_SetParameter
 *
 * @brief   Set a Simple Profile parameter.
 *
 * @param   param - Profile parameter ID
 * @param   len - length of data to right
 * @param   value - pointer to data to write.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate 
 *          data type (example: data type of uint16 will be cast to 
 *          uint16 pointer).
 *
 * @return  bStatus_t
 */
bStatus_t SimpleProfile_SetParameter( uint16 address, uint8 len, void *value )
{
  bStatus_t ret = SUCCESS;
  uint8 service = HI_UINT16(address);
  uint8 characteristic = LO_UINT16(address);
  
  GenericValue* pValue = GetCharacteristic(service, characteristic); 
  uint16 handel; 
  
  if(pValue==NULL)
    return INVALIDPARAMETER;

  handel = GetCharacteristicHandel(service, characteristic);
  if(len <= pValue->size && handel != 0)
  {
    uint8 index; 
    
    osal_memcpy(pValue->pValue,value,len); 
    for(index = len;index<pValue->size;index++)
    {
      pValue->pValue[index] = '\0';
    }
      
      
    SmartCommandsManger_AddHandleToUpdate(handel);
    simpleProfile_CCCUpdate(SmartCommandServices[service-1]);
    if(unreadValueChange != NULL)
      unreadValueChange(true,SmartCommandsManger_GetUpdateHandle(service-1));  
  }
  else
  {
    ret = FAILURE;
  }
  

  return ( ret );
}

/*********************************************************************
 * @fn      SimpleProfile_GetParameter
 *
 * @brief   Get a Simple Profile parameter.
 *
 * @param   param - Profile parameter ID
 * @param   value - pointer to data to put.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate 
 *          data type (example: data type of uint16 will be cast to 
 *          uint16 pointer).
 *
 * @return  bStatus_t
 */
bStatus_t SimpleProfile_GetParameter( uint16 address, GenericValue* value )
{
  bStatus_t ret = SUCCESS;

  uint8 service = HI_UINT16(address);
  uint8 characteristic = LO_UINT16(address);

  osal_memcpy(value, GetCharacteristic(service, characteristic), sizeof(GenericValue));
  
  return ( ret );
}

/*********************************************************************
 * @fn          simpleProfile_ReadAttrCB
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
static uint8 simpleProfile_ReadAttrCB( uint16 connHandle, gattAttribute_t *pAttr, 
                            uint8 *pValue, uint8 *pLen, uint16 offset, uint8 maxLen )
{
  bStatus_t status = SUCCESS;
  if(EC_isLocked == TRUE)
  {
    return ( ATT_ERR_READ_NOT_PERMITTED );
  }
  
  
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
          uint8 len = strlen(pAttr->pValue);
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
        
      case SUPSCRIPTIONOPTION_DESCRIPTOR_UUID:
        {
          Subscription* value = (Subscription*) pAttr->pValue;
          if(offset==0)
          {
            *pLen = sizeof(Subscription);
            osal_memcpy(pValue,value,*pLen); 
            break;
          }
          else
          {
            return ( ATT_ERR_INVALID_OFFSET );
          }
        }
        
      case GUIPREFORMAT_DESCRIPTOR_UUID:
        {
          GUIPresentationFormat* value = (GUIPresentationFormat*) pAttr->pValue;
          if(offset==0)
          {
            *pLen = sizeof(GUIPresentationFormat);
            osal_memcpy(pValue,value,*pLen); 
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
        *pLen = SmartCommandsManger_GetUpdate(pValue, maxLen);
        if(unreadValueChange != NULL)
          unreadValueChange(false,0); 
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

/*********************************************************************
 * @fn      simpleProfile_WriteAttrCB
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
static bStatus_t simpleProfile_WriteAttrCB( uint16 connHandle, gattAttribute_t *pAttr,
                                 uint8 *pValue, uint8 len, uint16 offset )
{
  bStatus_t status = SUCCESS;
  
  if(EC_isLocked == TRUE)
  {
    return ( ATT_ERR_WRITE_NOT_PERMITTED );
  }
  
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
}

/*********************************************************************
 * @fn          simpleProfile_HandleConnStatusCB
 *
 * @brief       Simple Profile link status change handler function.
 *
 * @param       connHandle - connection handle
 * @param       changeType - type of change
 *
 * @return      none
 */
static void simpleProfile_HandleConnStatusCB( uint16 connHandle, uint8 changeType )
{ 
  // Make sure this is not loopback connection
  if ( connHandle != LOOPBACK_CONNHANDLE )
  {
    // Reset Client Char Config if connection has dropped
    if ( ( changeType == LINKDB_STATUS_UPDATE_REMOVED )      ||
         ( ( changeType == LINKDB_STATUS_UPDATE_STATEFLAGS ) && 
           ( !linkDB_Up( connHandle ) ) ) )
    { 
      GATTServApp_InitCharCfg( connHandle, UpdateConfig );
    }
  }
}


/*********************************************************************
*********************************************************************/


static void simpleProfile_CCCUpdate(SmartService* service)
{
  GATTServApp_ProcessCharCfg( UpdateConfig, NULL, FALSE,
                                    service->llReg, SmartCommandsManger_ElementsInService(service),
                                    INVALID_TASK_ID );
}


