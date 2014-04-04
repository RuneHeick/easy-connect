using System;
using System.Collections;
using System.IO;
using ECRU.Utilities.SD.Exceptions;

namespace ECRU.Utilities.SD
{
    public static class SD
    {

        public static string ReadJSONFromFile(string filePath)
        {
            string result = null;
            
            lock (filePath)
            {
                try
                {
                    if (DoesFileExist(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        var streamReader = new StreamReader(fileStream);

                        result = streamReader.ReadToEnd();

                        streamReader.Close();
                        fileStream.Close();
                    }
                }
                catch (Exception)
                {
                    
                    throw;
                }
                
            }

            return result;
        }

        public static bool WriteJSONToFile(string filePath, string jsonString)
        {
            lock (filePath)
            {
                try
                {
                    if (DoesFileExist(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        var streamWriter = new StreamWriter(fileStream);

                        streamWriter.WriteLine(jsonString);

                        streamWriter.Close();
                        fileStream.Close();
                    }
                }
                catch (Exception exception)
                {
                    
                    throw;
                }
                
            }
            return true;
        }

        public static bool WriteConfigurationToFile(string filePath, Hashtable configuration)
        {
            lock (filePath)
            {
                try
                {
                    if (DoesFileExist(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        var streamWriter = new StreamWriter(fileStream);

                        foreach (var key in configuration.Keys)
                        {
                            streamWriter.WriteLine(key + "=" + configuration[key]);
                        }

                        streamWriter.Close();
                        fileStream.Close();
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return true;
        }

        public static Hashtable ReadConfugurationFromFile(string filePath)
        {

            var config = new Hashtable();

            lock (filePath)
            {
                try
                {
                    if (DoesFileExist(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        var streamreader = new StreamReader(fileStream);

                        var line = streamreader.ReadLine();
                        if (line != null)
                        {
                            while (line != null)
                            {
                                if (line[0] != '#')
                                {
                                    var splitVals = line.Split('=');
                                    if (splitVals.Length == 2)
                                    {
                                        config.Add(splitVals[0], splitVals[1]);
                                    }
                                }
                                line = streamreader.ReadLine();
                            }
                        }
                        streamreader.Close();
                        fileStream.Close();

                    }
                    
                }
                catch (Exception)
                {
                    
                    throw;
                }
            }
            return config;
        }

        private static bool DoesFileExist(string filePath)
        {
            if (File.Exists(filePath)) return true;

            var splitValues = filePath.Split('\\');
            var fileName = splitValues[splitValues.Length - 1];
            throw new ECRUSDFileNotFoundException(filePath, fileName);
        }

    }
}
