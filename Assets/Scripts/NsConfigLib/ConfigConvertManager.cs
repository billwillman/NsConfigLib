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

        public static void ConvertToBinarySplitFile(string fileName, string configName, string json,
            int maxSplitCnt = 50) {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(json))
                return;

            ConvertClassInfo info = GetConvertClass(configName);
            if (info == null || info.DictionaryType == null)
                return;

            System.Collections.IDictionary values = LitJsonHelper.ToTypeObject(json, info.DictionaryType) as System.Collections.IDictionary;
            if (values == null)
                return;

            string newFileName = string.Format("{0}/{1}.bytes", Path.GetDirectoryName(fileName), info.convertName);
            FileStream stream = new FileStream(newFileName, FileMode.Create, FileAccess.Write);
            try {
                try {
                    ConfigWrap.ToStreamSplit(stream, newFileName, values, maxSplitCnt);
                } catch (Exception e) {
                    UnityEngine.Debug.LogErrorFormat("【转换异常】{0}=>{1}", fileName, e.ToString());
                }
            } finally {
                stream.Flush();
                stream.Close();
                stream.Dispose();
            }
        }

        // LitJson转换成自定义格式
        public static void ConvertToBinaryFile(string fileName, string configName, string json) {

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(json))
                return;

            ConvertClassInfo info = GetConvertClass(configName);
            if (info == null || info.DictionaryType == null)
                return;

          //  System.Type dictType = info.DictionaryType;
            System.Collections.IDictionary values = LitJsonHelper.ToTypeObject(json, info.DictionaryType) as System.Collections.IDictionary;
            if (values == null)
                return;
            string newFileName = string.Format("{0}/{1}.bytes", Path.GetDirectoryName(fileName), info.convertName);
            FileStream stream = new FileStream(newFileName, FileMode.Create, FileAccess.Write);
            try {
                try {
                    ConfigWrap.ToStream(stream, values);
                } catch (Exception e) {
                    UnityEngine.Debug.LogErrorFormat("【转换异常】{0}=>{1}", fileName, e.ToString());
                }
            } finally {
                stream.Flush();
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

        public static ConvertClassInfo GetTargetConvert(string targetConvertName) {
            if (string.IsNullOrEmpty(targetConvertName))
                return null;
            ConvertClassInfo ret;
            if (!m_TargetNameMap.TryGetValue(targetConvertName, out ret))
                ret = null;
            return ret;
        }

        // 检查
        private static void PrintCheckErrorPropertys(string str, System.Type tt) {
            // 字段和Property的名字
            List<string> fieldAndPropNamesNoConfigId = new List<string>();
            System.Reflection.PropertyInfo[] props =
                   tt.GetProperties(System.Reflection.BindingFlags.Public |
                       System.Reflection.BindingFlags.Instance |
                       System.Reflection.BindingFlags.SetProperty |
                       System.Reflection.BindingFlags.GetProperty);

            HashSet<uint> hasIdsMap = new HashSet<uint>();
            if (props != null && props.Length > 0) {
                for (int i = 0; i < props.Length; ++i) {
                    var prop = props[i];
                    object[] attrs = prop.GetCustomAttributes(false);
                    bool isFound = false;
                    if (attrs != null && attrs.Length > 0) {
                        for (int j = 0; j < attrs.Length; ++j) {
                            ConfigIdAttribute attr = attrs[j] as ConfigIdAttribute;
                            if (attr != null) {
                                if (hasIdsMap.Contains(attr.ID)) {
                                    UnityEngine.Debug.LogErrorFormat("typeName: {0}=>attrId {1:D} has exists~~!!", tt.Name, attr.ID);
                                } else
                                    hasIdsMap.Add(attr.ID);
                                isFound = true;
                                break;
                            }
                        }
                    }
                    if (!isFound && prop.CanRead && prop.CanWrite) {
                        fieldAndPropNamesNoConfigId.Add(prop.Name);
                    }
                }
            }

            FieldInfo[] fields = tt.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields != null && fields.Length > 0) {
                for (int i = 0; i < fields.Length; ++i) {
                    var field = fields[i];
                    if (field == null)
                        continue;
                    fieldAndPropNamesNoConfigId.Add(field.Name);
                }
            }

            if (fieldAndPropNamesNoConfigId.Count > 0) {
                for (int i = 0; i < fieldAndPropNamesNoConfigId.Count; ++i) {
                    string fieldName = fieldAndPropNamesNoConfigId[i];
                    if (string.IsNullOrEmpty(fieldName))
                        continue;
                    if (str.IndexOf(fieldName) >= 0) {
                        UnityEngine.Debug.LogErrorFormat("typeName: {0}=>【{1}】 is no ConfigId or no Property~~~~!!!!", tt.Name, fieldName);
                    }
                }
            }

            // 检查那些设置了ConfigId，但表里没有的，可以删除
            fieldAndPropNamesNoConfigId.Clear();
            if (props != null && props.Length > 0) {
                for (int i = 0; i < props.Length; ++i) {
                    var prop = props[i];
                    object[] attrs = prop.GetCustomAttributes(false);
                    bool isFound = false;
                    if (attrs != null && attrs.Length > 0) {
                        for (int j = 0; j < attrs.Length; ++j) {
                            ConfigIdAttribute attr = attrs[j] as ConfigIdAttribute;
                            if (attr != null) {
                                isFound = true;
                                break;
                            }
                        }
                    }
                    if (isFound && prop.CanRead && prop.CanWrite) {
                        fieldAndPropNamesNoConfigId.Add(prop.Name);
                    }
                }

                if (fieldAndPropNamesNoConfigId.Count > 0) {
                    for (int i = 0; i < fieldAndPropNamesNoConfigId.Count; ++i) {
                        string fieldName = fieldAndPropNamesNoConfigId[i];
                        if (string.IsNullOrEmpty(fieldName)) {
                            if (str.IndexOf(fieldName) < 0) {
                                UnityEngine.Debug.LogErrorFormat("typeName: {0}=>【{1}】 is can delete~~~~!!!!", tt.Name, fieldName);
                            }
                        }
                    }
                }
            }
        }

        // 检查定义错的耳机只能回VO文件类型
        // Assets/Configs/Resources/Config/table/
        public static void PrintCheckErrorPropertys(string rootPath) {
            if (string.IsNullOrEmpty(rootPath))
                return;
            // 重新生成定义
            BuildConfigConvert();
            // 检查定义

            if (rootPath[rootPath.Length - 1] != '/')
                rootPath += "/";

            var iter = m_ConvertClassMap.GetEnumerator();
            while (iter.MoveNext()) {
                string fileName = iter.Current.Value.configName;
                // fileName = string.Format("Assets/Configs/Resources/Config/table/{0}.txt", fileName);
                fileName = string.Format("{0}{1}.txt", rootPath, fileName);
                if (File.Exists(fileName)) {
                    FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    try {
                        if (stream.Length > 0) {
                            byte[] buf = new byte[stream.Length];
                            stream.Read(buf, 0, buf.Length);
                            string str = System.Text.Encoding.UTF8.GetString(buf);
                            PrintCheckErrorPropertys(str, iter.Current.Value.type);
                        }
                    } finally {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            iter.Dispose();
        }


        private static Dictionary<string, ConvertClassInfo> m_ConvertClassMap = new Dictionary<string, ConvertClassInfo>();
        private static Dictionary<string, ConvertClassInfo> m_TargetNameMap = new Dictionary<string, ConvertClassInfo>();

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
                    m_TargetNameMap[info.convertName] = info;
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
            m_TargetNameMap.Clear();
        }

        // 重新编译配置Convert表
        public static void BuildConfigConvert() {
            ClearConvertMaps();
			// 清理掉所以定义
            ConfigStringKey.ClearPropsMap();

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