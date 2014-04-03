#pragma once

#define MAX_HW_SUPPORTED_DEVICES 3

typedef void (*Scancallback)(gapDevRec_t *devices, uint8 length);
typedef void (*Callback)(uint8* buffer, uint8 length);
typedef void (*ErrorCallback)();

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
//      Queue Structs 
//***********************************************************

typedef enum
{
  Read,
  Write,
  Connect,
  Disconnect,
  Scan, 
  ServiceDiscovery
}EventType_t;

typedef enum ScanType
{
  Primary,
  Characteristic,
  Descriptor
}ScanType;

/*********** base of queue items *************/
typedef struct EventQueueItemBase_t 
{
  uint8 addr[B_ADDR_LEN];
  EventType_t action;
  
  ErrorCallback errorcall; 
  
  struct EventQueueItemBase_t* next;
  
}EventQueueItemBase_t;


/*** RW item ***/
typedef struct EventQueueRWItem_t 
{
  EventQueueItemBase_t base;
  
  uint16 handel;
  uint8* data;
  uint8 length; 
  
  Callback callback;
  
}EventQueueRWItem_t;

/*** Scan item ***/
typedef struct EventQueueScanItem_t 
{
  EventQueueItemBase_t base;
  Scancallback callback;
  
}EventQueueScanItem_t;

/*** ServiceDir item ***/
typedef struct EventQueueServiceDirItem_t 
{
  EventQueueItemBase_t base;
  Callback callback;
  
  ScanType type; 
  uint16 startHandle;
  uint16 endHandle; 
  
  void* Items; 
  
}EventQueueServiceDirItem_t;

//***********************************************************
//      SericeDiscovery Structs 
//***********************************************************

typedef struct primary_ServiceItem
{
  uint16 handle; 
  uint16 endHandle; 
  uint16 ServiceUUID; 
  
  struct primary_ServiceItem* next; 
  
}primary_ServiceItem;


typedef struct Chara_ServiceItem
{
  uint16 Handle;
  uint16 UUID;
  
  struct Chara_ServiceItem* next; 
  
}Chara_ServiceItem;

typedef struct Decs_ServiceItem
{
  uint16 Handle;
  uint16 UUID;
  
  struct Decs_ServiceItem* next; 
  
}Decs_ServiceItem;

//***********************************************************
//      Functions 
//***********************************************************

void ConnectionManger_Init( uint8 task_id);
uint16 ConnectionManger_ProcessEvent( uint8 task_id, uint16 events );

void Queue_addWrite(uint8* write, uint8 len, uint8* addr, uint16 handel, Callback call, ErrorCallback ecall);
void Queue_Scan(Scancallback call, ErrorCallback ecall);
void Queue_addServiceDiscovery(uint8* addr, Callback call ,ErrorCallback ecall, ScanType type, uint16 startHandle, uint16 endHandle);