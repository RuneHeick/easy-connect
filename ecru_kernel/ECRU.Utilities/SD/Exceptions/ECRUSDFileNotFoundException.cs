using System;

namespace ECRU.Utilities
{
    internal class ECRUSDFileNotFoundException : Exception
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