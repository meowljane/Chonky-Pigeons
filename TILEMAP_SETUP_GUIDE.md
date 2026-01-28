# 타일맵 레이어 구조 설정 가이드

## 개요

이 프로젝트는 3개의 타일맵 레이어를 사용하여 맵 범위를 관리합니다:
1. **MapLayer**: 맵 범위 (플레이어 인식, 비둘기 존재, 덫 설치 인식 범위 통합)
2. **TerrainLayer**: 지형 타입 (GRASS, SAND, WATER 등)
3. **PlayerMovementLayer**: 플레이어 이동 가능 범위 (맵 + 다리 포함)

## Unity 씬에서 타일맵 레이어 생성 방법

### 1. 기본 타일맵 그리드 생성

1. Hierarchy 창에서 우클릭
2. **2D Object** → **Tilemap** → **Rectangular** 선택
3. 자동으로 `Grid` GameObject와 `Tilemap` GameObject가 생성됩니다

### 2. 레이어 구조 설정

다음과 같은 구조로 타일맵을 생성하세요:

```
Grid (Grid 컴포넌트)
├── MapLayer (Tilemap 컴포넌트)
│   └── MapLayer (TilemapRenderer 컴포넌트)
├── TerrainLayer (Tilemap 컴포넌트)
│   └── TerrainLayer (TilemapRenderer 컴포넌트)
└── PlayerMovementLayer (Tilemap 컴포넌트)
    └── PlayerMovementLayer (TilemapRenderer 컴포넌트)
```

### 3. 각 레이어별 설정

#### MapLayer (맵 범위)
- **용도**: 플레이어 인식 범위, 비둘기 존재 범위, 덫 설치 인식 범위를 모두 통합
- **타일 배치**: 모든 맵 영역에 타일을 배치
- **렌더링**: 게임 플레이 중 보이지 않아도 됨 (Sorting Layer를 낮게 설정하거나 비활성화 가능)
- **타일 에셋**: 단색 타일이나 투명 타일 사용 가능

#### TerrainLayer (지형 타입)
- **용도**: 각 위치의 지형 타입 정의 (GRASS, SAND, WATER 등)
- **타일 배치**: 지형에 맞는 타일을 배치
- **타일 이름 규칙**: 
  - Grass 타일: 이름에 "grass" 포함
  - Sand 타일: 이름에 "sand" 포함
  - Water 타일: 이름에 "water" 포함
- **렌더링**: 게임 플레이 중 보이지 않아도 됨 (데이터용)

#### PlayerMovementLayer (플레이어 이동 범위)
- **용도**: 플레이어가 이동 가능한 모든 영역 (맵 + 다리 포함)
- **타일 배치**: 
  - 모든 맵 영역에 타일 배치
  - 모든 다리 영역에도 타일 배치
- **렌더링**: 게임 플레이 중 보이지 않아도 됨 (데이터용)

### 4. 타일맵 컴포넌트 설정

각 Tilemap GameObject에 다음 설정을 권장합니다:

#### Tilemap 컴포넌트
- **Cell Size**: (1, 1, 0) - 기본값 유지
- **Tile Anchor**: (0.5, 0.5, 0) - 타일 중심점

#### TilemapRenderer 컴포넌트
- **Mode**: Chunk (기본값)
- **Sorting Layer**: 적절한 레이어 선택 (보이지 않게 하려면 낮은 레이어 사용)
- **Order in Layer**: 각 레이어별로 구분

### 5. TilemapRangeManager 설정

1. 빈 GameObject 생성 (예: "TilemapRangeManager")
2. `TilemapRangeManager` 컴포넌트 추가
3. Inspector에서 다음을 설정:
   - **Map Layer**: MapLayer 타일맵 할당
   - **Terrain Layer**: TerrainLayer 타일맵 할당
   - **Player Movement Layer**: PlayerMovementLayer 타일맵 할당
   - **Map Tile**: MapLayer에 사용할 타일 에셋 할당 (선택사항)
   - **Player Movement Tile**: PlayerMovementLayer에 사용할 타일 에셋 할당 (선택사항)

## 기존 콜라이더를 타일맵으로 변환하는 방법

### 방법 1: 수동 변환

1. 기존 맵 콜라이더의 Bounds 확인
2. 해당 영역에 타일을 배치
3. 콜라이더 내부인지 확인하며 타일 배치

### 방법 2: 에디터 스크립트 사용 (추천)

`TilemapRangeManager`의 `FillTilemapFromCollider` 메서드를 사용하여 콜라이더를 타일맵으로 변환할 수 있습니다.

