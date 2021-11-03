using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMasters.Popup
{
    public class Achievements : MonoBehaviour
    {
        [SerializeField] private GameObject achievementPanel;
        [SerializeField] [Range(0, 1)] private float showTime;

        private GameObject[] achievementRows, achievementDisplay;
        private GameObject[,] achievements;
        private Button[,] achievementBtns;

        private Image[] achievementImgs;

        private int rowSize, colSize;

        private IEnumerator showImageCrt;

        // (temp variable)
        private int achievementNum = 15;

        private void Awake()
        {
            rowSize = achievementPanel.transform.childCount / 2;
            colSize = achievementPanel.transform.GetChild(0).childCount;

            achievementRows = new GameObject[rowSize];
            achievementDisplay = new GameObject[rowSize];
            achievementImgs = new Image[rowSize];
            achievements = new GameObject[rowSize, colSize];
            achievementBtns = new Button[rowSize, colSize];

            for(int i=0; i<rowSize; i++)
            {
                achievementRows[i] = achievementPanel.transform.GetChild(i * 2).gameObject;
                achievementDisplay[i] = achievementPanel.transform.GetChild(i * 2 + 1).gameObject;
                achievementImgs[i] = achievementDisplay[i].GetComponent<Image>();

                for(int j=0; j<colSize; j++)
                {
                    achievements[i, j] = achievementRows[i].transform.GetChild(j).gameObject;
                    achievementBtns[i, j] = achievements[i,j].GetComponent<Button>();
                    int rowIndex = i;
                    int colIndex = j;
                    achievementBtns[i, j].onClick.AddListener(()=> { ShowImage(rowIndex, colIndex); });
                    achievements[i, j].SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            ResetButtonsAndDisplay();

            // 현재는 전체 업적이 보여짐
            // 각 줄에 보여질 요소 결정
            for(int i=0; i<achievementNum; i++)
            {
                int row = i / colSize;
                int col = i % colSize;
                if (col == 0) achievementRows[row].SetActive(true);
                achievements[row, col].SetActive(true);
                achievementBtns[i / colSize, i % colSize].image.color = Color.white;
            }
        }

        private void ResetButtonsAndDisplay()
        {
            for(int i=0; i<rowSize; i++)
            {
                for(int j=0; j<colSize; j++)
                {
                    achievements[i, j].SetActive(false);
                }
                achievementRows[i].SetActive(false);
                achievementDisplay[i].SetActive(false);
            }
        }

        // 나머지 업적 색상 변경 및 현재 업적 이미지를 보이기
        private void ShowImage(int row, int col)
        {
            for(int i=0; i<achievementNum; i++)
            {
                achievementBtns[i / colSize, i % colSize].image.color = Color.gray;
            }
            achievementBtns[row, col].image.color = Color.white;

            for(int i=0; i<rowSize; i++)
            {
                achievementDisplay[i].SetActive(false);
            }
            achievementDisplay[row].SetActive(true);

            if (showImageCrt != null) StopCoroutine(showImageCrt);
            showImageCrt = ShowImageCrt(row, col);
            StartCoroutine(showImageCrt);
        }

        private IEnumerator ShowImageCrt(int row, int col)
        {
            achievementDisplay[row].transform.localScale = new Vector3(1, 0, 1);
            Vector3 destScale = new Vector3(1, 1, 1);
            float offset = 0f;
            while (achievementDisplay[row].transform.localScale != destScale && offset < showTime)
            {
                achievementDisplay[row].transform.localScale = Vector3.Lerp(achievementDisplay[row].transform.localScale,  destScale, Time.deltaTime*10f);
                offset += Time.deltaTime;
                yield return null;
            }
            achievementDisplay[row].transform.localScale = destScale;
        }
    }
}