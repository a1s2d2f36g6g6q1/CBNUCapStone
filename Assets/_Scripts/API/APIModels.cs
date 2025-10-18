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
    public List<PlanetData> planets;
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
    public List<GuestbookEntry> guestbook;
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