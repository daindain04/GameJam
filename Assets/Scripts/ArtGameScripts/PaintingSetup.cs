using UnityEngine;

public class PaintingSetup : MonoBehaviour
{
    [Header("자동 설정")]
    public ArtGameManager gameManager;

    void Start()
    {
        // GameManager 자동 찾기
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<ArtGameManager>();
        }

        // 모든 자식 AnswerButton에 GameManager 연결
        AnswerButton[] buttons = GetComponentsInChildren<AnswerButton>(true);

        foreach (AnswerButton button in buttons)
        {
            if (button.gameManager == null)
            {
                button.gameManager = gameManager;
            }
        }

        Debug.Log(gameObject.name + "에 " + buttons.Length + "개의 버튼이 설정되었습니다.");
    }
}