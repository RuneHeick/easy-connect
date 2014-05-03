#pragma once 
#include "bcomdef.h"

#define SYSID_SIZE 8
#define ECCONNECT_STARTTIME 60000 // in ms
#define TIMER_PERIODE 1000 // in ms

typedef enum 
{
  CONNECTED_ACCEPTED, 
  CONNECTED_NOTACCEPTED,
  DISCONNECTED,
  CONNECTED_SLEEPING
}ECC_Status_t;

typedef void (*ECConnect_StatusChange_t)(ECC_Status_t newState );

typedef void (*ECConnect_GotPassCode)(void);

extern ECC_Status_t ECConnect_AcceptedHost;

/*********************************************************************
 * API FUNCTIONS
 */

/*
 * ECConnect_AddService- Initializes the Device Information service by registering
 *          GATT attributes with the GATT server.
 *
 */

extern bStatus_t ECConnect_AddService();

/*********************************************************************
 * @fn      ECConnect_SetParameter
 *
 * @brief   Set a Device Information parameter.
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
bStatus_t ECConnect_SetParameter( uint8 param, uint8 len, void *value );

/*
 * ECConnect_GetParameter - Get a Device Information parameter.
 *
 *    param - Profile parameter ID
 *    value - pointer to data to write.  This is dependent on
 *          the parameter ID and WILL be cast to the appropriate
 *          data type (example: data type of uint16 will be cast to
 *          uint16 pointer).
 */
extern bStatus_t ECConnect_GetParameter( uint8 param, void *value );

extern void ECConnect_Init( uint8 task_id );

extern uint16 ECConnect_ProcessEvent( uint8 task_id, uint16 events );


/*
 * ECConnect_RegistreChangedCallback - registre a callback function.
    This is called if there is any changes in the ECConnect State. 
 */
extern void ECConnect_RegistreChangedCallback(ECConnect_StatusChange_t handler);

extern void ECConnect_RegistrePassCodeCallback(ECConnect_GotPassCode handler);

extern uint8* GetSetSystemID();

extern void ECConnect_Reset();

extern void ECConnect_ClearPassCode();