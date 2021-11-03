using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private GameObject attackPanel, joystickPanel;

    public int leftTimeVal = 43200;   // 60(분)*24(시간)*30(일)
    public int d_day, curTimeVal;
    float timeScale = 6f;
    int gameTimeScale = 10; // 실제시간 timeScale 초마다 인게임 gameTimeScale 분씩 지남
    float delay = 0.0f;
    IEnumerator timeCoroutine;

    public bool isPlay;
    public int tempSpendTime = 0;  // 시뮬레이션 실행시 소모되는 시간 임시 저장

    private float zoomMidSize = 3.5f;   // 줌 인·아웃 중간지점
    private float zoomDelta = 1.5f;  // 줌 이동량

    private float zoomXMidpos = 0.375f; // 줌 인·아웃 시 x좌표 중간지점(현재는 행동 선택 시에만 사용)
    private float zoomXDelta = 0.125f;   // 줌 인·아웃 시 x좌표 변화량

    private int areaCode = 0;    // 타 지역 구현 전이므로 지리산 코드 고정

    private CinemachineFramingTransposer vcamTransposer;
    private IEnumerator zoomCrt;

    private void Start()
    {
        vcamTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        CalcTime();
        Resume();
    }

    IEnumerator TimeCoroutine()
    {
        while (true)
        {
            delay += Time.deltaTime;
            if (delay >= timeScale)
            {
                leftTimeVal -= gameTimeScale;
                delay = 0.0f;
                CalcTime();
            }
            yield return null;
        }
    }

    // 남은 시간으로 남은 일수와 현재시각을 계산 및 값 갱신
    private void CalcTime()
    {
        d_day = (int)Mathf.Ceil((float)leftTimeVal / 1440);
        curTimeVal = (1440 - leftTimeVal % 1440) % 1440;
    }

    public int GetAreaCode()
    {
        return areaCode;
    }

    public void SetAreaCode()
    {

    }

    public string GetDDay()
    {
        return TextFormatter.GetBasicDDay(d_day);
    }

    public string GetCurrentTimeByValue()
    {
        return TextFormatter.GetBasicTime(curTimeVal);
    }

    public void Pause()
    {
        isPlay = false;
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
    }

    public void Resume()
    {
        leftTimeVal += tempSpendTime;
        tempSpendTime = 0;
        CalcTime();
        isPlay = true;
        timeCoroutine = TimeCoroutine();
        StartCoroutine(timeCoroutine);
    }

    public void SkipTime(int val)
    {
        leftTimeVal -= val;
        CalcTime();
    }

    public void SetController(bool value)
    {
        attackPanel.SetActive(value);
        joystickPanel.SetActive(value);
    }

    public void Zoom(Vector3 dir)
    {
        if(zoomCrt!=null) StopCoroutine(zoomCrt);
        zoomCrt = CameraZoomCrt(dir);
        StartCoroutine(zoomCrt);
    }

    // 카메라 줌 인·아웃 코루틴
    private IEnumerator CameraZoomCrt(Vector3 dir)
    {
        float destZoomSize = zoomMidSize + zoomDelta * -dir.z;
        float destZoomXAxis = zoomXMidpos + zoomXDelta * -dir.z;
        float offset = 0f;
        while (vcam.m_Lens.OrthographicSize != destZoomSize && offset < 1.5f)
        {
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(vcam.m_Lens.OrthographicSize, destZoomSize, Time.deltaTime * 3.5f);
            vcamTransposer.m_ScreenX = Mathf.Lerp(vcamTransposer.m_ScreenX, destZoomXAxis, Time.deltaTime * 3.5f);
            offset += Time.deltaTime;
            yield return null;
        }
        vcam.m_Lens.OrthographicSize = destZoomSize;
        vcamTransposer.m_ScreenX = destZoomXAxis;
    }
}
