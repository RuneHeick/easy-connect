#include "GenericList.h"

List GenericList_create()
{
  List list; 
  list.count = 0; 
  list.first = NULL; 
  
  return list; 
}

bool GenericList_add(List* list,uint8* val, uint8 len)
{
  ListItem* item = osal_mem_alloc(sizeof(ListItem));
  item->value = osal_mem_alloc(len);
  
  if(item && item->value)
  {
    ListItem* temp = list->first;
    osal_memcpy(item->value,val,len);
    item->size = len; 
    item->next = NULL;
    
    if(temp==NULL)
      list->first = item; 
    else
    {
      while(temp->next!=NULL)
        temp = temp->next; 
      
      temp->next = item; 
    }
    
    list->count = list->count + 1 ; 
    return true; 
  }
  else
  {
    osal_mem_free(item);
    osal_mem_free(item->value);
    return false;
  }
}

void GenericList_remove(List* list, uint8 index)
{
  if(index < list->count && list->count>0)
  {
    ListItem* prior = NULL; 
    ListItem* item = GenericList_at(list,index); 
    ListItem* next = NULL;
    
    if(index != list->count-1)
      next = GenericList_at(list,index+1); 
    if(index != 0)
      prior = GenericList_at(list,index-1); 
    
    osal_mem_free(item->value);
    osal_mem_free(item);
    
    if(prior==NULL)
      list->first = next;
    else
      prior->next = next; 
    
    list->count = list->count-1; 
  }
}

void GenericList_dispose(List* list)
{
  while(list->count != 0)
  {
     GenericList_remove(list,0); 
  }
}

ListItem* GenericList_at(List* list,uint8 index)
{
  if(index < list->count && list->count>0)
  {
    uint8 destination = 0; 
    ListItem* item = list->first; 
    
    while(destination!=index)
    {
      item = item->next; 
      destination++;
    }
    
    return item; 
  }
  
  return NULL; 
}

bool GenericList_contains(List* list, uint8* val, uint8 len)
{
  if(list->count>0) 
  {
    ListItem* item = list->first; 
    if(item->size == len && osal_memcmp(val,item->value,len)==TRUE)
       return true; 
    
    while(item->next != NULL)
    {
      item = item->next;
      if(item->size == len && osal_memcmp(val,item->value,len)==TRUE)
       return true; 
    }
  
  }
    
  return false; 
}

bool GenericList_HasElement(List* list,Condition con)
{
  if(list->count>0) 
  {
    ListItem* item = list->first; 
    if(con(item))
       return true; 
    
    while(item->next != NULL)
    {
      item = item->next;
      if(con(item))
       return true; 
    }
  }
    
  return false; 
}

ListItem* GenericList_First(List* list,Condition con)
{
  if(list->count>0) 
  {
    ListItem* item = list->first; 
    if(con(item))
       return item; 
    
    while(item->next != NULL)
    {
      item = item->next;
      if(con(item))
       return item; 
    }
  }
    
  return NULL; 
}

uint16 GenericList_TotalSize(List* list)
{
  uint16 count = 0; 
  
  for(uint8 i = 0; i < list->count; i++)
  {
    ListItem* item = GenericList_at(list,i);
    count = count + item->size;
  }
  
  return count; 
}