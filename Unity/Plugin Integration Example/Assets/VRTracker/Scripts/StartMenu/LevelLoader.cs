using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelLoader : MonoBehaviour {

    public static LevelLoader instance;

    public Slider loadingBar;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one LevelLoader in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            loadingBar.value = progress;

            yield return null;
        }
    }
}
