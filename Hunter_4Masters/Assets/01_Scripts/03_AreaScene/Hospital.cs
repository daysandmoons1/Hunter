using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Hospital : MonoBehaviour
{
    [SerializeField] private GameObject scPanel, hpPanel, willPanel,
                                        scHealOptGroup, hpHealOptGroup, willHealOptGroup, hp, will,
                                        scBtnLeft, scBtnRight;
    [SerializeField] private GameObject healEffect;

    private Image hpGauge, willGauge;
    private Text hpTxt, willTxt;

    private GameObject[] scHealOptions = new GameObject[Const.etcMaxSize];
    private Button[] scHealBtn, hpHealBtn, willHealBtn;
    private Image[] scIcon = new Image[Const.etcMaxSize];
    private Image[] scShadow, hpShadow, willShadow;    // 버튼 그림자(치료 불가 시 어둡게)
    private Text[] scHealCost, scHealTime,
                   hpHealPercent, hpHealCost, hpHealTime,
                   willHealPercent, willHealCost, willHealTime;

    private PlayerData pd = new PlayerData();
    private List<ScHospitalData> shdl = new List<ScHospitalData>();
    private List<OtherHospitalData>[] ohdl = new List<OtherHospitalData>[2];

    private IEnumerator spConSlideCrt;
    private IEnumerator healEffectCrt;

    public enum HospitalType
    {
        SpCon,
        HP,
        WILL
    }

    private void Awake()
    {
        scBtnLeft.GetComponent<Button>().onClick.RemoveAllListeners();
        scBtnRight.GetComponent<Button>().onClick.RemoveAllListeners();
        scBtnLeft.GetComponent<Button>().onClick.AddListener(() => { SlideEtc(Vector3.left); });
        scBtnRight.GetComponent<Button>().onClick.AddListener(() => { SlideEtc(Vector3.right); });

        scHealCost = new Text[Const.etcMaxSize];
        scHealTime = new Text[Const.etcMaxSize];
        scHealBtn = new Button[Const.etcMaxSize];
        scShadow = new Image[Const.etcMaxSize];
        for(int i=0; i<scIcon.Length; i++)
        {
            scHealOptions[i] = scHealOptGroup.transform.GetChild(i).gameObject;
            scHealBtn[i] = scHealOptions[i].GetComponent<Button>();
            scIcon[i] = scHealOptions[i].transform.GetChild(0).GetChild(0).GetComponent<Image>();
            scHealCost[i] = scHealOptions[i].transform.GetChild(1).GetComponent<Text>();
            scHealTime[i] = scHealOptions[i].transform.GetChild(2).GetComponent<Text>();
            scShadow[i] = scHealOptions[i].transform.GetChild(3).GetComponent<Image>();
        }

        hpGauge = hp.transform.GetChild(0).GetComponent<Image>();
        hpTxt = hp.transform.GetChild(1).GetComponent<Text>();

        hpHealPercent = new Text[Const.healOptNum];
        hpHealCost = new Text[Const.healOptNum];
        hpHealTime = new Text[Const.healOptNum];
        hpHealBtn = new Button[Const.healOptNum];
        hpShadow = new Image[Const.healOptNum];
        for (int i=0; i< Const.healOptNum; i++)
        {
            GameObject option = hpHealOptGroup.transform.GetChild(i).gameObject;
            hpHealBtn[i] = option.GetComponent<Button>();
            hpHealPercent[i] = option.transform.GetChild(0).GetComponent<Text>();
            hpHealCost[i] = option.transform.GetChild(1).GetComponent<Text>();
            hpHealTime[i] = option.transform.GetChild(2).GetComponent<Text>();
            hpShadow[i] = option.transform.GetChild(3).GetComponent<Image>();
        }

        willGauge = will.transform.GetChild(0).GetComponent<Image>();
        willTxt = will.transform.GetChild(1).GetComponent<Text>();

        willHealPercent = new Text[Const.healOptNum];
        willHealCost = new Text[Const.healOptNum];
        willHealTime = new Text[Const.healOptNum];
        willHealBtn = new Button[Const.healOptNum];
        willShadow = new Image[Const.healOptNum];
        for (int i = 0; i < Const.healOptNum; i++)
        {
            GameObject option = willHealOptGroup.transform.GetChild(i).gameObject;
            willHealBtn[i] = option.GetComponent<Button>();
            willHealPercent[i] = option.transform.GetChild(0).GetComponent<Text>();
            willHealCost[i] = option.transform.GetChild(1).GetComponent<Text>();
            willHealTime[i] = option.transform.GetChild(2).GetComponent<Text>();
            willShadow[i] = option.transform.GetChild(3).GetComponent<Image>();
        }

    }

    private void OnEnable()
    {
        ResetPopup();
        SetPopup();
    }

    public void SetPopup()
    {
        pd = FosterManager.Instance.GetPlayerData();
        ohdl = DataManager.Instance.GetOtherHospitalData();

        // 특수상태 가지고 있을 경우에만
        if (pd.Cons.ETC.Count > 0)
        {
            scPanel.SetActive(true);
            shdl = DataManager.Instance.GetScHospitalData();
            SetScOption();
        }

        hpGauge.fillAmount = pd.GetStatPercent(Const.hp);
        hpTxt.text = pd.GetStateOutOfMax(Const.hp);

        // WILL 치료 가능한 건물일 경우
        if (ohdl[1].Count > 0)
        {
            willPanel.SetActive(true);
            willGauge.fillAmount = pd.GetStatPercent(Const.will);
            willTxt.text = pd.GetStateOutOfMax(Const.will);
        }

        SetOtherOption();
    }

    // 모든 선택지 활성/비활성화 상태 초기화
    public void ResetPopup()
    {
        scBtnLeft.SetActive(false);
        scBtnRight.SetActive(false);

        for(int i=0; i<Const.etcMaxSize; i++)
        {
            scHealBtn[i].interactable = true;
            scShadow[i].enabled = false;
            scHealOptions[i].SetActive(false);
        }
        for(int i=0; i<Const.healOptNum; i++)
        {
            hpHealBtn[i].interactable = true;
            willHealBtn[i].interactable = true;
            hpShadow[i].enabled = false;
            willShadow[i].enabled = false;
        }

        scPanel.SetActive(false);
        willPanel.SetActive(false);
    }

    // 특수상태 선택지 리스트 세팅
    private void SetScOption()
    {
        // 치료 종류 4가지 이상일 경우 스크롤 활성화
        if (pd.Cons.ETC.Count > 3)
        {
            scBtnLeft.SetActive(true);
            scBtnRight.SetActive(true);
        }

        int i = 0;

        for (i = 0; i < pd.Cons.ETC.Count; i++)
        {
            foreach (ScHospitalData h in shdl)
            {
                if (pd.Cons.ETC[i].index == h.target_sc)
                {
                    if (pd.Mon_Inven.Money < h.cost_money)
                    {
                        scHealBtn[i].interactable = false;
                        scShadow[i].enabled = true;
                    }
                    scHealOptions[i].SetActive(true);
                    scHealBtn[i].onClick.RemoveAllListeners();
                    int index = i;
                    scHealBtn[i].onClick.AddListener(()=> Heal(HospitalType.SpCon, index));
                    scIcon[i].sprite = DataManager.Instance.GetSprite("icons", pd.Cons.ETC[i].name);
                    scHealCost[i].text = TextFormatter.GetMoney(h.cost_money);
                    scHealTime[i].text = TextFormatter.GetEstimatedTime(h.cost_time);
                }
            }
        }
    }

    // HP, WILL 선택지 리스트 세팅
    private void SetOtherOption()
    {
        int i = 0;

        foreach (OtherHospitalData h in ohdl[0])
        {
            if (pd.Mon_Inven.Money < h.cost_money) {
                hpHealBtn[i].interactable = false;
                hpShadow[i].enabled = true;
            }
            hpHealBtn[i].onClick.RemoveAllListeners();
            int index = i;
            hpHealBtn[i].onClick.AddListener(() => Heal(HospitalType.HP, index));
            hpHealPercent[i].text = h.healPercent + "%";
            hpHealCost[i].text = TextFormatter.GetMoney(h.cost_money);
            hpHealTime[i].text = TextFormatter.GetEstimatedTime(h.cost_time);
            i++;
        }
        foreach (OtherHospitalData h in ohdl[1])
        {
            if (pd.Mon_Inven.Money < h.cost_money)
            {
                willHealBtn[i].interactable = false;
                willShadow[i].enabled = true;
            } 
            willHealBtn[i].onClick.RemoveAllListeners();
            int index = i;
            willHealBtn[i].onClick.AddListener(() => Heal(HospitalType.WILL, index));
            willHealPercent[i].text = h.healPercent + "%";
            willHealCost[i].text = TextFormatter.GetMoney(h.cost_time);
            willHealTime[i].text = TextFormatter.GetEstimatedTime(h.cost_time);
            i++;
        }
    }

    public void Heal(HospitalType type, int index)
    {
        switch (type)
        {
            case HospitalType.SpCon:
                pd.RemoveSpecialCondition(index);
                pd.ChangeCurPoint(Const.money, -shdl[index].cost_money);
                GameManager.Instance.SkipTime((int)shdl[index].cost_time);
                break;
            case HospitalType.HP:
                pd.ChangeConsByPer(Const.hp, ohdl[0][index].healPercent);
                pd.ChangeCurPoint(Const.money, -ohdl[0][index].cost_money);
                GameManager.Instance.SkipTime((int)ohdl[0][index].cost_time);
                break;
            case HospitalType.WILL:
                pd.ChangeConsByPer(Const.will, ohdl[1][index].healPercent);
                pd.ChangeCurPoint(Const.money, -ohdl[1][index].cost_money);
                GameManager.Instance.SkipTime((int)ohdl[1][index].cost_time);
                break;
            default:
                break;
        }

        if (healEffectCrt != null)
        {
            healEffect.SetActive(false);
            StopCoroutine(healEffectCrt);
        }
        healEffectCrt = HealEffectCoroutine();
        StartCoroutine(healEffectCrt);
        
        ResetPopup();
        SetPopup();
        UIManager_Area.Instance.SetUI();
    }

    // 테스트용, 플레이어 스크립트에서 컨트롤 예정
    public IEnumerator HealEffectCoroutine()
    {
        healEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        healEffect.SetActive(false);
    }

    public void SlideEtc(Vector3 dir)
    {
        if(spConSlideCrt!=null) StopCoroutine(spConSlideCrt);
        spConSlideCrt = SpConSlideCoroutine(dir);
        StartCoroutine(spConSlideCrt);
    }

    private IEnumerator SpConSlideCoroutine(Vector3 dir)
    {
        RectTransform rt = scHealOptGroup.GetComponent<RectTransform>();
        Vector2 destPos = new Vector2(Mathf.Clamp(rt.anchoredPosition.x - 190 * dir.x, -rt.sizeDelta.x , 0), rt.anchoredPosition.y);
        float offset = 0f;
        while (rt.anchoredPosition != destPos && offset < 1.5f)
        {
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, destPos, Time.deltaTime * 4.5f);
            offset += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = destPos;
    }
}
