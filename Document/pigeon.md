1) 핵심 규칙: 스트레스가 높으면 경쟁 참여 안 함

추천 상태 규칙(이미 너가 정한 임계치랑 잘 맞음):

alert < warnThreshold : 정상적으로 Eat 경쟁 참여

warnThreshold ≤ alert < backoffThreshold : Eat 참여 확률/속도 감소 (먹다 말고 주춤)

backoffThreshold ≤ alert < fleeThreshold : Eat 중단 + BackOff로 이동 (경쟁에서 사실상 이탈)

alert ≥ fleeThreshold : Flee(화면 밖 도망)

이렇게 하면 “가만히 두면 잡비둘기들이 계속 먹어서 막타”가 되고,
플레이어가 움직여 혼잡/압박을 만들면 “잡비둘기들이 턴을 잃어 희귀에게 기회”가 생겨.

2) JSON 스키마 예시
(A) 프리셋 파일: PigeonAIProfiles.json

rarityTier별 기본 파라미터 + obesity(비만도)로 eatInterval 보정 규칙 포함.

{
  "version": 1,
  "obesityRule": {
    "bitePowerEqualsObesity": true,
    "eatIntervalMultiplierByObesity": {
      "1": 1.08,
      "2": 1.0,
      "3": 0.92,
      "4": 0.85,
      "5": 0.8
    },
    "obesityPriceDiscount": {
      "1": 1.1,
      "2": 1.0,
      "3": 0.9,
      "4": 0.8,
      "5": 0.7
    }
  },
  "rarityBasePrice": {
    "1": 8,
    "2": 18,
    "3": 40,
    "4": 90,
    "5": 180
  },
  "tiers": {
    "1": {
      "eatInterval": 0.65,
      "eatChance": 0.95,
      "personalSpaceRadius": 0.55,
      "playerAlertPerSec": 38,
      "crowdAlertPerNeighborPerSec": 6,
      "alertDecayPerSec": 10,
      "warnThreshold": 45,
      "backoffThreshold": 80,
      "fleeThreshold": 110,
      "backoffDuration": 0.7,
      "backoffDistance": 0.7,
      "alertWeight": 1.0
    },
    "2": {
      "eatInterval": 0.85,
      "eatChance": 0.88,
      "personalSpaceRadius": 0.6,
      "playerAlertPerSec": 30,
      "crowdAlertPerNeighborPerSec": 7,
      "alertDecayPerSec": 9,
      "warnThreshold": 50,
      "backoffThreshold": 82,
      "fleeThreshold": 110,
      "backoffDuration": 0.85,
      "backoffDistance": 0.8,
      "alertWeight": 1.0
    },
    "3": {
      "eatInterval": 1.1,
      "eatChance": 0.75,
      "personalSpaceRadius": 0.75,
      "playerAlertPerSec": 22,
      "crowdAlertPerNeighborPerSec": 11,
      "alertDecayPerSec": 11,
      "warnThreshold": 55,
      "backoffThreshold": 78,
      "fleeThreshold": 105,
      "backoffDuration": 1.15,
      "backoffDistance": 1.05,
      "alertWeight": 1.0
    },
    "4": {
      "eatInterval": 1.35,
      "eatChance": 0.65,
      "personalSpaceRadius": 0.9,
      "playerAlertPerSec": 18,
      "crowdAlertPerNeighborPerSec": 14,
      "alertDecayPerSec": 12,
      "warnThreshold": 58,
      "backoffThreshold": 74,
      "fleeThreshold": 100,
      "backoffDuration": 1.35,
      "backoffDistance": 1.25,
      "alertWeight": 1.0
    },
    "5": {
      "eatInterval": 1.55,
      "eatChance": 0.55,
      "personalSpaceRadius": 1.1,
      "playerAlertPerSec": 14,
      "crowdAlertPerNeighborPerSec": 16,
      "alertDecayPerSec": 13,
      "warnThreshold": 60,
      "backoffThreshold": 70,
      "fleeThreshold": 95,
      "backoffDuration": 1.55,
      "backoffDistance": 1.45,
      "alertWeight": 1.0
    }
  },
  "stressToEatModifier": {
    "enabled": true,
    "warnEatChanceMultiplier": 0.65,
    "warnEatIntervalMultiplier": 1.25,
    "backoffStopsEating": true
  }
}

