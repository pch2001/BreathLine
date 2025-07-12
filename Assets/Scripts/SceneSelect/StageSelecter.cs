using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelecter : MonoBehaviour
{
    [SerializeField] private GameObject MainSelect;
    [SerializeField] private GameObject StageSelect;


    public void StartNew()
    {
        //SceneManager.LoadScene("Stage01");
        LoadingScene.LoadScene("Stage01");
    }
    public void ChangeScene(string SceneName)
    {
        //SceneManager.LoadScene(SceneName);
        LoadingScene.LoadScene(SceneName);
    }

    public void LoadScene()
    {

    }
    public void SetSelect()
    {
        MainSelect.SetActive(false);
        StageSelect.SetActive(true);
    }
    public void SetMain()
    {
        MainSelect.SetActive(true);
        StageSelect.SetActive(false);
    }
}
