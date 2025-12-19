using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject mirrorPrefab;
    public float yOffset = 0.5f;
    public LayerMask groundLayer;

    [Header("조작 설정")]
    [Tooltip("거울 중심에서 이 거리보다 멀리 클릭하면 회전 모드가 됩니다 (0.1 ~ 0.5 권장)")]
    public float rotationThreshold = 0.2f;
    [Tooltip("더블 클릭으로 인정될 최대 시간 (초)")]
    public float doubleClickThreshold = 2.0f;

    private GameObject draggingMirror;
    private bool isRotating = false;
    private Camera mainCam;

    // 더블 클릭 체크를 위한 변수
    private float lastClickTime = -10f;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 왼쪽 마우스 클릭 감지
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float currentTime = Time.time;

            // 1. 더블 클릭 체크 (이전 클릭으로부터 2초 이내인지)
            if (currentTime - lastClickTime <= doubleClickThreshold)
            {
                // 더블 클릭 성공: 생성 로직 실행
                SpawnMirror();

                // 더블 클릭 후 바로 드래그가 시작되지 않도록 처리하거나, 
                // 생성된 직후에 조작하고 싶다면 StartDragging()을 호출하지 않습니다.
                lastClickTime = -10f; // 더블 클릭 처리 후 시간 초기화 (연속 3번 클릭 방지)
            }
            else
            {
                // 2. 단일 클릭: 드래그/회전 시작 및 시간 기록
                lastClickTime = currentTime;
                StartDragging();
            }
        }

        // 3. 왼쪽 마우스 버튼을 떼면 드래그 중지
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingMirror = null;
            isRotating = false;
        }

        // 4. 드래그 중일 때 동작 수행
        if (draggingMirror != null)
        {
            ContinueDragging();
        }
    }

    void SpawnMirror()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Ground 레이어나 이름을 확인하여 생성
            if (hit.collider.gameObject.name.Contains("Ground"))
            {
                Vector3 spawnPos = hit.point;
                spawnPos.y += yOffset;

                Quaternion spawnRotation = Quaternion.identity;

                // 특정 구역 내 자동 회전 조건 유지
                if (spawnPos.x >= 6f && spawnPos.x <= 9f && spawnPos.z >= -9f && spawnPos.z <= -6f)
                {
                    spawnRotation = Quaternion.Euler(0, -45f, 0);
                }

                Instantiate(mirrorPrefab, spawnPos, spawnRotation);
            }
        }
    }

    void StartDragging()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Mirror") || hit.collider.name.Contains("Mirror"))
            {
                draggingMirror = hit.collider.gameObject;
                Vector3 localHitPoint = draggingMirror.transform.InverseTransformPoint(hit.point);

                if (Mathf.Abs(localHitPoint.x) > rotationThreshold)
                {
                    isRotating = true;
                }
                else
                {
                    isRotating = false;
                }
            }
        }
    }

    void ContinueDragging()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (isRotating)
            {
                Vector3 direction = hit.point - draggingMirror.transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    draggingMirror.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
            else if (hit.collider.gameObject.name.Contains("Ground"))
            {
                Vector3 newPos = hit.point;
                newPos.y += yOffset;
                draggingMirror.transform.position = newPos;
            }
        }
    }
}
