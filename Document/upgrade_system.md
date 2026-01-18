# 업그레이드 시스템

## 개요
게임 내 5가지 업그레이드를 관리하는 시스템입니다.

## 업그레이드 종류

### 1. 인벤토리 슬롯 증가
- 기본값: 20 슬롯
- 업그레이드: 추가 슬롯 증가

### 2. 특정 비둘기 확률 증가/감소
- 종별 스폰 가중치 배율 조정
- 1.0 = 기본값, 2.0 = 2배, 0.5 = 절반

### 3. 동시 덫 설치 개수 제한
- 기본값: 제한 없음 (-1)
- 업그레이드: 최대 개수 설정

### 4. 맵당 비둘기 스폰 제한 증가
- 기본값: 20마리
- 업그레이드: 추가 마리 수 증가

## 사용 방법

### GameManager를 통한 업그레이드 적용

```csharp
// 1. 인벤토리 슬롯 10개 증가
GameManager.Instance.UpgradeInventorySlots(10);

// 2. 특정 비둘기 확률 2배로 증가
GameManager.Instance.UpgradeSpeciesSpawnWeight(PigeonSpecies.SP01, 2.0f);

// 3. 특정 비둘기 확률 0.5배로 감소
GameManager.Instance.UpgradeSpeciesSpawnWeight(PigeonSpecies.SP02, 0.5f);

// 4. 동시 덫 설치 개수 5개로 제한
GameManager.Instance.UpgradeMaxTrapCount(5);

// 5. 맵당 비둘기 스폰 제한 10마리 증가
GameManager.Instance.UpgradePigeonsPerMap(10);
```

### 업그레이드 데이터 확인

```csharp
var upgrades = GameManager.Instance.Upgrades;

// 인벤토리 슬롯 보너스 확인
int totalSlots = GameManager.Instance.MaxInventorySlots; // 기본값 + 보너스

// 특정 비둘기 확률 배율 확인
float multiplier = upgrades.GetSpeciesWeightMultiplier(PigeonSpecies.SP01);

// 동시 덫 설치 개수 제한 확인
int maxTraps = upgrades.MaxTrapCount; // -1이면 제한 없음

// 맵당 비둘기 스폰 제한 보너스 확인
int bonus = upgrades.PigeonsPerMapBonus;
```

## 자동 적용

업그레이드는 자동으로 적용됩니다:

- **인벤토리 슬롯**: `GameManager.MaxInventorySlots`에서 자동 계산
- **비둘기 확률**: `WorldPigeonManager`에서 스폰 시 자동 적용
- **덫 설치 제한**: `TrapPlacer`에서 설치 시 자동 확인
- **비둘기 스폰 제한**: `WorldPigeonManager`에서 자동 계산

## 이벤트

업그레이드 변경 시 `OnUpgradeChanged` 이벤트가 발생합니다:

```csharp
GameManager.Instance.OnUpgradeChanged += () => {
    Debug.Log("업그레이드가 변경되었습니다!");
};
```

## 데이터 구조

업그레이드 데이터는 `UpgradeData` 클래스에 저장되며, `GameManager`의 `Upgrades` 프로퍼티를 통해 접근할 수 있습니다.