**에디터 스크립트 예시:**
```csharp
// 에디터 스크립트에서 실행
using UnityEngine;
using UnityEditor;
using PigeonGame.Gameplay;
using UnityEngine.Tilemaps;

public class ConvertCollidersToTilemap : EditorWindow
{
    [MenuItem("Tools/Convert Colliders to Tilemap")]
    static void Convert()
    {
        var mapManager = FindFirstObjectByType<MapManager>();
        var tilemapManager = FindFirstObjectByType<TilemapRangeManager>();
        
        if (mapManager == null || tilemapManager == null)
        {
            Debug.LogError("MapManager 또는 TilemapRangeManager를 찾을 수 없습니다!");
            return;
        }
        
        // MapLayer 타일맵 찾기
        var mapLayer = GameObject.Find("MapLayer")?.GetComponent<Tilemap>();
        var playerMovementLayer = GameObject.Find("PlayerMovementLayer")?.GetComponent<Tilemap>();
        
        if (mapLayer == null || playerMovementLayer == null)
        {
            Debug.LogError("타일맵 레이어를 찾을 수 없습니다!");
            return;
        }
        
        // 타일 에셋 가져오기 (Inspector에서 설정한 것 사용)
        var mapTile = Resources.Load<TileBase>("Tiles/MapTile"); // 경로는 프로젝트에 맞게 수정
        var movementTile = Resources.Load<TileBase>("Tiles/MovementTile");
        
        // 모든 맵 콜라이더를 MapLayer에 그리기
        var mapColliders = mapManager.GetAllMapColliders();
        if (mapColliders != null && mapTile != null)
        {
            foreach (var collider in mapColliders)
            {
                tilemapManager.FillTilemapFromCollider(collider, mapLayer, mapTile);
            }
        }
        
        // 모든 맵 콜라이더를 PlayerMovementLayer에도 그리기
        if (mapColliders != null && movementTile != null)
        {
            foreach (var collider in mapColliders)
            {
                tilemapManager.FillTilemapFromCollider(collider, playerMovementLayer, movementTile);
            }
        }
        
        // 모든 다리 콜라이더를 PlayerMovementLayer에 그리기
        var bridgeColliders = mapManager.GetAllBridgeColliders();
        if (bridgeColliders != null && movementTile != null)
        {
            foreach (var collider in bridgeColliders)
            {
                tilemapManager.FillTilemapFromCollider(collider, playerMovementLayer, movementTile);
            }
        }
        
        Debug.Log("콜라이더를 타일맵으로 변환 완료!");
    }
}
```

또는 Unity 에디터에서 직접:
1. Play 모드에서 `TilemapRangeManager` 컴포넌트의 `FillTilemapFromCollider` 메서드를 호출
2. 또는 에디터 스크립트를 작성하여 위 예시처럼 사용

## 레이어별 타일 배치 전략

### MapLayer
- 모든 맵 영역을 완전히 채우기
- 맵 경계를 정확하게 따라가기
- 타일이 없는 곳은 맵 범위 밖으로 인식됨

### TerrainLayer
- 각 지형 타입에 맞는 타일 배치
- 타일 이름으로 지형 타입이 자동 인식됨
- 타일이 없으면 기본값(SAND) 사용

### PlayerMovementLayer
- MapLayer의 모든 영역 + 다리 영역
- 다리 영역도 포함하여 배치
- 플레이어가 이동 가능한 모든 영역을 포함

## 주의사항

1. **타일 크기**: 모든 레이어는 동일한 Grid를 사용해야 합니다
2. **좌표 정렬**: 타일맵의 셀 좌표와 월드 좌표가 정확히 맞아야 합니다
3. **성능**: 타일맵이 너무 크면 성능에 영향을 줄 수 있으므로 필요한 영역만 채우세요
4. **하위 호환성**: 타일맵이 없으면 기존 콜라이더 시스템으로 자동 전환됩니다

## 디버깅 팁

1. **타일맵 시각화**: TilemapRenderer를 활성화하여 타일 배치 확인
2. **Gizmo 사용**: `TilemapRangeManager`에 Gizmo를 추가하여 범위 확인 가능
3. **에디터 모드 테스트**: Play 모드에서 타일맵 기반 체크가 제대로 작동하는지 확인

## 다음 단계

1. 씬에 타일맵 레이어 3개 생성
2. 기존 콜라이더 영역을 타일맵에 타일로 그리기
3. TilemapRangeManager 컴포넌트 생성 및 설정
4. 게임 실행하여 타일맵 기반 범위 체크 확인
