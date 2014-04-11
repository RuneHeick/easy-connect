#pragma once 
#include "OSAL.h"
#include "bcomdef.h"
#include "gatt.h"
#include "Uart.h"
#include "ConnectionManger.h"

extern void UartManager_Init();

extern void SendDeviceInfo(ScanResponse_t* item);

extern void SendDataCommand(EventQueueRWItem_t*);

extern void SendDisconnectedCommand(uint8* addr);