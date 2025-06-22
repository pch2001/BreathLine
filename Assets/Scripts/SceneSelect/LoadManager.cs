using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // ���� ������ ����ϵ��� ����
            PlayerPrefs.SetString("LoadScene", data.sceneName);
            PlayerPrefs.SetFloat("PlayerX", data.playerPosX);
            PlayerPrefs.SetFloat("PlayerY", data.playerPosY);

            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.LogWarning("����� ������ �����ϴ�.");
        }
    }
}
