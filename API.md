# API 통합 명세서

## 📊 API 개요

총 API 개수: 48개
- 유저 관리: 7개
- 게임 (싱글): 3개
- 게임 (멀티): 6개
- 행성 관리: 9개
- 친구 관리: 7개
- 출결 관리: 2개

---

## 🔑 인증 방식

JWT 토큰 기반 인증
- 로그인 시 토큰 발급
- 이후 요청 시 Header에 포함: `Authorization: Bearer {TOKEN}`

---

## 📡 공통 응답 구조

```json
{
  "isSuccess": true,
  "code": 200,
  "message": "성공 메시지",
  "result": { /* 데이터 */ }
}
```

---

# 1️⃣ 유저 관리

## 회원가입 아이디 중복 확인

**GET** `/users/check-username`

### Query Parameter
- `username`: 중복 여부를 확인할 사용자 아이디

### Response (200)
```json
{
  "available": true
}
```

### Response (400)
```json
{
  "available": false
}
```

---

## 회원가입

**POST** `/users/signup`

### Headers
- Content-Type: application/json

### Request
```json
{
  "username": "testuser",
  "password": "1234",
  "nickname": "tester1"
}
```

### Response (201)
```json
{
  "성공": true,
  "userId": 1
}
```

### Response (400)
```json
{
  "에러": "사용 중인 아이디"
}
```

### Response (500)
```json
{
  "에러": "서버 오류"
}
```

---

## 로그인

**POST** `/users/login`

### Headers
- Content-Type: application/json

### Request
```json
{
  "userId": "testUser",
  "password": "1234"
}
```

