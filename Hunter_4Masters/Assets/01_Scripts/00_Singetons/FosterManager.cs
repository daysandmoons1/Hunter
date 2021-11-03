using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Linq;
using Newtonsoft.Json;

#region Define Foster Classes
public class PlayerData : ICloneable
{
    public Dictionary<string, Stat> Stats;
    public Con Cons;
    public MonInven Mon_Inven;

    public object Clone()
    {
        PlayerData clone = new PlayerData();
        clone.Stats = new Dictionary<string, Stat>();
        clone.Cons = (Con)this.Cons.Clone();
        foreach (KeyValuePair<string, Stat> pair in Stats)
        {
            clone.Stats.Add(pair.Key, (Stat)pair.Value.Clone());
        }
        clone.Mon_Inven = (MonInven)this.Mon_Inven.Clone();
        return clone;
    }

    #region public Get() functions ===========================================================================================================
    public float GetCurPoint(string key)
    {
        if (Stats.ContainsKey(key))
        {
            return Stats[key].cur_point;
        }
        else if (Cons.n_Cons.ContainsKey(key))
        {
            return Cons.n_Cons[key].cur_point;
        }
        else if (key == Const.time) return GameManager.Instance.leftTimeVal;
        else if (key == Const.money) return Mon_Inven.Money;
        else Debug.LogError(key + "에 해당하는 플레이어 데이터가 존재하지 않음");
        return 0;
    }

    public float GetStatMax(string key)
    {
        if (Stats.ContainsKey(key)) return Stats[key].max_point;
        else if (Cons.n_Cons.ContainsKey(key)) return Cons.n_Cons[key].max_point;
        else throw new NullReferenceException();
    }

    public float GetStatPercent(string key)
    {
        if (Stats.ContainsKey(key)) return Stats[key].cur_point / Stats[key].max_point;
        else if (Cons.n_Cons.ContainsKey(key)) return Cons.n_Cons[key].cur_point / Cons.n_Cons[key].max_point;
        else throw new NullReferenceException();
    }

    public string GetDefStat(string key)
    {
        float effectValue=0;
        for(int i=0; i < Const.equipNum; i++)
        {
            if (Mon_Inven.Equipment[i].item_name == "none") continue;
            Equipment itemData = (Equipment)DataManager.Instance.GetItemData(Mon_Inven.Equipment[i]);
            for (int j=0; j<itemData.effect.Count; j++)
            {
                if (itemData.effect[j].effect_variable == key) effectValue += itemData.effect[j].GetMinValue();
            }
        }
        return TextFormatter.DefStatFormat(Stats[key].name, Stats[key].cur_point, Stats[key].max_point, effectValue);
    }

    public string GetBatStat()
    {
        string[] names = new string[3];
        float[] points = new float[3];

        for(int i=0; i<3; i++)
        {
            names[i] = Stats[Const.batStats[i]].name;
            points[i] = Stats[Const.batStats[i]].cur_point;
        }

        return TextFormatter.BatStatFormat(names, points);
    }

    public string GetStateOutOfMax(string key)
    {
        if (Stats.ContainsKey(key)) return TextFormatter.AOutOfB(Stats[key].cur_point,Stats[key].max_point);
        else if (Cons.n_Cons.ContainsKey(key)) return TextFormatter.AOutOfB(Cons.n_Cons[key].cur_point, Cons.n_Cons[key].max_point);
        else throw new NullReferenceException();
    }

    public string GetMoney()
    {
        return TextFormatter.GetMoney(Mon_Inven.Money);
    }

    public PlayerItem GetItemByIndex(int slotIndex)
    {
        for (int i = 0; i < Mon_Inven.Inven.Count; i++)
        {
            if (Mon_Inven.Inven[i].inven_index == slotIndex) return Mon_Inven.Inven[i];
        }
        return null;
    }

    public List<EquipItem> GetAllEquipItems()
    {
        return Mon_Inven.Equipment;
    }

    #endregion public Get() functions ===========================================================================================================

    public void ChangeCurPoint(string key, float value)
    {
        if (Stats.ContainsKey(key))
        {
            Stats[key].cur_point += value;
        }
        else if (Cons.n_Cons.ContainsKey(key))
        {
            Cons.n_Cons[key].cur_point = Mathf.Clamp(Mathf.RoundToInt(Cons.n_Cons[key].cur_point + value), 0, Cons.n_Cons[key].max_point); ;
        }
        else if (key == Const.time) GameManager.Instance.tempSpendTime += (int)value;
        else if (key == Const.money) Mon_Inven.Money += value;
        else Debug.LogError(key + "에 해당하는 플레이어 데이터가 존재하지 않음");
    }

    public void ChangeConsByPer(string stat, int percent)
    {
        // 소수점 반올림
        int val = Mathf.RoundToInt(Cons.n_Cons[stat].cur_point * percent * 0.01f);
        Cons.n_Cons[stat].cur_point = Mathf.Clamp(Cons.n_Cons[stat].cur_point + val, 0, Cons.n_Cons[stat].max_point);
    }

