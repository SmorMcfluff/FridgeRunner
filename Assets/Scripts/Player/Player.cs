using System.Collections.Generic;
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
    private Vector3 lastRecordedPosition = Vector3.positiveInfinity;

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

            UpdateInteractPrompt();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu();

        if (GameManager.Instance.isCutscene)
        {
            UpdateInteractPrompt();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) bufferedJump = true;
        if (Input.GetMouseButtonDown(0)) bufferedLeftClick = true;
        if (Input.GetMouseButtonDown(1)) bufferedRightClick = true;


        inputDir = GetMoveDir();

        bool inputDirChanged = inputDir != currentInputDir;

        if (inputDirChanged)
        {
            currentInputDir = inputDir;
        }

        UpdateInteractPrompt();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isCutscene)
            return;

        if (!ReplayRecordingManager.Instance.isReplay && isAlive)
        {
            RecordFrameInput();
        }

        if (!ReplayRecordingManager.Instance.isReplay || !isReplaying)
        {
            HandlePlayerInputMovement();
        }
    }

    private void TryShoot()
    {
        if (gunEquipped && !isSwitching)
        {
            Shoot();
        }
    }

    private void TryInteract()
    {
        if (!gunEquipped && !isSwitching && !isInteracting)
        {
            Interact();
        }
    }

    private void TrySwitchHand()
    {
        if (hasGun && !isSwitching && !isInteracting)
        {
            SwitchHand();
        }
    }


    private void RecordFrameInput()
    {
        float timestamp = Time.timeSinceLevelLoad;
        ReplayRecordingManager record = ReplayRecordingManager.Instance;

        if ((transform.position - lastRecordedPosition).sqrMagnitude > 0.0001f)
        {
            record.inputList.Add(new ME(timestamp, transform.position));
            lastRecordedPosition = transform.position;
        }

        float currentY = transform.eulerAngles.y;
        if (lastRecordedRotationY == float.MinValue || Mathf.Abs(Mathf.DeltaAngle(currentY, lastRecordedRotationY)) > 0.1f)
        {
            ReplayRecordingManager.Instance.inputList.Add(new RE(timestamp, currentY));
            lastRecordedRotationY = currentY;
        }
    }

    private void RecordFrameInput(string id)
    {
        float timestamp = Time.timeSinceLevelLoad;
        ReplayRecordingManager.Instance.inputList.Add(new IE(timestamp, id));
    }

    private void HandlePlayerInputMovement()
    {
        if (bufferedLeftClick)
        {
            if (gunEquipped)
                TryShoot();
            else
                TryInteract();
        }

        if (bufferedRightClick)
        {
            TrySwitchHand();
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
        Debug.Log("Interacted!");
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.OnInteract();
            Debug.Log("We hit " + hit.collider.gameObject.name);
            RecordFrameInput(hit.collider.gameObject.name);
        }
        else
        {
            Debug.Log("We hit nothing");
            RecordFrameInput(string.Empty);
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

        if (!ReplayRecordingManager.Instance.isReplay)
        {
            ReplayRecordingManager.Instance.inputList.Add(new SH(Time.timeSinceLevelLoad, gunEquipped));
        }
    }

    public void Shoot()
    {
        if (!ReplayRecordingManager.Instance.isReplay)
        {
            ReplayRecordingManager.Instance.inputList.Add(new SE(Time.timeSinceLevelLoad));
            ReplayRecordingManager.Instance.inputList.Add(new RE(Time.timeSinceLevelLoad, transform.eulerAngles.y));

        }

        sound.ShootSound();
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit hit, Mathf.Infinity))
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
            if (hit.collider.TryGetComponent<IInteractable>(out _))
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

    public void SetReplaySprinting(bool isSprinting) => replayIsSprinting = isSprinting;
}
