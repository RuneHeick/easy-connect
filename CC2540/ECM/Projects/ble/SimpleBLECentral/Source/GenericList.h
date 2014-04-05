#pragma once
#include "OSAL.h"

typedef struct ListItem
{
   uint8* value; 
   uint8 size; 
   
   struct ListItem* next; 
}ListItem;

typedef struct List
{
  uint8 count; 
  ListItem* first; 
}List;

List GenericList_create(); 

void GenericList_add(List* list,uint8* val, uint8 len);  
void GenericList_remove(List* list, uint8 index); 
void GenericList_dispose(List* list); 

ListItem* GenericList_at(List* list,uint8 index); 