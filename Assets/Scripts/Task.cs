using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NsLib.Config;

[ConfigConvert("TaskTalkCfg", 
    typeof(Dictionary<string, List<TaskTalkCfg>>),
    "TaskTalkCfg_Binary")]
public class TaskTalkCfg : ConfigStringKey {
    [ConfigId(0)]
    public int id {
        get; set;
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

[ConfigConvert("TaskStepCfg", 
    typeof(Dictionary<string, Dictionary<string, TaskStepVO>>),
    "TaskStepCfg_Binary")]
public class TaskStepVO: ConfigStringKey {
    [ConfigId(0)]
    public int id { set; get; }
    [ConfigId(1)]
    public int stepId { set; get; }
    [ConfigId(2)]
    public int type { set; get; }
    [ConfigId(3)]
    public bool autoRun { set; get; }
    [ConfigId(4)]
    public int para1 { set; get; }
    [ConfigId(5)]
    public int para2 { set; get; }
    [ConfigId(6)]
    public int para3 { set; get; }
    [ConfigId(7)]
    public int para4 { set; get; }
    [ConfigId(8)]
    public int para5 { set; get; }
    [ConfigId(9)]
    public string para6 { set; get; }
    [ConfigId(10)]
    public string para7 { set; get; }
    [ConfigId(11)]
    public int limitTime { set; get; }
    [ConfigId(12)]
    public string intro { set; get; }
    [ConfigId(13)]
    public string trackIntro { set; get; }
    [ConfigId(14)]
    public int sendItemId { set; get; }
    [ConfigId(15)]
    public int sendItemNum { set; get; }
    [ConfigId(16)]
    public int recycleItemId { set; get; }
    [ConfigId(17)]
    public int recycleItemNum { set; get; }
    [ConfigId(18)]
    public int objectNpc { set; get; }
    public bool isNpcTask {
        get {
            return objectNpc > 0;
        }
    }
    [ConfigId(19)]
    public string gossip { set; get; }
}

[ConfigConvert("TaskCfg", typeof(Dictionary<string, TaskConfigVO>),
    "TaskCfg_Binary")]
public class TaskConfigVO: ConfigStringKey {
    [ConfigId(0)]
    public int id { set; get; }
    [ConfigId(1)]
    public string name { set; get; }
    [ConfigId(2)]
    public int preId { set; get; }
    [ConfigId(3)]
    public int allowLv { set; get; }
    [ConfigId(4)]
    public string allowJob { set; get; }
    [ConfigId(5)]
    public int type { set; get; }
    [ConfigId(6)]
    public int clientType { set; get; }
    [ConfigId(7)]
    public int countType { set; get; }
    [ConfigId(8)]
    public int count { set; get; }
    [ConfigId(9)]
    public int limitTime { set; get; }
    [ConfigId(10)]
    public int prizeId { set; get; }
    [ConfigId(11)]
    public int npcId { set; get; }
    [ConfigId(12)]
    public bool autoGet { set; get; }
    [ConfigId(13)]
    public int isDynamicNpc { set; get; }
    [ConfigId(14)]
    public int isGiveup { set; get; }
    [ConfigId(15)]
    public string taskdesc { get; set; }
    [ConfigId(16)]
    public string taskgoal { get; set; }
    [ConfigId(17)]
    public int activityID { get; set; }
}