### Response (200)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6..."
}
```

**로그인 성공 시 JWT 토큰 발급. 이후 요청에 `Authorization: Bearer <token>` 형태로 사용**

### Response (400)
```json
{
  "에러": "아이디 또는 비밀번호가 잘못됨"
}
```

### Response (500)
```json
{
  "에러": "서버 오류"
}
```

---

## 프로필 정보 조회

**GET** `/users/profile`

### Headers
- Authorization: Bearer {TOKEN}

### Response (200)
```json
{
  "isSuccess": true,
  "code": "USER200",
  "message": "프로필 조회 성공",
  "result": {
    "nickname": "철수",
    "profileImageUrl": "https://your-bucket.s3.ap-northeast-2.amazonaws.com/profile/user123.png"
  }
}
```

### Response (401)
```json
{
  "error": "토큰 없음"
}
```

### Response (403)
```json
{
  "error": "토큰이 유효하지 않음"
}
```

### Response (404)
```json
{
  "error": "유저를 찾을 수 없음"
}
```

---

## 닉네임 변경

**PUT** `/users/nickname`

### Headers
- Authorization: Bearer {TOKEN}
- Content-Type: application/json

### Request
```json
{
  "nickname": "tester11"
}
```

### Response (200)
```json
{
  "success": true
}
```

### Response (400)
```json
{
  "error": "닉네임 변경 실패"
}
```

### Response (403)
```json
{
  "error": "토큰이 유효하지 않음"
}
```

---

## 비밀번호 변경

**PUT** `/users/password`

### Headers
- Authorization: Bearer {TOKEN}
- Content-Type: application/json

### Request
```json
{
  "oldPassword": "1234",
  "newPassword": "5678"
}
```

### Response (200)
```json
{
  "success": true
}
```

### Response (400)
```json
{
  "error": "기존 비밀번호가 틀림"
}
```

### Response (403)
```json
{
  "error": "토큰이 유효하지 않음"
}
```

### Response (500)
```json
{
  "error": "서버 오류"
}
```

---

## 모든 유저 목록 조회

**GET** `/users`

### Response
```json
{
  "content": [
    {
      "userId": 1,
      "name": "HongGilDong",
      "grade": 3,
      "phoneNumber": "010-1234-5678",
      "birth": "2003-03-03"
    }
  ]
}
```

---

## 유저 조회

**GET** `/users/{user_id}`

### Response
```json
{
  "userId": 1,
  "name": "HongGilDong",
  "grade": 3,
  "phoneNumber": "010-1234-5678",
  "birth": "2003-03-03"
}
```

---

# 2️⃣ 게임 - 싱글플레이

## 게임 시작 (개인)

**POST** `/games/single/start`

### Request
```json
{
  "tags": ["tag1", "tag2", "tag3", "tag4"]
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "AI 이미지 생성 성공",
  "result": {
    "roomId": "room-uuid-123",
    "gameCode": "SINGLE789",
    "imageUrl": "https://s3.amazonaws.com/bucket/generated-image.jpg"
  }
}
```

**참고**: 현재 데이터베이스 구조상 개인플레이도 roomId가 필요함

---

## 게임 클리어 (개인)

**POST** `/games/single/complete`

### Request
```json
{
  "gameCode": "SINGLE",
  "startTime": "2025-01-01T10:00:00.000Z",
  "endTime": "2025-01-01T10:00:45.000Z"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "클리어 기록 저장 성공",
  "result": {
    "gameId": "game-uuid-456",
    "gameCode": "SINGLE789",
    "clearTimeMs": 45000,
    "imageUrl": "https://s3.amazonaws.com/bucket/generated-image.jpg",
    "gameStatus": "completed"
  }
}
```

---

## 이미지 → 행성 저장 (개인)

**POST** `/games/single/save-to-planet`

### Request
```json
{
  "gameCode": "게임 코드",
  "title": "갤러리 제목 (선택사항)"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "게임 이미지가 행성에 저장되었습니다",
  "result": {
    "planetId": "planet-uuid-789",
    "imageUrl": "https://s3.amazonaws.com/bucket/generated-image.jpg",
    "galleryTitle": "갤러리 제목 (선택사항)"
  }
}
```

---

# 3️⃣ 게임 - 멀티플레이

## 방 생성 (멀티)

**POST** `/games/multiplay/rooms/create`

### Request
```json
{
  "tags": ["cat", "forest", "sunset", "magic"]
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "ROOM200",
  "message": "방 생성 및 이미지 생성 성공",
  "result": {
    "roomId": "room-uuid-123",
    "gameCode": "ABC123",
    "hostUsername": "player1",
    "imageUrl": "https://bucket.s3.region.amazonaws.com/game/image-uuid.png",
    "tags": ["cat", "forest", "sunset", "magic"]
  }
}
```

---

## 방 입장 (멀티)

**POST** `/games/multiplay/rooms/join`

### Request
```json
{
  "gameCode": "123456"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "ROOM200",
  "message": "방 입장 성공",
  "result": {
    "roomId": 10,
    "gameCode": "123456",
    "participants": [
      {"userId": 1, "nickname": "철수", "isReady": 0},
      {"userId": 2, "nickname": "영희", "isReady": 0}
    ],
    "hostUsername": "철수",
    "imageUrl": "https://bucket.s3.region.amazonaws.com/game/image-uuid.png",
    "tags": ["cat", "forest", "sunset", "magic"]
  }
}
```

---

## 준비 상태 토글 (멀티)

**POST** `/games/multiplay/rooms/ready`

**참고**: 팀원만 사용 가능 (호스트는 사용 불가)

### Request
```json
{
  "gameCode": "string"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "ROOM200",
  "message": "준비 완료",
  "result": {
    "isReady": 1,
    "participants": [
      {
        "userId": "user1-uuid",
        "username": "testuser1",
        "isReady": 1
      },
      {
        "userId": "user2-uuid",
        "username": "testuser2",
        "isReady": 0
      }
    ]
  }
}
```

---

## 게임 시작 (멀티)

**POST** `/games/multiplay/rooms/start`

**참고**: 다른 팀원들이 모두 준비 완료 상태면, 팀장은 따로 준비 완료 버튼 없이 바로 시작 버튼을 누를 수 있음

### Request
```json
{
  "gameCode": "string"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "게임 시작 성공",
  "result": {
    "roomId": 123,
    "gameCode": "ABC123",
    "gameStatus": "playing",
    "participants": [
      {
        "userId": 1,
        "username": "player1",
        "isReady": false,
        "isHost": true
      },
      {
        "userId": 2,
        "username": "player2",
        "isReady": true,
        "isHost": false
      }
    ]
  }
}
```

---

## 게임 완료 (멀티)

**POST** `/games/multiplay/rooms/complete`

### Request
```json
{
  "gameCode": "string"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "게임 완료 성공 - 승리!",
  "result": {
    "gameId": "game-uuid-123",
    "gameCode": "ABC123",
    "isWinner": true,
    "totalParticipants": 2,
    "gameStatus": "completed",
    "participants": [
      {
        "userId": 1,
        "username": "player1",
        "isWinner": true
      },
      {
        "userId": 2,
        "username": "player2",
        "isWinner": false
      }
    ]
  }
}
```

---

## 승리자 이미지 → 행성 저장 (멀티)

**POST** `/games/multiplay/save-to-planet`

### Request
```json
{
  "gameCode": "게임 코드",
  "title": "갤러리 제목 (선택사항)"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": "GAME200",
  "message": "멀티플레이 승리 이미지가 행성에 저장되었습니다",
  "result": {
    "planetId": "550e8400-e29b-41d4-a716-446655440000",
    "imageUrl": "https://your-bucket.s3.ap-northeast-2.amazonaws.com/game/uuid.png",
    "galleryTitle": "멀티플레이 승리 이미지",
    "gameCode": "ABC123",
    "clearTimeMs": 45000,
    "isWinner": true
  }
}
```

---

## 웹소켓 이벤트 (멀티)

### 클라이언트 → 서버 이벤트

**1. 연결**
- Unity에서 Socket.IO 사용 시 SocketIOUnity 패키지 설치 필요

**2. 인증 (이벤트명: authenticate)**
```json
{
  "token": "JWT토큰"
}
```

**3. 방 입장 (이벤트명: join_room)**
```json
{
  "gameCode": "ABC123"
}
```

**4. 방 퇴장 (이벤트명: leave_room)**
```json
{
  "gameCode": "ABC123"
}
```

### 서버 → 클라이언트 이벤트

**1. 인증 결과 (이벤트명: authenticated)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "인증이 완료되었습니다",
  "result": {
    "userId": "사용자UUID",
    "username": "사용자명"
  }
}
```

