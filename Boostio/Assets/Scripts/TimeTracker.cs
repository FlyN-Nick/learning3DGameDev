using System;
using System.Collections;
using System.Collections.Generic;
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
                //print("TIME TRACKING STOPPING.");
                isTrackingTime = false;
                canvas.SetActive(true);
                GetLeaderboard();
            }
            else if (currentIndex == 1)
            {
                //print("TIME TRACKING STARTING.");
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
        //print("Fetching...");
        Dictionary<string, object> recordData = (await docRef.GetSnapshotAsync()).ToDictionary();
        //print($"Fetched.\nrecordData: {recordData}");
        long timeRecord = (long) recordData["time"];
        //print($"timeRecord: {timeRecord}");
        int flooredTime = (int) Math.Floor(time);
        string message = $"Time to completion: {flooredTime} seconds.";
        if (flooredTime < timeRecord)
        {
            message += $"\nThat's the fastest playthrough EVER.\nThe old record was {timeRecord} seconds.";
            NewRecord(flooredTime);
        }
        else if (flooredTime == timeRecord)
        {
            message += $"\nThat's the same time as the record!";
        }
        else
        {
            message += $"\nThe current playthrough time record is {timeRecord} seconds.";
        }
        messageUGUI.text = message;
        //print($"message: {message}");
    }

    async void NewRecord(int time)
    {
        DocumentReference docRef = db.Collection("data").Document("record");
        Dictionary<string, object> data = new Dictionary<string, object> { { "time", time } };
        await docRef.SetAsync(data);
        //print($"Updated record to: {time} seconds.");
    }

    public void Restart() { SceneManager.LoadScene(0); }
}
