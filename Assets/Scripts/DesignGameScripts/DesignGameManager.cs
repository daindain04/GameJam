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
    public int totalDocuments = 12;

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
    public Image[] documentImages; // ★ 변경: 12개의 Image 컴포넌트

    [Header("UI")]
    public Image timerFillImage;
    public TextMeshProUGUI leftAmountText;
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("메인 씬")]
    public string mainSceneName = "Main";

    private int documentsRemaining;
    private float timeLeft;
    private bool isGameRunning = false;
    private int bonusScore = 0;

    void Start()
    {
        documentsRemaining = totalDocuments;
        timeLeft = gameTime;

        // 보너스 점수 로드
        bonusScore = PlayerPrefs.GetInt("DesignBonusScore", 0);

        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

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