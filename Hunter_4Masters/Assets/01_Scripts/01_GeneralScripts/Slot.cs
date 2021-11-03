using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Slot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler, IEndDragHandler, IDropHandler,
    IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
    IDragHandler
{
    public string slotType;
    public int slotIndex;
    public PlayerItem playerItem;
    public delegate void SDelegate(int a);
    public SDelegate selectDel;

    [SerializeField] private GameObject itemEach;
    [SerializeField] private Text itemEachTxt;
    public Image itemImage;
    public Sprite defaultSprite;

    private bool shopSlot = false;
    private bool isPressed = false;
    private float pressTime = 0;

    public void isShopSlot(bool flag)
    {
        shopSlot = flag;
    }

    public void SetItem(PlayerItem newPlayerItem)
    {
        if (newPlayerItem != null && newPlayerItem.item_name != Const.defStr)
        {
            playerItem = newPlayerItem.Clone() as PlayerItem;
        }
        else playerItem = null;
        itemImage.sprite = defaultSprite;
        Color color = itemImage.color;
        color.a = 1f;
        itemEach.SetActive(false);

        if (playerItem != null)
        {
            if (slotType == Const.inven)    // 인벤슬롯일 때
            {
                ((InvenItem)playerItem).inven_index = slotIndex;


            }
            else if(slotType == Const.equip)    // 장비슬롯일 때
            {

            }
            // 소비 아이템일 경우 개수 표시
            if(DataManager.Instance.GetItemData(playerItem).GetType() == typeof(Potion))
            {
                itemEach.SetActive(true);
                itemEachTxt.text = playerItem.item_each.ToString();
            }
            itemImage.sprite = DataManager.Instance.GetSprite("Items",playerItem.item_name);
        }
        else
        {
            if (slotType == Const.inven) color.a = 0f;
        }
        itemImage.color = color;
    }

    // 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        if (shopSlot) return;
        isPressed = true;
        SelectedSlot.Instance.SetItem(playerItem);
        StartCoroutine(PressCoroutine(eventData));
        //Debug.Log("OnPointerDown");
    }

    IEnumerator PressCoroutine(PointerEventData eventData)
    {
        while (isPressed)
        {
            // 0.5초 이상 눌렀고 해당 슬롯에 아이템 있을 시 정보 팝업 띄우기
            if (pressTime > 0.5f && this.playerItem != null)
            {
                SelectedSlot.Instance.SetActiveItemInfo(true);
                //SelectedSlot.Instance.touchPanel.enabled = true;
                SelectedSlot.Instance.SetItemInfoPos(eventData);
            }
            pressTime += Time.deltaTime;
            yield return null;
        }
        pressTime = 0;
        //SelectedSlot.Instance.SetActiveItemInfo(false);
        //yield return null;
    }

    // 같은 위치에서 눌렀다 뗐을 때
    public void OnPointerClick(PointerEventData eventData)
    {
        isPressed = false;

        if (playerItem != null)
        {
            SelectedSlot.Instance.SetSelectedItem(playerItem);
            SelectedSlot.Instance.isSelected = true;
        }

        // 해당 슬롯에 소비아이템이 있을 경우 사용 팝업 띄우기
        //Debug.Log("OnPointerClick");

        // 상점 인벤슬롯인 경우 상세정보 띄우기
        if (shopSlot)
        {
            selectDel(slotIndex+3);
        }
    }

    // 드래그 시작 시
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!shopSlot)
        {
            isPressed = false;
            if (playerItem != null)
            {
                SelectedSlot.Instance.SetDragSlotImage(itemImage);
                SelectedSlot.Instance.SetDragSlotPos(eventData);
                if (slotType == Const.equip) itemImage.sprite = defaultSprite;
                else itemImage.enabled = false;
                itemEach.SetActive(false);
            }
            //Debug.Log("OnBeginDrag");
        }
    }

    // 드래그 중인 동안
    public void OnDrag(PointerEventData eventData)
    {
        if (!shopSlot && playerItem != null)
        {
            SelectedSlot.Instance.SetDragSlotPos(eventData);
        }
        //Debug.Log("OnDrag");
    }

    // 드래그 끝났을 때
    public void OnEndDrag(PointerEventData eventData)
    {
        if (shopSlot) return;
        PlayerItem afterPlayerItem = SelectedSlot.Instance.playerItem;

        // 아이템이 교체되었을 경우
        if (afterPlayerItem != playerItem) {
            // 장비슬롯이라면 - 교체할 아이템을 해당 슬롯번호의 장비인덱스에 대입.(null이면 리셋만)
            if (slotType == Const.equip)
            {
                FosterManager.Instance.GetPlayerData().SetEquipItem(slotIndex, afterPlayerItem);
                
            }
            else
            {   // 인벤슬롯이라면

                // 인벤토리 리스트에서 해당 슬롯 아이템을 제거
                FosterManager.Instance.GetPlayerData().RemoveInvenItem(playerItem);
                // 인벤토리 리스트에 교체할 아이템 추가
                if (afterPlayerItem!=null && DataManager.Instance.GetItemData(afterPlayerItem).category>2)
                    FosterManager.Instance.GetPlayerData().AddInvenItem(afterPlayerItem, slotIndex, afterPlayerItem.item_each);
                else FosterManager.Instance.GetPlayerData().AddInvenItem(afterPlayerItem, slotIndex);
            }
        }
        FosterManager.Instance.SetChangeFlag(true);
        UIManager_Area.Instance.SetSkills();
        itemImage.enabled = true;
        //if(playerItem != null && playerItem.item_name!="none") itemEach.SetActive(DataManager.Instance.GetItemData(playerItem.item_name).type == "consumable");
        SelectedSlot.Instance.ResetDragSlot();
        //Debug.Log("OnEndDrag");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중인 상태에서 해당 슬롯에 접근한 경우 포커스 표시 보이기(반투명 흰색 박스?)
        //Debug.Log("OnPointerEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 드래그 중인 상태에서 해당 슬롯에 접근했다가 나간 경우 포커스 표시 숨기기
        //Debug.Log("OnPointerExit");
    }

    // 이 슬롯에 드롭되었을 때
    public void OnDrop(PointerEventData eventData)
    {
        if (shopSlot) return;
        PlayerItem beforePlayerItem = SelectedSlot.Instance.playerItem;
        if (beforePlayerItem == null) return;

        if (slotType == Const.equip)
        {   // 장비슬롯에 드롭했을 때

            // 장착불가한 아이템이면 리턴
            if (Const.equipType[slotIndex] != Const.equipType[DataManager.Instance.GetItemData(beforePlayerItem).category]) return;
            else
            {
                // 장비 리스트의 이 슬롯 아이템과 같으면 병합하고, 그렇지 않으면 장비 리스트에 교체할 아이템을 장착
                PlayerItem equipItem = FosterManager.Instance.GetPlayerData().Mon_Inven.Equipment[slotIndex].Clone() as PlayerItem;
                if (MergePotion(ref equipItem, beforePlayerItem))
                {
                    FosterManager.Instance.GetPlayerData().SetEquipItem(slotIndex, equipItem);
                }
                else
                {
                    FosterManager.Instance.GetPlayerData().SetEquipItem(slotIndex, beforePlayerItem);
                }

            }
        }
        else
        {   // 인벤슬롯에 드롭했을 때

            // 교체할 아이템이 (현재 장착된)장비템이고
            if (beforePlayerItem.GetType() == typeof(EquipItem)) {
                // 이 슬롯의 아이템이 null이 아니면서 장비로 장착 불가능하면 교체하지 않고 리턴
                if (playerItem != null &&
                    Const.equipType[((EquipItem)beforePlayerItem).equip_index] != Const.equipType[DataManager.Instance.GetItemData(playerItem).category]) return;
                else
                {
                    // 인벤토리 리스트의 이 슬롯 아이템과 같으면 병합하고, 그렇지 않으면 이 슬롯 아이템을 제거하고 인벤토리 리스트에 교체할 아이템 추가
                    PlayerItem invenItem = FosterManager.Instance.GetPlayerData().GetItemByIndex(slotIndex);
                    if (!MergePotion(ref invenItem, beforePlayerItem))
                    {
                        FosterManager.Instance.GetPlayerData().RemoveInvenItem(playerItem);
                        if (((EquipItem)beforePlayerItem).equip_index > 2)
                            FosterManager.Instance.GetPlayerData().AddInvenItem(beforePlayerItem, slotIndex, beforePlayerItem.item_each);
                        else FosterManager.Instance.GetPlayerData().AddInvenItem(beforePlayerItem, slotIndex);
                    }
                }
            }
            else
            {
                PlayerItem invenItem = FosterManager.Instance.GetPlayerData().GetItemByIndex(slotIndex);
                if (!MergePotion(ref invenItem, beforePlayerItem))
                {
                    if(playerItem!=null) FosterManager.Instance.GetPlayerData().RemoveInvenItem(playerItem, playerItem.item_each);
                    FosterManager.Instance.GetPlayerData().AddInvenItem(beforePlayerItem, slotIndex, beforePlayerItem.item_each);
                }

            }

        }

        // 해당 슬롯과 상대 슬롯의 invenItem 객체를 교환
        SelectedSlot.Instance.SetItem(playerItem);

        //Debug.Log("OnDrop");
    }

    // 동일한 포션 아이템일 경우 병합(나머지는 이 슬롯에 있던 아이템 개수로 갱신)
    private bool MergePotion(ref PlayerItem item, PlayerItem addItem)
    {
        if (playerItem==null || playerItem.item_index != addItem.item_index || DataManager.Instance.GetItemData(playerItem).category != 3) return false;
        else
        {
            int surplus = FosterManager.Instance.GetPlayerData().AddPotion(ref item, addItem.item_each);
            if (surplus > 0)
            {
                playerItem.item_each = surplus;
            }
            else playerItem = null;
            return true;
        }
    }
}
