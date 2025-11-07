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
    public string resultSceneName = "Result"; // ★ 추가

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (artButton != null)
        {
            artButton.onClick.AddListener(() => OnGameButtonClick(artSceneName));
        }

        if (techButton != null)
        {
            techButton.onClick.AddListener(() => OnGameButtonClick(techSceneName));
        }

        if (designButton != null)
        {
            designButton.onClick.AddListener(() => OnGameButtonClick(designSceneName));
        }

        if (resultButton != null)
        {
            resultButton.onClick.AddListener(OnResultButtonClick);
        }

        // 점수 표시 업데이트
        UpdateScoreDisplays();
    }

    void OnGameButtonClick(string sceneName)
    {
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
        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "None");

        // Art 점수 표시
        UpdateSingleScore(artScoreText, "Art", selectedCharacter == "Artist");

        // Tech 점수 표시
        UpdateSingleScore(techScoreText, "Tech", selectedCharacter == "Programmer");

        // Design 점수 표시
        UpdateSingleScore(designScoreText, "Design", selectedCharacter == "Designer");
    }

    void UpdateSingleScore(TextMeshProUGUI scoreText, string gameType, bool hasBonus)
    {
        if (scoreText == null) return;

        string keyPrefix = gameType;
        int bonusScore = PlayerPrefs.GetInt(gameType + "BonusScore", 0);

        if (PlayerPrefs.HasKey(keyPrefix + "LastScore"))
        {
            int lastScore = PlayerPrefs.GetInt(keyPrefix + "LastScore");
            int result = PlayerPrefs.GetInt(keyPrefix + "GameResult", 0);
            int totalScore = lastScore + bonusScore;

            string text = "";

            if (hasBonus && bonusScore > 0)
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
                scoreText.color = hasBonus ? new Color(1f, 0.84f, 0f) : Color.green;
            }
            else // 실패
            {
                text += "최근: 실패";

                if (bonusScore > 0)
                {
                    text += "\n보너스: +" + bonusScore + "점";
                }

                scoreText.color = Color.red;
            }

            scoreText.text = text;
        }
        else
        {
            if (hasBonus && bonusScore > 0)
            {
                scoreText.text = "★ 특화 직군 ★\n보너스: +" + bonusScore + "점\n플레이 기록 없음";
                scoreText.color = new Color(1f, 0.84f, 0f);
            }
            else
            {
                scoreText.text = "플레이 기록 없음";
                scoreText.color = Color.gray;
            }
        }
    }
}