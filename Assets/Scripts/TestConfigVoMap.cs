using UnityEngine;
using System.Collections;
using NsLib.Config;
using LitJson;
using System.Collections.Generic;
using System.Net.Json;

public class TestConfigVoMap : MonoBehaviour {

    private float m_StartTime = 0f;
    private void OnReadEnd(IConfigVoMap<string> map) {
        float delta = Time.realtimeSinceStartup - m_StartTime;
        Debug.LogFormat("异步读取完成消耗：{0}", delta.ToString());

        AndroidJavaClass testClass = new AndroidJavaClass("abc");
        testClass.CallStatic("GSDKSaveFps", new object[] { "", -1, -1, -1, -1, -1, -1, -1, "-1" });
    }

    private static string _cJson = "TaskCfg";
    private static string _cBinary = "TaskCfg_Binary";

    private void OnGUI() {

        if (GUI.Button(new Rect(100, 100, 150, 50), "二进制加载全部")) {
            TextAsset asset = Resources.Load<TextAsset>(_cBinary);
            if (asset != null) {
                ConfigVoMapMap<string, string, TaskStepVO> maps = new ConfigVoMapMap<string, string, TaskStepVO>();
                maps.LoadFromTextAsset(asset, true);
            }
        }

        if (GUI.Button(new Rect(250, 100, 150, 50), "二进制加载索引")) {
            TextAsset asset = Resources.Load<TextAsset>(_cBinary);
            if (asset != null) {
                ConfigVoMapMap<string, string, TaskStepVO> maps = new ConfigVoMapMap<string, string, TaskStepVO>();
                maps.LoadFromTextAsset(asset, false);
            }
        }

        if (GUI.Button(new Rect(400, 100, 150, 50), "二进制预加载全部")) {
            TextAsset asset = Resources.Load<TextAsset>(_cBinary);
            if (asset != null) {

                ConfigVoMapMap<string, string, TaskStepVO> maps = new ConfigVoMapMap<string, string, TaskStepVO>();
                m_StartTime = Time.realtimeSinceStartup;
                maps.Preload(asset, this, OnReadEnd, null);
            }
        }

        if (GUI.Button(new Rect(100, 150, 150, 50), "LITJSON加载")) {
            TextAsset asset = Resources.Load<TextAsset>(_cJson);
            if (asset != null) {
                LitJson.JsonMapper.ToObject<Dictionary<string, Dictionary<string, TaskStepVO>>>(asset.text);
            }
        }

        if (GUI.Button(new Rect(250, 150, 150, 50), "FastJson加载")) {
            TextAsset asset = Resources.Load<TextAsset>(_cJson);
            if (asset != null) {
                fastJSON.JSON.ToObject<Dictionary<string, Dictionary<string, TaskStepVO>>>(asset.text);
            }
        }

        if (GUI.Button(new Rect(400, 150, 150, 50), "System.Net.Json测试")) {
            
            TextAsset asset = Resources.Load<TextAsset>(_cJson);
            if (asset != null) {
                JsonTextParser parser = new JsonTextParser();
                parser.Parse(asset.text);
            }
        }

        if (GUI.Button(new Rect(550, 150, 150, 50), "Newtonsoft.Json测试")) {
            TextAsset asset = Resources.Load<TextAsset>(_cJson);
            if (asset != null) {
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TaskStepVO>>>(asset.text);
            }
        }
            
    }
}
