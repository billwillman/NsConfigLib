using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utils;

namespace NsLib.Config {

    public interface IConfigDataStream {
        Stream LoadConfigDataFromFile(string fileName);
    }

    public class ResourcesConfigDataStream: IConfigDataStream {
        public Stream LoadConfigDataFromFile(string fileName) {
            int idx = fileName.LastIndexOf('.');
            if (idx >= 0)
                fileName = fileName.Substring(0, idx);
            UnityEngine.TextAsset textAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(fileName);
            MemoryStream stream = new MemoryStream(textAsset.bytes);
            return stream;
        }
    }


    // 配置文件
    public abstract class ConfigFile<K, V> where V: class, new(){

        private ConfigIndexFile<K> m_IndexFile = null;
        private IConfigDataStream m_DataStream = null;
        private Dictionary<K, V> m_DataMap = null;
        private string m_Name = string.Empty;
        private string m_Dir = string.Empty;

        public V this[K key] {
            get {
                V ret;
                if (!TryGetValue(key, out ret))
                    ret = null;
                return ret;
            }
        }

        public List<V> ValueList {
            get {
                List<V> ret = null;
                if (m_IndexFile == null)
                    return ret;
                var iter = m_IndexFile.GetIter();
                while (iter.MoveNext()) {
                    V v;
                    if (TryGetValue(iter.Current.Key, out v) && v != null) {
                        if (ret == null)
                            ret = new List<V>();
                        ret.Add(v);
                    }
                }
                iter.Dispose();
                return ret;
            }
        }

        public void Clear() {
            if (m_DataMap != null)
                m_DataMap.Clear();
            m_IndexFile = null;
        }

        protected ConfigIndexFile<K> IndexFile {
            get {
                if (m_IndexFile == null)
                    m_IndexFile = new ConfigIndexFile<K>();
                return m_IndexFile;
            }
        }

        public ConfigFile(IConfigDataStream dataStream) {
            m_DataStream = dataStream;
        }

        protected bool LoadFromStream(Stream stream) {
            if (stream == null || !stream.CanRead)
                return false;
            ConfigIndexFile<K> indexFile = this.IndexFile;
            if (indexFile == null)
                return false;
            if (!indexFile.LoadFromStream(stream))
                return false;
            var iter =indexFile.GetIter();
            while (iter.MoveNext()) {
                if (m_DataMap == null)
                    m_DataMap = new Dictionary<K, V>();
                m_DataMap[iter.Current.Key] = null;
            }
            iter.Dispose();
            return true;
        }

        public bool LoadFromFileName(string fileName) {
            if (m_DataStream == null)
                return false;
            Stream stream = m_DataStream.LoadConfigDataFromFile(fileName);
            if (stream == null || !stream.CanRead)
                return false;
            m_Name = Path.GetFileNameWithoutExtension(fileName);
            m_Dir = Path.GetDirectoryName(fileName);
            return LoadFromStream(stream);
        }

        public ConfigWrap.ConfigValueType ValueType {
            get {
                var indexFile = this.IndexFile;
                if (indexFile == null)
                    return ConfigWrap.ConfigValueType.cvNone;
                return indexFile.ValueType;
            }
        }

        public bool ContainsKey(K key) {
            if (m_DataMap == null)
                return false;
            bool ret = m_DataMap.ContainsKey(key);
            return ret;
        }

        private bool LoadDataFromFile(string fileName) {
            if (string.IsNullOrEmpty(fileName) || m_DataStream == null)
                return false;
            Stream stream = m_DataStream.LoadConfigDataFromFile(fileName);
            return LoadDataFromStream(stream);
        }

        protected abstract V ReadItemValue(Stream stream, K key);

        private bool LoadDataFromStream(Stream stream) {
            if (stream == null || !stream.CanRead)
                return false;
            long cntOffset = stream.Length - 4;
            if (cntOffset <= 0)
                return false;
            stream.Seek(cntOffset, SeekOrigin.Begin);
            int cnt = FilePathMgr.GetInstance().ReadInt(stream);
            if (cnt <= 0)
                return false;

            ConfigWrap.ConfigValueType valueType = this.ValueType;
            System.Type tt = typeof(K);
            stream.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < cnt; ++i) {
                System.Object key = FilePathMgr.GetInstance().ReadObject(stream, tt);
                K k = (K)key;
                V value = ReadItemValue(stream, k);
                if (m_DataMap == null)
                    m_DataMap = new Dictionary<K, V>();
                m_DataMap[k] = value;
            }
            return true;
        }

        public bool TryGetValue(K key, out V value) {
            value = null;
            if (m_DataMap == null) {
                return false;
            }
            if (m_DataMap.TryGetValue(key, out value)) {

                if (value != null) {
                    return true;
                }

                if (m_IndexFile != null) {
                    IndexFileData indexData;
                    if (!m_IndexFile.FindIndexData(key, out indexData) || 
                        !indexData.IsVaild)
                        return false;

                    string fileName;
                    if (!string.IsNullOrEmpty(m_Dir))
                        fileName = string.Format("{0}/@{1}/{1}_{2:D}.bytes", m_Dir, m_Name, indexData.Index);
                    else
                        fileName = string.Format("@{0}/{0}_{1:D}.bytes", m_Name, indexData.Index);

                    if (!LoadDataFromFile(fileName))
                        return false;
                    bool ret = m_DataMap.TryGetValue(key, out value);
                    return ret;
                }
            }

            return false;
        }

    }

    public class ConfigFile_Object<K, V> : ConfigFile<K, V>  where V: ConfigBase<K>, new() {
        public ConfigFile_Object(IConfigDataStream dataStream): base(dataStream) { }

        protected override V ReadItemValue(Stream stream, K key) {
            V ret = Activator.CreateInstance<V>();
            ret.stream = stream;
            if (!ret.ReadValue()) {
                ret.stream = null;
                return null;
            }
            ret.stream = null;
            return ret;
        }
    }

    public class ConfigFile_List<K, V> : ConfigFile<K, List<V>> where V : ConfigBase<K>, new() {
        public ConfigFile_List(IConfigDataStream dataStream) : base(dataStream) { }

        protected override List<V> ReadItemValue(Stream stream, K key) {
            var indexFile = this.IndexFile;
            IndexFileData data;
            if (!indexFile.FindIndexData(key, out data) || !data.IsVaild)
                return null;
            List<V> ret = new List<V>(data.Count);
            for (int i = 0; i < data.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                config.stream = stream;
                config.ReadValue();
                config.stream = null;
                ret.Add(config);
            }
            return ret;
        }
    }

    public class ConfigFile_Map<K1, K2, V2>: ConfigFile<K1, Dictionary<K2, V2>> where V2: ConfigBase<K2>, new() {
        public ConfigFile_Map(IConfigDataStream dataStream) : base(dataStream) { }

        protected override Dictionary<K2, V2> ReadItemValue(Stream stream, K1 key) {
            Dictionary<K2, V2> ret = null;
            var indexFile = this.IndexFile;
            IndexFileData data;
            if (!indexFile.FindIndexData(key, out data) || !data.IsVaild)
                return ret;
            ret = new Dictionary<K2, V2>();
            for (int i = 0; i < data.Count; ++i) {
                V2 config = Activator.CreateInstance<V2>();
                config.stream = stream;
                K2 k2 = config.ReadKey();
                config.ReadValue();
                ret[k2] = config;
            }

            return ret;
        }
    }


}