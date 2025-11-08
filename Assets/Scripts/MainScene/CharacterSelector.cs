using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [Header("직군 선택 패널")]
    public GameObject characterChoosePanel;

    [Header("직군 버튼들")]
    public Button artistButton;
    public Button programmerButton;
    public Button designerButton;

    [Header("보너스 점수")]
    public int bonusScore = 10;

    [Header("노하우 시스템")]
    public TextMeshProUGUI knowHowText; // 노하우 수치 표시
    public TextMeshProUGUI remainingSelectionsText; // 남은 선택 횟수 표시

    private int knowHow; // 현재 노하우 수치
    private int remainingSelections; // 남은 선택 횟수

    void Start()
    {
        // 노하우 수치 불러오기
        knowHow = PlayerPrefs.GetInt("KnowHow", 0);

        // 노하우 100당 1번 추가 선택 가능
        int bonusSelections = knowHow / 100;

        // 캐릭터 재선택이 필요한지 확인
        bool needSelection = PlayerPrefs.GetInt("NeedCharacterSelection", 0) == 1;

        // 최초 실행인지 확인 (캐릭터를 한 번도 선택하지 않았고, 재선택 플래그도 없음)
        bool isFirstTime = !PlayerPrefs.HasKey("SelectedCharacter") && !needSelection;

        if (isFirstTime)
        {
            // 최초 실행: 기본 1회 선택
            remainingSelections = 1;
            ShowCharacterPanel();
            Debug.Log("최초 캐릭터 선택");
        }
        else if (needSelection)
        {
            // 재도전/좀 더 하기로 돌아온 경우: 1회 + 보너스 선택
            remainingSelections = 1 + bonusSelections;
            ShowCharacterPanel();
            Debug.Log($"재선택 - 기본 1회 + 보너스 {bonusSelections}회 = 총 {remainingSelections}회");
        }
        else
        {
            // 이미 선택 완료 상태
            HideCharacterPanel();
            Debug.Log("이미 선택 완료");
        }

        // UI 업데이트
        UpdateUI();
    }

    void ShowCharacterPanel()
    {
        if (characterChoosePanel != null)
        {
            characterChoosePanel.SetActive(true);
        }

        // 버튼 이벤트 연결
        if (artistButton != null)
        {
            artistButton.onClick.RemoveAllListeners();
            artistButton.onClick.AddListener(() => SelectCharacter("Artist"));
        }
        if (programmerButton != null)
        {
            programmerButton.onClick.RemoveAllListeners();
            programmerButton.onClick.AddListener(() => SelectCharacter("Programmer"));
        }
        if (designerButton != null)
        {
            designerButton.onClick.RemoveAllListeners();
            designerButton.onClick.AddListener(() => SelectCharacter("Designer"));
        }
    }

    void HideCharacterPanel()
    {
        if (characterChoosePanel != null)
        {
            characterChoosePanel.SetActive(false);
        }
    }

    void UpdateUI()
    {
        // 노하우 수치 표시
        if (knowHowText != null)
        {
            knowHowText.text = $"노하우: {knowHow}";
        }

        // 남은 선택 횟수 표시
        if (remainingSelectionsText != null)
        {
            remainingSelectionsText.text = $"남은 선택 횟수: {remainingSelections}";
        }
    }

    void SelectCharacter(string characterName)
    {
        Debug.Log($"선택한 직군: {characterName}");

        // 기존 보너스 점수 불러오기 (이미 선택한 적이 있다면 누적)
        int currentArtBonus = PlayerPrefs.GetInt("ArtBonusScore", 0);
        int currentTechBonus = PlayerPrefs.GetInt("TechBonusScore", 0);
        int currentDesignBonus = PlayerPrefs.GetInt("DesignBonusScore", 0);

        // 선택한 직군에 보너스 점수 추가
        if (characterName == "Artist")
        {
            currentArtBonus += bonusScore;
        }
        else if (characterName == "Programmer")
        {
            currentTechBonus += bonusScore;
        }
        else if (characterName == "Designer")
        {
            currentDesignBonus += bonusScore;
        }

        // 보너스 점수 저장 (누적)
        PlayerPrefs.SetInt("ArtBonusScore", currentArtBonus);
        PlayerPrefs.SetInt("TechBonusScore", currentTechBonus);
        PlayerPrefs.SetInt("DesignBonusScore", currentDesignBonus);

        Debug.Log($"보너스 점수 - Art: {currentArtBonus}, Tech: {currentTechBonus}, Design: {currentDesignBonus}");

        // 선택 횟수 감소
        remainingSelections--;
        UpdateUI();

        // 모든 선택을 완료했는지 확인
        if (remainingSelections <= 0)
        {
            // 선택 완료 처리
            PlayerPrefs.SetString("SelectedCharacter", characterName); // 마지막 선택 저장
            PlayerPrefs.DeleteKey("NeedCharacterSelection"); // 재선택 플래그 제거
            PlayerPrefs.Save();

            Debug.Log("캐릭터 선택 완료!");
            HideCharacterPanel();

            // MainSceneManager에게 알림 (점수 업데이트)
            MainSceneManager mainManager = FindObjectOfType<MainSceneManager>();
            if (mainManager != null)
            {
                mainManager.UpdateScoreDisplays();
            }
        }
        else
        {
            Debug.Log($"남은 선택 횟수: {remainingSelections}");
        }
    }

    // 테스트용: 직군 선택 초기화
    public void ResetCharacterSelection()
    {
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.DeleteKey("ArtBonusScore");
        PlayerPrefs.DeleteKey("TechBonusScore");
        PlayerPrefs.DeleteKey("DesignBonusScore");
        PlayerPrefs.DeleteKey("NeedCharacterSelection");
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}