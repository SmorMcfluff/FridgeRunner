using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HandAnimator : MonoBehaviour
{
    private enum HandState
    {
        Idle,
        Walking,
        Interacting,
        ByWall
    }

    private HandState currentState = HandState.Idle;

    Image imgComponent;

    public Sprite idleSprite;
    public Sprite wallSprite;
    public Sprite gunSprite;
    public Sprite[] walkSprites;
    public Sprite[] interactSprites;
    public Sprite[] fridgeDrinkSprites;
    public Sprite[] fridgeThumbSprites;


    private GroundCheck groundCheck;
    private Player player;
    private Rigidbody playerRb;

    private Coroutine walkCoroutine;
    private Coroutine interactCoroutine;

    private float walkFPS = 6f;

    private void Start()
    {
        player = Player.Instance;
        playerRb = player.GetComponent<Rigidbody>();
        groundCheck = player.GetComponentInChildren<GroundCheck>();
        imgComponent = GetComponent<Image>();
    }

    private void Update()
    {
        if (player.isInteracting || player.isSwitching) return;

        if (player.gunEquipped)
        {
            StopWalkAnimationIfNeeded();
            return;
        }

        if (player.IsByWall)
        {
            if (currentState != HandState.ByWall)
            {
                player.sound.WallSound();
                currentState = HandState.ByWall;
                StopAllAnimations();
                imgComponent.sprite = wallSprite;
                SetImageSize();
            }
            return;
        }

        if (currentState == HandState.ByWall && !player.IsByWall)
        {
            currentState = HandState.Idle;
            imgComponent.sprite = idleSprite;
            SetImageSize();
        }

        CheckWalkAnimTrigger();
    }


    private void CheckWalkAnimTrigger()
    {
        if (player.isSwitching || player.gunEquipped) return;

        if (currentState == HandState.Interacting || currentState == HandState.ByWall)
            return;

        Vector3 velocity = playerRb.linearVelocity;
        Vector3 flatVel = new Vector3(velocity.x, 0, velocity.z);
        float speed = flatVel.magnitude;

        if (speed > 0.001f)
        {
            float fps = groundCheck.IsGrounded()
                ? (player.IsSprinting ? 12 : 6)
                : Mathf.Lerp(6f, 24f, Mathf.Clamp01(speed / player.aerialSpeed));

            if (currentState != HandState.Walking)
            {
                walkFPS = fps;
                walkCoroutine = StartCoroutine(WalkAnim());
                currentState = HandState.Walking;
            }
            else
            {
                walkFPS = fps;
            }
        }
        else if (currentState == HandState.Walking)
        {
            StopCoroutine(walkCoroutine);
            walkCoroutine = null;
            imgComponent.sprite = idleSprite;
            SetImageSize();
            currentState = HandState.Idle;
        }
    }



    private IEnumerator WalkAnim()
    {
        int frameCount = walkSprites.Length;
        int index = 0;
        bool forward = true;

        while (true)
        {
            imgComponent.sprite = walkSprites[index];
            SetImageSize();

            float waitTime = 1f / Mathf.Max(walkFPS, 0.01f);
            yield return new WaitForSeconds(waitTime);

            if (forward)
            {
                index++;
                if (index >= frameCount - 1)
                {
                    forward = false;
                }
            }
            else
            {
                index--;
                if (index <= 0)
                {
                    forward = true;
                }
            }
        }
    }

    private void SetImageSize()
    {
        imgComponent.SetNativeSize();
        imgComponent.preserveAspect = true;
    }

    public void PlayInteract()
    {
        StopAllAnimations();
        currentState = HandState.Interacting;
        interactCoroutine = StartCoroutine(InteractAnim());
    }

    private IEnumerator InteractAnim()
    {
        int frameCount = interactSprites.Length;
        int index = 0;
        bool forward = true;
        float interactFPS = 12;

        while (true)
        {
            imgComponent.sprite = interactSprites[index];
            SetImageSize();
            yield return new WaitForSeconds(1f / interactFPS);

            if (forward)
            {
                index++;
                if (index >= frameCount - 1)
                    forward = false;
            }
            else
            {
                index--;
                if (index <= 0)
                    break;
            }
        }

        player.isInteracting = false;
        interactCoroutine = null;
        imgComponent.sprite = idleSprite;
        currentState = HandState.Idle;
    }


    public void PlayFinishAnimation()
    {
        StartCoroutine(FinishAnimation());
    }

    private IEnumerator FinishAnimation()
    {
        yield return new WaitForSeconds(2.5f);
        int frameCount = fridgeDrinkSprites.Length;

        float waitTime = 1f / 8f;

        for (int i = 0; i < frameCount; i++)
        {
            imgComponent.sprite = fridgeDrinkSprites[i];
            SetImageSize();
            yield return new WaitForSeconds(waitTime);
        }

        player.sound.DrinkSound();
        yield return new WaitForSeconds(1.55f);

        imgComponent.sprite = fridgeDrinkSprites[frameCount - 2];
        SetImageSize();
        yield return new WaitForSeconds(0.7f);

        for (int i = frameCount - 3; i >= 5; i--)
        {
            imgComponent.sprite = fridgeDrinkSprites[i];
            SetImageSize();
            yield return new WaitForSeconds(waitTime);
        }
        yield return new WaitForSeconds(0.2f);

        player.sound.WinSound();
        foreach (var sprite in fridgeThumbSprites)
        {
            imgComponent.sprite = sprite;
            SetImageSize();
            yield return new WaitForSeconds(waitTime);
        }
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("LevelSelect");
    }

    private void StopAllAnimations()
    {
        if (walkCoroutine != null)
        {
            StopCoroutine(walkCoroutine);
            walkCoroutine = null;
        }

        if (interactCoroutine != null)
        {
            StopCoroutine(interactCoroutine);
            interactCoroutine = null;
        }
    }

    public void SwitchHand(bool toGun)
    {
        StartCoroutine(SwitchHandCoroutine(toGun));
    }

    private IEnumerator SwitchHandCoroutine(bool toGun)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 originalPos = imgComponent.rectTransform.localPosition;

        float spriteHeight = imgComponent.rectTransform.rect.height;
        float offsetY = spriteHeight + 100f;  // Extra 20 for margin
        Vector3 downPos = originalPos + new Vector3(0, -offsetY, 0);

        // Move down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            imgComponent.rectTransform.localPosition = Vector3.Lerp(originalPos, downPos, t);
            yield return null;
        }

        imgComponent.rectTransform.localPosition = downPos;

        // Switch sprite
        imgComponent.sprite = toGun ? gunSprite : idleSprite;
        SetImageSize();

        elapsed = 0f;

        yield return new WaitForSeconds(0.1f);

        // Move back up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            imgComponent.rectTransform.localPosition = Vector3.Lerp(downPos, originalPos, t);
            yield return null;
        }

        imgComponent.rectTransform.localPosition = originalPos;
        player.isSwitching = false;
    }

    private void StopWalkAnimationIfNeeded()
    {
        if (walkCoroutine != null)
        {
            StopCoroutine(walkCoroutine);
            walkCoroutine = null;
        }

        if (currentState == HandState.Walking)
        {
            currentState = HandState.Idle;
        }

        if (imgComponent.sprite != gunSprite)
        {
            imgComponent.sprite = gunSprite;
            SetImageSize();
        }
    }

}
