 using UnityEngine;
using UnityEngine.InputSystem;

// 代码统一管理动作状态；物理移动不受攻击动画锁定。
[RequireComponent(typeof(Rigidbody2D))]
public class DafeiyuMovement : MonoBehaviour
{
    private enum Motion { Idle, Walk, Run, Attack, Jump, ClimbUp, ClimbDown }

    [Header("组件")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool startFacingLeft;

    [Header("移动：A/D 跑步，Shift 慢走")]
    [SerializeField, Min(0f)] private float walkSpeed = 2f;
    [SerializeField, Min(0f)] private float runSpeed = 5f;
    [SerializeField, Min(1f)] private float groundAcceleration = 100f;
    [SerializeField, Min(1f)] private float airAcceleration = 80f;
    [SerializeField, Min(1f)] private float braking = 140f;
    [SerializeField, Min(1f)] private float turnAcceleration = 180f;

    [Header("跳跃：空格，轻点低跳、按住高跳")]
    [SerializeField, Min(0.1f)] private float jumpSpeed = 9f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Min(0.01f)] private float groundProbeDistance = 0.06f;
    [SerializeField, Range(0f, 0.25f)] private float coyoteTime = 0.1f;
    [SerializeField, Range(0f, 0.25f)] private float jumpBufferTime = 0.12f;
    [SerializeField, Range(0.1f, 0.9f)] private float jumpCutMultiplier = 0.45f;
    [SerializeField, Min(0.1f)] private float riseGravityScale = 2f;
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.65f;
    [SerializeField, Min(1f)] private float releasedGravityMultiplier = 2f;
    [SerializeField, Min(1f)] private float maxFallSpeed = 22f;

    [Header("攻击：J，允许移动和空中攻击")]
    [Tooltip("完整攻击动画的播放倍率；不会按固定秒数提前截断。")]
    [SerializeField, Min(0.1f)] private float attackPlaybackSpeed = 1.5f;
    [SerializeField, Range(0f, 0.3f)] private float attackBufferTime = 0.12f;
    [SerializeField, Min(0f)] private float attackRecovery = 0.04f;
    [SerializeField, Range(0f, 1f)] private float attackActiveStart = 0.25f;
    [SerializeField, Range(0f, 1f)] private float attackActiveEnd = 0.65f;

    [Header("动画")]
    [SerializeField, Min(0.01f)] private float landingDuration = 0.1f;

    [Header("爬墙:w上爬。s下爬")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField,Min(0.12f)] private float climbSpeed=3f;
    [SerializeField,Min(0.01f)] private float wallCheckDistance=0.08f;
    [Tooltip("只调整爬墙动画播放倍率，不改变角色上下移动速度。")]
    [SerializeField, Min(0.1f)] private float climbAnimationSpeed = 2f;
    [Tooltip("碰墙后，用多长时间从跳跃速度过渡到爬墙速度。")]
    [SerializeField, Range(0.02f, 0.2f)]
    private float wallGrabBlendTime = 0.1f;

    [Tooltip("蹬墙后，短时间保留向外的速度。")]
    [SerializeField, Range(0.05f, 0.2f)]
    private float wallJumpControlTime = 0.12f;

    private float wallGrabStartedAt;
    private float wallGrabStartVelocityY;

    private float wallJumpControlUntil = float.NegativeInfinity;
    private int wallJumpAwaySide;

    private float stunnedUntil;


    private  int climbInput;
    private int queuedClimbInput;
    private float climbInputBufferedUntil = float.NegativeInfinity;
    private bool isClimbing;
    private int wallSide;
    private float wallGrabBlockedUtil;
    private Motion climbMotion = Motion.ClimbUp;
    private float climbAnimationPhase;
    private readonly float[,] climbLengths = { { 3f, 3f }, { 3f, 3f } };
    private readonly int[,] climbFrameCounts = { { 36, 36 }, { 36, 36 } };
    private ContactFilter2D wallFilter;
    private readonly RaycastHit2D[] wallHits=new RaycastHit2D[8]; 

