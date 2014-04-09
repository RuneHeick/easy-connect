#pragma once
#include "hal_uart.h"
#include "OSAL.h"

#define UART_PACKET_EVENT 0x0001
#define UART_TRANSMITTING_EVENT 0x0002
#define UART_RETRANSMIT_EVENT 0x0004
#define UART_ACK_TIMEOUT_EVENT 0x0008

#define RETRANSMIT_TIME 100
#define UART_ACK_TIMEOUT_TIME   (uint32)RETRANSMIT_TIME*0.9

#define RETRY_COUNT 3
#define UART_MAX_SUBCRIPTIONS 10
#define UART_BUFFER_SIZE 128


/*****************Command types***************************/

#define COMMAND_DEVICENAME      0x11
#define COMMAND_MANIFACTURE     0x12
#define COMMAND_MODELNR         0x13
#define COMMAND_SERIALNR        0x14
#define COMMAND_SMARTFUNCTION   0x15
#define COMMAND_GENRICVALUE     0x16
#define COMMAND_REANGES         0x17

#define UART_RQ 0x0005




typedef enum 
{
  Ready,
  Receiving,
  Has_Packet,
  Transmitting,
  Waiting_For_Reply
}BufferStatus; 

typedef struct
{
  uint8 buffer[UART_BUFFER_SIZE];
  uint8 count; 
  BufferStatus status;
  
}Buffer;

typedef enum 
{
  SUCSSES, 
  TIME_OUT
}TXStatus;

typedef void (*CallBackFunction)(TXStatus);

typedef void (*RequestHandler)(uint8);

typedef struct 
{
  uint8 command;
  uint8 id; 
}RequestBank; 

typedef struct
{
  uint8* bufferPtr; 
  uint8 count; 
}PayloadBuffer;

typedef struct 
{
  osal_event_hdr_t info; 
  uint8 command; 
}RqMsg; 


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

extern bool Uart_Send(uint8* buffer, uint8 len, uint8 command, CallBackFunction func);

extern bool Uart_Send_Response(uint8* buffer, uint8 len);

bool Uart_Subscribe(uint8 tarskId,uint8 command);

bool Uart_Unsubscribe(uint8 tarskId,uint8 command);

PayloadBuffer Uart_getRXpayload();

uint8* Uart_TxGetDataBuffer();