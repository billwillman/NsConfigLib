using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NsLib.Config;

public class TaskTalkCfg: ConfigStringKey {
    [ConfigId(0)]
    public int id {
        get;
        set;
    }

    [ConfigId(1)]
    public int stepId {
        get;
        set;
    }

    [ConfigId(2)]
    public int npcId {
        get;
        set;
    }

    [ConfigId(3)]
    public int talkId {
        get;
        set;
    }

    [ConfigId(4)]
    public int talkNpcId {
        get;
        set;
    }
}