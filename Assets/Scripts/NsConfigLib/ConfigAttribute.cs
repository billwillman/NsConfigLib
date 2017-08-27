using System;

namespace NsLib.Config {

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ConfigIdAttribute: Attribute {

        public ConfigIdAttribute(uint id) {
            this.ID = id;
        }

        internal static int OnSort(System.Reflection.PropertyInfo prop1, System.Reflection.PropertyInfo prop2) {
            object[] objs1 = prop1.GetCustomAttributes(false);
            if (objs1 == null)
                return 1;
            object[] objs2 = prop2.GetCustomAttributes(false);
            if (objs2 == null)
                return -1;
            ConfigIdAttribute attr1 = null;
            for (int i = 0; i < objs1.Length; ++i) {
                attr1 = objs1[i] as ConfigIdAttribute;
                if (attr1 != null)
                    break;
            }

            if (attr1 == null)
                return 1;

            ConfigIdAttribute attr2 = null;
            for (int i = 0; i < objs2.Length; ++i) {
                attr2 = objs2[i] as ConfigIdAttribute;
                if (attr2 != null)
                    break;
            }

            if (attr2 == null)
                return -1;

            return (int)attr1.ID - (int)attr2.ID;
        }

        public uint ID {
            get;
            private set;
        }
    }
}