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

    void Start()
    {
        // 이미 직군을 선택했는지 확인
        if (PlayerPrefs.HasKey("SelectedCharacter"))
        {
            // 이미 선택했으면 패널 비활성화
            if (characterChoosePanel != null)
            {
                characterChoosePanel.SetActive(false);
            }

            Debug.Log("이미 선택한 직군: " + PlayerPrefs.GetString("SelectedCharacter"));
        }
        else
        {
            // 처음이면 패널 활성화
            if (characterChoosePanel != null)
            {
                characterChoosePanel.SetActive(true);
            }

            // 버튼 이벤트 연결
            if (artistButton != null)
            {
                artistButton.onClick.AddListener(() => SelectCharacter("Artist"));
            }

            if (programmerButton != null)
            {
                programmerButton.onClick.AddListener(() => SelectCharacter("Programmer"));
            }

            if (designerButton != null)
            {
                designerButton.onClick.AddListener(() => SelectCharacter("Designer"));
            }
        }
    }

    void SelectCharacter(string characterName)
    {
        Debug.Log("선택한 직군: " + characterName);

        // 직군 저장
        PlayerPrefs.SetString("SelectedCharacter", characterName);

        // 각 직군별 보너스 점수 초기화
        PlayerPrefs.SetInt("ArtBonusScore", characterName == "Artist" ? bonusScore : 0);
        PlayerPrefs.SetInt("TechBonusScore", characterName == "Programmer" ? bonusScore : 0);
        PlayerPrefs.SetInt("DesignBonusScore", characterName == "Designer" ? bonusScore : 0);

        PlayerPrefs.Save();

        // 패널 비활성화
        if (characterChoosePanel != null)
        {
            characterChoosePanel.SetActive(false);
        }

        // MainSceneManager에게 알림 (점수 업데이트)
        MainSceneManager mainManager = FindObjectOfType<MainSceneManager>();
        if (mainManager != null)
        {
            mainManager.UpdateScoreDisplays();
        }
    }

    // 테스트용: 직군 선택 초기화
    public void ResetCharacterSelection()
    {
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.DeleteKey("ArtBonusScore");
        PlayerPrefs.DeleteKey("TechBonusScore");
        PlayerPrefs.DeleteKey("DesignBonusScore");
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}