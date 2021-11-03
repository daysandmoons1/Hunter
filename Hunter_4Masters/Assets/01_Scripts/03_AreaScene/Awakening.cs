using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Awakening : MonoBehaviour
{
    [SerializeField] private GameObject curStat, awakeDesc;
    [SerializeField] private Text costMoneyTxt;
    [SerializeField] private Button awakeBtn;

    private Text[] curStatTxt, awakeDescTxt;

    private int[] amount = { 10, 50, 70, 100 };
    private int[] percent = { 1, 35, 60, 4 };

    private int[,] awakePlus = new int[Const.defStats.Length, 4];
    private float costMoney = 0;
    private PlayerData pd = new PlayerData();

    private void Awake()
    {
        curStatTxt = new Text[Const.defStats.Length];
        awakeDescTxt = new Text[Const.defStats.Length];
        for(int i=0; i<Const.defStats.Length; i++)
        {
            curStatTxt[i] = curStat.transform.GetChild(i).GetComponent<Text>();
            awakeDescTxt[i] = awakeDesc.transform.GetChild(i).GetComponent<Text>();
        }
    }

    private void OnEnable()
    {
        pd = FosterManager.Instance.GetPlayerData();
        costMoney = Mathf.Pow(2, FosterManager.Instance.GetAwakeningCnt()) * 100 - 100;
        GetAwakeVal();
        SetPopup();
    }

    private void GetAwakeVal()
    {
        for (int i = 0; i < Const.defStats.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                awakePlus[i, j] = Mathf.RoundToInt(pd.GetCurPoint(Const.defStats[i]) * amount[j] * 0.01f);
            }
        }
    }

    private int GetRandomIndex()
    {
        float randomVal = Random.Range(0, 100.1f);
        int percentSum = 0;
        int randomIndex = -1;

        for (int i = 0; i < percent.Length; i++)
        {
            percentSum += percent[i];
            if (randomVal <= percentSum)
            {
                randomIndex = i;
                break;
            }
        }

        return randomIndex;
    }

    public void SetPopup()
    {
        awakeBtn.interactable = (costMoney <= pd.Mon_Inven.Money);
        
        for (int i = 0; i < Const.defStats.Length; i++)
        {
            curStatTxt[i].text = "("+pd.GetStateOutOfMax(Const.defStats[i])+")";
            awakeDescTxt[i].text = "최대치 " + (pd.GetStatMax(Const.defStats[i]) + awakePlus[i, 0])+" ~ "+
                                               (pd.GetStatMax(Const.defStats[i]) + awakePlus[i, 3]);
        }
        costMoneyTxt.text = TextFormatter.GetMoney(costMoney);
    }

    // 각성 실행
    public void Execute()
    {
        int randomIndex = GetRandomIndex();

        for (int i=0; i<Const.defStats.Length; i++)
        {
            pd.IncreaseMaxPoint(Const.defStats[i], awakePlus[i,randomIndex]);
        }
        GameManager.Instance.SkipTime(12 * 60);
        FosterManager.Instance.Awakening();
        pd.ChangeCurPoint(Const.money, -costMoney);
        Exit();
    }

    public void Exit()
    {
        gameObject.SetActive(false);
        GameManager.Instance.Resume();
    }
}
