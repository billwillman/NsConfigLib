using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NsLib.Config;

[ConfigConvert("TaskTalkCfg", true)]
public class TaskTalkCfg: ConfigStringKey {
    [ConfigId(0)]
    public int id {
        get;set;
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
    [ConfigId(5)]
    public string npcTalk {
        get;
        set;
    }
    [ConfigId(6)]
    public string answer1Text {
        get;
        set;
    }
    [ConfigId(7)]
    public int answer1Next {
        get;
        set;
    }
    [ConfigId(8)]
    public int answer1Action {
        get;
        set;
    }
    [ConfigId(9)]
    public string answer2Text {
        get;
        set;
    }
    [ConfigId(10)]
    public int answer2Next {
        get;
        set;
    }
    [ConfigId(11)]
    public int answer2Action {
        get;
        set;
    }
}