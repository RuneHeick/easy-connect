/**
  @headerfile:       central.h
  $Date: 2012-01-09 12:08:41 -0800 (Mon, 09 Jan 2012) $
  $Revision: 28871 $

  @mainpage TI BLE GAP Central Role

  This GAP profile discovers and initiates connections.

  Copyright 2011 Texas Instruments Incorporated. All rights reserved.

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
  PROVIDED �AS IS� WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED,
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
*/

#ifndef CENTRAL_H
#define CENTRAL_H

#ifdef __cplusplus
extern "C"
{
#endif

/*********************************************************************
 * INCLUDES
 */
#include "bcomdef.h"
#include "OSAL.h"
#include "gap.h"

/*********************************************************************
 * CONSTANTS
 */

/** @defgroup GAPCENTRALROLE_PROFILE_PARAMETERS GAP Central Role Parameters
 * @{
 */
#define GAPCENTRALROLE_IRK                 0x400  //!< Identity Resolving Key. Read/Write. Size is uint8[KEYLEN]. Default is all 0, which means that the IRK will be randomly generated.
#define GAPCENTRALROLE_SRK                 0x401  //!< Signature Resolving Key. Read/Write. Size is uint8[KEYLEN]. Default is all 0, which means that the SRK will be randomly generated.
#define GAPCENTRALROLE_SIGNCOUNTER         0x402  //!< Sign Counter. Read/Write. Size is uint32. Default is 0.
#define GAPCENTRALROLE_BD_ADDR             0x403  //!< Device's Address. Read Only. Size is uint8[B_ADDR_LEN]. This item is read from the controller.
#define GAPCENTRALROLE_MAX_SCAN_RES        0x404  //!< Maximum number of discover scan results to receive. Default is 0 = unlimited.
/** @} End GAPCENTRALROLE_PROFILE_PARAMETERS */

/**
 * Number of simultaneous links with periodic RSSI reads
 */
#ifndef GAPCENTRALROLE_NUM_RSSI_LINKS
#define GAPCENTRALROLE_NUM_RSSI_LINKS     4
#endif

  
  /*-------------------------------------------------------------------
 * CONSTANTS
 */

/** @defgroup GAPROLE_PROFILE_PARAMETERS GAP Role Parameters
 * @{
 */
#define GAPROLE_PROFILEROLE         0x300  //!< Reading this parameter will return GAP Role type. Read Only. Size is uint8.
#define GAPROLE_BD_ADDR             0x301  //!< Device's Address. Read Only. Size is uint8[B_ADDR_LEN]. This item is read from the controller.
#define GAPROLE_ADVERT_ENABLED      0x302  //!< Enable/Disable Advertising. Read/Write. Size is uint8. Default is TRUE=Enabled.
#define GAPROLE_ADVERT_OFF_TIME     0x303  //!< Advertising Off Time for Limited advertisements (in milliseconds). Read/Write. Size is uint16. Default is 30 seconds.
#define GAPROLE_ADVERT_DATA         0x304  //!< Advertisement Data. Read/Write. Size is uint8[B_MAX_ADV_LEN].  Default is "02:01:01", which means that it is a Limited Discoverable Advertisement.
#define GAPROLE_SCAN_RSP_DATA       0x305  //!< Scan Response Data. Read/Write. Size is uint8[B_MAX_ADV_LEN]. Defaults to all 0.
#define GAPROLE_ADV_EVENT_TYPE      0x306  //!< Advertisement Type. Read/Write. Size is uint8.  Default is GAP_ADTYPE_ADV_IND (defined in GAP.h).
#define GAPROLE_ADV_DIRECT_TYPE     0x307  //!< Direct Advertisement Address Type. Ready/Write. Size is uint8. Default is ADDRTYPE_PUBLIC (defined in GAP.h).
#define GAPROLE_ADV_DIRECT_ADDR     0x308  //!< Direct Advertisement Address. Read/Write. Size is uint8[B_ADDR_LEN]. Default is NULL.
#define GAPROLE_ADV_CHANNEL_MAP     0x309  //!< Which channels to advertise on. Read/Write Size is uint8. Default is GAP_ADVCHAN_ALL (defined in GAP.h)
#define GAPROLE_ADV_FILTER_POLICY   0x30A  //!< Filter Policy. Ignored when directed advertising is used. Read/Write. Size is uint8. Default is GAP_FILTER_POLICY_ALL (defined in GAP.h).
/** @} End GAPROLE_PROFILE_PARAMETERS */
  
  
/*********************************************************************
 * VARIABLES
 */

/*********************************************************************
 * MACROS
 */

/*********************************************************************
 * TYPEDEFS
 */
/**
 * GAP Broadcaster Role States.
 */
typedef enum
{
  GAPROLE_INIT = 0,                       //!< Waiting to be started
  GAPROLE_STARTED,                        //!< Started but not advertising
  GAPROLE_ADVERTISING,                    //!< Currently Advertising
  GAPROLE_WAITING,                        //!< Device is started but not advertising, is in waiting period before advertising again
  GAPROLE_WAITING_AFTER_TIMEOUT,          //!< Device just timed out from a connection but is not yet advertising, is in waiting period before advertising again
  GAPROLE_CONNECTED,                      //!< In a connection
  GAPROLE_ERROR                           //!< Error occurred - invalid state
} gaprole_States_t;
  
  
/**
 * Central Event Structure
 */
typedef union
{
  gapEventHdr_t             gap;                //!< GAP_MSG_EVENT and status.
  gapDeviceInitDoneEvent_t  initDone;           //!< GAP initialization done.
  gapDeviceInfoEvent_t      deviceInfo;         //!< Discovery device information event structure.
  gapDevDiscEvent_t         discCmpl;           //!< Discovery complete event structure.
  gapEstLinkReqEvent_t      linkCmpl;           //!< Link complete event structure.
  gapLinkUpdateEvent_t      linkUpdate;         //!< Link update event structure.
  gapTerminateLinkEvent_t   linkTerminate;      //!< Link terminated event structure.
} gapCentralRoleEvent_t;

/**
 * RSSI Read Callback Function
 */
typedef void (*pfnGapCentralRoleRssiCB_t)
(
  uint16 connHandle,                    //!< Connection handle.
  int8  rssi                            //!< New RSSI value.
);

/**
 * Central Event Callback Function
 */
typedef void (*pfnGapCentralRoleEventCB_t)
(
  gapCentralRoleEvent_t *pEvent         //!< Pointer to event structure.
);


// Peripheral callback state notify
typedef void (*gapRolesStateNotify_t)
(
  gaprole_States_t newState 
);

/**
 * Central Callback Structure
 */
typedef struct
{
  pfnGapCentralRoleRssiCB_t   rssiCB;   //!< RSSI callback.
  pfnGapCentralRoleEventCB_t  centralCB;  //!< Event callback.
  gapRolesStateNotify_t       broadcastCB;  //!< Whenever the device changes state  
} gapCentralRoleCB_t;

typedef struct
{
  gapRolesStateNotify_t    pfnStateChange;  //!< Whenever the device changes state
} gapRolesCBs_t;

/*-------------------------------------------------------------------
 * Profile Callbacks
 */



/*********************************************************************
 * VARIABLES
 */

/*********************************************************************
 * API FUNCTIONS
 */

/*-------------------------------------------------------------------
 * Central Profile Public APIs
 */

/**
 * @defgroup CENTRAL_PROFILE_API Central Profile API Functions
 *
 * @{
 */

/**
 * @brief   Start the device in Central role.  This function is typically
 *          called once during system startup.
 *
 * @param   pAppCallbacks - pointer to application callbacks
 *
 * @return  SUCCESS: Operation successful.<BR>
 *          bleAlreadyInRequestedMode: Device already started.<BR>
 */
extern bStatus_t GAPCentralRole_StartDevice( gapCentralRoleCB_t *pAppCallbacks, gapRolesCBs_t* calls, bool isCentral );

/**
 * @brief   Set a parameter in the Central Profile.
 *
 * @param   param - profile parameter ID: @ref GAPCENTRALROLE_PROFILE_PARAMETERS
 * @param   len - length of data to write
 * @param   pValue - pointer to data to write.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate
 *          data type.
 *
 * @return  SUCCESS: Operation successful.<BR>
 *          INVALIDPARAMETER: Invalid parameter ID.<BR>
 */
extern bStatus_t GAPCentralRole_SetParameter( uint16 param, uint8 len, void *pValue );

/**
 * @brief   Get a parameter in the Central Profile.
 *
 * @param   param - profile parameter ID: @ref GAPCENTRALROLE_PROFILE_PARAMETERS
 * @param   pValue - pointer to buffer to contain the read data
 *
 * @return  SUCCESS: Operation successful.<BR>
 *          INVALIDPARAMETER: Invalid parameter ID.<BR>
 */
extern bStatus_t GAPCentralRole_GetParameter( uint16 param, void *pValue );

/**
 * @brief   Terminate a link.
 *
 * @param   connHandle - connection handle of link to terminate
 *          or @ref GAP_CONN_HANDLE_DEFINES
 *
 * @return  SUCCESS: Terminate started.<BR>
 *          bleIncorrectMode: No link to terminate.<BR>
 */
extern bStatus_t GAPCentralRole_TerminateLink( uint16 connHandle );

/**
 * @brief   Establish a link to a peer device.
 *
 * @param   highDutyCycle -  TRUE to high duty cycle scan, FALSE if not
 * @param   whiteList - determines use of the white list: @ref GAP_WHITELIST_DEFINES
 * @param   addrTypePeer - address type of the peer device: @ref GAP_ADDR_TYPE_DEFINES
 * @param   peerAddr - peer device address
 *
 * @return  SUCCESS: started establish link process.<BR>
 *          bleIncorrectMode: invalid profile role.<BR>
 *          bleNotReady: a scan is in progress.<BR>
 *          bleAlreadyInRequestedMode: can�t process now.<BR>
 *          bleNoResources: too many links.<BR>
 */
extern bStatus_t GAPCentralRole_EstablishLink( uint8 highDutyCycle, uint8 whiteList,
                                               uint8 addrTypePeer, uint8 *peerAddr );

/**
 * @brief   Update the link connection parameters.
 *
 * @param   connHandle - connection handle
 * @param   connIntervalMin - minimum connection interval in 1.25ms units
 * @param   connIntervalMax - maximum connection interval in 1.25ms units
 * @param   connLatency - number of LL latency connection events
 * @param   connTimeout - connection timeout in 10ms units
 *
 * @return  SUCCESS: Connection update started started.<BR>
 *          bleIncorrectMode: No connection to update.<BR>
 */
extern bStatus_t GAPCentralRole_UpdateLink( uint16 connHandle, uint16 connIntervalMin,
                                            uint16 connIntervalMax, uint16 connLatency,
                                            uint16 connTimeout );
/**
 * @brief   Start a device discovery scan.
 *
 * @param   mode - discovery mode: @ref GAP_DEVDISC_MODE_DEFINES
 * @param   activeScan - TRUE to perform active scan
 * @param   whiteList - TRUE to only scan for devices in the white list
 *
 * @return  SUCCESS: Discovery scan started.<BR>
 *          bleIncorrectMode: Invalid profile role.<BR>
 *          bleAlreadyInRequestedMode: Not available.<BR>
 */
extern bStatus_t GAPCentralRole_StartDiscovery( uint8 mode, uint8 activeScan, uint8 whiteList );

/**
 * @brief   Cancel a device discovery scan.
 *
 * @return  SUCCESS: Cancel started.<BR>
 *          bleInvalidTaskID: Not the task that started discovery.<BR>
 *          bleIncorrectMode: Not in discovery mode.<BR>
 */
extern bStatus_t GAPCentralRole_CancelDiscovery( void );

/**
 * @brief   Start periodic RSSI reads on a link.
 *
 * @param   connHandle - connection handle of link
 * @param   period - RSSI read period in ms
 *
 * @return  SUCCESS: Terminate started.<BR>
 *          bleIncorrectMode: No link.<BR>
 *          bleNoResources: No resources.<BR>
 */
extern bStatus_t GAPCentralRole_StartRssi( uint16 connHandle, uint16 period );

/**
 * @brief   Cancel periodic RSSI reads on a link.
 *
 * @param   connHandle - connection handle of link
 *
 * @return  SUCCESS: Operation successful.<BR>
 *          bleIncorrectMode: No link.<BR>
 */
extern bStatus_t GAPCentralRole_CancelRssi(uint16 connHandle );

/**
 * @}
 */

/*-------------------------------------------------------------------
 * TASK API - These functions must only be called by OSAL.
 */

/**
 * @internal
 *
 * @brief   Central Profile Task initialization function.
 *
 * @param   taskId - Task ID.
 *
 * @return  void
 */
extern void GAPCentralRole_Init( uint8 taskId );

/**
 * @internal
 *
 * @brief   Central Profile Task event processing function.
 *
 * @param   taskId - Task ID
 * @param   events - Events.
 *
 * @return  events not processed
 */
extern uint16 GAPCentralRole_ProcessEvent( uint8 taskId, uint16 events );

/*********************************************************************
*********************************************************************/

/*-------------------------------------------------------------------
 * API FUNCTIONS 
 */

/**
 * @defgroup GAPROLES_BROADCASTER_API GAP Broadcaster Role API Functions
 * 
 * @{
 */
  
/**
 * @brief       Set a GAP Role parameter.
 *
 *  NOTE: You can call this function with a GAP Parameter ID and it will set the 
 *        GAP Parameter.  GAP Parameters are defined in (gap.h).  Also, 
 *        the "len" field must be set to the size of a "uint16" and the
 *        "pValue" field must point to a "uint16".
 *
 * @param       param - Profile parameter ID: @ref GAPROLE_PROFILE_PARAMETERS
 * @param       len - length of data to write
 * @param       pValue - pointer to data to write.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate 
 *          data type (example: data type of uint16 will be cast to 
 *          uint16 pointer).
 *
 * @return      SUCCESS or INVALIDPARAMETER (invalid paramID)
 */
extern bStatus_t GAPRole_SetParameter( uint16 param, uint8 len, void *pValue );
  
/**
 * @brief       Get a GAP Role parameter.
 *
 *  NOTE: You can call this function with a GAP Parameter ID and it will get a 
 *        GAP Parameter.  GAP Parameters are defined in (gap.h).  Also, the
 *        "pValue" field must point to a "uint16".
 *
 * @param       param - Profile parameter ID: @ref GAPROLE_PROFILE_PARAMETERS
 * @param       pValue - pointer to location to get the value.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate 
 *          data type (example: data type of uint16 will be cast to 
 *          uint16 pointer).
 *
 * @return      SUCCESS or INVALIDPARAMETER (invalid paramID)
 */
extern bStatus_t GAPRole_GetParameter( uint16 param, void *pValue );

/**
 * @brief       Does the device initialization.  Only call this function once.
 *
 * @param       pAppCallbacks - pointer to application callbacks.
 *
 * @return      SUCCESS or bleAlreadyInRequestedMode
 */

  
/**
 * @} End GAPROLES_BROADCASTER_API
 */   



#ifdef __cplusplus
}
#endif

#endif /* CENTRAL_H */
