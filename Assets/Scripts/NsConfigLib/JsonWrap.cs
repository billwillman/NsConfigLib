using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NsLib.Config
{
    public static class JsonMapper
    {
        public static T ToObject<T>(string json)
        {
            if (string.IsNullOrEmpty (json))
                return default(T);
            T ret;
            try
            {
                ret = JsonConvert.DeserializeObject<T> (json);
            } catch (Exception e) {
#if DEBUG
                Debug.LogError(e.ToString());
#endif
                ret = default(T);
            }

            return ret;
        }

    }
}
