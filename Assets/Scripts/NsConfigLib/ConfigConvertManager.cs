using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.Config {
    public static class ConfigConvertManager {

        public class ConvertClassInfo {

            public ConvertClassInfo(System.Type target, ConfigConvertAttribute attr) {
                this.type = target;
                this.isListMode = attr.IsListMode;
                this.configName = attr.ConfigName;
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


            public System.Type type {
                get; set;
            }

            public bool isListMode {
                get;
                set;
            }

            public string configName {
                get;
                set;
            }

            public string convertName {
                get;
                set;
            }
        }

        public static void ConvertToBinaryFile(string configName) {
            ConvertClassInfo info = GetConvertClass(configName);
            if (info == null)
                return;

            
            if (info.isListMode) {
                // List模式
            } else {
                // 非List模式
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

        private static void BuildConfigConvertClass(System.Type t) {
            if (t == null)
                return;
            // 先找标记了的类
            object[] attrs = t.GetCustomAttributes(false);
            if (attrs != null && attrs.Length > 0) {
                for (int i = 0; i < attrs.Length; ++i) {
                    ConfigConvertAttribute attr = attrs[i] as ConfigConvertAttribute;
                    if (attr == null || string.IsNullOrEmpty(attr.ConfigName))
                        continue;
                    ConvertClassInfo info = new ConvertClassInfo(t, attr);
                    m_ConvertClassMap[attr.ConfigName] = info;
                    break;
                }
            }
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

                    System.Type TKey = interfaces[0];
                    System.Type TValue = interfaces[1];

                    System.Type targetType;
                    bool isListMode = TValue.IsSubclassOf(typeof(IList));
                    if (isListMode) {
                        System.Type[] listTypes = TValue.GetInterfaces();
                        if (listTypes == null || listTypes.Length != 1)
                            continue;
                        targetType = listTypes[0];
                    } else
                        targetType = TValue;

                    object[] attrs = field.GetCustomAttributes(false);
                    if (attrs != null && attrs.Length > 0) {
                        for (int j = 0; j < attrs.Length; ++j) {
                            ConfigConvertField attr = attrs[j] as ConfigConvertField;
                            if (attr == null || string.IsNullOrEmpty(attr.ConfigName))
                                continue;

                            ConvertClassInfo info = new ConvertClassInfo(targetType, attr, isListMode);
                            m_ConvertClassMap[attr.ConfigName] = info;

                            break;
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
                BuildConfigConvertClass(t);
                // 再找这个类里面的field字段是否有标记位
                //BuildConfigConvertFields(t);
            }
        }
    }
}