#pragma once 
#include "OSAL.h"
#include "bcomdef.h"
#include "gatt.h"
#include "Uart.h"
#include "ConnectionManger.h"

void UartManager_Init();

void SendDeviceInfo(ScanResponse_t* item);

void SendDataCommand(EventQueueRWItem_t*);

void SendDisconnectedCommand(uint8* addr);