# 미친개뚱뚱한비둘기

Unity로 만드는 비둘기 수집 게임 (WebGL 타겟)

## 프로젝트 구조

```
Assets/
  GameData/
    SourceJson/          # 원본 JSON (Cursor에서 편집)
      pigeon_ai_profiles.json
      pigeon_faces.json
      pigeon_species.json
      world_locations.json
      trap_types.json
    
    Generated/           # 생성된 ScriptableObject 에셋
      (Unity에서 Import 후 생성됨)
  
  Scripts/
    Data/
      Importers/         # JSON → SO 변환
      Runtime/           # 런타임 데이터 클래스
  
    Gameplay/            # 게임플레이 로직
  
  Art/
    Pigeons/            # 비둘기 스프라이트
    UI/                 # UI 스프라이트
```

## 개발 워크플로우

1. **Cursor에서 JSON 수정** (`Assets/GameData/SourceJson/*.json`)
2. **Cursor에서 C# 수정** (Importer/Runtime 코드)
3. **Unity에서 Import**: `Tools > Game Data > Import All` 클릭
4. **플레이 테스트**

## 필수 패키지

- **Newtonsoft.Json** (JSON 파싱용)
  - Unity Package Manager에서 추가 필요
  - 또는 `com.unity.nuget.newtonsoft-json` 패키지

## 주요 시스템

### 데이터 관리
- JSON (원본) → ScriptableObject (런타임) 변환
- 스프라이트는 SO에 직접 할당 (Resources.Load 사용 안 함)

### 비둘기 시스템
- **종(Species)**: 희귀도(Rarity Tier) 결정
- **비만도(Obesity)**: BitePower와 비례 (1~5)
- **얼굴(Face)**: 가격 보너스만

### AI 시스템
- **Alert(스트레스)** 시스템
  - 플레이어 접근 + 군집 밀도로 증가
  - Alert 수준에 따라 행동 변화
  - BackOff 이상이면 먹이 경쟁 참여 안 함

### 포획 시스템
- 먹이의 `feedAmount`가 20에서 시작
- 비둘기가 EatTick 시 `feedAmount -= bitePower`
- `feedAmount <= 0`이 되는 순간 그 비둘기 포획

## 다음 단계

1. Unity Package Manager에서 Newtonsoft.Json 추가
2. Unity에서 `Tools > Game Data > Import All` 실행
3. GameDataRegistry를 씬에 배치하고 SO 에셋들 할당
4. 비둘기 스프라이트를 Species SO에 할당
5. 게임플레이 로직 구현 (이동, 덫 설치, 비둘기 스폰 등)


