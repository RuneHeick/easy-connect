using System.Collections;

namespace ECRU.Utilities
{
    public interface IFile
    {
        string FilePath { get; }
        string FileName { get; }
        Hashtable FileContent { get; }
    }
}