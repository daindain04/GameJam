using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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


    [Header("Timer Settings")]
    public float turnTime = 20f;   // 제한 시간 (초)
    public float inPenalty = 1f;
    public float timePenalty = 1f;
    private float currentTime;
    private bool isPlaying = true;

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
            StartCoroutine(HideFeedbackAfterDelay(1f,feedbackText));
        }
        else
        {
            currentTime -= inPenalty;
            feedbackText.text = "incorrect.";
            StartCoroutine(HideFeedbackAfterDelay(1f,feedbackText));
        }
    }

    void TimeOver()
    {
        isPlaying = false;
        feedbackText.text = "time out!";
        inputField.interactable = false;
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
    }



}