# 코드 리팩토링 가이드

## 📁 프로젝트 구조

```
Assets/_Scripts/
├── AI Image/          # AI 이미지 생성 관련
├── API/              # API 통신 및 모델
│   ├── APIManager.cs
│   └── APIModels.cs  ⭐ (모든 Serializable 클래스 여기로)
├── Feature/          # 공통 기능/유틸리티
│   ├── UserSession.cs
│   ├── FadeController.cs
│   └── LoadingPanel.cs
├── Friend/           # 친구 기능
├── Game/             # 게임 로직
│   ├── PuzzleManager.cs
│   ├── ClearPopup.cs
│   └── MultiplayRankPopup.cs
├── Planet/           # 행성 기능
├── SceneManager/     # 씬별 매니저
│   ├── 000_Mainmenu.cs
│   ├── B001_CreateParty.cs
│   ├── B002_JoinParty.cs
│   └── G001_TagInput.cs
└── WebSocket/        # 멀티플레이 통신
    ├── SocketIOManager.cs
    └── MultiplaySession.cs
```

---

## 📌 기본 원칙

### 해야 할 것

-   한글 주석 → 영어 (짧고 간결하게)
-   메서드 순서 정리
-   로그 메시지 통일

### 하면 안 되는 것

-   변수/메서드명 변경 ❌
-   로직 수정 ❌
-   기능 추가/삭제 ❌

> **핵심**: 기능은 그대로 두고 정리해서 코드 가독성 높이기

---

## 🎯 작업 순서

### Phase 1: 코드 리팩토링 (파일별로 하나씩)

#### 1단계: 주석 영어 변환

```csharp
// ❌ 나쁜 예
// 사용자 인증 후 토큰 저장하는 메서드입니다

// ✅ 좋은 예
// Authenticate user and store token
```

**규칙**

-   짧고 간결하게
-   명령형 동사 사용 (Initialize, Load, Check...)
-   당연한 내용은 주석 삭제

#### 2단계: 메서드 순서 정리

```csharp
public class ExampleManager : MonoBehaviour
{
    // ===== Singleton =====
    public static ExampleManager Instance { get; private set; }

    // ===== Inspector Fields =====
    [Header("UI References")]
    public Button button;

    [Header("Settings")]
    public float timeout = 30f;

    // ===== Properties =====
    public bool IsActive { get; private set; }

    // ===== Private Fields =====
    private bool isProcessing;
    private const int MAX_RETRIES = 3;

    // ===== Unity Lifecycle =====
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }

    // ===== Public Methods =====
    public void Initialize() { }

    // ===== Event Handlers =====
    public void OnButtonClick() { }

    // ===== Private Methods =====
    private void ProcessData() { }
    private IEnumerator LoadCoroutine() { }
}
```

#### 3단계: 로그 메시지 통일

```csharp
// ❌ 나쁜 예
Debug.Log("연결 시작");
Debug.Log("Connection started");
Debug.Log("Starting connection...");

// ✅ 좋은 예
Debug.Log("[APIManager] Connecting to server...");
Debug.Log("[APIManager] Connection successful");
Debug.LogError("[APIManager] Connection failed: " + error);
```

**규칙**

-   `[클래스명]` 태그 붙이기
-   진행형: "Loading...", "Connecting..."
-   완료: "Connection successful", "Data loaded"
-   에러: "Connection failed: " + 상세내용

---

### Phase 2: 네이밍 검토 (나중에 별도 작업)

**지금은 절대 건드리지 말 것!**

나중에 할 일:

1. 전체 프로젝트 네이밍 패턴 분석
2. 일관성 검토 리포트 작성
3. 변경 계획 수립
4. IDE의 Rename Refactoring으로 일괄 변경

---

---

## 🚨 주의사항

1. **한 번에 한 파일씩**

    - 여러 파일 동시 작업 금지
    - 파일 하나 완료 → 테스트 → 커밋 → 다음 파일

2. **변수/메서드명 절대 변경 금지**

    - `userName` → `username` 이런 것도 안 됨
    - 나중에 Phase 2에서 일괄 처리

3. **로직 수정 금지**
    - if문, for문 등 로직은 그대로
    - 오로지 코드 가독성 향상을 위한 순서 정리 및 주석 변경만

=====

AI Image (Test), API, WebSocket 완료
Feature / Friend / Game / Planet / SceneManager 남음
