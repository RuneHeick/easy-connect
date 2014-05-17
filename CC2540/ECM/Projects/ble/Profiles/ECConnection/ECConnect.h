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

/* Add the service to the GATT Manager and starts it*/
extern bStatus_t ECConnect_AddService();


/*  OSAL functions*/
extern void ECConnect_Init( uint8 task_id );
extern uint16 ECConnect_ProcessEvent( uint8 task_id, uint16 events );


/*
 * ECConnect_RegistreChangedCallback - registre a callback function.
    This is called if there is any changes in the ECConnect State. 
 */
extern void ECConnect_RegistreChangedCallback(ECConnect_StatusChange_t handler);


/*
 * ECConnect_RegistrePassCodeCallback - registre a callback function.
    This is called if there is any changes in the SYSTEMID. 
 */
extern void ECConnect_RegistrePassCodeCallback(ECConnect_GotPassCode handler);


/* Get a pointer to the System ID */ 
extern uint8* GetSetSystemID();

 /* Init */
extern void ECConnect_Reset();

/* Clear the systemID */
extern void ECConnect_ClearPassCode();