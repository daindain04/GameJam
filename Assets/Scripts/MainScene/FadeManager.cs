using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("페이드 패널")]
    public Image fadePanel;

    [Header("페이드 설정")]
    public float fadeDuration = 1f;

    private bool isFading = false;
    private bool isFirstLoad = true; // ★ 첫 로드 체크

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // FadeCanvas도 DontDestroyOnLoad
            if (fadePanel != null && fadePanel.canvas != null)
            {
                DontDestroyOnLoad(fadePanel.canvas.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 페이드 패널 초기화
        if (fadePanel != null)
        {
            Debug.Log("FadePanel 초기화 완료");

            // ★ 시작 시 불투명하게 설정
            fadePanel.gameObject.SetActive(true);
            Color color = fadePanel.color;
            color.a = 1f;
            fadePanel.color = color;
        }
        else
        {
            Debug.LogError("FadePanel이 연결되지 않았습니다!");
        }
    }

    void Start()
    {
        // 씬 로드 완료 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;

        // ★ StartScene에서 시작하는 경우 바로 페이드 인
        if (isFirstLoad)
        {
            Debug.Log("첫 로드 - 페이드 인 시작");
            StartCoroutine(FadeIn());
            isFirstLoad = false;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("씬 로드됨: " + scene.name);

        // ★ 첫 로드가 아닐 때만 자동 페이드 인
        if (!isFirstLoad)
        {
            StartCoroutine(FadeIn());
        }
    }

    // 페이드 인: 불투명(1) → 투명(0) → 비활성화
    public IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanel이 null입니다!");
            yield break;
        }

        Debug.Log("페이드 인 시작");

        isFading = true;

        // 패널 활성화 및 불투명 상태로 시작
        fadePanel.gameObject.SetActive(true);
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;

        float elapsedTime = 0f;

        // 불투명 → 투명
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        // 완전히 투명하게
        color.a = 0f;
        fadePanel.color = color;

        // ★ 투명해지면 패널 비활성화!
        fadePanel.gameObject.SetActive(false);

        Debug.Log("페이드 인 완료 - 패널 비활성화됨");

        isFading = false;
    }

    // 페이드 아웃: 비활성화 → 활성화 → 투명(0) → 불투명(1)
    public IEnumerator FadeOut()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanel이 null입니다!");
            yield break;
        }

        Debug.Log("페이드 아웃 시작");

        isFading = true;

        // ★ 패널 활성화 및 투명 상태로 시작
        fadePanel.gameObject.SetActive(true);
        Color color = fadePanel.color;
        color.a = 0f;
        fadePanel.color = color;

        float elapsedTime = 0f;

        // 투명 → 불투명
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        // 완전히 불투명하게
        color.a = 1f;
        fadePanel.color = color;

        Debug.Log("페이드 아웃 완료 - 패널 활성화 상태 유지");

        isFading = false;
    }

    // 페이드 아웃 후 씬 전환
    public void FadeOutAndLoadScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
        else
        {
            Debug.LogWarning("이미 페이드 중입니다!");
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        Debug.Log(sceneName + " 씬으로 전환");
        SceneManager.LoadScene(sceneName);
    }

}