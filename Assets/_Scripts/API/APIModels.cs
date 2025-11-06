using System;
using System.Collections.Generic;

// ==========================================
// 유저 관련
// ==========================================

[Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string token;
    public UserData user;
}

[Serializable]
public class SignupRequest
{
    public string username;
    public string password;
    public string nickname;
}

[Serializable]
public class UserData
{
    public string username;
    public string nickname;
}

[Serializable]
public class NicknameUpdateRequest
{
    public string nickname;
}

[Serializable]
public class PasswordUpdateRequest
{
    public string oldPassword;
    public string newPassword;
}

[System.Serializable]
public class UpdateNicknameRequest
{
    public string nickname;
}


[Serializable]
public class CheckUsernameResponse
{
    public bool available;
}

// ==========================================
// 행성 관련
// ==========================================

[Serializable]
public class PlanetData
{
    public string planetId;
    public string ownerUsername;
    public string ownerNickname;
    public int visitCount;
}

[Serializable]
public class PlanetListItem
{
    public string planetId;
    public string ownerUsername;
    public string ownerNickname;
    public int visitCount;
}

[Serializable]
public class PlanetListResponse
{
    public PlanetListItem[] result;
}

[Serializable]
public class PlanetDetail
{
    public string id;
    public string ownerId;
    public string ownerUsername;
    public string title;
    public int visitCount;
    public string createdAt;
    public string profileImageUrl;
    public bool isOwner;
    public bool canEdit;
}

[Serializable]
public class PlanetDetailResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public PlanetDetail result;
}

[Serializable]
public class FavoriteListResponse
{
    public PlanetListItem[] result;
}

[Serializable]
public class VisitResponse
{
    public bool success;
    public string message;
    public int visitCount;
}

// ==========================================
// 갤러리 관련
// ==========================================

[Serializable]
public class GalleryItem
{
    public string imageId;
    public string imageUrl;
    public string description;
    public List<string> tags;
}

[Serializable]
public class GalleryResult
{
    public string username;
    public List<GalleryItem> galleries;
}

[Serializable]
public class GalleryListResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public GalleryResult result;
}

[Serializable]
public class GalleryUploadRequest
{
    public string imageBase64;
    public string description;
    public string[] tags;
}

[Serializable]
public class GalleryUploadResponse
{
    public bool success;
    public string imageId;
    public string message;
}

// ==========================================
// 방명록 관련
// ==========================================

[Serializable]
public class GuestbookEntry
{
    public string id;
    public string content;
    public string authorUsername;
    public string authorProfileImageUrl;
    public string written_at;
}

[Serializable]
public class GuestbookResult
{
    public string username;
    public GuestbookEntry[] guestbooks;
}

[Serializable]
public class GuestbookListResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public GuestbookResult result;
}

[Serializable]
public class GuestbookWriteRequest
{
    public string content;
}

// ==========================================
// 친구 관련
// ==========================================

[Serializable]
public class FriendItem
{
    public string username;
    public string nickname;
    public string planetId;
}

[Serializable]
public class FriendListResponse
{
    public FriendItem[] result;
}

// ==========================================
// 멀티플레이 관련
// ==========================================

[Serializable]
public class PlayerData
{
    public string userId;
    public string nickname;
    public bool isReady;
    public bool isHost;
    public float clearTime = -1f;
    public int rank = 0;
}

[Serializable]
public class RoomData
{
    public string roomId;
    public string sessionCode;
    public string hostId;
    public List<PlayerData> players;
    public bool isGameStarted;
    public int maxPlayers = 4;

    // 추가
    public string imageUrl;
    public string[] tags;
}
[Serializable]
public class CreateRoomRequest
{
    public string[] tags; // 4개 태그 필수
}

[Serializable]
public class CreateRoomResult
{
    public string roomId;
    public string gameCode; // sessionCode
    public string hostUsername;
    public string imageUrl;
    public string[] tags;
}

[Serializable]
public class CreateRoomResponseWrapper
{
    public bool isSuccess;
    public string code;
    public string message;
    public CreateRoomResult result;
}

[Serializable]
public class JoinRoomRequest
{
    public string sessionCode;
}

[Serializable]
public class JoinRoomResponse
{
    public string roomId;
    public RoomData roomData;
}

// ==========================================
// 공통 응답
// ==========================================

[Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
}

// ==========================================
// Single Game API Models
// ==========================================

[Serializable]
public class SingleGameStartRequest
{
    public string[] tags;
}

[Serializable]
public class SingleGameStartResult
{
    public string roomId;
    public string gameCode;
    public string imageUrl;
}

[Serializable]
public class SingleGameStartResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public SingleGameStartResult result;
}

[Serializable]
public class SingleGameCompleteRequest
{
    public string gameCode;
    public string startTime;
    public string endTime;
}

[Serializable]
public class SingleGameCompleteResult
{
    public string gameId;
    public string gameCode;
    public int clearTimeMs;
    public string imageUrl;
    public string gameStatus;
}

[Serializable]
public class SingleGameCompleteResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public SingleGameCompleteResult result;
}

[Serializable]
public class SaveToPlanetRequest
{
    public string gameCode;
    public string title;
}

[Serializable]
public class SaveToPlanetResult
{
    public string planetId;
    public string imageUrl;
    public string galleryTitle;
}

[Serializable]
public class SaveToPlanetResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public SaveToPlanetResult result;
}