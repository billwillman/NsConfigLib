using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace NsLib.Config {

    public static class ConfigDictionary {
        public static Dictionary<K, V> ToWrap<K, V>(TextAsset asset, out bool isJson, bool isLoadAll = false) where V: ConfigBase<K> {
            isJson = false;
            if (asset == null)
                return null;

            Dictionary<K, V>  ret = ConfigWrap.ToObject<K, V>(asset.bytes, isLoadAll);
            if (ret == null) {
                try {
                    ret = JsonMapper.ToObject<Dictionary<K, V>>(asset.text);
                    isJson = true;
                } catch {
                    ret = null;
                }
            }
            return ret;
        }

        // 预加载用
        public static void PreloadWrap<K, V>(Dictionary<K, V> maps, TextAsset asset,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {
            isJson = false;
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
                    isJson = true;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }
            
        }

        public static void PreloadWrap<K, V>(Dictionary<K, List<V>> maps, TextAsset asset,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {
            isJson = false;
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
                    isJson = true;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }

        }

        public static Dictionary<K, List<V>> ToWrapList<K, V>(TextAsset asset,
            out bool isJson,
            bool isLoadAll = false) where V : ConfigBase<K> {
            isJson = false;
            if (asset == null)
                return null;

            Dictionary<K, List<V>> ret = ConfigWrap.ToObjectList<K, V>(asset.bytes, isLoadAll);
            if (ret == null) {
                try {
                    ret = JsonMapper.ToObject<Dictionary<K, List<V>>>(asset.text);
                    isJson = true;
                } catch {
                    ret = null;
                }
            }
            return ret;
        }

        
        
    }

    public interface IConfigVoMap<K> {
        bool ContiansKey(K key);
        bool IsJson {
            get;
        }

        bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false);

        // 预加载
        bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd);
    }

    
    // 两个配置
    public class ConfigVoMap<K, V>: IConfigVoMap<K> where V: ConfigBase<K> {
        private bool m_IsJson = true;
        private Dictionary<K, V> m_Map = null;

        public bool IsJson {
            get {
                return m_IsJson;
            }
        }

        public bool ContiansKey(K key) {
            if (m_Map == null)
                return false;
            return m_Map.ContainsKey(key);
        }

        public bool TryGetValue(K key, out V value) {
            value = default(V);
            if (m_Map == null)
                return false;
            if (m_IsJson) {
                return m_Map.TryGetValue(key, out value);
            }
            if (!ConfigWrap.ConfigTryGetValue<K, V>(m_Map, key, out value)) {
                value = default(V);
                return false;
            }
            return true;
        }


        public bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false) {
            if (asset == null)
                return false;
            m_Map = ConfigDictionary.ToWrap<K, V>(asset, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        public V this[K key] {
            get {
                if (m_Map == null)
                    return default(V);
                V ret;
                if (m_IsJson) {
                    if (!m_Map.TryGetValue(key, out ret))
                        ret = default(V);
                    return ret;
                }
                if (!TryGetValue(key, out ret))
                    ret = default(V);
                return ret;
            }
        } 

        public bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (asset == null || mono == null)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, V>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, asset, mono, out m_IsJson, 
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }
    }

    public class ConfigVoListMap<K, V> : IConfigVoMap<K> where V : ConfigBase<K> {

        private bool m_IsJson = true;
        private Dictionary<K, List<V>> m_Map = null;

        public bool IsJson {
            get {
                return m_IsJson;
            }
        }

        public bool ContiansKey(K key) {
            if (m_Map == null)
                return false;
            return m_Map.ContainsKey(key);
        }

        public bool TryGetValue(K key, out List<V> value) {
            value = null;
            if (m_Map == null)
                return false;
            if (m_IsJson) {
                return m_Map.TryGetValue(key, out value);
            }
            if (!ConfigWrap.ConfigTryGetValue<K, V>(m_Map, key, out value)) {
                value = null;
                return false;
            }
            return true;
        }

        public List<V> this[K key] {
            get {
                if (m_Map == null)
                    return null;

                List<V> ret;
                if (m_IsJson) {
                    if (!m_Map.TryGetValue(key, out ret))
                        ret = null;
                    return ret;
                }
                if (!this.TryGetValue(key, out ret))
                    ret = null;
                return ret;
            }
        }

        public bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false) {
            if (asset == null)
                return false;
            m_Map = ConfigDictionary.ToWrapList<K, V>(asset, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        // 预加载
        public bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (asset == null || mono == null)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, List<V>>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, asset, mono, out m_IsJson,
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }
    }

}