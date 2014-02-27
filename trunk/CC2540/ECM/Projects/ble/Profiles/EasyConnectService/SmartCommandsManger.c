#include "bcomdef.h"
#include "OSAL.h"
#include "linkdb.h"
#include "att.h"
#include "gatt.h"
#include "gatt_uuid.h"
#include "gattservapp.h"
#include "gapbondmgr.h"
#include "SmartCommandsManger.h"
#include "EasyConnectProfile.h"
#include "SmartCommandsProperties.h"

#define GENERICCHAR_MANDATORY_DESCRIPTORS_COUNT       3
#define SERVICE_SELF_COUNT                            1
#define CHAR_SELF_COUNT                               2
#define DESC_SELF_COUNT                               1


/*********************************************************************
 * GLOBAL VARIABLES
 */

SmartService* SmartCommandServices[MAX_SERVICES_SUPPORTED]; 
uint8 SmartCommandServices_Count = 0; 


/*********************************************************************
 * LOCAL VARIABLES
 */

static void Local_RemoveCharacteristic(GenericCharacteristic* Characteristic);
static uint8 Local_CreateInfo(SmartService* service, gattAttribute_t* att, uint8 index, GenericCharacteristic* chara );
static void Local_Insert(gattAttribute_t* att, gattAttrType_t type, uint8 permissions, uint8 * pValue);
static uint8* GetReadAddress(uint8 readwrite);



uint8 SmartCommandsManger_ElementsInService(SmartService* service)
{
  uint8 count = SERVICE_SELF_COUNT+CHAR_SELF_COUNT+CHAR_SELF_COUNT; //the service and the description and the Update 
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
  if(SmartCommandServices_Count>=MAX_SERVICES_SUPPORTED )
    return NULL;
  
  bool succses = true; 
  SmartService* returnvalue = osal_mem_alloc(sizeof(SmartService));
  if(returnvalue!=NULL)
  {
    succses = GenericValue_SetString(&returnvalue->description, description);
    returnvalue->llReg = NULL; 
    returnvalue->first = NULL; 
  }
  
  
  
  if(succses==true)
  {
    SmartCommandServices[SmartCommandServices_Count++] = returnvalue;
    return returnvalue;
  }
  
  osal_mem_free(returnvalue);
  return NULL; 
}

bool SmartCommandsManger_DeleteService(SmartService* service)
{
  return false; 
}