**2. 사용자 입장 알림 (이벤트명: user_joined)**
- 다른 사용자들에게만 알림이 감
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "새로운 사용자가 입장했습니다",
  "result": {
    "userId": "새사용자UUID",
    "username": "새사용자명",
    "gameCode": "ABC123",
    "participants": [
      {
        "userId": "팀장UUID",
        "username": "팀장사용자명",
        "isReady": false,
        "isHost": true
      },
      {
        "userId": "팀원UUID",
        "username": "팀원사용자명",
        "isReady": false,
        "isHost": false
      }
    ]
  }
}
```

**3. 사용자 퇴장 알림 (이벤트명: user_left)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "사용자가 방을 떠났습니다",
  "result": {
    "userId": "사용자UUID",
    "username": "사용자명",
    "gameCode": "ABC123"
  }
}
```

**4. 준비상태 토글 업데이트 (이벤트명: room_updated)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "방 상태가 업데이트되었습니다",
  "result": {
    "gameCode": "ABC123",
    "participants": [
      {
        "userId": "팀장UUID",
        "username": "팀장사용자명",
        "isReady": false,
        "isHost": true
      },
      {
        "userId": "팀원UUID",
        "username": "팀원사용자명",
        "isReady": true,
        "isHost": false
      }
    ]
  }
}
```

**5. 게임 시작 알림 (이벤트명: game_started)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "게임이 시작되었습니다!",
  "result": {
    "gameId": "게임UUID",
    "gameCode": "ABC123",
    "participants": [
      {
        "userId": "팀장UUID",
        "username": "팀장사용자명",
        "isReady": true,
        "isHost": true
      },
      {
        "userId": "팀원UUID",
        "username": "팀원사용자명",
        "isReady": true,
        "isHost": false
      }
    ]
  }
}
```

