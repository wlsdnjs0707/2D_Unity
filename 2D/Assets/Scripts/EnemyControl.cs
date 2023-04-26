using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyControl : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject target; // 추적할 타겟 (플레이어)
    private GameObject StageManager;
    private Image healthBarImage;

    // 속성
    private bool isGround = true; // 땅에 닿아있는가
    private bool canTrack = true; // 플레이어를 추적중인 상태인가 (false : 추적중)
    private bool canAttack = true; // 공격 가능한 상태인가 (false : 공격중)
    private bool canJump = true; // 점프 가능한 상태인가 (false : 점프 쿨타임중)

    public float moveSpeed = 5.0f; // 이동 속도
    public float jumpPower = 25.0f; // 점프력
    public float attackRange = 1.5f; // 공격 사거리
    public float attackCoolTime = 1.5f; // 공격 쿨타임

    public float life = 1000.0f; // 체력
    private float life_max; // 최대체력
    public float takeDamage = 10f; // 피격시 받는 데미지

    // Attack Effect 프리팹
    public GameObject effectPrefab_slash;
    public GameObject effectPrefab_hit;

    // 땅 레이어
    [SerializeField]
    private LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        life_max = life;
        StageManager = GameObject.FindWithTag("StageManager");
        target = GameObject.FindWithTag("Player");
        healthBarImage = GameObject.FindWithTag("HealthBar").GetComponent<Image>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 포인트를 기준으로 원을 그려 바닥 레이어에 닿아있는지 확인
        isGround = Physics2D.OverlapCircle(transform.position, 1f, groundLayer);

        // 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, target.transform.position);

        // 바라보는 방향 설정
        if (canTrack == true)
        {
            if (target.transform.position.x <= transform.position.x)
            {
                transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
            }
            else
            {
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
        }

        // [TRACKING & ATTACK]
        // 공격 사거리 안에 없으면 Track
        if (distance >= attackRange)
        {
            if (canTrack == true)
            {
                Tracking();
            }
            
        }
        else // 공격 사거리 내에 들어오면 Attack
        {
            // 공격 가능 여부 체크 후 공격
            if (canAttack == true)
            {
                // 공격 시 1.5초 동안 추적 해제
                canTrack = false;
                Invoke("EnableTracking", 1.5f);

                // 공격 수행
                canAttack = false;
                Attack();
            }
        }

        // [JUMP]
        if (System.Math.Abs(target.transform.position.x - transform.position.x) <= 3.0f)
        {
            // 플레이어가 위에있으면 점프
            if (System.Math.Abs(target.transform.position.y - transform.position.y) >= 1.0f && target.transform.position.y >= transform.position.y)
            {
                if (canTrack == true && canJump == true)
                {
                    canJump = false;
                    Jump();
                }
            }
        }

        // 체력바 UI 업데이트
        healthBarImage.fillAmount = life / life_max;

        // 체력이 0 이하가 되면
        if (life <= 0)
        {
            // 스테이지 매니저의 SetNextStage 함수 호출
            StageManager.GetComponent<StageControl>().SetNextStage();
            Destroy(gameObject);
        }

    }

    void Tracking()
    {
        // 타겟을 향해 이동 (X축)
        this.transform.position = new Vector2(transform.position.x + ((target.transform.position - transform.position).normalized).x * moveSpeed * Time.deltaTime, transform.position.y);
    }

    void Attack()
    {
        StartCoroutine(SlashEffect());
        Invoke("EnableAttack", attackCoolTime);
    }

    void Jump()
    {
        // 캐릭터가 땅에 닿아있을 때만 점프 가능
        if (isGround == true)
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }

        Invoke("EnableJump", 2f);
    }

    IEnumerator SlashEffect()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject slashEffect = Instantiate(effectPrefab_slash, new Vector3(transform.position.x, transform.position.y, -3), transform.rotation);
        slashEffect.transform.localScale = new Vector3(1f, 1f, 1f);

        // 왼쪽 바라볼 때
        if (transform.localScale.x < 0)
        {
            slashEffect.transform.localEulerAngles = new Vector3(0, 0, 270);
        }
        else // 오른쪽 바라볼 때
        {
            slashEffect.transform.localEulerAngles = new Vector3(0, 0, 90);
        }

        Destroy(slashEffect, 0.1f);
    }

    IEnumerator HitEffect()
    {
        yield return null;

        GameObject hitEffect = Instantiate(effectPrefab_hit, transform.position, transform.rotation);
        hitEffect.transform.localScale = new Vector3(1f, 1f, 1f);

        Destroy(hitEffect, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = collision.gameObject;

        if (obj.tag == "Bullet")
        {
            life -= takeDamage;

            // 피격 이펙트 출력
            StartCoroutine(HitEffect());

            // 충돌한 오브젝트 삭제
            Destroy(obj);
        }
    }

    void EnableTracking()
    {
        canTrack = true;
    }

    void EnableAttack()
    {
        canAttack = true;
    }

    void EnableJump()
    {
        canJump = true;
    }

}
