#include "FileManager.h"
#include "hal_flash.h"
#include "SmartCommandsManger.h"
#include "devinfoservice.h"
#include "InitState.h"
#include "ResetManager.h"
#include "ECConnect.h"
#include "osal_snv.h"

#define ENDPAGE 68
#define MEMORYSIZE 4000

#define MEMORYPAGES  2 //(((uint32)MEMORYSIZE)/((uint32)HAL_FLASH_PAGE_SIZE))+1
#define INFO_ADDRESS (((uint32)ENDPAGE* (uint32)HAL_FLASH_PAGE_SIZE) - MEMORYSIZE)
#define DATASTART_ADDRESS INFO_ADDRESS+4 //one address is 4 byte. This is the write address

#define START_FALSHID 0x80
#define END_FALSHID 0xFE

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

//to FlashBuilder 
static uint8 BuilderID = 0;
static uint8 PrepLen = 0; 
static GenericValue BuliderContainor = { .status = NOT_INIT, .pValue = NULL, .size = 0  }; 

//Local Address 
static uint16 currentAddress = 0;


bool FileManager_HasLoadedImage = false; 
static uint8 PasscodeID = 0; 

static void setFileState(uint8 EndID)
{
  uint8 data[4] = {(uint8)EndID%2,EndID,!EndID, 0}; 
  data[3] = (uint8)(data[0] + data[1] + data[2]); 
  osal_snv_write(START_FALSHID,4,data);
}

static void InitFlashBuilder()
{
  GenericValue_CreateContainer(&BuliderContainor, 255);
}

static void WriteInFlashWithSNV()
{
  if(PrepLen != 0)
  {
    osal_snv_write(BuilderID,PrepLen,BuliderContainor.pValue);
    PrepLen = 0; 
  }
}

static void DisposeFlashBuilder()
{
    WriteInFlashWithSNV();
    GenericValue_DeleteValue(&BuliderContainor);
}



static void FlashBuilder(uint8 FlashID, uint8* buffer, uint8 len )
{
  if(BuilderID != FlashID && BuilderID != 0)
  {
    WriteInFlashWithSNV(); 
  }
  
    
  BuilderID = FlashID; 
  if(BuliderContainor.status == READY && PrepLen+len < BuliderContainor.size)
  {
    osal_memcpy(&BuliderContainor.pValue[PrepLen],buffer,len);
    PrepLen += len;
  }
  
}


