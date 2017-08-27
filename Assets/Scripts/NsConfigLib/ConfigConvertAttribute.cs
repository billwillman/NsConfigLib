using System;

namespace NsLib.Config
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigConvertAttribute: Attribute
    {
        public ConfigConvertAttribute(string configName, bool isListMode, string convertName = "")
        {
            ConfigName = configName;
            IsListMode = isListMode;
            ConvertName = convertName;
        }

        public string ConfigName {
            get;
            private set;
        }

        public bool IsListMode {
            get;
            private set;
        }

        public string ConvertName {
            get;
            private set;
        }
    }

    /*
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ConfigConvertField: Attribute
    {
        public ConfigConvertField(string configName, string convertName = "")
        {
            ConfigName = configName;
            ConvertName = convertName;
        }

        public string ConfigName {
            get;
            private set;
        }

        public string ConvertName {
            get;
            private set;
        }
    }*/

}

