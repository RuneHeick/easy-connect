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

typedef bool (*Condition)(ListItem* item);

List GenericList_create(); 

bool GenericList_add(List* list,uint8* val, uint8 len);  
void GenericList_remove(List* list, uint8 index); 
void GenericList_dispose(List* list); 
bool GenericList_contains(List* list, uint8* val, uint8 len); 
bool GenericList_HasElement(List* list,Condition con); 
ListItem* GenericList_First(List* list,Condition con);
ListItem* GenericList_at(List* list,uint8 index);