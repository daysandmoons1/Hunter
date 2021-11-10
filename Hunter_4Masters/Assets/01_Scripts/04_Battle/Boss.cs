using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    private string name = "Test";
    private float maxHp = 10_000;
    private float curHp;
    private float attackDmg = 100;

    private Image HpGauge;
    private Text nameTxt;
    private Text hpTxt;

    public GameObject HpBar;

    void Start()
    {
        HpGauge = HpBar.transform.GetChild(0).GetComponent<Image>();
        nameTxt = HpBar.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        hpTxt = HpBar.transform.GetChild(2).GetComponent<Text>();

        curHp = maxHp;
        nameTxt.text = name;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Damaged();
        }

        if(curHp <= 0)
            Die();
    }

    private void Damaged()
    {
        StopCoroutine("ShowHpBar");
        StartCoroutine("ShowHpBar");
        curHp -= 100;
        Debug.Log("curHp : " + curHp);
        ControlHpBar();
    }

    IEnumerator ShowHpBar()
    {
        HpBar.SetActive(true);
        yield return new WaitForSeconds(3f);
        HpBar.SetActive(false);
    }

    private void ControlHpBar()
    {
        hpTxt.text = curHp.ToString() + " / " + maxHp.ToString();
        HpGauge.fillAmount = curHp / maxHp;
        Debug.Log("HpGauge.fillAmount : " + HpGauge.fillAmount);
    }

    private void Die()
    {
        Debug.Log("±³°ü is dead");
    }
}