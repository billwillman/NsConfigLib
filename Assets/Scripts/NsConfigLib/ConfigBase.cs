using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Utils;

namespace NsLib.Config {
    public abstract class ConfigBase<KEY> {

        private static List<System.Reflection.PropertyInfo> m_Props = null;

        internal Stream stream {
            get;
            set;
        }
        private bool InitPropertys() {
            // 一个类型值只读一次
            if (m_Props == null) {
                System.Type type = GetType();
                System.Reflection.PropertyInfo[] props =
                    type.GetProperties(System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.SetProperty |
                        System.Reflection.BindingFlags.GetProperty);
                if (props == null || props.Length <= 0)
                    return false;
                m_Props = new List<System.Reflection.PropertyInfo>(props);
                m_Props.Sort(ConfigIdAttribute.OnSort);
               
            }
            return m_Props != null && m_Props.Count > 0;
        }

        internal long dataOffset {
            get;
            set;
        }

        internal abstract KEY ReadKey();
        internal abstract bool WriteKey(KEY key);

        internal bool StreamSeek()
        {
            if (stream == null)
                return false;
            stream.Seek (dataOffset, SeekOrigin.Begin);
            return true;
        }

        // 使用工具对继承的类进行反射，然后生成这部分代码，
        // 不使用反射来做（为了性能）
        // 默认情况使用反射来处理，后面类继承这个方法，工具生成这个代码
        // 后面一定要用工具生成代码，减少反射（因为是正式游戏运行时使用）
        internal virtual bool ReadValue() {
            if (stream == null)
                return false;

            if (IsReaded)
                return true;

            IsReaded = true;

            if (!InitPropertys())
                return false;
            
            for (int i = 0; i < m_Props.Count; ++i) {
                System.Reflection.PropertyInfo prop = m_Props[i];
                FilePathMgr.Instance.ReadProperty(stream, prop, this);
            }

            return true;
        }

        ///   <summary>   
        ///   写入用反射，读取不会使用，
        ///   所以不要在正式发布去调用这个方法
        ///   并且会new List
        ///   </summary> 
        ///   <param name="stream">写入的流</param>
        /// <returns>是否写入</returns>
        internal bool WriteValue() {
            if (stream == null)
                return false;

            if (!InitPropertys())
                return false;

            for (int i = 0; i < m_Props.Count; ++i) {
                System.Reflection.PropertyInfo prop = m_Props[i];
                object value = prop.GetValue(this, null);
                FilePathMgr.Instance.WriteProperty(stream, prop, value);
            }

            return true;
        }

        

        // 是否已经读取
        internal bool IsReaded {
            get;
            private set;
        }
    }

    public abstract class ConfigStringKey: ConfigBase<string> {
        internal override string ReadKey() {
            if (stream == null)
                return default(string);
            string key = FilePathMgr.Instance.ReadString(stream);
            return key;
        }

        internal override bool WriteKey(string key) {
            return FilePathMgr.Instance.WriteString(stream, key);
        }
    }

    public abstract class ConfigUIntKey: ConfigBase<uint> {
        internal override uint ReadKey() {
            if (stream == null)
                return default(uint);
            uint key = (uint)FilePathMgr.Instance.ReadInt(stream);
            return key;
        }

        internal override bool WriteKey(uint key) {
            return FilePathMgr.Instance.WriteInt(stream, (int)key);
        }
    }

}