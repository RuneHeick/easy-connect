using System;
using Microsoft.SPOT;
using System.Collections;
using System.IO; 

namespace ECRU.File.Files
{
    public class ConfigFile:FileBase
    {
        Hashtable values = new Hashtable(); 

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(string key)
        {
            return values.Contains(key);
        }

        public ConfigFile(FileBase file)
        {
            Data = file.Data;
            Closefunc = file.Closefunc;
            Path = file.Path; 
        }

        public string this[string key]
        {
            get
            {
                if(values.Contains(key))
                    return values[key] as string;
                else
                    return null; 
            }
            set
            {
                if(values.Contains(key))
                    values[key] = value; 
                else
                    values.Add(key, value);
            }
        }


        public override byte[] Data
        {
            get
            {
                MemoryStream m = new MemoryStream();
                StreamWriter s = new StreamWriter(m);
                try
                {
                    foreach (object key in values.Keys)
                    {
                        s.WriteLine(key + "=" + values[key]);
                    }
                    s.Close();
                    return m.ToArray();
                }
                finally
                {
                    s.Close();
                    m.Dispose(); 
                }
            }
            set
            {
                if (value == null) return; 
                MemoryStream m = new MemoryStream(value);
                StreamReader r = new StreamReader(m);
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
        }


    }
}
