#include "Uart.h"

#if !defined NPI_UART_PORT
#if ((defined HAL_UART_DMA) && (HAL_UART_DMA  == 1))
#define NPI_UART_PORT                  HAL_UART_PORT_0
#elif ((defined HAL_UART_DMA) && (HAL_UART_DMA  == 2))
#define NPI_UART_PORT                  HAL_UART_PORT_1
#else
#define NPI_UART_PORT                  HAL_UART_PORT_0
#endif
#endif

#if !defined( NPI_UART_FC )
#define NPI_UART_FC                    FALSE
#endif // !NPI_UART_FC

#define NPI_UART_FC_THRESHOLD          0
#define NPI_UART_RX_BUF_SIZE           128
#define NPI_UART_TX_BUF_SIZE           128
#define NPI_UART_IDLE_TIMEOUT          100
#define NPI_UART_INT_ENABLE            FALSE

#if !defined( NPI_UART_BR )
#define NPI_UART_BR                    HAL_UART_BR_115200
#endif // !NPI_UART_BR

#define EMPTY 0
#define START 2
#define SYNCWORD 0xEC
#define INITIAL_CRC 0xffff


static uint8 Uart_TaskID; 
static halUARTCfg_t uartConfig; 

static Buffer bufferRX; 

static Buffer bufferTX; 
static uint8 TransmitRetryCount; 

static void Uart_TransmitBuffer();

static void cSerialPacketParser( uint8 port, uint8 events );
static void Uart_ProcessOSALMsg( osal_event_hdr_t *pMsg );
static void ReadFromUart(uint8 port);
static unsigned short update_crc(unsigned short crc, char c);
static unsigned short CalcCrc(uint8* buffer, uint8 count);
static void Uart_HandelRequest();
static void Uart_ClearPendingResponse();


static CallBackFunction ReplyFunc;
static bool pendingResponse = false; 


static RequestBank Uart_Subscriptions[UART_MAX_SUBCRIPTIONS];

void Uart_Init( uint8 task_id )
{
  uint8 index; 
  Uart_TaskID = task_id;
  
  HalUARTInit();
  
  // configure UART
  uartConfig.configured           = TRUE;
  uartConfig.baudRate             = NPI_UART_BR;
  uartConfig.flowControl          = NPI_UART_FC;
  uartConfig.flowControlThreshold = NPI_UART_FC_THRESHOLD;
  uartConfig.rx.maxBufSize        = NPI_UART_RX_BUF_SIZE;
  uartConfig.tx.maxBufSize        = NPI_UART_TX_BUF_SIZE;
  uartConfig.idleTimeout          = NPI_UART_IDLE_TIMEOUT;
  uartConfig.intEnable            = NPI_UART_INT_ENABLE;
  uartConfig.callBackFunc         = (halUARTCBack_t)cSerialPacketParser;
  
  
  for(index=0;index<UART_MAX_SUBCRIPTIONS;index++)
  {
    Uart_Subscriptions[index].command = 0; 
    Uart_Subscriptions[index].id = 0; 
    
  }
  
  HalUARTOpen(HAL_UART_PORT_0, &uartConfig);
  
}


bool Uart_Subscribe(uint8 tarskId,uint8 Command)
{
  uint8 index;
  for(index=0;index<UART_MAX_SUBCRIPTIONS;index++)
  {
    if(Uart_Subscriptions[index].id == 0)
    {
      Uart_Subscriptions[index].command = Command; 
      Uart_Subscriptions[index].id = tarskId;
      return true; 
    }
  }
  return false;
}

bool Uart_Unsubscribe(uint8 tarskId,uint8 Command)
{
  uint8 index;
  for(index=0;index<UART_MAX_SUBCRIPTIONS;index++)
  {
    if(Uart_Subscriptions[index].id == tarskId && Uart_Subscriptions[index].command == Command)
    {
      Uart_Subscriptions[index].command = 0; 
      Uart_Subscriptions[index].id = 0; 
      return true; 
    }
  }
  return false;
}

PayloadBuffer Uart_getRXpayload()
{
  return (PayloadBuffer){&bufferRX.buffer[3],bufferRX.count-3-2 };  
}

/*
 * Task Event Processor 
 */

