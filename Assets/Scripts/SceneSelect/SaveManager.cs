using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public GameObject player;

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        data.playerPosX = player.transform.position.x;
        data.playerPosY = player.transform.position.y;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);

        Debug.Log(Application.persistentDataPath);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
