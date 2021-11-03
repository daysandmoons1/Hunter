using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMasters.Popup
{
    public class Status : MonoBehaviour
    {
        [SerializeField] private Image rankImg;
        [SerializeField] private GameObject equipmentPanel, statePanel;
        [SerializeField] private Text timeTxt, defStatTxt, batStatTxt;

        private GameObject[] equipments = new GameObject[3];
        private Image[] equipmentImgs = new Image[3];
        //private List<Slot> equipmentSlots = new List<Slot>();
        private Image[] stateBarImg = new Image[3];
        private Text[] stateTxt = new Text[3];

        private PlayerData pd;

        private void Awake()
        {
            for (int i=0; i<3; i++)
            {
                equipments[i] = equipmentPanel.transform.GetChild(i).gameObject;
                equipmentImgs[i] = equipments[i].transform.GetChild(0).GetComponent<Image>();
                equipmentImgs[i].color = new Color(1, 1, 1, 0);

                stateBarImg[i] = statePanel.transform.GetChild(i).GetChild(0).GetComponent<Image>();
                stateTxt[i] = statePanel.transform.GetChild(i).GetChild(1).GetComponent<Text>();
            }
        }

        private void OnEnable()
        {
            pd = FosterManager.Instance.GetPlayerData();

            rankImg.sprite = DataManager.Instance.GetSprite("icons", "rank_F");

            List<EquipItem> equipmentData = pd.GetAllEquipItems();
            for (int i = 0; i < 3; i++)
            {
                if(equipmentData[i].item_name!=Const.defStr)
                {
                    // * 프레임 자체가 보이지 않게 구현할 경우 색상변경 대신 상위 게임오브젝트 비활성화할 것.
                    equipmentImgs[i].color = Color.white;
                    equipmentImgs[i].sprite = 
                        DataManager.Instance.GetSprite("Items", DataManager.Instance.GetItemKey(equipmentData[i].item_index));
                }
                else
                {
                    // * 프레임 자체가 보이지 않게 구현할 경우 색상변경 대신 상위 게임오브젝트 비활성화할 것.
                    equipmentImgs[i].color = new Color(1, 1, 1, 0);
                }
            }

            timeTxt.text = GameManager.Instance.GetDDay() + "\n" +
                GameManager.Instance.GetCurrentTimeByValue();

            for(int i=0; i<Const.state.Length; i++)
            {
                stateBarImg[i].fillAmount = pd.GetStatPercent(Const.state[i]);
                stateTxt[i].text = Const.state[i]+"\n"+pd.GetStateOutOfMax(Const.state[i]);
            }

            defStatTxt.text = "";

            for(int i=0; i<Const.defStats.Length; i++)
            {
                defStatTxt.text += pd.GetDefStat(Const.defStats[i])+"\n";
            }
            batStatTxt.text = pd.GetBatStat();
        }
    }
}