    public void IncreaseMaxPoint(string key, int value)
    {
        Stats[key].max_point += value;
    }

    // 아이템 인벤토리에 추가 시 호출(ItemData로)
    public void AddInvenItem(ItemData item, int index = -1, int itemEach = -1)
    {
        if (item == null || itemEach == 0) return;

        if (index == -1)    // 사용자 지정 인덱스가 아닐 경우 적절한 인덱스 탐색
        {
            Mon_Inven.Inven = Mon_Inven.Inven.OrderBy(x => x.inven_index).ToList();

            // 포션아이템이면 이미 있는 아이템인지(+ 최대 개수보다 작은지) 체크
            if (itemEach != -1)
            {
                for (int i = 0; i < Mon_Inven.Inven.Count; i++)
                {
                    if (Mon_Inven.Inven[i].item_index == item.data_ID &&
                        Mon_Inven.Inven[i].item_each < ((Potion)DataManager.Instance.GetItemData(Mon_Inven.Inven[i])).max_capacity)
                    {
                        index = Mon_Inven.Inven[i].inven_index;
                    }
                }

            }

            if (index != -1)
            {
                // 이미 있는 포션아이템이라면
                GetItemByIndex(index).item_each += itemEach;
                return;
            }
            else
            {   // 장비아이템이거나 동일 포션아이템이라면
                index = GetFirstEmptyIndex();

            }
        }

        Mon_Inven.Inven.Add(new InvenItem()
        {
            inven_index = index,
            item_index = item.data_ID,
            item_name = DataManager.Instance.GetItemKey(item)
        });

        // 포션아이템이라면 개수데이터 추가
        if (itemEach != -1) GetItemByIndex(index).item_each = itemEach;

    }

    // 아이템 인벤토리에 추가 시 호출(PlayerItem으로)
    public void AddInvenItem(PlayerItem item, int index = -1, int itemEach = -1)
    {
        if (item == null || itemEach == 0) return;
        AddInvenItem(DataManager.Instance.GetItemData(item), index, itemEach);
    }


    public void RemoveInvenItem(PlayerItem item)
    {
        if (item == null) return;
        for (int i = 0; i < Mon_Inven.Inven.Count; i++)
        {
            if (((InvenItem)item).inven_index == Mon_Inven.Inven[i].inven_index)
            {
                Mon_Inven.Inven.RemoveAt(i);
            }
        }
    }

    public void RemoveInvenItem(PlayerItem item, int itemEach)
    {
        if (item == null) return;
        for (int i=0; i < Mon_Inven.Inven.Count; i++)
        {
            if(((InvenItem)item).inven_index == Mon_Inven.Inven[i].inven_index)
            {
                if (Mon_Inven.Inven[i].item_each == itemEach) Mon_Inven.Inven.RemoveAt(i);
                else Mon_Inven.Inven[i].item_each -= itemEach;
            }
        }
    }

    public void SetEquipItem(int index, PlayerItem item)
    {
        ResetEquipItem(index);
        if (item != null)
        {
            ItemData itemData = DataManager.Instance.GetItemData(item);
            for (int i = 0; i < itemData.effect.Count; i++)
            {
                ChangeCurPoint(itemData.effect[i].effect_variable, itemData.effect[i].GetMinValue());
            }
            Mon_Inven.Equipment[index].item_name = item.item_name;
            Mon_Inven.Equipment[index].item_index = item.item_index;
            if (itemData.category==3) Mon_Inven.Equipment[index].item_each = item.item_each;
        }
    }

    // 기존 장비의 장착을 해제하고, 장착 효과를 없앰
    public void ResetEquipItem(int index)
    {
        if (Mon_Inven.Equipment[index].item_name != Const.defStr)   // 해당 인덱스에 아이템이 저장되어 있는 경우
        {
            ItemData itemData = DataManager.Instance.GetItemData(Mon_Inven.Equipment[index]);
            for (int i = 0; i < itemData.effect.Count; i++)
            {
                ChangeCurPoint(itemData.effect[i].effect_variable, -itemData.effect[i].GetMinValue());
            }
            Mon_Inven.Equipment[index].item_name = "none";
            Mon_Inven.Equipment[index].item_index = 0;
        }
    }

    public void RemoveEquipItem(EquipItem item, int itemEach)
    {
        if (Mon_Inven.Equipment[item.equip_index].item_each == itemEach) ResetEquipItem(item.equip_index);
        else Mon_Inven.Equipment[item.equip_index].item_each -= itemEach;
    }

