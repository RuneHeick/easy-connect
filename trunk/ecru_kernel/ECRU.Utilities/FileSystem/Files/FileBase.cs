﻿namespace ECRU.Utilities
{
    public class FileBase
    {
        public virtual string Path { get; set; }

        public virtual byte[] Data { get; set; }

        public CloseFunction Closefunc { get; set; }

        public void Close()
        {
            if (Closefunc != null)
                Closefunc(this);
            Closefunc = null;
        }
    }

    public delegate void CloseFunction(FileBase file);
}