using System.Collections;

namespace ECRU.Utilities
{
    public interface IConfigurationFile : IFile
    {
        //incase expansion
    }

    public class ConfigurationFile : IConfigurationFile
    {
        private Hashtable _fileContent;
        private string _fileName;
        private string _filePath;

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