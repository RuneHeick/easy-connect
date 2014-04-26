#pragma once 
#include "OSAL.h"
#include "bcomdef.h"
#include "gatt.h"
#include "Uart.h"
#include "ConnectionManger.h"

 enum CommandType
 {
        AddDeviceEvent = 0x10,
        DataEvent = 0x50,
        DeviceEvent =0x70,
        DisconnectEvent = 0x60,
        ReadEvent = 0x30,
        WriteEvent = 0x40,
        ServiceEvent = 0x20,
        SystemInfo = 0x80, 
        DiscoverEvent = 0x90,
        NameEvent = 0xA1,
        PassCodeEvent = 0xA2,
        
        AddrRqEvent = 0xE0,
        Reset = 0xF0,
 };

void UartManager_Init(uint8 tarskId, uint16 eventhandle);

extern void SendDeviceInfo(ScanResponse_t* item);

extern void SendDataCommand(EventQueueRWItem_t*);

extern void SendDisconnectedCommand(uint8* addr);

extern void SendName(); 

extern void SendPassCode(); 

extern void SendResetCommand();

extern void SendMac();

extern void UartManager_HandelUartPacket(osal_event_hdr_t * msg);

extern void SendServicesCommand(EventQueueServiceDirItem_t* data);

extern void UartManager_DequeueEvent();