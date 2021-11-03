using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequestPopup : MonoBehaviour
{
    private GameObject reqInfoPanel;
    private string[] curRequestKey = new string[Const.requestNum];
    private int selectedReqIndex = -1;

    [SerializeField] private GameObject[] request = new GameObject[Const.requestNum+1];
    [SerializeField] private Image selectFrame, playerRank, SPGauge;
    [SerializeField] private Text reqDesc, reqCSP, reqCTime, SPText, curDDay, curTime, curMoney;
    [SerializeField] private Button battleBtn;

    private Button[] reqButton = new Button[Const.requestNum];

    private Image[] areaColor = new Image[Const.requestNum+1];
    private Image[] areaImage = new Image[Const.requestNum+1];
    private Image[] reqRank = new Image[Const.requestNum+1];
    private Image[] reqType = new Image[Const.requestNum+1];

    private Text[] areaName = new Text[Const.requestNum+1];
    private Text[] reqName = new Text[Const.requestNum+1];
    private Text[] rewardMoney = new Text[Const.requestNum+1];
    private Text[] rewardItem = new Text[Const.requestNum+1];

    // 임시 사용
    [SerializeField] private RequestResultPopup reqResultPopup;

    private void Awake()
    {
        reqInfoPanel = request[Const.requestNum].transform.parent.gameObject;

        for(int i=0; i< Const.requestNum+1; i++)
        {
            areaColor[i] = request[i].transform.GetChild(0).GetComponent<Image>();
            areaName[i] = request[i].transform.GetChild(1).GetComponent<Text>();
            areaImage[i] = request[i].transform.GetChild(2).GetChild(0).GetComponent<Image>();
            reqName[i] = request[i].transform.GetChild(3).GetComponent<Text>();
            reqRank[i] = request[i].transform.GetChild(4).GetChild(0).GetComponent<Image>();
            reqType[i] = request[i].transform.GetChild(5).GetChild(0).GetComponent<Image>();
            rewardMoney[i] = request[i].transform.GetChild(7).GetComponent<Text>();
            rewardItem[i] = request[i].transform.GetChild(8).GetComponent<Text>();
        }

        for(int i=0; i< Const.requestNum; i++)
        {
            reqButton[i] = request[i].transform.GetChild(9).GetComponent<Button>();
            int index = i;
            reqButton[i].onClick.AddListener(()=> { SelectRequest(index); });
        }
    }

    private void OnEnable()
    {
        SetPlayerStat(FosterManager.Instance.GetPlayerData());
        reqInfoPanel.SetActive(false);
        selectFrame.enabled = false;
        battleBtn.interactable = false;
        selectedReqIndex = -1;

        SetPopup(DataManager.Instance.GetCurRequest());
    }

    // 의뢰 선택
    public void SelectRequest(int index)
    {
        if (selectedReqIndex == index) return;
        selectedReqIndex = index;
        battleBtn.interactable = true;


        // 선택된 옵션으로 프레임 이미지 세팅
        if (!selectFrame.enabled) selectFrame.enabled = true;
        reqInfoPanel.SetActive(true);
        selectFrame.transform.position = request[selectedReqIndex].transform.position;
        selectFrame.rectTransform.sizeDelta = request[selectedReqIndex].GetComponent<Image>().rectTransform.sizeDelta;

        // 상세정보창 세팅 후 선택된 옵션으로 selectedReqIndex 갱신
        Request req = DataManager.Instance.GetRequest(curRequestKey[index]);
        SetRequest(Const.requestNum, req);
        reqDesc.text = req.detail;
        reqCSP.text = DataManager.Instance.GetRequestCost(CostType.SP, req.map).ToString();
        reqCTime.text = TextFormatter.GetEstimatedTime(DataManager.Instance.GetRequestCost(CostType.TIME, req.map));
    }

    // 출정
    public void StartBattle()
    {
        gameObject.SetActive(false);
        // 전투기능 구현예정 - 현재는 바로 결과창 표시
        reqResultPopup.ShowPopup(DataManager.Instance.GetRequest(curRequestKey[selectedReqIndex]));
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
        GameManager.Instance.Resume();
    }

    // 현재 캐릭터 상태바 세팅
    private void SetPlayerStat(PlayerData pd)
    {
        playerRank.sprite = DataManager.Instance.GetSprite("icons", "rank_A");
        SPGauge.fillAmount = pd.GetStatPercent(Const.sp);
        SPText.text = pd.GetStateOutOfMax(Const.sp);
        curDDay.text = GameManager.Instance.GetDDay();
        curTime.text = GameManager.Instance.GetCurrentTimeByValue();
        curMoney.text = pd.GetMoney();
    }

    // 의뢰UI 세팅
    private void SetPopup(string[] requestKey)
    {
        for (int i = 0; i < Const.requestNum; i++)
        {
            // 키값이 현재와 같으면 갱신하지 않음
            if (curRequestKey[i] == requestKey[i]) continue;

            SetRequest(i, DataManager.Instance.GetRequest(requestKey[i]));

            // 팝업 스크립트 내의 키값 갱신
            curRequestKey[i] = requestKey[i];
        }
    }

    // 하나의 의뢰 UI 세팅
    private void SetRequest(int i, Request req)
    {
        areaColor[i].color = Const.reqMapColor[req.map];
        areaName[i].text = Const.requestMap[req.map];
        areaImage[i].sprite = DataManager.Instance.GetSprite("request", Const.requestMap[req.map]);
        reqName[i].text = req.name;
        reqRank[i].sprite = DataManager.Instance.GetSprite("icons", "rank_"+req.rank);
        reqType[i].sprite = DataManager.Instance.GetSprite("request", Const.requestType[req.clear_condition]);
        rewardMoney[i].text = TextFormatter.GetMoney(req.reward_money);
        rewardItem[i].gameObject.SetActive(false);
        if (req.reward_item.Length > 0)
        {
            rewardItem[i].gameObject.SetActive(true);
            rewardItem[i].text = "";
            for (int j = 0; j < req.reward_item.Length; j++)
            {
                if (j > 0) rewardItem[i].text += "\n";
                rewardItem[i].text += DataManager.Instance.GetItemData(req.reward_item[j]).name;
            }
        }

    }
}
