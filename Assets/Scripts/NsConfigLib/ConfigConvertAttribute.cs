using System;

namespace NsLib.Config
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigConvertAttribute: Attribute
    {
        public ConfigConvertAttribute(string configName, 
            System.Type mapType, string convertName = "")
        {
            ConfigName = configName;
            ConvertName = convertName;
            MapType = mapType;
        }

        public string ConfigName {
            get;
            private set;
        }

        public string ConvertName {
            get;
            private set;
        }

        public System.Type MapType {
            get;
            private set;
        }
    }

    /*
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ConfigConvertField: Attribute
    {
        public ConfigConvertField(string configName)
        {
            ConfigName = configName;
        }

        public string ConfigName {
            get;
            private set;
        }
    }*/

}

