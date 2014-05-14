#pragma once 
#include "OSAL.h"

void FileManager_Save(); 

void FileManager_Load(); 

void FileManager_Clear(); 

void ReadFromFlash(uint16 addr, uint8 count, uint8* buffer);

void WriteToFlash(uint16 addr, uint8 count, uint8* buffer);