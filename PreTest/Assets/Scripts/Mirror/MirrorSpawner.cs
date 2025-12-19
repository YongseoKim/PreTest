using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject mirrorPrefab;
    public LayerMask groundLayer;
    private float surfaceOffset = 0.005f;

    [Header("조작 설정")]
    public float rotationThreshold = 0.2f;
    public float doubleClickThreshold = 0.3f; // 0.3초 내에 두 번 클릭 시 더블 클릭

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;
    private GameObject lastSelectedMirror;
    private Color originalColor;

    private GameObject draggingMirror;
    private bool isRotating = false;
    private Camera mainCam;
    private float lastClickTime = -10f;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 시
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float currentTime = Time.time;

            // [더블 클릭 판정]
            if (currentTime - lastClickTime <= doubleClickThreshold)
            {
                // 더블 클릭 시: 거울 생성 시도
                SpawnMirrorAtMouse();
                lastClickTime = -10f; // 더블 클릭 후 타이머 초기화 (연속 생성 방지)
            }
            else
            {
                // 단일 클릭 시: 타이머 기록 및 드래그/선택 시도
                lastClickTime = currentTime;
                StartDragging();
            }
        }

        // 클릭 해제 시
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingMirror = null;
            isRotating = false;
        }

        // 드래그 중인 경우
        if (draggingMirror != null)
        {
            ContinueDragging();
        }

        // 단축키 로직
        HandleKeyboardInputs();
    }

    void HandleKeyboardInputs()
    {
        // Space: X축 토글
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            RotateSelectedMirrorX();

        // Backspace: 제거
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            DeleteSelectedMirror();
    }

    // [더블 클릭 시 호출되는 생성 로직]
    void SpawnMirrorAtMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // groundLayer 레이어를 가진 바닥에 부딪혔을 때만 생성
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 위치: 바닥 좌표 + 법선 방향 오프셋
            Vector3 spawnPos = hit.point + (hit.normal * surfaceOffset);

            // 회전: 바닥 기울기에 수직으로 정렬
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            GameObject newMirror = Instantiate(mirrorPrefab, spawnPos, spawnRotation);

            // 생성 직후 해당 거울을 바로 선택(하이라이트) 상태로 만듭니다.
            SetHighlight(newMirror);

            Debug.Log("더블 클릭으로 거울이 생성되었습니다.");
        }
    }

    void StartDragging()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 거울(부모 혹은 자식) 클릭 여부 확인
            Transform targetTransform = hit.collider.transform;
            while (targetTransform != null && !targetTransform.name.Contains("Mirror"))
            {
                targetTransform = targetTransform.parent;
            }

            if (targetTransform != null)
            {
                draggingMirror = targetTransform.gameObject;
                SetHighlight(draggingMirror);

                // 로컬 좌표 기준 회전/이동 판정
                Vector3 localHitPoint = draggingMirror.transform.InverseTransformPoint(hit.point);
                isRotating = Mathf.Abs(localHitPoint.x) > rotationThreshold;
            }
            else
            {
                // 빈 공간 클릭 시 하이라이트 해제
                ClearHighlight();
            }
        }
    }

    void ContinueDragging()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (isRotating)
            {
                // [회전] 바닥 기울기를 유지하며 마우스 방향 바라보기
                Vector3 direction = hit.point - draggingMirror.transform.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    draggingMirror.transform.rotation = Quaternion.LookRotation(direction, hit.normal);
                }
            }
            else
            {
                // [이동] 바닥 기울기 및 기존 사용자가 설정한 Y축 회전값 유지
                draggingMirror.transform.position = hit.point + (hit.normal * surfaceOffset);

                float currentYRotation = draggingMirror.transform.localEulerAngles.y;
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                draggingMirror.transform.rotation = surfaceRotation * Quaternion.Euler(0, currentYRotation, 0);
            }
        }
    }

    void SetHighlight(GameObject target)
    {
        if (lastSelectedMirror != null && lastSelectedMirror != target)
            ClearHighlight();

        if (lastSelectedMirror != target)
        {
            lastSelectedMirror = target;
            Renderer renderer = lastSelectedMirror.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                originalColor = renderer.material.color;
                renderer.material.color = highlightColor;
            }
        }
    }

    void ClearHighlight()
    {
        if (lastSelectedMirror != null)
        {
            Renderer renderer = lastSelectedMirror.GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material.color = originalColor;
            lastSelectedMirror = null;
        }
    }

    void DeleteSelectedMirror()
    {
        if (lastSelectedMirror != null)
        {
            if (draggingMirror == lastSelectedMirror) draggingMirror = null;
            GameObject toDestroy = lastSelectedMirror;
            lastSelectedMirror = null;
            Destroy(toDestroy);
        }
    }

    void RotateSelectedMirrorX()
    {
        if (lastSelectedMirror != null)
        {
            Vector3 currentRotation = lastSelectedMirror.transform.localEulerAngles;
            float targetX = Mathf.Abs(currentRotation.x - 55f) < 0.1f ? 0f : 55f;
            lastSelectedMirror.transform.localRotation = Quaternion.Euler(targetX, currentRotation.y, currentRotation.z);
        }
    }
}
