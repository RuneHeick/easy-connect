#include "bcomdef.h"
#include "OSAL.h"
#include "linkdb.h"
#include "att.h"
#include "gatt.h"
#include "gatt_uuid.h"
#include "gattservapp.h"
#include "gapbondmgr.h"
#include "SmartCommandsManger.h"

#define GENERICCHAR_MANDATORY_DESCRIPTORS_COUNT       3
#define SERVICE_SELF_COUNT                            1
#define CHAR_SELF_COUNT                               2
#define DESC_SELF_COUNT                               1

uint8 SmartCommandsManger_ElementsInService(SmartService* service)
{
  uint8 count = SERVICE_SELF_COUNT+DESC_SELF_COUNT+DESC_SELF_COUNT; //the service and the description and the Update 
  GenericCharacteristic* temp = service->first; 
  
  while(temp!=NULL)
  {
    count += CHAR_SELF_COUNT+(DESC_SELF_COUNT*GENERICCHAR_MANDATORY_DESCRIPTORS_COUNT); // Contains a Characteristic and have 3 mandatory Decriptors. 
    
    if(temp->range.status != NOT_INIT)
    {
      count+=DESC_SELF_COUNT;                          //have optinal descriptor
    }
    
    if(temp->subscribtion != NONE)
    {
      count+=DESC_SELF_COUNT;                          //have optinal descriptor
    }
    
    temp = temp->nextitem; 
  }
  
  return count;
}

SmartService* SmartCommandsManger_CreateService(uint8* description)
{
  bool succses = true; 
  SmartService* returnvalue = osal_mem_alloc(sizeof(SmartService));
  if(returnvalue!=NULL)
  {
    succses = GenericValue_SetString(&returnvalue->description, description);
    returnvalue->handel = 0; 
    returnvalue->first = NULL; 
  }

  if(succses==true)
    return returnvalue;
  
  osal_mem_free(returnvalue);
  return NULL; 
}

bool SmartCommandsManger_DeleteService(SmartService* service);

bool SmartCommandsManger_addCharacteristic(SmartService* service,GenericValue* initialValue,uint8* description, GUIPresentationFormat guiPresentationFormat, uint8 typeFormat,uint8* range, Subscription subscription)
{
  GenericCharacteristic* chare = osal_mem_alloc(sizeof(GenericCharacteristic));
  bool succses = true; 
  if(chare == NULL && initialValue->status==READY) 
    return false; 
  
  chare->handel = 0; 
  chare->guiPresentationFormat = guiPresentationFormat; 
  chare->subscribtion = subscription; 
  chare->typePresentationFormat = typeFormat;
  chare->value = initialValue;
  chare->nextitem = NULL; 
  
  if(!GenericValue_SetString(&chare->userDescription, description))
    succses = false; 
  
  if(range !=NULL)
    if(!GenericValue_SetValue(&chare->range, range,chare->value->size*2))
       succses = false; 
  
  if(succses == false)
  {
    GenericValue_DeleteValue(&chare->userDescription);
    if(chare->range.status != NOT_INIT)
      GenericValue_DeleteValue(&chare->range);
    osal_mem_free(chare);
  }
  else
  {
    if(service->first==NULL)
      service->first = chare; 
    else
    {
      GenericCharacteristic* temp = service->first; 
      while(temp->nextitem != NULL)
      {
        temp = temp->nextitem; 
      }
      temp->nextitem = chare; 
    }
  }
  
  return succses;
}

bool SmartCommandsManger_RemoveCharacteristic(SmartService* service,GenericCharacteristic* Characteristic);