using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Utils;

namespace NsLib.Config {

    internal interface IConfigBase {
        bool StreamSeek();

        Stream stream {
            get;
            set;
        }

        long dataOffset {
            get;
            set;
        }

        bool WriteValue();
        bool ReadValue();

        bool WriteKey(System.Object key);
		System.Type GetKeyType();

        System.Object ReadKEY();
    }

    public abstract class ConfigBase<KEY>: IConfigBase {
        // private static List<System.Reflection.PropertyInfo> m_Props = null;
        private static Dictionary<System.Type, List<System.Reflection.PropertyInfo>> m_PropsMap = new Dictionary<Type, List<System.Reflection.PropertyInfo>>();
        private List<System.Reflection.PropertyInfo> _Propertys = null;

        protected List<System.Reflection.PropertyInfo> Propertys {
            get {
                if (_Propertys == null) {
                    System.Type type = GetType ();
                    List<System.Reflection.PropertyInfo> ret = null;
                    if (!m_PropsMap.TryGetValue (type, out ret))
                        ret = null;
                    _Propertys = ret;
                    return ret;
                }

                return _Propertys;
            }
            set {
                if (value != null && value.Count > 0) {
                    System.Type type = GetType();
                    m_PropsMap[type] = value;
                    _Propertys = value;
                }
            }
        }

        public Stream stream {
            get;
            set;
        }
		public System.Type GetKeyType() {
            return typeof(KEY);
        }
		
        private bool InitPropertys() {
            // 一个类型值只读一次
            var m_Props = Propertys;
            if (m_Props == null) {
                System.Type type = GetType();
                System.Reflection.PropertyInfo[] props =
                    type.GetProperties(System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.SetProperty |
                        System.Reflection.BindingFlags.GetProperty);
                if (props == null || props.Length <= 0)
                    return false;
                m_Props = new List<System.Reflection.PropertyInfo>();
                for (int i = 0; i < props.Length; ++i) {
                    var prop = props[i];
                    object[] attrs = prop.GetCustomAttributes(false);
                    if (attrs != null && attrs.Length > 0) {
                        for (int j = 0; j < attrs.Length; ++j) {
                            ConfigIdAttribute attr = attrs[j] as ConfigIdAttribute;
                            if (attr != null) {
                                m_Props.Add(prop);
                                break;
                            }
                        }
                    }
                }
                m_Props.Sort(ConfigIdAttribute.OnSort);
                Propertys = m_Props;
            }
            return m_Props != null && m_Props.Count > 0;
        }

        public long dataOffset {
            get;
            set;
        }

        internal abstract KEY ReadKey();
        internal abstract bool WriteKey(KEY key);

        public System.Object ReadKEY() {
            return ReadKey();
        }

        public bool StreamSeek()
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
        public virtual bool ReadValue() {
            if (IsReaded)
                return true;

            if (stream == null)
                return false;

            IsReaded = true;

            if (!InitPropertys())
                return false;

            var m_Props = this.Propertys;
            for (int i = 0; i < m_Props.Count; ++i) {
                System.Reflection.PropertyInfo prop = m_Props[i]; 
                FilePathMgr.Instance.ReadProperty(stream, prop, this);
            }
            stream = null;

            return true;
        }

        ///   <summary>   
        ///   写入用反射，读取不会使用，
        ///   所以不要在正式发布去调用这个方法
        ///   并且会new List
        ///   </summary> 
        ///   <param name="stream">写入的流</param>
        /// <returns>是否写入</returns>
        public bool WriteValue() {
            if (stream == null)
                return false;

            if (!InitPropertys())
                return false;

            var m_Props = this.Propertys;
            for (int i = 0; i < m_Props.Count; ++i) {
                System.Reflection.PropertyInfo prop = m_Props[i];
				if (prop.CanRead && prop.CanWrite)
				{
					object value = prop.GetValue(this, null); 
					FilePathMgr.Instance.WriteProperty(stream, prop, value);
				}
            }

            return true;
        }

        public bool WriteKey(System.Object key) {
            return FilePathMgr.Instance.WriteObject(stream, key, GetKeyType());
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