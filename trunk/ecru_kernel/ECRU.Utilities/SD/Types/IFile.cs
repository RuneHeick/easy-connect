using System.Collections;

namespace ECRU.Utilities.SD.Types
{
    public interface IFile
    {
        string FilePath { get; }
        string FileName { get; }
        Hashtable FileContent { get; }
    }
}