/*
 * 目前只支持三种Json的Dictionary数据
 * Dictionary<K, V> 
 * Dictionary<K, List<V>> 
 * Dictionary<K1, Dictionary<K2, V>>
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Utils;

// 配置文件库
namespace NsLib.Config {

    // 转换器
    public static class ConfigWrap {

        public enum ConfigValueType {
            cvObject = 0,
            cvList = 1,
            cvMap = 2
        }

        public static Dictionary<K, V> ToObject<K, V>(byte[] buffer, bool isLoadAll = false) where V : ConfigBase<K>, new() {
            Dictionary<K, V> ret = null;
            if (buffer == null || buffer.Length <= 0)
                return ret;

            MemoryStream stream = new MemoryStream(buffer);
            ret = ToObject<K, V>(stream, isLoadAll);
            if (ret == null) {
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            return ret;
        }

        // 首次读取
        public static Dictionary<K, V> ToObject<K, V>(Stream stream, bool isLoadAll = false,
            UnityEngine.MonoBehaviour loadAllCortine = null, Action<float> onProcess = null, int maxAsynReadCnt = 500) where V : ConfigBase<K>, new() {
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
                V config = new V();
                config.stream = stream;
                K key = config.ReadKey();
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>((int)header.Count);
                maps[key] = config;
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine(maps, loadAllCortine, valueType, onProcess, maxAsynReadCnt);
				if (loadAllCortine == null)
				{
					stream.Close();
					stream.Dispose();
					ConfigStringKey.ClearPropertys(typeof(V));
				}
            }

            return maps;
        }

        public static Dictionary<K, List<V>> ToObjectList<K, V>(byte[] buffer,
            bool isLoadAll = false) where V : ConfigBase<K>, new() {
            Dictionary<K, List<V>> ret = null;
            if (buffer == null || buffer.Length <= 0)
                return ret;

            MemoryStream stream = new MemoryStream(buffer);
            ret = ToObjectList<K, V>(stream, isLoadAll);
            if (ret == null) {
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

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
            if (!config.StreamSeek())
                return null;

            bool ret = config.ReadValue();
            if (!ret)
                return null;
            return config;
        }

        private static Dictionary<K2, V> ReadItem<K1, K2, V>(Dictionary<K1, Dictionary<K2, V>> maps,
            K1 key) where V : ConfigBase<K2> {
            if (maps == null || maps.Count <= 0)
                return null;
            Dictionary<K2, V> vs;
            if (!maps.TryGetValue(key, out vs) || vs == null || vs.Count <= 0)
                return null;
            var iter = vs.GetEnumerator();
            try {
                if (iter.MoveNext()) {
                    V config = iter.Current.Value;
                    if (config == null)
                        return null;
                    if (!config.IsReaded)
                    {
                        if (!config.StreamSeek())
                             return null;
                        bool ret = config.ReadValue();
                        if (!ret)
                            return null;
                    

                        while (iter.MoveNext()) {
                            config = iter.Current.Value;
                            if (config.IsReaded)
                                continue;
                            ret = config.ReadValue();
                            if (!ret)
                                return null;
                        }
                    }
                }

                return vs;
            } finally {
                iter.Dispose();
            }
        }

        private static List<V> ReadItem<K, V>(Dictionary<K, List<V>> maps, K key) where V : ConfigBase<K> {
            if (maps == null || maps.Count <= 0)
                return null;
            List<V> vs;
            if (!maps.TryGetValue(key, out vs) || vs == null || vs.Count <= 0)
                return null;
            V config = vs[0];
            if (config == null)
                return null;

            if (config.IsReaded)
                return vs;
            if (!config.StreamSeek())
                return null;

            bool ret = config.ReadValue();
            if (!ret)
                return null;

            for (int i = 1; i < vs.Count; ++i) {
                config = vs[i];
                if (config.IsReaded)
                    continue;
                ret = config.ReadValue();
                if (!ret)
                    return null;
            }
            return vs;
        }

        public static bool ConfigTryGetValue<K1, K2, V>(this Dictionary<K1, Dictionary<K2, V>> maps, K1 key, out Dictionary<K2, V> value) where V: ConfigBase<K2> {
            value = null;
            if (maps == null || maps.Count <= 0)
                return false;
            value = ReadItem(maps, key);
            return value != null;
        }

        public static bool ConfigTryGetValue<K, V>(this Dictionary<K, V> maps, K key, out V value) where V : ConfigBase<K> {
            value = default(V);
            if (maps == null || maps.Count <= 0)
                return false;
            value = ReadItem(maps, key);
            return value != null;
        }

        public static bool ConfigTryGetValue<K, V>(this Dictionary<K, List<V>> maps, K key, out List<V> values) where V : ConfigBase<K> {
            values = null;
            if (maps == null || maps.Count <= 0)
                return false;
            values = ReadItem(maps, key);
            return values != null;
        }

        private static void DisposeIter(this IDictionaryEnumerator iter) {
            IDisposable disp = iter as IDisposable;
            if (disp != null)
                disp.Dispose();
        }

        private static UnityEngine.Coroutine StartLoadAllCortine(IDictionary maps,
            UnityEngine.MonoBehaviour parent, ConfigValueType valueType, Action<float> onProcess, int maxAsyncReadCnt) {
            if (maps == null || maps.Count <= 0)
                return null;
            if (parent != null) {
                return parent.StartCoroutine(StartLoadCortine(maps, valueType, onProcess, maxAsyncReadCnt));
            } else {
                if (valueType == ConfigValueType.cvObject) {
                    var iter = maps.GetEnumerator();
                    while (iter.MoveNext()) {
                        IConfigBase config = iter.Value as IConfigBase;
                        Stream stream = config.stream;
                        if (stream == null)
                            continue;
                        stream.Seek(config.dataOffset, SeekOrigin.Begin);
                        config.ReadValue();
                    }
                    iter.DisposeIter();
                } else if (valueType == ConfigValueType.cvList) {
                    var iter = maps.GetEnumerator();
                    while (iter.MoveNext()) {
                        IList vs = iter.Value as IList;
                        IConfigBase v = vs[0] as IConfigBase;
                        Stream stream = v.stream;
                        if (stream == null)
                            continue;
                        stream.Seek(v.dataOffset, SeekOrigin.Begin);
                        for (int i = 0; i < vs.Count; ++i) {
                            v = vs[i] as IConfigBase;
                            v.ReadValue();
                        }
                    }
                    iter.DisposeIter();
                } else if (valueType == ConfigValueType.cvMap) {
                    // 字典类型
                    var iter = maps.GetEnumerator();
                    while (iter.MoveNext()) {
                        IDictionary map = iter.Value as IDictionary;
                        var subIter = map.GetEnumerator();
                        if (subIter.MoveNext()) {
                            IConfigBase v = subIter.Value as IConfigBase;
                            if (!v.StreamSeek ())
                                continue;
                            v.ReadValue();
                            while (subIter.MoveNext()) {
                                v = subIter.Value as IConfigBase;
                                v.ReadValue();
                            }
                        }
                        subIter.DisposeIter();
                    }
                    iter.DisposeIter();
                }
            }

            return null;
        }

        private static UnityEngine.WaitForEndOfFrame m_EndFrame = null;
        private static void InitEndFrame() {
            if (m_EndFrame == null)
                m_EndFrame = new UnityEngine.WaitForEndOfFrame();
        }

        private static bool DoLoadThreadAsync(IDictionary maps,
            ConfigValueType valueType, Action<float> onProcess, int maxAsyncReadCnt) {
            if (maps == null || maps.Count <= 0)
                return false;

            int curCnt = 0;
            int idx = 0;
            if (valueType == ConfigValueType.cvObject) {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IConfigBase config = iter.Value as IConfigBase;
                    if (!config.StreamSeek())
                        continue;
                    config.ReadValue();

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess(process);
                    }

                    ++curCnt;
                    if (curCnt >= maxAsyncReadCnt) {
                        curCnt = 0;
                        InitEndFrame();
                        System.Threading.Thread.Sleep(1);
                    }
                }
                iter.DisposeIter();
            } else if (valueType == ConfigValueType.cvList) {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IList vs = iter.Value as IList;
                    IConfigBase v = vs[0] as IConfigBase;
                    if (!v.StreamSeek())
                        continue;
                    for (int i = 0; i < vs.Count; ++i) {
                        v = vs[i] as IConfigBase;
                        v.ReadValue();

                        ++curCnt;
                        if (curCnt >= maxAsyncReadCnt) {
                            curCnt = 0;
                            InitEndFrame();
                            System.Threading.Thread.Sleep(1);
                        }
                    }

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess(process);
                    }

                }
                iter.DisposeIter();
            } else if (valueType == ConfigValueType.cvMap) {
                // 字典类型
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IDictionary map = iter.Value as IDictionary;
                    var subIter = map.GetEnumerator();
                    if (subIter.MoveNext()) {
                        IConfigBase v = subIter.Value as IConfigBase;
                        if (!v.StreamSeek())
                            continue;
                        v.ReadValue();
                        while (subIter.MoveNext()) {
                            v = subIter.Value as IConfigBase;
                            v.ReadValue();

                            ++curCnt;
                            if (curCnt >= maxAsyncReadCnt) {
                                curCnt = 0;
                                InitEndFrame();
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                    }
                    subIter.DisposeIter();

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess(process);
                    }

                }
                iter.DisposeIter();
            }

            return true;
        }

        private static IEnumerator StartLoadCortine(IDictionary maps,
            ConfigValueType valueType, Action<float> onProcess, int maxAsyncReadCnt) {
            if (maps == null || maps.Count <= 0)
                yield break;

            int curCnt = 0;
            int idx = 0;
            if (valueType == ConfigValueType.cvObject) {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IConfigBase config = iter.Value as IConfigBase;
                    if (!config.StreamSeek ())
                        continue;
                    config.ReadValue();

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess (process);
                    }

                    ++curCnt;
                    if (curCnt >= maxAsyncReadCnt) {
                        curCnt = 0;
                        InitEndFrame();
                        yield return m_EndFrame;
                    }
                }
                iter.DisposeIter();
            } else if (valueType == ConfigValueType.cvList) {
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IList vs = iter.Value as IList;
                    IConfigBase v = vs [0] as IConfigBase;
                    if (!v.StreamSeek ())
                        continue;
                    for (int i = 0; i < vs.Count; ++i) {
                        v = vs[i] as IConfigBase;
                        v.ReadValue();

                        ++curCnt;
                        if (curCnt >= maxAsyncReadCnt) {
                            curCnt = 0;
                            InitEndFrame();
                            yield return m_EndFrame;
                        }
                    }

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess (process);
                    }
                    
                }
                iter.DisposeIter();
            } else if (valueType == ConfigValueType.cvMap) {
                // 字典类型
                var iter = maps.GetEnumerator();
                while (iter.MoveNext()) {
                    IDictionary map = iter.Value as IDictionary;
                    var subIter = map.GetEnumerator();
                    if (subIter.MoveNext()) {
                        IConfigBase v = subIter.Value as IConfigBase;
                        if (!v.StreamSeek ())
                            continue;
                        v.ReadValue();
                        while (subIter.MoveNext()) {
                            v = subIter.Value as IConfigBase;
                            v.ReadValue();

                            ++curCnt;
                            if (curCnt >= maxAsyncReadCnt) {
                                curCnt = 0;
                                InitEndFrame();
                                yield return m_EndFrame;
                            }
                        }
                    }
                    subIter.DisposeIter();

                    if (onProcess != null) {
                        ++idx;
                        float process = 0.5f + (float)idx / (float)maps.Count;
                        onProcess (process);
                    }

                }
                iter.DisposeIter();
            }
        }

        // 多线程调用
        private static bool _ToObjectThreadAsync<K, V>(Stream stream, Dictionary<K, V> maps,
            bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 200) where V : ConfigBase<K>, new() {

            if (stream == null || maps == null)
                return false;

            maps.Clear();
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return false;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvObject) {
                return false;
            }

            int curCnt = 0;
            for (uint i = 0; i < header.Count; ++i) {
                V config = new V();
                config.stream = stream;
                K key = config.ReadKey();
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>((int)header.Count);
                maps[key] = config;

                ++curCnt;
                if (curCnt >= maxAsyncReadCnt) {
                    curCnt = 0;
                    InitEndFrame();
                    System.Threading.Thread.Sleep(1);
                }
            }

            bool ret = true;
            if (isLoadAll && maps.Count > 0) {
                ret = DoLoadThreadAsync(maps, valueType, onProcess, maxAsyncReadCnt);
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys(typeof(V));
            }

            if (onOK != null)
                onOK(maps);

            return ret;
        }

        private static IEnumerator _ToObjectAsync<K, V>(Stream stream, Dictionary<K, V> maps,
            bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {
            if (stream == null || maps == null) {
                yield break;
            }
            maps.Clear();
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                yield break;
            }

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvObject) {
                yield break;
            }

            int curCnt = 0;
            for (uint i = 0; i < header.Count; ++i) {
                V config = new V();
                config.stream = stream;
                K key = config.ReadKey();
                config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                if (maps == null)
                    maps = new Dictionary<K, V>((int)header.Count);
                maps[key] = config;

                ++curCnt;
                if (curCnt >= maxAsyncReadCnt) {
                    curCnt = 0;
                    InitEndFrame();
                    yield return m_EndFrame;
                }
            }

            if (isLoadAll && maps.Count > 0) {
                yield return StartLoadCortine(maps, valueType, onProcess, maxAsyncReadCnt);
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            if (onOK != null)
                onOK(maps);
        }

        public static bool ToObjectThreadAsync<K, V>(Stream stream,
            Dictionary<K, V> maps, bool isLoadAll = false,
            Action<IDictionary> onOk = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {
            if (stream == null || maps == null)
                return false;

            return _ToObjectThreadAsync<K, V>(stream, maps, isLoadAll, onOk, onProcess, maxAsyncReadCnt);
        }

        public static UnityEngine.Coroutine ToObjectAsync<K, V>(Stream stream,
            Dictionary<K, V> maps, UnityEngine.MonoBehaviour mono, bool isLoadAll = false,
            Action<IDictionary> onOk = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {
            if (stream == null || maps == null || mono == null)
                return null;

            return mono.StartCoroutine(_ToObjectAsync<K, V>(stream, maps, isLoadAll, onOk, onProcess, maxAsyncReadCnt));
        }

        public static bool GetConfigValueType(Stream stream, out ConfigValueType valueType) {
            valueType = ConfigValueType.cvObject;
            if (stream == null)
                return false;

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return false;

            stream.Seek(header.indexOffset, SeekOrigin.Begin);
            valueType = (ConfigValueType)stream.ReadByte();

            return true;
        }

        // 测试用
#if UNITY_EDITOR
        // 通用转换
        // 可以使用List<ConfigBase>或者ConfigBase
        public static Dictionary<K, V> TestCommonToObject<K, V>(byte[] buffer,
            bool isLoadAll = false,
            UnityEngine.MonoBehaviour loadAllCortine = null) where V : class, new() {

            MemoryStream stream = new MemoryStream(buffer);
            Dictionary<K, V> ret = TestCommonToObject<K, V>(stream, isLoadAll, loadAllCortine);
            if (ret == null) {
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            return ret;
        }

        public static IDictionary TestCommonToObject(byte[] buffer, System.Type configType,
            System.Type dictType, bool isLoadAll = false,
            UnityEngine.MonoBehaviour loadAllCortine = null) {
            if (buffer == null || buffer.Length <= 0 || configType == null || dictType == null)
                return null;
            MemoryStream stream = new MemoryStream(buffer);
            IDictionary ret = TestCommonToObject(stream, configType, dictType, isLoadAll,
                loadAllCortine);
            if (ret == null) {
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (configType);
            }
            return ret;
        }

        public static IDictionary TestCommonToObject(Stream stream, System.Type configType,
            System.Type dictType, bool isLoadAll = false,
            UnityEngine.MonoBehaviour loadAllCortine = null, int maxAsyncReadCnt = 500) {
            if (stream == null || configType == null || dictType == null)
                return null;

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                return null;
            }

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);
            // 读取类型(之前已经获取到了)
            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();

            IDictionary maps = null;
            switch (valueType) {
                case ConfigValueType.cvObject: {
                        for (uint i = 0; i < header.Count; ++i) {
                            IConfigBase config = Activator.CreateInstance(configType) as IConfigBase;
                            config.stream = stream;
                            System.Object key = config.ReadKEY();
                            config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                            if (maps == null)
                                maps = Activator.CreateInstance(dictType) as IDictionary;
                            maps[key] = config;
                        }
                        break;
                    }
                case ConfigValueType.cvList: {
                        var vsType = typeof(List<>).MakeGenericType(configType);
                        for (uint i = 0; i < header.Count; ++i) {
                            IConfigBase config = Activator.CreateInstance(configType) as IConfigBase;
                            config.stream = stream;
                            System.Object key = config.ReadKEY();
                            long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                            config.dataOffset = dataOffset;
                            int listCnt = FilePathMgr.Instance.ReadInt(stream);
                            if (maps == null)
                                maps = Activator.CreateInstance(dictType) as IDictionary;
                            IList list = Activator.CreateInstance(vsType) as IList;
                            maps[key] = list;
                            list.Add(config);
                            for (int j = 1; j < listCnt; ++j) {
                                config = Activator.CreateInstance(configType) as IConfigBase;
                                config.stream = stream;
                                config.dataOffset = dataOffset;
                                list.Add(config);
                            }
                        }
                        break;
                    }

                    // 有问题
                case ConfigValueType.cvMap: {
                        Type[] vTypes = dictType.GetInterfaces();
                        if (vTypes == null || vTypes.Length < 2)
                            return null;
                        Type k1 = vTypes[0];
                        if (k1 == null)
                            return null;

                        Type vType = vTypes[1];
                        if (vType == null)
                            return null;

                        if (!vType.IsSubclassOf(typeof(IDictionary)))
                            return null;

                        Type[] subTypes = vType.GetInterfaces();
                        if (subTypes == null || subTypes.Length < 2)
                            return null;

                        Type k2 = subTypes[0];
                        Type v = subTypes[1];
                        if (k2 == null || v == null)
                            return null;

                        var subDictType = typeof(Dictionary<System.Object, System.Object>).MakeGenericType(k2, v);
                        if (subDictType == null)
                            return null;

                        for (uint i = 0; i < header.Count; ++i) {

                        }

                        break;
                    }
                default:
                    return null;
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine(maps, loadAllCortine, valueType, null, maxAsyncReadCnt);
				if (loadAllCortine == null)
				{
					stream.Close();
					stream.Dispose();
					ConfigStringKey.ClearPropertys(configType);
				}
            }

            return maps;
        }


        public static Dictionary<K, V> TestCommonToObject<K, V>(Stream stream,
            bool isLoadAll = false,
            UnityEngine.MonoBehaviour loadAllCortine = null,
            int maxAsyncReadCnt = 500) where V: class, new() {

            if (stream == null)
                return null;

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                return null;
            }

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);
            // 读取类型(之前已经获取到了)
            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();

            Dictionary<K, V> maps = null;
            switch (valueType) {
                case ConfigValueType.cvObject: {
                        for (uint i = 0; i < header.Count; ++i) {
                            ConfigBase<K> config = new V() as ConfigBase<K>;
                            config.stream = stream;
                            K key = config.ReadKey();
                            config.dataOffset = FilePathMgr.Instance.ReadLong(stream);
                            if (maps == null)
                                maps = new Dictionary<K, V>((int)header.Count);
                            maps[key] = config as V;
                        }
                        break;
                    }
                case ConfigValueType.cvList: {
                        System.Type t = typeof(V);
                        // 这里有数组分配，不要频繁使用TestCommonToObject
                        var interfaces = t.GetInterfaces();
                        if (interfaces == null || interfaces.Length <= 0) {
                            return null;
                        }
                        var inter = interfaces[0];
                        if (inter == null) {
                            return null;
                        }
                        for (uint i = 0; i < header.Count; ++i) {
                            ConfigBase<K> config = Activator.CreateInstance(inter) as ConfigBase<K>;
                            config.stream = stream;
                            K key = config.ReadKey();
                            long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                            config.dataOffset = dataOffset;
                            int listCnt = FilePathMgr.Instance.ReadInt(stream);
                            if (maps == null)
                                maps = new Dictionary<K, V>((int)header.Count);
                            V vs = new V();
                            maps[key] = vs;
                            IList list = vs as IList;
                            list.Add(config);
                            for (int j = 1; j < listCnt; ++j) {
                                config = Activator.CreateInstance(inter) as ConfigBase<K>;
                                config.stream = stream;
                                config.dataOffset = dataOffset;
                                list.Add(config);
                            }
                        }
                        break;
                    }
                default:
                    return null;
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine(maps, loadAllCortine, valueType, null, maxAsyncReadCnt);
				if (loadAllCortine == null)
				{
					stream.Close();
					stream.Dispose();
					ConfigStringKey.ClearPropertys(typeof(V));
				}
            }

            return maps;
        }
#endif

        private static bool _ToObjectListThreadAsync<K, V>(Stream stream,
            Dictionary<K, List<V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {

            if (stream == null || maps == null) {
                return false;
            }

            maps.Clear();

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                return false;
            }

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvList) {
                return false;
            }

            int curCnt = 0;
            for (uint i = 0; i < header.Count; ++i) {
                V config = new V();
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
                    config = new V();
                    config.stream = stream;
                    config.dataOffset = dataOffset;
                    vs.Add(config);

                    ++curCnt;
                    if (curCnt >= maxAsyncReadCnt) {
                        curCnt = 0;
                        InitEndFrame();
                        System.Threading.Thread.Sleep(1);
                    }
                }

                if (onProcess != null) {
                    float delta = isLoadAll ? 0.5f : 1f;
                    float process = ((float)i / (float)header.Count) * delta;
                    onProcess(process);
                }

            }

            bool ret = true;
            if (isLoadAll && maps.Count > 0) {
                ret = DoLoadThreadAsync(maps, valueType, onProcess, maxAsyncReadCnt);
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys(typeof(V));
            }

            if (onOK != null)
                onOK(maps);

            return ret;
        }


        private static IEnumerator _ToObjectListAsync<K, V>(Stream stream, 
            Dictionary<K, List<V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {

            if (stream == null || maps == null) {
                yield break;
            }

            maps.Clear ();

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                yield break;
            }

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvList) {
                yield break;
            }

            int curCnt = 0;
            for (uint i = 0; i < header.Count; ++i) {
                V config = new V();
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
                    config = new V();
                    config.stream = stream;
                    config.dataOffset = dataOffset;
                    vs.Add(config);

                    ++curCnt;
                    if (curCnt >= maxAsyncReadCnt) {
                        curCnt = 0;
                        InitEndFrame();
                        yield return m_EndFrame;
                    }
                }

                if (onProcess != null) {
                    float delta = isLoadAll ? 0.5f : 1f;
                    float process = ((float)i / (float)header.Count) * delta;
                    onProcess (process);
                }
                
            }

            if (isLoadAll && maps.Count > 0) {
                yield return StartLoadCortine(maps, valueType,  onProcess, maxAsyncReadCnt) ;
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            if (onOK != null)
                onOK(maps);
        }

        public static UnityEngine.Coroutine ToObjectListAsync<K, V>(Stream stream,
            Dictionary<K, List<V>> maps, UnityEngine.MonoBehaviour mono, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, 
            int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {
            if (stream == null || maps == null || mono == null)
                return null;
            return mono.StartCoroutine(_ToObjectListAsync<K, V>(stream, maps, isLoadAll, onOK, onProcess, maxAsyncReadCnt));
        }

        public static bool ToObjectListThreadAsyncc<K, V>(Stream stream,
            Dictionary<K, List<V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null,
            int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {
            if (stream == null || maps == null)
                return false;
            return _ToObjectListThreadAsync<K, V>(stream, maps, isLoadAll, onOK, onProcess, maxAsyncReadCnt);
        }

        public static bool ToObjectMapThreadAsync<K1, K2, V>(Stream stream,
            Dictionary<K1, Dictionary<K2, V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K2>, new() {
            if (stream == null || maps == null)
                return false;
            return _ToObjectMapThreadAsync<K1, K2, V>(stream, maps, isLoadAll, onOK, onProcess, maxAsyncReadCnt);
        }

        public static UnityEngine.Coroutine ToObjectMapAsync<K1, K2, V>(Stream stream,
            Dictionary<K1, Dictionary<K2, V>> maps, UnityEngine.MonoBehaviour mono, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K2>, new() {
            if (stream == null || maps == null || mono == null)
                return null;
            return mono.StartCoroutine(_ToObjectMapAsync<K1, K2, V>(stream, maps, isLoadAll, onOK, onProcess, maxAsyncReadCnt));
        }

        private static bool _ToObjectMapThreadAsync<K1, K2, V>(Stream stream,
            Dictionary<K1, Dictionary<K2, V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K2>, new() {

            if (stream == null || maps == null) {
                return false;
            }

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                return false;
            }

            maps.Clear();

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvMap) {
                return false;
            }

            int curCnt = 0;
            System.Type keyType1 = typeof(K1);
            //   System.Type keyType2 = typeof(K2);
            //    System.Type subDictType = typeof(Dictionary<K2, V>);
            for (uint i = 0; i < header.Count; ++i) {
                System.Object key1 = FilePathMgr.Instance.ReadObject(stream, keyType1);
                long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                int DictCnt = FilePathMgr.Instance.ReadInt(stream);
                if (DictCnt > 0) {
                    Dictionary<K2, V> subMap = new Dictionary<K2, V>();
                    for (int j = 0; j < DictCnt; ++j) {
                        V config = new V();
                        config.stream = stream;
                        config.dataOffset = dataOffset;
                        K2 key2 = config.ReadKey();
                        subMap[(K2)key2] = config;

                        ++curCnt;
                        if (curCnt >= maxAsyncReadCnt) {
                            curCnt = 0;
                            InitEndFrame();
                            System.Threading.Thread.Sleep(1);
                        }
                    }


                    if (subMap != null && subMap.Count > 0) {
                        maps[((K1)key1)] = subMap;
                    }
                }
            }

            bool ret = true;
            if (isLoadAll && maps.Count > 0) {
                ret = DoLoadThreadAsync(maps, valueType, onProcess, maxAsyncReadCnt);
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys(typeof(V));
            }

            if (onOK != null)
                onOK(maps);
            return ret;
        }

        private static IEnumerator _ToObjectMapAsync<K1, K2, V>(Stream stream,
            Dictionary<K1, Dictionary<K2, V>> maps, bool isLoadAll = false,
            Action<IDictionary> onOK = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K2>, new() {
            if (stream == null || maps == null) {
                yield break;
            }

            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild) {
                yield break;
            }

            maps.Clear();

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvMap) {
                yield break;
            }

            int curCnt = 0;
            System.Type keyType1 = typeof(K1);
         //   System.Type keyType2 = typeof(K2);
        //    System.Type subDictType = typeof(Dictionary<K2, V>);
            for (uint i = 0; i < header.Count; ++i) {
                System.Object key1 = FilePathMgr.Instance.ReadObject(stream, keyType1);
                long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                int DictCnt = FilePathMgr.Instance.ReadInt(stream);
                if (DictCnt > 0) {
                    Dictionary<K2, V> subMap = new Dictionary<K2, V>();
                    for (int j = 0; j < DictCnt; ++j) {
                        V config = new V();
                        config.stream = stream;
                        config.dataOffset = dataOffset;
                        K2 key2 = config.ReadKey();
                        subMap[(K2)key2] = config;

                        ++curCnt;
                        if (curCnt >= maxAsyncReadCnt) {
                            curCnt = 0;
                            InitEndFrame();
                            yield return m_EndFrame;
                        }
                    }


                    if (subMap != null && subMap.Count > 0) {
                        maps[((K1)key1)] = subMap;
                    }
                }
            }

            if (isLoadAll && maps.Count > 0) {
                yield return StartLoadCortine(maps, valueType, onProcess, maxAsyncReadCnt);
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            if (onOK != null)
                onOK(maps);

        }

        public static Dictionary<K1, Dictionary<K2, V>> ToObjectMap<K1, K2, V>(byte[] buffer, bool isLoadAll = false,
           UnityEngine.MonoBehaviour loadAllCortine = null)
            where V : ConfigBase<K2>, new() {
            Dictionary<K1, Dictionary<K2, V>> ret = null;
            if (buffer == null || buffer.Length <= 0)
                return ret;
            MemoryStream stream = new MemoryStream(buffer);
            ret = ToObjectMap<K1, K2, V>(stream, isLoadAll, loadAllCortine);
            if (ret == null) {
                stream.Close();
                stream.Dispose();
                ConfigStringKey.ClearPropertys (typeof(V));
            }

            return ret;
        }


        // 转换成字典类型
        public static Dictionary<K1, Dictionary<K2, V>> ToObjectMap<K1, K2, V>(Stream stream, bool isLoadAll = false,
           UnityEngine.MonoBehaviour loadAllCortine = null,
           Action<float> onProcess = null, int maxAsyncReadCnt = 500) 
            where V : ConfigBase<K2>, new() {

            if (stream == null)
                return null;
            ConfigFileHeader header = new ConfigFileHeader();
            if (!header.LoadFromStream(stream) || !header.IsVaild)
                return null;

            // 读取索引
            stream.Seek(header.indexOffset, SeekOrigin.Begin);

            ConfigValueType valueType = (ConfigValueType)stream.ReadByte();
            if (valueType != ConfigValueType.cvMap)
                return null;

            Dictionary<K1, Dictionary<K2, V>> maps = null;
            System.Type keyType1 = typeof(K1);
          //  System.Type keyType2 = typeof(K2);
         //   System.Type subDictType = typeof(Dictionary<K2, V>);
            for (uint i = 0; i < header.Count; ++i) {
                System.Object key1 = FilePathMgr.Instance.ReadObject(stream, keyType1);
                long dataOffset = FilePathMgr.Instance.ReadLong(stream);
                int DictCnt = FilePathMgr.Instance.ReadInt(stream);
                if (DictCnt > 0) {
                    if (maps == null)
                        maps = new Dictionary<K1, Dictionary<K2, V>>();

                    Dictionary<K2, V> subMap = new Dictionary<K2, V>();
                    for (int j = 0; j < DictCnt; ++j) {
                        V config = new V();
                        config.stream = stream;
                        config.dataOffset = dataOffset;
                        K2 key2 = config.ReadKey();
                        subMap[(K2)key2] = config;
                    }


                    if (subMap != null && subMap.Count > 0) {
                        maps[((K1)key1)] = subMap;
                    }
                }
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine(maps, loadAllCortine, valueType, onProcess, maxAsyncReadCnt);
				if (loadAllCortine == null)
				{
					stream.Close();
					stream.Dispose();
					ConfigStringKey.ClearPropertys(typeof(V));
				}
            }

            return maps;
        }

        public static Dictionary<K, List<V>> ToObjectList<K, V>(Stream stream, bool isLoadAll = false, 
            UnityEngine.MonoBehaviour loadAllCortine = null, Action<float> onProcess = null, int maxAsyncReadCnt = 500) where V : ConfigBase<K>, new() {

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
                V config = new V();
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
                    config = new V();
                    config.stream = stream;
                    config.dataOffset = dataOffset;
                    vs.Add(config);
                }
            }

            if (isLoadAll && maps != null && maps.Count > 0) {
                StartLoadAllCortine(maps, loadAllCortine, valueType, onProcess, maxAsyncReadCnt);
				if (loadAllCortine == null)
				{
					stream.Close();
					stream.Dispose();
					ConfigStringKey.ClearPropertys(typeof(V));
				}
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
                IDictionary subMap = iter.Value as IDictionary;
                if (vs != null) {
                    // 说明是List
                    valueType = ConfigValueType.cvList;
                    long dataOffset = stream.Position;
                    for (int i = 0; i < vs.Count; ++i) {
                        IConfigBase v = vs[i] as IConfigBase;
                        v.stream = stream;
                        v.dataOffset = dataOffset;
                        v.WriteValue();
                    }
                } else if (subMap != null) {
                    // 字典类型
                    valueType = ConfigValueType.cvMap;
                    long dataOffset = stream.Position;
                    var subIter = subMap.GetEnumerator();
                    while (subIter.MoveNext()) {
                        IConfigBase v = subIter.Value as IConfigBase;
                        v.stream = stream;
                        v.dataOffset = dataOffset;
                        v.WriteValue();
                    }
                    subIter.DisposeIter();
                } else {
                    // 普通对象类型
                    valueType =  ConfigValueType.cvObject;
                    IConfigBase v = iter.Value as IConfigBase;
                    v.stream = stream;
                    v.dataOffset = stream.Position;
                    v.WriteValue();
                }
            }
            iter.DisposeIter();

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
                iter.DisposeIter();
            } else if (valueType == ConfigValueType.cvMap) {
                // 字典类型
                iter = values.GetEnumerator();
                while (iter.MoveNext()) {
                    System.Object key = iter.Key;
                    IDictionary vs = iter.Value as IDictionary;
                    if (vs != null) {
                        var subIter = vs.GetEnumerator();
                        // 取出第一个
                        if (subIter.MoveNext()) {
                            IConfigBase v = subIter.Value as IConfigBase;
                            FilePathMgr.Instance.WriteObject(stream, key, v.GetKeyType());
                            // 偏移
                            FilePathMgr.Instance.WriteLong(stream, v.dataOffset);
                            // 数量
                            FilePathMgr.Instance.WriteInt(stream, vs.Count);

                            System.Object key2 = subIter.Key;
                            v.WriteKey(key2);
                            while (subIter.MoveNext()) {
                                key2 = subIter.Key;
                                v.WriteKey(key2);
                            }
                        }
                        subIter.DisposeIter();
                    }
                }
                iter.DisposeIter();

            } else if (valueType == ConfigValueType.cvObject) {
                iter = values.GetEnumerator();
                while (iter.MoveNext()) {
                    System.Object key = iter.Key;
                    IConfigBase v = iter.Value as IConfigBase;
                    v.WriteKey(key);
                    FilePathMgr.Instance.WriteLong(stream, v.dataOffset);
                }
                iter.DisposeIter();
            }

            // 重写Header
            header.indexOffset = indexOffset;
            header.SeekFileToHeader(stream);
            header.SaveToStream(stream);

            return true;
        }

        public static bool ToStream<K, V>(Stream stream, Dictionary<K, V> values) where V : ConfigBase<K> {
            if (stream == null || values == null || values.Count <= 0)
                return false;
            IDictionary map = values as IDictionary;
            return ToStream(stream, map);
        }


    }
}
