#pragma once

#define MAX_HW_SUPPORTED_DEVICES 3


typedef void (*Callback)(uint8* buffer, uint8 length);

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
  
}EventType_t;

typedef struct EventQueueItemBase_t 
{
  uint8 addr[B_ADDR_LEN];
  EventType_t action;
  
  struct EventQueueItemBase_t* next;
  
}EventQueueItemBase_t;


typedef struct EventQueueRWItem_t 
{
  EventQueueItemBase_t base;
  
  uint16 handel;
  uint8* data;
  uint8 length; 
  
  Callback callback;
  
}EventQueueRWItem_t;