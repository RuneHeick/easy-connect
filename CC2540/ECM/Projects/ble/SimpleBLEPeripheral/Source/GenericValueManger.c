#include "GenericValueManger.h"


//Set an GenericValue to the value, need the length; 
bool GenericValue_SetValue(GenericValue* item, uint8* value, uint8 len)
{
  if(item->status != READY && item->status != NOT_INIT)
    return false; 
  
  if(item->status==READY)
    GenericValue_DeleteValue(item);
  
  item->pValue = osal_mem_alloc(len);
  if(item->pValue==NULL)
  { 
    item->status=ERROR; 
    return false; 
  }
  
  item->size = len; 
  
  osal_memcpy(item->pValue, value, len);
  item->status=READY; 
  return true;
}

bool GenericValue_CreateContainer(GenericValue* item, uint8 len)
{
  if(item->status != READY && item->status != NOT_INIT)
    return false; 
  
  if(item->status==READY)
    GenericValue_DeleteValue(item);
  
  item->pValue = osal_mem_alloc(len);
  if(item->pValue==NULL)
  { 
    item->status=ERROR; 
    return false; 
  }
  
  item->size = len; 
  item->status=READY; 
  return true;
}


// set string in item 
bool GenericValue_SetString(GenericValue* item, uint8* value)
{
  uint8 len = 0; 
  while(value[len]!='\0')
  {
    len++; // no end char; 
  }
    len++; // end char; 
  
  return GenericValue_SetValue(item, value, len);
}

//Get value or first part of value. 
uint8 GenericValue_GetValue(GenericValue* item, uint8* buffer, uint8 length)
{
  if(item->status == READY)
  {
    uint8 count = item->size>=length ? length : item->size;
    osal_memcpy(buffer, item->pValue, count);
    return count;
  }
  return 0;  
}




//Delete item, use to release heap
bool GenericValue_DeleteValue(GenericValue* item)
{
  if(item->status==READY)
  {
    osal_mem_free(item->pValue);
    item->status = NOT_INIT; 
    return true;     
  }
  return false; 
}