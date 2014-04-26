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
  
  uint8 addr[B_ADDR_LEN];
}AcceptedDeviceInfo;

//***********************************************************
//      SericeDiscovery Structs 
//***********************************************************

typedef enum DiscoveryRange
{
  Primary = 0x00,
  Characteristic = 0x01,
  Descriptor = 0x02
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

typedef struct ValueHandelPropPair
{
  uint16 Handle;
  uint16 UUID; 
  uint8 Prop; 
}ValueHandelPropPair;

typedef union
{
  primary_ServiceItem service;
  ValueHandelPropPair characteristic;
  ValueHandelPair descriptors; 
}DiscoveryItem;

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
  ServiceDiscovery,
  None
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
  
  List result; 
  
}EventQueueServiceDirItem_t;


typedef union 
{
  EventQueueItemBase_t base;
  EventQueueRWItem_t read;
  EventQueueRWItem_t write;
  EventQueueScanItem_t scan;
  EventQueueServiceDirItem_t serviceDir;
  
}ConnectionEvents_t; 

//***********************************************************
//      Functions 
//***********************************************************

extern bool IsCentral; 

void ConnectionManger_Init( uint8 task_id);
uint16 ConnectionManger_ProcessEvent( uint8 task_id, uint16 events );

void ConnectionManager_Start(bool Central);

bool Queue_Contains(EventType_t action);
void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel, Callback call, Callback ecall);
void Queue_Scan(Callback call, Callback ecall);
void Queue_addServiceDiscovery(uint8* addr, Callback call ,Callback ecall, DiscoveryRange range, uint16 startHandle, uint16 endHandle);
void Queue_addRead(uint8* addr, uint16 handel, Callback call, Callback ecall);
uint8 Queue_Count();