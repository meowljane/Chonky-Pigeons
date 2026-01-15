목표

Cursor로 데이터(JSON) + 코드(C#) 를 같이 관리

Unity에서는 Import(버튼 한 번) → ScriptableObject 에셋 생성/갱신

런타임에서는 SO만 참조 (WebGL 안정/속도)

스프라이트는 Resources.Load 없이 SO에 직접 할당하거나 Enum→Sprite Dictionary 캐싱

1) 폴더/에셋 구조 (Unity Project)
Assets/
  GameData/
    SourceJson/          // 원본 JSON (Cursor에서 편집)
      pigeon_ai_profiles.json
      pigeon_faces.json
      pigeon_species.json
      terrain_types.json
      trap_types.json

    Generated/           // 생성된 ScriptableObject 에셋
      AIProfiles.asset
      Faces.asset
      SpeciesSet.asset
      TerrainTypes.asset
      Traps.asset

  Art/
    Pigeons/
    UI/

  Scripts/
    Data/
      Importers/
      Runtime/
    Gameplay/


핵심: JSON은 “원본”, SO는 “Unity가 읽는 실행 데이터”.

2) Cursor에서 하는 작업 (개발 루프)
루프는 이렇게 딱 3단계

Cursor에서 JSON 수정 (Assets/GameData/SourceJson/*.json)

Cursor에서 C# 수정 (Importer/Runtime 코드)

Unity 돌아가서 Tools > Game Data > Import All 클릭 → 바로 플레이

Cursor는 Unity 프로젝트 폴더를 그대로 열어두면 됨.

3) Unity Import 워크플로우 (버튼 한 번으로 끝)
Importer가 하는 일

JSON 읽기

스키마 검증(필수 키 누락 체크)

기존 SO 있으면 업데이트 / 없으면 생성

변경점 로그 출력

(선택) Species마다 “최종 파라미터 미리 계산”해서 SO에 bake 가능

메뉴

Tools > Game Data > Import All

Tools > Game Data > Import Species

Tools > Game Data > Import AI Profiles

4) 런타임 연결 방식 (Game 시작할 때)
GameDataRegistry(단 하나)만 참조

AIProfiles.asset

Faces.asset

SpeciesSet.asset

TerrainTypes.asset

Traps.asset

그리고 나머지 시스템은 전부 Registry에서 데이터 가져옴.

WebGL에서 JSON 파싱을 매번 하는 게 아니라, 에디터에서 SO로 만들어서 런타임은 SO만 → 안정/빠름.

5) “Unity랑 연결된 Cursor”에서 가장 중요한 포인트: 자동 갱신 트리거

두 가지 중 하나 선택하면 됨:

A안 (가장 단순/안전): 메뉴 버튼으로 수동 Import

JSON 수정 후 Unity에서 클릭 한 번

디버깅 쉽고 오류 원인 추적 쉬움

B안 (조금 더 편함): JSON 변경 시 자동 Import (권장 X, 나중에)

AssetPostprocessor로 json 변경 감지 → 자동 Import

편한 대신 “언제 Import됐는지” 헷갈릴 수 있음

너는 지금 2번째 게임을 빠르게 만들 거니까 A안 추천.

6) “수동 스프라이트 매핑”을 깨끗하게 유지하는 방법

너가 원했던 스타일(수동 캐싱) 그대로 가자.

Species SO에는 Sprite를 “직접 필드로” 둔다

PigeonSpeciesDefinition

speciesId

displayName

rarityTier

defaultObesityRange

Sprite icon ✅ (에디터에서 드래그로 지정)

JSON에는 sprite path 같은 거 넣지 말고,
SO에만 스프라이트를 직접 연결해.

Importer는 스프라이트는 건드리지 않고 텍스트/수치만 업데이트하도록 설계.

이러면 데이터 수정해도 아이콘 연결이 끊기지 않음.

7) 게임플레이 코드에서의 “최종 파라미터 생성” 위치

너가 원한 규칙:

비만도 → bitePower

rarityTier → AI 프리셋

얼굴 → 가격 배수만

이 “합성”을 어디서 하느냐가 중요한데, 추천은:

추천: 런타임에서 PigeonInstanceFactory 한 곳에서만 합성

입력: speciesId, obesity, faceId

출력: PigeonInstanceStats (bitePower/eatInterval/threshold/price 등 “완성본”)

장점:

밸런스 수정 포인트가 한 곳

디버깅 쉬움

저장/로드도 간단 (speciesId, obesity, faceId만 저장하면 됨)

8) 개발 루프 예시 (너가 실제로 하게 될 흐름)

Cursor에서 pigeon_ai_profiles.json에서 T3의 alertWeight를 조절 (기본값 1.0)

Unity에서 Import AI Profiles 클릭

Play → 희귀가 군집에서 더 잘 BackOff 하는지 확인

Cursor에서 pigeon_species.json에서 SP18의 trapPreference.SHINY를 55→80으로 변경

Unity Import Species 클릭

Play → 옥상/반짝간식에서 SP18 뜨는지 확인

9) 네가 지금 바로 만들면 좋은 “Importer 체크리스트”

Importer는 처음부터 크게 만들 필요 없어. 최소만:

 JSON 파싱

 필수 필드 검증(없으면 에러 로그)

 SO 생성/업데이트

 기존 SO에 들어있는 Sprite 레퍼런스는 절대 덮어쓰지 않기

 Import 후 요약 로그 출력 (“Species 20 updated, Faces 6 updated…”)