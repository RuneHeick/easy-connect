using System;
using Microsoft.SPOT;

namespace ECRU.Utilities.SD.Exceptions
{
    class ECRUSDFileNotFoundException : Exception
    {
        public ECRUSDFileNotFoundException(string filePath, string fileName)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

    }
}