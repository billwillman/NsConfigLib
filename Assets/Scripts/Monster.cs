using System;
using System.Collections.Generic;
using NsLib.Config;

[ConfigConvert("NpcCfg",
    typeof(Dictionary<string, MapMonsterVO>),
    "NpcCfg_Binary")]
[Serializable]
public class MapMonsterVO : ConfigStringKey, ICloneable {
    public Object Clone()  //实现ICloneable接口，达到浅表复制。浅表复制与深度复制无直接有关系。 对外提供一个创建自身的浅表副本的能力  
        {
        return this.MemberwiseClone();
    }
    private int m_id = 0;

    [ConfigId(0)]
    public int id {
        get {
            return m_id;
        }
        set {
            m_id = value;
        }
    }
    private long m_uuid = -1;
    public long uuid {
        get {
            return m_uuid == -1 ? id : m_uuid;
        }
        set {
            m_uuid = value;
        }
    }
    private int m_userid = 0;
    public int userid {
        get {
            return m_userid;
        }
        set {
            m_userid = value;
        }
    }
    private int m_objType = 0;
    public int objType {
        get {
            return m_objType;
        }
        set {
            m_objType = value;
        }
    }
    private string m_name = string.Empty;

    [ConfigId(1)]
    public string name {
        get {
            return m_name;
        }
        set {
            m_name = value;
        }
    }
    private string m_title = string.Empty;
    [ConfigId(2)]
    public string title {
        get {
            return m_title;
        }
        set {
            m_title = value;
        }
    }
    private string m_npcTalk = string.Empty;
    private string[] m_npcTalkArray;

    [ConfigId(11)]
    public string npcTalk {
        get;
        set;
    }
    private int m_model = 0;
    [ConfigId(3)]
    public int model {
        get {
            return m_model;
        }
        set {
            m_model = value;
        }
    }
    private int m_mapId = 0;

    [ConfigId(6)]
    public int mapId {
        get {
            return m_mapId;
        }
        set {
            m_mapId = value;
        }
    }
    private int m_x = 0;

    [ConfigId(7)]
    public int x {
        get {
            return m_x;
        }
        set {
            m_x = value;
        }
    }
    private int m_y = 0;

    [ConfigId(8)]
    public int y {
        get {
            return m_y;
        }
        set {
            m_y = value;
        }
    }
    private int m_direct = -1;

    [ConfigId(9)]
    public int direct {
        get {
            return m_direct;
        }
        set {
            m_direct = value;
        }
    }

    [ConfigId(15)]
    // 客户端选择
    public string function {
        get;
        set;
    }


    private int m_turn = 0;

    [ConfigId(10)]
    public int turn {
        get {
            return m_turn;
        }
        set {
            m_turn = value;
        }
    }
    private bool m_isStaticNpc = true; //是否是配置表NPC还是动态下发NPC
    public bool isStaticNpc {
        get {
            return m_isStaticNpc;
        }
        set {
            m_isStaticNpc = value;
        }
    }
    private int m_isPrivate = 0;
    [ConfigId(12)]
    public int isPrivate {
        get {
            return m_isPrivate;
        }
        set {
            m_isPrivate = value;
        }
    }
    private int m_isDynamicNpc = 0; //用于直接忽略一些配置
    [ConfigId(15)]
    public int isDynamicNpc {
        get {
            return m_isDynamicNpc;
        }
        set {
            m_isDynamicNpc = value;
        }
    }
    private int m_isNotVisible = 0; //如果大于等于1则不显示
    [ConfigId(13)]
    public int isNotVisible {
        get {
            return m_isNotVisible;
        }
        set {
            m_isNotVisible = value;
        }
    }
    private string m_script = "";

    [ConfigId(14)]
    public string script {
        get {
            return m_script;
        }
        set {
            m_script = value;
        }
    }
    private int m_tempTeam = 0; //临时数据
    public int tempTeam {
        get {
            return m_tempTeam;
        }
        set {
            m_tempTeam = value;
        }
    }
    private int m_group = 0;
    public int group {
        get {
            return m_group;
        }
        set {
            m_group = value;
        }
    }
    private float m_scale = 1;

