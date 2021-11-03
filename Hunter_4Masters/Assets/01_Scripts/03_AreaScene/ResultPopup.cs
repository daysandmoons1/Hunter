using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class ResultPopup : MonoBehaviour
{
    private Dictionary<string, GameObject> Stats;

    private PlayerData beforePd, afterPd;

    private float duration = 1.0f; // 카운팅에 걸리는 시간 설정.

    private void Awake()
    {
        Stats = new Dictionary<string, GameObject>();
        Transform statPanel = transform.GetChild(0).GetChild(0);
        for (int i = 0; i < statPanel.childCount; i++)
        {
            Stats.Add(statPanel.GetChild(i).name, statPanel.GetChild(i).gameObject);
        }
    }

    public void SetPopup(KeyValuePair<string, EventData>[] oea, Dictionary<string, float>[] ca, PlayerData beforePd, PlayerData afterPd)
    {
        this.beforePd = beforePd;
        this.afterPd = afterPd;

        for (int i = 0; i < oea.Length; i++)
        {
            foreach (var pair in ca[i])
            {
                Debug.Log(i + "번째 딕셔너리 변경 항목 출력 : " + pair.Key);
                SetStatValue(pair.Key);
            }
        }

        FosterManager.Instance.SetPlayerData(afterPd);
    }

    // 하나의 스탯게이지를 세팅.(이전의 PlayAnim)
    void SetStatValue(string statName)
    {
        if (!Stats.ContainsKey(statName)) return;

        Stats[statName].SetActive(true);

        float target = afterPd.GetCurPoint(statName);
        float current = beforePd.GetCurPoint(statName);

        Text deltaTxt = Stats[statName].transform.Find("Delta").GetComponent<Text>();

        if (statName == Const.time)
        {
            target = current + GameManager.Instance.tempSpendTime;
            StartCoroutine(NumCoroutine(statName, target, current));
            deltaTxt.text = TextFormatter.GetEstimatedTime(current - target);
        }
        else if (statName == Const.money)
        {
            StartCoroutine(NumCoroutine(statName, target, current));
            deltaTxt.text = TextFormatter.GetMoney(target - current);
        }
        else
        {
            StartCoroutine(GaugeCoroutine(statName, target, current));
            deltaTxt.text = (target - current).ToString();
        }

    }

    // 게이지 형식 스탯 출력
    IEnumerator GaugeCoroutine(string statName, float target, float current)
    {
        Image gaugeBar = Stats[statName].transform.Find("Gauge").GetComponent<Image>();
        //gaugeBar.fillAmount = current / beforePd.GetCurPoint(prefabs[i]); // max
        gaugeBar.fillAmount = beforePd.GetStatPercent(statName);

        yield return new WaitForSeconds(2);

        if (target <= current)
        {   // 감소한 경우

            float offset = (current - target) / duration;
            while (target < current)
            {
                current -= offset * Time.deltaTime;
                //gaugeBar.fillAmount = (target / current) - (1 - curFillAmount); 
                gaugeBar.fillAmount = current / beforePd.GetStatMax(statName);
                yield return null;
            }
        }
        else
        {   // 증가한 경우
            //float curFillAmount = target / 100;

            float offset = (target - current) / duration;
            while (current < target)
            {
                current += offset * Time.deltaTime;
                //gaugeBar.fillAmount = (current / target) - (1 - curFillAmount);
                gaugeBar.fillAmount = current / beforePd.GetStatMax(statName);
                yield return null;
            }
        }
    }

    // 숫자 형식 스탯 출력(현재는 재화,시간만 해당)
    IEnumerator NumCoroutine(string statName, float target, float current)
    {
        //temp = Stats[str].transform.GetChild(0).gameObject;
        Text textBox = Stats[statName].transform.Find("Gauge").GetComponent<Text>();
        textBox.text = current.ToString();

        if (statName == Const.time) textBox.text = TextFormatter.GetBasicTime((int)(1440 - current % 1440) % 1440);
        else if (statName == Const.money) textBox.text = TextFormatter.GetMoney(current);

        yield return new WaitForSeconds(2);

        if (target <= current)
        {
            float offset = (current - target) / duration;
            while (current >= target)
            {
                current -= offset * Time.deltaTime;
                if (current < target) { current = target; }
                int tmp = (int)current;
                textBox.text = tmp.ToString();
                if(statName==Const.time) textBox.text = TextFormatter.GetBasicTime((int)(1440 - current % 1440) % 1440);
                else if (statName == Const.money) textBox.text = TextFormatter.GetMoney(current);
                yield return null;
            }
        }
        else
        {   // 주의 - 시간은 증가하는(뒤로 가는) 경우가 없음.
            float offset = (target - current) / duration;
            while (current <= target)
            {
                current += offset * Time.deltaTime;
                if (current > target) { current = target; }
                int tmp = (int)current;
                textBox.text = tmp.ToString();
                yield return null;
            }
        }
    }

    private void Exit()
    {
        foreach(GameObject obj in Stats.Values)
        {
            obj.SetActive(false);
        }

        GameManager.Instance.Zoom(Vector3.back);
        GameManager.Instance.SetController(true);
        GameManager.Instance.Resume();
    }
}