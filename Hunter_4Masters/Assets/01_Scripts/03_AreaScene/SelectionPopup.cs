using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPopup : MonoBehaviour
{
    [SerializeField] private GameObject simulationPanel;
    private SimulationPopup simulationPopup;

    // 실행 행동 UI
    [SerializeField] private Image actionImg;

    // 카테고리 및 선택지 UI
    [SerializeField] private GameObject scrollView;
    private GameObject optionGroup;
    private GameObject[] options = new GameObject[10];
    private Text[] optionName = new Text[10];
    private Text[,] optionContents = new Text[10, 4];
    private Text[,] optionTitleTxts = new Text[10, 4];
    private Image[,] optionTitleImgs_rt = new Image[10, 4];
    private Image[,] optionTitleImgs_sq = new Image[10, 4];
    private RectTransform optionGroupRT;
    private float scrollViewHeight;
    private float optionBtnHeight;

    private PlayerData pd;
    private ChoiceData title;
    private Dictionary<string, ChoiceData> cd;

    // 행동 선택, 선택지 선택 시 값이 할당됨
    private string executedAction;
    private string selectedOption;

    // SimulationPopup에 보내는 데이터. 모두 8개씩 들어감
    private static int eventNum = 8;
    private KeyValuePair<string, EventData>[] occurEventArray = new KeyValuePair<string, EventData>[eventNum];
    private Dictionary<string, float>[] changesArray = new Dictionary<string, float>[eventNum];    // 변화량 합산할때도 사용


    private void Awake()
    {
        simulationPopup = simulationPanel.GetComponent<SimulationPopup>();
        optionGroup = scrollView.transform.GetChild(0).GetChild(0).gameObject;
        for (int i = 0; i < optionGroup.transform.childCount; i++)
        {
            options[i] = optionGroup.transform.GetChild(i).gameObject;
            optionName[i] = options[i].transform.GetChild(0).GetComponent<Text>();
            for(int j=0; j<4; j++)
            {
                optionTitleImgs_rt[i, j] = options[i].transform.GetChild(j + 1).GetChild(0).GetComponent<Image>();
                optionTitleImgs_sq[i, j] = options[i].transform.GetChild(j + 1).GetChild(1).GetComponent<Image>();
                optionTitleTxts[i, j] = options[i].transform.GetChild(j + 1).GetChild(2).GetComponent<Text>();

                optionContents[i, j] = options[i].transform.GetChild(j + 5).GetComponent<Text>();
            }
        }
        optionGroupRT = optionGroup.GetComponent<RectTransform>();
        // 선택지 버튼의 height값 가져오기(이후 선택지 개수에 따라 선택지박스 height 조정됨)
        scrollViewHeight = scrollView.GetComponent<RectTransform>().rect.height;
        optionBtnHeight = options[0].GetComponent<RectTransform>().rect.height;
    }

    public void SetPopup(string action)
    {
        this.executedAction = action;

        pd = FosterManager.Instance.GetPlayerData();
        title = DataManager.Instance.GetChoiceTitle(action);
        cd = DataManager.Instance.GetChoiceData(action, 0);  //일단 areaCode 0으로 넣음

        // 행동 이름 표시
        actionImg.sprite = DataManager.Instance.GetSprite("icons",action);

        // 선택지 리셋
        for (int i=0; i<options.Length; i++)
        {
            options[i].GetComponent<Button>().onClick.RemoveAllListeners();
            options[i].SetActive(false);
        }

        // 선택지박스 스크롤높이 & 선택지 데이터 UI 적용
        optionGroupRT.offsetMin = new Vector2(optionGroupRT.offsetMin.x, ((scrollViewHeight/optionBtnHeight) - cd.Count) * optionBtnHeight);
        int index = 0;
        foreach (KeyValuePair<string, ChoiceData> pair in cd)
        {
            SetOptionUI(pair, index);
            options[index].SetActive(true);
            index++;
        }

    }

    // (하나의) 선택지버튼 UI를 세팅
    private void SetOptionUI(KeyValuePair<string, ChoiceData> pair, int index)
    {
        optionName[index].text = pair.Value.name;

        int i = 0;
        for (i = 0; i < pair.Value.consume.Count; i++)
        {
            // 돈과 시간만 형식이 지정되어있고 나머지는 단순 값 출력
            switch (title.consume[i].consume_variable)
            {
                case Const.money:
                    SetOptionTitle(Const.money,index,i);
                    optionContents[index,i].text = TextFormatter.GetMoney(pair.Value.consume[i].consume_value);
                    break;
                case Const.time:
                    SetOptionTitle(Const.time, index, i);
                    optionContents[index, i].text = TextFormatter.GetEstimatedTime(pair.Value.consume[i].consume_value);
                    break;
                default:
                    SetOptionTitle(title.consume[i].consume_variable, index, i);
                    optionContents[index, i].text = pair.Value.consume[i].consume_value.ToString();
                    break;
            }
        }

        // 조건이 있는 경우 세팅
        if(pair.Value.condition.Count!=0 && pair.Value.condition[0].condition_variable != Const.defStr2)
        {
            SetOptionTitle("condition", index, i);

            for (int j = 0; j < pair.Value.condition.Count; j++)
            {
                if (j != 0) optionContents[index, i].text += "/";
                optionContents[index, i].text += DataManager.Instance.GetConditionRange(pair.Value.condition[j]);
            }
        }

        // 추가정보가 있는 경우 세팅(*훈련 기준, 나머지 행동데이터 추가 시 변경 예정)
        if (title.plusInfo != Const.defStr2)
        {
            string[] plusInfo = pair.Value.plusInfo.Split('/');
            SetOptionTitle(plusInfo[0], index, 3);
            optionContents[index, 3].text = plusInfo[1];
        }
        else
        {
            optionTitleImgs_rt[index, 3].enabled = false;
            optionTitleImgs_sq[index, 3].enabled = false;
            optionContents[index, 3].text = "";
        }

        options[index].GetComponent<Button>().onClick.AddListener(() => OptionClick(pair.Key));
        options[index].GetComponent<Button>().interactable = IsOptionAvailable(pair.Value);
    }

    private void SetOptionTitle(string title, int index, int subIndex)
    {
        if(title == Const.money || title == Const.time || title == "condition")
        {
            optionTitleImgs_rt[index, subIndex].enabled = false;
            optionTitleImgs_sq[index, subIndex].enabled = true;
            optionTitleImgs_sq[index, subIndex].sprite = (index == 0) ? 
                DataManager.Instance.GetSprite("icons", title) : optionTitleImgs_sq[0, subIndex].sprite;
            optionTitleTxts[index, subIndex].text = "";
        }
        else
        {
            optionTitleImgs_rt[index, subIndex].enabled = true;
            optionTitleImgs_sq[index, subIndex].enabled = false;
            optionTitleTxts[index, subIndex].text = title;
        }
    }

    // 육성데이터와 비교하여 선택가능한 선택지인지 확인
    private bool IsOptionAvailable(ChoiceData choiceData)
    {
        for (int i = 0; i < choiceData.consume.Count; i++)
        {
            string variable = choiceData.consume[i].consume_variable;
            if (variable == Const.time)
            {
                if (GameManager.Instance.curTimeVal + choiceData.consume[i].consume_value >= 1440) return false; // 실행 후 24시가 넘어간다면 선택불가
            }
            else if (pd.GetCurPoint(variable) + choiceData.consume[i].consume_value < 0) return false;
        }

        return true;
    }

    public void OptionClick(string option)
    {
        this.selectedOption = option;
        MakeOccurEvents();
        this.gameObject.SetActive(false);
        simulationPanel.SetActive(true);
        simulationPopup.Simulate(occurEventArray, changesArray, pd, GetAfterPd());
    }

    private void MakeOccurEvents()
    {
        List<KeyValuePair<string, EventData>>[] secondaryEventData = new List<KeyValuePair<string, EventData>>[cd[selectedOption].events.Count];
        // 현재 육성데이터 수치와 선택지에 따라서 발생하게 될 이벤트(8개)를 가중치 랜덤값으로 결정하고
        // 해당 이벤트에서 발생할 특정요소의 변화량(랜덤) 결정(ex: 무장강도, hp -20)

        // 각 이벤트에 대한 가중치 계산 & 2차이벤트 저장
        float weightPer = 0;
        List<float> weightNum = new List<float>();
        for (int i = 0; i < cd[selectedOption].events.Count; i++)
        {
            weightPer += cd[selectedOption].events[i].event_per;
            weightNum.Add(weightPer);
            Debug.Log(cd[selectedOption].events[i].event_ID + "=" + weightPer + ", ");
            secondaryEventData[i] = DataManager.Instance.GetSecondaryEventList(executedAction, cd[selectedOption].events[i].event_ID);
        }

        // 발생 이벤트 랜덤생성

        for (int i = 0; i < eventNum; i++)
        {
            float randomNum = Random.Range(0, 100.1f);
            int randomIndex = 0;

            // 랜덤숫자가 속하는 인덱스의 이벤트 저장(ex: '걷기' 선택지에서 랜덤값이 30이 나왔을 경우 2번째 이벤트 발생함)
            for (int j = 0; j < weightNum.Count; j++)
            {
                if (randomNum <= weightNum[j])
                {
                    randomIndex = j;
                    break;
                }
            }

            occurEventArray[i] = MakeSecondaryOccurEvent(DataManager.Instance.GetEventDataById(executedAction, cd[selectedOption].events[randomIndex].event_ID), secondaryEventData[randomIndex]);


            // 해당 이벤트에서 발생할 특정요소의 변화량(랜덤) 결정(ex: 무장강도, hp -20)
            Dictionary<string, float> changes = new Dictionary<string, float>();
            for (int j = 0; j < occurEventArray[i].Value.effect.Count; j++)
            {
                int changeAmount = (int)(Random.Range(occurEventArray[i].Value.effect[j].GetMinValue(), occurEventArray[i].Value.effect[j].GetMaxValue()));
                changes.Add(occurEventArray[i].Value.effect[j].effect_variable, changeAmount);
            }
            changesArray[i] = changes;

        }
    }

    // 이벤트에서 발생하는 2차 이벤트 결정
    private KeyValuePair<string, EventData> MakeSecondaryOccurEvent(KeyValuePair<string, EventData> primaryData, List<KeyValuePair<string, EventData>> secondaryEventData)
    {
        // 각 이벤트에 대한 가중치 계산
        float weightPer = 0;
        List<float> weightNum = new List<float>();
        for (int i = 0; i < secondaryEventData.Count; i++)
        {
            if (!IsEventAvailable(secondaryEventData[i].Value)) continue;
            // string형 확률을 float값으로 전환 (육성데이터값 대입)
            for (int j = 0; j < secondaryEventData[i].Value.condition.Count; j++)
            {
                weightPer += StringCalculator.Calculate(secondaryEventData[i].Value.condition[j].condition_per);
            }
            weightNum.Add(weightPer);
            Debug.Log(secondaryEventData[i].Value.data_ID + "=" + weightPer + ", ");
        }

        float randomNum = Random.Range(0, 100.1f);
        int randomIndex = -1;

        // 랜덤숫자가 속하는 2차이벤트의 인덱스 찾기(없으면 1차데이터 리턴)
        for (int i = 0; i < weightNum.Count; i++)
        {
            if (randomNum <= weightNum[i])
            {
                randomIndex = i;
                break;
            }
        }
        return (randomIndex == -1) ? primaryData : secondaryEventData[randomIndex];
    }

    // 육성데이터와 비교하여 이벤트가 발생가능한지 체크
    private bool IsEventAvailable(EventData eventData)
    {
        for (int i = 0; i < eventData.condition.Count; i++)
        {
            string variable = eventData.condition[i].condition_variable;
            // variable이 null이 아니고 조건 범위에 속하지 않으면 return false
            if (variable != "null" && (pd.GetCurPoint(variable) < eventData.condition[i].condition_min ||
                pd.GetCurPoint(variable) > eventData.condition[i].condition_max)) return false;
        }
        return true;
    }

    private PlayerData GetAfterPd()
    {
        PlayerData afterPd = (PlayerData)pd.Clone();

        // 선택지로 인해 변화한 값 적용
        for (int i = 0; i < cd[selectedOption].consume.Count; i++)
        {
            // AddToCurPoint에서 처리가능하나 현재 선택지 데이터의 시간소모가 양수인 관계로 여기서 처리해줌.
            if(cd[selectedOption].consume[i].consume_variable == Const.time)
            {
                GameManager.Instance.tempSpendTime -= (int)cd[selectedOption].consume[i].consume_value;
            }
            else afterPd.ChangeCurPoint(cd[selectedOption].consume[i].consume_variable, cd[selectedOption].consume[i].consume_value);
        }

        // 이벤트로 인해 변화한 값 적용
        for (int i = 0; i < eventNum; i++)
        {
            foreach (KeyValuePair<string, float> pair in changesArray[i])
            {
                if (pair.Key != Const.defStr2)
                {
                    afterPd.ChangeCurPoint(pair.Key, pair.Value);
                    if (afterPd.GetCurPoint(pair.Key) < 0) afterPd.ChangeCurPoint(pair.Key, -afterPd.GetCurPoint(pair.Key));
                }
            }
        }
        return afterPd;
    }
}
