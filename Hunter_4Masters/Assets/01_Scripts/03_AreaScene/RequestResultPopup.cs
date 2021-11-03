using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RequestResultPopup : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject requestInfo, resultInfo, rewardMoney, rewardItemInfo;
    [SerializeField] private Image requestAreaImg, beforeRank, afterRank;
    [SerializeField] private Text requestName, resultDesc, resultSuccess, resultRank, rewardMoneyTxt, itemZero;

    private GameObject[] rewardItem;
    private Image[] rewardItemImg;
    private IEnumerator resCrt;
    private float waitTime = 1f;

    // 테스트용 임시 데이터
    private int playerClearAmount = 3;
    private string[] playerRank = { "A", "S" };
    private Dictionary<int, string> objectDict = new Dictionary<int, string>()
    {
        {555555, "스켈레톤"},
        {555556, "마정석"},
    };

    private void Awake()
    {
        int rewardItemNum = rewardItemInfo.transform.GetChild(1).childCount;
        rewardItem = new GameObject[rewardItemNum];
        rewardItemImg = new Image[rewardItemNum];
        for(int i=0; i<rewardItemImg.Length; i++)
        {
            rewardItem[i] = rewardItemInfo.transform.GetChild(1).GetChild(i).gameObject;
            rewardItemImg[i] = rewardItem[i].transform.GetChild(0).GetComponent<Image>();
        }

        ResetPopup();
    }

    public void ResetPopup()
    {
        requestInfo.SetActive(false);
        afterRank.color = new Color(1f, 1f, 1f, 0f);
        resultInfo.SetActive(false);
        rewardMoney.SetActive(false);
        if (itemZero.enabled) itemZero.enabled = false;
        for (int i = 0; i < rewardItem.Length; i++)
        {
            rewardItem[i].SetActive(false);
        }
        rewardItemInfo.SetActive(false);
    }

    public void SetPopup(Request req)
    {
        requestAreaImg.sprite = DataManager.Instance.GetSprite("request", Const.requestMap[req.map]);

        requestName.text = req.name;

        if (req.clear_amount <= 0)
        {   //정찰 의뢰
            resultDesc.text = Const.requestMap[req.map] + " " + Const.requestType[req.clear_condition] + " 완료";
        }
        else
        {
            resultDesc.text = 
                objectDict[req.clear_target] + 
                " (" + playerClearAmount + "/" + req.clear_amount + ")개 " +
                Const.requestType[req.clear_condition];
        }

        resultSuccess.text = "< 의뢰를 " + ((req.clear_amount<=playerClearAmount)? "성공":"실패") + "하였습니다 >";

        beforeRank.sprite = DataManager.Instance.GetSprite("icons", "rank_" + playerRank[0]);
        afterRank.sprite = DataManager.Instance.GetSprite("icons", "rank_" + playerRank[1]);

        resultRank.text = "랭크 " + ((playerRank[0] == playerRank[1])? "유지" : "상승");

        rewardMoneyTxt.text = TextFormatter.GetMoney(req.reward_money);

        if (req.reward_item.Length == 0) itemZero.enabled = true;
        else
        {
            for (int i = 0; i < 2; i++)
            {
                rewardItem[i].SetActive(true);
                rewardItemImg[i].sprite = DataManager.Instance.GetSprite("Items", DataManager.Instance.GetItemKey(req.reward_item[i]));
            }
        }
    }

    public void ShowPopup(Request req)
    {
        gameObject.SetActive(true);
        SetPopup(req);
        resCrt = ShowResultCoroutine();
        StartCoroutine(resCrt);
    }

    public void Skip()
    {
        StopCoroutine(resCrt);

        requestInfo.SetActive(true);
        if (playerRank[0] != playerRank[1]) afterRank.color = new Color(1f, 1f, 1f, 1f);
        resultInfo.SetActive(true);
        rewardMoney.SetActive(true);
        rewardItemInfo.SetActive(true);
    }

    public void Exit()
    {
        ResetPopup();
        gameObject.SetActive(false);
        GameManager.Instance.Resume();
    }

    private IEnumerator ShowResultCoroutine()
    {
        requestInfo.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        resultInfo.SetActive(true);
        yield return new WaitForSeconds(waitTime*0.7f);
        if (playerRank[0] != playerRank[1])
        {
            Color color = afterRank.color;

            float alpha = 0f;
            while (alpha < waitTime)
            {
                color.a = alpha;
                afterRank.color = color;
                alpha += Time.deltaTime;
                yield return null;
            }
            afterRank.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(waitTime*0.5f);
        }
        rewardMoney.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        rewardItemInfo.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Skip();
    }
}
