using System.IO;
using UnityEngine;

[System.Serializable]
public class Config
{
    public string openAiApiKey;
    public string openAiOrgId;
}

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    public Config Config { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadConfig()
    {
        string path = Path.Combine(Application.dataPath, "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Config = JsonUtility.FromJson<Config>(json);
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }
}
