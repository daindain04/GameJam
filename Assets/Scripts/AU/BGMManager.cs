using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource audioSource;
    public AudioClip mainBGM;
    public AudioClip artBGM;
    public AudioClip techBGM;
    public AudioClip planBGM;

    private string currentSceneName = "";

    private HashSet<string> sharedScenes = new HashSet<string> { "StartScene", "MainScene", "ResultScene" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSource가 없으면 자동 추가
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.playOnAwake = false;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        PlaySceneBGM(currentSceneName);
    }

    void PlaySceneBGM(string sceneName)
    {
        AudioClip newClip = null;

        if (sharedScenes.Contains(sceneName))
        {
            newClip = mainBGM;
        }
        else
        {
            switch (sceneName)
            {
                case "ArtScene":
                    newClip = artBGM;
                    break;
                case "TechScene":
                    newClip = techBGM;
                    break;
                case "PlanScene":
                    newClip = planBGM;
                    break;
                default:
                    newClip = null;
                    break;
            }
        }

        // 같은 음악이면 재생 안 함
        if (audioSource.clip == newClip) return;

        audioSource.clip = newClip;
        audioSource.Play();
    }
}
