using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;
using System.IO;
using System;

// 상수 정의
public static class Const
{
    public const int etcMaxSize = 10;
    public const int minDef = -10000;
    public const int maxDef = 9999;
    public const int equipNum = 3;
    public const int potionNum = 2;
    public const int requestNum = 3;
    public const int healOptNum = 3;

    public const string defStr = "none";
    public const string defStr2 = "null";  // 통일해야 될듯

    public static readonly string[] state = { "HP", "SP", "WILL" };
    public const string hp = "HP";
    public const string sp = "SP";
    public const string will = "WILL";

    public const string money = "MONEY";
    public const string time = "TIME";

    public const string inven = "Inven";
    public const string equip = "Equipment";

    public static readonly string[] defStats = { "STR", "DEX", "INT" };
    public static readonly string[] batStats = { "ATK", "APS", "DEF" };

    public static readonly string[] equipType = { "weapon", "armor", "accessory", "potion", "potion" };
    public static readonly string[] itemCategory = { "장비-무기", "장비-갑옷", "장비-악세", "포션" };

    public static readonly string[] area = { "지리산", "세종", "서울" };
    public static readonly string[] requestMap = { "숲", "동굴", "묘지", "오거 촌락", "연구실", "용암지대" };
    public static readonly string[] requestType = { "토벌","파괴","정찰" };
    public static readonly Color[] reqMapColor = { 
        new Color(0.6f,0.8f,0.4f), 
        Color.black,
        new Color(0.2f, 0.2f, 0.5f),
        new Color(0.3f, 0.4f, 0.2f), 
        new Color(0.6f,0.5f,0.8f),
        Color.red };

}

public enum CostType { SP, TIME, }

#region Define Data Classes
public class ActionData
{
    public Dictionary<string, ChoiceData> choiceDataDict;  // Title, walk, bus, convoy,...
    public Dictionary<string, EventData> eventDataDict;    // 
}

public class Request
{
    public int data_ID;
    public string name;
    public string rank;
    public string detail;
    public int map;
    public int additional_quest;
    public int clear_condition;
    public int clear_target;
    public int clear_amount;
    public int reward_money;
    public int[] reward_item;
}

public class ScHospitalData : HospitalBaseData
{
    public int data_ID;
    public int target_sc;
    public int[] able_object;
}

public class OtherHospitalData : HospitalBaseData
{
    public int healPercent;
}

public class HospitalBaseData
{
    public float cost_money;
    public float cost_time;
}

public class ChoiceData
{
    public int data_ID;
    public string name;
    public int area;
    public string category;
    public List<Consume> consume;
    public string plusInfo;
    public List<Condition> condition;
    public List<Event> events; // 'event'는 에러남
}
public class EventData
{
    public int data_ID;
    public string name;
    public string category;
    public int pre_ID;
    public string mention;
    public List<Condition> condition;
    public List<Effect> effect;
}

public class Consume
{
    public string consume_variable;
    public float consume_value;
}

public class Condition
{
    public string condition_variable;
    public float condition_min;
    public float condition_max;
    public string condition_per;
}
public class Event
{
    public int event_ID;
    public float event_per;
}
public class Effect
{
    public string effect_variable;
    public string effect_min;
    public string effect_max;

    public float GetMinValue()
    {
        return StringCalculator.Calculate(effect_min);
    }
    public float GetMaxValue()
    {
        return StringCalculator.Calculate(effect_max);
    }
}

public class Equipment : ItemData
{
    public List<Condition> condition;
    public int spEffectIndex;
    public int skillIndex;
}

public class Potion : ItemData
{
    public int max_capacity;
}

public class ItemData
{
    public int data_ID;
    public string name;
    public int category;
    public string rank;
    public float price;
    public List<Effect> effect;

    public string GetItemName()
    {
        string description = "<size=30>" + name + "</size>" + "\n" + "<color=#85a763>" + Const.itemCategory[category] + "</color>\n";
        return description;
    }
}
#endregion