bool SmartCommandsManger_addCharacteristic(SmartService* service,GenericValue* initialValue,uint8* description, GUIPresentationFormat guiPresentationFormat, PresentationFormat typeFormat,uint8* range, Subscription subscription, uint8 premission)
{
  if(service->llReg==NULL)
  {
    GenericCharacteristic* chare = osal_mem_alloc(sizeof(GenericCharacteristic));
    bool succses = true; 
    if(chare == NULL || initialValue->status!=READY) 
      return false; 
    
    chare->premission = premission; 
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
      Local_RemoveCharacteristic(chare);
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
  return false; 
}

bool SmartCommandsManger_RemoveCharacteristic(SmartService* service,GenericCharacteristic* characteristic)
{
  if(service->llReg==NULL)
    return false; 
  
  GenericCharacteristic* Tempchara = service->first;
  GenericCharacteristic* PriorItem = NULL;
  if(Tempchara != NULL)
  {
      GenericCharacteristic* nextItem = NULL;
      
      while(Tempchara != characteristic && Tempchara != NULL)
      {
        PriorItem = Tempchara;
        Tempchara = Tempchara->nextitem; 
      }
      
      if(Tempchara==NULL)
        return false; 
      
      nextItem = Tempchara->nextitem;
      
      if(PriorItem == NULL)
      {
        service->first = nextItem; 
      }
      else
      {
        PriorItem->nextitem = nextItem;
      }
      
      Local_RemoveCharacteristic(Tempchara); 
      
      return true;
  }
  
  return false; 
}

bool SmartCommandsManger_CompileServices()
{
  uint8 index = 0; 
  
  for(;index<SmartCommandServices_Count;index++)
  {
    if(!SmartCommandsManger_CompileService(SmartCommandServices[index]))
      return false; 
  }
  return true; 
}

bool SmartCommandsManger_CompileService(SmartService* service)
{
  uint8 numberOfItems = SmartCommandsManger_ElementsInService(service);
  uint8 index = 0; 
  
  if(service->llReg != NULL)
    osal_mem_free(service->llReg);
  
  service->llReg = osal_mem_alloc(sizeof(gattAttribute_t)*numberOfItems);
  if(service->llReg == NULL)
    return false; 
  
  GenericCharacteristic* chara = service->first;
  index += Local_CreateInfo(service, service->llReg, index, NULL);
  index++;
  for(; index<numberOfItems;index++)
  {
      index += Local_CreateInfo(service, service->llReg, index, chara);
      chara = chara->nextitem;
  }
 
  return true; 
}

static void Local_RemoveCharacteristic(GenericCharacteristic* characteristic)
{
    GenericValue_DeleteValue(&characteristic->userDescription);
    GenericValue_DeleteValue(&characteristic->range);
    osal_mem_free(characteristic);
}

static uint8 Local_CreateInfo(SmartService* service, gattAttribute_t* att, uint8 index, GenericCharacteristic* chara )
{
    uint8 count = index; 
    if( index == 0)
    {
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, primaryServiceUUID },GATT_PERMIT_READ,(uint8 *)&smartCommandServUUID); // service start;
      
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,&ReadProps); //Description String Declaration    
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, descriptionStringCharUUID },GATT_PERMIT_READ,service->description.pValue); //Description String Value
      
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,&ReadWriteProps); //Description String Declaration    
      Local_Insert(&att[index],(gattAttrType_t){ ATT_BT_UUID_SIZE, updateCharUUID },GATT_PERMIT_READ|GATT_PERMIT_WRITE,NULL); //Description String Value
      
    }
    else if(chara!=NULL)
    {
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,GetReadAddress(chara->premission)); //char Declaration
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, genericValuecharUUID },chara->premission,(uint8*)chara->value); //Value;
      
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, charUserDescUUID },GATT_PERMIT_READ ,chara->userDescription.pValue); //char description
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, guiPresentationDescUUID },GATT_PERMIT_READ ,(uint8*)&chara->guiPresentationFormat); //char description
      Local_Insert(&att[index],(gattAttrType_t){ ATT_BT_UUID_SIZE, charFormatUUID },GATT_PERMIT_READ ,(uint8*)&chara->typePresentationFormat); //char description
      
      if(chara->range.status == READY)
      {
        index++;
        Local_Insert(&att[index],(gattAttrType_t){ ATT_BT_UUID_SIZE, validRangeUUID },GATT_PERMIT_READ ,(uint8*)&chara->range); //char description
      }
      if(chara->subscribtion != NONE)
      {
        index++;
        Local_Insert(&att[index],(gattAttrType_t){ ATT_BT_UUID_SIZE, subscriptionDescUUID },GATT_PERMIT_READ ,(uint8*)&chara->subscribtion); //char description
      }
        
      
      
    }
    
    
    return index-count; 
}

static uint8* GetReadAddress(uint8 readwrite)
{
  switch(readwrite)
  {
    case GATT_PERMIT_AUTHEN_READ|GATT_PERMIT_AUTHEN_WRITE:
    case GATT_PERMIT_READ|GATT_PERMIT_WRITE:
      return &ReadWriteProps;
    case GATT_PERMIT_AUTHEN_WRITE:
    case GATT_PERMIT_WRITE:
      return &WriteProps;
    default:
      return &ReadProps;
  }
}


static void Local_Insert(gattAttribute_t* att, gattAttrType_t type, uint8 permissions, uint8 * pValue)
{
    struct attAttribute_t item = 
    {
      type,
      permissions,
      0,
      pValue
    };
    
    osal_memcpy(att, &item, sizeof(struct attAttribute_t));
}