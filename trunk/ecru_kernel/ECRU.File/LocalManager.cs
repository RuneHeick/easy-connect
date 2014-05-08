using System;
using System.Threading;
using ECRU.Utilities.HelpFunction;
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
        readonly  object SDLOCK = new object();

        public LocalManager(string path)
        {
            DirectoryInfo files = new DirectoryInfo(path);
            if (!files.Exists)
            {
                files.Create();
            }
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
                
                    if (!IsOpen(path))
                    {
                        FileInfo file = new FileInfo(MasterPath + @"\" + path);
                        if (file.Exists)
                        {
                            lock (Mutexs)
                                Mutexs.Add(path);
                            lock (SDLOCK)
                            {
                                using (FileStream r = new FileStream(file.FullName, FileMode.Open))
                                {
                                    FileBase localfile = new FileBase();

                                    try
                                    {
                                        byte[] data = new byte[r.Length];
                                        r.Read(data, 0, data.Length);
                                        localfile.Data = data;
                                        localfile.Closefunc = CloseFile;
                                        localfile.Path = path;
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                    finally
                                    {
                                        r.Close();
                                        r.Dispose();
                                    }


                                    return localfile;

                                }
                            }
                        }
                    }
                    return null;
                
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
                lock (SDLOCK)
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
                lock (SDLOCK)
                {
                    FileInfo file = new FileInfo(MasterPath + @"\" + path);
                    if (file.Exists)
                        file.Delete();
                }
                return true; 
            }
            return false; 
        }

        public void CloseFile(FileBase localfile)
        {
            lock (Mutexs)
            {
                if (IsOpen(localfile.Path))
                {
                    Mutexs.Remove(localfile.Path);

                    if (localfile.Data != null)
                    {
                        lock (SDLOCK)
                        {
                            using (
                                var fs = new FileStream(MasterPath + @"\" + localfile.Path, FileMode.Create,
                                    System.IO.FileAccess.ReadWrite, FileShare.None))
                            {

                                try
                                {
                                    fs.Write(localfile.Data, 0, localfile.Data.Length);
                                    fs.Flush();
                                }
                                catch (Exception exception)
                                {
                                    Debug.Print("something error: " + exception.Message + " stacktrace: " +
                                                exception.StackTrace);
                                }
                                finally
                                {
                                    fs.Close();
                                    fs.Dispose();
                                }
                            }
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

        public FileInfo[] GetFiles()
        {
            DirectoryInfo files = new DirectoryInfo(MasterPath);
            return files.GetFiles();
        }


    }
}