public class DataManager : Singleton<DataManager>
{
    Dictionary<string, ActionData> actionDataDict = new Dictionary<string, ActionData>();    // moveData, laborData, ...
    Dictionary<string, Request> requestDict = new Dictionary<string, Request>();
    Dictionary<string, ScHospitalData> scHospitalDict = new Dictionary<string, ScHospitalData>();
    Dictionary<string, Equipment> equipmentDict = new Dictionary<string, Equipment>();
    Dictionary<string, Potion> potionDict = new Dictionary<string, Potion>();
    Dictionary<int, string> detailMsg = new Dictionary<int, string>()
        {
            {300001, "[빈곤] \n 돈이 없다... \n 근력 * 125% \n 민첩 * 125% \n 재화 획득량 * 125%"},
            {300003, "[좌절] \n 하...이번 생은 망했어 \n 모든 증가 스탯 * 50% \n 모든 감소 스탯 * 200%"},
            {300000, "[영양실조] \n 공격속도 * 50% \n 의지 * 75%"}
        };
    

    private int[,] requestCost = new int[2, Const.requestMap.Length];   // 의뢰 소모량(스태미나, 시간)
    private string[,] curRequest = new string[Const.area.Length, Const.requestNum];   // *0번째(지리산)는 데이터 없음

    private OtherHospitalData[,] otherHospitalData = new OtherHospitalData[4, Const.healOptNum];

    // 0,1,3: HP, 2: WILL
    private int[,] healPercent = { { 5, 10, 25 }, { 10, 50, 100 }, { 10, 50, 100 }, { 10, 25, 50} };
    private int[,] costMoney = { { 5, 8, 20 }, { 20, 90, 160 }, { 50, 200, 350 }, { 30, 70, 150 } };
    private float[,] costTime = { { 6, 30, 60 }, { 30, 120, 180 }, { 120, 240, 360 }, { 60, 180, 360 } };


    public override void Awake()
    {
        base.Awake();
        LoadActionData("Move");
        LoadActionData("Training");
        LoadRequestData();
        LoadHospitalData();
        LoadItemData();

        InitRequestCost();
    }

    #region Load Json
    private void LoadActionData(string action)
    {
        ActionData actionData = new ActionData();

        Dictionary<string, Dictionary<string, ChoiceData>> cdd = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ChoiceData>>>(Resources.Load<TextAsset>($"Data/" + action + "Data_choice").text);
        actionData.choiceDataDict = cdd.Values.ElementAt(0);
        Dictionary<string, Dictionary<string, EventData>> edd = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, EventData>>>(Resources.Load<TextAsset>($"Data/" + action + "Data_event").text);
        actionData.eventDataDict = edd.Values.ElementAt(0);

        actionDataDict.Add(action + "Data", actionData);

        // 행동 단위로 로드할 경우
        //ActionData.Add(action+"Data", JsonConvert.DeserializeObject<Action>(Resources.Load<TextAsset>($"Data/"+action+"Data").text));

    }

    private void LoadRequestData()
    {
        requestDict = JsonConvert.DeserializeObject<Dictionary<string, Request>>(Resources.Load<TextAsset>($"Data/request").text);
    }

    private void LoadHospitalData()
    {
        Dictionary<string, Dictionary<string, ScHospitalData>> hdd = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ScHospitalData>>>(Resources.Load<TextAsset>($"Data/Hospital").text);
        scHospitalDict = hdd.Values.ElementAt(0);

        for(int i=0; i<4; i++)
        {
            for(int j=0; j<Const.healOptNum; j++)
            {
                otherHospitalData[i, j] = new OtherHospitalData();
                otherHospitalData[i, j].healPercent = healPercent[i, j];
                otherHospitalData[i, j].cost_money = costMoney[i, j];
                otherHospitalData[i, j].cost_time = costTime[i, j];
            }
        }
    }

    private void LoadItemData()
    {
        equipmentDict = JsonConvert.DeserializeObject<Dictionary<string, Equipment>>(Resources.Load<TextAsset>($"Data/equipment").text);
        potionDict = JsonConvert.DeserializeObject<Dictionary<string, Potion>>(Resources.Load<TextAsset>($"Data/potion").text);
    }
    #endregion

    // 의뢰 수행 소모량 설정
    private void InitRequestCost()
    {
        requestCost[0, 0] = 20;
        requestCost[0, 1] = 30;

        requestCost[1, 0] = 60;
        requestCost[1, 1] = 120;

        for (int i = 2; i < Const.requestMap.Length; i++)
        {
            requestCost[0, i] = requestCost[0, i - 1] + requestCost[0, i - 2];
            requestCost[1, i] = requestCost[1, i - 1] + requestCost[1, i - 2];
        }
    }