static uint8 getFileState()
{
  uint8 data[4];
  osal_snv_read(START_FALSHID,4,data);
  uint16 End = data[1];
  uint8 check = data[0]+data[1]+data[2];
  if(data[0] == End%2 && check == data[3] && !data[1] == data[2])
  {
    return End; 
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

void FileManager_UpdatePassCode()
{
  if(PasscodeID != 0)
  {
    uint8 byte[2+SYSID_SIZE] = {PASSCODE_CMD, SYSID_SIZE};
    uint8* id = GetSetSystemID(); 
    osal_memcpy(&byte[2],id,SYSID_SIZE);
    osal_snv_write(PasscodeID,2+SYSID_SIZE,byte);
  }
}


void FileManager_Save()
{
  uint8 ID = START_FALSHID+1;
   
  GenericValue serial;
  GenericValue model;
  GenericValue mani;
  
  DevInfo_GetParameter(DEVINFO_SERIAL_NUMBER, &serial );
  DevInfo_GetParameter(DEVINFO_MODEL_NUMBER, &model );
  DevInfo_GetParameter(DEVINFO_MANUFACTURER_NAME, &mani);
  
  InitFlashBuilder();
  
  uint8 byte[2];
  if(mani.status == READY)
  {
    byte[0] = MAINI_CMD;
    byte[1] = mani.size;
    FlashBuilder(ID,byte,2);
    currentAddress += 2; 
    FlashBuilder(ID,mani.pValue,mani.size);
    currentAddress += mani.size;
    ID++;
  }
  
  
  if(model.status == READY)
  {
    byte[0] = MODEL_CMD;
    byte[1] = model.size;
    FlashBuilder(ID,byte,2);
    currentAddress += 2; 
    FlashBuilder(ID,model.pValue,model.size);
    currentAddress += model.size;
    ID++;
  }

    byte[0] = PASSCODE_CMD;
    byte[1] = SYSID_SIZE;
    FlashBuilder(ID,byte,2);
    currentAddress += 2;
    PasscodeID = ID; 
    uint8* id = GetSetSystemID(); 
    FlashBuilder(ID,id,SYSID_SIZE);
    currentAddress += SYSID_SIZE;
    ID++;

  
  pBuffer_t nameBuf =  GAPManget_GetName();
  if(nameBuf.count != 0)
  {
    byte[0] = DEVICENAME_CMD;
    byte[1] = nameBuf.count;
    FlashBuilder(ID,byte,2);
    currentAddress += 2; 
    FlashBuilder(ID,nameBuf.pValue,nameBuf.count);
    currentAddress += nameBuf.count;
    ID++;
  }
  
  if(serial.status == READY)
  {
    byte[0] = SERIAL_CMD;
    byte[1] = serial.size;
    FlashBuilder(ID,byte,2);
    currentAddress += 2; 
    FlashBuilder(ID,serial.pValue,serial.size);
    currentAddress += serial.size;
    ID++;
  }
  
  
  
  for(uint8 i = 0; i < SmartCommandServices_Count ; i++)
  {
    SmartService* service = SmartCommandServices[i];
    
    byte[0] = SERVICE_CMD;
    byte[1] = 0;
    FlashBuilder(ID,byte,2);
    currentAddress += 2;
    ID++;
    
    byte[0] = SERVICEDEC_CMD;
    byte[1] = service->description.size;
    FlashBuilder(ID,byte,2);
    currentAddress += 2;
    FlashBuilder(ID,service->description.pValue,service->description.size);
    currentAddress += service->description.size;
    ID++;
    
    GenericCharacteristic* temp = service->first;
    
    while( temp != NULL)
    {
      byte[0] = CHAREVAL_CMD;
      byte[1] = 3;
      FlashBuilder(ID,byte,2);
      currentAddress += 2;
      uint8 value[3] = { temp->value.size, temp->premission, temp->gpio };
      FlashBuilder(ID,value,3);
      currentAddress += 3;
      ID++;
      
      byte[0] = CHAREDEC_CMD;
      byte[1] = temp->userDescription.size;
      FlashBuilder(ID,byte,2);
      currentAddress += 2;
      FlashBuilder(ID,temp->userDescription.pValue,temp->userDescription.size);
      currentAddress += temp->userDescription.size;
      ID++;
      
      byte[0] = CHAREFOR_CMD;
      byte[1] = sizeof(temp->typePresentationFormat);
      FlashBuilder(ID,byte,2);
      currentAddress += 2;
      FlashBuilder(ID,(uint8*)&temp->typePresentationFormat,sizeof(temp->typePresentationFormat));
      currentAddress += sizeof(temp->typePresentationFormat);
      ID++;
      
      byte[0] = CHAREGUI_CMD;
      byte[1] = sizeof(temp->guiPresentationFormat);
      FlashBuilder(ID,byte,2);
      currentAddress += 2;
      FlashBuilder(ID,(uint8*)&temp->guiPresentationFormat,sizeof(temp->guiPresentationFormat));
      currentAddress += sizeof(temp->guiPresentationFormat);
      ID++;
      
      if(temp->range.status == READY)
      {
        byte[0] = CHARERANGE_CMD;
        byte[1] = temp->range.size;
        FlashBuilder(ID,byte,2);
        currentAddress += 2;
        FlashBuilder(ID, temp->range.pValue ,temp->range.size );
        currentAddress += temp->range.size;
        ID++;
      }
      

        byte[0] = CHARESUB_CMD;
        byte[1] = sizeof(temp->subscribtion);
        FlashBuilder(ID,byte,2);
        currentAddress += 2;
        FlashBuilder(ID, (uint8*)&temp->subscribtion ,sizeof(temp->subscribtion) );
        currentAddress += sizeof(temp->subscribtion);
        ID++;
      
      temp = temp->nextitem;
    }
    
  }
  DisposeFlashBuilder();
  ID--;
  setFileState(ID);
  
}

static bool LoadDataObject(uint8 command, uint8 len,uint8* data)
{
  switch(command)
  {
    case PASSCODE_CMD: 
    {
      uint8* id = GetSetSystemID(); 
      osal_memcpy(id, data, len); 
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
  uint16 EndID = getFileState();
  if(EndID != 0)
  {
    uint16 ID = START_FALSHID+1; 
    
    while(EndID >= ID)
    {
      uint8 commandData[2]; 
      osal_snv_read(ID,2,commandData);
      
      uint8 command = commandData[0]; 
      uint8 datalen = commandData[1]; 
      
      
       uint8* data = osal_mem_alloc(datalen+2); 
       if(data)
       {
          osal_snv_read(ID,datalen+2,data);
          bool stat = LoadDataObject(command,datalen,&data[2]); 
          
          if(command == PASSCODE_CMD)
          {
            PasscodeID = ID; 
          }
          
          osal_mem_free(data);
          ID ++;
          
       }
    
    }
    FileManager_HasLoadedImage = true; 
  }
}

void FileManager_Clear()
{
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