using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace NsLib.Config {

    public static class ConfigDictionary {
        public static Dictionary<K, V> ToWrap<K, V>(TextAsset asset, bool isLoadAll = false) where V: ConfigBase<K> {
            if (asset == null)
                return null;

            Dictionary<K, V>  ret = ConfigWrap.ToObject<K, V>(asset.bytes, isLoadAll);
            if (ret == null) {
                try {
                    ret = JsonMapper.ToObject<Dictionary<K, V>>(asset.text);
                } catch {
                    ret = null;
                }
            }
            return ret;
        }

        // 预加载用
        public static void PreloadWrap<K, V>(Dictionary<K, V> maps, TextAsset asset,
            MonoBehaviour mono,
            Action<Dictionary<K, V>> onEnd) where V : ConfigBase<K> {
            if (maps == null || asset == null || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            maps.Clear();

            MemoryStream stream = new MemoryStream(asset.bytes);


            Coroutine cor = ConfigWrap.ToObjectAsync<K, V>(stream, maps, mono, true, onEnd);
            if (cor == null) {
                stream.Close();
                stream.Dispose();

                Dictionary<K, V> ret;
                try {
                    maps = JsonMapper.ToObject<Dictionary<K, V>>(asset.text);
                    ret = maps;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }
            
        }

        public static void PreloadWrap<K, V>(Dictionary<K, List<V>> maps, TextAsset asset,
            MonoBehaviour mono,
            Action<Dictionary<K, List<V>>> onEnd) where V : ConfigBase<K> {

            if (maps == null || asset == null || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            maps.Clear();

            MemoryStream stream = new MemoryStream(asset.bytes);


            Coroutine cor = ConfigWrap.ToObjectListAsync<K, V>(stream, maps, mono, true, onEnd);
            if (cor == null) {
                stream.Close();
                stream.Dispose();

                Dictionary<K, List<V>> ret;
                try {
                    maps = JsonMapper.ToObject<Dictionary<K, List<V>>>(asset.text);
                    ret = maps;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }

        }

        public static Dictionary<K, List<V>> ToWrapList<K, V>(TextAsset asset, 
            bool isLoadAll = false) where V : ConfigBase<K> {
            if (asset == null)
                return null;

            Dictionary<K, List<V>> ret = ConfigWrap.ToObjectList<K, V>(asset.bytes, isLoadAll);
            if (ret == null) {
                try {
                    ret = JsonMapper.ToObject<Dictionary<K, List<V>>>(asset.text);
                } catch {
                    ret = null;
                }
            }
            return ret;
        }

        
        
    }

}