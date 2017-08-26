using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Utils;

// 配置文件库
namespace NsLib.Config {
    
    // 转换器
    public static class ConfigWrap {

        public static Dictionary<K, V> ToObject<K, V>(byte[] buffer, bool isLoadAll = false) where V: ConfigBase<K> {
            Dictionary<K, V> ret = null;
            if (buffer == null || buffer.Length <= 0)
                return ret;

            MemoryStream stream = new MemoryStream(buffer);
            ret = ToObject<K, V>(stream, isLoadAll);

            return ret;
        }

        public static bool ReadItem<K, V>(Stream stream, Dictionary<K, V> maps, K key) where V : ConfigBase<K> {
            if (stream == null || maps == null || maps.Count <= 0)
                return false;
            V config;
            if (!maps.TryGetValue(key, out config) || config == null)
                return false;
            return config.ReadValue(stream);
        }

        // 首次读取
        public static Dictionary<K, V> ToObject<K, V>(Stream stream, bool isLoadAll = false) where V : ConfigBase<K> {
            if (stream == null)
                return null;
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return null;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);
            Dictionary<K, V> maps = null;
            for (uint i = 0; i < header.Count; ++i) {
                V config = Activator.CreateInstance<V>();
                K key = config.ReadKey(stream);
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>();
                maps[key] = config;
            }

            if (isLoadAll && maps != null) {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    V config = iter.Current.Value;
                    stream.Seek(config.dataOffset, SeekOrigin.Begin);
                    config.ReadValue(stream);
                }
                iter.Dispose();
            }

            return maps;
        }

        public static bool ToStream<K, V>(Stream stream, Dictionary<K, V> values) where V : ConfigBase<K> {
            if (stream == null || values == null || values.Count <= 0)
                return false;

            ConfigFileHeader header = new ConfigFileHeader((uint)values.Count, 0);
            header.SaveToStream(stream);

            var iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                V v = iter.Current.Value;
                v.dataOffset = stream.Position;
                v.WriteValue(stream);
            }
            iter.Dispose();

            long indexOffset = stream.Position;

            // 写入索引
            iter = values.GetEnumerator();
            while (iter.MoveNext()) {
                K key = iter.Current.Key;
                V v = iter.Current.Value;
                v.WriteKey(stream, key);
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
