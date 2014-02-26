#pragma once

#include "GenericValueManger.h"

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
  uint16 handel;
  
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
  uint16 handel; 
  
  GenericCharacteristic* first; 
  
}SmartService;


//Functions 

uint8 SmartCommandsManger_ElementsInService(SmartService* service); 

SmartService* SmartCommandsManger_CreateService(uint8* description);

bool SmartCommandsManger_DeleteService(SmartService* service);

bool SmartCommandsManger_addCharacteristic(SmartService* service,GenericValue* initialValue,uint8* description, GUIPresentationFormat guiPresentationFormat, uint8 typeFormat,uint8* range, Subscription subscription);

bool SmartCommandsManger_RemoveCharacteristic(SmartService* service,GenericCharacteristic* Characteristic);

