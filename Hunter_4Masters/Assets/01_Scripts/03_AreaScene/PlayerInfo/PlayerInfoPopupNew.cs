using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMasters.Popup;

public class PlayerInfoPopupNew : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel, menuSelectEffect, screenPanel;
    
    private GameObject[] screens = new GameObject[5];
    private float[] menuPositions = new float[5];
    private int currentScreenIndex = 0;
    private Button[] menuBtns = new Button[5];
    private Animation anim;
    private IEnumerator menuSelectEffectCrt;

    private void Awake()
    {
        for(int i=0; i<5; i++)
        {
            menuBtns[i] = menuPanel.transform.GetChild(i+1).GetComponent<Button>();
            int index = i;
            menuBtns[i].onClick.AddListener(()=> { DisplayScreen(index); });
            screens[i] = screenPanel.transform.GetChild(i).gameObject;
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.Pause();
        // 애니메이션 실행
        // ...

        // 해상도 변경 기능을 고려해 이곳에서 실행
        for(int i=0; i<5; i++)
        {
            menuPositions[i] = menuPanel.transform.GetChild(i+1).position.y;
        }

        currentScreenIndex = 0;
        menuSelectEffect.transform.position = new Vector3(
            menuSelectEffect.transform.position.x,
            menuPositions[currentScreenIndex],
            menuSelectEffect.transform.position.z);

        // 메뉴선택 이미지 위치 설정
        MoveSelectEffect();
        screens[0].SetActive(true);
    }

    private void OnDisable()
    {
        screens[currentScreenIndex].SetActive(false);
    }

    public void DisplayScreen(int index)
    {
        screens[currentScreenIndex].SetActive(false);
        currentScreenIndex = index;
        MoveSelectEffect();
        screens[currentScreenIndex].SetActive(true);
    }

    // 메뉴선택효과 이동
    private void MoveSelectEffect()
    {
        if (menuSelectEffectCrt != null) StopCoroutine(menuSelectEffectCrt);
        menuSelectEffectCrt = MenuSelectEffectCrt();
        StartCoroutine(menuSelectEffectCrt);
    }

    // 메뉴선택효과 이동 코루틴
    private IEnumerator MenuSelectEffectCrt()
    {
        Vector3 destPos = new Vector3(
            menuSelectEffect.transform.position.x, 
            menuPositions[currentScreenIndex], 
            menuSelectEffect.transform.position.z);
        float offset = 0f;
        while(menuSelectEffect.transform.position != destPos && offset<0.3f)
        {
            menuSelectEffect.transform.position = Vector3.Lerp(menuSelectEffect.transform.position, destPos, Time.deltaTime*15f);
            offset += Time.deltaTime;
            yield return null;
        }
        menuSelectEffect.transform.position = destPos;
    }
}
