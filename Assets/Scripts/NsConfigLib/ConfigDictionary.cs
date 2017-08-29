using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace NsLib.Config {

    // 配置文件定义的字典基础类，主要用于重载方法，不影响使用
    // 可以使用ContainsKey方法，可以使用[]
    public class ConfigDictionary<T> : Dictionary<string, T> {
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

        public static Dictionary<K, List<V>> ToWrap<K, V>(TextAsset asset, bool isLoadAll = false) where V : ConfigBase<K> {
            if (asset == null)
                return null;

            Dictionary<K, List<V>> ret = ConfigWrap.ToObjectList<K, V>(asset.bytes, isLoadAll);
            if (ret == null) {
                try {
                    ret = JsonMapper.ToObject<Dictionary<K, V>>(asset.text);
                } catch {
                    ret = null;
                }
            }
            return ret;
        }
    }

}