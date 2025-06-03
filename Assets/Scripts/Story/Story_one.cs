using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper; //�ؽ�Ʈ ��� ����
using UnityEngine.InputSystem; //�Է� ���°�
public class Story_one : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //��� ���
    public GameObject talkImage1;
    public GameObject talkImage2;
    public Text printText;
    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����


    void Start()
    {

        dialoguescript = new List<List<string>>
        {
        new List<string> { ".....", "�Ӹ� ����...", "���Ⱑ �����?", "�ȳ�", "��! �� ���? ��Ƹ����� �� �� ���� ���̾�", "�� ��� ���� ������ ����.. \n ���� ������ ������ ���� �����ٱ�? �� ���� �����ΰ���", "�ϴ� ������ ����.." },
        new List<string> { "������!", "���� ������.", "������͵��� �����ؼ� ��������" , "�ƴ� �ٸ� ����� ������?"},
        new List<string> { "�� ��ǥ�� ���� �뺸��!", "(���� �뺸�� ��ǥ������ ���� �� ����� ��� �ȴ�..)" }
        };

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))  // Ư�� �±׷� Ȯ��
        {
            talkImage1.SetActive(true);
            talkImage2.SetActive(true);

            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                StartCoroutine(TypingText(index));
            }
        }
    }

    public IEnumerator TypingText(int index)
    {
        GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
        playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        playerCtrl.OnDisable();


        yield return new WaitForSeconds(0.3f);

        printText.text = "";

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

        //�ؽ�Ʈ ���
        for (int t = 0; t < dialoguescript[index].Count; t++)
        {
            int strTypingLength = dialoguescript[index][t].GetTypingLength();
            for (int i = 0; i <= strTypingLength; i++)
            {
                Debug.Log(dialoguescript[index][t]);
                printText.text = dialoguescript[index][t].Typing(i);
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(2f);
        }
        // Wait 1 second at the end | �������� 2�� �߰� �����
        yield return new WaitForSeconds(1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.3f);

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

        talkImage1.SetActive(false);
        talkImage2.SetActive(false);



        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }


}