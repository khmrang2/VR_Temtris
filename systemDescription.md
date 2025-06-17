# VR Temtris 시스템 구성
###### 더 자세한 건 주석 참고
---
## 1. 인터페이스 구성

### • 시작 인터페이스

* `FixedGameMenuManager.cs`, `HierarchicalMenuController.cs`
* `Panel`, `Button` 프리팹으로 고정형 메뉴 구성

### • 메뉴 인터페이스 (시선 기반)

* `GameMenuManager.cs`
* 인터페이스가 플레이어의 시선에 따라 위치 조정

### • 오브젝트 인터페이스

* `ItemInfoHolder.cs`: 오브젝트 설명 첨부
* `ShowInfoOnGrab.cs`: 플레이어 Grab 시 `ItemInfoUI`를 보여줌
* 지정된 버튼 입력으로 인터페이스 표시/숨김 전환

---

## 2. 씬 및 맵 시스템

### • 맵 변경

* `MapSelectionManager.cs`: 맵 선택 인터페이스
* `StageNode.cs`: 맵, 난이도를 종합
* `SceneLoaderSO.cs`: StageNode에서 종합된 씬 로드

### • 스테이지 설정

* `StageDataSO`: 난이도 설정 (박스 생성 간격, 랜덤 시드, 제한 시간, 목표 점수 등)

### • 페이드 인/아웃

* `ScreenFader.cs`: Fade 이미지 및 지속 시간 설정, 씬 변경 호출 시 페이드 아웃 -> 씬 로드 후 페이드 인

---

## 3. 렌더링 및 카메라

### • 보드 렌더 카메라

* `BoardCam`: 테트리스 보드를 바라보는 카메라
* `BoardCam.cs`: padding, FOV 자동 조정
* `BoardCam.Texture`: CCTV 오브젝트에서 화면 출력

### • 점유율 시각화

* `Trigger.cs`: 라인별 점유율 계산
* `Portion` 큐브를 사용하여 크기 + 색상으로 시각화
* 보드 렌더 카메라에 함께 렌더링

---

## 4. UI 및 입력 시스템

### • UI

* `HandUIManager.cs`: XR Rig 내부 부착
* XR Interaction Toolkit의 기본 입력 버튼으로 카메라 이동

### • 키 바인딩

* `showButton`: XRI Input System의 Secondary 버튼 (Y, B)
* 버튼 누르면 손에 UI 표시/해제

---

## 5. 주요 시스템 구성

### • Object Pooling

* `ObjectPoolManager.cs`: 상자, 손, 이펙트 풀 생성
* `ObjectSpawnManager.cs`: 기계 손 생성 위치, 아이템 확률 설정, 7-BAG 알고리즘 기반 상자 블록 설정, 설정된 간격으로 기계 손 생성

### • 기계 손 시스템

* `Gripper.cs`, `PathFollower.cs`: 박스 집기 및 WayPoint 경로 이동
* 보드 위 무작위 위치에서 상자를 강제로 오픈

### • 상자 시스템

* `BoxOpen.cs`: 블록/아이템 프리팹 리스트 및 생성 위치 설정
* 플레이어가 Grab 이후 Trigger로 오픈
* 랜덤한 블록 + 확률적 아이템 드랍
* 기계 손이 강제 오픈 시, 랜덤 회전 상태로 블록 드랍

### • 절단 시스템

* `LineTrigger.cs`: 블록 정지 여부 및 점유율 계산
* `EdgeCuttingManager.cs`: 절단 or 삭제 판단
* `EdgeCuttingHelper.cs`: 절단 실행
* X, Z 축 기준 점유 면적 계산

### • 점수 시스템

* `ScoreManager.cs`: 절단 성공 시 점수 누적
* `GetScore.cs`: UI에 실시간 점수 반영

---

## 6. 아이템 시스템

### • 아이템 ‘풀’

* `AttachableBlock.cs`: 블록 충돌 감지 및 이펙트 생성
* `BlockAttacher.cs`: 블록 상태 변경
* `BlockConnectHelper.cs`: 올바른 면 탐지 후 블록 결합
* 접착 가능한 블록은 반짝이 이펙트로 표시

### • 아이템 ‘지우개’

* `BlockRemover.cs`: 충돌 시 블록 삭제
* 접착된 블록일 경우 하나만 삭제

### • 아이템 ‘칼’ (절단 나이프)

* `VelocityEstimator.cs`: VR 손의 속도 및 각속도 측정
* `Cutting_knife.cs`: 방향 벡터로 절단 평면 계산
* 외부 라이브러리 `EzySlice`를 사용해 위/아래로 절단 → 아래쪽은 제거
* VR에서 칼 휘둘러 직접 절단 가능

