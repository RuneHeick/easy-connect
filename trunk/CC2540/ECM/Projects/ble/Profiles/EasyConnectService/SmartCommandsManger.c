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
#include "GPIOManager.h"

#define GENERICCHAR_MANDATORY_DESCRIPTORS_COUNT       3
#define SERVICE_SELF_COUNT                            1
#define CHAR_SELF_COUNT                               2
#define DESC_SELF_COUNT                               1


/*********************************************************************
 * GLOBAL VARIABLES
 */

SmartService* SmartCommandServices[MAX_SERVICES_SUPPORTED]; // COntains all services, Max 10 In this device. 
uint8 SmartCommandServices_Count = 0; // number of services creaded; 

UpdateHandle* HandelsToUpdate = NULL;   //Dynamicly allocating, will grow in size if possible, but not decrease. 

/*********************************************************************
 * LOCAL VARIABLES
 */

static void Local_RemoveCharacteristic(GenericCharacteristic* Characteristic);
static uint8 Local_CreateInfo(SmartService* service, gattAttribute_t* att, uint8 index, GenericCharacteristic* chara );
static void Local_Insert(gattAttribute_t* att, gattAttrType_t type, uint8 permissions, uint8 * pValue);
static uint8* GetReadAddress(uint8 readwrite);
static bool SmartCommandsManger_CompileService(SmartService* service);

