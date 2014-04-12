#pragma once
#include "OSAL.h"

typedef struct ListItem
{
   void* value; 
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

extern bool GenericList_add(List* list,void* val, uint8 len);  
extern void GenericList_remove(List* list, uint8 index); 
extern void GenericList_dispose(List* list); 
extern bool GenericList_contains(List* list, uint8* val, uint8 len); 
extern bool GenericList_HasElement(List* list,Condition con); 
extern ListItem* GenericList_First(List* list,Condition con);
extern ListItem* GenericList_at(List* list,uint8 index);

extern uint16 GenericList_TotalSize(List* list); 
extern uint8 GenericList_FirstAt(List* list,Condition con);