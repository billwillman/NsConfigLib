using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace NsLib.Config {
    public static class ConfigConvertManager {

        public class ConvertClassInfo {

            public ConvertClassInfo(System.Type target, ConfigConvertAttribute attr) {
                this.type = target;
                this.configName = attr.ConfigName;
                this.DictionaryType = attr.MapType;
                this.convertName = attr.ConvertName;
                if (string.IsNullOrEmpty(convertName))
                    this.convertName = configName;
            }

            /*
            public ConvertClassInfo(System.Type target, ConfigConvertField attr, bool isListMode) {
                this.type = target;
                this.isListMode = isListMode;
                this.configName = attr.ConfigName;
                this.convertName = attr.ConvertName;
                if (string.IsNullOrEmpty(convertName))
                    this.convertName = configName;
            }*/

            // 字典类型
            public System.Type DictionaryType {
                get;
                private set;
            }

            public System.Type type {
                get;
                private set;
            }

            public string configName {
                get;
                private set;
            }

            public string convertName {
                get;
                private set;
            }
        }

        // LitJson转换成自定义格式
        public static void ConvertToBinaryFile(string fileName, string configName, string json) {

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(json))
                return;

            ConvertClassInfo info = GetConvertClass(configName);
            if (info == null || info.DictionaryType == null)
                return;

            System.Type dictType = info.DictionaryType;
            System.Collections.IDictionary values = LitJsonHelper.ToTypeObject(json, info.DictionaryType) as System.Collections.IDictionary;
            if (values == null)
                return;
            string newFileName = string.Format("{0}/{1}.bytes", Path.GetDirectoryName(fileName), info.convertName);
            FileStream stream = new FileStream(newFileName, FileMode.Create, FileAccess.Write);
            try {
                ConfigWrap.ToStream(stream, values);
            } finally {
                stream.Close();
                stream.Dispose();
            }
        }

        public static ConvertClassInfo GetConvertClass(string configName) {
            if (string.IsNullOrEmpty(configName))
                return null;
            ConvertClassInfo ret;
            if (!m_ConvertClassMap.TryGetValue(configName, out ret))
                ret = null;
            return ret;
        }


        private static Dictionary<string, ConvertClassInfo> m_ConvertClassMap = new Dictionary<string, ConvertClassInfo>();

        private static bool BuildConfigConvertClass(System.Type t) {
            if (t == null)
                return false;
            // 先找标记了的类
            bool ret = false;
            object[] attrs = t.GetCustomAttributes(false);
            if (attrs != null && attrs.Length > 0) {
                for (int i = 0; i < attrs.Length; ++i) {
                    ConfigConvertAttribute attr = attrs[i] as ConfigConvertAttribute;
                    if (attr == null || string.IsNullOrEmpty(attr.ConfigName))
                        continue;
                    ConvertClassInfo info = new ConvertClassInfo(t, attr);
                    m_ConvertClassMap[attr.ConfigName] = info;
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        /*
        private static void BuildConfigConvertFields(System.Type t) {
            if (t == null)
                return;
            FieldInfo[] fields = t.GetFields(BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.Public | BindingFlags.NonPublic);

            if (fields != null && fields.Length > 0) {
                for (int i = 0; i < fields.Length; ++i) {
                    FieldInfo field = fields[i];
                    if (field == null)
                        continue;
                    System.Type fieldType = field.FieldType;

                    // 必须是类
                    // 类型必须是继承IDictionary, Dictionary<K, V>
                    if (!fieldType.IsClass ||
                        string.IsNullOrEmpty(fieldType.Name) ||
                        fieldType.Name.IndexOf("Dictionary", StringComparison.CurrentCultureIgnoreCase) < 0)
                        continue;

                    System.Type[] interfaces = fieldType.GetInterfaces();
                    if (interfaces == null)
                        continue;
                    bool isFound = false;
                    for (int j = 0; j < interfaces.Length; ++j) {
                        System.Type inter = interfaces[j];
                        if (string.IsNullOrEmpty(inter.Name) ||
                            inter.Name.IndexOf("IDictionary`2") < 0)
                            continue;
                        isFound = true;

                        break;
                    }

                    if (!isFound)
                        continue;

                    System.Object[] attrs = field.GetCustomAttributes(false);
                    if (attrs != null) {
                        for (int j = 0; j < attrs.Length; ++j) {
                            ConfigConvertField attr = attrs[j] as ConfigConvertField;
                            if (attr != null) {
                                ConvertClassInfo info;
                                if (m_ConvertClassMap.TryGetValue(attr.ConfigName, out info)) {
                                    info.DictionaryType = fieldType;
                                }
                                break;
                            }
                        }
                    }

                }
            }
        }*/

        private static void ClearConvertMaps() {
            m_ConvertClassMap.Clear();
        }

        // 重新编译配置Convert表
        public static void BuildConfigConvert() {
            ClearConvertMaps();

            Assembly asm = Assembly.GetExecutingAssembly();
            System.Type[] types = asm.GetTypes();
            if (types == null || types.Length <= 0)
                return;

            
            for (int i = 0; i < types.Length; ++i) {
                System.Type t = types[i];
                if (BuildConfigConvertClass(t)) {
                    // 再找这个类里面的field字段是否有标记位
                   // BuildConfigConvertFields(t);
                }
            }
        }
    }
}