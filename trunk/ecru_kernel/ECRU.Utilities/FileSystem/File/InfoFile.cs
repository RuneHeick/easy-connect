using System;
using Microsoft.SPOT;

namespace ECRU.Utilities
{
    public class InfoFile: ConfigFile
    {
        public InfoFile(FileBase file):
            base(file)
        {
            if (!base.Contains("Version"))
                base["Version"] = "0";

            if (!base.Contains("Hash"))
                base["Hash"] = "0"; 
        }

        public long Version
        {
            get
            {
                return Convert.ToInt64(base["Version"]);
            }
            set
            {
                base["Version"] = value.ToString();
            }
        }

        public long Hash
        {
            get
            {
                return Convert.ToInt64(base["Hash"]);
            }
            set
            {
                base["Hash"] = value.ToString();
            }
        }


    }
}
