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
    private MemoryStream m_Stream = null;
    private void Start() {
        m_Text = Resources.Load<TextAsset>("TaskTalkCfg");

        /*
        string fileName = string.Format ("Assets/Resources/{0}.bytes", _cTaskListFileName);
        FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        try {
            ConfigWrap.ToStream<string, TaskTalkCfg>(stream, m_Map);
        } finally {
            stream.Close();
            stream.Dispose();
        }
        */

        m_Binary = Resources.Load<TextAsset> (_cTaskListFileName);
    }

    private Dictionary<string, List<TaskTalkCfg>> m_TaskDict = new Dictionary<string, List<TaskTalkCfg>>();

    void OnGUI()
    {
        if (m_Text == null)
            return;
        if (GUI.Button (new Rect(100, 100, 150, 50), "测试LitJson")) {
            string str = m_Text.text;
            LitJson.JsonMapper.ToObject<Dictionary<string, List<TaskTalkCfg>>>(str);
        }

        if (m_Binary != null) {
            if (GUI.Button (new Rect (100, 150, 150, 50), "测试二进制首次不全读取")) {
                m_Stream = new MemoryStream (m_Binary.bytes);
                var dict = ConfigWrap.ToObjectList<string, TaskTalkCfg> (m_Stream);
                List<TaskTalkCfg> list;
                if (dict.ConfigTryGetValue ("5", out list)) {
                }
            }

            if (GUI.Button (new Rect (250, 150, 150, 50), "二进制全部读取")) {
                m_Stream = new MemoryStream (m_Binary.bytes);
                ConfigWrap.ToObjectList<string, TaskTalkCfg> (m_Stream, true);
            }

            if (GUI.Button (new Rect (400, 150, 150, 50), "二进制全部读取携程")) {
                m_Stream = new MemoryStream (m_Binary.bytes);
                ConfigWrap.ToObjectList<string, TaskTalkCfg> (m_Stream, true, this);
            }

            if (GUI.Button (new Rect (550, 150, 150, 50), "二进制异步全读取")) {
                m_Stream = new MemoryStream (m_Binary.bytes);
                m_StartTime = Time.realtimeSinceStartup;
                ConfigWrap.ToObjectListAsync<string, TaskTalkCfg> (m_Stream, 
                    m_TaskDict, this, true, OnReadEnd);
            }

            if (GUI.Button (new Rect (700, 150, 150, 50), "二进制异步非全读取")) {
                m_Stream = new MemoryStream (m_Binary.bytes);
                m_StartTime = Time.realtimeSinceStartup;
                ConfigWrap.ToObjectListAsync<string, TaskTalkCfg> (m_Stream, 
                    m_TaskDict, this, false, OnReadEnd);
            }
        }
    }

    private float m_StartTime = 0f;
    private void OnReadEnd(IDictionary map) {
        float delta = Time.realtimeSinceStartup - m_StartTime;
        Debug.LogFormat("异步读取完成消耗：{0}", delta.ToString());
    }

  
    private static readonly string _cTaskListFileName = "TaskTalkCfg_Binary";
}