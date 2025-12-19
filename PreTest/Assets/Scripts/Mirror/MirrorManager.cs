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

    [Header("지면 설정")]
    public Transform groundA; // 첫 번째 바닥
    public Transform groundB; // 두 번째 바닥 (벽)

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
                    isRotatingMode = false; // 누운 상태일 때는 회전 불가
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

        // Spacebar 키 입력 시 Mirror는 두 Ground 각도의 절반으로 회전
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

    // Ground 간 각도에 따라 Mirror 회전 각도 조정
    private void ToggleMirrorRotationX()
    {
        // 각 지면의 로컬 X 회전값의 절댓값을 가져옴
        float rotA = Mathf.Abs(groundA.localEulerAngles.x);
        float rotB = Mathf.Abs(groundB.localEulerAngles.x);

        // 두 절댓값의 합의 절반을 계산
        float calculatedHalfAngle = (rotA + rotB) / 2f;

        // 3. 현재 거울의 상태 확인 및 토글
        float currentX = selectedMirror.transform.localEulerAngles.x;

        // 유니티 오일러 각도는 360도를 넘어가면 보정되므로 0.1f 오차 범위로 체크
        float targetX = (Mathf.Abs(currentX - calculatedHalfAngle) < 0.1f) ? 0f : calculatedHalfAngle;

        // 4. 회전 적용
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
