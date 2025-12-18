using UnityEngine;

public class LaserController : MonoBehaviour
{
    private LineRenderer line;

    [Header("Laser Settings")]
    public float maxDistance = 100f; // 레이저가 뻗어나갈 최대 거리
    public LayerMask hitLayers;      // 충돌 검사할 레이어 (Wall 등)

    void Start()
    {
        line = GetComponent<LineRenderer>();

        // 레이저 선의 시작점과 끝점 개수 설정
        line.positionCount = 2;

        // Line Renderer의 좌표계를 World가 아닌 Local로 설정하면 관리가 더 편합니다.
        // 하지만 이미 월드 좌표를 사용 중이라면 아래 ShootLaser 함수에서 처리합니다.
        line.useWorldSpace = true;
    }

    void Update()
    {
        UpdateLaser();
    }

    void UpdateLaser()
    {
        // 1. 레이저의 시작점: 현재 Line 오브젝트의 위치
        Vector3 startPos = transform.position;
        line.SetPosition(0, startPos);

        // 2. 레이저의 방향: 현재 Line 오브젝트의 정면(Z축, 파란색 화살표)
        Vector3 direction = transform.forward;

        RaycastHit hit;

        // 3. Physics.Raycast를 이용한 Collider 충돌 판정
        // (시작점, 방향, 충돌정보저장, 최대거리, 검사할 레이어)
        if (Physics.Raycast(startPos, direction, out hit, maxDistance, hitLayers))
        {
            // [충돌 발생] Collider와 부딪힌 정확한 지점(hit.point)까지만 레이저를 그립니다.
            line.SetPosition(1, hit.point);

            // 디버그용: 충돌한 물체 이름을 콘솔에 찍어보고 싶다면 주석 해제하세요.
            // Debug.Log($"레이저가 {hit.collider.name}에 충돌함!");
        }
        else
        {
            // [충돌 없음] 아무것도 부딪히지 않으면 최대 거리만큼 무한히(?) 뻗어나갑니다.
            line.SetPosition(1, startPos + (direction * maxDistance));
        }
    }
}
