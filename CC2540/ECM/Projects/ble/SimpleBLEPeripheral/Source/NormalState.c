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
#include "FileManager.h"
#include "UartReadWriteCommands.h"
#include "GPIOManager.h"

#define PERIODIC_TEST_EVENT (1<<10)


//variabels 
static uint8 TarskID;
static bool requestRead = false; 
static uint16 NewUpdateHandle = 0; 
//prototypes
static void ECConnectionChanged(ECC_Status_t newState);
static void hasApplicationUnreadData(bool hasUnreadData, uint16 handle);
static void WriteFromBLE(bool hasUnreadData, uint16 handle);
static void GotPasscode();
static void UpdateGPIO(GenericCharacteristic* chara);

static ECC_Status_t currentState = DISCONNECTED;

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
  
  if ( events & PERIODIC_TEST_EVENT ) // Only for test 
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
  ECConnect_RegistrePassCodeCallback(GotPasscode);
  SimpleProfile_RegistreUnreadCallback(hasApplicationUnreadData);
  SimpleProfile_RegistreBLEWriteCallback(WriteFromBLE); 
  //osal_start_reload_timer(tarskID,PERIODIC_TEST_EVENT,30000); // for test 
}

void NormalState_Exit()
{
  ECConnect_RegistreChangedCallback(NULL);
  SimpleProfile_RegistreUnreadCallback(NULL);
  SimpleProfile_RegistreBLEWriteCallback(NULL); 
}


static void ECConnectionChanged(ECC_Status_t newState)
{
  currentState = newState;
  
  switch(newState)
  {
    case CONNECTED_ACCEPTED:
      {
        printf("CA");
        SimpleProfile_SetItemLocked(false); //making profiles RW'abel 
      }
      break; 
    case CONNECTED_NOTACCEPTED:
      {
        printf("CN");
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break; 
    case DISCONNECTED:
      {
        printf("Dd");
        Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead,NewUpdateHandle);
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break;
    case CONNECTED_SLEEPING:
      {
        printf("Ss");
        if(requestRead)
          Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead,NewUpdateHandle);
        else
          Setup_discoverableMode(GAP_ADTYPE_FLAGS_NON,requestRead,NewUpdateHandle);
        SimpleProfile_SetItemLocked(true); //making profiles non RW'abel 
      }
      break;
  }

}

static void hasApplicationUnreadData(bool hasUnreadData, uint16 handle)
{
  requestRead = hasUnreadData;
  NewUpdateHandle = handle;
  
  GenericCharacteristic* chara =  GetCharaFromHandle(handle);
  UpdateGPIO(chara);
  
  /*  Wake from sleep-mode  */
  if(hasUnreadData == true && currentState == CONNECTED_SLEEPING)
    Setup_discoverableMode(GAP_ADTYPE_FLAGS_GENERAL,requestRead,handle);
}

/*  Updated Value from ECRU  */
static void WriteFromBLE(bool hasUnreadData, uint16 uarthandle)
{
  UartReadWrite_UpdateHandle(uarthandle); 
  
  GenericCharacteristic* chara = GetChare((uint8)(uarthandle>>8),(uint8)(uarthandle));
  UpdateGPIO(chara);
}

/*  Recived Passcode from BLE */
static void GotPasscode()
{
  FileManager_UpdatePassCode();
}

/*  Update the GPIO pin if any */
static void UpdateGPIO(GenericCharacteristic* chara)
{
  uint8 i; 
  for(i = 0; i<8; i++)
  {
    if( (chara->gpio>>i) & 0x01)
    {
      GPIO_Trig(i, chara->value.pValue[0]);
    }
  }
}