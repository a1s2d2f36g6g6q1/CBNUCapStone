using System;
using System.Collections.Generic;

// ==========================================
// Common Response Wrapper
// ==========================================

[Serializable]
public class ApiResponse<T>
{
    public bool isSuccess;
    public string code;
    public string message;
    public T result;
}

[Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
}

// ==========================================
// User Management
// ==========================================

[Serializable]
public class LoginRequest
{
    public string userId;  // API spec uses "userId", not "username"
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string token;
}

[Serializable]
public class SignupRequest
{
    public string username;
    public string password;
    public string nickname;
}

[Serializable]
public class SignupResponse
{
    public bool success;  // API returns Korean key "성공" but we'll handle as "success"
    public int userId;
}

[Serializable]
public class CheckUsernameResponse
{
    public bool available;
}

[Serializable]
public class ProfileResult
{
    public string nickname;
    public string profileImageUrl;
}

[Serializable]
public class ProfileResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public ProfileResult result;
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

// ==========================================
// Single Play Game
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
    public string startTime;  // ISO 8601 format
    public string endTime;    // ISO 8601 format
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
    public string title;  // Optional
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

// ==========================================
// Multiplayer Game
// ==========================================

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
public class CreateRoomResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public CreateRoomResult result;
}

[Serializable]
public class JoinRoomRequest
{
    public string gameCode;  // API spec uses "gameCode", not "sessionCode"
}

[Serializable]
public class ParticipantData
{
    public int userId;
    public string nickname;
    public int isReady;  // 0 or 1
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
public class JoinRoomResponse
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
public class ReadyParticipant
{
    public string userId;
    public string username;
    public int isReady;
}

[Serializable]
public class ReadyToggleResult
{
    public int isReady;
    public ReadyParticipant[] participants;
}

[Serializable]
public class ReadyToggleResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public ReadyToggleResult result;
}

[Serializable]
public class GameStartRequest
{
    public string gameCode;
}

[Serializable]
public class GameStartParticipant
{
    public int userId;
    public string username;
    public bool isReady;
    public bool isHost;
}

[Serializable]
public class GameStartResult
{
    public int roomId;
    public string gameCode;
    public string gameStatus;
    public GameStartParticipant[] participants;
}

[Serializable]
public class GameStartResponse
{
    public bool isSuccess;
    public string code;
    public string message;
    public GameStartResult result;
}

// Legacy multiplayer classes (for compatibility)
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
    public string sessionCode;  // Note: API uses "gameCode" but keeping this for internal use
    public string hostId;
    public List<PlayerData> players;
    public bool isGameStarted;
    public int maxPlayers = 4;
}

// ==========================================
// Planet Management
// ==========================================

[Serializable]
public class PlanetListItem
{
    public string planetId;
    public string username;
    public string title;
    public int visitCount;
    public string createdAt;
    public string profileImageUrl;
}

[Serializable]
public class PlanetListResult
{
    public PlanetListItem[] planets;
}

[Serializable]
public class PlanetListResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public PlanetListResult result;
}

[Serializable]
public class PlanetDetail
{
    public string id;
    public string ownerId;
    public string username;
    public string title;
    public int visitCount;
    public string createdAt;
    public string profileImageUrl;
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
public class PlanetUpdateRequest
{
    public string title;          // Optional
    public string profileImage;   // Optional
}

[Serializable]
public class VisitResult
{
    public string username;
    public int visitCount;
}

[Serializable]
public class VisitResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public VisitResult result;
}

// ==========================================
// Gallery Management
// ==========================================

[Serializable]
public class GalleryItem
{
    public int galleryId;
    public string title;
    public string image_url;    // Note: API uses snake_case
    public string created_at;   // Note: API uses snake_case
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
public class GalleryDetailResult
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
    public GalleryDetailResult result;
}

// ==========================================
// Guestbook Management
// ==========================================

[Serializable]
public class GuestbookEntry
{
    public string id;
    public string content;
    public string authorUsername;
    public string authorProfileImageUrl;
    public string written_at;  // Note: API uses snake_case
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

[Serializable]
public class GuestbookWriteResult
{
    public string username;
    public string guestbookId;
    public string content;
    public string writtenAt;
}

[Serializable]
public class GuestbookWriteResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public GuestbookWriteResult result;
}

// ==========================================
// Favorite Management
// ==========================================

[Serializable]
public class FavoriteItem
{
    public string planetId;
    public string username;
    public string title;
    public int visitCount;
    public string createdAt;
    public string profileImageUrl;
    public string favoritedAt;
}

[Serializable]
public class FavoriteListResult
{
    public FavoriteItem[] favorites;
}

[Serializable]
public class FavoriteListResponse
{
    public bool isSuccess;
    public int code;
    public string message;
    public FavoriteListResult result;
}

// ==========================================
// Friend Management
// ==========================================

[Serializable]
public class FriendItem
{
    public string id;
    public string created_at;
    public string friend_id;
    public string friend_username;
    public string friend_nickname;
    public string friend_profile_image_url;
}

[Serializable]
public class FriendListResponse
{
    public bool success;
    public FriendItem[] friends;
}

[Serializable]
public class FriendRequestSendRequest
{
    public string username;
}

[Serializable]
public class FriendRequestSendResponse
{
    public bool success;
    public string message;
    public string requestId;
}

[Serializable]
public class ReceivedFriendRequest
{
    public string requestId;
    public string requester_id;
    public string requested_at;
    public string username;
    public string nickname;
    public string profile_image_url;
}

[Serializable]
public class ReceivedFriendRequestsResponse
{
    public bool success;
    public ReceivedFriendRequest[] requests;
}

[Serializable]
public class SentFriendRequest
{
    public string requestId;
    public string target_id;
    public string status;
    public string requested_at;
    public string responded_at;
    public string username;
    public string nickname;
    public string profile_image_url;
}

[Serializable]
public class SentFriendRequestsResponse
{
    public bool success;
    public SentFriendRequest[] requests;
}

[Serializable]
public class FriendRequestActionRequest
{
    public string requestId;
}

[Serializable]
public class FriendRequestActionResponse
{
    public bool success;
    public string message;
}

[Serializable]
public class FriendDeleteRequest
{
    public string username;
}

[Serializable]
public class FriendDeleteResponse
{
    public bool success;
    public string message;
}