using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    protected Rigidbody2D rigidBody;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;
    protected AudioSource audioSource;
    protected GameObject player; // 플레이어 오브젝트 확인용

    private Color originalColor; // 현재 SpriteRender 색상 저장
    private Color currentColor;

    public GameObject hitEffect; // 피격 이펙트
    public GameObject dieEffect; // Die 이펙트
    public GameObject enemyFadeEffect; // 사라질 때 이펙트
    public GameObject echoGuardEffect; // 에코가드 성공 이펙트
    [SerializeField] private Transform enemyHpGauge; // 적 Hp(오염도) UI

    public float maxHp; // 적 최대 HP (최대 오염도)
    [SerializeField] private float _currentHp;
    public float currentHp // 현재 적 HP (현재 오염도)
    {
        get => _currentHp;
        set
        {
            _currentHp = Mathf.Clamp(value, 0f, maxHp);
            UpdateHpGauge(); // hp 게이지 업데이트
        }
    }

    public float defaultMoveSpeed; // 적 기본 이동속도
    [SerializeField] private float _moveSpeed;
    public float moveSpeed // 적 현재 이동속도
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Clamp(value, 0f, defaultMoveSpeed);
    }

    public float decreaseHpSpeed; // 적 HP(오염도) 감소 속도
    public float damage; // 적 공격력
    public float pollutionDegree; // 처치시 오염도 오르는 정도
    public float pollutionResist = 1; // 오염도 감소 비율
    public int maxGroggyCnt; // 최대 그로기 게이지 개수
    public int currentGroggyCnt; // 현재 그로기 게이지 개수
    public EnemyGroggyUI groggyUI; // 그로기 UI 오브젝트
    public Coroutine attackCoroutine; // 스턴, 사망시 코루틴 중간 탈출

    public bool attackMode = false; // 적 공격 상태 여부(경계 <-> 추격)
    public bool isStune = false; // 스턴 상태 여부
    public bool isDead = false; // 죽음 여부
    public bool enemyIsReturn; // // 적 회귀 상태 설정
    public bool isAttacking = false; // 공격 간격 제한 변수

    public bool isPurifying = false; // 정화 중인지(늑대 등장, 정화의 걸음)
    public bool isReadyPeaceMelody = false; // 평화의 멜로디 준비중인지 (준비파동 계산)

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        groggyUI = GetComponentInChildren<EnemyGroggyUI>();
        player = GameObject.FindGameObjectWithTag("Player");

        originalColor = spriteRenderer.color;
    }

    public virtual IEnumerator Stunned(float delay) // 적 기절 반응 구현
    {
        moveSpeed = 0;
        isStune = true; // 잠시 스턴 상태
        currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);
        CancelAttack(); // 공격 중인 경우 취소
        animator.SetBool("isRun", false); // 잠시 Idle 모션

        yield return new WaitForSeconds(delay);

        moveSpeed = defaultMoveSpeed; // 이동속도 복구
        isStune = false; // 스턴 상태 해제
        spriteRenderer.color = originalColor; // 색상 복구
    }

    public virtual IEnumerator EnemyFade(float duration) // 평화의 악장으로 적 사라짐 함수
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        isDead = true; // 죽음 상태로 변경
        enemyFadeEffect.SetActive(true);
        defaultMoveSpeed = 0f; // 이동 불가능
        animator.SetBool("isRun", false);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

        gameObject.SetActive(false); // 적 비활성화
    }

    public virtual IEnumerator Damaged() // 피격시 반응 구현
    {
        if (currentHp >= maxHp) yield break; // 사망 이펙트와 충돌하여 hp 증가 방지

        if (GameManager.Instance.isReturned) // 회귀 후 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill_R>().playerDamage;
        else // 회귀 전 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill>().playerDamage;

        Debug.Log("현재 오염도 : " + currentHp);

        if (currentHp < maxHp) // 피격 반응
        {
            Debug.Log("적이 공격으로 인해 피해를 입습니다.");
            StartCoroutine(Stunned(0.7f)); // 0.7초 경직
            animator.SetTrigger("Damaged"); // 피격 애니메이션 실행
            hitEffect.SetActive(true); // 피격 이펙트 활성화

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // 피격 이펙트 비활성화
        }
        else // 사망 반응 
        {
            Debug.Log("적이 고통스럽게 소멸합니다...");
            isDead = true;
            moveSpeed = 0f;
            animator.SetTrigger("Die");
            dieEffect.SetActive(true);
            GameManager.Instance.AddPolution(pollutionDegree);
            yield return new WaitForSeconds(0.5f);

            dieEffect.SetActive(false);
            gameObject.SetActive(false); // 적 비활성화
        }
    }

    public virtual void PushBack(float dir) // 밀격 반응 구현
    {
        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 650);
        else
            rigidBody.AddForce(Vector2.left * 650); // 뒤로 일정 거리 밀격

        StartCoroutine(Stunned(5f)); // 5초간 기절

        if (GameManager.Instance.isReturned) // 늑대의 밀격이 아닌 에코가드로 인한 그로기 밀격일 때 추가기능 
        {
            groggyUI.ResetGroggyState(); // 그로기 내용 초기화
            currentGroggyCnt = 0;
        }
    }

    public virtual void EchoGuardPushBack(float dir) // 에코가드 밀격 반응 구현
    {
        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 450);
        else
            rigidBody.AddForce(Vector2.left * 450); // 뒤로 일정 거리 밀격

        StartCoroutine(Stunned(1.5f)); // 1.5초간 기절
    }

    private void UpdateHpGauge() // 적 hp(오염도) 게이지 업데이트
    {
        float hpRatio = currentHp / maxHp;
        enemyHpGauge.localScale = new Vector2(hpRatio, enemyHpGauge.localScale.y);

    }

    private void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine); 
            hitEffect.SetActive(false);
            attackCoroutine = null;
            isAttacking = false;
            Debug.LogWarning("공격 강제 종료!");
        }
    }

    protected abstract void HandlerTriggerEnter(Collider2D collision); // 충돌시 범위 주변(Enter) 담당 함수
    protected abstract void HandlerTriggerStay(Collider2D collision); // 충돌시 범위 내(Stay) 담당 함수

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return; // 사망시 충돌체크 X

        HandlerTriggerEnter(collision); // 구체적인 충돌 처리과정은 자식 스크립트에게 맡김!
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;

        HandlerTriggerStay(collision); // 위에와 동일하지만 이때는 범위 오브젝트 생성시 적이 이미 범위 내부에 있을 경우 (내용 동일)
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            Debug.Log("적이 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
        }
    }
}
