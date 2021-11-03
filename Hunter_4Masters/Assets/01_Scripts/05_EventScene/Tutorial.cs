using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

// 시나리오 진행을 update에서 진행하는 것이 아니라 Skip 버튼 혹은, 대화가 끝나고 다음 화면으로 넘어갈 때 작동하게끔 함수를 새로 짜면 좋을 듯 함

public class Tutorial : MonoBehaviour
{
    public InputField InputTxt;
    public GameObject NameBox;

    Animator playerAnim;

    protected GameObject canvas;
    protected GameObject DialogBox;
    protected Talk talkScript;


    private bool getName = false;

    private bool triggerOn = false;

    //canvas
    [SerializeField] private GameObject fadeout;
    //private Color fadeoutColor;

    [SerializeField] private GameObject leafletImg;
    [SerializeField] private GameObject leaflet;
    [SerializeField] private GameObject skipBtn;
    [SerializeField] private GameObject exitBtn;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject npcInstructor;
    [SerializeField] private GameObject npcStaff;
    [SerializeField] private GameObject nosilyBallon;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject trainee;
    [SerializeField] private GameObject hyunjun;
    [SerializeField] private GameObject partLeader;

    public string name = "";

    public void StartTutorial()
    {
        talkScript.txt = Resources.Load<TextAsset>($"Scripts/TutorialScript");

        DialogBox.SetActive(true);
       // DialogBox.GetComponent<Talk>().StartDialog();

    }

    void Awake()
    {
        canvas = GameObject.Find("Canvas");
        DialogBox = canvas.transform.Find("DialogBox").gameObject;
        talkScript = DialogBox.GetComponent<Talk>();
        playerAnim = player.GetComponent<Animator>();

        //fadeoutColor = fadeout.GetComponent<Image>().color;
        
        // //Test
        //DialogBox.TestStart();
    }

