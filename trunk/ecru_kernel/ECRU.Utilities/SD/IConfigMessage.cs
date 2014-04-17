using System.Collections;

namespace ECRU.Utilities.SD
{
    public interface IConfigMessage
    {
        Hashtable ContentHashtable { get; set; }
    }

    public class ConfigMessage : IConfigMessage
    {
        public ConfigMessage(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
        public Hashtable ContentHashtable { get; set; }
    }
}