    // 정해진 시각에 플레이어 등급에 따라 의뢰수주 목록 갱신
    public void SetCurRequest()
    {
        int index = 0;

        // 데이터 추가 시 확률 적용 예정
        for(int i=0; i<Const.area.Length; i++)
        {
            index = 0;
            foreach (KeyValuePair<string, Request> pair in requestDict)
            {
                curRequest[i, index++] = pair.Key;
            }
        }
    }

    // 현재 등록된 의뢰 목록 반환
    public string[] GetCurRequest()
    {
        SetCurRequest();
        // 테스트용으로 무조건 1번째(세종) 반환. GameManager.Instance.GetAreaCode()로 변경 예정
        string[] result = { curRequest[1,0], curRequest[1,1],curRequest[1,2] };
        return result;
    }

    public Request GetRequest(string key)
    {
        return requestDict[key];
    }

    public int GetRequestCost(CostType cost, int index)
    {
        return requestCost[(int)cost, index];
    }

    //public List<HospitalBase>[] GetScHospitalData()
    //{
    //    List<HospitalBase>[] hdl = new List<HospitalBase>[3];
    //    hdl[0] = new List<HospitalBase>();
    //    hdl[1] = new List<HospitalBase>();
    //    hdl[2] = new List<HospitalBase>();
    //    int curObjCode = UIManager_Area.Instance.GetSpObjectCode();
    //    if (curObjCode == -1) Debug.LogError("병원 건물 번호가 지정되지 않았음(objectCode==-1)");

    //    foreach(KeyValuePair<string, HospitalData> pair in hospitalDict)
    //    {
    //        if (pair.Value.able_object.Contains<int>(curObjCode))
    //        {
    //            hdl[0].Add(pair.Value);
    //        }
    //    }

    //    if (curObjCode == 2)
    //    {
    //        for (int i = 0; i < Const.healOptNum; i++)
    //        {
    //            hdl[2].Add(otherHospitalData[curObjCode, i]); // 세종-정신과
    //        }
    //    }
    //    else {
    //        for(int i=0; i<Const.healOptNum; i++)
    //        {
    //            hdl[1].Add(otherHospitalData[curObjCode, i]);
    //        }
    //    }

    //    return hdl;
    //}

    public List<ScHospitalData> GetScHospitalData()
    {
        List<ScHospitalData> hd = new List<ScHospitalData>();

        int curObjCode = UIManager_Area.Instance.GetSpObjectCode();
        if (curObjCode == -1) Debug.LogError("병원 건물 번호가 지정되지 않았음(objectCode==-1)");

        foreach (KeyValuePair<string, ScHospitalData> pair in scHospitalDict)
        {
            if (pair.Value.able_object.Contains<int>(curObjCode))
            {
                hd.Add(pair.Value);
            }
        }

        return hd;
    }

    public List<OtherHospitalData>[] GetOtherHospitalData()
    {
        List<OtherHospitalData>[] hdl = new List<OtherHospitalData>[2]; // 0:hp, 1:will
        hdl[0] = new List<OtherHospitalData>();
        hdl[1] = new List<OtherHospitalData>();

        int curObjCode = UIManager_Area.Instance.GetSpObjectCode();
        if (curObjCode == -1) Debug.LogError("병원 건물 번호가 지정되지 않았음(objectCode==-1)");

        if (curObjCode == 2)
        {
            for (int i = 0; i < Const.healOptNum; i++)
            {
                hdl[1].Add(otherHospitalData[curObjCode, i]); // 세종-정신과
            }
        }
        else
        {
            for (int i = 0; i < Const.healOptNum; i++)
            {
                hdl[0].Add(otherHospitalData[curObjCode, i]);
            }
        }

        return hdl;
    }

    public ChoiceData GetChoiceTitle(string action)
    {
        return actionDataDict[action + "Data"].choiceDataDict[action + "_Title"];
    }

