using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text ruleText;
    public TMP_InputField inputField;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;
    public TMP_Text timerText;  // ⏱ 추가!
    public Slider timeSlider;
    public Image countdownImage;

    [Header("Game Data")]
    public List<string> wordList;
    private string currentWord;
    private int score = 0;
    public int Bonus = 10;
    private bool timerActive = false;
    public Sprite count3Sprite;
    public Sprite count2Sprite;
    public Sprite count1Sprite;
    public Sprite succes_1;
    public Sprite failed;

    [Header("Random Event Settings")]
    public int clearCount = 2;          // 총 3회 발생
    public float minClearDelay = 5f;    // 최소 대기 시간 (초)
    public float maxClearDelay = 20f;   // 최대 대기 시간 (초)
    private int clearsDone = 0;         // 지금까지 몇 번 발생했는지
    public AudioSource sfxSource;       // 효과음 재생기
    public AudioClip clearSound;        // 입력 삭제 효과음

    [Header("Timer Settings")]
    public float turnTime = 20f;   // 제한 시간 (초)
    public float inPenalty = 1f;
    public float timePenalty = 1f;
    private float currentTime;
    private bool isPlaying = true;


    [Header("Result UI")]
    public TMP_Text resultText;     // 점수 표시용 텍스트
    public Image succes;

    [Header("main scene")]
    public string mainSceneName = "Main";
    void Start()
    {
        wordList = new List<string>() { "feedbackText.text = 'correct!';", "string userWord = input.Trim();", "if (!isPlaying) return;", "feedbackText.text = ' ';", "ruleText.text = $'word: {currentWord}';", "void OnWordSubmitted(string input)" };
        currentWord = "Ready...";
        inputField.ActivateInputField();
        ruleText.text = $"word: {currentWord}";
        feedbackText.text = "";
        scoreText.text = "point: 0";

        currentTime = turnTime;
        UpdateTimerText();

        inputField.onSubmit.AddListener(OnWordSubmitted);
        StartCoroutine(StartCountdown());
        succes.enabled = false;
        resultText.text = "";
    }


    void Update()
    {
        if (!isPlaying) return;
        if (!timerActive) return;

        // 타이머 감소
        currentTime -= Time.deltaTime;
        timeSlider.value = currentTime;
        UpdateTimerText();


        // 시간 초과 체크
        if (currentTime <= 0)
        {
            TimeOver();
        }
    }

    void OnWordSubmitted(string input)
    {
        string userWord = input.Trim();
        inputField.ActivateInputField();

        if (CheckWord(userWord))
        {
            score += Bonus;
            feedbackText.text = "correct!";
            inputField.text = "";

            currentWord = wordList[Random.Range(0, wordList.Count)];
            ruleText.text = $"word: {currentWord}";
            scoreText.text = $"point: {score}";

            currentTime = turnTime - timePenalty++;
            StartCoroutine(HideFeedbackAfterDelay(1f, feedbackText));
        }
        else
        {
            currentTime -= inPenalty;
            feedbackText.text = "incorrect.";
            StartCoroutine(HideFeedbackAfterDelay(1f, feedbackText));
        }
    }

    void TimeOver()
    {
        isPlaying = false;
        inputField.interactable = false;
        resultText.text = $"게임 종료!\n최종 점수: {score}";

        StartCoroutine(FinalResult(2f));

    }

    void UpdateTimerText()
    {
        timerText.text = $"{currentTime:F1}";
    }

    bool CheckWord(string userWord)
    {
        if (userWord == currentWord) return true;

        else return false;
    }
    private IEnumerator HideFeedbackAfterDelay(float delay, TMP_Text a)
    {
        yield return new WaitForSeconds(delay);
        a.text = "";
    }
    IEnumerator StartCountdown()
    {
        countdownImage.enabled = true;
        countdownImage.sprite = count3Sprite;
        yield return new WaitForSeconds(1f);

        countdownImage.sprite = count2Sprite;
        yield return new WaitForSeconds(1f);

        countdownImage.sprite = count1Sprite;
        yield return new WaitForSeconds(1f);

        // 🔸 카운트다운 종료 → 실제 단어 시작
        StartTimer();
    }
    void StartTimer()
    {
        countdownImage.enabled = false;

        currentWord = wordList[Random.Range(0, wordList.Count)];
        ruleText.text = $"word: {currentWord}";

        timerActive = true;

        StartCoroutine(RandomClearRoutine());
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
    IEnumerator FinalResult(float delay)
    {
        if (score > 50)
        {
            yield return new WaitForSeconds(delay);
            resultText.text = "";
            succes.enabled = true;
            succes.sprite = succes_1;
            PlayerPrefs.SetInt("TechLastScore", score);
            PlayerPrefs.SetInt("TechGameResult", 1);
            PlayerPrefs.Save();
        }
        else if (score < 49)
        {
            yield return new WaitForSeconds(delay);
            resultText.text = "";
            succes.enabled = true;
            succes.sprite = failed;
            PlayerPrefs.SetInt("TechLastScore", score);
            PlayerPrefs.SetInt("TechGameResult", 0);
            PlayerPrefs.Save();
        }


        StartCoroutine(ReturnToMainScene(3f));
    }
    IEnumerator RandomClearRoutine()
    {
        while (clearsDone < clearCount && isPlaying)
        {
            // 랜덤 시간 기다림
            float waitTime = Random.Range(minClearDelay, maxClearDelay);
            yield return new WaitForSeconds(waitTime);

            // 게임이 아직 진행 중이면 실행
            if (isPlaying && inputField.text.Length > 0)
            {
                StartCoroutine(PlayShortSound(1f));
                ForceClearInput();
                clearsDone++;
            }
        }
    }

    void ForceClearInput()
    {
        if (inputField.text.Length > 0)
        {
            inputField.text = "";
        }
    }
    IEnumerator PlayShortSound(float duration)
    {
        if (sfxSource != null && clearSound != null)
        {
            sfxSource.clip = clearSound;
            sfxSource.Play();
            yield return new WaitForSeconds(duration);
            sfxSource.Stop();
        }
        
    }
}