    void Update()
    {
        switch(talkScript.currentIdx){
            case 3: // 전단지 날아오는 애니메이션에서 대화창 잠시 꺼지고 애니메이션 끝난 뒤 다시 활성화
                if(!leaflet.activeInHierarchy)
                    leaflet.SetActive(true);
                break;
            case 4:
                if(!leafletImg.activeInHierarchy)
                    leafletImg.SetActive(true);
                break;
            case 10:
                triggerOn = false;
                leaflet.SetActive(false);
                leafletImg.SetActive(false);
                if(!getName) GetName();
                break;
            case 12:
                // 지리산 앞으로 이동
                Debug.Log("*****지리산 앞으로 이동합니다.*****");
                break;
            case 13:
                // 안내표시출력
                if(!arrow.activeInHierarchy)
                    arrow.SetActive(true);
                break;
            case 14:
                // 근데 이 부분 수진언니랑 이야기 해봐야함. (돈을 지불하면 빠르고 안전하게..) 이 부분 출력 뒤 UI 띄우는건지
                // 대화창 종료, 이동 UI 출력, 이동 UI 출력된 상태에서 대화창 출력. 주인공 이동 중
                if(arrow.activeInHierarchy)
                    arrow.SetActive(false);
                break;
            case 19:
                // (장소이동) 세종으로 이동
                Debug.Log("*****세종으로 이동합니다.*****");
                break;
            case 20:
                // (장소이동) 세종 헌터 건물 내
                Debug.Log("*****세종 헌터 건물 안으로 이동합니다.*****");
                // 헌터 협회 직원 출연
                if(!npcStaff.activeInHierarchy)
                    npcStaff.SetActive(true);
                break;
            case 26:
                // 건물 밖이 시끄럽다는 표현
                if(!nosilyBallon.activeInHierarchy)
                    nosilyBallon.SetActive(true);
                // (장소이동) 주인공 : 헌터건물 내에서 훈련소로 이동
                Debug.Log("*****훈련소로 이동합니다.*****");

                //            NPC : 화면 밖으로 페이드아웃
                // 화면 밖으로 페이드아웃이라는게 .... 걸어서 나간다는 소린가?
                if(npcStaff.activeInHierarchy)
                    npcStaff.SetActive(false);
                break;
            case 27:
                // 일시적인 엑스트라 중앙 카메라 연출, 헌터 교관 밑에 엑스트라들 훈련 연출
                if(!npcInstructor.activeInHierarchy)
                    nosilyBallon.SetActive(false);
                    npcInstructor.SetActive(true);
                break;
            case 29:
                // 교관 NPC 강조
                // 교관 NPC에게 2초 동안 카메라 확대
                // 카메라 원위치

                // 스킵버튼 비활성화
                //skipBtn.SetActive(false);
                // 교관 NPC 머리 위에 말풍선 띄우기
                // 클릭 시 스킵버튼 활성화
                 break;
            case 33:
                // 훈련 목록 UI 출력, 훈련 UI 외 딤 처리
                // 훈련 중
                // 훈련 결과 UI 출력
                break;
            case 34:
                // 교관 앞에서 일반공격 제스처 출력 후 인사
                playerAnim.SetBool("isAttack", true);
                break;
            case 35:
                playerAnim.SetBool("isAttack", false);
                // 훈련 애니메이션
                break;
            case 39:
                // 훈련소에서 협회 건물 내로 이동, 교관 NPC 화면 밖으로 페이드 아웃
                npcInstructor.SetActive(false);
                break;
            case 40:
                // 세종 헌터 건물 내
                break;
            case 41:
                npcStaff.SetActive(true);
                // 헌터협회 사원 강조
                break;
            case 42:
                // 의뢰 UI 출력
                break;
            case 43:
                // 장소 이동 : 시험장(헌터 훈련소 앞)
                break;
            case 44:
                // 장소 : 시험장(헌터 훈련소 앞)
                // 콘티 13번
                // 대결 상대 배치 중
                // 여러명이 파트장 앞에 서 있음
                // 주인공은 훈련병들 중간에 위치함
                if(!trainee.activeInHierarchy)
                    trainee.SetActive(true);
                break;
            case 47:
                // 대전 상대 발표
                // 신현준 등장
                trainee.SetActive(false);
                if(!hyunjun.activeInHierarchy)
                    hyunjun.SetActive(true);
                break;
            case 53:
                // 주인공 시선 바닥
                break;
            case 55:
                // 근데 이게 진짜 전투인건가? 아니면 그냥 애니메이션인가
                // 일반 공격 버튼 깜박이는 효과
                break;
            case 57:
                // 전투
                break;
            case 59:
                if(!partLeader.activeInHierarchy)
                    partLeader.SetActive(true);
                // 파트장 NPC가 주인공에게 무기 던지는 애니메이션
                break;
            case 61:
                partLeader.SetActive(false);
                // 인벤토리 활성화 될 때까지 기다리기
                // 무기를 칸 안으로 이동시킬 때까지 기다리기
                // 닫을 때까지 기다리기
                break;
            case 64:
                // 스킬 버튼 강조 후
                // 스킬 공격 애니메이션
                break;
            case 65:
                // 대화창 비활성화

                // 30초 동안 실제 전투 (전투하는 애니메이션인지, 실제 전투인지)
                // 주인공의 스킬 사용, 스킬 제스처 및 이펙트
                // 신현준의 머리와 옷 휘날리는 애니메이션, 타격은 X
                // 패배
                // 신현준의 막타
                // 신현준 스킬 사용 : 안보이는 팔을 앞으로 내밀어 주인공 당긴 후 어퍼 사용
                // 주인공 날아가면서 기절 이 때 신현준은 여전히 어퍼컷 포즈 (마비노기 스크류어퍼 참고)
                // 시합 끝나는 소리
                if(!triggerOn)
                {
                    triggerOn = !triggerOn;
                    playerAnim.SetTrigger("isKO");
                }
                break;
            case 67:
                // 대화창 활성화
                // 스크립트 진행
                // 주인공은 누워있는 상태로 신현준 퇴장
                hyunjun.SetActive(false);
                break;
            case 70:
                // 안내 표시
                if(!arrow.activeInHierarchy)
                    arrow.SetActive(true);
                break;
            case 71:
                // 임무 완료 UI 출력
                // 보상 획득
                break;
            case 72:
                //Move(세종헌터건물내);
                // 헌터협회 사원 NPC 활성화
                if(!npcStaff.activeInHierarchy)
                    npcStaff.SetActive(true);
                break;
            case 83:
                //안내원 인력사무소로 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 84:
                //Move(인력사무소앞);
                // 안내원 인력사무소로 도착 // NPC 대기중
                break;
            case 85:
                // NPC 강조 후 낮은 노동 강조
                // 노동 UI 출력
                // 노동 완료UI 출력
                break;
            case 89:
                //안내원 호텔촌 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 93:
                // NPC 강조 후 휴식하기 강조
                break;
            case 95:
                // 안내원 상점가 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 99:
                // NPC 강조 후 아이템 강조
                // 아이템 구매하기
                break;
            case 102:
                //사운드 휴대폰 벨소리
                break;
            case 106:
                // 카지노 직원 NPC 활성화
                break;
            case 107:
                // 카지노 직원머리 위에 느낌표
                break;
            case 114:
                //카지노 직원 카지노로 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 119:
                // 카지노 직원 룰렛으로 이동 // NPC 화면 밖으로 페이드아웃 / 룰렛 강조
                break;
            case 120:
                // 카지노 직원 룰렛으로 이동 // NPC 화면 밖으로 페이드아웃(필요 시) / 룰렛 강조 / 주인공 룰렛 1회분 금화 획득
                break;
            case 122:
                // 룰렛 중
                // 룰렛 보상 획득
                break;
            case 125:
                //카지노에서 상점가 내로 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 128:
                // 카지노 직원 밖으로 뛰어가기
                break;
            case 129:
                //안내원 상점가 내로 뛰어오기
                break;
            case 130:
                // 안내원 병원 이동 // NPC 화면 밖으로 페이드아웃 // 안내 표시 출력
                break;
            case 134:
                // 세종 헌터 건물 내로 장소 이동
                break;
        }

    }

