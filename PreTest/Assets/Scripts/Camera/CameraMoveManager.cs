using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMoveManager : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 10f;

    [Header("회전 설정")]
    public float lookSensitivity = 0.2f; // 마우스 감도
    public float minPitch = -80f;      // 아래 보기 제한
    public float maxPitch = 80f;       // 위 보기 제한

    private float rotationX = 0f; // 누적 상하 회전 값
    private float rotationY = 0f; // 누적 좌우 회전 값

    void Start()
    {
        // 시작할 때 현재 카메라의 회전 값 초기화
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void Update()
    {
        MoveCameraRelativeToView();
        RotateCamera();
    }

    void MoveCameraRelativeToView()
    {
        Vector3 moveInput = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) moveInput += transform.forward;
        if (Keyboard.current.sKey.isPressed) moveInput -= transform.forward;
        if (Keyboard.current.aKey.isPressed) moveInput -= transform.right;
        if (Keyboard.current.dKey.isPressed) moveInput += transform.right;

        if (moveInput != Vector3.zero)
        {
            transform.position += moveInput.normalized * moveSpeed * Time.deltaTime;
        }
    }

    void RotateCamera()
    {
        // 마우스 오른쪽 버튼으로 회전
        if (Mouse.current.rightButton.isPressed)
        {
            // 마우스 델타 값 가져오기
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // 마우스 움직임에 따른 회전값 계산
            rotationY += mouseDelta.x * lookSensitivity;
            rotationX -= mouseDelta.y * lookSensitivity;
            rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }
}
