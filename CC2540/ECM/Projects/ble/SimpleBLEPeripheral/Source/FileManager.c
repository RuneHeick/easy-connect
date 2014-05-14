#include "FileManager.h"
#include "hal_flash.h"
#include "SmartCommandsManger.h"
#include "devinfoservice.h"
#include "InitState.h"
#include "ResetManager.h"
#include "ECConnect.h"

#define ENDPAGE 68
#define MEMORYSIZE 4000

#define MEMORYPAGES  2 //(((uint32)MEMORYSIZE)/((uint32)HAL_FLASH_PAGE_SIZE))+1
#define INFO_ADDRESS (((uint32)ENDPAGE* (uint32)HAL_FLASH_PAGE_SIZE) - MEMORYSIZE)
#define DATASTART_ADDRESS INFO_ADDRESS+4 //one address is 4 byte. This is the write address

//file Command values 
enum 
{
  PASSCODE_CMD = 0xA2,
  DEVICENAME_CMD = 0x12,
  SERIAL_CMD = 0x13,
  MODEL_CMD = 0x11,
  MAINI_CMD = 0x10,
  SERVICE_CMD = 0x20,
  SERVICEDEC_CMD = 0x14,
  CHAREVAL_CMD = 0xC0,
  CHAREDEC_CMD = 0xC1,
  CHAREFOR_CMD = 0xC2,
  CHAREGUI_CMD = 0xC3,
  CHARERANGE_CMD = 0xC4,
  CHARESUB_CMD = 0xC5
};

typedef struct TempCharacteristic  
{
  uint8  valueSize; 
  uint8  premission; 
  uint8 gpio; 
  
  GenericValue userDescription;
  GUIPresentationFormat guiPresentationFormat; 
  PresentationFormat typePresentationFormat; 
  
  GenericValue range; 
  Subscription subscribtion; 
  
}TempCharacteristic;

static TempCharacteristic tempChare; 

//Local Address 
static uint16 currentAddress = 0;

static void setFileState(uint16 len)
{
  uint8 data[4] = {(uint8)len%2,(uint8)(len>>8),(uint8)len, 0}; 
  data[3] = (uint8)(data[0] + data[1] + data[2]); 
  HalFlashWrite(INFO_ADDRESS/4,data,1); 
}

static uint16 getFileState()
{
  uint8 data[4];
  uint16 offset = (INFO_ADDRESS%HAL_FLASH_PAGE_SIZE);
  HalFlashRead((INFO_ADDRESS/HAL_FLASH_PAGE_SIZE),offset,data,4);
  uint16 len = (data[1]<<8)+data[2];
  uint8 check = data[0]+data[1]+data[2];
  if(data[0] == len%2 && check == data[3])
  {
    return len; 
  }
  
  return 0; 
}

static void ClearAllRam()
{
  uint8 i = ENDPAGE - MEMORYPAGES;
    
  for(; i <= ENDPAGE ; i++)
  {
    HalFlashErase(i);
    while(FCTL & 0x80);  // Wait until is done.
  }
}

void ReadFromFlash(uint16 addr, uint8 count, uint8* buffer)
{
  uint32 Addres =  addr + DATASTART_ADDRESS;
  uint8 bufferIndex = 0; 
  uint8 page = (Addres/HAL_FLASH_PAGE_SIZE);
  uint16 offset = (Addres%HAL_FLASH_PAGE_SIZE);
  
  // check for bank change  
  if(page % HAL_FLASH_PAGE_PER_BANK == HAL_FLASH_PAGE_PER_BANK-1 && count+offset > HAL_FLASH_PAGE_SIZE)
  {
      uint8 bankOneCount = HAL_FLASH_PAGE_SIZE - offset;
      count -= bankOneCount;
      
      HalFlashRead(page,offset,buffer,bankOneCount);
      page ++;
      offset = 0; 
      bufferIndex = bankOneCount;
      
  }
  
  HalFlashRead(page,offset,&buffer[bufferIndex],count);
}

void WriteToFlash(uint16 addr, uint8 count, uint8* buffer)
{
  uint8 index = 0; 
  uint32 Addres =  addr + DATASTART_ADDRESS;
  uint16 writeStartaddr = (Addres/4); 
  uint8 startFrac = Addres%4;
  
  uint16 writeEndaddr = ((Addres+count)/4); 
  uint8 EndFrac = ((Addres+count)%4);
  
  uint8 inBetween =  (writeEndaddr-writeStartaddr)-1;
  if(writeEndaddr == writeStartaddr) inBetween = 0; 
  
  uint8 tempBuf[HAL_FLASH_WORD_SIZE];
  osal_memcpy(&tempBuf[startFrac],buffer,HAL_FLASH_WORD_SIZE-startFrac); 
  index += HAL_FLASH_WORD_SIZE-startFrac; 
  
  if(startFrac)
  {
    ReadFromFlash(addr-startFrac,startFrac,tempBuf); 
  }
  
  if(writeEndaddr == writeStartaddr)
  {
    ReadFromFlash(addr+count,HAL_FLASH_WORD_SIZE- EndFrac,&tempBuf[EndFrac]); 
  }
  
  HalFlashWrite(writeStartaddr,tempBuf,1); 
  
  if(inBetween != 0)
  {
    HalFlashWrite(writeStartaddr+1,&buffer[index],inBetween); 
  }
  
  if(writeEndaddr != writeStartaddr && EndFrac != 0)
  {
    osal_memcpy(tempBuf,&buffer[count-EndFrac],EndFrac); 
    ReadFromFlash(addr+count,HAL_FLASH_WORD_SIZE-EndFrac,&tempBuf[EndFrac]); 
    HalFlashWrite(writeEndaddr,tempBuf,1); 
  }
  
}




