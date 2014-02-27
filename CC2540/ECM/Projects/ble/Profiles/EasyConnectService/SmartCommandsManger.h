#pragma once

#include "GenericValueManger.h"
#include "gatt.h" 
//Defines 



typedef enum 
{
  NONE = 0,
  YES
}Subscription;


//containing the GUI Presentation Format Descriptor
typedef struct {uint8 format; uint8 color;} GUIPresentationFormat;

// is containing all the Characteristics and is making a list with a pointer to next item in list
// is Null if end of list.

typedef struct GenericCharacteristic GenericCharacteristic;

struct GenericCharacteristic  
{
  GenericValue* value; 
  uint8  premission; 
  
  GenericValue userDescription;
  GUIPresentationFormat guiPresentationFormat; 
  uint8 typePresentationFormat; 
  
  GenericValue range; 
  Subscription subscribtion; 
  
  GenericCharacteristic* nextitem; 
  
}; 

//contains a service, and have a pointer to the first element in the generic ValueList. 
typedef struct 
{
  GenericValue description;
         
  gattAttribute_t* llReg;  //used to add the profile to the ble stack. Is createt by the complie function. 
  
  GenericCharacteristic* first; 
  
}SmartService;


//Functions 

uint8 SmartCommandsManger_ElementsInService(SmartService* service); 

SmartService* SmartCommandsManger_CreateService(uint8* description);

bool SmartCommandsManger_DeleteService(SmartService* service);

bool SmartCommandsManger_addCharacteristic(SmartService* service,GenericValue* initialValue,uint8* description, GUIPresentationFormat guiPresentationFormat, uint8 typeFormat,uint8* range, Subscription subscription, uint8 premission);

bool SmartCommandsManger_RemoveCharacteristic(SmartService* service,GenericCharacteristic* Characteristic);

bool SmartCommandsManger_CompileService(SmartService* service);