/* Counts the number of BLE handle needed for the service  */
uint8 SmartCommandsManger_ElementsInService(SmartService* service)
{
  uint8 count = SERVICE_SELF_COUNT+CHAR_SELF_COUNT+CHAR_SELF_COUNT+DESC_SELF_COUNT; //the service and the description and the Update 
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

/* Create a Smart service, needs to be compiled before it can be used */
SmartService* SmartCommandsManger_CreateService(uint8* description, uint8 len)
{
  if(SmartCommandServices_Count>=MAX_SERVICES_SUPPORTED )
    return NULL;
  
  bool succses = true; 
  SmartService* returnvalue = osal_mem_alloc(sizeof(SmartService));
  if(returnvalue!=NULL)
  {
    osal_memset((uint8*)returnvalue,0,sizeof(SmartService)); // init
    succses = GenericValue_SetValue(&returnvalue->description, description, len);
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

/* Removes A service */
bool SmartCommandsManger_DeleteService(SmartService* service)
{
  return false; /* This is not possible at this time */
}

/* add Characteristic to the last regisred service, returns the Uart handle to the Characteristic. Retuens 0 if not possible */
uint16 SmartCommandsManger_addCharacteristic(uint8 initialValueSize,uint8* description, uint8 descriptionCount, GUIPresentationFormat guiPresentationFormat, PresentationFormat typeFormat, Subscription subscription, uint8 premission, uint8 gpio)
{
  SmartService* service; 
  
  if(SmartCommandServices_Count==0)
      return 0; 
    
  service = SmartCommandServices[SmartCommandServices_Count-1];
    
  if(service->llReg==NULL)
  {
    uint8 address = 1; 
    GenericCharacteristic* chare = osal_mem_alloc(sizeof(GenericCharacteristic));
    osal_memset((uint8*)chare,0,sizeof(GenericCharacteristic)); // set to init state
    bool succses = true; 
    if(chare == NULL) 
      return 0; 
    
    chare->premission = premission; 
    chare->guiPresentationFormat = guiPresentationFormat; 
    chare->subscribtion = subscription; 
    chare->typePresentationFormat = typeFormat;
    chare->gpio = gpio;
    
    uint8 i; 
    for(i = 0; i<8; i++)
    {
      if( (gpio>>i) & 0x01)
      {
        GPIO_register(i,typeFormat.Format);
      }
    }
    
    
    chare->nextitem = NULL; 
    
    if(!GenericValue_CreateContainer(&chare->value, initialValueSize))
      succses = false;
    
    if(!GenericValue_SetValue(&chare->userDescription, description, descriptionCount+1)) // one for the zero termination. 
      succses = false; 
    
    if(succses == false)
    {
      Local_RemoveCharacteristic(chare);
    }
    else
    {
      chare->userDescription.pValue[chare->userDescription.size-1] = 0; // must be 0 terminated; 
      if(service->first==NULL)
      {
        service->first = chare; 
      }
      else
      {
        GenericCharacteristic* temp = service->first;
        address++;
        while(temp->nextitem != NULL)
        {
          temp = temp->nextitem;
          address++;
        }
        temp->nextitem = chare; 
      }
    }
    if(succses)
    {
      uint8 servicecount = 0; 
      for(;servicecount<SmartCommandServices_Count;servicecount++)
      {
        if(SmartCommandServices[servicecount]== service)
        {
          return BUILD_UINT16(address,++servicecount);
        }
      }
    }
    else
      return 0;
  }
  return 0; 
}

/*  Add the Range descriptor to the last registed Characteristic */
bool SmartCommandsManger_addRange(uint8* Range,uint8 len)
{
  GenericCharacteristic* temp;
  SmartService* service;
  
  if(SmartCommandServices_Count==0)
      return false; 
  
  service = SmartCommandServices[SmartCommandServices_Count-1];
  
  temp = service->first;
  
  if(temp == NULL)
    return false; 
  
  while(temp->nextitem != NULL)
  { 
    temp = temp->nextitem; 
  }
  
  if(temp->range.status == NOT_INIT)
  {
    return GenericValue_SetValue(&temp->range,Range,len);  
  }
  
  return false; 
}

/*   Removes a Characteristic. THIS IS NOT USED IN THIS PROJECT*/
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

/*   Get the BLE handle for a Service Update Characteristic */
uint16 SmartCommandsManger_GetUpdateHandle(uint8 ServiceIndex)
{
  if(SmartCommandServices_Count>0)
  {
    SmartService* ser = SmartCommandServices[ServiceIndex];
    if(ser->llReg != NULL)
    {
      return ser->llReg[4].handle; 
    }
  }
  return 0; 
}

/*   Comples all services, after this is it not possible to changed the service, but it can be added to the GATT Manager */
bool SmartCommandsManger_CompileServices()
{
  uint8 index = 0; 
  
  for(;index<SmartCommandServices_Count;index++)
  {
    if(!SmartCommandsManger_CompileService(SmartCommandServices[index]))
      return false;     
  }
  
  if(HandelsToUpdate==NULL)
  {
      UpdateHandle* tempUpdate = osal_mem_alloc(sizeof(UpdateHandle));
      HandelsToUpdate = tempUpdate;
      if(HandelsToUpdate==NULL)
        return false; 
      
      tempUpdate->handle=0;
      tempUpdate->next = NULL; 
      
      for(index=1; index<UPDATE_START_COUNT;index++)
      {
        UpdateHandle* temp2update = osal_mem_alloc(sizeof(UpdateHandle));
        if(temp2update==NULL)
          break; 
        
        tempUpdate->next = temp2update;
        tempUpdate = temp2update;
        
        tempUpdate->handle=0;
        tempUpdate->next = NULL;  
      }
  }
  
  return true; 
}

/* Add a handle to the Update Characteristic of All services */
void SmartCommandsManger_AddHandleToUpdate(uint16 handel)
{
  UpdateHandle* tempValue = HandelsToUpdate;
  
  while(tempValue->handle != 0 && tempValue->handle != handel && tempValue->next != NULL)
  {
    tempValue = tempValue->next;
  }
  
  if(tempValue->handle==0)
  {
    tempValue->handle = handel;
  }
  else if(tempValue->next == NULL)
  {
    UpdateHandle* newItem = osal_mem_alloc(sizeof(UpdateHandle));
    if(newItem!=NULL)
    {
      tempValue->next = newItem;
      newItem->handle = handel;
      newItem->next = NULL; 
    }
  }
  
}

/* get all the handles in the Update Characteristic, Returns the number of handles in the Update Characteristic. */
uint8 SmartCommandsManger_GetUpdate(uint8* ptr, uint8 maxsize)
{
  UpdateHandle* tempValue = HandelsToUpdate;
  uint8 count = 0; 
  
  for(;count<maxsize && tempValue->handle != 0 && tempValue->next != NULL;count+=sizeof(uint16))
  {
    osal_memcpy(&ptr[count],&tempValue->handle,sizeof(uint16));
    tempValue = tempValue->next;
  }
  
  tempValue = HandelsToUpdate;
  tempValue->handle = 0; 
  
  while(tempValue->next != NULL)
  {
    tempValue = tempValue->next;
    tempValue->handle = 0; 
  }
  
  return count;
}


/* Get the Characteristic Value, in a service */
GenericValue* GetCharacteristic(uint8 service,uint8 characteristic)
{
  GenericCharacteristic* chara = GetChare(service,characteristic);
  return &chara->value;
}


/* Get the Characteristic, in a service */
GenericCharacteristic* GetChare(uint8 service,uint8 characteristic)
{
  uint8 count = 1; 
  
  if(service>SmartCommandServices_Count||service==0)
    return NULL;
  
  
  SmartService* servicePtr = SmartCommandServices[service-1];
  GenericCharacteristic* chara = servicePtr->first;
  
  if(chara==NULL)
    return NULL; 
  
  for(;count != characteristic;count++)
  {
    chara = chara->nextitem;
    if(chara == NULL)
      return NULL;
  }
  
  return chara;
}

/* Get the Uart handle for a special Characteristic by sercing for the pointer*/
uint16 GetCharacteristicUartHandle(GenericValue* data)
{
  for(uint8 s = 0; s < SmartCommandServices_Count; s++)
  {
    GenericCharacteristic* temp = SmartCommandServices[s]->first; 
    uint8 count = 1; 
    
    while(temp!=NULL)
    {
      if(&temp->value == data)
      return ((s+1)<<8)+count;
      
      temp = temp->nextitem;
      count++;
    }
    
  }
}

/* Get the BLE handle for a Characteristic */
uint16 GetCharacteristicHandel(uint8 service,uint8 characteristic)
{
  SmartService* servicePtr = SmartCommandServices[service-1];
  if(servicePtr != NULL && servicePtr->llReg != NULL)
  {
    uint8 count = SERVICE_SELF_COUNT+CHAR_SELF_COUNT+CHAR_SELF_COUNT+DESC_SELF_COUNT; //the service and the description and the Update 
    uint8 addrCount = 1; 
    GenericCharacteristic* temp = servicePtr->first; 
    
    while(temp!=NULL)
    {
      if(addrCount==characteristic)
        return servicePtr->llReg[++count].handle;
      
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
      addrCount++;
    }
  }
  
  return 0; 
}


/* Get the BLE handle for a Characteristic */
GenericCharacteristic* GetCharaFromHandle(uint16 handle)
{
   for(uint8 s = 0; s < SmartCommandServices_Count; s++)
   {
      SmartService* servicePtr = SmartCommandServices[s-1];
      
      if(servicePtr != NULL && servicePtr->llReg != NULL)
      {
        uint8 count = SERVICE_SELF_COUNT+CHAR_SELF_COUNT+CHAR_SELF_COUNT+DESC_SELF_COUNT; //the service and the description and the Update 
        uint8 addrCount = 1; 
        GenericCharacteristic* temp = servicePtr->first; 
        
        while(temp!=NULL)
        {
          
          if(servicePtr->llReg[++count].handle == handle)
            return temp;
          
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
          addrCount++;
        }
      }
    
   }
   return NULL; 
}

/* Compiles a singele service */
static bool SmartCommandsManger_CompileService(SmartService* service)
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

/* Used when compiling the service to create the GATT item*/
static uint8 Local_CreateInfo(SmartService* service, gattAttribute_t* att, uint8 index, GenericCharacteristic* chara )
{
    uint8 count = index; 
    if( index == 0)
    {
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, primaryServiceUUID },GATT_PERMIT_READ,(uint8 *)&smartConnectService); // service start;
      
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,&ReadProps); //Description String Declaration    
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, descriptionStringCharUUID },GATT_PERMIT_READ,(uint8*)&service->description); //Description String Value
      
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,&ReadWriteProps); //Description String Declaration    
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, updateCharUUID },GATT_PERMIT_READ|GATT_PERMIT_WRITE,NULL); //Description String Value
      Local_Insert(&att[index],(gattAttrType_t){ ATT_BT_UUID_SIZE, clientCharCfgUUID },GATT_PERMIT_READ|GATT_PERMIT_WRITE, (uint8 *)UpdateConfig); //Description String Value
      
    }
    else if(chara!=NULL)
    {
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, characterUUID },GATT_PERMIT_READ,GetReadAddress(chara->premission)); //char Declaration
      Local_Insert(&att[index++],(gattAttrType_t){ ATT_BT_UUID_SIZE, genericValuecharUUID },chara->premission,(uint8*)&chara->value); //Value;
      
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

/* Translate to GATT READ WRITE INFO*/
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

/*Inserts GATT Item */
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