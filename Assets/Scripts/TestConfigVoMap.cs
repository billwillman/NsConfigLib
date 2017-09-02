using UnityEngine;
using System.Collections;
using NsLib.Config;

public class TestConfigVoMap : MonoBehaviour {

    private void OnGUI() {

        if (GUI.Button(new Rect(100, 100, 150, 50), "加载全部")) {
            TextAsset asset = Resources.Load<TextAsset>("TaskStepCfg_Binary");
            if (asset != null) {
                ConfigVoMapMap<string, string, TaskStepVO> maps = new ConfigVoMapMap<string, string, TaskStepVO>();
                maps.LoadFromTextAsset(asset, true);
            }
        }

        if (GUI.Button(new Rect(250, 100, 150, 50), "加载索引")) {
            TextAsset asset = Resources.Load<TextAsset>("TaskStepCfg_Binary");
            if (asset != null) {
                ConfigVoMapMap<string, string, TaskStepVO> maps = new ConfigVoMapMap<string, string, TaskStepVO>();
                maps.LoadFromTextAsset(asset, false);
            }
        }
    }
}
