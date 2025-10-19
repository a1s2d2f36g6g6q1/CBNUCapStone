using System;
using System.Collections.Generic;

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
    public string currentPassword;
    public string newPassword;
}

[Serializable]
public class PlanetData
{
    public string planetId;
    public string ownerUsername;
    public string ownerNickname;
    public int visitCount;
}

[Serializable]
public class PlanetListResponse
{
    public PlanetListItem[] result;  // ← 이 필드 추가
}
[Serializable]
public class GalleryItem
{
    public string imageId;
    public string imageUrl;
    public string description;
    public List<string> tags;
}

[Serializable]
public class GalleryListResponse
{
    public List<GalleryItem> result;
}

[Serializable]
public class GuestbookEntry
{
    public string id;
    public string author;
    public string content;
    public string createdAt;
}

[Serializable]
public class GuestbookListResponse
{
    public GuestbookEntry[] guestbook;  // ← 배열로 유지
}

[Serializable]
public class GuestbookWriteRequest
{
    public string content;
}

[Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
}
[Serializable]
public class CheckUsernameResponse
{
    public bool available;
}
[Serializable]
public class PlanetDetailResponse
{
    public string planetId;
    public string ownerUsername;
    public string ownerNickname;
    public int visitCount;
    public bool isFavorite;
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
public class FavoriteListResponse
{
    public PlanetListItem[] result;  // ← 이 필드 추가
}

[Serializable]
public class VisitResponse
{
    public bool success;
    public string message;
    public int visitCount;
}
[Serializable]
public class FriendItem
{
    public string username;
    public string nickname;
    public string planetId;  // 나중에 planetId 준비되면 사용
}

[Serializable]
public class FriendListResponse
{
    public FriendItem[] result;
}
[Serializable]
public class GalleryUploadRequest
{
    public string imageBase64;  // Base64 인코딩된 이미지
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