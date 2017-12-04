using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utils;

namespace NsLib.Config {

    public struct IndexFileData {

        public long Offset {
            get;
            set;
        }

        public int Index {
            get;
            set;
        }

        public bool IsVaild {
            get {
                return (Count > 0) && (Index >= 0) && (Offset >= 0);
            }
        }

        public int Count {
            get;
            set;
        }
    }

    // 索引文件
    public class ConfigIndexFile<K> {
        private Dictionary<K, IndexFileData> m_IndexDataMap = null;
        private ConfigWrap.ConfigValueType m_ValueType = ConfigWrap.ConfigValueType.cvNone;

        public Dictionary<K, IndexFileData>.Enumerator GetIter() {
            if (m_IndexDataMap == null)
                return new Dictionary<K, IndexFileData>.Enumerator();
            return m_IndexDataMap.GetEnumerator();
        }

        public bool FindIndexData(K key, out IndexFileData data) {
            if (m_IndexDataMap == null || m_IndexDataMap.Count <= 0) {
                data = new IndexFileData();
                data.Offset = -1;
                data.Index = -1;
                data.Count = 0;
                return false;
            }
            return m_IndexDataMap.TryGetValue(key, out data);
        }

        public ConfigWrap.ConfigValueType ValueType {
            get {
                return m_ValueType;
            }
        }

        protected Dictionary<K, IndexFileData> IndexDataMap {
            get {
                if (m_IndexDataMap == null)
                    m_IndexDataMap = new Dictionary<K, IndexFileData>();
                return m_IndexDataMap;
            }
        }

        protected virtual K ReadKey(Stream stream) {
            System.Object value = FilePathMgr.GetInstance().ReadObject(stream, typeof(K));
            return (K)value;
        }

        private void LoadObjectIndex(ConfigFileHeader header, Stream stream) {
            var map = this.IndexDataMap;
            for (int i = 0; i < header.Count; ++i) {
                K key = ReadKey(stream);
                long offset = FilePathMgr.GetInstance().ReadLong(stream);
                int index = FilePathMgr.GetInstance().ReadInt(stream);
                IndexFileData data = new IndexFileData();
                data.Index = index;
                data.Offset = offset;
                data.Count = 1;
                m_IndexDataMap[key] = data;
            }
        }

        private void LoadListIndex(ConfigFileHeader header, Stream stream) {
            for (int i = 0; i < header.Count; ++i) {
                K key = ReadKey(stream);
                long offset = FilePathMgr.GetInstance().ReadLong(stream);
                int cnt = FilePathMgr.GetInstance().ReadInt(stream);
                int index = FilePathMgr.GetInstance().ReadInt(stream);
                IndexFileData data = new IndexFileData();
                data.Index = index;
                data.Offset = offset;
                data.Count = cnt;
                if (m_IndexDataMap == null)
                    m_IndexDataMap = new Dictionary<K, IndexFileData>();
                m_IndexDataMap[key] = data;
            }
        }

        private void LoadMapIndex(ConfigFileHeader header, Stream stream) {
            for (int i = 0; i < header.Count; ++i) {
                K key = ReadKey(stream);
                long offset = FilePathMgr.GetInstance().ReadLong(stream);
                int cnt = FilePathMgr.GetInstance().ReadInt(stream);
                int index = FilePathMgr.GetInstance().ReadInt(stream);
                IndexFileData data = new IndexFileData();
                data.Index = index;
                data.Offset = offset;
                data.Count = cnt;
                if (m_IndexDataMap == null)
                    m_IndexDataMap = new Dictionary<K, IndexFileData>();
                m_IndexDataMap[key] = data;
            }
        }

        public bool LoadFromStream(Stream stream) {
            if (stream == null || !stream.CanRead)
                return false;

            ConfigFileHeader header = new ConfigFileHeader();
            header.SeekFileToHeader(stream);
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return false;

            if (!header.IsSplitFile)
                return false;

            // 索引偏移
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigWrap.ConfigValueType valueType = (ConfigWrap.ConfigValueType)stream.ReadByte();
            m_ValueType = valueType;

            switch (valueType) {
                case ConfigWrap.ConfigValueType.cvObject:
                    LoadObjectIndex(header, stream);
                    break;
                case ConfigWrap.ConfigValueType.cvList:
                    LoadListIndex(header, stream);
                    break;
                case ConfigWrap.ConfigValueType.cvMap:
                    LoadMapIndex(header, stream);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }

    public class ConfigStringKeyIndexFile: ConfigIndexFile<string> {
        protected override string ReadKey(Stream stream) {
            if (stream == null)
                return default(string);
            string key = FilePathMgr.GetInstance().ReadString(stream);
            return key;
        }
    }

    public class ConfigUIntKeyIndexFile : ConfigIndexFile<uint> {
        protected override uint ReadKey(Stream stream) {
            if (stream == null)
                return default(uint);
            uint key = (uint)FilePathMgr.Instance.ReadInt(stream);
            return key;
        }
    }

}
