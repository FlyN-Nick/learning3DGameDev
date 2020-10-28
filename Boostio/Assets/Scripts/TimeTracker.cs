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
    [SerializeField] private TextMeshProUGUI messageUGUI;
    [SerializeField] private GameObject canvas;

    private int rememberedSceneIndex = 0;
    private int totalNumLevels;
    private bool isTrackingTime = true;
    private float time = 0f;

    private FirebaseFirestore db;

    void Awake()
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

    void Update()
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
            else if (currentIndex == 1)
            {
                canvas.SetActive(false);
                time = 0;
                isTrackingTime = true;
            }
            else if (currentIndex == 0)
            {
                canvas.SetActive(false);
            }
            rememberedSceneIndex = currentIndex;
        }
        else if (isTrackingTime) { time += Time.deltaTime; }   
    }

    async void GetLeaderboard()
    {
        db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("data").Document("record");
        Dictionary<string, object> recordData = (await docRef.GetSnapshotAsync()).ToDictionary();
        long timeRecord = (long) recordData["time"];
        int flooredTime = (int) Math.Floor(time);
        CreateMessage(flooredTime, timeRecord);
    }

    async void CreateMessage(int userTime, long timeRecord)
    {
        string message = $"Time to completion: {userTime} seconds.";
        if (userTime < timeRecord)
        {
            message += $"\nThat's the fastest playthrough EVER.\nThe old record was {timeRecord} seconds.";
            await NewRecord(userTime);
        }
        else if (userTime == timeRecord)
        {
            message += $"\nThat's the same time as the record!";
        }
        else
        {
            message += $"\nThe current playthrough time record is {timeRecord} seconds.";
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
