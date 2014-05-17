#pragma once

#include "GenericValueManger.h"
#include "gatt.h" 

//Defines 
#define MAX_SERVICES_SUPPORTED  10
#define UPDATE_START_COUNT      3


typedef enum 
{
  NONE = 0,
  YES
}Subscription;


//containing the GUI Presentation Format Descriptor
typedef struct {uint8 format; uint8 color;} GUIPresentationFormat;
typedef struct 
{
  uint8 Format;
  uint8 Exponent;
  uint16 Unit;
  uint8 Namespace;
  uint16 Description;
}PresentationFormat;

// is containing all the Characteristics and is making a list with a pointer to next item in list
// is Null if end of list.

typedef struct GenericCharacteristic  
{
  GenericValue value; 
  uint8  premission; 
  
  GenericValue userDescription;
  GUIPresentationFormat guiPresentationFormat; 
  PresentationFormat typePresentationFormat; 
  
  GenericValue range; 
  Subscription subscribtion; 
  uint8 gpio; 
  
  struct GenericCharacteristic* nextitem; 
  
}GenericCharacteristic; 

//contains a service, and have a pointer to the first element in the generic ValueList. 
typedef struct 
{
  GenericValue description;
         
  gattAttribute_t* llReg;  //used to add the profile to the ble stack. Is createt by the complie function. 
  
  GenericCharacteristic* first; // NULL if none 
  
}SmartService;

/* Contains handles to show in the Update Characteristic*/
typedef struct updateHandleContainor 
{
  uint16 handle; 
  struct updateHandleContainor* next;
}UpdateHandle;

extern SmartService* SmartCommandServices[MAX_SERVICES_SUPPORTED]; 
extern uint8 SmartCommandServices_Count; 
extern UpdateHandle* HandelsToUpdate;  //Contains handles to show in the Update Characteristic
//Functions 

/*
*   Description is in the .c file
*/

uint8 SmartCommandsManger_ElementsInService(SmartService* service); 

SmartService* SmartCommandsManger_CreateService(uint8* description, uint8 len);

bool SmartCommandsManger_DeleteService(SmartService* service);

uint16 SmartCommandsManger_addCharacteristic(uint8 initialValueSize,uint8* description, uint8 descriptioncount, GUIPresentationFormat guiPresentationFormat, PresentationFormat typeFormat, Subscription subscription, uint8 premission, uint8 gpio);

bool SmartCommandsManger_addRange(uint8* Range,uint8 len);

bool SmartCommandsManger_RemoveCharacteristic(SmartService* service,GenericCharacteristic* Characteristic);

bool SmartCommandsManger_CompileServices();

uint8 SmartCommandsManger_GetUpdate(uint8* ptr, uint8 maxsize);

void SmartCommandsManger_AddHandleToUpdate(uint16 handel);

GenericValue* GetCharacteristic(uint8 service,uint8 characteristic);

uint16 GetCharacteristicHandel(uint8 service,uint8 characteristic);

uint16 SmartCommandsManger_GetUpdateHandle(uint8 ServiceIndex);

uint16 GetCharacteristicUartHandle(GenericValue* data);

GenericCharacteristic* GetChare(uint8 service,uint8 characteristic);

GenericCharacteristic* GetCharaFromHandle(uint16 handle);