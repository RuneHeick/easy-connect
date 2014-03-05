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
#define NPI_UART_IDLE_TIMEOUT          6
#define NPI_UART_INT_ENABLE            FALSE

#if !defined( NPI_UART_BR )
#define NPI_UART_BR                    HAL_UART_BR_115200
#endif // !NPI_UART_BR


static uint8 Uart_TaskID; 
static halUARTCfg_t uartConfig; 

static uint8 UART_buffer[128];
static uint8 UART_bufferCount = 0; 
static uint8 UART_missingCount = 0; 

static void cSerialPacketParser( uint8 port, uint8 events );
static void Uart_ProcessOSALMsg( osal_event_hdr_t *pMsg );

void Uart_Init( uint8 task_id )
{
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
  
  HalUARTOpen(HAL_UART_PORT_0, &uartConfig);
  
}

/*
 * Task Event Processor for the BLE Application
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

static void cSerialPacketParser( uint8 port, uint8 events )
{
  uint8 len = Hal_UART_RxBufLen(0);
  HalUARTRead(0,UART_buffer,len);
}