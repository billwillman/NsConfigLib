using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using NsLib.Config;
using System.IO;

public class TestListConfig: MonoBehaviour {

    private TextAsset m_Text = null;
    private TextAsset m_Binary = null;
    private void Start() {
        m_Text = Resources.Load<TextAsset>("TaskTalkCfg");
        if (m_Text == null)
            return;
        string str = m_Text.text;
        m_Map = JsonMapper.ToObject<Dictionary<string, List<TaskTalkCfg>>>(str);

        string fileName = string.Format ("Assets/Resources/{0}.bytes", _cTaskListFileName);
        FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        try {
            ConfigWrap.ToStream<string, TaskTalkCfg>(stream, m_Map);
        } finally {
            stream.Close();
            stream.Dispose();
        }

        m_Binary = Resources.Load<TextAsset> (_cTaskListFileName);
    }

    void OnGUI()
    {
        if (m_Text == null)
            return;
        if (GUI.Button (new Rect(100, 100, 100, 50), "测试LitJson")) {
            string str = m_Text.text;
            m_Map = JsonMapper.ToObject<Dictionary<string, List<TaskTalkCfg>>>(str);
        }

        if (m_Binary != null) {
            if (GUI.Button (new Rect (100, 150, 100, 50), "测试二进制首次不全读取")) {  
                MemoryStream stream = new MemoryStream (m_Binary.bytes);
                var dict = ConfigWrap.ToObjectList<string, TaskTalkCfg> (stream);
                List<TaskTalkCfg> list;
                if (dict.ConfigTryGetValue ("5", out list)) {
                }
            }

            if (GUI.Button (new Rect (200, 150, 100, 50), "二进制全部读取")) {
                MemoryStream stream = new MemoryStream (m_Binary.bytes);
                var dict = ConfigWrap.ToObjectList<string, TaskTalkCfg> (stream, true);
            }

            if (GUI.Button (new Rect (300, 150, 100, 50), "二进制全部读取携程")) {
                MemoryStream stream = new MemoryStream (m_Binary.bytes);
                var dict = ConfigWrap.ToObjectList<string, TaskTalkCfg> (stream, true, this);
            }
        }
    }

    private Dictionary<string, List<TaskTalkCfg>> m_Map = null;
    private static readonly string _cTaskListFileName = "taskList";
}