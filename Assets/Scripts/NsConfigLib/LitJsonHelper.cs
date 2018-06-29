using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitJson {
    public static class LitJsonHelper {
        public static System.Object ToTypeObject(string json, System.Type type) {
            if (type == null || string.IsNullOrEmpty(json))
                return null;
            try {
                JsonReader reader = new JsonReader(json);
                // 需要把LitJson库的这个方法改成public访问
                return JsonMapper.ReadValue(type, reader);
            } catch (Exception e) {
                UnityEngine.Debug.LogError(e.ToString());
                return null;
            }
        }
    }
}
