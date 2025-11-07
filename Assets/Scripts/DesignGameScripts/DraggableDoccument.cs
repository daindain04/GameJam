using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableDocument : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image image;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private Vector3 originalPosition;
    private Transform originalParent;

    private DocumentType documentType;
    private DesignGameManager gameManager;
    private bool isPlaced = false;

    void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 원래 위치 저장
        originalPosition = rectTransform.position;
        originalParent = transform.parent;
    }

    // ★ 변경: sprite 파라미터 제거 (이미 Image에 할당되어 있음)
    public void Initialize(DocumentType type, DesignGameManager manager)
    {
        documentType = type;
        gameManager = manager;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        Debug.Log("드래그 시작: " + gameObject.name);

        // 드래그 중에는 반투명하게
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 부모를 Canvas로 변경 (다른 UI 위에 표시)
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        // 마우스 위치를 따라감
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        Debug.Log("드래그 종료: " + gameObject.name);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Raycast로 드롭 위치 확인
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool foundTarget = false;

        foreach (var result in results)
        {
            Image targetImage = result.gameObject.GetComponent<Image>();

            if (targetImage != null && gameManager != null)
            {
                // 올바른 타겟인지 확인
                if (gameManager.IsCorrectTarget(documentType, targetImage))
                {
                    Debug.Log("정답! " + documentType + " → " + result.gameObject.name);

                    // 정답 처리
                    isPlaced = true;
                    gameManager.OnDocumentPlaced();

                    // 문서 사라짐
                    gameObject.SetActive(false);

                    foundTarget = true;
                    break;
                }
            }
        }

        if (!foundTarget)
        {
            // 잘못된 위치에 드롭 - 원래 위치로 복귀
            Debug.Log("잘못된 위치 - 원래 위치로 복귀");
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}