    private static readonly int ModeId = Animator.StringToHash("Mode");
    private static readonly int FacingId = Animator.StringToHash("Facing");
    private static readonly string[] RightNames =
        { "idle", "walkright", "runright", "attackright", "jumpright", "wallrightup", "wallrightdown" };
    private static readonly string[] LeftNames =
        { "idleleft", "walkleft", "runleft", "attackleft", "jumpleft", "wallleftup", "wallleftdown" };
    private readonly int[,] states = new int[2, 7];
    private readonly float[] attackLengths = { 0.8333333f, 0.8333333f };
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];
    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private PhysicsMaterial2D originalMaterial;
    private PhysicsMaterial2D movementMaterial;
    private bool ownsMovementMaterial;
    private ContactFilter2D groundFilter;
    private float originalGravityScale;
    private bool ownsGravity;
    private int moveInput;
    private int facing;
    private int attackFacing;
    private bool slowWalk;
    private bool jumpHeld;
    private bool cutJumpRequested;
    private bool grounded;
    private bool isAttacking;
    private bool hasPhysicsSample;
    private float jumpBufferedUntil = float.NegativeInfinity;
    private float coyoteUntil = float.NegativeInfinity;
    private float ignoreGroundUntil = float.NegativeInfinity;
    private float attackBufferedUntil = float.NegativeInfinity;
    private float attackStartedAt;
    private float attackDuration;
    private float nextAttackAt;
    private float landingUntil = float.NegativeInfinity;
    private float airborneTime;
    private int visualState;
    private Motion visualMotion;

    public bool IsGrounded => grounded;
    public bool IsAttacking => isAttacking;
    public int Facing => facing;
    public int AttackFacing => attackFacing;
    public bool IsAttackActive => isAttacking &&
        AttackProgress >= attackActiveStart && AttackProgress <= attackActiveEnd;
    private float AttackProgress => Mathf.Clamp01((Time.time - attackStartedAt) / attackDuration);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        foreach (Collider2D candidate in GetComponents<Collider2D>())
            if (candidate.enabled && !candidate.isTrigger) { bodyCollider = candidate; break; }
        if (bodyCollider == null)
        {
            Debug.LogError("大肥鱼需要一个非 Trigger 的 Collider2D 来检测落地。", this);
            enabled = false;
            return;
        }
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Ground");
        // 优先使用项目已有的 Wall 层；旧项目没有该层时再沿用地面层。
        if (wallLayer.value == 0)
        {
            wallLayer = LayerMask.GetMask("Wall");
            if (wallLayer.value == 0) wallLayer = groundLayer;
        }
        groundFilter = new ContactFilter2D { useTriggers = false };
        groundFilter.SetLayerMask(groundLayer.value | wallLayer.value);
        wallFilter = new ContactFilter2D { useTriggers = false };
        wallFilter.SetLayerMask(wallLayer);
        facing = startFacingLeft ? -1 : 1;
        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CacheAnimations();
    }

    private void OnEnable()
    {
        if (body == null || bodyCollider == null) return;
        if (!ownsMovementMaterial)
        {
            originalMaterial = bodyCollider.sharedMaterial;
            if (movementMaterial == null)
                movementMaterial = new PhysicsMaterial2D("Dafeiyu Movement")
                {
                    friction = 0f,
                    bounciness = 0f,
                    hideFlags = HideFlags.HideAndDontSave
                };
            // 水平移动由代码控制；去掉碰撞摩擦，防止持续顶墙时悬停。
            bodyCollider.sharedMaterial = movementMaterial;
            ownsMovementMaterial = true;
        }
        originalGravityScale = body.gravityScale;
        body.gravityScale = 0f; // StepMotor 统一计算重力，避免叠加。
        ownsGravity = true;
        visualState = 0;
        nextAttackAt = 0f;
        hasPhysicsSample = false;
    }

    private void CacheAnimations()
    {
        for (int side = 0; side < 2; side++)
            for (int mode = 0; mode < RightNames.Length; mode++)
                states[side, mode] = Animator.StringToHash("Base Layer." +
                    (side == 0 ? LeftNames[mode] : RightNames[mode]));
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.applyRootMotion = false;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "attackleft") attackLengths[0] = Mathf.Max(0.05f, clip.length);
            if (clip.name == "attackright") attackLengths[1] = Mathf.Max(0.05f, clip.length);
            if (clip.name == "wallleftup") climbLengths[0, 0] = Mathf.Max(0.05f, clip.length);
            if (clip.name == "wallleftdown") climbLengths[0, 1] = Mathf.Max(0.05f, clip.length);
            if (clip.name == "wallrightup") climbLengths[1, 0] = Mathf.Max(0.05f, clip.length);
            if (clip.name == "wallrightdown") climbLengths[1, 1] = Mathf.Max(0.05f, clip.length);
            for (int side = 0; side < 2; side++)
                for (int motion = 0; motion < 2; motion++)
                    if (clip.name == (side == 0 ? LeftNames[5 + motion] : RightNames[5 + motion]))
                        climbFrameCounts[side, motion] = Mathf.Max(1,
                            Mathf.RoundToInt(clip.length * clip.frameRate));
        }
        for (int side = 0; side < 2; side++)
            for (int mode = 0; mode < RightNames.Length; mode++)
                if (!animator.HasState(0, states[side, mode]))
                    Debug.LogWarning("Animator 缺少 Base Layer." +
                        (side == 0 ? LeftNames[mode] : RightNames[mode]), this);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) ClearInput();
        else
        {
            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            moveInput = (right ? 1 : 0) - (left ? 1 : 0);
            slowWalk = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            jumpHeld = keyboard.spaceKey.isPressed;
            int heldClimb = (keyboard.wKey.isPressed ? 1 : 0) -
                (keyboard.sKey.isPressed ? 1 : 0);
            int pressedClimb = (keyboard.wKey.wasPressedThisFrame ? 1 : 0) -
                (keyboard.sKey.wasPressedThisFrame ? 1 : 0);
            if (keyboard.wKey.isPressed && keyboard.sKey.isPressed) pressedClimb = 0;
            ReadClimbInput(heldClimb, pressedClimb, Time.time);

            if (Time.time < wallJumpControlUntil)
                    {
                        facing = wallJumpAwaySide;
                    } else if (moveInput != 0)
                            facing = moveInput;
            
            bool movePressed=keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame ||
                             keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;

            bool cancelledAttack = isAttacking && movePressed;
            if (cancelledAttack) CancelAttack();

            if (keyboard.spaceKey.wasPressedThisFrame)
                jumpBufferedUntil = Time.time + jumpBufferTime;
            if (keyboard.spaceKey.wasReleasedThisFrame)
                cutJumpRequested = true;

            // 取消攻击这一帧优先移动，避免同时按 J 又重新出招。
            if (!cancelledAttack && keyboard.jKey.wasPressedThisFrame)
                attackBufferedUntil = Time.time + attackBufferTime;
        }
        TickAttack(Time.time);
    }

    private void ReadClimbInput(int held, int pressed, float now)
    {
        climbInput = held;
        if (!isClimbing)
        {
            queuedClimbInput = 0;
            return;
        }
        if (pressed == 0) return;
        queuedClimbInput = pressed;
        climbInputBufferedUntil = now + 0.1f;
        climbMotion = pressed > 0 ? Motion.ClimbUp : Motion.ClimbDown;
        int frames = climbFrameCounts[wallSide < 0 ? 0 : 1, pressed > 0 ? 0 : 1];
        // 短按当帧就跨到下一张图，不等待一整张图片的播放间隔。
        climbAnimationPhase = Mathf.Repeat(
            (Mathf.Floor(climbAnimationPhase * frames + 0.0001f) + 1f) / frames, 1f);
    }


    private void CancelAttack(){
        isAttacking=false;
        attackBufferedUntil=float.NegativeInfinity;
        nextAttackAt=Time.time+attackRecovery;
    }



    private void TickAttack(float now)
    {
        if(isClimbing){
            attackBufferedUntil=float.NegativeInfinity;
            return;
        }
        if (isAttacking && now >= attackStartedAt + attackDuration)
        {
            isAttacking = false;
            nextAttackAt = attackStartedAt + attackDuration + attackRecovery;
        }
        if (isAttacking || now < nextAttackAt || now > attackBufferedUntil) return;
        attackBufferedUntil = float.NegativeInfinity;
        attackFacing = facing; // 每次挥击固定出手方向，移动方向仍可改变。
        attackDuration = attackLengths[attackFacing < 0 ? 0 : 1] / Mathf.Max(0.1f, attackPlaybackSpeed);
        attackStartedAt = now;
        isAttacking = true;
    }

    private void FixedUpdate() => StepMotor(Time.fixedDeltaTime, Time.time);

    private void StepMotor(float dt, float now)
    {
        if(TryClimb(now) &&now >= stunnedUntil) return;
        Vector2 velocity = body.linearVelocity;
        bool wasGrounded = grounded;
        grounded = now >= ignoreGroundUntil && velocity.y <= 0.1f && CheckGround();
        if (grounded)
        {
            coyoteUntil = now + coyoteTime;
            if (!wasGrounded && hasPhysicsSample) landingUntil = now + landingDuration;
            airborneTime = 0f;
        }
        else airborneTime += dt;
        hasPhysicsSample = true;

        bool preserveWallJump =
                                !grounded &&
                                now < wallJumpControlUntil &&
                                moveInput != wallJumpAwaySide;

        bool stunned=now<stunnedUntil;

        if (!preserveWallJump && !stunned)
        {
            float targetSpeed =
                moveInput * (slowWalk ? walkSpeed : runSpeed);

            float acceleration =
                grounded ? groundAcceleration : airAcceleration;

            if (moveInput == 0)
                acceleration = braking;
            else if (velocity.x * targetSpeed < 0f)
                acceleration = turnAcceleration;

            velocity.x = Mathf.MoveTowards(
                velocity.x, targetSpeed, acceleration * dt);
        }

        if (now <= jumpBufferedUntil && now <= coyoteUntil)
        {
            velocity.y = jumpSpeed;
            if (!jumpHeld) velocity.y *= jumpCutMultiplier;
            jumpBufferedUntil = coyoteUntil = float.NegativeInfinity;
            ignoreGroundUntil = now + 0.08f;
            landingUntil = float.NegativeInfinity;
            grounded = false;
            airborneTime = 0f;
            cutJumpRequested = false;
        }
        else if (cutJumpRequested && velocity.y > 0f) velocity.y *= jumpCutMultiplier;
        cutJumpRequested = false;

        float gravityMultiplier = velocity.y < 0f ? fallGravityMultiplier
            : (!jumpHeld && velocity.y > 0f ? releasedGravityMultiplier : 1f);
        if (!grounded)
            velocity.y = Mathf.Max(-maxFallSpeed, velocity.y +
                Physics2D.gravity.y * riseGravityScale * gravityMultiplier * dt);
        body.linearVelocity = velocity;
    }

    private bool CheckGround()
    {
        // 下探角色碰撞体，只接受朝上的表面；墙面和 Trigger 不能刷新跳跃。
        int count = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundProbeDistance);
        for (int i = 0; i < count; i++)
            if (groundHits[i].normal.y >= 0.65f &&
                groundHits[i].point.y <= bodyCollider.bounds.center.y) return true;
        return false;
    }

    private bool CheckWall(int direction, out float distance)
    {
        // 已抓墙时稍微扩大维持距离，避免接触皮肤宽度导致状态反复切换。
        float probeDistance = wallCheckDistance + (isClimbing ? 0.04f : 0f);
        int count = bodyCollider.Cast(Vector2.right * direction, wallFilter, wallHits, probeDistance);
        distance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = wallHits[i];
            if (hit.normal.x * direction < -0.7f)
            {
                float separation = Mathf.Max(0f, bodyCollider.Distance(hit.collider).distance);
                distance = Mathf.Min(distance, Mathf.Min(hit.distance, separation));
            }
        }
        return !float.IsPositiveInfinity(distance);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 碰撞发生在本次 FixedUpdate 之后；当次接触就更新姿势，不多等一个物理帧。
        if (!enabled || isClimbing ||
            (wallLayer.value & (1 << collision.collider.gameObject.layer)) == 0) return;
        TryClimb(Time.time);
    }

    private bool TryClimb(float now)
    {
        if(now < wallGrabBlockedUtil){
            isClimbing=false;
            return false;
        }
        // 新抓墙只检测玩家正在按的水平方向，不依赖上一次面朝方向。
        if (!isClimbing && moveInput == 0) return false;
        int side = isClimbing ? wallSide : moveInput;

        float wallDistance;
        if (!CheckWall(side, out wallDistance))
        {
            isClimbing = false;
            return false;
        }

        if(moveInput == -side){
            isClimbing=false;
            wallGrabBlockedUtil=now + 0.15f;
            return false;
        }

        // 下爬到地面后回到普通移动，不能继续挂墙或向地板施加下爬速度。
        bool standingOnGround = now >= ignoreGroundUntil &&
            body.linearVelocity.y <= 0.1f && CheckGround();
        if (standingOnGround)
        {
            isClimbing = false;
            return false;
        }

        bool enteringWall = !isClimbing;
        if (enteringWall)
        {
            wallGrabStartedAt = now;
            wallGrabStartVelocityY = body.linearVelocity.y;

            climbMotion = climbInput != 0
                ? (climbInput > 0 ? Motion.ClimbUp : Motion.ClimbDown)
                : (body.linearVelocity.y > 0f ? Motion.ClimbUp : Motion.ClimbDown);
            // 从抓稳墙面的姿势开始，避免挂墙时停在素材的起始准备帧。
            climbAnimationPhase = 0.1f;
        }
        wallSide=side;
        facing=side;
        isClimbing=true;
        grounded=false;

        coyoteUntil=float.NegativeInfinity;
        landingUntil = float.NegativeInfinity;

        if (isAttacking) CancelAttack();
        attackBufferedUntil = float.NegativeInfinity;

        // 新接触时不把起跳前尚未消耗的缓冲误当成蹬墙指令。
        if (!enteringWall && now <= jumpBufferedUntil){
            isClimbing=false;
            facing = -side;
            wallGrabBlockedUtil = now + 0.15f;

            wallJumpAwaySide = -side;
            wallJumpControlUntil = now + wallJumpControlTime;

            jumpBufferedUntil = float.NegativeInfinity;
            ignoreGroundUntil = now + 0.08f;
            airborneTime = 0f;
            cutJumpRequested=false;

            body.linearVelocity=new Vector2(-side* runSpeed,
            jumpHeld?jumpSpeed:jumpSpeed*jumpCutMultiplier);

            return true;
        }
        if (enteringWall) jumpBufferedUntil = float.NegativeInfinity;
        // 只吸附检测范围内的微小间隙，保留碰撞皮肤，不能穿进墙面。
        float snap = Mathf.Max(0f, wallDistance - 0.01f);
        if (snap > 0f)
            body.position += Vector2.right * (side * snap);
        int motorClimbInput = climbInput;
        if (motorClimbInput == 0 && now <= climbInputBufferedUntil)
            motorClimbInput = queuedClimbInput;
        queuedClimbInput = 0;
        climbInputBufferedUntil = float.NegativeInfinity;
        if (motorClimbInput != 0)
            climbMotion = motorClimbInput > 0 ? Motion.ClimbUp : Motion.ClimbDown;
        float targetY = motorClimbInput * climbSpeed;

        // 从触墙开始，经过指定时间后到达目标速度。
        float progress = Mathf.Clamp01(
            (now - wallGrabStartedAt) / wallGrabBlendTime);

        float blend = Mathf.SmoothStep(0f, 1f, progress);

        float velocityY = Mathf.Lerp(
            wallGrabStartVelocityY, targetY, blend);

        body.linearVelocity = new Vector2(0f, velocityY);
        cutJumpRequested=false;
        // 爬墙已接管这一物理帧，不再执行普通移动和重力。
        return true;

    }
    private void LateUpdate()
    {
        if (animator == null || !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null) return;
        if(isClimbing)
            ShowClimbAnimation(Time.deltaTime);
        else if(isAttacking)
            ShowAnimation(Motion.Attack, attackFacing, Mathf.Min(AttackProgress, 0.999f));
        else if (!grounded)
        {
            // 十帧：03 蹬地，04～06 上升/顶点，07～08 下落；09～10 留给落地。
            float normalized = body.linearVelocity.y > 0f
                ? Mathf.Lerp(0.2f, 0.5f, 1f - Mathf.Clamp01(body.linearVelocity.y / jumpSpeed))
                : Mathf.Lerp(0.6f, 0.7f, Mathf.Clamp01(-body.linearVelocity.y / 6f));
            if (airborneTime < 0.04f && body.linearVelocity.y > 0f) normalized = 0.2f;
            ShowAnimation(Motion.Jump, facing, normalized);
        }
        else if (Time.time < landingUntil && moveInput == 0)
        {
            float progress = 1f - (landingUntil - Time.time) / landingDuration;
            ShowAnimation(Motion.Jump, facing, Mathf.Lerp(0.8f, 0.999f, progress));
        }
        else
        {
            Motion motion = moveInput == 0 ? Motion.Idle : (slowWalk ? Motion.Walk : Motion.Run);
            ShowAnimation(motion, facing);
        }
    }

    private void ShowClimbAnimation(float dt)
    {
        if (climbInput != 0)
        {
            int side = wallSide < 0 ? 0 : 1;
            int motion = climbMotion == Motion.ClimbDown ? 1 : 0;
            climbAnimationPhase = Mathf.Repeat(climbAnimationPhase +
                dt * climbAnimationSpeed / climbLengths[side, motion], 1f);
        }
        // 显式采样循环，避免受到跳跃/攻击留下的 Animator.speed=0 影响。
        ShowAnimation(climbMotion, wallSide, climbAnimationPhase);
    }

    private void ShowAnimation(Motion motion, int direction, float sample = -1f)
    {
        int state = states[direction < 0 ? 0 : 1, (int)motion];
        if (!animator.HasState(0, state)) return;
        animator.SetInteger(ModeId, (int)motion);
        animator.SetInteger(FacingId, direction);
        bool sampled = sample >= 0f;
        animator.speed = sampled ? 0f : 1f;
        if (sampled)
        {
            animator.Play(state, 0, sample);
            animator.Update(0f);
        }
        else if (state != visualState)
        {
            float phase = 0f;
            if (motion == visualMotion && (motion == Motion.Walk || motion == Motion.Run))
                phase = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
            animator.Play(state, 0, phase);
            animator.Update(0f);
        }
        visualState = state;
        visualMotion = motion;
    }

    private void ClearInput()
    {
        wallJumpControlUntil = float.NegativeInfinity;
        climbInput=0;
        queuedClimbInput = 0;
        climbInputBufferedUntil = float.NegativeInfinity;
        isClimbing=false;
        moveInput = 0;
        slowWalk = jumpHeld = false;
        cutJumpRequested = true;
        jumpBufferedUntil = attackBufferedUntil = float.NegativeInfinity;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) ClearInput();
    }

    private void OnDisable()
    {
        ClearInput();
        isAttacking = grounded = false;
        coyoteUntil = ignoreGroundUntil = landingUntil = float.NegativeInfinity;
        if (bodyCollider != null && ownsMovementMaterial)
        {
            bodyCollider.sharedMaterial = originalMaterial;
            ownsMovementMaterial = false;
        }
        if (body != null && ownsGravity)
        {
            body.gravityScale = originalGravityScale;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            ownsGravity = false;
        }
        // 禁用或退出运行时不调用 Animator.Play，避免销毁期间的警告。
        if (animator != null) animator.speed = 1f;
    }

    private void OnDestroy()
    {
        if (movementMaterial == null) return;
        if (Application.isPlaying) Destroy(movementMaterial);
        else DestroyImmediate(movementMaterial);
    }

    public void ApplyKnockback(Vector2 sourcePosition,float speed, float stunTime){
        int dir = transform.position.x>=sourcePosition.x?1:-1;
        stunnedUntil = Time.time + stunTime;
        body.linearVelocity=new Vector2(dir * speed,body.linearVelocity.y);
    }
}

