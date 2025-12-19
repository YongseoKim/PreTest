using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorManager : MonoBehaviour
{
    [Header("프리팹 설정")]
    public GameObject mirrorPrefab;
    public LayerMask groundLayer;
    public float surfaceOffset = 0.01f;

    [Header("조작 설정")]
    public float rotationSensitivity = 0.5f;
    public float doubleClickThreshold = 0.3f;
    public float rotationZoneWidth = 0.3f; // 거울의 가장자리 클릭 시 회전 모드

    [Header("하이라이트")]
    public Color highlightColor = Color.yellow;

    private GameObject selectedMirror;
    private GameObject draggingMirror;
    private Color originalColor;
    private bool isRotatingMode = false;
    private float lastClickTime = -10f;
    private Camera mainCam;

    void Awake() => mainCam = Camera.main;

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();

        if (draggingMirror != null)
            ExecuteDragOrRotate();
    }

    private void HandleMouseInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float timeSinceLast = Time.time - lastClickTime;
            if (timeSinceLast <= doubleClickThreshold)
            {
                SpawnMirror();
                lastClickTime = -10f;
            }
            else
            {
                lastClickTime = Time.time;
                StartInteraction();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingMirror = null;
        }
    }

    private void SpawnMirror()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Mirror의 Vector3.up = hit.normal
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            GameObject newMirror = Instantiate(mirrorPrefab, hit.point + (hit.normal * surfaceOffset), spawnRot);
            SetHighlight(newMirror);
        }
    }

    private void StartInteraction()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObj = FindMirrorParent(hit.collider.gameObject);
            if (hitObj != null)
            {
                draggingMirror = hitObj;
                SetHighlight(draggingMirror);

                // 0도(평행)일 때만 가장자리 클릭 시 회전 모드 작동
                float currentX = draggingMirror.transform.localEulerAngles.x;
                bool isTilted = Mathf.Abs(currentX - 55f) < 0.1f;

                if (isTilted)
                {
                    isRotatingMode = false; // 55도일 때는 회전 불가
                }
                else
                {
                    Vector3 localPoint = draggingMirror.transform.InverseTransformPoint(hit.point);
                    isRotatingMode = Mathf.Abs(localPoint.x) > rotationZoneWidth;
                }
            }
            else
            {
                ClearHighlight();
            }
        }
    }

    private void ExecuteDragOrRotate()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // groundLayer(바닥/벽)에 부딪힌 정보가 있을 때만 처리
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (isRotatingMode)
            {
                // [회전] 로컬 Y축(지면 수직축) 기준 회전
                float mouseX = Mouse.current.delta.x.ReadValue();
                draggingMirror.transform.Rotate(Vector3.up * -mouseX * rotationSensitivity, Space.Self);
            }
            else
            {
                // 드래그로 Mirror 위치 이동
                draggingMirror.transform.position = hit.point + (hit.normal * surfaceOffset);

                // 수평 유지: 현재 지면의 법선(hit.normal) 방향으로 Mirror의 Up축을 강제 고정
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // 기존 회전값 유지: 이동 중 거울 회전 방지를 위해 Y축만 합침
                float currentYRot = draggingMirror.transform.localEulerAngles.y;
                draggingMirror.transform.rotation = surfaceRotation * Quaternion.Euler(0, currentYRot, 0);
            }
        }
    }

    private void HandleKeyboardInput()
    {
        if (selectedMirror == null) return;

        // Spacebar 키 입력 시 Mirror 55도 회전
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleMirrorRotationX();
        }

        // Backspace 키 입력 시 하이라이트된 Mirror 제거
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            Destroy(selectedMirror);
            selectedMirror = null;
        }
    }

    private void ToggleMirrorRotationX()
    {
        // 현재 로컬 X 회전값을 확인 (0~360 범위 고려)
        float currentX = selectedMirror.transform.localEulerAngles.x;
        // 55도 근처라면 0으로, 아니면 55로 토글
        float targetX = Mathf.Abs(currentX - 55f) < 0.1f ? 0f : 55f;

        Vector3 currentRot = selectedMirror.transform.localEulerAngles;
        selectedMirror.transform.localRotation = Quaternion.Euler(targetX, currentRot.y, currentRot.z);
    }

    // 부모 오브젝트 중 Mirror가 포함된 객체 찾기
    private GameObject FindMirrorParent(GameObject child)
    {
        Transform curr = child.transform;
        while (curr != null)
        {
            if (curr.name.Contains("Mirror")) return curr.gameObject;
            curr = curr.parent;
        }
        return null;
    }

    private void SetHighlight(GameObject target)
    {
        if (selectedMirror != null) ClearHighlight();
        selectedMirror = target;
        Renderer r = selectedMirror.GetComponentInChildren<Renderer>();
        if (r != null) { originalColor = r.material.color; r.material.color = highlightColor; }
    }

    private void ClearHighlight()
    {
        if (selectedMirror == null) return;
        Renderer r = selectedMirror.GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = originalColor;
        selectedMirror = null;
    }
}
