using System.Collections;
using System.Collections.Generic;
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

    [Header("카운트다운")]
    public GameObject countdownPanel;
    public Image countdownImage;
    public Sprite count3Sprite;
    public Sprite count2Sprite;
    public Sprite count1Sprite;
    public float countdownDuration = 1f;

    [Header("방해 공작 시스템")]
    public GameObject sleepTextPanel; // 졸음 글자 패널
    public Sprite[] sleepSprites; // 졸음 글자 스프라이트들
    public Image sleepImagePrefab; // 졸음 이미지 프리팹
    public int sleepImageCount = 15; // 생성할 졸음 이미지 개수
    public float sleepDuration = 2f; // 졸음 지속 시간
    public float sleepSpawnInterval = 0.1f; // 졸음 이미지 생성 간격
    public float sleepFallSpeedMin = 200f; // 최소 낙하 속도
    public float sleepFallSpeedMax = 400f; // 최대 낙하 속도
    public float mouseSensitivityDuration = 5f; // 마우스 감도 이상 지속 시간
    public float abnormalSensitivity = 3f; // 비정상 마우스 감도 배율

    [Header("메인 씬 이름")]
    public string mainSceneName = "Main";

    private int currentDifferences;
    private float timeLeft;
    private bool isGameRunning = false;
    private int bonusScore = 0;

    private bool sleepObstacleUsed = false;
    private bool mouseObstacleUsed = false;
    private float mouseSensitivityMultiplier = 1f;

    // 떨어지는 이미지 정보 저장용 클래스
    private class FallingSleepImage
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public float fallSpeed;
    }

    void Start()
    {
        currentDifferences = totalDifferences;
        timeLeft = gameTime;

        bonusScore = PlayerPrefs.GetInt("ArtBonusScore", 0);

        if (bonusScore > 0)
        {
            Debug.Log("Artist 직군 보너스: +" + bonusScore + "점");
        }

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
        if (sleepTextPanel != null) sleepTextPanel.SetActive(false);

        UpdateUI();
        SelectRandomPainting();

        StartCoroutine(CountdownSequence());
    }

    IEnumerator CountdownSequence()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        if (countdownImage != null && count3Sprite != null)
        {
            countdownImage.sprite = count3Sprite;
        }
        yield return new WaitForSeconds(countdownDuration);

        if (countdownImage != null && count2Sprite != null)
        {
            countdownImage.sprite = count2Sprite;
        }
        yield return new WaitForSeconds(countdownDuration);

        if (countdownImage != null && count1Sprite != null)
        {
            countdownImage.sprite = count1Sprite;
        }
        yield return new WaitForSeconds(countdownDuration);

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        isGameRunning = true;
        Debug.Log("게임 시작!");

        // 방해 공작 스케줄링
        StartCoroutine(ScheduleObstacles());
    }

    IEnumerator ScheduleObstacles()
    {
        // 게임 시간을 3등분하여 각 구간에서 방해 공작 발동
        float thirdTime = gameTime / 3f;

        // 첫 번째 구간에서 랜덤 타이밍
        float firstObstacleTime = Random.Range(5f, thirdTime);
        yield return new WaitForSeconds(firstObstacleTime);

        if (isGameRunning)
        {
            TriggerRandomObstacle();
        }

        // 두 번째 구간에서 랜덤 타이밍
        float secondObstacleTime = Random.Range(thirdTime, thirdTime * 2f) - firstObstacleTime;
        yield return new WaitForSeconds(secondObstacleTime);

        if (isGameRunning)
        {
            TriggerRandomObstacle();
        }
    }

    void TriggerRandomObstacle()
    {
        // 아직 사용하지 않은 방해 공작 중에서 선택
        bool canUseSleep = !sleepObstacleUsed;
        bool canUseMouse = !mouseObstacleUsed;

        if (!canUseSleep && !canUseMouse) return;

        int obstacleType;

        if (canUseSleep && canUseMouse)
        {
            obstacleType = Random.Range(0, 2); // 0: 졸음, 1: 마우스
        }
        else if (canUseSleep)
        {
            obstacleType = 0;
        }
        else
        {
            obstacleType = 1;
        }

        if (obstacleType == 0)
        {
            StartCoroutine(SleepObstacle());
        }
        else
        {
            StartCoroutine(MouseSensitivityObstacle());
        }
    }

    IEnumerator SleepObstacle()
    {
        sleepObstacleUsed = true;
        Debug.Log("졸음 방해 공작 발동!");

        if (sleepTextPanel != null && sleepImagePrefab != null && sleepSprites != null && sleepSprites.Length > 0)
        {
            sleepTextPanel.SetActive(true);

            List<FallingSleepImage> fallingImages = new List<FallingSleepImage>();

            // 졸음 이미지를 불규칙하게 생성하고 떨어뜨리기
            StartCoroutine(SpawnSleepImages(fallingImages));

            // 낙하 애니메이션
            float elapsedTime = 0f;
            while (elapsedTime < sleepDuration)
            {
                elapsedTime += Time.deltaTime;

                // 각 이미지를 개별 속도로 낙하
                for (int i = fallingImages.Count - 1; i >= 0; i--)
                {
                    if (fallingImages[i].gameObject != null && fallingImages[i].rectTransform != null)
                    {
                        // 각자의 속도로 아래로 이동
                        fallingImages[i].rectTransform.anchoredPosition +=
                            Vector2.down * fallingImages[i].fallSpeed * Time.deltaTime;
                    }
                }

                yield return null;
            }

            // 생성된 이미지들 제거
            foreach (var fallingImage in fallingImages)
            {
                if (fallingImage.gameObject != null)
                {
                    Destroy(fallingImage.gameObject);
                }
            }

            sleepTextPanel.SetActive(false);
        }

        Debug.Log("졸음 방해 공작 종료");
    }

    IEnumerator SpawnSleepImages(List<FallingSleepImage> fallingImages)
    {
        for (int i = 0; i < sleepImageCount; i++)
        {
            Image sleepImage = Instantiate(sleepImagePrefab, sleepTextPanel.transform);

            // 랜덤 스프라이트 선택
            sleepImage.sprite = sleepSprites[Random.Range(0, sleepSprites.Length)];

            RectTransform rectTransform = sleepImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 랜덤 위치 설정 (화면 위쪽에서 시작, X 위치도 더 넓게)
                rectTransform.anchoredPosition = new Vector2(
                    Random.Range(-600f, 600f),
                    Random.Range(500f, 700f) // 화면 위쪽 넓은 범위
                );

                // 랜덤 크기
                float scale = Random.Range(0.5f, 1.5f);
                rectTransform.localScale = Vector3.one * scale;

                // 회전 제거
            }

            // 각 이미지마다 다른 낙하 속도
            FallingSleepImage fallingImage = new FallingSleepImage
            {
                gameObject = sleepImage.gameObject,
                rectTransform = rectTransform,
                fallSpeed = Random.Range(sleepFallSpeedMin, sleepFallSpeedMax)
            };

            fallingImages.Add(fallingImage);

            // 불규칙한 간격으로 생성
            yield return new WaitForSeconds(Random.Range(sleepSpawnInterval * 0.5f, sleepSpawnInterval * 1.5f));
        }
    }

    IEnumerator MouseSensitivityObstacle()
    {
        mouseObstacleUsed = true;
        Debug.Log("마우스 감도 이상 발동!");

        mouseSensitivityMultiplier = abnormalSensitivity;

        yield return new WaitForSeconds(mouseSensitivityDuration);

        mouseSensitivityMultiplier = 1f;

        Debug.Log("마우스 감도 정상화");
    }

    // 마우스 입력 처리 시 이 함수 사용
    public float GetAdjustedMouseSensitivity()
    {
        return mouseSensitivityMultiplier;
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

        // 모든 방해 공작 중지
        StopAllCoroutines();
        if (sleepTextPanel != null)
        {
            // 모든 자식 오브젝트 제거
            foreach (Transform child in sleepTextPanel.transform)
            {
                Destroy(child.gameObject);
            }
            sleepTextPanel.SetActive(false);
        }
        mouseSensitivityMultiplier = 1f;

        if (isSuccess)
        {
            int baseScore = Mathf.CeilToInt(timeLeft);
            int totalScore = baseScore + bonusScore;

            Debug.Log("게임 성공! 기본 점수: " + baseScore + "점, 보너스: " + bonusScore + "점, 총점: " + totalScore + "점");

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