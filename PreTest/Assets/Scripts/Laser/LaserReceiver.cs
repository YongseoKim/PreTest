using UnityEngine;

public class LaserReceiver : MonoBehaviour
{
    public Color activeColor = Color.red;
    private Color originalColor;
    private Renderer targetRenderer;
    private bool isHitThisFrame = false;

    void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            originalColor = targetRenderer.material.color;
        }
    }

    // 레이저가 닿았을 때 매 프레임 호출될 메서드
    public void Activate()
    {
        isHitThisFrame = true;
        if (targetRenderer != null)
        {
            targetRenderer.material.color = activeColor;
        }
    }

    void LateUpdate()
    {
        // 이번 프레임에 레이저가 닿지 않았다면 원래 색상으로 복구
        if (!isHitThisFrame)
        {
            if (targetRenderer != null)
            {
                targetRenderer.material.color = originalColor;
            }
        }
        isHitThisFrame = false; // 매 프레임 초기화
    }
}
