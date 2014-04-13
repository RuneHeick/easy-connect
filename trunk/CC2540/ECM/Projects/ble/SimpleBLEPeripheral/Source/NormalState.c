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

#include "peripheral.h"

#include "gapbondmgr.h"
#include "EasyConnectProfile.h"
#include "simpleBLEPeripheral.h"
#include "GenericValueManger.h"
#include "Uart.h"
#include "SmartCommandsManger.h"
#include "SmartCommandsProperties.h"
#include "ECConnect.h"
#include "InitState.h"


#define PERIODIC_TEST_EVENT (1<<10)

//variabels 
static uint8 TarskID;
static bool requestRead = false; 
//prototypes
static void ECConnectionChanged(ECC_Status_t newState);
static void hasApplicationUnreadData(bool hasUnreadData);


//functions
uint16 NormalState_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;

    if ( (pMsg = osal_msg_receive( task_id )) != NULL )
    {
      osal_event_hdr_t * pHdrMsg = (osal_event_hdr_t *)pMsg;
      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }

    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  
  if ( events & PERIODIC_TEST_EVENT )
  {
    SimpleProfile_SetParameter(0x0101,3,"HEJ"); // set to hej every time; 

    // return unprocessed events
    return (events ^ PERIODIC_TEST_EVENT);
  }

  // Discard unknown events
  return 0;
}


void NormalState_Enter(uint8 tarskID)
{
  TarskID = tarskID;
  ECConnect_RegistreChangedCallback(ECConnectionChanged);
  SimpleProfile_RegistreUnreadCallback(hasApplicationUnreadData);
  osal_start_reload_timer(tarskID,PERIODIC_TEST_EVENT,30000);
}

void NormalState_Exit()
{
  ECConnect_RegistreChangedCallback(NULL);
  SimpleProfile_RegistreUnreadCallback(NULL);
}


static void ECConnectionChanged(ECC_Status_t newState)
{
  switch(newState)
  {
    case CONNECTED_ACCEPTED:
      {
        Setup_discoverableMode(GAP_ADTYPE_FLAGS_NON,requestRead);
        SimpleProfile_SetItemLocked(false); //making profiles RW'abel 
      }
      break; 
    case CONNECTED_NOTACCEPTED:
      {
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break; 
    case DISCONNECTED:
      {
        Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead);
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break;
    case CONNECTED_SLEEPING:
      {
        if(requestRead)
          Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead);
        else
          Setup_discoverableMode(GAP_ADTYPE_FLAGS_NON,requestRead);
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break;
  }
  
}

static void hasApplicationUnreadData(bool hasUnreadData)
{
  requestRead = hasUnreadData; 
  if(hasUnreadData == true)
    Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead);
}