    [ConfigId(17)]
    public float scale {
        get {
            return m_scale;
        }
        set {
            m_scale = value;
        }
    }
    private int m_level = 0;
    public int level {
        get {
            return m_level;
        }
        set {
            m_level = value;
        }
    }
    private int m_maxHp = 0;
    public int maxHp {
        get {
            return m_maxHp;
        }
        set {
            m_maxHp = value;
        }
    }
    private int m_maxMp = 0;
    public int maxMp {
        get {
            return m_maxMp;
        }
        set {
            m_maxMp = value;
        }
    }
    private float m_attack = 0;
    public float attack {
        get {
            return m_attack;
        }
        set {
            m_attack = value;
        }
    }
    private float m_defence = 0;
    public float defence {
        get {
            return m_defence;
        }
        set {
            m_defence = value;
        }
    }
    private float m_agile = 0;
    public float agile {
        get {
            return m_agile;
        }
        set {
            m_agile = value;
        }
    }
    private float m_spirit = 0;
    public float spirit {
        get {
            return m_spirit;
        }
        set {
            m_spirit = value;
        }
    }
    private float m_recovery = 0;
    public float recovery {
        get {
            return m_recovery;
        }
        set {
            m_recovery = value;
        }
    }
    private float m_kill = 0;
    public float kill {
        get {
            return m_kill;
        }
        set {
            m_kill = value;
        }
    }
    private float m_miss = 0;
    public float miss {
        get {
            return m_miss;
        }
        set {
            m_miss = value;
        }
    }
    private float m_hit = 0; //命中
    public float hit {
        get {
            return m_hit;
        }
        set {
            m_hit = value;
        }
    }
    private float m_counter = 0; //反击
    public float counter {
        get {
            return m_counter;
        }
        set {
            m_counter = value;
        }
    }
    private int m_normalSkill = 0; //普通攻击
    public int normalSkill {
        get {
            return m_normalSkill;
        }
        set {
            m_normalSkill = value;
        }
    }
    private int m_defenseSkill = 0; //防御攻击
    public int defenseSkill {
        get {
            return m_defenseSkill;
        }
        set {
            m_defenseSkill = value;
        }
    }
    private int m_skill1 = 0;
    public int skill1 {
        get {
            return m_skill1;
        }
        set {
            m_skill1 = value;
        }
    }
    private int m_skill2 = 0;
    public int skill2 {
        get {
            return m_skill2;
        }
        set {
            m_skill2 = value;
        }
    }
    private int m_skill3 = 0;
    public int skill3 {
        get {
            return m_skill3;
        }
        set {
            m_skill3 = value;
        }
    }
    private int m_race = 0;
    public int race {
        get {
            return m_race;
        }
        set {
            m_race = value;
        }
    }
    private string m_attr = "";
    public string attr {
        get {
            return m_attr;
        }
        set {
            m_attr = value;
        }
    }
    private int m_curHp = 0;
    public int curHp {
        get {
            return m_curHp;
        }
        set {
            m_curHp = value;
        }
    }
    private int m_curMp = 0;
    public int curMp {
        get {
            return m_curMp;
        }
        set {
            m_curMp = value;
        }
    }
    private int m_isclick = 1;
    [ConfigId(14)]
    public int isclick {
        get {
            return m_isclick;
        }
        set {
            m_isclick = value;
        }
    }
    private int m_icon = 0;

    [ConfigId(5)]
    public int icon {
        get {
            return m_icon;
        }
        set {
            m_icon = value;
        }
    }
    private int m_workCount = 0;
    public int workCount {
        get {
            return m_workCount;
        }
        set {
            m_workCount = value;
        }
    }
    private int m_maxCount = 1;
    public int maxCount {
        get {
            return m_maxCount;
        }
        set {
            m_maxCount = value;
        }
    }
    public string spriteName {
        get;
        set;
    }

    public bool IsPrivate {
        get { return isPrivate > 0; }
    }
    private int m_time = 0;

    [ConfigId(16)]
    public int time {
        get {
            return m_time;
        }
        set {
            m_time = value;
        }
    }

    [ConfigId(4)]
    public int stand {
        set;
        get;
    }


    /// <summary>
    /// NPC配置表里优先加载的NPC
    /// </summary>
    private int m_iFirstLoad = 0;

    [ConfigId(18)]
    public int FirstLoad { get { return m_iFirstLoad; } set { m_iFirstLoad = value; } }

    public enum NpcType {
        UnKnown = 0,
        Creature,
        Player,
        Npc,
        Monster,
        Pet,
        Stone,
        ChampionNpc = 9,
        Box = 10,
        MonsterVisible = 12,
        MapBoss = 13,
        ElementBoss = 14,
        MonsterVisible1 = 15,
        MoveObject = 16,
        CrystalNpc = 18,
        ProtectNpc = 19, // 护送NCP不可点击&&移动的对象
        MonsterDirect = 21
    }
    public bool IsStaticObj {
        get {
            return objType == (int)NpcType.Stone || objType == (int)NpcType.Box || objType == (int)NpcType.MonsterVisible
              || objType == (int)NpcType.ElementBoss || objType == (int)NpcType.MonsterVisible1;
        }
    }
    public bool IsMoveObj {
        get {
            return objType == (int)NpcType.MoveObject || objType == (int)NpcType.ProtectNpc;
        }
    }
    public bool IsProtectNpc {
        get {
            return objType == (int)NpcType.ProtectNpc;
        }
    }
}