uint16 Uart_ProcessEvent( uint8 task_id, uint16 events )
{
  if ( events & SYS_EVENT_MSG )
  {
    uint8 *pMsg;
    
    if ( (pMsg = osal_msg_receive( Uart_TaskID )) != NULL )
    {
      Uart_ProcessOSALMsg( (osal_event_hdr_t *)pMsg );
      
      // Release the OSAL message
      VOID osal_msg_deallocate( pMsg );
    }
    
    // return unprocessed events
    return (events ^ SYS_EVENT_MSG);
  }
  else if ( events & UART_PACKET_EVENT )
  {
    unsigned short Crc = (((unsigned short)bufferRX.buffer[bufferRX.count-2])<<8)
      +((unsigned short)bufferRX.buffer[bufferRX.count-1]);
    
    if(Crc == CalcCrc(bufferRX.buffer,bufferRX.count-2))
    {
      if(bufferRX.buffer[1]&0x80) //reply
      {
        if(bufferTX.status==Waiting_For_Reply)
        {
          if(bufferTX.buffer[2]==bufferRX.buffer[2])// check if it is for this command 
          {
            osal_stop_timerEx(Uart_TaskID,UART_RETRANSMIT_EVENT);
            if(ReplyFunc!=NULL)
              ReplyFunc(SUCSSES); 
            bufferTX.status = Ready;
          }
        }
      }
      else // request 
      {
         Uart_HandelRequest();
      } 
    }
    
    return (events ^ UART_PACKET_EVENT);
  }
  
  
  
  else if ( events & UART_TRANSMITTING_EVENT )
  {
    Uart_TransmitBuffer();
    
    TransmitRetryCount = 0; 
    // Set timer for first periodic event
    osal_start_timerEx( Uart_TaskID, UART_RETRANSMIT_EVENT, RETRANSMIT_TIME );
    
    return (events ^ UART_TRANSMITTING_EVENT);
  }
  
  else if ( events & UART_ACK_TIMEOUT_EVENT )
  {
    Uart_Send_Response("\0",1);
    
    return (events ^ UART_ACK_TIMEOUT_EVENT);
  }
  
  
  else if ( events & UART_RETRANSMIT_EVENT )
  {
    if(bufferTX.status==Waiting_For_Reply)
    {
      if(TransmitRetryCount<RETRY_COUNT)
      {
        TransmitRetryCount++; 
        Uart_TransmitBuffer();
        osal_start_timerEx( Uart_TaskID, UART_RETRANSMIT_EVENT, RETRANSMIT_TIME );
      }
      else
      {
        if(ReplyFunc!=NULL)
           ReplyFunc(TIME_OUT); 
        bufferTX.status = Ready; 
      }
    }
    return (events ^ UART_RETRANSMIT_EVENT);
  }
  
  
  // Discard unknown events
  return 0;
}

static void Uart_TransmitBuffer()
{
  HalUARTWrite(NPI_UART_PORT,bufferTX.buffer,bufferTX.count);
}

static void Uart_HandelRequest()
{
  uint8 index; 
  pendingResponse = true; 
  for(index=0;index<UART_MAX_SUBCRIPTIONS;index++)
  {
    if(Uart_Subscriptions[index].command == bufferRX.buffer[2])
    { 
      RqMsg* msgPtr = (RqMsg*)osal_msg_allocate(sizeof(RqMsg));
      msgPtr->info.event = UART_RQ;
      msgPtr->command = bufferRX.buffer[2];
      
      osal_msg_send(Uart_Subscriptions[index].id , (uint8*)msgPtr);
      osal_start_timerEx( Uart_TaskID, UART_ACK_TIMEOUT_EVENT, UART_ACK_TIMEOUT_TIME); 
      return;
    }
  }
  osal_start_timerEx( Uart_TaskID, UART_ACK_TIMEOUT_EVENT, UART_ACK_TIMEOUT_TIME); 
  bufferRX.count = 0; 
  bufferRX.status = Ready; 
}

bool Uart_Send_Response(uint8* buffer, uint8 len)
{
  if(bufferTX.status == Ready && pendingResponse)
  {
    unsigned short crc;
 
    Uart_ClearPendingResponse();
    if(&bufferTX.buffer[3]!=buffer && len>0)
      osal_memcpy(&bufferTX.buffer[3],buffer,len);  
    
    bufferTX.buffer[0] = 0xEC; 
    bufferTX.buffer[1] = 0x80 | len+2+2; 
    bufferTX.buffer[2] = bufferRX.buffer[2];
    bufferTX.count = 3+2+len;
    
    crc = CalcCrc(bufferTX.buffer,len+3);
    
    bufferTX.buffer[3+len] =  (uint8)(crc>>8); 
    bufferTX.buffer[3+len+1] =(uint8)(crc);
    
    Uart_TransmitBuffer();
    bufferRX.count = 0; 
    bufferRX.status = Ready;
    
    return true; 
  }
  
  return false; 

}

