using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_four_R : MonoBehaviour
{
    private PlayerCtrl_R playerCtrl;

    //��� ���
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;


    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����

    private bool isSkipping = false;
    private bool isTyping = false;

    public GameObject telepoint;
    public GameObject player;
    public GameObject Boss4;
    public GameObject Boss4R;
    public GameObject BossHp;
    public GameObject BossRHp;

    public Story_note storyVideo; // ����� Ending ���丮 ������


    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
            new List<string>
            {
                "g:�������... �� ���ƿ��� �� ���� ������.",
                "g:(��������. �׷���, ���� �𸣰� ������ �ʾ�.)",
                "g:�ʴ¡� ��ü ����?",
                "g:(�и� �������� ���ø� ���ѵ�, ����̡� �帴��.)",
                "g:�� �ʸ� ���� ������ ���ñ�?",
                "g:���밡�� �� ���� �̰����� �̲�������, \n������ ������ �� �� ����.",
                "g:������, �� ���� �غ���� �ʾҾ�.",
                "g:�׷����� ������ ���� �ž�.",
                "n:(�ҳ�� �ָ��� �� ��� �� ���� ������ ���ư���.)",
                "n:(����� ���� ��, �׳��� ���Ҹ����� �Ƿ��ϰ� �鸰��.)"
            },
            new List<string>
            {
                "g:��...?",
                "w:������ �ձ� �ϳ� ����� �޾ƺ� �� ���� ������",
                "w:���� ȯ�� ���� �� ���ٱ� �ôٰ� �ʹ� ��� �� �ƴϾ�?",
                "w:���ư��� ���� �ʴ� ��� �ൿ����?",
                "w:���� �ʾ�. �������� �� ���� �� ����. \n�г뵵, ���ĵ�, ��������",
                "w:�ʴ� �ʸ� ����Ʈ���鼭���� ������ ������ �ʾ��ݾ�?",
                "w:�׷� ������ �������� ��� �� �������� �����ϴٴ�..",
                "w:��û������",
                "w:�� ������.",
                "w:�ʴ� ������ ������ �� �˸鼭�� �ܸ�����.",
                "w:�� ���� ���� ���̾�. �׸���, ��� �Ǿ��",
                "w:�ٸ��� ���� �ž�."
             },

            new List<string>
            {
                "w:�ڱ� �ϳ� ����� ���� ���ϴ� ������.",
                "w:���� ��� �� �� ���� �� ���Ҿ�?",
                "w:�װ� ���� �� �г�� �����ۿ� ����.",
                "g:�׷�. ���¡� �� ��ο� ��� ����, ���� ������ ����� ���߾�.",
                "g:���� ������ �𸣴� ���� ƴ�� �� �� �ٱⰡ �� ���ο���.",
                "g:���ϰڴٴ� �� �ϳ��� ��� �� �ǵ��� �� ���ٴ� �� �˾�.",
                "g:������, ���⼭ ���� ���� ����.",
                "g:�����ڶ� �ص� ����."
            },

            new List<string> {
                "n:���� ���� ���� �̿��ߴ� ���� ����� ��� �־���.",
                "n:�߸��Ǿ����� �Ϳ��� ���� �� ��, ���� �� �װ� ���ο��⿡.",
                "n:�׷� ���� ���� ���� �þ� ���� �ٻ���.",
                "n:���� ���� ��� �ǰ� �;���.",
                "n:������ �� �ְ�, �Ⱦ��� �� �ִ� ������ ��� �ǰ� �;���.",
                "n:���� ���̿��Դ� ������ ����, ��������� ���� ���ϸ���",
                "n:������ �׷� ������ �ߴ� �� ����.",
                "n:������ �������� �� ���İ��̾���.",
                "n:������,",
                "g:�� ������, ������ ���� ��Ǯ���� ���� ����.",
                "g:��Ǯ���ϰ� ���� �ʾ�.",
                "n:(�ε巯�� �Ǹ� �Ҹ��� ����� ä��ϴ�.)",
                "n:(����� ������ �Ǻ��� �Ǿ� �������ϴ�.)",
                "n:(����� ���� � �������� �����ϰ� �ֳ���?)"
            }
        };

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameManager.Instance.Pollution = 10f; // ������ 10���� ����
            Debug.Log("������ 10���� ������");
        }

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
        if (collider.gameObject.CompareTag("Player"))  // Ư�� �±׷� Ȯ��
        {
            sideImage.SetActive(true);

            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                if(index == 0)
                {
                    TeleportPlayer();
                }
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

        //Ȯ��
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
        //�ؽ�Ʈ ���
        // ��� ���
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
        // Wait 1 second at the end | �������� 2�� �߰� �����
        yield return new WaitForSeconds(1f);


        //Ȯ��� ī�޶� �ǵ����� 
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

        if (index == 0)
        {
            Boss4.GetComponent<EnemyBase>().attackMode = true;
            BossHp.SetActive(true);
        }
        else if(index == 1)
        {
            Boss4R.GetComponent<EnemyBase>().attackMode = true;
            BossRHp.SetActive(true);

            Boss4.SetActive(false);
        }
        else if (index == 2)
        {
            Boss4R.GetComponent<EnemyBase>().attackMode = true;
        }
        else if (index == 3)
        {
            storyVideo.PlayVideo();
            yield break;
        }
        yield return new WaitForSeconds(0.1f);
        
        playerCtrl.OnEnable();
    }
    void TeleportPlayer()
    {
        if (telepoint != null && player != null)
        {
            player.transform.position = telepoint.transform.position;
        }
    }
}
