using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public ArtGameManager gameManager;
    private Button button;
    private Image buttonImage;
    private bool isClicked = false;

    void Start()
    {
        // 컴포넌트 가져오기
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        // GameManager 찾기
        gameManager = FindObjectOfType<ArtGameManager>();

        // 에러 체크
        if (button == null)
        {
            Debug.LogError("Button 컴포넌트가 없습니다: " + gameObject.name);
        }

        if (buttonImage == null)
        {
            Debug.LogError("Image 컴포넌트가 없습니다: " + gameObject.name);
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }

        // 동그라미 초기 투명화 (Alpha = 0)
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 0f;
            buttonImage.color = color;
        }

        // 버튼 클릭 이벤트 연결
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (isClicked) return;

        isClicked = true;

        Debug.Log("정답 버튼 클릭: " + gameObject.name);

        // 동그라미 표시 (Alpha = 1)
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 1f;
            buttonImage.color = color;
        }

        // GameManager에 정답 처리 알림
        if (gameManager != null)
        {
            gameManager.OnCorrectAnswer();
        }

        // 버튼 비활성화
        if (button != null)
        {
            button.interactable = false;
        }
    }
}