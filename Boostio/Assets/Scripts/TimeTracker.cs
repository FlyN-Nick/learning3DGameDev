using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;

[DisallowMultipleComponent]
public class TimeTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageUGUI = null;
    [SerializeField] private GameObject canvas = null;

    private int rememberedSceneIndex = 0;
    private int totalNumLevels;
    private bool isTrackingTime = true;
    private bool loadedStart = false; 
    private float time = 0f;
    private float[] levelTimes;

    private FirebaseFirestore db;
    private FirebaseFunctions func;

    private List<float> percentiles = new List<float>();

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
            canvas.SetActive(false);
            totalNumLevels = SceneManager.sceneCountInBuildSettings;
            levelTimes = new float[totalNumLevels];
            if (SceneManager.GetActiveScene().buildIndex != 0 && !loadedStart)
            {
                SceneManager.LoadScene(0);
            }
            loadedStart = true;
        }
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        func = FirebaseFunctions.DefaultInstance;
    }

    private async void Update()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (rememberedSceneIndex != currentIndex)
        {
            rememberedSceneIndex = currentIndex;
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
            
            if (currentIndex > 1)
            {
                int adjustedIndex = currentIndex - 2;
                float levelTime;
                if (adjustedIndex == 0)
                {
                    levelTime = time;
                }
                else
                {
                    levelTime = time - levelTimes[adjustedIndex-1];
                }
                levelTimes[adjustedIndex] = levelTime;
                if (InternetAvailable())
                {
                    int levelNum = adjustedIndex + 1;
                    await UploadLevelData(levelTime, levelNum);
                    percentiles.Add(await FetchPercentile(levelTime, levelNum.ToString()));
                }
            }
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
        if (InternetAvailable())
        {
            DocumentReference recordDocRef = db.Collection("recordStats").Document("total");
            /*
            Task<DocumentSnapshot> fetchRecordTask = recordDocRef.GetSnapshotAsync();
            await Task.WhenAll(fetchRecordTask, UploadTotalData(flooredTime));
            Dictionary<string, object> recordData = (await fetchRecordTask).ToDictionary();
            timeRecord = (long) recordData["time"];
            */
            /*
             * The above code is commented out 
             * because I am using firebase cloud functions 
             * to check when a new playthrough time is added and update the record if it's faster.
             * This is problematic because when fetching for the record,
             * I want the old record if the user just set a new record,
             * and therefore I want to first fetch the record, 
             * then push to the database the user's playthrough time.
             */
            double timeRecord = Math.Round((double) (await recordDocRef.GetSnapshotAsync()).ToDictionary()["time"], 2);
            await UploadTotalData(time);
            percentiles.Add(await FetchPercentile(time, "total"));
            CreateMessage(Math.Round(time, 2), timeRecord, percentiles.ToArray());
        }
        else { CreateMessage(Math.Round(time, 2)); }
        percentiles = new List<float>();
    }

    // TODO: display the stats with a nice UI instead of this
    private void CreateMessage(double userTime, double timeRecord, float[] percentiles)
    {
        string message = $"Time to completion: {userTime} seconds.";
        if (userTime < timeRecord)
        {
            message += $"\nThat's the fastest playthrough EVER.\nThe old record was {timeRecord} seconds.";
        }
        else if (userTime == timeRecord)
        {
            message += $"\nThat's the same time as the record!";
        }
        else
        {
            message += $"\nThe current playthrough time record is {timeRecord} seconds.";
        }
        for (int i = 0; i < percentiles.Length; i++)
        {
            if (i == percentiles.Length-1)
            {
                message += $"\n{((int)Math.Round(percentiles[i])).AddSuffix()} percentile overall.";
            }
            else
            {
                message += $"\n{((int)Math.Round(percentiles[i])).AddSuffix()} percentile for level #{i+1}.";
            }
        }
        messageUGUI.text = message;
    }

    private void CreateMessage(double userTime)
    {
        string message = $"Time to completion: {userTime} seconds.\nGG WP :D";
        messageUGUI.text = message;
    }

    private Task UploadLevelData(float time, int levelNum)
    {
        DocumentReference docRef = db.Collection($"levelData/eachLevel/{levelNum}").Document();
        Dictionary<string, object> data = new Dictionary<string, object> { { "time", time } };
        return docRef.SetAsync(data);
    }

    private Task UploadTotalData(float time)
    {
        DocumentReference docRef = db.Collection("levelData/eachLevel/total").Document();
        Dictionary<string, object> data = new Dictionary<string, object> { { "time", time } };
        return docRef.SetAsync(data);
    }

    public void Restart() { SceneManager.LoadScene(0); }

    private Task<float> FetchPercentile(float time, string levelNum)
    {
        var data = new Dictionary<string, object>();
        data["level"] = levelNum;
        data["time"] = time;

        var function = func.GetHttpsCallable("getPercentile");

        return function.CallAsync(data).ContinueWith((task) => {
            var json = JsonConvert.SerializeObject(task.Result.ToDictionary());
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
            return dict["Data"]["percentile"];
         });
    }
}

// Adapted from https://stackoverflow.com/questions/11576886/how-to-convert-object-to-dictionarytkey-tvalue-in-c
public static class ObjectToDictionaryHelper
{
    public static Dictionary<string, object> ToDictionary(this object source)
    {
        return source.ToDictionary<object>();
    }

    public static Dictionary<string, T> ToDictionary<T>(this object source)
    {
        var dictionary = new Dictionary<string, T>();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        {
            AddPropertyToDictionary<T>(property, source, dictionary);
        }
        return dictionary;
    }

    private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
    {
        object value = property.GetValue(source);
        if (value is T) { dictionary.Add(property.Name, (T)value); }
    }
}

// Adapted from https://stackoverflow.com/questions/13627308/add-st-nd-rd-and-th-ordinal-suffix-to-a-number
public static class NumberSuffixHelper
{
    public static string AddSuffix(this int num)
    {
        var lastDig = num % 10;
        var lastTwoDig = num % 100;
        var stringVer = num.ToString();
        if (lastDig == 1 && lastTwoDig != 11) { return stringVer + "st"; }
        else if (lastDig == 2 && lastTwoDig != 12) { return stringVer + "nd"; }
        else if (lastDig == 3 && lastTwoDig != 13) { return stringVer + "rd"; }
        else { return stringVer + "th"; }
    }
}