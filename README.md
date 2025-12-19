# Deepfine 사전 과제 수행 내용
Edit. 2025-12-19


## 1. 조작 방법

### 전체 조작 내용
Ground에 여러 Mirror를 생성해 레이저가 Receiver에 닿게 해 **Receiver 색상을 빨간색**으로 변경하는 미션 수행

### 카메라
- WASD 방향키를 이용해 확대, 축소, 이동
- 마우스 오른쪽 버튼으로 드래그해 시야 전환

### Mirror
- Ground에서 마우스 왼쪽 버튼 더블 클릭으로 Mirror **생성**
- Mirror를 마우스 왼쪽 버튼으로 한번 클릭하면 금색으로 색상이 바뀌며 선택됨
- Mirror를 선택한 뒤, Backspace를 클릭하면 해당 Mirror가 제거됨
- 선택한 Mirror 가장자리를 드래그해 **회전**
- 선택한 Mirror 중앙을 잡고 드래그해 **이동**
- Mirror를 선택하고 Space bar 클릭하면 두 Ground 각도의 중앙값으로 기울어짐

## 2. 구현한 구조

### 구조 간단 설명
1. CameraMoveManager에서 카메라의 로컬 좌표에 따라 이동, 회전 수행
2. LaserController로 레이저 벡터 리스트 생성 (레이저 발사)
3. MirrorManager에서 마우스 커서로 지정한 Ground 좌표의 normal vector에 Mirror 프리팹 생성
4. Mirror 태그가 붙은 물체와 충돌하면 Vector3.Reflect에 따라 정반사하고 새 레이저 벡터 생성
5. LaserReceiver 컴포넌트가 있는 물체(Receiver)와 레이저가 충돌하면 Activate 메서드 활성화로 Receiver 색상 변경

### CameraMoveManager 상세 설명
1. 게임 플레이 시작 시 카메라 위치와 회전값을 로컬 회전값으로 지정
2. wasd 키를 이용해 카메라가 바라보는 방향을 기준으로 이동
3. 마우스 오른쪽 버튼 클릭 상태로 드래그한 값 계산에 따라 카메라 로컬 회전값 계산
- 컴포넌트 위치: [Hierarchy] MainCamera

### LaserController 상세 설명
1. Ray의 시작점과 끝점 두 laserPoints가 생성할 Laser 벡터를 List<Vector3>로 리스트화
2. 만약 Ray가 Mirror 태그가 붙은 물체와 충돌하면, 충돌한 Ray 벡터 하나 만들고 Vector3.Reflect에 따라 반사 벡터 계산
3. 최대 반사 횟수를 10회로 지정
4. Mirror 충돌지점을 새 Ray 시작점으로 지정해 새 Ray 벡터 생성
5. Mirror 태그가 붙지 않은 물체와 충돌하면, LaserReceiver 컴포넌트가 있는지 확인
6. LaserReceiver 컴포넌트가 있다면 Activate 메서드 활성화하고 Ray 벡터 생성하고 종료
7. LaserReceiver 컴포넌트가 없다면 바로 Ray 벡터 생성하고 종료
8. Ray가 최대 길이(100f)에 이르면 Ray 벡터 생성하고 종료
9. 생성한 벡터를 모두 LineRenderer에 적용
- 컴포넌트 위치: [Hierarchy] Laser > Upper > Muzzle > Line

### MirrorManager 상세 설명
1. 두 Ground에 Ground Layer 추가
2. Mirror 프리팹에 Mirror 태그 추가
3. 마우스 커서에 Raycast를 발사해 Ground Layer과 닿는 좌표 지정
4. 마우스 왼쪽 버튼 더블 클릭 시 해당 좌표의 normal vector에 Mirror 프리팹 생성
5. 새로 생성한 Mirror나 이미 생성된 것 중 하나를 마우스 왼쪽 버튼으로 한번 클릭하면 selectedMirror로 지정
6. selectedMirror에 하이라이트 이벤트 트리거 (색상이 금색으로 변경)
7. Backspace 버튼 클릭 시 selectedMirror 제거
8. rotationZoneWidth 변수 지정으로 Mirror의 가장자리 위치 지정
9. 마우스 왼쪽 버튼으로 가장자리 클릭 후 드래그하면 Mirror의 로컬 Y축을 기준으로 회전
10. 가장자리가 아닌 Mirror의 다른 부위 클릭 후 드래그하면 normal vector 방향을 기준으로 Up 축을 고정한 체 이동
11. 선택한 Mirror에 Space bar 클릭 시, 두 Ground 사이 각도의 1/2 값으로 X 회전값 변경
- 컴포넌트 위치: [Hierarchy] MirrorManager

### LaserReceiver 상세 설명
1. 게임 플레이 시작 시 매 프레임마다 originalColor 색상 유지하는 것 업데이트
2. LaserController 클래스에 의해 Activate 메서드 활성화되는 프레임 동안 activeColor로 색상 변경 업데이트
3. Activate 메서드 비활성화 시 originalColor로 색상 업데이트
- 컴포넌트 위치: [Hierarchy] Receiver > Sphere

## 플레이 성공 모습
<img width="2879" height="1601" alt="Deepfine" src="https://github.com/user-attachments/assets/9f7f3bd4-4433-46aa-bc18-1a7bccf43bae" />

[Youtube 링크 - 동영상 링크로만 공유](https://youtu.be/Ia2_YuT27gE)



