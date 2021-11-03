using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectedSlot : Singleton<SelectedSlot>
{
    public RectTransform rt;    // 터치 좌표와 오브젝트 좌표 맞춤 기준

    //[SerializeField] private GameObject itemInfo; // 아이템 정보창
    [SerializeField] private Image dragImage;     // 드래그 시 활성화될 이미지
    //[SerializeField] private GameObject equipErrorMsg;

    public PlayerItem playerItem;    // 드래그 대상 아이템
    public PlayerItem selectedItem; // 터치된 아이템(정보표시에 사용)
    public bool isSelected = false;
    //private ItemInfo info;
    //public Image touchPanel;

    public Vector2 dragOffset = new Vector2(0, 0);
    private Vector2 InfoOffset = new Vector2(0,0);
    private Vector2 inputDir;   // 터치 좌표

    private void Start()
    {
        //touchPanel = GetComponent<Image>();
        //info = itemInfo.GetComponent<ItemInfo>();
    }

    //private void OnMouseDown()
    //{
    //    itemInfo.SetActive(false);
    //    touchPanel.enabled = false;
    //}

    public void SetSelectedItem(PlayerItem playeItem)
    {
        selectedItem = playeItem;
    }

    public void SetItem(PlayerItem playerItem)
    {
        this.playerItem = playerItem;
    }

    public void SetDragSlotPos(PointerEventData ped)
    {
        dragOffset = new Vector2(rt.rect.width, rt.rect.height);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, ped.position, Camera.main, out inputDir);
        //dragImage.rectTransform.anchoredPosition = (inputDir + dragOffset);
        dragImage.rectTransform.localPosition = ped.position - (dragOffset*0.5f);
    }

    public void SetItemInfoPos(PointerEventData ped)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, ped.position, Camera.main, out inputDir);
        //itemInfo.GetComponent<Image>().rectTransform.anchoredPosition = inputDir+InfoOffset;
    }

    public void SetActiveItemInfo(bool activeState)
    {
        //itemInfo.SetActive(activeState);
        if(activeState && playerItem != null)
        {
            //info.SetItemInfo(DataManager.Instance.GetItemData(playerItem));
            //itemInfoTxt.text = DataManager.Instance.GetItemData(playerItem.item_name).GetItemDescription();
        }
    }

    public void ResetDragSlot()
    {
        SetDragImageColor(0);
        playerItem = null;
        dragImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetDragSlotImage(Image itemImage)
    {
        dragImage.sprite = itemImage.sprite;
        SetDragImageColor(0.5f);
    }

    public void SetDragImageColor(float alpha)
    {
        Color color = dragImage.color;
        color.a = alpha;
        dragImage.color = color;
    }
}
