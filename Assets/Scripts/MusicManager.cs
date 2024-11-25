using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Static instance to ensure only one MusicManager exists
    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        // If an instance already exists and it's not this, destroy this to enforce singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this and make it persistent
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