void FileManager_Save()
{
  ClearAllRam();
    
  GenericValue serial;
  GenericValue model;
  GenericValue mani;
  
  DevInfo_GetParameter(DEVINFO_SERIAL_NUMBER, &serial );
  DevInfo_GetParameter(DEVINFO_MODEL_NUMBER, &model );
  DevInfo_GetParameter(DEVINFO_MANUFACTURER_NAME, &mani);
  
  uint8 byte[2];
  if(mani.status == READY)
  {
    byte[0] = MAINI_CMD;
    byte[1] = mani.size;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2; 
    WriteToFlash(currentAddress,mani.size,mani.pValue);
    currentAddress += mani.size;
  }
  
  if(model.status == READY)
  {
    byte[0] = MODEL_CMD;
    byte[1] = model.size;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2; 
    WriteToFlash(currentAddress,model.size,model.pValue);
    currentAddress += model.size;
  }

    byte[0] = PASSCODE_CMD;
    byte[1] = SYSID_SIZE;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2;
    uint8* id = GetSetSystemID(); 
    WriteToFlash(currentAddress,SYSID_SIZE,id);
    currentAddress += SYSID_SIZE;

  
  pBuffer_t nameBuf =  GAPManget_GetName();
  if(nameBuf.count != 0)
  {
    byte[0] = DEVICENAME_CMD;
    byte[1] = nameBuf.count;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2; 
    WriteToFlash(currentAddress,nameBuf.count,nameBuf.pValue);
    currentAddress += nameBuf.count;
  }
  
  if(serial.status == READY)
  {
    byte[0] = SERIAL_CMD;
    byte[1] = serial.size;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2; 
    WriteToFlash(currentAddress,serial.size,serial.pValue);
    currentAddress += serial.size;
  }
  
  
  
  for(uint8 i = 0; i < SmartCommandServices_Count ; i++)
  {
    SmartService* service = SmartCommandServices[i];
    
    byte[0] = SERVICE_CMD;
    byte[1] = 0;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2;
    
    byte[0] = SERVICEDEC_CMD;
    byte[1] = service->description.size;
    WriteToFlash(currentAddress,2,byte);
    currentAddress += 2;
    WriteToFlash(currentAddress,service->description.size,service->description.pValue);
    currentAddress += service->description.size;
    
    GenericCharacteristic* temp = service->first;
    
    while( temp != NULL)
    {
      byte[0] = CHAREVAL_CMD;
      byte[1] = 3;
      WriteToFlash(currentAddress,2,byte);
      currentAddress += 2;
      uint8 value[3] = { temp->value.size, temp->premission, temp->gpio };
      WriteToFlash(currentAddress,3,value);
      currentAddress += 3;
      
      byte[0] = CHAREDEC_CMD;
      byte[1] = temp->userDescription.size;
      WriteToFlash(currentAddress,2,byte);
      currentAddress += 2;
      WriteToFlash(currentAddress,temp->userDescription.size,temp->userDescription.pValue);
      currentAddress += temp->userDescription.size;
      
      byte[0] = CHAREFOR_CMD;
      byte[1] = sizeof(temp->typePresentationFormat);
      WriteToFlash(currentAddress,2,byte);
      currentAddress += 2;
      WriteToFlash(currentAddress,sizeof(temp->typePresentationFormat),(uint8*)&temp->typePresentationFormat);
      currentAddress += sizeof(temp->typePresentationFormat);
      
      byte[0] = CHAREGUI_CMD;
      byte[1] = sizeof(temp->guiPresentationFormat);
      WriteToFlash(currentAddress,2,byte);
      currentAddress += 2;
      WriteToFlash(currentAddress,sizeof(temp->guiPresentationFormat),(uint8*)&temp->guiPresentationFormat);
      currentAddress += sizeof(temp->guiPresentationFormat);
      
      if(temp->range.status == READY)
      {
        byte[0] = CHARERANGE_CMD;
        byte[1] = temp->range.size;
        WriteToFlash(currentAddress,2,byte);
        currentAddress += 2;
        WriteToFlash(currentAddress, temp->range.size , temp->range.pValue  );
        currentAddress += temp->range.size;
      }
      

        byte[0] = CHARESUB_CMD;
        byte[1] = sizeof(temp->subscribtion);
        WriteToFlash(currentAddress,2,byte);
        currentAddress += 2;
        WriteToFlash(currentAddress, sizeof(temp->subscribtion) , (uint8*)&temp->subscribtion  );
        currentAddress += sizeof(temp->subscribtion);
      
      temp = temp->nextitem;
    }
    
  }
  
  setFileState(currentAddress);
  
}

