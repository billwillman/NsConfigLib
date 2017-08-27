using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Reflection;

namespace NsLib.Config
{
    public class ConfigAssetImporter : AssetPostprocessor {
        
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths) {
            if (importedAsset != null && importedAsset.Length > 0) {
                if (!IsContainConfigFiles(importedAsset))
                    return;

                // 初始化构建
                TestBuildConfigConvertMap();
                for (int i = 0; i < importedAsset.Length; ++i) {
                    string assetFileName = importedAsset [i];
                    string ext = Path.GetExtension (assetFileName);
                    if (string.Compare (ext, ".txt") == 0) {
                        ProcessConfigConvert (assetFileName);
                    }
                }
            }
        }

        private static bool IsContainConfigFiles(string[] importedAsset) {
            if (importedAsset == null || importedAsset.Length <= 0)
                return false;
            for (int i = 0; i < importedAsset.Length; ++i) {
                string fileName = importedAsset[i];
                if (string.IsNullOrEmpty(fileName))
                    continue;
                string ext = Path.GetExtension(fileName);
                if (string.Compare(ext, ".txt", true) == 0)
                    return true;
            }
            return false;
        }


        private static void ProcessConfigConvert(string fileName)
        {
            string configName = Path.GetFileNameWithoutExtension (fileName);
            if (string.IsNullOrEmpty(configName))
                return;
            // 转换为二进制文件
            FileStream srcStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try {
                if (srcStream.Length <= 0)
                    return;
                byte[] buffer = new byte[srcStream.Length];
                if (buffer == null || buffer.Length <= 0)
                    return;
                srcStream.Read(buffer, 0, buffer.Length);
                string json = System.Text.Encoding.UTF8.GetString(buffer);
                if (string.IsNullOrEmpty(json))
                    return;
                ConfigConvertManager.ConvertToBinaryFile(fileName, configName, json);
            } finally {
                srcStream.Close();
                srcStream.Dispose();
            }
        }

        [MenuItem("Tools/测试配合转换表生成")]
        public static void TestBuildConfigConvertMap() {
            ConfigConvertManager.BuildConfigConvert();
        }

    }

}