(B) 얼굴(가격만) 파일: PigeonFaces.json
{
  "version": 1,
  "faces": [
    { "id": "F00", "name": "기본", "priceMultiplier": 1.0 },
    { "id": "F01", "name": "찡긋", "priceMultiplier": 1.05 },
    { "id": "F02", "name": "상처", "priceMultiplier": 1.08 },
    { "id": "F03", "name": "하트눈", "priceMultiplier": 1.15 },
    { "id": "F04", "name": "왕눈", "priceMultiplier": 1.2 },
    { "id": "F05", "name": "마스크", "priceMultiplier": 1.25 }
  ]
}

(C) 비둘기 종(20종) 파일: PigeonSpecies.json

종은 rarityTier만 확정하고, 나머지 AI는 런타임에서 tier 프리셋으로 자동 생성.

{
  "version": 1,
  "species": [
    {
      "speciesId": "SP01",
      "name": "도시회색",
      "rarityTier": 1,
      "defaultObesityRange": [4, 5],
      "locationPreference": { "PARK": 70, "PLAZA": 80, "ALLEY": 60, "HARBOR": 40, "ROOFTOP": 30 },
      "trapPreference": { "BREAD": 90, "SEEDS": 60, "CORN": 70, "PELLET": 20, "SHINY": 10 }
    },
    {
      "speciesId": "SP07",
      "name": "빵중독",
      "rarityTier": 1,
      "defaultObesityRange": [5, 5],
      "locationPreference": { "PARK": 60, "PLAZA": 90, "ALLEY": 50, "HARBOR": 30, "ROOFTOP": 20 },
      "trapPreference": { "BREAD": 100, "SEEDS": 40, "CORN": 60, "PELLET": 10, "SHINY": 10 }
    },
    {
      "speciesId": "SP14",
      "name": "무지개기름광",
      "rarityTier": 3,
      "defaultObesityRange": [1, 2],
      "locationPreference": { "PARK": 60, "PLAZA": 70, "ALLEY": 40, "HARBOR": 45, "ROOFTOP": 50 },
      "trapPreference": { "BREAD": 55, "SEEDS": 60, "CORN": 60, "PELLET": 55, "SHINY": 45 }
    },
    {
      "speciesId": "SP20",
      "name": "왕관비둘기",
      "rarityTier": 5,
      "defaultObesityRange": [1, 1],
      "locationPreference": { "PARK": 20, "PLAZA": 60, "ALLEY": 15, "HARBOR": 50, "ROOFTOP": 100 },
      "trapPreference": { "BREAD": 10, "SEEDS": 25, "CORN": 35, "PELLET": 85, "SHINY": 80 }
    }
  ]
}


위는 예시로 4종만 넣었고, 나머지 16종도 같은 포맷으로 추가하면 돼.

3) 자동 매핑 규칙 (런타임에서 스탯 생성)
PigeonInstance 생성 시

입력: speciesId, obesity(1~5), faceId

생성 로직(개념):

tier = species.rarityTier

profile = profiles.tiers[tier]

bitePower = obesity (비만도=한입값)

finalEatInterval = profile.eatInterval * obesityMultiplier[obesity]

가격:

base = rarityBasePrice[tier]

price = base * obesityDiscount[obesity] * face.priceMultiplier

AI 파라미터는 profile를 그대로 복사(개체 랜덤 ±10%는 선택)

4) 먹이 20 막타 포획 시스템과 AI의 연결

먹이에는 feedAmount = 20

비둘기는 EatTick 시도:

가능한 상태일 때만(Eat 참여 가능)

성공 시 feedAmount -= bitePower

feedAmount <= 0이 되는 그 tick을 만든 비둘기 = 포획

“Eat 참여 가능” 조건(스트레스 반영)

alert >= backoffThreshold면 EatTick 시도 자체를 중단 (경쟁 참여 X)

warnThreshold ≤ alert < backoffThreshold면:

eatChance *= warnEatChanceMultiplier

eatInterval *= warnEatIntervalMultiplier

이게 너 질문에 대한 답: 맞아, 스트레스가 높으면 경쟁에 참여 안 하게 되는 게 맞고, 너 게임에서는 오히려 반드시 그렇게 해야 함.