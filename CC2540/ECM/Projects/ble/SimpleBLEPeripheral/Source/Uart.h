#pragma once
#include "hal_uart.h"
#include "OSAL.h"

/*********************************************************************
 * FUNCTIONS
 */

/*
 * Task Initialization for the BLE Application
 */
extern void Uart_Init( uint8 task_id );

/*
 * Task Event Processor for the BLE Application
 */
extern uint16 Uart_ProcessEvent( uint8 task_id, uint16 events );