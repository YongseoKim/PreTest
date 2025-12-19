using UnityEngine;
using UnityEngine.EventSystems;

public class MirrorRoationManager : MonoBehaviour
{
    [Header("회전 대상")]
    public Transform targetToRotate; // 회전시킬 Mirror

    [Header("설정")]
    public float sensitivity = 0.2f; // 회전 속도

    // 마우스를 클릭한 상태에서 움직일 때마다 호출됨
    public void OnDrag(PointerEventData eventData)
    {
        if (targetToRotate == null) return;

        // 마우스의 좌우 이동량(delta.x)
        float rotationAmount = eventData.delta.x * sensitivity;

        // Y축을 기준으로 회전
        targetToRotate.Rotate(Vector3.up, -rotationAmount, Space.World);
    }
}
