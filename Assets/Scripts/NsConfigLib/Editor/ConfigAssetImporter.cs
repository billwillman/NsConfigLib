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
            ConfigConvertManager.ConvertToBinaryFile(configName);
        }

        [MenuItem("Tools/测试配合转换表生成")]
        public static void TestBuildConfigConvertMap() {
            ConfigConvertManager.BuildConfigConvert();
        }

    }

}
