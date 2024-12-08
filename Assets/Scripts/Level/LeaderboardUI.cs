using UnityEngine;
using UnityEngine.UI; // or using TMPro if using TextMeshPro

public class LeaderboardUI : MonoBehaviour
{
    // Assuming you have Text fields or TMP_Text fields for name and time
    public TMPro.TextMeshProUGUI T_Name;
    public TMPro.TextMeshProUGUI T_Time;

    // void Awake()
    // {
    //     DontDestroyOnLoad(this.gameObject);
    // }


    // Example method for updating the Freerun leaderboard entry
    public void UpdateFreerunLeaderboard(float bestDistance)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" };
        string randomWord = namePool[Random.Range(0, namePool.Length)];
        int randomNum = Random.Range(1, 1000);
        string username = randomWord + randomNum; // e.g., "MonkeyBlues317"

        T_Name.text = username; // Or a saved player name
        T_Time.text = bestDistance.ToString("F2");
    }
}