    void GetName()
    {
        getName = true;

        skipBtn.SetActive(false);
        exitBtn.SetActive(false);

        if(NameBox.activeSelf != true)
        {
            NameBox.SetActive(true);
            //test
            //DialogBox.SetActive(false);
        }
    }

    public void SetName()
    {
        try{
            name = InputTxt.text;

            if(isCorrectName(name))
            {
                Debug.Log("내 이름은 " + name);
                InputTxt.text = "";
                NameBox.SetActive(false);
                //test
                //DialogBox.SetActive(true);
                //DialogBox.TestStart();
                //talkScript.currentIdx = 11;
                skipBtn.SetActive(true);
                exitBtn.SetActive(true);
            }
            else
            {
                Debug.Log("이상한 이름임. 다시!");
                InputTxt.text = "";
                name = "";
                GetName();
            }
        }catch(ArgumentOutOfRangeException e){
            Debug.Log("ArgumentOutOfRangeException 발생!!");
            GetName();
        }
        
        // Talk 스크립트의 currentIdx++ , Play 함수 실행 
    }

    bool isCorrectName(string name)
    {
        // 한글 혹은 영어여야 함
        // 특수문자는 허용되지 않음
        // 최소 2글자, 최대 8글자 (한글인 경우이고 영어와 숫자인 경우도 설정해야함)
        
        Regex regex = new Regex(@"^[0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]{2,8}$");
        if(!(regex.IsMatch(name)))
        {
            Debug.Log("잘못입력함 : " + name);
            return false;
        }
        else
        {
            Debug.Log("제대로 입력함 : " + name);
            return true;
        }
    }

    void FadeInOut()
    {
        fadeout.SetActive(true);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Image fadeImg = fadeout.GetComponent<Image>();
        Color tempColor = fadeImg.color;

        float FadeTime = 3f;

        while(tempColor.a < 1f)
        {
            tempColor.a += Time.deltaTime / FadeTime;
            fadeout.GetComponent<Image>().color = tempColor;

            yield return null;
        }

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        Image fadeImg = fadeout.GetComponent<Image>();
        Color tempColor = fadeImg.color;

        float FadeTime = 3f;

        while(tempColor.a > 0f)
        {
            tempColor.a -= Time.deltaTime / FadeTime;
            fadeout.GetComponent<Image>().color = tempColor;

            yield return null;
        }

        fadeout.SetActive(false);

    }
}