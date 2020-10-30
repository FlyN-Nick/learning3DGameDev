using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;

public class TimeTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageUGUI = null;
    [SerializeField] private GameObject canvas = null;

    private int rememberedSceneIndex = 0;
    private int totalNumLevels;
    private bool isTrackingTime = true;
    private float time = 0f;

    private FirebaseFirestore db;

    private void Awake()
    {
        int amount = FindObjectsOfType<TimeTracker>().Length;
        if (amount > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            totalNumLevels = SceneManager.sceneCountInBuildSettings;
            canvas.SetActive(false);
        }
    }

    private void Update()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (rememberedSceneIndex != currentIndex)
        {
            if (currentIndex == totalNumLevels - 1)
            {
                isTrackingTime = false;
                canvas.SetActive(true);
                GetLeaderboard();
            }
            else if (currentIndex == 0)
            {
                messageUGUI.text = "";
                canvas.SetActive(false);
                time = 0;
            }
            else if (currentIndex == 1) { isTrackingTime = true; }
            
            rememberedSceneIndex = currentIndex;
        }
        else if (isTrackingTime) { time += Time.deltaTime; }   
    }

    private bool InternetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }
        else { return true; }
    }

    private async void GetLeaderboard()
    {
        int flooredTime = (int)Math.Floor(time);
        long timeRecord = -1;
        if (InternetAvailable())
        {
            db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("data").Document("record");
            Dictionary<string, object> recordData = (await docRef.GetSnapshotAsync()).ToDictionary();
            timeRecord = (long)recordData["time"];
        }
        CreateMessage(flooredTime, timeRecord);
    }

    private async void CreateMessage(int userTime, long timeRecord = -1)
    {
        string message = $"Time to completion: {userTime} seconds.";
        if (timeRecord == -1)
        {
            message += "\nGG WP :D";
        }
        else
        {
            if (userTime < timeRecord)
            {
                message += $"\nThat's the fastest playthrough EVER.\nThe old record was {timeRecord} seconds.";
                if (InternetAvailable()) { await NewRecord(userTime); }
            }
            else if (userTime == timeRecord)
            {
                message += $"\nThat's the same time as the record!";
            }
            else
            {
                message += $"\nThe current playthrough time record is {timeRecord} seconds.";
            }
        }
        messageUGUI.text = message;
    }

    private Task NewRecord(int time)
    {
        DocumentReference docRef = db.Collection("data").Document("record");
        Dictionary<string, object> data = new Dictionary<string, object> { { "time", time } };
        return docRef.SetAsync(data);
    }

    public void Restart() { SceneManager.LoadScene(0); }
}
