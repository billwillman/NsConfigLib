using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using NsLib.Config;
using System.IO;

public class TestListConfig: MonoBehaviour {
    private void Start() {
        TextAsset asset = Resources.Load<TextAsset>("TaskTalkCfg");
        if (asset == null)
            return;
        string str = asset.text;
        m_Map = JsonMapper.ToObject<Dictionary<string, List<TaskTalkCfg>>>(str);

        FileStream stream = new FileStream("Assets/Resources/task.bytes", FileMode.Create, FileAccess.Write);
        try {
            ConfigWrap.ToStream<string, TaskTalkCfg>(stream, m_Map);
        } finally {
            stream.Close();
            stream.Dispose();
        }
    }

    private Dictionary<string, List<TaskTalkCfg>> m_Map = null;
}