    // (장비/인벤 공통) 동일 포션을 병합: item 개수를 itemEach만큼 더하고 남는 개수를 리턴
    public int AddPotion(ref PlayerItem item, int itemEach)
    {
        int surplus = item.item_each + itemEach - ((Potion)DataManager.Instance.GetItemData(item)).max_capacity;

        item.item_each = Mathf.Clamp(item.item_each + itemEach, 0, ((Potion)DataManager.Instance.GetItemData(item)).max_capacity);

        return (surplus > 0) ? surplus : 0;
    }

    public void UsePotion(int index)
    {
        RemoveEquipItem(Mon_Inven.Equipment[index], 1);
    }


    // 플레이어에게 옷 입히기
    public void SetArmor()
    {
        Debug.Log("옷입히기");
    }

    public void RemoveSpecialCondition(int index)
    {
        Cons.ETC.RemoveAt(index);
    }

    private int GetFirstEmptyIndex()
    {
        int index = 0;

        for (int i = 0; i < Mon_Inven.Inven.Count; i++)
        {
            if (index == Mon_Inven.Inven[i].inven_index) index++;   // 리스트 인덱스와 인벤 인덱스가 불일치하는 구간이 가장 처음 비는 인덱스
        }
        return index;
    }
}

public class Stat : ICloneable
{
    public string name;
    public float max_point;
    public float cur_point;

    public object Clone()
    {
        return new Stat
        {
            name = this.name,
            max_point = this.max_point,
            cur_point = this.cur_point
        };
    }
}

public class Con : ICloneable
{
    public Dictionary<string, Stat> n_Cons;
    public List<ConsEtc> ETC;

    public object Clone()
    {
        Con clone = new Con();
        clone.n_Cons = new Dictionary<string, Stat>();
        clone.ETC = new List<ConsEtc>();
        foreach (KeyValuePair<string, Stat> pair in n_Cons)
        {
            clone.n_Cons.Add(pair.Key, pair.Value.Clone() as Stat);
        }
        for(int i=0; i<ETC.Count; i++)
        {
            clone.ETC.Add(this.ETC[i].Clone() as ConsEtc);
        }
        return clone;
    }
}

public class ConsEtc : ICloneable
{
    public string name;
    public int index;

    public object Clone()
    {
        return new ConsEtc
        {
            name = this.name,
            index = this.index
        };
    }
}

public class MonInven : ICloneable
{
    public float Money;
    public List<EquipItem> Equipment;
    public List<InvenItem> Inven;

    public object Clone()
    {
        MonInven monInven = new MonInven();
        monInven.Money = this.Money;
        monInven.Equipment = new List<EquipItem>();
        monInven.Inven = new List<InvenItem>();

        for(int i=0; i < Equipment.Count; i++)
        {
            monInven.Equipment.Add(Equipment[i].Clone() as EquipItem);
        }
        for (int i = 0; i < Inven.Count; i++)
        {
            monInven.Inven.Add(Inven[i].Clone() as InvenItem);
        }
        return monInven;
    }
}

public class EquipItem : PlayerItem, ICloneable
{
    public int equip_index;

    public EquipItem()
    {
    }

    protected EquipItem(EquipItem that) : base(that)
    {
        this.equip_index = that.equip_index;
    }

    public override object Clone()
    {
        return new EquipItem(this);
    }
}

public class InvenItem : PlayerItem, ICloneable
{
    public int inven_index;

    public InvenItem()
    {
    }

    protected InvenItem(InvenItem that) : base(that)
    {
        this.inven_index = that.inven_index;
    }

    public override object Clone()
    {
        return new InvenItem(this);
    }
}

public class PlayerItem : ICloneable
{
    public string item_name;
    public int item_each = -1;
    public int item_index;

    public PlayerItem()
    {
    }

    protected PlayerItem(PlayerItem that)
    {
        this.item_name = that.item_name;
        this.item_each = that.item_each;
        this.item_index = that.item_index;
    }

    public virtual object Clone()
    {
        return new PlayerItem(this);
    }
}
#endregion

public class FosterManager : Singleton<FosterManager>
{

    private PlayerData playerData;
    private int awakeningCnt = 1;
    private bool changeFlag = false;

    public override void Awake()
    {
        base.Awake();
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        Dictionary<string, PlayerData> data = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(Resources.Load<TextAsset>($"Data/player_base").text);
        playerData = data.Values.ElementAt(0);   // KeyValuePair<> 형태로 역직렬화 불가-> Dictionary<>로 받음
        playerData.Mon_Inven.Inven.Clear();
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }

    public int GetAwakeningCnt()
    {
        return awakeningCnt;
    }

    public void Awakening()
    {
        awakeningCnt++;
    }

    public void SetPlayerData(PlayerData pd)
    {
        playerData = pd;
    }

    public bool GetChangeFlag()
    {
        return changeFlag;
    }

    // 플레이어 데이터 변경/적용 시 플래그 세팅(UI 갱신 필요 여부 확인)
    public void SetChangeFlag(bool flag)
    {
        changeFlag = flag;
    }
}
