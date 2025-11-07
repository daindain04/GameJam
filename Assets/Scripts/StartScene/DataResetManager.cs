using UnityEngine;

public class DataResetManager : MonoBehaviour
{
    [Header("데이터 초기화 (인스펙터 전용)")]
    [SerializeField] private bool resetArtData = false;
    [SerializeField] private bool resetTechData = false;
    [SerializeField] private bool resetDesignData = false;
    [SerializeField] private bool resetAllData = false;

    // ★ 데이터 초기화 함수들
    private void ResetGameData(string gameType)
    {
        PlayerPrefs.DeleteKey(gameType + "LastScore");
        PlayerPrefs.DeleteKey(gameType + "GameResult");
        PlayerPrefs.DeleteKey(gameType + "BonusScore");
        PlayerPrefs.Save();
        Debug.Log(gameType + " 데이터 초기화 완료!");
    }

    public void ResetArtData()
    {
        ResetGameData("Art");
    }

    public void ResetTechData()
    {
        ResetGameData("Tech");
    }

    public void ResetDesignData()
    {
        ResetGameData("Design");
    }

    public void ResetAllGameData()
    {
        ResetArtData();
        ResetTechData();
        ResetDesignData();
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.Save();
        Debug.Log("모든 게임 데이터 초기화 완료!");
    }

    // 인스펙터에서 체크박스로 초기화
    private void OnValidate()
    {
        if (resetArtData)
        {
            resetArtData = false;
            if (Application.isPlaying)
            {
                ResetArtData();
            }
        }

        if (resetTechData)
        {
            resetTechData = false;
            if (Application.isPlaying)
            {
                ResetTechData();
            }
        }

        if (resetDesignData)
        {
            resetDesignData = false;
            if (Application.isPlaying)
            {
                ResetDesignData();
            }
        }

        if (resetAllData)
        {
            resetAllData = false;
            if (Application.isPlaying)
            {
                ResetAllGameData();
            }
        }
    }
}