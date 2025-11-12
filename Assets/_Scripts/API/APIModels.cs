using System;
using System.Collections.Generic;

// ===== User Related =====

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


// ===== Planet Related =====

[Serializable]
public class PlanetData
{
    public string planetId;
    public string ownerUsername;
    public string title;
    public int visitCount;
}

[Serializable]
public class PlanetListItem
{
    public string id;
    public string username;
    public string title;
    public int visit_count;
    public string created_at;
}

[Serializable]
public class PlanetListResponse
{
    public bool isSuccess;
    public int code;
    public string message;
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

// ===== Gallery Related =====

[Serializable]
public class GalleryItem
{
    public string galleryId;
    public string imageId;
    public string title;
    public string image_url;
    public string created_at;
}

[Serializable]
public class GalleryResult
{
    public string username;
    public GalleryItem[] galleries;
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
public class GalleryMetadata
{
    public string[] tags;
    public string generatedAt;
    public bool pollinateApi;
}

[Serializable]
public class GalleryDetailItem
{
    public string username;
    public string imageId;
    public string galleryId;
    public string title;
    public string image_url;
    public GalleryMetadata metadata;
}

[Serializable]
public class GalleryDetailResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public GalleryDetailItem result;
}

// ===== Guestbook Related =====

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

// ===== Friend Related =====

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

// ===== Multiplayer Related =====

[Serializable]
public class ParticipantData
{
    public string userId;
    public string username;
    public int isReady;
    public bool isHost;
}

[Serializable]
public class PlayerData
{
    public string userId;
    public string username;
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
    public string gameCode;
    public string hostId;
    public string hostUsername;
    public List<PlayerData> players;
    public List<ParticipantData> participants;
    public bool isGameStarted;
    public int maxPlayers = 4;

    public string imageUrl;
    public string[] tags;
}

[Serializable]
public class CreateRoomRequest
{
    public string[] tags;
}

[Serializable]
public class CreateRoomResult
{
    public string roomId;
    public string gameCode;
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
    public string gameCode;
}

[Serializable]
public class JoinRoomResult
{
    public int roomId;
    public string gameCode;
    public ParticipantData[] participants;
    public string hostUsername;
    public string imageUrl;
    public string[] tags;
}

[Serializable]
public class JoinRoomResponseWrapper
{
    public bool isSuccess;
    public string code;
    public string message;
    public JoinRoomResult result;
}

[Serializable]
public class ReadyToggleRequest
{
    public string gameCode;
}

[Serializable]
public class ReadyToggleResult
{
    public int isReady;
    public ParticipantData[] participants;
}

[Serializable]
public class ReadyToggleResponseWrapper
{
    public bool isSuccess;
    public string code;
    public string message;
    public ReadyToggleResult result;
}

[Serializable]
public class StartGameRequest
{
    public string gameCode;
}

[Serializable]
public class StartGameResult
{
    public int roomId;
    public string gameCode;
    public string gameStatus;
    public ParticipantData[] participants;
}

[Serializable]
public class StartGameResponseWrapper
{
    public bool isSuccess;
    public string code;
    public string message;
    public StartGameResult result;
}

// ===== WebSocket Event Models =====

// Authentication
[Serializable]
public class WS_AuthenticateRequest
{
    public string token;
}

[Serializable]
public class WS_AuthResponse
{
    public bool isSuccess;
    public string code;
    public string message;
}

// Room Management
[Serializable]
public class WS_JoinRoomRequest
{
    public string gameCode;
}

[Serializable]
public class WS_LeaveRoomRequest
{
    public string gameCode;
}

// User Joined Event
[Serializable]
public class WS_UserJoinedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_UserJoinedResult result;
}

[Serializable]
public class WS_UserJoinedResult
{
    public string userId;
    public string username;
    public string gameCode;
    public ParticipantData[] participants;
}

// User Left Event
[Serializable]
public class WS_UserLeftEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_UserLeftResult result;
}

[Serializable]
public class WS_UserLeftResult
{
    public string userId;
    public string username;
    public string gameCode;
}

// User Disconnected Event
[Serializable]
public class WS_UserDisconnectedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_UserDisconnectedResult result;
}

[Serializable]
public class WS_UserDisconnectedResult
{
    public string userId;
    public string username;
    public bool isHost;
}

// Room Updated Event
[Serializable]
public class WS_RoomUpdatedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_RoomUpdatedResult result;
}

[Serializable]
public class WS_RoomUpdatedResult
{
    public string gameCode;
    public ParticipantData[] participants;
}

// Game Started Event
[Serializable]
public class WS_GameStartedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_GameStartedResult result;
}

[Serializable]
public class WS_GameStartedResult
{
    public string gameId;
    public string gameCode;
    public ParticipantData[] participants;
}

// Game Completed Event
[Serializable]
public class WS_GameCompletedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_GameCompletedResult result;
}

[Serializable]
public class WS_GameCompletedResult
{
    public string gameId;
    public string gameCode;
    public WS_WinnerData winner;
    public string gameStatus;
}

[Serializable]
public class WS_WinnerData
{
    public string userId;
    public string username;
    public int clearTimeMs;
}

// ===== Common Response =====

[Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
}

// ===== Single Game API Models =====

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

// ===== Multiplay Complete Response =====

[Serializable]
public class MultiplayCompleteParticipant
{
    public string userId;
    public string username;
    public bool isWinner;
}

[Serializable]
public class MultiplayCompleteResult
{
    public string gameId;
    public string gameCode;
    public bool isWinner;
    public int totalParticipants;
    public string gameStatus;
    public MultiplayCompleteParticipant[] participants;
}

[Serializable]
public class MultiplayCompleteResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public MultiplayCompleteResult result;
}