**6. 게임 완료 알림 (이벤트명: game_completed)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "게임이 완료되었습니다!",
  "result": {
    "gameId": "게임UUID",
    "gameCode": "ABC123",
    "winner": {
      "userId": "승자UUID",
      "username": "승자사용자명",
      "clearTimeMs": 45000
    },
    "gameStatus": "completed"
  }
}
```

**7. 사용자 연결 해제 알림 (이벤트명: user_disconnected)**
```json
{
  "isSuccess": true,
  "code": "WS200",
  "message": "사용자 연결이 해제되었습니다",
  "result": {
    "userId": "사용자UUID",
    "username": "사용자명"
  }
}
```

---

# 4️⃣ 행성 관리

## 행성 목록 조회

**GET** `/planets`

**참고**: 
- 한 번에 전체 사용자를 보냄
- 전체 공개, 비로그인 접근 가능

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "행성 목록 조회 성공",
  "result": [
    {
      "id": "1a339047-ae9d-11f0-9ef2-005056c00001",
      "ownerUsername": "test3",
      "title": "tester3의 행성",
      "visit_count": 0,
      "created_at": "2025-10-21T16:43:42.000Z"
    },
    {
      "id": "2224bc3c-ae9d-11f0-9ef2-005056c00001",
      "ownerUsername": "test4",
      "title": "tester4의 행성",
      "visit_count": 0,
      "created_at": "2025-10-21T16:43:55.000Z"
    }
  ]
}
```

---

## 행성 상세 정보 조회

**GET** `/planets/:username`

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "행성 상세 조회 성공",
  "result": {
    "id": "행성ID",
    "ownerId": "소유자ID",
    "ownerUsername": "test3",
    "title": "행성 제목",
    "visitCount": 5,
    "createdAt": "2025-10-21T16:43:42.000Z",
    "profileImageUrl": "https://example.com/profile-image.jpg",
    "isOwner": false,
    "canEdit": false
  }
}
```

**참고**: 
- `owner_id === userId`이면 자신의 행성
- `owner_id !== userId`이면 타인의 행성

---

## 내 행성 정보 수정

**PUT** `/planets/me`

### Headers
- Authorization: Bearer {TOKEN}

**참고**: request에는 수정하고 싶은 항목(title, profileImage)만 넣으면 됨. 물론 둘 다 넣어도 됨.

### Request (둘 다 수정)
```json
{
  "title": "새로운 행성 제목",
  "profileImage": "https://s3.amazonaws.com/bucket/profile.jpg"
}
```

### Request (행성 제목만 수정)
```json
{
  "title": "새로운 행성 제목"
}
```

### Request (프로필 이미지만 수정)
```json
{
  "profileImage": "https://s3.amazonaws.com/bucket/profile.jpg"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "행성 정보 수정 성공",
  "result": {
    "id": "1a339047-ae9d-11f0-9ef2-005056c00001",
    "ownerId": "abb4a772-1e31-415f-a947-08cad4133fa5",
    "username": "test3",
    "title": "new-new title",
    "visitCount": 0,
    "createdAt": "2025-10-21T16:43:42.000Z",
    "profileImageUrl": "https://example.com/new-new-profile-image.jpg"
  }
}
```

**참고**: 자신의 행성일 경우 (owner_id === userId) 에는 방문 수 증가하지 않음

---

## 타사용자 행성 방문

**POST** `/planets/:username/visit`

### Headers
- Authorization: Bearer {TOKEN}

**참고**: 
- 본인 행성은 방문 수 증가하지 않음
- 행성 방문 중복 카운트 안 됨 (예: a가 b의 행성에 3번 방문하면 1번으로 카운트함)

### Response (첫 방문)
```json
{
  "isSuccess": true,
  "code": 201,
  "message": "행성 방문 성공",
  "result": {
    "username": "test4",
    "visitCount": 1
  }
}
```

### Response (이미 방문한 경우)
```json
{
  "isSuccess": false,
  "code": 400,
  "message": "자신의 행성은 방문할 수 없습니다"
}
```

---

## 갤러리 목록 조회

**GET** `/planets/:username/gallery`

**참고**: 비어 있어도 `result: []` 로 응답

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "갤러리 목록 조회 성공",
  "result": {
    "username": "test3",
    "galleries": [
      {
        "galleryId": 1,
        "title": "이미지 제목2",
        "image_url": "https://s3.amazonaws.com/bucket/magic-castle.jpg",
        "created_at": "2025-10-21T16:43:42.000Z"
      },
      {
        "galleryId": 2,
        "title": "이미지 제목3",
        "image_url": "https://s3.amazonaws.com/bucket/fantasy-forest.jpg",
        "created_at": "2025-10-21T17:15:30.000Z"
      }
    ]
  }
}
```

---

## 갤러리 상세 조회

**GET** `/planets/:username/gallery/:imageId`

