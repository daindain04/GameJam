using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    [Header("버튼들")]
    public Button artButton;
    public Button techButton;
    public Button designButton;
    public Button resultButton;

    [Header("점수 텍스트들")]
    public TextMeshProUGUI artScoreText;
    public TextMeshProUGUI techScoreText;
    public TextMeshProUGUI designScoreText;

    [Header("씬 이름들")]
    public string artSceneName = "Art";
    public string techSceneName = "Tech";
    public string designSceneName = "Design";
    public string resultSceneName = "Result";

    [Header("경고 메시지 (선택사항)")]
    public GameObject warningPanel; // 경고 패널 (선택사항)
    public TextMeshProUGUI warningText; // 경고 텍스트 (선택사항)

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (artButton != null)
        {
            artButton.onClick.AddListener(() => OnGameButtonClick(artSceneName, "Art"));
        }

        if (techButton != null)
        {
            techButton.onClick.AddListener(() => OnGameButtonClick(techSceneName, "Tech"));
        }

        if (designButton != null)
        {
            designButton.onClick.AddListener(() => OnGameButtonClick(designSceneName, "Design"));
        }

        if (resultButton != null)
        {
            resultButton.onClick.AddListener(OnResultButtonClick);
        }

        // 경고 패널 초기화
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }

        // 점수 표시 업데이트
        UpdateScoreDisplays();

        // 버튼 상태 업데이트
        UpdateGameButtonsState();
        UpdateResultButtonState();
    }

    void OnGameButtonClick(string sceneName, string gameType)
    {
        // 이미 플레이한 게임인지 확인
        if (PlayerPrefs.HasKey(gameType + "LastScore"))
        {
            Debug.Log($"{gameType} 게임은 이미 플레이했습니다!");
            ShowWarning($"{gameType} 게임은 이미 플레이했습니다!\n다른 게임을 선택해주세요.");
            return;
        }

        Debug.Log(sceneName + " 씬으로 이동");

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    void OnResultButtonClick()
    {
        // 모든 게임을 플레이했는지 확인
        bool hasArtRecord = PlayerPrefs.HasKey("ArtLastScore");
        bool hasTechRecord = PlayerPrefs.HasKey("TechLastScore");
        bool hasDesignRecord = PlayerPrefs.HasKey("DesignLastScore");

        if (!hasArtRecord || !hasTechRecord || !hasDesignRecord)
        {
            // 플레이하지 않은 게임이 있음
            string missingGames = "";

            if (!hasArtRecord) missingGames += "Art ";
            if (!hasTechRecord) missingGames += "Tech ";
            if (!hasDesignRecord) missingGames += "Design ";

            Debug.Log($"모든 게임을 플레이해야 합니다! 남은 게임: {missingGames}");

            // 경고 메시지 표시 (패널이 있는 경우)
            ShowWarning($"모든 게임을 플레이해야 합니다!\n남은 게임: {missingGames}");

            return; // 결과 화면으로 이동하지 않음
        }

        // 모든 게임을 플레이했으면 결과 계산
        Debug.Log("결과 보기 - 총점 계산 중...");

        // 각 게임의 점수 계산
        int artTotal = CalculateGameTotal("Art");
        int techTotal = CalculateGameTotal("Tech");
        int designTotal = CalculateGameTotal("Design");

        int finalScore = artTotal + techTotal + designTotal;

        Debug.Log($"Art: {artTotal}점, Tech: {techTotal}점, Design: {designTotal}점 = 총합: {finalScore}점");

        // 총점을 PlayerPrefs에 저장
        PlayerPrefs.SetInt("FinalTotalScore", finalScore);
        PlayerPrefs.Save();

        // Result 씬으로 이동
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(resultSceneName);
        }
        else
        {
            SceneManager.LoadScene(resultSceneName);
        }
    }

    int CalculateGameTotal(string gameType)
    {
        int result = PlayerPrefs.GetInt(gameType + "GameResult", 0);

        // 실패한 게임은 0점
        if (result == 0)
        {
            return 0;
        }

        // 성공한 게임: 기본 점수 + 보너스 점수
        int lastScore = PlayerPrefs.GetInt(gameType + "LastScore", 0);
        int bonusScore = PlayerPrefs.GetInt(gameType + "BonusScore", 0);

        return lastScore + bonusScore;
    }

    public void UpdateScoreDisplays()
    {
        // Art 점수 표시
        UpdateSingleScore(artScoreText, "Art");

        // Tech 점수 표시
        UpdateSingleScore(techScoreText, "Tech");

        // Design 점수 표시
        UpdateSingleScore(designScoreText, "Design");

        // 버튼 상태 업데이트
        UpdateGameButtonsState();
        UpdateResultButtonState();
    }

    void UpdateSingleScore(TextMeshProUGUI scoreText, string gameType)
    {
        if (scoreText == null) return;

        string keyPrefix = gameType;
        int bonusScore = PlayerPrefs.GetInt(gameType + "BonusScore", 0);

        // 게임 플레이 기록이 있는 경우
        if (PlayerPrefs.HasKey(keyPrefix + "LastScore"))
        {
            int lastScore = PlayerPrefs.GetInt(keyPrefix + "LastScore");
            int result = PlayerPrefs.GetInt(keyPrefix + "GameResult", 0);
            int totalScore = lastScore + bonusScore;

            string text = "";

            // 보너스가 있으면 특화 직군 표시
            if (bonusScore > 0)
            {
                text = "★ 특화 직군 ★\n";
            }

            if (result == 1) // 성공
            {
                text += "최근: " + lastScore + "점";

                if (bonusScore > 0)
                {
                    text += "\n보너스: +" + bonusScore + "점";
                    text += "\n총점: " + totalScore + "점";
                }

                text += "\n(성공)";
                scoreText.color = bonusScore > 0 ? new Color(1f, 0.84f, 0f) : Color.green;
            }
            else // 실패
            {
                text += "최근: 실패";

                if (bonusScore > 0)
                {
                    text += "\n보너스: +" + bonusScore + "점";
                    text += "\n(실패했지만 보너스 획득)";
                }

                scoreText.color = bonusScore > 0 ? new Color(1f, 0.5f, 0f) : Color.red;
            }

            scoreText.text = text;
        }
        // 게임 플레이 기록이 없는 경우
        else
        {
            if (bonusScore > 0)
            {
                scoreText.text = "★ 특화 직군 ★\n보너스: +" + bonusScore + "점\n플레이 기록 없음";
                scoreText.color = new Color(1f, 0.84f, 0f); // 골드 컬러
            }
            else
            {
                scoreText.text = "플레이 기록 없음";
                scoreText.color = Color.gray;
            }
        }
    }

    // ★ 게임 버튼들의 활성화/비활성화 상태 업데이트
    void UpdateGameButtonsState()
    {
        // Art 버튼 상태
        if (artButton != null)
        {
            bool hasArtRecord = PlayerPrefs.HasKey("ArtLastScore");
            artButton.interactable = !hasArtRecord;
        }

        // Tech 버튼 상태
        if (techButton != null)
        {
            bool hasTechRecord = PlayerPrefs.HasKey("TechLastScore");
            techButton.interactable = !hasTechRecord;
        }

        // Design 버튼 상태
        if (designButton != null)
        {
            bool hasDesignRecord = PlayerPrefs.HasKey("DesignLastScore");
            designButton.interactable = !hasDesignRecord;
        }
    }

    // 결과 버튼 활성화/비활성화 상태 업데이트
    void UpdateResultButtonState()
    {
        if (resultButton == null) return;

        bool hasArtRecord = PlayerPrefs.HasKey("ArtLastScore");
        bool hasTechRecord = PlayerPrefs.HasKey("TechLastScore");
        bool hasDesignRecord = PlayerPrefs.HasKey("DesignLastScore");

        bool allGamesPlayed = hasArtRecord && hasTechRecord && hasDesignRecord;

        // 모든 게임을 플레이했으면 버튼 활성화, 아니면 비활성화
        resultButton.interactable = allGamesPlayed;

        // 버튼 색상 변경 (선택사항)
        ColorBlock colors = resultButton.colors;
        if (allGamesPlayed)
        {
            colors.normalColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        resultButton.colors = colors;

        Debug.Log($"결과 버튼 상태: {(allGamesPlayed ? "활성화" : "비활성화")} (Art: {hasArtRecord}, Tech: {hasTechRecord}, Design: {hasDesignRecord})");
    }

    // 경고 메시지 표시
    void ShowWarning(string message)
    {
        if (warningPanel != null && warningText != null)
        {
            warningText.text = message;
            warningPanel.SetActive(true);

            // 2초 후 자동으로 숨기기
            Invoke("HideWarning", 2f);
        }
        else
        {
            // 패널이 없으면 콘솔에만 출력
            Debug.LogWarning(message);
        }
    }

    void HideWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
}