    public Dictionary<string, ChoiceData> GetChoiceData(string action, int areaCode)
    {
        Dictionary<string, ChoiceData> choiceDataDict = new Dictionary<string, ChoiceData>();

        // key값이 action + "_" + areaCode + "_"를 포함하는 밸류를 다 가져오기...
        foreach (KeyValuePair<string, ChoiceData> pair in actionDataDict[action + "Data"].choiceDataDict)
        {
            if (pair.Key.Contains(action + "_" + areaCode + "_"))
            {
                choiceDataDict.Add(pair.Key, pair.Value);
            }
        }

        return choiceDataDict;
    }

    public KeyValuePair<string, EventData> GetEventDataById(string action, int id)
    {
        foreach (KeyValuePair<string, EventData> pair in actionDataDict[action + "Data"].eventDataDict)
        {
            if (pair.Value.data_ID == id)
            {
                return new KeyValuePair<string, EventData>(pair.Key, pair.Value);
            }
        }
        return new KeyValuePair<string, EventData>(null, null);
    }

    public List<KeyValuePair<string, EventData>> GetSecondaryEventList(string action, int primaryId)
    {
        List<KeyValuePair<string, EventData>> secondaryEventData = new List<KeyValuePair<string, EventData>>();

        foreach (KeyValuePair<string, EventData> pair in actionDataDict[action + "Data"].eventDataDict)
        {
            if (pair.Value.pre_ID == primaryId)
            {
                secondaryEventData.Add(pair);
            }
        }

        return secondaryEventData;
    }

    public string GetItemKey(ItemData id)
    {
        foreach(KeyValuePair<string, Equipment> pair in equipmentDict)
        {
            if (pair.Value == id) return pair.Key;
        }

        foreach (KeyValuePair<string, Potion> pair in potionDict)
        {
            if (pair.Value == id) return pair.Key;
        }

        throw new NullReferenceException();
    }

    public string GetItemKey(int index)
    {
        foreach (KeyValuePair<string, Equipment> pair in equipmentDict)
        {
            if (pair.Value.data_ID == index) return pair.Key;
        }

        foreach (KeyValuePair<string, Potion> pair in potionDict)
        {
            if (pair.Value.data_ID == index) return pair.Key;
        }

        throw new NullReferenceException();
    }

    public ItemData GetItemData(PlayerItem item)
    {
        if (equipmentDict.ContainsKey(item.item_name)) return equipmentDict[item.item_name];
        else if (potionDict.ContainsKey(item.item_name)) return potionDict[item.item_name];
        else throw new NullReferenceException();
    }

    public ItemData GetItemData(string name)
    {
        if (equipmentDict.ContainsKey(name)) return equipmentDict[name];
        else if (potionDict.ContainsKey(name)) return potionDict[name];
        else throw new NullReferenceException();
    }

    public ItemData GetItemData(int index)
    {
        foreach (KeyValuePair<string, Equipment> pair in equipmentDict)
        {
            if (pair.Value.data_ID == index) return pair.Value;
        }

        foreach (KeyValuePair<string, Potion> pair in potionDict)
        {
            if (pair.Value.data_ID == index) return pair.Value;
        }

        throw new NullReferenceException();
    }

    public string GetConditionRange(Condition con)
    {
        bool hasMinLimit = Mathf.Abs(con.condition_min - Const.minDef) > 0.0001f;
        bool hasMaxLimit = Mathf.Abs(con.condition_max - Const.maxDef) > 0.0001f;
        string str = "";

        if (hasMinLimit || hasMaxLimit)
        {
            str += con.condition_variable + " ";
            if (hasMinLimit) str += con.condition_min + " 이상 ";
            if (hasMaxLimit) str += con.condition_max + " 이하";
        }

        return str;
    }

    public Dictionary<string, Equipment> GetAllEquipments()
    {
        return equipmentDict;
    }

    public Dictionary<string, Potion> GetAllPotions()
    {
        return potionDict;
    }

    // 임시
    public string GetEtcDetail(int index)
    {
        return detailMsg[index];
    }

    // ResourceManager 구현 후 교체 예정
    public Sprite GetSprite(string folder, string name)
    {
        if(folder=="") return Resources.Load(name, typeof(Sprite)) as Sprite;
        else return Resources.Load(folder + "/" + name, typeof(Sprite)) as Sprite;
    }
}
