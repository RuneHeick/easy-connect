using System.Collections;
using System.IO;

namespace ECRU.Utilities
{
    public class ConfigFile
    {
        private readonly Hashtable values = new Hashtable();
        private CloseFunction closeFunc;

        public ConfigFile(FileBase file)
        {
            File = file;
            if (File == null)
            {
                return;
            }
            closeFunc = File.Closefunc;
            File.Closefunc = f => Close();
            Init();
        }

        public FileBase File { get; private set; }

        public string this[string key]
        {
            get
            {
                if (values.Contains(key))
                    return values[key] as string;
                return null;
            }
            set
            {
                if (values.Contains(key))
                    values[key] = value;
                else
                    values.Add(key, value);
            }
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(string key)
        {
            return values.Contains(key);
        }

        private void Init()
        {
            if (File.Data == null) return;
            var m = new MemoryStream(File.Data);
            var r = new StreamReader(m);
            try
            {
                values.Clear();
                string line = r.ReadLine();
                if (line != null)
                {
                    while (line != null)
                    {
                        if (line[0] != '#')
                        {
                            string[] splitVals = line.Split('=');
                            if (splitVals.Length == 2)
                            {
                                values.Add(splitVals[0], splitVals[1]);
                            }
                        }
                        line = r.ReadLine();
                    }
                }
            }
            finally
            {
                r.Close();
                m.Dispose();
            }
        }

        public void Close()
        {
            if (closeFunc != null)
            {
                var m = new MemoryStream();
                m.Position = 0;
                var s = new StreamWriter(m);
                try
                {
                    foreach (object key in values.Keys)
                    {
                        s.WriteLine(key + "=" + values[key]);
                    }
                    s.Close();
                    File.Data = m.ToArray();
                }
                finally
                {
                    s.Close();
                }
                closeFunc(File);
                closeFunc = null;
            }
        }
    }
}