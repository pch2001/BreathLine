using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_two_R : MonoBehaviour
{
    private PlayerCtrl_R playerCtrl;

    //대사 출력
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;


    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;

    public GameObject Boss2;
    public GameObject BossHp;

    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
        new List<string> { "n:이번에도 못 보던 것이 앞으로 막고 있다...",
                            "n:쉽게 보내줄 거 같지 않다."
        },
        new List<string> { "w:1", "g:2" },
        new List<string> { "w:1", "g:2" }
        };

        

    }
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                isSkipping = true;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))  // 특정 태그로 확인
        {
            sideImage.SetActive(true);

            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                StartCoroutine(TypingText(index));
            }
        }
    }

    public IEnumerator TypingText(int index)
    {
        GameObject playerCode = GameObject.FindWithTag("Player");

        playerCtrl = playerCode.GetComponent<PlayerCtrl_R>();
        playerCtrl.OnDisable();

        yield return new WaitForSeconds(0.3f);

        Camera cam = Camera.main;
        float startZoom = cam.orthographicSize;
        float targetZoom = 3f;
        float zoomDuration = 0.5f;
        float elapsed = 0f;

        Transform player = GameObject.FindWithTag("Player").transform;
        Vector3 camStartPos = cam.transform.position;
        Vector3 camTargetPos = new Vector3(player.position.x, player.position.y, cam.transform.position.z);

        //확대
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(camStartPos, camTargetPos, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetZoom;
        cam.transform.position = camTargetPos;
        textbehind.SetActive(true);
        //텍스트 출력
        // 대사 출력
        for (int t = 0; t < dialoguescript[index].Count; t++)
        {
            string line = dialoguescript[index][t];
            string[] parts = line.Split(':');

            if (parts.Length < 2) continue;

            string speaker = parts[0].Trim();
            string dialogue = parts[1].Trim();

            printText1.text = "";
            printText2.text = "";
            printText3.text = "";

            if (line.Contains("n:쉽게 보내줄 거 같지 않다."))
            {
                Boss2.SetActive(true);

            }

            // 말풍선 분기
            if (speaker == "g")
            {
                girlImage.SetActive(true);
                wolfImage.SetActive(false);
            }
            else if (speaker == "w")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(true);
            }
            else if (speaker == "n")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(false);
            }

            int length = dialogue.GetTypingLength();
            isTyping = true;
            isSkipping = false;

            for (int i = 0; i <= length; i++)
            {
                if (isSkipping)
                {
                    if (speaker == "g")
                        printText1.text = dialogue;
                    else if (speaker == "w")
                        printText2.text = dialogue;
                    else if (speaker == "n")
                        printText3.text = dialogue;
                    break;
                }
                if (speaker == "g")
                    printText1.text = dialogue.Typing(i);
                else if (speaker == "w")
                    printText2.text = dialogue.Typing(i);
                else if (speaker == "n")
                    printText3.text = dialogue.Typing(i);

                yield return new WaitForSeconds(0.03f);
            }
            isTyping = false;
            isSkipping = false;
            while (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                yield return null;
            }
        }
        // Wait 1 second at the end | 마지막에 2초 추가 대기함
        yield return new WaitForSeconds(1f);


        //확대된 카메라 되돌리기 
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(targetZoom, startZoom, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(camTargetPos, camStartPos, elapsed / zoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = startZoom;
        cam.transform.position = camStartPos;

        textbehind.SetActive(false);
        sideImage.SetActive(false);
        girlImage.SetActive(false);
        wolfImage.SetActive(false);

        Boss2.GetComponent<EnemyBase>().attackMode = true;
        BossHp.SetActive(true);
        playerCtrl.OnEnable();


        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }
}
