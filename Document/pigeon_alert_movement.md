# 비둘기 Alert 및 이동 시스템

## 개요

비둘기는 `alert` 값에 따라 4가지 상태로 동작합니다:
- **Normal**: 평상시 상태, 자유롭게 이동하며 덫을 찾음
- **Cautious**: 경고 상태, 플레이어나 군집으로 인한 스트레스 증가
- **BackOff**: 후퇴 상태, 플레이어나 군집에서 멀어짐
- **Flee**: 도망 상태, 플레이어에서 최대한 멀어짐

## Alert 시스템

### Alert 증가 조건

1. **플레이어 접근**
   - 플레이어가 `detectionRadius` 내에 있으면 alert 증가
   - 거리에 반비례하여 증가 (가까울수록 더 많이 증가)
   - 공식: `alert += playerAlertPerSec * alertWeight * deltaTime * distanceFactor`
   - `distanceFactor = 1 - (거리 / detectionRadius)`

2. **먹이 경쟁**
   - 덫 근처에서 먹이를 먹으려는 비둘기들끼리만 alert 증가
   - 경쟁자 수만큼 증가
   - 공식: `alert += crowdAlertPerNeighborPerSec * alertWeight * 경쟁자수 * deltaTime`

### Alert 감소

- 매 프레임마다 자동으로 감소
- 공식: `alert = max(0, alert - alertDecayPerSec * deltaTime)`
- **Flee 상태일 때는 alert가 변경되지 않음** (증가/감소 모두 안 함)

### Alert 임계값

모든 비둘기가 동일한 임계값을 사용 (PigeonMovement에서 설정):
- `warnThreshold` (기본값: 45): Cautious 상태 시작
- `backoffThreshold` (기본값: 80): BackOff 상태 시작
- `fleeThreshold` (기본값: 100): Flee 상태 시작

### Alert 가중치

- `alertWeight` (기본값: 1.0): 모든 alert 증가에 적용되는 전역 가중치
- PigeonMovement에서 Inspector로 조절 가능

## 상태 전환

```
Normal (alert < 45)
  ↓
Cautious (45 ≤ alert < 80)
  ↓
BackOff (80 ≤ alert < 100)
  ↓
Flee (alert ≥ 100)
```

**중요**: Flee 상태가 되면 alert가 더 이상 변경되지 않습니다. 상태가 유지됩니다.

## 이동 행동

### Normal 상태
- 덫을 찾아 이동하거나 랜덤하게 배회
- 속도: `wanderSpeed` (기본값: 2)

### Cautious 상태
- Normal과 동일하게 이동하지만 먹이 먹기 확률/간격이 변경됨
- 속도: `wanderSpeed`

### BackOff 상태
- 플레이어나 군집에서 멀어지는 방향으로 이동
- 속도: `backoffSpeed` (기본값: 3)
- 플레이어가 가까이 있으면 무조건 BackOff (alert와 관계없이)

### Flee 상태
- 플레이어에서 최대한 멀어지는 방향으로 이동
- 속도: `fleeSpeed` (기본값: 4)
- alert가 변경되지 않으므로 상태가 유지됨

## 강제 Flee 상태 (forceFleeState)

`WorldPigeonManager`가 비둘기를 제거하기 위해 설정하는 상태:
- alert와 관계없이 무조건 Flee 상태
- alert가 변경되지 않음
- 일정 시간 후 자동으로 제거됨

## 플레이어 감지

- 각 비둘기가 독립적으로 `PlayerController.Instance`를 감지
- `detectionRadius` 내에 있으면 감지됨
- 모든 비둘기가 동일하게 작동

## 먹이 경쟁

- 덫의 `eatingRadius` 내에 있는 비둘기들만 경쟁에 참여
- `CanEat()`가 true인 비둘기만 참여 (BackOff/Flee 상태 제외)
- 경쟁자 수만큼 alert 증가
- 일반적인 군집 밀도로는 alert 증가하지 않음 (먹이 경쟁에만 해당)

## 주요 컴포넌트

### PigeonAI
- alert 값 관리
- 상태 전환 처리
- Flee 상태일 때 alert 변경 방지

### PigeonMovement
- 플레이어 감지 및 alert 증가
- 상태에 따른 이동 행동 처리
- 임계값 및 가중치 관리

### FoodTrap
- 먹이 경쟁 alert 증가 처리
- 덫 근처 비둘기 감지

## 설정 가능한 값

### PigeonMovement (Inspector)
- `detectionRadius`: 플레이어/덫 감지 반경 (기본값: 5)
- `eatingRadius`: 먹이를 먹을 수 있는 반경 (기본값: 0.5)
- `alertWeight`: alert 가중치 (기본값: 1.0)
- `warnThreshold`: Cautious 상태 임계값 (기본값: 45)
- `backoffThreshold`: BackOff 상태 임계값 (기본값: 80)
- `fleeThreshold`: Flee 상태 임계값 (기본값: 100)

### PigeonAIProfile (JSON)
- `playerAlertPerSec`: 플레이어 접근 시 초당 alert 증가량
- `crowdAlertPerNeighborPerSec`: 군집(경쟁자)당 초당 alert 증가량
- `alertDecayPerSec`: 초당 alert 감소량
