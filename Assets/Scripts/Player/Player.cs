using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;

    Rigidbody rb;
    GroundCheck groundCheck;
    public HandAnimator handAnim;
    public Heat heat;
    public PlayerSounds sound;

    [Header("Movement")]
    public float defaultSpeed = 5;
    public float sprintSpeed = 10;
    public float aerialSpeed = 30f;
    public float jumpForce = 15;

    public float acceleration = 30f;
    public float deceleration = 90f;

    [Header("Interaction")]
    public float interactRange;

    public bool IsSprinting => !isReplaying && Input.GetKey(KeyCode.LeftShift);
    private bool replayIsSprinting = false;

    private bool isJumpQueued;

    private bool bufferedJump;
    public bool bufferedLeftClick;
    public bool bufferedRightClick;

    public bool isInteracting = false;
    public bool isAlive = true;

    public bool IsByWall => CheckWall();
    public LayerMask wallLayer;

    Vector2 inputDir;
    Vector2 currentInputDir;
    private float lastRecordedRotationY = float.MinValue;

    [Header("Keycard")]
    public bool hasKeycard = false;

    [Header("Gun")]
    public bool hasGun = false;
    public bool gunEquipped = false;
    public bool isSwitching = false;

    [Header("UI")]
    public TextMeshProUGUI interactPrompt;
    public TextMeshProUGUI keycardPrompt;
    public TextMeshProUGUI deathText;

    // Replay variables
    private Vector2 replayInputDir = Vector2.zero;
    private bool isReplaying = false;

    private bool replayJumpQueued = false;
    private bool replayClickQueued = false;
    private bool replayRightClickQueued = false;
    private float lastReplayClickTime = -1f;
    private float replayClickCooldown = 0.1f;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        heat = GetComponent<Heat>();
        sound = GetComponent<PlayerSounds>();
        keycardPrompt.enabled = false;
        interactPrompt.gameObject.SetActive(false);
    }

    void Start()
    {
        if (ReplayRecordingManager.Instance.isReplay)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (ReplayRecordingManager.Instance.isReplay || isReplaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu();
            if (!GameManager.Instance.isCutscene)
            {
                if (replayJumpQueued && groundCheck.IsGrounded())
                {
                    isJumpQueued = true;
                    replayJumpQueued = false;
                }

                if (replayClickQueued && Time.time - lastReplayClickTime > replayClickCooldown)
                {
                    if (gunEquipped)
                        Shoot();
                    else
                        handAnim.PlayInteract();

                    lastReplayClickTime = Time.time;
                    replayClickQueued = false;
                }

                if (replayRightClickQueued && hasGun && !isSwitching && !isInteracting)
                {
                    SwitchHand();
                    replayRightClickQueued = false;
                }
            }

            UpdateInteractPrompt();
            return;
        }

        if (GameManager.Instance.isCutscene)
        {
            UpdateInteractPrompt();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) bufferedJump = true;
        if (Input.GetMouseButtonDown(0)) bufferedLeftClick = true;
        if (Input.GetMouseButtonDown(1)) bufferedRightClick = true;

        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu();

        inputDir = GetMoveDir();

        float currentRotationY = transform.eulerAngles.y;
        bool rotationChanged = Mathf.Abs(Mathf.DeltaAngle(currentRotationY, lastRecordedRotationY)) > 0.1f;
        bool inputDirChanged = inputDir != currentInputDir;

        if (rotationChanged || inputDirChanged)
        {
            lastRecordedRotationY = currentRotationY;
            currentInputDir = inputDir;
        }

        UpdateInteractPrompt();
    }

    private void RecordFrameInput()
    {
        var input = new InputEvent(
            Time.timeSinceLevelLoad,
            inputDir,
            transform.eulerAngles.y,
            IsSprinting,
            bufferedJump,
            bufferedLeftClick,
            bufferedRightClick,
            false,
            transform.position,
            ""
        );

        ReplayRecordingManager.Instance.inputList.Add(input);
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isCutscene)
            return;

        if (!ReplayRecordingManager.Instance.isReplay && isAlive)
        {
            RecordFrameInput();
        }

        if (ReplayRecordingManager.Instance.isReplay || isReplaying)
        {
            HandleReplayMovement();
        }
        else
        {
            HandlePlayerInputMovement();
        }

    }


    private void HandlePlayerInputMovement()
    {
        if (bufferedLeftClick && !isInteracting && !isSwitching)
        {
            if (gunEquipped)
                Shoot();
            else
                Interact();
        }

        if (bufferedRightClick && hasGun && !isSwitching && !isInteracting)
        {
            SwitchHand();
        }

        if (bufferedJump && groundCheck.IsGrounded())
        {
            isJumpQueued = true;
        }

        HandleMovement();

        if (isJumpQueued)
            Jump();

        bufferedJump = bufferedLeftClick = bufferedRightClick = false;
    }

    private void HandleReplayMovement()
    {
        // Use replayInputDir instead of live inputDir
        bool grounded = groundCheck.IsGrounded();
        bool sprint = replayIsSprinting;
        float speedCap = grounded ? (sprint ? sprintSpeed : defaultSpeed) : aerialSpeed;

        Vector3 currentVel = rb.linearVelocity;
        Vector3 verticalVel = Vector3.up * currentVel.y;

        if (replayInputDir != Vector2.zero)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
            if (currentSpeed > speedCap)
                currentSpeed = speedCap;

            Vector3 inputVector = (transform.forward * replayInputDir.y + transform.right * replayInputDir.x).normalized;
            Vector3 newFlatVel = inputVector * currentSpeed;

            rb.linearVelocity = newFlatVel + verticalVel;
        }
        else
        {
            if (grounded)
            {
                currentSpeed = 0f;
                rb.linearVelocity = verticalVel;
            }
            else
            {
                currentSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentSpeed < 0f)
                    currentSpeed = 0f;

                Vector3 flatVel = new Vector3(currentVel.x, 0, currentVel.z);
                Vector3 direction = flatVel.normalized;
                Vector3 newFlatVel = direction * currentSpeed;

                rb.linearVelocity = newFlatVel + verticalVel;
            }
        }

        if (replayJumpQueued && grounded)
        {
            Jump();
            replayJumpQueued = false;
        }

        if (replayClickQueued)
        {
            if (gunEquipped)
                Shoot();
            else
                handAnim.PlayInteract();

            replayClickQueued = false;
        }

        if (replayRightClickQueued && hasGun && !isSwitching && !isInteracting)
        {
            SwitchHand();
            replayRightClickQueued = false;
        }
    }


    private Vector2 GetMoveDir()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float yInput = Input.GetAxisRaw("Vertical");
        return new(xInput, yInput);
    }

    private void Interact()
    {
        isInteracting = true;
        handAnim.PlayInteract();
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.OnInteract();
            RecordInteractionEvent(hit.collider.gameObject.name);
        }
    }

    private float currentSpeed = 0f;

    private void HandleMovement()
    {
        bool grounded = groundCheck.IsGrounded();
        bool sprint = isReplaying ? replayIsSprinting : IsSprinting;
        float speedCap = grounded ? (sprint ? sprintSpeed : defaultSpeed) : aerialSpeed;

        Vector3 currentVel = rb.linearVelocity;
        Vector3 verticalVel = Vector3.up * currentVel.y;

        if (inputDir != Vector2.zero)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
            if (currentSpeed > speedCap)
                currentSpeed = speedCap;

            Vector3 inputVector = (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;
            Vector3 newFlatVel = inputVector * currentSpeed;

            rb.linearVelocity = newFlatVel + verticalVel;
        }
        else
        {
            if (grounded)
            {
                currentSpeed = 0f;
                rb.linearVelocity = verticalVel;
            }
            else
            {
                currentSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentSpeed < 0f)
                    currentSpeed = 0f;

                Vector3 flatVel = new Vector3(currentVel.x, 0, currentVel.z);
                Vector3 direction = flatVel.normalized;
                Vector3 newFlatVel = direction * currentSpeed;

                rb.linearVelocity = newFlatVel + verticalVel;
            }
        }
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumpQueued = false;
    }

    private bool CheckWall()
    {
        return Physics.Raycast(transform.position, transform.forward, 0.8f, wallLayer);
    }

    public void SwitchHand()
    {
        isSwitching = true;
        handAnim.SwitchHand(!gunEquipped);
        gunEquipped = !gunEquipped;
    }

    private void Shoot()
    {
        sound.ShootSound();
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(1);
        }
    }

    public void Die(bool heatDeath)
    {
        isAlive = false;
        string cause = heatDeath ? "Overheating" : "Hypothermia";
        deathText.text = "You have died of " + cause;
        deathText.GetComponentInParent<Image>().color = new Color(1, 1, 1, 0.75f);
        Invoke(nameof(BackToMenu), 1.5f);
    }

    private void UpdateInteractPrompt()
    {
        if (interactPrompt == null)
            return;

        if (GameManager.Instance.isCutscene)
        {
            interactPrompt.gameObject.SetActive(false);
            return;
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactPrompt.gameObject.SetActive(true);
                interactPrompt.text = gunEquipped ? "RightClick to holster Gun" : "LeftClick to Interact";
                return;
            }
        }

        interactPrompt.gameObject.SetActive(false);
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    private void RecordInteractionEvent(string id)
    {
        if (!ReplayRecordingManager.Instance.isReplay)
        {
            var input = new InputEvent(
                Time.timeSinceLevelLoad,
                inputDir,
                transform.eulerAngles.y,
                IsSprinting,
                bufferedJump,
                bufferedLeftClick,
                bufferedRightClick,
                false,
                transform.position,
                id
            );
            ReplayRecordingManager.Instance.inputList.Add(input);
        }
    }

    public void SetReplayMovement(Vector2 inputDir) => replayInputDir = inputDir;
    public void SetReplayRotation(float rotationY)
    {
        isReplaying = true;
        Vector3 euler = transform.eulerAngles;
        euler.y = rotationY;
        transform.eulerAngles = euler;
    }
    public void SetReplayPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void TriggerReplayJump() => replayJumpQueued = true;
    public void TriggerReplayClick() => replayClickQueued = true;
    public void TriggerReplayRightClick() => replayRightClickQueued = true;
    public void SetReplaySprinting(bool isSprinting) => replayIsSprinting = isSprinting;
}
