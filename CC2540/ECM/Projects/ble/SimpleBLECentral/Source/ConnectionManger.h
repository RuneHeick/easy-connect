#pragma once
#include "GenericList.h"
#define MAX_HW_SUPPORTED_DEVICES 3

//***********************************************************
//      Connection Structs 
//***********************************************************
typedef enum
{
  NOTCONNECTED,
  CONNECTING,
  CONNECTED, 
  INUSE
}ConnectionStatus_t;

/* For Connected Devices */
typedef struct 
{
  uint8 addr[B_ADDR_LEN];
  uint16 ConnHandel;
  ConnectionStatus_t status; 
}ConnectedDevice_t; 
 
/*For Known Devices that is RU responsibility but not Connected */
typedef struct 
{
  uint32 KeepAliveTime_ms; // timer reload time
  uint32 KeepAliveTimeLeft_ms; //time left til Keep Alive Service 
  uint16 KeppAliveHandel; 
  
  uint16 UpdateHandel; // Handel to the Update function. Is Used to automatic get updates. 
  uint8 addr[B_ADDR_LEN];
}AcceptedDeviceInfo;

//***********************************************************
//      SericeDiscovery Structs 
//***********************************************************

typedef enum DiscoveryRange
{
  Primary,
  Characteristic,
  Descriptor
}DiscoveryRange;

typedef struct primary_ServiceItem
{
  uint16 handle; 
  uint16 endHandle; 
  uint16 ServiceUUID;  
}primary_ServiceItem;


typedef struct ValueHandelPair
{
  uint16 Handle;
  uint16 UUID; 
}ValueHandelPair;

typedef union
{
  primary_ServiceItem service;
  ValueHandelPair characteristic;
  ValueHandelPair descriptors; 
}DiscoveryItem;

typedef struct DiscoveryResult
{
  DiscoveryItem item; 
  struct DiscoveryResult* next;
}DiscoveryResult; 
//***********************************************************
//      Queue Structs 
//***********************************************************

/*********** helper **************************/

typedef enum
{
  Read,
  Write,
  Connect,
  Disconnect,
  Scan, 
  ServiceDiscovery
}EventType_t;


typedef void (*Callback)(void* item);

typedef union 
{
  attReadBlobReq_t read;
  gattPrepareWriteReq_t write; 
}rwreq_t; 

typedef struct ScanResponse
{
  uint8 eventType;
  
  uint8 addr[B_ADDR_LEN];
  int8 rssi;  
  
  uint8 dataLen;
  uint8 *pEvtData;
  
}ScanResponse_t;

/*********** base of queue items *************/
typedef struct EventQueueItemBase_t 
{
  uint8 addr[B_ADDR_LEN];
  EventType_t action;
  
  Callback errorcall; 
  Callback callback;
  
  struct EventQueueItemBase_t* next;
  
}EventQueueItemBase_t;

/*********** Items *************/

/*** RW item ***/
typedef struct EventQueueRWItem_t 
{
  EventQueueItemBase_t base;
  
  rwreq_t item; 
  List response; 
  
}EventQueueRWItem_t;

/*** Scan item ***/
typedef struct EventQueueScanItem_t 
{
  EventQueueItemBase_t base;
  List response;
}EventQueueScanItem_t;

/*** ServiceDir item ***/
typedef struct EventQueueServiceDirItem_t 
{
  EventQueueItemBase_t base;
  
  DiscoveryRange type; 
  uint16 startHandle;
  uint16 endHandle; 
  
  DiscoveryResult* result; 
  
}EventQueueServiceDirItem_t;

//***********************************************************
//      Functions 
//***********************************************************

void ConnectionManger_Init( uint8 task_id);
uint16 ConnectionManger_ProcessEvent( uint8 task_id, uint16 events );

void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel, Callback call, Callback ecall);
void Queue_Scan(Callback call, Callback ecall);
void Queue_addServiceDiscovery(uint8* addr, Callback call ,Callback ecall, DiscoveryRange range, uint16 startHandle, uint16 endHandle);
void Queue_addRead(uint8* addr, uint16 handel, Callback call, Callback ecall);