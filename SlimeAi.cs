using UnityEngine;
using System.Collections;
using Cinemachine;
public class SlimeAi : EnemyAI
{
    [SerializeField] private float waitTime;
    [SerializeField] private float chaseTime;
    [SerializeField] private bool isChasing;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float chompForce;
    [SerializeField] private Vector3 jumpDirection;
    [SerializeField] private float jumpForce;
    [SerializeField] private float diveForce;
    [SerializeField] private float landingPause;
    [SerializeField] private float diveOffset;
    [SerializeField] private bool isDiving = false;
    [SerializeField] private float spinForce;
    [SerializeField] private float spinFriction;
    [SerializeField] private float spinBounciness;
    [SerializeField] private float spinChargeTimer;
    [SerializeField] private PhysicsMaterial2D spinMaterial;
    [SerializeField] private PhysicsMaterial2D defaultMaterial;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private FadeManager fade;
    [SerializeField] private float deathDuration;
    [SerializeField] private SlimeSoundManager soundManager;
    [SerializeField] private int previousChoice;
    [SerializeField] private bool isFirstAttack;
    private float defaultGrav;
    private float defaultBounciness;
    private float defaultFriction;
    private bool isWaiting;
    public bool desperationMode = false;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        currentState = State.Chase;
        staggerState = false;
        isFacingRight = false;
        isAttacking = false;
        defaultGrav = rb.gravityScale;
        isFirstAttack = true;
    }
    void FixedUpdate()
    {
        if (!isAttacking)
        {
        switch (currentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Hurt: UpdateHurt(); break;
            case State.Dying: break;
            case State.Midair: UpdateMidair(); break;
        }
        }
    }
    protected override void ChangeState(State newState)
    {
        if(newState != State.Hurt){
        currentState = newState;
        }
        switch (newState)
        {
            case State.Idle: 
            animator.Play("Idle");
            break;
            case State.Chase: 
                animator.Play("Run"); 
                break;
            case State.Attack: 
                exclusiveAction = StartCoroutine(Attack());
                break;
            case State.Hurt: break;
            case State.Dying: if(isAttacking || isChasing || isWaiting) {StopCoroutine(exclusiveAction);} break;
            case State.Midair: animator.Play("Jump"); break;

        }
    }
    public override void UpdateIdle()
    {
        if(!isWaiting){
        exclusiveAction = StartCoroutine(IdleRoutine());
        }
    }

    public IEnumerator IdleRoutine(){
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        isWaiting = false;
        ChangeState(State.Chase);
    }
    public override void UpdateChase()
    {
        if(!isChasing){
        exclusiveAction = StartCoroutine(ChaseTimer());
        }
        float xdist = transform.position.x - player.position.x;
        bool shouldFaceRight = xdist <= 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0,180,0);
        }
        if(DistanceToPlayer() < attackRange)
        {
            StopCoroutine(exclusiveAction);
            isChasing = false;
            ChangeState(State.Attack);
            return;
        }
        float step = moveSpeed * Time.deltaTime;
        Vector3 pathToPlayer = Vector3.MoveTowards(transform.position,player.position,step);
        pathToPlayer.y = transform.position.y;
        transform.position = pathToPlayer;
    }

    public IEnumerator ChaseTimer()
    {
        isChasing = true;
        yield return new WaitForSeconds(chaseTime);
        isChasing = false;
        ChangeState(State.Attack);

    }
    
    public void UpdateMidair()
    {
        if(rb.linearVelocity.y < -0.1f && !isDiving)
        {
            Debug.Log("Dive start");
            exclusiveAction = StartCoroutine(DiveAttack());
        }
    }

    public IEnumerator DiveAttack()
    {
        isDiving = true;
        attackHitbox.SetActive(true);
        rb.linearVelocity = Vector2.zero;
        LayerMask floorMask = LayerMask.GetMask("Default");
        RaycastHit2D targetPoint;
        //float defGrav = rb.gravityScale;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(0.5f);

        float xdist = transform.position.x - player.position.x;
        bool shouldFaceRight = xdist <= 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0,180,0);
        }
        targetPoint = Physics2D.Raycast(player.position, -Vector2.up, Mathf.Infinity, floorMask);
        Vector2 target = targetPoint.point;
        Debug.Log($"{target.x}, {target.y}");
        Vector3 diveDirection = new Vector3(target.x + (isFacingRight ? diveOffset : -diveOffset), target.y, transform.position.z) - transform.position;
        rb.AddForce(diveDirection.normalized * diveForce, ForceMode2D.Impulse);
        soundManager.PlayDive();
        //rb.gravityScale = defGrav;
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Floor")
        {
            if(isDiving){
            exclusiveAction = StartCoroutine(Landing());
            }
        }
    }

    public IEnumerator Landing()
    {
        rb.gravityScale = defaultGrav;
        animator.Play("Jump_Landing");
        yield return new WaitForSeconds(landingPause);
        isAttacking = false;
        isDiving = false;
        ChangeState(State.Idle);
    }
    public override IEnumerator Attack()
    {
        isAttacking = true;
        animator.Play("Attack_Windup");
        yield return null;

        int selection = Random.Range(0,3);
        if (!isFirstAttack)
        {
            if(selection == previousChoice)
            {
                selection = (selection+1) % 3;
            }
        }
        else
        {
            isFirstAttack = false;
        }
        previousChoice = selection;
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        switch (selection)
        {
            case 0:
                rb.AddForce(-transform.right * chompForce,ForceMode2D.Impulse);
                animator.Play("Chomp");
                break;
            case 1: 
                animator.Play("Jump_Start");
                rb.AddForce(transform.up*jumpForce,ForceMode2D.Impulse);
                yield return null;

                clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(clipLength);

                attackHitbox.SetActive(false);
                ChangeState(State.Midair);
                isAttacking = false;
                yield break;
            case 2: 
            animator.Play("Spin");
            exclusiveAction = StartCoroutine(SpinAttack());
            yield break;
        }
        yield return null;
        clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        isAttacking = false;
        ChangeState(State.Idle);
    }

    private IEnumerator SpinAttack()
    {
        attackHitbox.SetActive(false);
        yield return new WaitForSeconds(spinChargeTimer);
        
        attackHitbox.SetActive(true);
        rb.sharedMaterial = spinMaterial;
        rb.AddForce(-transform.right * spinForce,ForceMode2D.Impulse);
        soundManager.PlaySpin();
        yield return new WaitForSeconds(0.2f);
        while(Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            yield return null;
        }
 
        rb.sharedMaterial = defaultMaterial;
        isAttacking = false;
        ChangeState(State.Idle);
    }

    public override IEnumerator Die()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0.0f;
        attackHitbox.SetActive(false);
        animator.Play("Death");
        yield return new WaitForSeconds(0.5f);
        fade.FadeOut(deathDuration,"Ending1");
        yield return null;
    }
}