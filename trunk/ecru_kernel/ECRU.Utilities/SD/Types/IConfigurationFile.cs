using System;
using System.Collections;
using Microsoft.SPOT;

namespace ECRU.Utilities.SD.Types
{
    public interface IConfigurationFile : IFile
    {
         //incase expansion
    }

    public class ConfigurationFile : IConfigurationFile
    {
        private string _filePath = null;
        private string _fileName = null;
        private Hashtable _fileContent = null;

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath == null)
                {
                    _filePath = value;
                }
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == null)
                {
                    _fileName = value;
                }
            }
        }

        public Hashtable FileContent
        {
            get { return _fileContent; }
            set
            {
                if (_fileContent == null)
                {
                    _fileContent = value;
                }
            }
        }

    }
}
