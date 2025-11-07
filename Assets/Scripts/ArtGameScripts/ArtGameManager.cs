using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ArtGameManager : MonoBehaviour
{
    [Header("게임 설정")]
    public float gameTime = 60f;
    public int totalDifferences = 6;

    [Header("그림 버전들")]
    public GameObject painting1;
    public GameObject painting2;
    public GameObject painting3;

    [Header("이미지 표시")]
    public Image originalImage;
    public Image fixImage;

    [Header("각 버전별 이미지 스프라이트")]
    public Sprite painting1_Original;
    public Sprite painting1_Fix;

    public Sprite painting2_Original;
    public Sprite painting2_Fix;

    public Sprite painting3_Original;
    public Sprite painting3_Fix;

    [Header("UI")]
    public Image timerFillImage;
    public TextMeshProUGUI leftAmountText;
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("메인 씬 이름")]
    public string mainSceneName = "Main";

    private int currentDifferences;
    private float timeLeft;
    private bool isGameRunning = false;
    private int bonusScore = 0;

    void Start()
    {
        currentDifferences = totalDifferences;
        timeLeft = gameTime;

        // 보너스 점수 로드
        bonusScore = PlayerPrefs.GetInt("ArtBonusScore", 0);

        if (bonusScore > 0)
        {
            Debug.Log("Artist 직군 보너스: +" + bonusScore + "점");
        }

        // Timer Fill Image 설정
        if (timerFillImage != null)
        {
            if (timerFillImage.type != Image.Type.Filled)
            {
                timerFillImage.type = Image.Type.Filled;
                timerFillImage.fillMethod = Image.FillMethod.Horizontal;
            }
            timerFillImage.fillAmount = 1.0f;
        }

        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        UpdateUI();
        SelectRandomPainting();

        isGameRunning = true;
    }

    void SelectRandomPainting()
    {
        painting1.SetActive(false);
        painting2.SetActive(false);
        painting3.SetActive(false);

        int selectedVersion = Random.Range(1, 4);

        Debug.Log("선택된 버전: Painting_" + selectedVersion);

        switch (selectedVersion)
        {
            case 1:
                painting1.SetActive(true);
                SetImages(painting1_Original, painting1_Fix);
                break;
            case 2:
                painting2.SetActive(true);
                SetImages(painting2_Original, painting2_Fix);
                break;
            case 3:
                painting3.SetActive(true);
                SetImages(painting3_Original, painting3_Fix);
                break;
        }
    }

    void SetImages(Sprite original, Sprite fix)
    {
        if (originalImage != null && original != null)
        {
            originalImage.sprite = original;
        }

        if (fixImage != null && fix != null)
        {
            fixImage.sprite = fix;
        }
    }

    void Update()
    {
        if (!isGameRunning) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0) timeLeft = 0;

        UpdateUI();

        if (timeLeft <= 0)
        {
            GameOver(false);
        }
    }

    void UpdateUI()
    {
        if (timerFillImage != null)
        {
            float fillAmount = timeLeft / gameTime;
            timerFillImage.fillAmount = fillAmount;
        }

        if (leftAmountText != null)
        {
            leftAmountText.text = currentDifferences.ToString();
        }
    }

    public void OnCorrectAnswer()
    {
        if (!isGameRunning) return;

        currentDifferences--;

        Debug.Log("정답! 남은 개수: " + currentDifferences);

        if (currentDifferences <= 0)
        {
            GameOver(true);
        }
    }

    void GameOver(bool isSuccess)
    {
        isGameRunning = false;

        if (isSuccess)
        {
            int baseScore = Mathf.CeilToInt(timeLeft);
            int totalScore = baseScore + bonusScore;

            Debug.Log("게임 성공! 기본 점수: " + baseScore + "점, 보너스: " + bonusScore + "점, 총점: " + totalScore + "점");

            // 점수 저장 (기본 점수만 저장, 보너스는 표시할 때 더함)
            PlayerPrefs.SetInt("ArtLastScore", baseScore);
            PlayerPrefs.SetInt("ArtGameResult", 1);
            PlayerPrefs.Save();

            if (successPanel != null)
            {
                successPanel.SetActive(true);

                TextMeshProUGUI scoreText = successPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    string text = "성공!\n기본 점수: " + baseScore + "점";

                    if (bonusScore > 0)
                    {
                        text += "\n보너스: +" + bonusScore + "점";
                        text += "\n총점: " + totalScore + "점";
                    }

                    scoreText.text = text;
                }
            }
        }
        else
        {
            Debug.Log("게임 실패!");

            PlayerPrefs.SetInt("ArtLastScore", 0);
            PlayerPrefs.SetInt("ArtGameResult", 0);
            PlayerPrefs.Save();

            if (failPanel != null)
            {
                failPanel.SetActive(true);

                TextMeshProUGUI scoreText = failPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (scoreText != null && bonusScore > 0)
                {
                    scoreText.text = "실패!\n다음에 다시 도전하세요\n(보너스: +" + bonusScore + "점)";
                }
            }
        }

        StartCoroutine(ReturnToMainScene(3f));
    }

    IEnumerator ReturnToMainScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 페이드 아웃 후 메인 씬으로
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(mainSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }
}