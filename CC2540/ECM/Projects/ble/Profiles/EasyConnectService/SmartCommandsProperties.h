#pragma once 
#include "bcomdef.h"
#include "OSAL.h"
#include "linkdb.h"
#include "att.h"
#include "gatt.h"
#include "gatt_uuid.h"
#include "gattservapp.h"
#include "gapbondmgr.h"

/*********************************************************************
 * CONSTANTS
 */
  
// Easy Connect Service UUID
#define SMARTCOMMAND_SERV_UUID                  0x1820
    
// Generic Value Characteristics 
#define GENERICVALUE_CHARACTERISTICS_UUID       0x2A70
  
// Update Characteristics 
#define UPDATE_CHARACTERISTICS_UUID             0x2A71
   
//Description String Characteristic
#define DESCRIPTIONSTR_CHARACTERISTICS_UUID     0x2A72

//GUI Presentation Format Descriptor
#define  GUIPREFORMAT_DESCRIPTOR_UUID           0x2910
  
//Subscription Option Descriptor
#define  SUPSCRIPTIONOPTION_DESCRIPTOR_UUID     0x2911
  
//Characteristic Presentation Format Descriptor
#define  PRESENTATIONFORMAT_DESCRIPTOR_UUID     0x2904
  
//Client Characteristic Configuration Descriptor
#define  CCC_DESCRIPTOR_UUID                    0x2902

//Characteristic User Description Descriptor
#define USERDESCRIPTION_DESCRIPTOR_UUID         0x2901
  
// Simple Keys Profile Services bit fields
#define SIMPLEPROFILE_SERVICE_UUID              0x00000001
 


// Easy Connect Service UUID: 0x1820
extern CONST uint8 smartCommandServUUID[ATT_BT_UUID_SIZE];

// Generic Value Characteristics
extern CONST uint8 genericValuecharUUID[ATT_BT_UUID_SIZE];

// Update Characteristics 
extern CONST uint8 updateCharUUID[ATT_BT_UUID_SIZE];

//GUI Presentation Format Descriptor
extern CONST uint8 guiPresentationDescUUID[ATT_BT_UUID_SIZE];

//Subscription Option Descriptor
extern CONST uint8 subscriptionDescUUID[ATT_BT_UUID_SIZE];

extern CONST uint8 descriptionStringCharUUID[ATT_BT_UUID_SIZE];


extern uint8 ReadProps ;
extern uint8 WriteProps ;
extern uint8 ReadWriteProps;