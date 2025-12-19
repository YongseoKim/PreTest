using UnityEngine;
using System.Collections.Generic;

public class LaserController : MonoBehaviour
{
    private LineRenderer line;

    [Header("Laser Settings")]
    public float maxDistance = 100f;
    public LayerMask hitLayers;
    public int maxReflections = 10;   // 최대 반사 횟수

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    void Update()
    {
        UpdateLaser();
    }

    void UpdateLaser()
    {
        // 레이저의 지점들을 저장할 리스트
        List<Vector3> laserPoints = new List<Vector3>();

        Vector3 currentPos = transform.position; // 시작 위치
        Vector3 currentDir = transform.forward;  // 시작 방향

        laserPoints.Add(currentPos);

        for (int i = 0; i < maxReflections; i++)
        {
            Ray ray = new Ray(currentPos, currentDir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, hitLayers))
            {
                // 충돌 지점 추가
                laserPoints.Add(hit.point);

                // 1. 만약 부딪힌 물체의 태그가 "Mirror"라면 정반사 수행
                if (hit.collider.CompareTag("Mirror"))
                {
                    // Vector3.Reflect를 사용하여 반사 벡터 계산
                    currentDir = Vector3.Reflect(currentDir, hit.normal);
                    // 다음 레이의 시작점은 현재 충돌 지점
                    currentPos = hit.point + currentDir * 0.01f;
                }
                else
                {
                    // 부딪힌 물체에 LaserReceiver 컴포넌트가 있는지 확인
                    LaserReceiver receiver = hit.collider.GetComponent<LaserReceiver>();
                    if (receiver != null)
                    {
                        receiver.Activate(); // Receiver 빨간색으로 변경
                    }

                    // Mirror가 아니면 여기서 레이저 종료
                    break;
                }
            }
            else
            {
                // 아무것도 부딪히지 않으면 최대 거리까지 선 긋고 종료
                laserPoints.Add(currentPos + (currentDir * maxDistance));
                break;
            }
        }

        // LineRenderer에 계산된 모든 지점 적용
        line.positionCount = laserPoints.Count;
        line.SetPositions(laserPoints.ToArray());
    }
}
