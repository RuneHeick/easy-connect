#include "SmartCommandsProperties.h"

/*********************************************************************
 * GLOBAL VARIABLES
 */

CONST gattAttrType_t smartConnectService = { ATT_BT_UUID_SIZE, smartCommandServUUID };

// Easy Connect Service UUID: 0x1820
CONST uint8 smartCommandServUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(SMARTCOMMAND_SERV_UUID), HI_UINT16(SMARTCOMMAND_SERV_UUID)
};

// Generic Value Characteristics
CONST uint8 genericValuecharUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(GENERICVALUE_CHARACTERISTICS_UUID), HI_UINT16(GENERICVALUE_CHARACTERISTICS_UUID)
};

// Update Characteristics 
CONST uint8 updateCharUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(UPDATE_CHARACTERISTICS_UUID), HI_UINT16(UPDATE_CHARACTERISTICS_UUID)
};

//GUI Presentation Format Descriptor
CONST uint8 guiPresentationDescUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(GUIPREFORMAT_DESCRIPTOR_UUID), HI_UINT16(GUIPREFORMAT_DESCRIPTOR_UUID)
};

//Subscription Option Descriptor
CONST uint8 subscriptionDescUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(SUPSCRIPTIONOPTION_DESCRIPTOR_UUID), HI_UINT16(SUPSCRIPTIONOPTION_DESCRIPTOR_UUID)
};

//Description String Characteristic
CONST uint8 descriptionStringCharUUID[ATT_BT_UUID_SIZE] =
{ 
  LO_UINT16(DESCRIPTIONSTR_CHARACTERISTICS_UUID), HI_UINT16(DESCRIPTIONSTR_CHARACTERISTICS_UUID)
};

/* Needs to be in a static place in memory for the GATT Manager*/
uint8 ReadProps = GATT_PROP_READ ;
uint8 WriteProps =  GATT_PROP_WRITE;
uint8 ReadWriteProps = GATT_PROP_READ | GATT_PROP_WRITE;