### Response
```json
{
  "result": {
    "username": "test3",
    "imageId": "dd456499-af3b-11f0-9ef2-005056c00001",
    "galleryId": "5db8da8d-af3c-11f0-9ef2-005056c00001",
    "title": "첫번째 이미지 제목",
    "image_url": "https://...",
    "metadata": {
      "tags": ["1-1", "1-2", "1-3", "1-4"],
      "generatedAt": "2025-10-22T11:40:09.861Z",
      "pollinateApi": true
    }
  }
}
```

---

## 방명록 조회

**GET** `/planets/:username/guestbook`

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "방명록 조회 성공",
  "result": {
    "username": "test4",
    "guestbooks": [
      {
        "id": "13dffeb6-af52-11f0-9ef2-005056c00001",
        "content": "world",
        "authorUsername": "test3",
        "authorProfileImageUrl": "https://example.com/new-new-profile-image.jpg",
        "written_at": "2025-10-22T14:19:10.000Z"
      },
      {
        "id": "10259e67-af52-11f0-9ef2-005056c00001",
        "content": "hello",
        "authorUsername": "test3",
        "authorProfileImageUrl": "https://example.com/new-new-profile-image.jpg",
        "written_at": "2025-10-22T14:19:04.000Z"
      }
    ]
  }
}
```

**참고**: 
- 위쪽이 최신에 쓴 글 (world)
- 아래쪽이 가장 오래된 글 (hello)
- 프론트에서 방명록 글 + 해당 글을 쓴 사람의 "프로필 이미지"도 보이도록 만드는 것을 권장

---

## 방명록 작성

**POST** `/planets/:username/guestbook`

### Headers
- Authorization: Bearer {TOKEN}

### Request
```json
{
  "content": "안녕히 계세요"
}
```

### Response
```json
{
  "isSuccess": true,
  "code": 201,
  "message": "방명록 작성 성공",
  "result": {
    "username": "test3",
    "guestbookId": "방명록ID",
    "content": "안녕하 계세요",
    "writtenAt": "2025-10-22T10:30:00.000Z"
  }
}
```

---

## 즐겨찾기 목록 조회

**GET** `/planets/favorites/me`

### Headers
- Authorization: Bearer {TOKEN}

**참고**: 본인 즐겨찾기 전용

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "즐겨찾기 목록 조회 성공",
  "result": {
    "favorites": [
      {
        "planetId": "행성ID",
        "username": "test3",
        "title": "행성 제목",
        "visitCount": 5,
        "createdAt": "2025-10-21T16:43:42.000Z",
        "profileImageUrl": "https://example.com/profile.jpg",
        "favoritedAt": "2025-10-22T10:30:00.000Z"
      }
    ]
  }
}
```

---

## 즐겨찾기 추가

**POST** `/planets/:username/favorite`

