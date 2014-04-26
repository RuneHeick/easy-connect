using System;
using Microsoft.SPOT;
using System.Collections;
using System.IO;
using ECRU.File.Files; 

namespace ECRU.File
{
    class LocalManager
    {
        ArrayList Mutexs = new ArrayList();
        string MasterPath; 

        public LocalManager(string path)
        {
            MasterPath = path; 
        }

        public FileBase CreateFile(string path)
        {
            try
            {
                lock (path)
                {
                    if (!IsOpen(path))
                    {
                        FileInfo file = new FileInfo(MasterPath + @"\" + path);
                        if (file.Exists)
                            return null;

                        file.Directory.Create();
                        lock (Mutexs)
                            Mutexs.Add(path);
                        FileBase localfile = new FileBase();
                        localfile.Path = path;
                        localfile.Closefunc = CloseFile;
                        return localfile; 
                    }
                    return null;
                }
            }
            catch
            {
                return null; 
            }
            
        }

        public bool FileExists(string path)
        {
            FileInfo file = new FileInfo(MasterPath + @"\" + path);
            return file.Exists; 
        }

        public FileBase GetFile(string path)
        {
            try
            {
                lock (path)
                {
                    if (!IsOpen(path))
                    {
                        FileInfo file = new FileInfo(MasterPath + @"\" + path);
                        if (file.Exists)
                        {
                            lock (Mutexs)
                                Mutexs.Add(path);
                            using (FileStream r = new FileStream(file.FullName, FileMode.Open))
                            {
                                byte[] data = new byte[r.Length];
                                r.Read(data, 0, data.Length);
                                r.Close();
                                FileBase localfile = new FileBase();
                                localfile.Data = data;
                                localfile.Closefunc = CloseFile;
                                localfile.Path = path;
                                return localfile;
                            }
                        }
                    }
                    return null;
                }
            }
            catch
            {
                return null; 
            }
        }


        public FileBase GetReadOnlyFile(string path)
        {
            try
            {
                lock (path)
                {
                    FileInfo file = new FileInfo(MasterPath + @"\" + path);
                    if (file.Exists)
                    {
                        using (FileStream r = new FileStream(file.FullName, FileMode.Open))
                        {
                            byte[] data = new byte[r.Length];
                            r.Read(data, 0, data.Length);
                            r.Close();
                            FileBase localfile = new FileBase();
                            localfile.Data = data;
                            localfile.Closefunc = CloseReadOnlyFile;
                            localfile.Path = path;
                            return localfile;
                        }
                    }
                    return null; 
                }
            }
            catch
            {
                return null;
            }
        }

        private void CloseReadOnlyFile(FileBase file)
        {

        }

        public bool DeleteFile(string path)
        {
            if(!IsOpen(path))
            {
                FileInfo file = new FileInfo(MasterPath + @"\" + path);
                if (file.Exists)
                    file.Delete();
                return true; 
            }
            return false; 
        }

        public void CloseFile(FileBase localfile)
        {
            lock (Mutexs)
            {
                Mutexs.Remove(localfile.Path);
                if (localfile.Data == null)
                {
                    DeleteFile(localfile.Path);
                    return; 
                }
                else
                {
                    using (FileStream file = new FileStream(MasterPath + @"\" + localfile.Path, FileMode.Create))
                    {
                        try
                        {
                            file.Write(localfile.Data, 0, localfile.Data.Length);
                        }
                        finally
                        {
                            file.Close();
                        }
                    }
                }
            }
        }

        private bool IsOpen(string Path)
        {
            lock (Mutexs)
            {
                foreach (var s in Mutexs)
                {
                    string m = (string)s;
                    if (m == Path)
                        return true;
                }
                return false;
            }
        }

    }
}
