using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultSceneManager : MonoBehaviour
{
    [Header("패널")]
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("점수 텍스트")]
    public TextMeshProUGUI successScoreText;
    public TextMeshProUGUI failScoreText;

    [Header("씬 이름")]
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private string startSceneName = "Start";

    [Header("성공 기준")]
    [SerializeField] private int passScore = 120;

    void Start()
    {
        // 총점 불러오기
        int finalScore = PlayerPrefs.GetInt("FinalTotalScore", 0);

        Debug.Log($"최종 점수: {finalScore}점 (합격 기준: {passScore}점)");

        // 성공/실패 판정
        if (finalScore >= passScore)
        {
            ShowSuccess(finalScore);
        }
        else
        {
            ShowFail(finalScore);
        }
    }

    void ShowSuccess(int score)
    {
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }

        if (successScoreText != null)
        {
            successScoreText.text = "총점: " + score + "점\n합격!";
        }

        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }

        Debug.Log("합격!");
    }

    void ShowFail(int score)
    {
        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }

        if (failScoreText != null)
        {
            failScoreText.text = "총점: " + score + "점\n불합격";
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        Debug.Log("불합격...");
    }

    // 다시하기 버튼 (ReStartButton)에 연결
    public void OnClickRestart()
    {
        Debug.Log("다시하기 - 모든 데이터 초기화 후 메인 씬으로 이동");

        ResetAllData();

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(mainSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    // 메인 버튼 (MainButton)에 연결
    public void OnClickMainMenu()
    {
        Debug.Log("시작화면으로 - 모든 데이터 초기화 후 스타트 씬으로 이동");

        ResetAllData();

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(startSceneName);
        }
        else
        {
            SceneManager.LoadScene(startSceneName);
        }
    }

    // 모든 게임 데이터 초기화
    private void ResetAllData()
    {
        // Art 게임 데이터 삭제
        PlayerPrefs.DeleteKey("ArtLastScore");
        PlayerPrefs.DeleteKey("ArtGameResult");
        PlayerPrefs.DeleteKey("ArtBonusScore");

        // Tech 게임 데이터 삭제
        PlayerPrefs.DeleteKey("TechLastScore");
        PlayerPrefs.DeleteKey("TechGameResult");
        PlayerPrefs.DeleteKey("TechBonusScore");

        // Design 게임 데이터 삭제
        PlayerPrefs.DeleteKey("DesignLastScore");
        PlayerPrefs.DeleteKey("DesignGameResult");
        PlayerPrefs.DeleteKey("DesignBonusScore");

        // 선택된 캐릭터 정보 삭제
        PlayerPrefs.DeleteKey("SelectedCharacter");

        // 최종 점수 삭제
        PlayerPrefs.DeleteKey("FinalTotalScore");

        // 저장
        PlayerPrefs.Save();

        Debug.Log("모든 게임 데이터가 초기화되었습니다.");
    }
}