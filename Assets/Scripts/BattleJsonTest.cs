using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BattleActCmd {
    public string mark {
        get;
        set;
    }

    public int code {
        get;
        set;
    }

    public int[] cmds {
        get;
        set;
    }

    public BattleBufCmd[] bufs {
        get;
        set;
    }
}

public class BattleBufCmd {
    public string mark {
        get;
        set;
    }

    public int code {
        get;
        set;
    }

    public int[] vals {
        get;
        set;
    }
}

public class BattleRoundData {
    public int round {
        get;
        set;
    }

    public BattleActCmd[] acts {
        get;
        set;
    }
}

public enum JsonDllType {
    LitJson,
    NewtonJson,
    fastJson
}

public class BattleJsonTest : MonoBehaviour {
    private BattleRoundData m_RoundData = null;

    private void Awake() {
        /*
        m_FileNameList = Directory.GetFiles("Assets/Resources/round_3_14", "*.json", SearchOption.TopDirectoryOnly);
        if (m_FileNameList != null) {
            for (int i = 0; i < m_FileNameList.Length; ++i) {
                string fileName = m_FileNameList[i];
                // Assets/
                fileName = fileName.Substring(17).Replace('\\', '/');
                string dir = Path.GetDirectoryName(fileName);
                fileName = Path.GetFileNameWithoutExtension(fileName);
                fileName = string.Format("{0}/{1}", dir, fileName);
                m_FileNameList[i] = fileName;
            }
        }
        */
    }

    private void LoadJsonFromFile(string fileName, JsonDllType dllType) {
        if (string.IsNullOrEmpty(fileName))
            return;

        var asset = Resources.Load<TextAsset>(fileName);
        if (asset == null)
            return;
        string text = asset.text;
        LoadJsonData(text, dllType);
    }

    private void LoadJsonData(string text, JsonDllType dllType) {
        if (string.IsNullOrEmpty(text))
            return;
        Profiler.BeginSample("BattleJsonTest");
        try {
            try {
                switch (dllType) {
                    case JsonDllType.LitJson:
                        m_RoundData = LitJson.JsonMapper.ToObject<BattleRoundData>(text);
                        break;
                    case JsonDllType.NewtonJson:
                        m_RoundData = Newtonsoft.Json.JsonConvert.DeserializeObject<BattleRoundData>(text);
                        break;
                    case JsonDllType.fastJson:
                        m_RoundData = fastJSON.JSON.ToObject<BattleRoundData>(text);
                        break;
                }
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
        } finally {
            Profiler.EndSample();
        }
        
    }

    private void OnGUI() {
        if (GUILayout.Button("LitJson")) {
            CheckTestFileName();
            LoadJsonFromFile(m_TestFileName, JsonDllType.LitJson);
        }

        if (GUILayout.Button("NewtonJson")) {
            CheckTestFileName();
            LoadJsonFromFile(m_TestFileName, JsonDllType.NewtonJson);
        }
        if (GUILayout.Button("fastJson")) {
            CheckTestFileName();
            LoadJsonFromFile(m_TestFileName, JsonDllType.fastJson);
        }
        if (GUILayout.Button("随机配置")) {
            m_TestFileName = string.Empty;
            CheckTestFileName();
        }

        if (!string.IsNullOrEmpty(m_TestFileName)) {
            m_TestFileName = GUILayout.TextField(m_TestFileName);
        }
    }

    private void CheckTestFileName() {
        if (!string.IsNullOrEmpty(m_TestFileName))
            return;
        if (m_FileNameList == null || m_FileNameList.Length <= 0)
            return;
        var r = new System.Random();
        int idx = r.Next(0, m_FileNameList.Length - 1);
        m_TestFileName = m_FileNameList[idx];
        if (!string.IsNullOrEmpty(m_TestFileName))
            Resources.Load<TextAsset>(m_TestFileName);
    }

    private string m_TestFileName = string.Empty;
    private string[] m_FileNameList = {
        "round_3_14/round_3_15",
        "round_3_14/round_3_9",
        "round_3_14/round_3_3",
        "round_3_14/round_3_16",
        "round_3_14/round_3_10",
        "round_3_14/round_3_1",
        "round_3_14/round_3_6",
        "round_3_14/round_3_17",
        "round_3_14/round_3_4",
        "round_3_14/round_3_11",
        "round_3_14/round_3_21",
        "round_3_14/round_3_18",
        "round_3_14/round_3_13",

        "round_3_14/round_3_24",
        "round_3_14/round_3_20",
        "round_3_14/round_3_12",
        "round_3_14/round_3_2",
        "round_3_14/round_3_22",
        "round_3_14/round_3_19",
        "round_3_14/round_3_14",

        "round_3_14/round_3_7",
        "round_3_14/round_3_23",
        "round_3_14/round_3_8",

        "round_3_14/round_3_25",
        "round_3_14/round_3_5",
        "round_3_14/round_3_91",
    };
}