### Headers
- Authorization: Bearer {TOKEN}

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "즐겨찾기 추가 성공"
}
```

---

## 즐겨찾기 삭제

**DELETE** `/planets/:username/favorite`

### Headers
- Authorization: Bearer {TOKEN}

### Response
```json
{
  "isSuccess": true,
  "code": 200,
  "message": "즐겨찾기 삭제 성공"
}
```

---

# 5️⃣ 친구 관리

## 친구 목록 조회

**GET** `/friends`

### Response
```json
{
  "success": true,
  "friends": [
    {
      "id": "96d6eef2-7907-45b5-97d5-c82bc84963b4",
      "created_at": "2025-10-21T17:35:39.000Z",
      "friend_id": "d60758ec-740e-4a7c-a202-d62fc2a59deb",
      "friend_username": "test4",
      "friend_nickname": "tester4",
      "friend_profile_image_url": "url"
    },
    {
      "id": "12d6eef2-7905-45b5-97d5-c82bc84963b4",
      "created_at": "2025-10-21T17:35:39.000Z",
      "friend_id": "d4758ec-740e-4a7c-a202-d62fc2a59deb",
      "friend_username": "test5",
      "friend_nickname": "tester5",
      "friend_profile_image_url": "url"
    }
  ]
}
```

---

## 친구 요청 보내기

**POST** `/friends/request`

### Request
```json
{
  "username": "test1"
}
```

### Response
```json
{
  "success": true,
  "message": "친구 요청 전송 성공",
  "requestId": "리퀘스트 id"
}
```

---

## 받은 친구 요청 목록 조회

**GET** `/friends/received`

### Response
```json
{
  "success": true,
  "requests": [
    {
      "requestId": "c49cbc07-e312-4584-ac7e-81c15c259a5e",
      "requester_id": "abb4a772-1e31-415f-a947-08cad4133fa5",
      "requested_at": "2025-10-21T17:29:27.000Z",
      "username": "test3",
      "nickname": "tester3",
      "profile_image_url": "url"
    }
  ]
}
```

---

## 보낸 친구 요청 목록 조회

**GET** `/friends/sent`

### Response
```json
{
  "success": true,
  "requests": [
    {
      "requestId": "c49e",
      "target_id": "d60",
      "status": "pending",
      "requested_at": "2025-10-21T17:29:27.000Z",
      "responded_at": null,
      "username": "test4",
      "nickname": "tester4",
      "profile_image_url": "https://example.com/profile/test4.jpg"
    }
  ]
}
```

**참고**: `requestId`를 친구 요청 수락/거절 API의 request로 넣으면 됨

---

## 친구 요청 수락

**POST** `/friends/accept`

### Request
```json
{
  "requestId": "요청ID"
}
```

### Response
```json
{
  "success": true,
  "message": "친구 요청 처리 완료"
}
```

---

## 친구 요청 거절

**POST** `/friends/reject`

### Request
```json
{
  "requestId": "요청ID"
}
```

### Response
```json
{
  "success": true,
  "message": "친구 요청 처리 완료"
}
```

---

## 친구 삭제

**DELETE** `/friends`

### Request
```json
{
  "username": "삭제할 친구 username"
}
```

### Response
```json
{
  "success": true,
  "message": "test4을(를) 친구에서 삭제했습니다"
}
```

---

# 6️⃣ 출결 관리

## 출결 검색

**GET** `/search`

### Query Parameter
- `id`: 유저 ID
- `date`: 날짜 (예: 2024-10-20)

**예시**: `/api/search?id=1&date=2024-10-20`

### Response
```json
{
  "id": 1,
  "name": "HongGilDong",
  "grade": 3,
  "attendanceItems": [
    {
      "date": "2025-07-03",
      "status": "ATTEND",
      "reason": "I don't know"
    }
  ]
}
```

### Response 필드

| 필드 | 설명 | 타입 | Nullable | 예시 |
|------|------|------|----------|------|
| userId | ID | String | X | "1" |
| name | 이름 | String | X | "HongGilDong" |
| grade | 학년 | Number | X | 3 |
| attendanceItems | 출결 정보 배열 | Array | X | [{}] |
| date | 출결 날짜 | String | X | "2024-08-08" |
| status | 출결 정보 | String(ENUM) | X | "ATTEND", "ABSENT", "ONLINE", "NONE" |
| reason | 이유 | String | O | - |

---

# 📌 부록

## API 상태 코드

| 코드 | 의미 |
|------|------|
| 200 | 성공 |
| 201 | 생성 성공 |
| 400 | 잘못된 요청 |
| 401 | 인증 필요 |
| 403 | 권한 없음 |
| 404 | 찾을 수 없음 |
| 500 | 서버 오류 |

## 주요 특이사항

### 게임 관련
- 개인 플레이도 DB 구조상 roomId가 필요함
- 멀티 플레이 준비 상태 토글은 팀원만 사용 가능 (호스트 불가)

### 행성 관련
- 본인 행성 방문 시 방문 수 증가하지 않음
- 행성 방문 중복 카운트 안 됨 (같은 사용자의 재방문은 1회로 카운트)
- 행성 목록 조회는 비로그인 상태에서도 접근 가능

### 인증 관련
- JWT 토큰은 로그인 시 발급
- 이후 요청은 `Authorization: Bearer {TOKEN}` 헤더 사용

## 웹소켓 연결 (멀티플레이)

Unity에서 Socket.IO 사용 시 SocketIOUnity 패키지 설치 필요

### 주요 이벤트
- `authenticate`: 인증
- `join_room`: 방 입장
- `leave_room`: 방 퇴장
- `user_joined`: 사용자 입장 알림
- `user_left`: 사용자 퇴장 알림
- `room_updated`: 준비 상태 업데이트
- `game_started`: 게임 시작 알림
- `game_completed`: 게임 완료 알림
- `user_disconnected`: 연결 해제 알림