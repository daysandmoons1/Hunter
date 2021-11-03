using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] public GameObject playerInfoPopupTest;

    [SerializeField] private GameObject selectPanel, hospitalPanel, shopPanel, requestPanel, awakeningPanel;
    private SelectionPopup selectionPopup;
    private RectTransform selectionPopupRT, hospitalPopupRT;

    private float slideMidPos = -840f;  // 선택지 팝업창 슬라이드 전·후의 중간지점
    private float slideDelta = 340f; // 선택지 팝업창 슬라이드 이동량

    void Start()
    {
        selectionPopup = selectPanel.GetComponent<SelectionPopup>();
        selectionPopupRT = selectPanel.transform.GetChild(0).GetComponent<RectTransform>();
        hospitalPopupRT = hospitalPanel.transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OpenPlayerInfo()
    {
        GameManager.Instance.Pause();
        playerInfoPopupTest.SetActive(true);
    }

    public void ClosePlayerInfo()
    {
        playerInfoPopupTest.SetActive(false);
        GameManager.Instance.Resume();
    }

    public SelectionPopup GetSelectionPopup()
    {
        return selectionPopup;
    }

    public void DoAction(string action)
    {
        GameManager.Instance.Pause();

        if (action == "shop")
        {
            shopPanel.SetActive(true);
        }
        else if (action == "request")
        {
            requestPanel.SetActive(true);
        }
        else if (action == "awakening")
        {
            awakeningPanel.SetActive(true);
        }
        else
        {
            StopAllCoroutines();

            GameManager.Instance.SetController(false);
            GameManager.Instance.Zoom(Vector3.forward);
            if (action == "hospital")
            {
                hospitalPanel.SetActive(true);
                StartCoroutine(PopupSlideCoroutine(Vector3.left, hospitalPopupRT));
            }
            else
            {
                selectPanel.SetActive(true);
                selectionPopup.SetPopup(action);
                StartCoroutine(PopupSlideCoroutine(Vector3.left, selectionPopupRT));
            }

        }
    }

    // 선택지 팝업창 닫기 버튼에서 호출
    public void CancelAction(string action)
    {
        StopAllCoroutines();

        GameManager.Instance.Zoom(Vector3.back);
        StartCoroutine(popupExitCoroutine(action));
    }

    // 선택지 팝업창 닫을 때: 슬라이드 후 비활성화 해야 함
    private IEnumerator popupExitCoroutine(string action)
    {
        if (action == "hospital")
        {
            StartCoroutine(PopupSlideCoroutine(Vector3.right, hospitalPopupRT));
            yield return new WaitForSeconds(0.5f);
            hospitalPanel.SetActive(false);
        }
        else
        {
            StartCoroutine(PopupSlideCoroutine(Vector3.right, selectionPopupRT));
            yield return new WaitForSeconds(0.5f);
            selectPanel.SetActive(false); 
        }
        GameManager.Instance.SetController(true);
        GameManager.Instance.Resume();
        yield return null;
    }

    // 선택지 팝업창 슬라이드 코루틴
    private IEnumerator PopupSlideCoroutine(Vector3 dir, RectTransform rt)
    {
        Vector2 destPos = new Vector2(slideMidPos + slideDelta * dir.x, rt.anchoredPosition.y);
        float offset = 0f;
        while (rt.anchoredPosition != destPos && offset < 1.5f)
        {
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, destPos, Time.deltaTime*4.5f);
            offset += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = destPos;
    }
}
