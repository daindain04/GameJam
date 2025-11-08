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

    private int currentScore;

    void Start()
    {
        // 총점 불러오기
        currentScore = PlayerPrefs.GetInt("FinalTotalScore", 0);
        Debug.Log($"최종 점수: {currentScore}점 (합격 기준: {passScore}점)");

        // 성공/실패 판정
        if (currentScore >= passScore)
        {
            ShowSuccess(currentScore);
        }
        else
        {
            ShowFail(currentScore);
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

    // ===== 실패 패널 버튼들 =====

    // 재도전 버튼 - 총점을 노하우에 누적하고 메인씬(캐릭터 선택)으로
    public void OnClickRetry()
    {
        Debug.Log("재도전 - 노하우 누적 후 메인 씬으로 이동");

        // 노하우에 현재 점수 누적
        int currentKnowHow = PlayerPrefs.GetInt("KnowHow", 0);
        currentKnowHow += currentScore;
        PlayerPrefs.SetInt("KnowHow", currentKnowHow);

        Debug.Log($"노하우 누적: {currentScore}점 추가 -> 총 {currentKnowHow}점");

        // 게임 데이터만 초기화 (노하우는 유지)
        ResetGameDataOnly();

        // 캐릭터 재선택 플래그 설정
        PlayerPrefs.SetInt("NeedCharacterSelection", 1);
        PlayerPrefs.Save();

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(mainSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    // 포기 버튼 - 모든 데이터 초기화 후 스타트씬으로
    public void OnClickGiveUp()
    {
        Debug.Log("포기 - 모든 데이터 초기화 후 스타트 씬으로 이동");
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

    // ===== 성공 패널 버튼들 =====

    // 졸업 버튼 - 모든 데이터 초기화 후 스타트씬으로
    public void OnClickGraduate()
    {
        Debug.Log("졸업 - 모든 데이터 초기화 후 스타트 씬으로 이동");
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

    // 좀 더 하기 버튼 - 총점을 노하우에 누적하고 메인씬(캐릭터 선택)으로
    public void OnClickContinue()
    {
        Debug.Log("좀 더 하기 - 노하우 누적 후 메인 씬으로 이동");

        // 노하우에 현재 점수 누적
        int currentKnowHow = PlayerPrefs.GetInt("KnowHow", 0);
        currentKnowHow += currentScore;
        PlayerPrefs.SetInt("KnowHow", currentKnowHow);

        Debug.Log($"노하우 누적: {currentScore}점 추가 -> 총 {currentKnowHow}점");

        // 게임 데이터만 초기화 (노하우는 유지)
        ResetGameDataOnly();

        // 캐릭터 재선택 플래그 설정
        PlayerPrefs.SetInt("NeedCharacterSelection", 1);
        PlayerPrefs.Save();

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutAndLoadScene(mainSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    // ===== 데이터 관리 함수들 =====

    // 게임 데이터만 초기화 (노하우는 유지)
    private void ResetGameDataOnly()
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

        PlayerPrefs.Save();
        Debug.Log("게임 데이터가 초기화되었습니다. (노하우는 유지)");
    }

    // 모든 게임 데이터 초기화 (노하우 포함)
    private void ResetAllData()
    {
        ResetGameDataOnly();

        // 노하우 데이터도 삭제
        PlayerPrefs.DeleteKey("KnowHow");
        PlayerPrefs.DeleteKey("NeedCharacterSelection");

        PlayerPrefs.Save();
        Debug.Log("모든 게임 데이터가 초기화되었습니다. (노하우 포함)");
    }
}