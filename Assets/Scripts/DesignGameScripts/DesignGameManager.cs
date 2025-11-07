using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DesignGameManager : MonoBehaviour
{
    [Header("카운트다운")]
    public GameObject countdownPanel;
    public Image countdownImage;
    public Sprite count3Sprite;
    public Sprite count2Sprite;
    public Sprite count1Sprite;
    public float countdownDuration = 1f;

    [Header("게임 설정")]
    public float gameTime = 60f;
    public int totalDocuments = 18;

    [Header("파일 및 쓰레기통")]
    public Image file1Target;
    public Image file2Target;
    public Image file3Target;
    public Image trashTarget;

    [Header("문서 스프라이트 - 파일1용 (3개)")]
    public Sprite file1DocSprite;

    [Header("문서 스프라이트 - 파일2용 (3개)")]
    public Sprite file2DocSprite;

    [Header("문서 스프라이트 - 파일3용 (3개)")]
    public Sprite file3DocSprite;

    [Header("문서 스프라이트 - 쓰레기통용 (3개)")]
    public Sprite trash1DocSprite;
    public Sprite trash2DocSprite;
    public Sprite trash3DocSprite;

    [Header("미리 배치된 문서 이미지 (12개)")]
    public Image[] documentImages;

    [Header("UI")]
    public Image timerFillImage;
    public TextMeshProUGUI leftAmountText;
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("방해 공작 시스템")]
    public GameObject sleepTextPanel; // 졸음 글자 패널
    public Sprite[] sleepSprites; // 졸음 글자 스프라이트들
    public Image sleepImagePrefab; // 졸음 이미지 프리팹
    public int sleepImageCount = 15; // 생성할 졸음 이미지 개수
    public float sleepDuration = 2f; // 졸음 지속 시간
    public float sleepSpawnInterval = 0.1f; // 졸음 이미지 생성 간격
    public float sleepFallSpeedMin = 200f; // 최소 낙하 속도
    public float sleepFallSpeedMax = 400f; // 최대 낙하 속도
    public GameObject popupPanel; // 팝업 패널
    public Button popupCloseButton; // 팝업 닫기 버튼 (X 버튼)

    [Header("메인 씬")]
    public string mainSceneName = "Main";

    private int documentsRemaining;
    private float timeLeft;
    private bool isGameRunning = false;
    private int bonusScore = 0;

    private bool sleepObstacleUsed = false;
    private bool popupObstacleUsed = false;

    // 떨어지는 이미지 정보 저장용 클래스
    private class FallingSleepImage
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public float fallSpeed;
    }

    void Start()
    {
        documentsRemaining = totalDocuments;
        timeLeft = gameTime;

        // 보너스 점수 로드
        bonusScore = PlayerPrefs.GetInt("DesignBonusScore", 0);

        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (sleepTextPanel != null) sleepTextPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);

        // 팝업 닫기 버튼 연결
        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.AddListener(ClosePopup);
        }

        // 타이머 설정
        if (timerFillImage != null)
        {
            if (timerFillImage.type != Image.Type.Filled)
            {
                timerFillImage.type = Image.Type.Filled;
                timerFillImage.fillMethod = Image.FillMethod.Horizontal;
            }
            timerFillImage.fillAmount = 1.0f;
        }

        // 게임 시작 전 카운트다운
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        // 3
        if (countdownImage != null && count3Sprite != null)
        {
            countdownImage.sprite = count3Sprite;
            yield return new WaitForSeconds(countdownDuration);
        }

        // 2
        if (countdownImage != null && count2Sprite != null)
        {
            countdownImage.sprite = count2Sprite;
            yield return new WaitForSeconds(countdownDuration);
        }

        // 1
        if (countdownImage != null && count1Sprite != null)
        {
            countdownImage.sprite = count1Sprite;
            yield return new WaitForSeconds(countdownDuration);
        }

        // 카운트다운 종료
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        // 게임 시작!
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("기획 게임 시작!");

        // 문서에 스프라이트 할당
        AssignDocumentSprites();

        // 게임 시작
        isGameRunning = true;

        UpdateUI();

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
        bool canUsePopup = !popupObstacleUsed;

        if (!canUseSleep && !canUsePopup) return;

        int obstacleType;

        if (canUseSleep && canUsePopup)
        {
            obstacleType = Random.Range(0, 2); // 0: 졸음, 1: 팝업
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
            PopupObstacle();
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

    void PopupObstacle()
    {
        popupObstacleUsed = true;
        Debug.Log("팝업 방해 공작 발동!");

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        Debug.Log("팝업 닫음");
    }

    void AssignDocumentSprites()
    {
        // 문서 데이터 리스트 생성 (각 3개씩, 총 12개)
        List<DocumentData> documentList = new List<DocumentData>();

        // 파일1 문서 3개
        for (int i = 0; i < 3; i++)
        {
            documentList.Add(new DocumentData(file1DocSprite, DocumentType.File1));
        }

        // 파일2 문서 3개
        for (int i = 0; i < 3; i++)
        {
            documentList.Add(new DocumentData(file2DocSprite, DocumentType.File2));
        }

        // 파일3 문서 3개
        for (int i = 0; i < 3; i++)
        {
            documentList.Add(new DocumentData(file3DocSprite, DocumentType.File3));
        }

        // 쓰레기통 문서 3개
        documentList.Add(new DocumentData(trash1DocSprite, DocumentType.Trash));
        documentList.Add(new DocumentData(trash2DocSprite, DocumentType.Trash));
        documentList.Add(new DocumentData(trash3DocSprite, DocumentType.Trash));

        // ★ 추가 6개 문서를 랜덤으로 선택
        List<DocumentData> additionalDocs = new List<DocumentData>();

        // 가능한 모든 문서 타입의 풀 생성
        List<DocumentData> documentPool = new List<DocumentData>
    {
        new DocumentData(file1DocSprite, DocumentType.File1),
        new DocumentData(file2DocSprite, DocumentType.File2),
        new DocumentData(file3DocSprite, DocumentType.File3),
        new DocumentData(trash1DocSprite, DocumentType.Trash),
        new DocumentData(trash2DocSprite, DocumentType.Trash),
        new DocumentData(trash3DocSprite, DocumentType.Trash)
    };

        // 랜덤으로 6개 선택
        for (int i = 0; i < 6; i++)
        {
            int randomIndex = Random.Range(0, documentPool.Count);
            additionalDocs.Add(new DocumentData(documentPool[randomIndex].sprite, documentPool[randomIndex].type));
        }

        // 추가 문서를 메인 리스트에 합치기
        documentList.AddRange(additionalDocs);

        Debug.Log("총 문서 개수: " + documentList.Count + "개 (기본 12개 + 추가 6개)");

        // 리스트 섞기 (랜덤 배치)
        ShuffleList(documentList);

        // 각 Image에 스프라이트 및 타입 할당
        for (int i = 0; i < documentImages.Length && i < documentList.Count; i++)
        {
            if (documentImages[i] != null)
            {
                // 스프라이트 설정
                documentImages[i].sprite = documentList[i].sprite;

                // DraggableDocument 컴포넌트 초기화
                DraggableDocument doc = documentImages[i].GetComponent<DraggableDocument>();
                if (doc != null)
                {
                    doc.Initialize(documentList[i].type, this);
                }

                // 활성화
                documentImages[i].gameObject.SetActive(true);
            }
        }

        Debug.Log("문서 " + documentImages.Length + "개에 스프라이트 할당 완료");
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
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
            leftAmountText.text = documentsRemaining.ToString();
        }
    }

    public void OnDocumentPlaced()
    {
        if (!isGameRunning) return;

        documentsRemaining--;

        Debug.Log("문서 정리 완료! 남은 개수: " + documentsRemaining);

        if (documentsRemaining <= 0)
        {
            GameOver(true);
        }
    }

    public bool IsCorrectTarget(DocumentType docType, Image targetImage)
    {
        if (docType == DocumentType.File1 && targetImage == file1Target) return true;
        if (docType == DocumentType.File2 && targetImage == file2Target) return true;
        if (docType == DocumentType.File3 && targetImage == file3Target) return true;
        if (docType == DocumentType.Trash && targetImage == trashTarget) return true;

        return false;
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
        if (popupPanel != null) popupPanel.SetActive(false);

        if (isSuccess)
        {
            int baseScore = Mathf.CeilToInt(timeLeft);
            int totalScore = baseScore + bonusScore;

            Debug.Log("게임 성공! 기본 점수: " + baseScore + "점, 보너스: " + bonusScore + "점");

            PlayerPrefs.SetInt("DesignLastScore", baseScore);
            PlayerPrefs.SetInt("DesignGameResult", 1);
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

            PlayerPrefs.SetInt("DesignLastScore", 0);
            PlayerPrefs.SetInt("DesignGameResult", 0);
            PlayerPrefs.Save();

            if (failPanel != null)
            {
                failPanel.SetActive(true);
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

// 문서 타입 열거형
public enum DocumentType
{
    File1,
    File2,
    File3,
    Trash
}

// 문서 데이터 클래스
public class DocumentData
{
    public Sprite sprite;
    public DocumentType type;

    public DocumentData(Sprite sprite, DocumentType type)
    {
        this.sprite = sprite;
        this.type = type;
    }
}