#include "bcomdef.h"
#include "OSAL.h"
#include "OSAL_PwrMgr.h"
#include "OnBoard.h"
#include "hal_led.h"
#include "hal_key.h"
#include "hal_lcd.h"
#include "gatt.h"
#include "ll.h"
#include "hci.h"
#include "gapgattserver.h"
#include "gattservapp.h"
#include "central.h"
#include "gapbondmgr.h"
#include "simpleGATTprofile.h"
#include "simpleBLECentral.h"


#define MAX_HW_SUPPORTED_DEVICES 3

typedef struct 
{
  uint8 addr[B_ADDR_LEN];
  uint16 ConnHandel;
  bool InUse; 
}ConnectedDevice_t; 



static ConnectedDevice_t connectedDevices[MAX_HW_SUPPORTED_DEVICES];
static uint8 ConnectedIndex = 0; 
static uint8 ConnectedCount = 0; 



static void EstablishLink(ConnectedDevice_t* conContainor);
static void AcceptLink(uint8* addr, uint16 connHandel);

void CreateConnection(uint8* addr)
{
    uint8 i; 
    for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
    {
      if(connectedDevices[i].InUse == false)
      {
        osal_memcpy(connectedDevices[i].addr,addr,B_ADDR_LEN); 
        EstablishLink(&connectedDevices[i]); 
        return; 
      }
    }
    

     // fix me Some Error or correction.. maybe a queue  
}

static void EstablishLink(ConnectedDevice_t* conContainor)
{
  conContainor->InUse = true;
  
  if(conContainor->ConnHandel != GAP_CONNHANDLE_INIT)
  {
    GAPCentralRole_TerminateLink(conContainor->ConnHandel);
    conContainor->ConnHandel = GAP_CONNHANDLE_INIT;
  }
  /*
  GAPCentralRole_EstablishLink( DEFAULT_LINK_HIGH_DUTY_CYCLE,
                                      DEFAULT_LINK_WHITE_LIST,
                                      ADDRTYPE_PUBLIC, conContainor->addr );
  */
}

static void AcceptLink(uint8* addr, uint16 connHandel)
{
  uint8 i; 
  for(i = 0; i<MAX_HW_SUPPORTED_DEVICES; i++)
  {
      if(osal_memcmp(connectedDevices[i].addr,addr,B_ADDR_LEN))
      {
        connectedDevices[i].ConnHandel = connHandel;
        return; 
      }
   }
  
}
