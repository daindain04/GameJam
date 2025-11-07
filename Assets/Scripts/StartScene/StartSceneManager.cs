using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject explainPanel;   // 설명창 패널
    [SerializeField] private string mainSceneName = "Main"; // 이동할 메인 씬 이름

    void Awake()
    {
        // 실행 시 설명창은 닫힌 상태로 시작
        if (explainPanel != null) explainPanel.SetActive(false);
    }

    // Start 버튼에 연결
    public void OnClickStart()
    {
        // 빌드 세팅에 등록된 씬 이름과 일치해야 합니다.
        SceneManager.LoadScene(mainSceneName);
    }

    // How To Play 버튼에 연결
    public void OnClickHowToPlay()
    {
        if (explainPanel != null) explainPanel.SetActive(true);
    }

    // 닫기(X) 버튼에 연결
    public void OnClickCloseExplain()
    {
        if (explainPanel != null) explainPanel.SetActive(false);
    }
}
