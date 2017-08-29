﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NsLib.Utils;

// 配置文件库
namespace NsLib.Config {
    
    // 转换器
    public static class ConfigWrap {

        private enum ConfigValueType {
            cvObject = 0,
            cvList = 1,
            cvMap = 2
        }

        public static Dictionary<K, V> ToObject<K, V>(byte[] buffer, bool isLoadAll = false) where V: ConfigBase<K> {
            Dictionary<K, V> ret = null;
            if (buffer == null || buffer.Length <= 0)
                return ret;

            MemoryStream stream = new MemoryStream(buffer);
            ret = ToObject<K, V>(stream, isLoadAll);

            return ret;
        }

        private static V ReadItem<K, V>(Dictionary<K, V> maps, K key) where V : ConfigBase<K> {
            if (maps == null || maps.Count <= 0)
                return null;
            V config;
            if (!maps.TryGetValue(key, out config) || config == null)
                return null;
            if (config.IsReaded)
                return config;
            if (!config.StreamSeek ())
                return null;
            
            bool ret = config.ReadValue();
            if (!ret)
                return null;
            return config;
        }

        private static List<V> ReadItem<K, V>(Dictionary<K, List<V>> maps, K key) where V : ConfigBase<K> {
            if (maps == null || maps.Count <= 0)
                return null;
            List<V> vs;
            if (!maps.TryGetValue(key, out vs) || vs == null || vs.Count <= 0)
                return null;
            V config = vs [0];
            if (config == null)
                return null;
            
            if (config.IsReaded)
                return vs;
            if (!config.StreamSeek ())
                return null;

            bool ret = config.ReadValue ();
            if (!ret)
                return null;
            
            for (int i = 1; i < vs.Count; ++i) {
                config = vs [i];
                ret = config.ReadValue ();
                if (!ret)
                    return null;
            }
            return vs;
        }

        public static bool ConfigTryGetValue<K, V>(this Dictionary<K, V> maps, K key, out V value) where V : ConfigBase<K> {
            value = default(V);
            if (maps == null || maps.Count <= 0)
                return false;
            value = ReadItem(maps, key);
            return value != null;
        }

        public static bool ConfigTryGetValue<K, V>(this Dictionary<K, List<V>> maps, K key, out List<V> values) where V : ConfigBase<K>
        {
            values = null;
            if (maps == null || maps.Count <= 0)
                return false;
            values = ReadItem (maps, key);
            return values != null;
        }

        private static UnityEngine.Coroutine StartLoadAllCortine<K, V>(Dictionary<K, V> maps, UnityEngine.MonoBehaviour parent) where V : ConfigBase<K>
        {
            if (maps == null || maps.Count <= 0)
                return null;
            if (parent != null) {
                return parent.StartCoroutine(StartLoadCortine<K, V>(maps));
            } else {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    V config = iter.Current.Value;
                    Stream stream = config.stream;
                    stream.Seek(config.dataOffset, SeekOrigin.Begin);
                    config.ReadValue();
                }
                iter.Dispose();
            }

            return null;
        }

        private  static UnityEngine.Coroutine StartLoadAllCortine<K, V>(Dictionary<K, List<V>> maps, UnityEngine.MonoBehaviour parent) where V : ConfigBase<K>
        {
            if (maps == null || maps.Count <= 0)
                return null;

            if (parent != null) {
                return parent.StartCoroutine (StartLoadCortine<K, V> (maps));
            } else {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    List<V> vs = iter.Current.Value;
                    V v = vs[0];
                    Stream stream = v.stream;
                    stream.Seek(v.dataOffset, SeekOrigin.Begin);
                    for (int i = 0; i < vs.Count; ++i) {
                        v = vs[i];
                        v.ReadValue();
                    }
                }
                iter.Dispose();
            }

