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
                for (int i = 0; i < importedAsset.Length; ++i) {
                    string assetFileName = importedAsset [i];
                    string ext = Path.GetExtension (assetFileName);
                    if (string.Compare (ext, ".txt")) {
                        ProcessConfigConvert (assetFileName);
                    }
                }
            }
        }

        private static void InitConfigConverts()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            System.Type[] types = asm.GetTypes ();
            if (types == null || types.Length <= 0)
                return;
            for (int i = 0; i < types.Length; ++i) {
                System.Type t = types [i];

            }
        }


        private static void ProcessConfigConvert(string fileName)
        {
            string configName = Path.GetFileNameWithoutExtension (fileName);

        }

    }

   

}
