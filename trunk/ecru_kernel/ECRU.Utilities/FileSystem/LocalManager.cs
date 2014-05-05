using System;
using System.Collections;
using System.IO;
using System.Threading;
using Microsoft.SPOT;

namespace ECRU.Utilities
{
    public class LocalManager
    {
        private readonly string MasterPath;
        private readonly ArrayList Mutexs = new ArrayList();
        private readonly object SDLOCK = new object();

        public LocalManager(string path)
        {
            var files = new DirectoryInfo(path);
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
                        var file = new FileInfo(MasterPath + @"\" + path);
                        if (file.Exists)
                            return null;

                        file.Directory.Create();
                        lock (Mutexs)
                            Mutexs.Add(path);
                        var localfile = new FileBase();
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
            var file = new FileInfo(MasterPath + @"\" + path);
            return file.Exists;
        }

        public FileBase GetFile(string path)
        {
            try
            {
                if (!IsOpen(path))
                {
                    var file = new FileInfo(MasterPath + @"\" + path);
                    if (file.Exists)
                    {
                        lock (Mutexs)
                            Mutexs.Add(path);
                        lock (SDLOCK)
                        {
                            using (var r = new FileStream(file.FullName, FileMode.Open))
                            {
                                var localfile = new FileBase();

                                try
                                {
                                    var data = new byte[r.Length];
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
                    var file = new FileInfo(MasterPath + @"\" + path);
                    if (file.Exists)
                    {
                        using (var r = new FileStream(file.FullName, FileMode.Open))
                        {
                            var data = new byte[r.Length];
                            r.Read(data, 0, data.Length);
                            r.Close();
                            var localfile = new FileBase();
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
            if (!IsOpen(path))
            {
                lock (SDLOCK)
                {
                    var file = new FileInfo(MasterPath + @"\" + path);
                    if (file.Exists)
                        file.Delete();
                }
                Thread.Sleep(1000);
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
                        //using (FileStream file = new FileStream(MasterPath + @"\" + localfile.Path, FileMode.Create, System.IO.FileAccess.Write))
                        //{
                        //    try
                        //    {
                        //        file.Write(localfile.Data, 0, localfile.Data.Length);
                        //        file.Flush();
                        //    }
                        //    finally
                        //    {
                        //        file.Close();
                        //    }
                        //}
                    }
                }
            }
        }

        private bool IsOpen(string Path)
        {
            lock (Mutexs)
            {
                foreach (object s in Mutexs)
                {
                    var m = (string) s;
                    if (m == Path)
                        return true;
                }
                return false;
            }
        }

        public FileInfo[] GetFiles()
        {
            var files = new DirectoryInfo(MasterPath);
            return files.GetFiles();
        }
    }
}