            return null;
        }

        private static UnityEngine.WaitForEndOfFrame m_EndFrame = null;
        private static void InitEndFrame()
        {
            if (m_EndFrame == null)
                m_EndFrame = new UnityEngine.WaitForEndOfFrame ();
        }
        private static IEnumerator StartLoadCortine<K, V>(Dictionary<K, V> maps) where V : ConfigBase<K>
        {
            if (maps == null || maps.Count <= 0)
                yield break;

            var iter = maps.GetEnumerator();
            while (iter.MoveNext()) {
                V config = iter.Current.Value;
                Stream stream = config.stream;
                stream.Seek(config.dataOffset, SeekOrigin.Begin);
                config.ReadValue();
                InitEndFrame ();
                yield return m_EndFrame;
            }
            iter.Dispose();
        }

        private static IEnumerator StartLoadCortine<K, V>(Dictionary<K, List<V>> maps) where V : ConfigBase<K>
        {
            if (maps == null || maps.Count <= 0)
                yield break;
            var iter = maps.GetEnumerator();
            while (iter.MoveNext()) {
                List<V> vs = iter.Current.Value;
                V v = vs[0];
                Stream stream = v.stream;
                stream.Seek(v.dataOffset, SeekOrigin.Begin);
                for (int i = 0; i < vs.Count; ++i) {
                    v = vs[i];
                    v.ReadValue();
                }
                InitEndFrame ();
                yield return m_EndFrame;
            }
            iter.Dispose();
        }

        private static IEnumerator _ToObjectAsync<K, V>(Stream stream, Dictionary<K, V> maps, bool isLoadAll = false) where V: ConfigBase<K>
        {
            if (stream == null || maps == null || maps.Count <= 0)
                yield break;
            maps.Clear ();
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                yield break;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvObject)
                yield break;


            for (uint i = 0; i < header.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                config.stream = stream;
                K key = config.ReadKey();
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>((int)header.Count);
                maps[key] = config;

                InitEndFrame ();
                yield return m_EndFrame;
            }

            yield return StartLoadCortine<K, V> (maps);
        }

        public static UnityEngine.Coroutine ToObjectAsync<K, V>(Stream stream, 
            Dictionary<K, V> maps, UnityEngine.MonoBehaviour mono, bool isLoadAll = false) where V : ConfigBase<K>
        {
            if (stream == null || maps == null || mono == null)
                return null;
           
            return mono.StartCoroutine(_ToObjectAsync<K, V>(stream, maps, isLoadAll));
        }

        // 首次读取
        public static Dictionary<K, V> ToObject<K, V>(Stream stream, bool isLoadAll = false, 
            UnityEngine.MonoBehaviour loadAllCortine = null) where V : ConfigBase<K> {
            if (stream == null)
                return null;
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return null;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvObject)
                return null;

            Dictionary<K, V> maps = null;
            for (uint i = 0; i < header.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                config.stream = stream;
                K key = config.ReadKey();
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>((int)header.Count);
                maps[key] = config;
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine<K, V> (maps, loadAllCortine);
            }

            return maps;
        }


        private static IEnumerator _ToObjectListAsync<K, V>(Stream stream, 
            Dictionary<K, List<V>> maps, bool isLoadAll = false) where V : ConfigBase<K>
        {
            
            if (stream == null || maps == null)
                yield break;

            maps.Clear ();

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream (stream) || !header.IsVaild)
                yield break;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvList)
                yield break;


            for (uint i = 0; i < header.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                config.stream = stream;
                K key = config.ReadKey();
                long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                config.dataOffset = dataOffset;
                int listCnt = FilePathMgr.Instance.ReadInt(stream);
                if (maps == null)
                    maps = new Dictionary<K, List<V>>((int)header.Count);
                List<V> vs = new List<V>(listCnt);
                maps[key] = vs;
                vs.Add(config);
                for (int j = 1; j < listCnt; ++j) {
                    config = Activator.CreateInstance<V>();
                    config.stream = stream;
                    config.dataOffset = dataOffset;
                    vs.Add(config);
                }
                InitEndFrame ();
                yield return m_EndFrame;
            }

            if (isLoadAll && maps.Count > 0) {
                yield return StartLoadCortine<K, V> (maps);
            }
        }

        public static UnityEngine.Coroutine ToObjectListAsync<K, V>(Stream stream, 
            Dictionary<K, List<V>> maps, UnityEngine.MonoBehaviour mono, bool isLoadAll = false) where V : ConfigBase<K>
        {
            if (stream == null || maps == null || mono == null)
                return null;
            return mono.StartCoroutine(_ToObjectListAsync<K, V>(stream, maps, isLoadAll));
        }

        public static Dictionary<K, List<V>> ToObjectList<K, V>(Stream stream, bool isLoadAll = false, 
            UnityEngine.MonoBehaviour loadAllCortine = null) where V : ConfigBase<K> {

            if (stream == null)
                return null;
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return null;
            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvList)
                return null;

            Dictionary<K, List<V>> maps = null;
            for (uint i = 0; i < header.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                config.stream = stream;
                K key = config.ReadKey();
                long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                config.dataOffset = dataOffset;
                int listCnt = FilePathMgr.Instance.ReadInt(stream);
                if (maps == null)
                    maps = new Dictionary<K, List<V>>((int)header.Count);
                List<V> vs = new List<V>(listCnt);
                maps[key] = vs;
                vs.Add(config);
                for (int j = 1; j < listCnt; ++j) {
                    config = Activator.CreateInstance<V>();
                    config.stream = stream;
                    config.dataOffset = dataOffset;
                    vs.Add(config);
                }
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine<K, V> (maps, loadAllCortine);
            }


            return maps;
        }

        internal static bool ToStream(Stream stream, System.Collections.IDictionary values) {
            if (stream == null || values == null || values.Count <= 0)
                return false;

            ConfigFileHeader header = new ConfigFileHeader((uint)values.Count, 0);
            header.SaveToStream(stream);

            var iter = values.GetEnumerator();
            ConfigValueType valueType = ConfigValueType.cvObject;
            while (iter.MoveNext()) {
                IList vs = iter.Value as IList;
                if (vs != null) {
                    // 说明是listMode
                    valueType = ConfigValueType.cvList;
                    long dataOffset = stream.Position;
                    for (int i = 0; i < vs.Count; ++i) {
                        IConfigBase v = vs[i] as IConfigBase;
                        v.stream = stream;
                        v.dataOffset = dataOffset;
                        v.WriteValue();
                    }
                } else {
                    valueType =  ConfigValueType.cvObject;
                    IConfigBase v = iter.Value as IConfigBase;
                    v.stream = stream;
                    v.dataOffset = stream.Position;
                    v.WriteValue();
                }
            }


            long indexOffset = stream.Position;
            stream.WriteByte((byte)valueType);

            if (valueType == ConfigValueType.cvList) {
                iter = values.GetEnumerator();
                while (iter.MoveNext()) {
                    System.Object key = iter.Key;
                    IList vs = iter.Value as IList;
                    if (vs != null) {
                        IConfigBase v = vs[0] as IConfigBase;
                        v.WriteKey(key);
                        // 偏移
                        FilePathMgr.Instance.WriteLong(stream, v.dataOffset);
                        // 数量
                        FilePathMgr.Instance.WriteInt(stream, vs.Count);
                    }
                }
            } else if (valueType == ConfigValueType.cvObject) {
                iter = values.GetEnumerator();
                while (iter.MoveNext()) {
                    System.Object key = iter.Key;
                    IConfigBase v = iter.Value as IConfigBase;
                    v.WriteKey(key);
                    FilePathMgr.Instance.WriteLong(stream, v.dataOffset);
                }
            }

            // 重写Header
            header.indexOffset = indexOffset;
            header.SeekFileToHeader(stream);
            header.SaveToStream(stream);

            return true;
        }

        public static bool ToStream<K, V>(Stream stream, Dictionary<K, List<V>> values) where V : ConfigBase<K> {
            if (stream == null || values == null || values.Count <= 0)
                return false;

            ConfigFileHeader header = new ConfigFileHeader((uint)values.Count, 0);
            header.SaveToStream(stream);

            
            var iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                List<V> vs = iter.Current.Value;
                long dataOffset = stream.Position;
                for (int i = 0; i < vs.Count; ++i) {
                    V v = vs[i];
                    v.stream = stream;
                    v.dataOffset = dataOffset;
                    v.WriteValue();
                }
            }
            iter.Dispose();

            long indexOffset = stream.Position;
            // 是否是List
            FilePathMgr.Instance.WriteBool(stream, true);
            // 写入索引
            iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                K key = iter.Current.Key;
                List<V> vs = iter.Current.Value;
                vs[0].WriteKey(key);
                // 偏移
                FilePathMgr.Instance.WriteLong(stream, vs[0].dataOffset);
                // 数量
                FilePathMgr.Instance.WriteInt(stream, vs.Count);
            }
            iter.Dispose();

            // 重写Header
            header.indexOffset = indexOffset;
            header.SeekFileToHeader(stream);
            header.SaveToStream(stream);

            return true;
        }

        public static bool ToStream<K, V>(Stream stream, Dictionary<K, V> values) where V : ConfigBase<K> {
            if (stream == null || values == null || values.Count <= 0)
                return false;

            ConfigFileHeader header = new ConfigFileHeader((uint)values.Count, 0);
            header.SaveToStream(stream);

            var iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                V v = iter.Current.Value;
                v.stream = stream;
                v.dataOffset = stream.Position;
                v.WriteValue();
            }
            iter.Dispose();

            long indexOffset = stream.Position;

            // 是否是List
            FilePathMgr.Instance.WriteBool(stream, false);
            // 写入索引
            iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                K key = iter.Current.Key;
                V v = iter.Current.Value;
                v.WriteKey(key);
                FilePathMgr.Instance.WriteLong(stream, v.dataOffset);
            }
            iter.Dispose();

            // 重写Header
            header.indexOffset = indexOffset;
            header.SeekFileToHeader(stream);
            header.SaveToStream(stream);

            return true;
        }


    }
}