static bool LoadDataObject(uint8 command, uint8 len,uint8* data)
{
  switch(command)
  {
    case PASSCODE_CMD: 
    {
      uint8* id = GetSetSystemID(); 
      osal_memcmp(id, data, len); 
    }
      break;
    case DEVICENAME_CMD:
      GAPManget_SetupName(data,len);
      break;
    case SERIAL_CMD:
      DevInfo_SetParameter(DEVINFO_SERIAL_NUMBER,len,data);
      break;
    case MODEL_CMD:
      DevInfo_SetParameter(DEVINFO_MODEL_NUMBER,len,data);
      break;
    case MAINI_CMD:
      DevInfo_SetParameter(DEVINFO_MANUFACTURER_NAME,len,data);
      break;
    case SERVICE_CMD:
      break;
    case SERVICEDEC_CMD:
      SmartCommandsManger_CreateService(data,len);
      break;
    case CHAREVAL_CMD:
      osal_memset((uint8*)&tempChare,0,sizeof(TempCharacteristic)); // set to init state. 
      tempChare.valueSize = data[0]; 
      tempChare.premission = data[1];
      tempChare.gpio = data[2];
      break;
    case CHAREDEC_CMD:
      GenericValue_SetValue(&tempChare.userDescription,data,len); 
      break;
    case CHAREFOR_CMD:
      osal_memcmp(&tempChare.typePresentationFormat,data,len); 
      break;
    case CHAREGUI_CMD:
      osal_memcmp(&tempChare.guiPresentationFormat,data,len); 
      break;
    case CHARERANGE_CMD:+
      GenericValue_SetValue(&tempChare.range,data,len);
      break;
    case CHARESUB_CMD:
      osal_memcmp(&tempChare.subscribtion,data,len);
      {
        // add
        SmartCommandsManger_addCharacteristic(tempChare.valueSize,tempChare.userDescription.pValue,tempChare.userDescription.size,tempChare.guiPresentationFormat,tempChare.typePresentationFormat,tempChare.subscribtion, tempChare.premission, tempChare.gpio);
        if(tempChare.range.status == READY)
        {
          SmartCommandsManger_addRange(tempChare.range.pValue,tempChare.range.size);
          GenericValue_DeleteValue(&tempChare.range);
        }
        
        //Clean Up
        GenericValue_DeleteValue(&tempChare.userDescription);
      }
      break;
    default: 
      return false;
  }
  
  return true; 
}


void FileManager_Load()
{
  uint16 len = getFileState();
  if(len != 0)
  {
    uint16 addr = 0; 
    
    while(addr<len)
    {
      uint8 commandData[2]; 
      ReadFromFlash(addr,2,commandData);
      
      uint8 command = commandData[0]; 
      uint8 datalen = commandData[1]; 
      
      addr+= 2; 
      
       uint8* data = osal_mem_alloc(datalen); 
       if(data)
       {
          ReadFromFlash(addr,datalen, data);
          bool stat = LoadDataObject(command,datalen,data); 
          osal_mem_free(data);
          addr += datalen;
          
       }
    
    }
  }
}

void FileManager_Clear()
{
  ClearAllRam();
  setFileState(0);
}


#ifdef TEST

#define UFLEN 20
#define START 1000
void FileManager_Test(uint8 tarskID)
{
  
  FileManager_Clear(); 
  
  uint8 uffer[UFLEN]; 
  
  WriteToFlash(START,1, "D");
  WriteToFlash(START+UFLEN-1 ,1, "D");
  
  ReadFromFlash(START,UFLEN,uffer);
  
  osal_memset(&uffer[1],0xEC,UFLEN-2); 
 
  WriteToFlash(START+1 ,UFLEN-2, &uffer[1]);
  
  osal_memset(&uffer[1],0x00,UFLEN-2); 
  
  ReadFromFlash(START,UFLEN,uffer);
  
  
  /*
  uint8 datawrite[]={0xED,0xED,0x3d,0x3d,0xEC,0xDA}; 

  uint8 dataread[16];

  HalFlashRead(1,0, dataread, 16);

  HalFlashErase(1);

  while(FCTL & 0x80);

  HalFlashRead(1,0, dataread, 16);

  HalFlashWrite(0x0200, datawrite, 1);  // actual address is 4*0x4600=0x11800.

  HalFlashRead(1,0, dataread, 16);
  
  */
}
#endif 