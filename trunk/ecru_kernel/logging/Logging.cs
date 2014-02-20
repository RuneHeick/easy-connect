using System;
using System.IO;
using Microsoft.SPOT;

namespace logging
{
    public class Logging
    {
        #region Fields
        private static string _logFilePath;
        private static StreamWriter _streamWriter;
        private StreamWriter _customStreamWriter;
        private string _customLogFilePath;
        #endregion

        #region Private Properties
        private string CustomDirectoryName { get; set; }
        private string CustomFileNameWithExtension { get; set; }
        private bool CustomAppend { get; set; }

        private StreamWriter CustomStreamWriter
        {
            get {
                return _customStreamWriter ?? (_customStreamWriter = new StreamWriter(CustomFilePath, CustomAppend));
            }
        }

        #endregion

        #region Public Properties

        public string CustomFilePath
        {
            get
            {
                if (CustomDirectoryName == string.Empty)
                {
                    throw new Exception("Custom directory cannot be blank");
                }
                if (CustomFileNameWithExtension == string.Empty)
                {
                    throw new Exception("File name cannot be blank");
                }
                return _customLogFilePath ?? (_customLogFilePath = GetFilePath(
                    GetDirectoryPath(CustomDirectoryName) + Path.DirectorySeparatorChar +
                    CustomFileNameWithExtension, CustomAppend));
            }
        }

        public bool CustomPrefixDateTime { get; set; }
        public bool CustomLogToFile { get; set; }
        #endregion

        #region Private Static Properties
        private static string SDCardDirectory { get { return "SD"; } }
        private static StreamWriter StreamWriter
        {
            get { return _streamWriter ?? (_streamWriter = new StreamWriter(LogFilePath, (bool) Append)); }
        }
        #endregion

        #region Public Static Properties
        public static bool PrefixDateTime { set; get; }
        public static bool LogToFile { set; get; }
        public static bool Append { set; get; }
        public static string LogFilePath
        {
            get
            {
                return _logFilePath ?? (_logFilePath = GetFilePath(GetDirectoryPath("Report") +
                                                                   Path.DirectorySeparatorChar + "Log.txt", Append));
            }
        }
        #endregion

        #region Constructor
        public Logging(string directoryName, string fileNameWithExtension, bool append = true)
		{
			CustomDirectoryName = directoryName;
			CustomFileNameWithExtension = fileNameWithExtension;
			CustomAppend = append;
		}
        #endregion

        #region Private Static Methods
        private static string GetDirectoryPath(string trimmedDIrectoryPath)
        {
            if (!Directory.Exists(SDCardDirectory))
            {
                throw new Exception("SD card (directory) not found");
            }

            string directoryPath = SDCardDirectory + Path.DirectorySeparatorChar + trimmedDIrectoryPath;

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        private static string GetFilePath(string fullFileName, bool append)
        {
            if (!File.Exists(fullFileName) || !append)
            {
                File.Create(fullFileName);
            }
            return fullFileName;
        }

        private static void WriteLog(string message, StreamWriter streamWriter, bool addDateTime, bool logToFile)
        {
            if (addDateTime)
            {
                DateTime currenTime = DateTime.Now;
                message = "[" + currenTime + ":" + currenTime.Millisecond + "]" + message;
            }
            Debug.Print(message);
            if (logToFile)
            {
                streamWriter.WriteLine(message);
            }
        }
        #endregion

        #region Public Static Methods
        public static void Log(params object[] strings)
        {
            var logMessage = string.Empty;
            foreach (var message in strings)
            {
                logMessage = logMessage + message.ToString() + " ";
            }
            WriteLog(logMessage, StreamWriter, PrefixDateTime, LogToFile);
        }

        public static void Flush()
        {
            if (_streamWriter == null) return;
            StreamWriter.Flush();
        }

        public static void Close()
        {
            if (_streamWriter == null) return;
            StreamWriter.Flush();
            StreamWriter.Close();
            StreamWriter.Dispose();
        }

        #endregion

        #region Public Methods
        public void LogCustom(params object[] strings)
        {
            string message = string.Empty;
            for (int i = 0; i < strings.Length; i++)
            {
                message = message + strings[i].ToString() + " ";
            }
            WriteLog(message, CustomStreamWriter, CustomPrefixDateTime, CustomLogToFile);
        }
        public void FlushCustomLogger()
        {
            if (CustomStreamWriter == null) return;
            CustomStreamWriter.Flush();
        }
        public void CloseCustomStreamWriter()
        {
            if (CustomStreamWriter == null) return;
            CustomStreamWriter.Flush();
            CustomStreamWriter.Close();
            CustomStreamWriter.Dispose();
        }
        #endregion

    }
}
