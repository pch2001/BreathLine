using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Story_four : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //��� ���
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;

    public GameObject Boss4;
    public GameObject BossHp;
    public Story_note story4; 

    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����

    private bool isSkipping = false;
    private bool isTyping = false;
    public GameObject spawnThunder;

    public AudioSource bgm;
    public AudioClip audioClip; // ������ ����

    public GameObject telepoint;
    public GameObject player;

    public GameObject UI1;
    public GameObject UI2;
    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
        new List<string>
        {
            "n:(��½�̴� ���� ���� �ֺ��� ������.)",
            "g:�� ������� ����ü �̰� ����?",
            "w:�װ� �ݺ��� ����� �����̾�.\n���� ������ ���� ��ó���� �ֺ����� ���������� ����.",
            "n:(�ֺ��� ������ ������ �� ��ī�ο� �⼼�� �ٰ��´�.)",
            "g:�� ���顦 �� �� �����ϰ� ���� �ƴϿ���.\n��� �ٸ� ������� ������.",
            "w:���� ��ó��, �ݺ��Ǹ� ����� �ٲٰŵ�.",
            "w:� �� ����,\n� �� ħ������,\n�ƴϸ� ���ݰ� ���� ���·� ��Ÿ���⵵ ��.",
            "n:(�ҳడ ���ǽ������� �Ǹ��� �������.)",
            "g:�׳� ���� �� �����ϰ� �;��.\n�̷��� ��� �ϴٰ�, �ƹ��͵� ���� ���� �� ���Ƽ���",
            "n:(���밡 ������ ������ ���� ����.\n�������� �������� ������ �ִ�.)",
            "g:������, �����ƿ�?",
            "w:���ݡ� �̷� ������ �����Ե� ���̱� ��.\n�׷��� ������. ������ �ߵ� �� �־�.",
            "w:�װ� ������� �Դٴ� �͸����ε�, ���� �����.",
            "w:���� �ʴ� �ݺ��� �Ѱ���� �־�.\n�ͼ��� ������ �ٰ��� ������, �̰� ���̶�� ���� �� �־�.",
            "w:������ �װ� ������ �� �־�.\n�ݺ� �ӿ� �ӹ�����, �� ������ ���ư���.",
            "g:��"

        },new List<string>
        {
            "n:(���� ���ڶ�, �μ��� �ٴ� ƴ�� �Ʒ��� ����� ������ �ִ�.)",
            "g:���䡦 �Ʒ��� ��� �̾����� �־��.",
            "n:(����� ����, ���̸� ������ �� ���� ħ��. �������� ���̴�.)",
            "w:�װ��� �� ����� ���� �عٴ��̾�.",
            "w:���ݱ��� ������ ��������, �� �Ʒ����� �� ��ٸ��� ����.",
            "g:�������ϰ� ���� �ʾҴ� �͵��ΰ� ����.",
            "w:������ �ᱹ, �����߸� ��.\n��ó�� �������ٰ� ������� �� �ƴϴϱ�.",
            "n:(�ҳ�� ���ɽ��� �Ǹ��� ���. �ճ��� ���� ������ �ִ�.)",
            "g:�̹����¡� �ֺ��� ��ó�ִ� ������� �Ǹ��� �Ұ� ���� �ʾƿ�.\n�׷��� �װ� �� ���� ���� ���� �־��.",
            "w:��ó�� ��ó�� ���� ��,\n����� ������ ���󵵡� �ᱹ �ʸ� ���Ƹ԰� �� �ž�.",
            "w:�� �ٸ� ����� �˰� ���ݾ�.",
            "n:(����� ������ �ٰ��� ���� ����. �� ������ ������ �����ϴ�.)",
            "g:������, ���� ������?",
            "w:���� �ʴ� ���� �߿��� ������ �տ� �־�.",
            "w:����� ���������, �� ���� �� �������� �״ϱ�.",
            "n:(�ҳ�� ���� �����̸�, ���Ϸ� �������� �߰����� ����´�.)"
        },

        new List<string>
        {
            "n:(�ҳ��� ���� ���� �� �۽ο� ��򰡷� ��������.)",
            "n:(������ ���� �����ϰ� ���� ������ ����. ���տ� �Ŵ��� �׸��ڰ� ����� �巯����.)",
            "g:�����䡦 ����ҡ�?",
            "n:(�տ� �ִ� �Ŵ��� �Ƿ翧�� ���ݾ� �����δ�. ������ �ٸ�, ������ ��, �Ӱ� ������ ��.)",
            "g:�������������",
            "n:(�������� ���ſ�����. ������ �ʾƵ� �������� �йڰ�. �ͼ����� ������, �̻��ϸ���ġ �������� �ʴ�.)",
            "g:(ó�� ���� �����ε����� �� �̷��� ������ �˿� ���� ����?)",
            "g:(�̰ǡ��� �η����ΰ���, �ƴϸ顦��)",
            "n:(�Ź̰� õõ�� ���� ���δ�. �� �ü���, ��ġ �ſ��� ���̴�� �ҳฦ ��մ´�.)",
            "w:�����ڷ� ������. ���� �ʴ�, �� ���縦 �����ϱ⿣ �ʹ� ������ �־�.",
            "g:���������� �̰ǡ���",
            "n:(�ҳ�� �Ǹ��� �� ���, ��� �𸣰� ������ ������ ������ ������.)"
        },
        new List<string>
        {
            "g:���롦?! �� ��, �֡�!",
            "n:(�Ź��� ���� ����� ������ ���� Ÿ�� ������.\n������ ����� ���� ���������.)",

            "g:�֡� �� �� ��š�!\n���� ����, �� �������� �ֵѷȴµ���!",

            "w:�ʸ� ��Ű�� ��,\nó������ ���� ������ ���̾���.",
            "w:�ʿ��� ���� �ʹ� ���ſ� �� ���Ƽ�,\n���� ���� �����ϰ� �־���.",
            "n:(�ҳ��� �ճ��� ������ �̾��� ����\n�׷��� ���� ����� ���� �׳��� ������ ���ϰ� �ִ�.)",
            "n:(�ҳ��� �������� ü������ �ٲ�ϴ�.)",

            "g:���׷�, �� ������ �ʴ� ���� ������� �־��� �žߡ�?",

            "w:�װ� �� �߸��� �ƴϾ�.\n�װ� �������� �ʵ���, ���� �翡 �־��ְ� �;��� ���̾�.",
            "w:�׸��� ����, �װ� �������� �����̶�顦\n���� ���������� �� �����ְ� �;�.",

            "n:(����� �� �� �� ���� ���̽���.\n�� �����ڿ� ��ȸ��, ������ ����.)",

            "w:�� �ݺ��� ���� �װ� ���ߴ� ���� �ƴ϶�,\n�ٽ� ���۵Ǵ� ���̱� �ٶ���.",
            "w:���� �ʶ�顦 �г밡 �ƴ�, �ʸ��� ������� �ο� �� ���� �״ϱ�.",

            "g:������, ��Ź�ؿ�. �̹��� ��, ������ ���Կ�.",

            "w:���ơ� �� ���, ������.",
            "w:��, ���� ���ư�.\n�̹����� ���� ������.",

            "n:(���� �ҳฦ ���ΰ�, �ð��� �帧�� �Ųٷ� �������� �����Ѵ�.)"
        }
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
        if (collider.gameObject.CompareTag("Player"))  // Ư�� �±׷� Ȯ��
        {
            sideImage.SetActive(true);
            UI1.SetActive(false);
            UI2.SetActive(false);
            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                if(index == 2)
                {
                    bgm.clip = audioClip; // ���� ������� ���
                    TeleportPlayer();
                }
                StartCoroutine(TypingText(index));//�̰͸� �����ϸ� �ؽ�Ʈ ����
            }
        }
    }

    public IEnumerator TypingText(int index)
    {
        GameObject playerCode = GameObject.FindWithTag("Player");

        playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        playerCtrl.OnDisable();

        yield return new WaitForSeconds(1f);

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

            // ��ǳ�� �б�
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
        UI1.SetActive(true);
        UI2.SetActive(true);

        if(index == 1)
        {
            spawnThunder.SetActive(false);
        }

        if (index == 2)
        {
            bgm.clip = audioClip;
            bgm.volume = 0.6f;
            bgm.Play();

            Boss4.GetComponent<EnemyBase>().attackMode = true;
            BossHp.SetActive(true);
        }
        else if(index == 3)
        {
            GameManager.Instance.isReturned = true;
            GameManager.Instance.AddPolution(0f); // �ʱ�ȭ

            story4.nextScene(); // ���� ��� ��ũ��Ʈ ��� ��, ���� Scene���� �̵�
        }
        yield return new WaitForSeconds(0.1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }
    void TeleportPlayer()
    {
        if (telepoint != null && player != null)
        {
            player.transform.position = telepoint.transform.position;
        }
    }
}
