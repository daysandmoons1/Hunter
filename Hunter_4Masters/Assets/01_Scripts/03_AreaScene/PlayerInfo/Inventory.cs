using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMasters.Popup
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private Text timeTxt, moneyTxt, itemInfoTxt;
        [SerializeField] private Image rankImg;
        [SerializeField] private GameObject equipment, equipPotion, inventory, itemInfoPanel;

        private Slot[] equipmentSlots, inventorySlots;
        private PlayerData pd;
        private ItemInfo itemInfo;

        private void Awake()
        {
            var tempList = new List<Slot>();
            tempList.AddRange(equipment.GetComponentsInChildren<Slot>());
            tempList.AddRange(equipPotion.GetComponentsInChildren<Slot>());
            equipmentSlots = tempList.ToArray();

            itemInfo = itemInfoPanel.GetComponent<ItemInfo>();

            for (int i=0; i<equipmentSlots.Length; i++)
            {
                equipmentSlots[i].slotType = Const.equip;
                equipmentSlots[i].slotIndex = i;
                equipmentSlots[i].defaultSprite = equipmentSlots[i].transform.GetChild(0).GetComponent<Image>().sprite;
            }

            inventorySlots = inventory.GetComponentsInChildren<Slot>();
            for(int i=0; i<inventorySlots.Length; i++)
            {
                inventorySlots[i].slotType = Const.inven;
                inventorySlots[i].slotIndex = i;
            }
        }

        private void OnEnable()
        {
            itemInfoPanel.SetActive(false);
            SetUI();
        }

        private void Update()
        {
            if (FosterManager.Instance.GetChangeFlag())
            {
                SetUI();
                FosterManager.Instance.SetChangeFlag(false);
            }
            else if (SelectedSlot.Instance.isSelected)
            {
                itemInfoPanel.SetActive(true);
                ItemData id = DataManager.Instance.GetItemData(SelectedSlot.Instance.selectedItem);
                itemInfo.SetItemInfo(id);
                //itemInfoTxt.text = SetItemInfo(SelectedSlot.Instance.selectedItem);
                SelectedSlot.Instance.isSelected = false;
            }
        }

        private void SetUI()
        {
            pd = FosterManager.Instance.GetPlayerData();

            timeTxt.text = GameManager.Instance.GetDDay() + "\n" +
                GameManager.Instance.GetCurrentTimeByValue();
            moneyTxt.text = pd.GetMoney();
            rankImg.sprite = DataManager.Instance.GetSprite("icons", "rank_F");

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].SetItem(pd.Mon_Inven.Equipment[i]);
            }
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].SetItem(null);
            }
            for (int i = 0; i < pd.Mon_Inven.Inven.Count; i++)
            {
                inventorySlots[pd.Mon_Inven.Inven[i].inven_index].SetItem(pd.Mon_Inven.Inven[i]);
            }
        }

        private string SetItemInfo(PlayerItem pd)
        {
            ItemData id = DataManager.Instance.GetItemData(pd);

            string result = "";

            result += id.name + "\n" + Const.itemCategory[id.category] + "\n";

            if (id.GetType() == typeof(Equipment))
            {
                for (int i = 0; i < ((Equipment)id).condition.Count; i++)
                {
                    if (i == 0) result += "장착조건\n";
                    result += DataManager.Instance.GetConditionRange(((Equipment)id).condition[i]) + "\n";
                }

                for (int i = 0; i < id.effect.Count; i++)
                {
                    if (i == 0) result += "기본 효과\n";
                    result += id.effect[i].effect_variable +
                                        GetEffectRange(id.effect[i].effect_min, id.effect[i].effect_max) + "\n";
                }

                result += "\n"+((Equipment)id).skillIndex.ToString();


            }
            else
            {
                for (int i = 0; i < id.effect.Count; i++)
                {
                    result += id.effect[i].effect_variable + " " +
                                        GetEffectRange(id.effect[i].effect_min, id.effect[i].effect_max) + " 회복\n";
                }
            }

            return result;
        }

        private string GetEffectRange(string minStr, string maxStr)
        {
            string str = "";

            float min = StringCalculator.Calculate(minStr);
            float max = StringCalculator.Calculate(maxStr);

            str += string.Format("{0:+0;-0}", min);
            if (Mathf.Abs(min - max) > 0.0001f) str += "~" + string.Format("{0:+0;-0}", max);

            return str;
        }
    }

}
