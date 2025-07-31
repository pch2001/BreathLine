using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelecter : MonoBehaviour
{
    [SerializeField] private GameObject MainSelect;
    [SerializeField] private GameObject StageSelect;
    [SerializeField] private GameObject ExitSelect;

    public void StartNew()
    {
        //SceneManager.LoadScene("Stage01");
        StartCoroutine(PlayClickThenLoad(1));
        //LoadingScene.LoadScene(1);
    }
    public void ChangeScene(int SceneName)
    {
        //SceneManager.LoadScene(SceneName);
        StartCoroutine(PlayClickThenLoad(SceneName));
        //LoadingScene.LoadScene(SceneName);
    }

    public void LoadScene()
    {

    }
    public void SetSelect()
    {
        MainSelect.SetActive(false);
        StageSelect.SetActive(true);
        ExitSelect.SetActive(false);
    }
    public void SetMain()
    {
        MainSelect.SetActive(true);
        StageSelect.SetActive(false);
        ExitSelect.SetActive(true);
    }
    private IEnumerator PlayClickThenLoad(int sceneIndex)
    {
        UISoundManager.Instance.PlayClickSound();
        yield return new WaitForSeconds(UISoundManager.Instance.clickClip.length);
        LoadingScene.LoadScene(sceneIndex);
    }
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
