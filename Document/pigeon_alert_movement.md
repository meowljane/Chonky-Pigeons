# 비둘기 Alert & 이동 시스템 상세 설명

## 목차
1. [Alert 시스템 개요](#alert-시스템-개요)
2. [Alert 값 변화 메커니즘](#alert-값-변화-메커니즘)
3. [상태 전환 시스템](#상태-전환-시스템)
4. [이동 행동 시스템](#이동-행동-시스템)
5. [Alert와 이동의 상호작용](#alert와-이동의-상호작용)
6. [튜닝 포인트](#튜닝-포인트)

---

## Alert 시스템 개요

### 기본 구조
- **Alert 값**: `float` 타입, 0부터 시작하여 증가/감소
- **상태**: `PigeonState` enum (Normal, Cautious, BackOff, Flee)
- **관리 클래스**: `PigeonAI.cs`

### Alert의 역할
Alert는 비둘기의 **긴장도/스트레스**를 나타내는 수치입니다. 이 값에 따라 비둘기의 행동이 결정됩니다.

---

## Alert 값 변화 메커니즘

### 1. Alert 증가 요인

#### 플레이어 접근 (`AddPlayerAlert`)
```csharp
alert += stats.playerAlertPerSec * stats.alertWeight * deltaTime * distanceFactor
```

- **위치**: `PigeonMovement.UpdateAlertSystem()`
- **감지 방식**: `Physics2D.OverlapCircleAll()` 사용
- **감지 범위**: `alertDetectionRadius` (기본 10f) - 플레이어 및 군집 감지에 동일하게 사용
- **거리 비율**: 플레이어가 가까울수록 더 많이 증가
  - `distanceFactor = 1f - (minPlayerDistance / alertDetectionRadius)`
  - 플레이어가 바로 옆에 있으면 `distanceFactor = 1.0`
  - 감지 범위 끝에 있으면 `distanceFactor ≈ 0`

#### 군집 밀도 (`AddCrowdAlert`)
```csharp
alert += stats.crowdAlertPerNeighborPerSec * stats.alertWeight * neighborCount * deltaTime
```

- **감지 범위**: `alertDetectionRadius` (기본 10f) - 플레이어 감지와 동일한 반경 사용
- **계산 방식**: 주변 비둘기 수 × 시간 × 가중치
- **최적화**: `Physics2D.OverlapCircleAll()` 사용

### 2. Alert 감소

```csharp
alert = Mathf.Max(0, alert - stats.alertDecayPerSec * Time.deltaTime)
```

- **위치**: `PigeonAI.Update()` (매 프레임)
- **자연 감소**: 자극이 없으면 `alertDecayPerSec`만큼 초당 감소
- **최소값**: 0 이하로 내려가지 않음

### 3. Alert 임계값

각 비둘기는 다음 3개의 임계값을 가집니다 (rarityTier별로 다름):

- **`warnThreshold`**: 경고 상태 시작
- **`backoffThreshold`**: 뒷걸음질 시작
- **`fleeThreshold`**: 도망 시작

---

## 상태 전환 시스템

### 상태 전환 로직 (`PigeonAI.UpdateState()`)

```csharp
if (alert >= stats.fleeThreshold)
    currentState = PigeonState.Flee;
else if (alert >= stats.backoffThreshold)
    currentState = PigeonState.BackOff;
else if (alert >= stats.warnThreshold)
    currentState = PigeonState.Cautious;
else
    currentState = PigeonState.Normal;
```

### 상태별 특징

| 상태 | Alert 범위 | 먹이 경쟁 | 이동 행동 |
|------|-----------|----------|----------|
| **Normal** | `alert < warnThreshold` | ✅ 참여 | 배회 + 덫 탐색 |
| **Cautious** | `warnThreshold ≤ alert < backoffThreshold` | ✅ 참여 (느려짐) | 배회 + 덫 탐색 |
| **BackOff** | `backoffThreshold ≤ alert < fleeThreshold` | ❌ 미참여 | 플레이어 및 군집에서 후퇴 |
| **Flee** | `alert ≥ fleeThreshold` | ❌ 미참여 | 화면 밖으로 도망 |

### 먹이 경쟁과의 관계

#### `CanEat()` 메서드
```csharp
return currentState != PigeonState.BackOff && currentState != PigeonState.Flee;
```

- **BackOff 이상**이면 먹이 경쟁에서 **완전히 제외**
- 이로 인해 "잡비둘기들이 Alert로 경쟁에서 이탈 → 희귀 비둘기가 기회를 잡음" 구조가 가능

#### Cautious 상태의 먹이 행동
- **`GetEatChance()`**: `eatChance *= warnEatChanceMultiplier` (기본 0.65)
- **`GetEatInterval()`**: `eatInterval *= warnEatIntervalMultiplier` (기본 1.25)
- → 먹이를 먹지만 **느리고 확률이 낮음**

---

## 이동 행동 시스템

### 이동 우선순위 (`PigeonMovement.Update()`)

```
1. Alert 시스템 업데이트
2. Flee 상태 체크 → 최우선 처리 (다른 모든 조건 무시)
3. 플레이어 근접 체크 → BackOff 강제
4. 상태별 이동 처리
```

### 1. Flee 상태 (최우선)

**우선순위**: 모든 다른 조건보다 우선

```csharp
if (currentState == PigeonState.Flee)
{
    HandleFlee();
    return; // 즉시 종료
}
```

**행동**:
- 플레이어 반대 방향 또는 화면 밖으로 도망
- 속도: `fleeSpeed` (기본 4f)
- 다른 모든 행동 무시

### 2. 플레이어 근접 감지

**조건**: `alertDetectionRadius` 내에 플레이어가 있음

**감지 방식**: `Physics2D.OverlapCircleAll()` 사용

```csharp
Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, alertDetectionRadius);
// PlayerController 컴포넌트를 가진 오브젝트 찾기
```

**특징**:
- Alert 값과 **무관하게** 강제로 BackOff
- Flee 상태가 아닐 때만 적용
- `UpdateAlertSystem()`과 별도로 `Update()`에서도 동일한 방식으로 감지

### 3. 상태별 이동 처리

#### Normal / Cautious 상태 (`HandleNormalMovement`)

**행동**:
1. **랜덤 이탈**: `randomFlyAwayChance` 확률로 화면 밖으로 날아감
2. **덫 탐색**: `foodDetectionRadius` 내의 가장 가까운 덫 찾기
3. **배회**: 덫이 없으면 `wanderTarget`으로 이동
4. **이동 속도**: `wanderSpeed` (기본 2f)

**목표 우선순위**:
- 덫이 있고 비어있지 않으면 → 덫 위치
- 그 외 → 배회 목표 위치

#### BackOff 상태 (`HandleBackOff`)

**목적**: BackOff 원인에 따라 해당 원인에서 일정 거리만큼 후퇴

**원인 판단**:
1. **플레이어 강제 BackOff**: 플레이어가 `alertDetectionRadius` 내에 있으면 무조건 BackOff → 플레이어 원인
2. **Alert 기반 BackOff**: Alert가 `backoffThreshold`를 넘어서 BackOff 상태가 된 경우
   - 이전 프레임의 플레이어 Alert 증가량과 군집 Alert 증가량 비교
   - 플레이어 Alert 증가량이 더 크면 → 플레이어 원인
   - 군집 Alert 증가량이 더 크면 → 군집 원인

**메커니즘**:
1. **목표 계산**: 
   - **플레이어 원인**인 경우 → 플레이어 반대 방향으로만 후퇴
   - **군집 원인**인 경우 → 비둘기 군집 중심 반대 방향으로만 후퇴
   - 원인을 찾을 수 없으면 → 랜덤 방향
   - **덫은 고려하지 않음**
2. **거리**: `backoffDistance`만큼 떨어진 위치
3. **지속 시간**: `backoffDuration` 동안 유지
4. **이동 속도**: `backoffSpeed` (기본 3f)

**참고**: BackOff 상태에서는 원인에 따라 해당 원인에서만 후퇴합니다. 플레이어로 인한 BackOff는 플레이어에서만, 군집으로 인한 BackOff는 군집에서만 멀어집니다.

#### Flee 상태 (`HandleFlee`)

**방향 계산**:
1. 플레이어가 있으면 → 플레이어 반대 방향
2. 카메라 기준 → 화면 중심 반대 방향
3. 그 외 → 랜덤 방향

**이동**: `rb.linearVelocity = fleeDirection * fleeSpeed`

---

## Alert와 이동의 상호작용

### 상호작용 흐름도

```
플레이어 접근 / 군집 밀도 증가
    ↓
Alert 증가
    ↓
임계값 도달
    ↓
상태 전환 (Normal → Cautious → BackOff → Flee)
    ↓
이동 행동 변경
    ↓
먹이 경쟁 참여 여부 결정
```

### 핵심 메커니즘

1. **Alert 증가** → 상태 전환 → 이동 행동 변경
2. **BackOff 이상** → 먹이 경쟁에서 제외
3. **Flee 상태** → 모든 다른 조건 무시하고 도망

### 게임플레이 영향

- **가만히 두기**: 잡비둘기들이 계속 먹어서 막타
- **플레이어 개입**: 잡비둘기들이 Alert로 경쟁 이탈 → 희귀 비둘기가 기회를 잡음

---

## 튜닝 포인트

### AI 프로파일 (데이터)에서 조절

**Alert 관련**:
- `playerAlertPerSec`: 플레이어 접근 시 Alert 증가 속도
- `crowdAlertPerNeighborPerSec`: 주변 비둘기 1마리당 Alert 증가 속도
- `alertDecayPerSec`: Alert 자연 감소 속도
- `warnThreshold`: 경고 상태 시작 임계값
- `backoffThreshold`: 뒷걸음질 시작 임계값
- `fleeThreshold`: 도망 시작 임계값

**가중치**:
- `alertWeight`: 플레이어 및 군집 Alert 가중치 (통일) - 두 Alert 증가에 동일하게 적용

**먹이 행동**:
- `eatChance`: 기본 먹이 먹기 확률
- `eatInterval`: 기본 먹이 먹기 간격

### PigeonMovement에서 조절

**속도**:
- `wanderSpeed`: 배회 속도 (기본 2f)
- `backoffSpeed`: 뒷걸음질 속도 (기본 3f)
- `fleeSpeed`: 도망 속도 (기본 4f)

**반경**:
- `foodDetectionRadius`: 덫 탐색 반경 (기본 5f)
- `eatingRadius`: 실제 먹기 가능 반경 (기본 2f)
- `alertDetectionRadius`: 플레이어 및 군집 감지 반경 (기본 10f) - 두 감지에 동일하게 사용

**기타**:
- `wanderRadius`: 배회 반경 (기본 1f)
- `wanderInterval`: 배회 목표 변경 간격 (기본 2f)
- `randomFlyAwayChance`: 화면 밖으로 날아갈 확률 (기본 0.01f)

---

## 코드 구조 요약

### 주요 클래스

1. **`PigeonAI.cs`**
   - Alert 값 관리
   - 상태 전환 로직
   - 먹이 경쟁 참여 여부 결정

2. **`PigeonMovement.cs`**
   - 이동 행동 처리
   - Alert 시스템 업데이트
   - 상태별 이동 메서드

3. **`PigeonInstanceFactory.cs`**
   - rarityTier별 AI 프로파일 적용
   - 비둘기 인스턴스 생성

### 데이터 흐름

```
GameDataRegistry (AIProfiles)
    ↓
PigeonInstanceFactory.CreateInstanceStats()
    ↓
PigeonInstanceStats (개체별 파라미터)
    ↓
PigeonAI.Initialize()
    ↓
PigeonMovement.Update() → PigeonAI.Update()
    ↓
상태 전환 및 이동 행동
```

---

## 디버깅 팁

### Gizmos 시각화
`PigeonMovement.OnDrawGizmosSelected()`에서 다음을 표시:
- 노란색: 덫 탐색 반경
- 청록색: 먹기 반경
- 빨간색: 플레이어 감지 반경
- 상태별 색상: 비둘기 위치 표시

### Alert 바 UI
`PigeonStatusUI.cs`에서:
- Alert 값을 0-100 범위로 정규화하여 표시
- 상태별 색상 변경 (초록/노랑/마젠타/빨강)

---

## 참고사항

- Alert 값은 **0부터 시작**하며 상한선이 없음 (100을 넘어갈 수 있음)
- Flee 상태는 **절대 최우선**이며 다른 모든 조건을 무시
- 플레이어 근접 감지는 Flee 상태가 아닐 때만 BackOff를 강제
- BackOff 이상 상태에서는 **먹이 경쟁에 참여하지 않음**
