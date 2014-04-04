using System;
using System.Collections;
using Microsoft.SPOT;

namespace ECRU.Utilities.SD.Types
{
    public interface IFile
    {
        string FilePath { get; }
        string FileName { get; }
        Hashtable FileContent { get; }
    }
}
