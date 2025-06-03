using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper; //텍스트 출력 파일
using UnityEngine.InputSystem; //입력 막는거
public class Story_one : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //대사 출력
    public GameObject talkImage1;
    public GameObject talkImage2;
    public Text printText;
    List<List<string>> dialoguescript;//대사 스크립트 저장소


    void Start()
    {

        dialoguescript = new List<List<string>>
        {
        new List<string> { ".....", "머리 아파...", "여기가 어디지?", "안녕", "헉! 넌 모야? 잡아먹지마 난 맛 없단 말이야", "널 잡아 먹을 생각은 없어.. \n 길을 잃은거 같은데 내가 도와줄까? 날 따라 앞으로가자", "일단 앞으로 가자.." },
        new List<string> { "조심해!", "여긴 위험해.", "저기몬스터들을 조심해서 지나가자" , "아님 다른 방법이 있을까?"},
        new List<string> { "저 음표에 손을 대보자!", "(손을 대보자 음표안으로 빨려 들어가 기억이 재생 된다..)" }
        };

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))  // 특정 태그로 확인
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
        GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
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

        //텍스트 출력
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
        // Wait 1 second at the end | 마지막에 2초 추가 대기함
        yield return new WaitForSeconds(1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.3f);

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

        talkImage1.SetActive(false);
        talkImage2.SetActive(false);



        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }


}