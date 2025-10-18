using System.Collections.Generic;
using UnityEngine;

public class PlanetDataManager : MonoBehaviour
{
    public static PlanetDataManager Instance { get; private set; }

    [System.Serializable]
    public class GuestbookEntry
    {
        public string author;
        public string content;
        public System.DateTime timestamp;
    }

    public List<GuestbookEntry> guestbookEntries = new List<GuestbookEntry>();

    public void AddGuestbookEntry(string author, string content)
    {
        guestbookEntries.Add(new GuestbookEntry
        {
            author = author,
            content = content,
            timestamp = System.DateTime.Now
        });
    }
    public class PlanetPhotoData
    {
        public string description;
        public string[] tags;

        public PlanetPhotoData(string description, string[] tags)
        {
            this.description = description;
            this.tags = tags;
        }
    }

    private List<PlanetPhotoData> photoList = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPhoto(string description, string[] tags)
    {
        PlanetPhotoData newPhoto = new PlanetPhotoData(description, tags);
        photoList.Add(newPhoto);
    }

    public List<PlanetPhotoData> GetAllPhotos()
    {
        return photoList;
    }

    public PlanetPhotoData GetPhoto(int index)
    {
        if (index >= 0 && index < photoList.Count)
            return photoList[index];
        else
            return null;
    }
}