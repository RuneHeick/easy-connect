#pragma once 
#include "OSAL.h"

extern bool FileManager_HasLoadedImage;

extern void FileManager_Save(); 

extern void FileManager_Load(); 

extern void FileManager_Clear(); 

extern void ReadFromFlash(uint16 addr, uint8 count, uint8* buffer);

extern void WriteToFlash(uint16 addr, uint8 count, uint8* buffer);

extern void FileManager_UpdatePassCode();