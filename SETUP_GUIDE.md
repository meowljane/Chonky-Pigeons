# 씬 설정 가이드

## 1. GameDataRegistry 설정

1. **씬에 빈 GameObject 생성**
   - 이름: `GameDataRegistry`
   - `GameDataRegistry` 컴포넌트 추가

2. **ScriptableObject 에셋 할당**
   - Project 창에서 `Assets/GameData/Generated/` 폴더로 이동
   - 다음 에셋들을 GameDataRegistry의 Inspector에 드래그:
     - `AIProfiles.asset` → AI Profiles
     - `Faces.asset` → Faces
     - `SpeciesSet.asset` → Species Set
     - `TerrainTypes.asset` → Terrain Types
     - `Traps.asset` → Traps

## 2. 플레이어 설정

1. **플레이어 GameObject 생성**
   - 이름: `Player`
   - SpriteRenderer 추가 (임시 스프라이트 할당)
   - `Rigidbody2D` 추가
     - Gravity Scale: 0
     - Drag: 10
   - `PlayerController` 컴포넌트 추가
   - `CircleCollider2D` 추가 (비둘기 감지용)

## 3. 덫 설치 시스템

1. **TrapPlacer GameObject 생성**
   - 이름: `TrapPlacer`
   - `TrapPlacer` 컴포넌트 추가

2. **덫 프리팹 생성**
   - 빈 GameObject 생성 → 이름: `FoodTrap`
   - `SpriteRenderer` 추가 (덫 스프라이트)
   - `CircleCollider2D` 추가 (Is Trigger 체크)
   - `FoodTrap` 컴포넌트 추가
   - TrapId 필드에 `BREAD` 입력
   - 이 GameObject를 프리팹으로 저장: `Assets/Prefabs/FoodTrap.prefab`
   - TrapPlacer의 Trap Prefab에 할당

## 4. 비둘기 스폰 시스템

1. **PigeonSpawner GameObject 생성**
   - 이름: `PigeonSpawner`
   - `PigeonSpawner` 컴포넌트 추가

2. **비둘기 프리팹 생성**
   - 빈 GameObject 생성 → 이름: `Pigeon`
   - `SpriteRenderer` 추가
   - `CircleCollider2D` 추가
   - `Rigidbody2D` 추가 (Gravity Scale: 0, Linear Damping: 5)
   - `PigeonAI` 컴포넌트 추가
   - `PigeonController` 컴포넌트 추가
   - **`PigeonMovement` 컴포넌트 추가** ⚠️ 중요!
   - 이 GameObject를 프리팹으로 저장: `Assets/Prefabs/Pigeon.prefab`
   - PigeonSpawner의 Pigeon Prefab에 할당
   - TrapPlacer의 Spawner에 PigeonSpawner 할당

## 5. 카메라 설정

- Main Camera가 있어야 함 (기본 씬에 있음)
- TrapPlacer가 자동으로 찾음

## 6. 테스트

1. **Play 버튼 클릭**
2. **마우스 클릭** → 덫 설치 + 비둘기 스폰
3. **비둘기들이 자동으로 먹이를 먹음**
4. **먹이가 0이 되면 포획!** (콘솔에 로그 출력)

## 문제 해결

- **비둘기가 이동 안 함**: Pigeon 프리팹에 `PigeonMovement` 컴포넌트가 있는지 확인
- **비둘기가 안 먹음**: FoodTrap의 Detection Radius 확인
- **에러 발생**: GameDataRegistry에 모든 SO 에셋이 할당되었는지 확인
- **비둘기가 안 보임**: Pigeon 프리팹의 SpriteRenderer에 스프라이트 할당


