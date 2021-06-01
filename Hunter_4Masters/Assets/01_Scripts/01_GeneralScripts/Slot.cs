﻿using System.Collections;
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

    [SerializeField] private GameObject itemEach;
    [SerializeField] private Text itemEachTxt;
    public Image itemImage;
    public Sprite defaultSprite;

    private bool isPressed = false;
    private float pressTime = 0;

    public void SetItem(PlayerItem newPlayerItem)
    {
        if (newPlayerItem != null && newPlayerItem.item_name != "none")
        {
            if(slotType == Constants.Equip)
            {
                playerItem = new EquipItem(){
                    equip_index = ((EquipItem)newPlayerItem).equip_index,
                    item_name = newPlayerItem.item_name,
                    item_each = newPlayerItem.item_each
                };
            }
            else
            {
                playerItem = new InvenItem()
                {
                    inven_index = ((InvenItem)newPlayerItem).inven_index,
                    item_name = newPlayerItem.item_name,
                    item_each = newPlayerItem.item_each
                };
            }
        }
        else playerItem = null;
        itemImage.sprite = defaultSprite;
        Color color = itemImage.color;
        color.a = 1f;
        itemEach.SetActive(false);

        if (playerItem != null)
        {
            if (slotType == Constants.Inven)    // 인벤슬롯일 때
            {
                ((InvenItem)playerItem).inven_index = slotIndex;
                // 소비 아이템일 경우 개수 표시
                itemEach.SetActive(DataManager.Instance.GetItemData(playerItem.item_name).type == Constants.consumable);
                itemEachTxt.text = playerItem.item_each.ToString();

            }
            else if(slotType == Constants.Equip)    // 장비슬롯일 때
            {

            }
            itemImage.sprite = Resources.Load("Items/" + playerItem.item_name, typeof(Sprite)) as Sprite;
        }
        else
        {
            if (slotType == Constants.Inven) color.a = 0f;
        }
        itemImage.color = color;
    }

    // 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        SelectedSlot.Instance.SetSelectedItem(playerItem);
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
                SelectedSlot.Instance.SetItemInfoPos(eventData);
            }
            pressTime += Time.deltaTime;
            yield return null;
        }
        pressTime = 0;
        SelectedSlot.Instance.SetActiveItemInfo(false);
        yield return null;
    }

    // 같은 위치에서 눌렀다 뗐을 때
    public void OnPointerClick(PointerEventData eventData)
    {
        isPressed = false;
        // 해당 슬롯에 소비아이템이 있을 경우 사용 팝업 띄우기
        //Debug.Log("OnPointerClick");
    }

    // 드래그 시작 시
    public void OnBeginDrag(PointerEventData eventData)
    {
        isPressed = false;
        if (playerItem != null)
        {
            SelectedSlot.Instance.SetDragSlotImage(itemImage);
            SelectedSlot.Instance.SetDragSlotPos(eventData);
            if (slotType == Constants.Equip) itemImage.sprite = defaultSprite;
            else itemImage.enabled = false;
            itemEach.SetActive(false);
        }
        //Debug.Log("OnBeginDrag");
    }

    // 드래그 중인 동안
    public void OnDrag(PointerEventData eventData)
    {
        if (playerItem != null)
        {
            SelectedSlot.Instance.SetDragSlotPos(eventData);
        }
        //Debug.Log("OnDrag");
    }

    // 드래그 끝났을 때
    public void OnEndDrag(PointerEventData eventData)
    {
        PlayerItem afterPlayerItem = SelectedSlot.Instance.playerItem;

        // 아이템이 교체되었을 경우
        if (afterPlayerItem != playerItem) {
            // 장비슬롯이라면 - 교체할 아이템이 null이면 해당 슬롯번호와 같은 인덱스의 장비 리셋 / 그렇지 않으면 교체할 아이템을 해당 슬롯번호의 장비인덱스에 대입.
            if (slotType == Constants.Equip)
            {
                if (afterPlayerItem == null)
                {
                    FosterManager.Instance.GetPlayerData().ResetEquipItem(slotIndex);
                }
                else FosterManager.Instance.GetPlayerData().SetEquipItem(slotIndex, afterPlayerItem.item_name);
            }
            else
            {   // 인벤슬롯이라면

                // 인벤토리 리스트에서 해당 슬롯 아이템을 제거
                FosterManager.Instance.GetPlayerData().RemoveInvenItem(playerItem, playerItem.item_each);
                // 교체할 아이템이 null이 아니면 인벤토리 리스트에 교체할 아이템 추가
                if (afterPlayerItem != null) FosterManager.Instance.GetPlayerData().AddInvenItemAt(slotIndex, afterPlayerItem.item_name, afterPlayerItem.item_each);
            }
        }
        PlayerInfoPopup.Instance.setInfo();
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
        PlayerItem afterPlayerItem = SelectedSlot.Instance.playerItem;
        if (afterPlayerItem == null) return;
        if (slotType == Constants.Equip)
        {   // 장비슬롯에 드롭했을 때
            
            if (Constants.equipType[slotIndex] != DataManager.Instance.GetItemData(afterPlayerItem.item_name).type) return; // 장착불가한 아이템이면 리턴
            else
            {
                // 장비 리스트에 교체할 아이템을 장착.
                FosterManager.Instance.GetPlayerData().SetEquipItem(slotIndex, afterPlayerItem.item_name);
            }
        }
        else
        {   // 인벤슬롯에 드롭했을 때

            // 교체할 아이템이 장비슬롯으로부터 가져왔고, 해당 슬롯에 아이템이 있으면서 장비로 장착 불가능하면 교체하지 않고 리턴
            if (afterPlayerItem.GetType() == typeof(EquipItem) &&
                playerItem != null && Constants.equipType[((EquipItem)afterPlayerItem).equip_index] != DataManager.Instance.GetItemData(playerItem.item_name).type) return;

            // 인벤토리 리스트에서 해당 슬롯 아이템을 제거
            if(playerItem!=null) FosterManager.Instance.GetPlayerData().RemoveInvenItem(playerItem, playerItem.item_each);
            // 인벤토리 리스트에 교체할 아이템 추가
            FosterManager.Instance.GetPlayerData().AddInvenItemAt(slotIndex, afterPlayerItem.item_name, afterPlayerItem.item_each);

        }
        // 해당 슬롯과 상대 슬롯의 invenItem 객체를 교환
        SelectedSlot.Instance.SetSelectedItem(playerItem);

        //Debug.Log("OnDrop");
    }
}