using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Reflection;
using System;

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
                        //ProcessConfigConvert(assetFileName, 50);
                    }
                }
            }
        }

        [MenuItem("Assets/生成拆分二进制配置表")]
        public static void ProcessConfigConvertCmd() {
            if (Selection.activeObject == null)
                return;
            string assetFileName = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetFileName))
                return;

            string ext = Path.GetExtension(assetFileName);
            if (string.Compare(ext, ".txt") != 0)
                return;

            TestBuildConfigConvertMap();
            ProcessConfigConvert(assetFileName, 50);
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


        private static void ProcessConfigConvert(string fileName, int maxSplitCnt = -1)
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
                if (maxSplitCnt <= 0)
                    ConfigConvertManager.ConvertToBinaryFile(fileName, configName, json);
                else
                    ConfigConvertManager.ConvertToBinarySplitFile(fileName, configName, json);
            } finally {
                srcStream.Close();
                srcStream.Dispose();
            }

            AssetDatabase.Refresh ();
        }

        [MenuItem("Tools/测试配合转换表生成")]
        public static void TestBuildConfigConvertMap() {
            ConfigConvertManager.BuildConfigConvert();
        }

        [MenuItem("Assets/配置读取测试", true)]
        public static bool IsCanReadConfig() {
            var selectObj = Selection.activeObject;
            if (selectObj == null)
                return false;
            string fileName = AssetDatabase.GetAssetPath(selectObj);
            if (string.IsNullOrEmpty(fileName))
                return false;
            string ext = Path.GetExtension(fileName);
            if (string.Compare(ext, ".bytes", true) != 0)
                return false;
            return (Selection.activeObject as TextAsset) != null;
        }


        [MenuItem("Assets/配置读取测试")]
        public static void TestReadConfig() {
            TextAsset asset = (Selection.activeObject as TextAsset);
            if (asset == null)
                return;
            byte[] buffer = asset.bytes;
            if (buffer == null || buffer.Length <= 0)
                return;
            
            string fileName = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(fileName))
                return;

            string configName = Path.GetFileNameWithoutExtension(fileName);
            
            ConfigConvertManager.BuildConfigConvert();
            var info = ConfigConvertManager.GetTargetConvert(configName);
            if (info == null)
                return;

            IDictionary map = ConfigWrap.TestCommonToObject(buffer, info.type, info.DictionaryType, true);
            if (map != null)
                Debug.Log("二进制配置读取正确");
            else
                Debug.LogError("二进制配置读取错误");
        } 
    }

}