static void Uart_ClearPendingResponse()
{
  pendingResponse = false;
  osal_stop_timerEx( Uart_TaskID, UART_ACK_TIMEOUT_EVENT);
}

uint8* Uart_TxGetDataBuffer()
{
  if(bufferTX.status == Ready)
  {
    return &bufferTX.buffer[3];
  }
  return NULL;
}
  
bool Uart_Send(uint8* buffer, uint8 len, uint8 command, CallBackFunction func)
{
  if(bufferTX.status == Ready)
  {
    unsigned short crc;
      
    bufferTX.status = Transmitting;
    ReplyFunc = func; 
    if(&bufferTX.buffer[3]!=buffer && len>0)
      osal_memcpy(&bufferTX.buffer[3],buffer,len); 
    
    
    bufferTX.buffer[0] = 0xEC; 
    bufferTX.buffer[1] = len+2+2; 
    bufferTX.buffer[2] = command;
    bufferTX.count = 3+2+len;
    
    crc = CalcCrc(bufferTX.buffer,len+3);
    
    bufferTX.buffer[3+len] =  (uint8)(crc>>8); 
    bufferTX.buffer[3+len+1] =(uint8)(crc); 
    
    osal_set_event(Uart_TaskID,UART_TRANSMITTING_EVENT);
    
    return true; 
  }
  
  return false; 
}

static void Uart_ProcessOSALMsg( osal_event_hdr_t *pMsg )
{
  switch ( pMsg->event )
  {
  default:
    // do nothing
    break;
  }
}

/*
HAL_UART_RX_FULL         0x01
HAL_UART_RX_ABOUT_FULL   0x02
HAL_UART_RX_TIMEOUT      0x04
HAL_UART_TX_FULL         0x08
HAL_UART_TX_EMPTY        0x10
*/
static void cSerialPacketParser( uint8 port, uint8 events )
{
  switch(events)
  {
    case HAL_UART_RX_FULL:
    case HAL_UART_RX_ABOUT_FULL:
    case HAL_UART_RX_TIMEOUT:
      ReadFromUart(port);
      break; 
    case HAL_UART_TX_EMPTY:
        if(bufferTX.status == Transmitting)
          bufferTX.status = Waiting_For_Reply;
        break; 
    
  }
}

static void ReadFromUart(uint8 port)
{
  uint8 len = Hal_UART_RxBufLen(port);
  
  if(bufferRX.status != Has_Packet)
  {
    if(bufferRX.count>=START)
    {
      uint8 packetlen  = (bufferRX.buffer[1]&0x7F)+1; 
      uint8 restpacketlen  = packetlen-bufferRX.count; 
      
      if(restpacketlen <= 0)
      {
        bufferRX.count = 0; 
        bufferRX.status = Ready; 
      }
      
      len = len<=restpacketlen ? len : restpacketlen;
      
      HalUARTRead(port,&bufferRX.buffer[bufferRX.count],len);
      bufferRX.count += len;
      
      if(bufferRX.count==packetlen)
      {
        bufferRX.status = Has_Packet; 
        osal_set_event(Uart_TaskID,UART_PACKET_EVENT);
      }
      
    }
    else if(len>1)
    {
      HalUARTRead(port,bufferRX.buffer,START);
      bufferRX.count = START; 
      bufferRX.status = Receiving;
      
      if(bufferRX.buffer[0]!=SYNCWORD)
      {
         bufferRX.count = 0;
         bufferRX.status = Ready; 
      }
    }
  }
  
}

static unsigned short update_crc(unsigned short crc, char c) 
{
    char i;

    crc ^= (unsigned short)c<<8;
    for (i=0; i<8; i++) {
        if (crc & 0x8000) crc = (crc<<1)^0x1021;
        else crc <<=1;
    }
    return crc;
}

static unsigned short CalcCrc(uint8* buffer, uint8 count)
{
uint8 byteCount; 
unsigned short Crc = INITIAL_CRC;

    for (byteCount=0; byteCount<count; byteCount++) {
        Crc = update_crc(Crc, buffer[byteCount] );
    }
return Crc;
    
}