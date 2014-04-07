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


//variabels 
static uint8 TarskID;

//prototypes
static void ECConnectionChanged(ECC_Status_t newState);


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


  // Discard unknown events
  return 0;
}


void NormalState_Enter(uint8 tarskID)
{
  TarskID = tarskID;
  ECConnect_RegistreChangedCallback(ECConnectionChanged);
  
}

void NormalState_Exit()
{
  

    
}


static void ECConnectionChanged(ECC_Status_t newState)
{
  switch(newState)
  {
    case CONNECTED_ACCEPTED:
      {
        Setup_discoverableMode(GAP_ADTYPE_FLAGS_NON);
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
        Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL);
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break;
  }
  
}