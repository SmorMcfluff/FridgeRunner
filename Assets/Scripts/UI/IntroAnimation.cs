using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroAnimation : MonoBehaviour
{
    public TextMeshProUGUI introText;
    public Image introImage;
    public Image blackFade;

    public Sprite[] slide1;
    public Sprite[] slide2;
    public Sprite[] slide3;

    public float slide1FPS;
    public float slide2FPS;
    public float slide3FPS;

    public float blackFadeDuration = 0.5f;

    private Coroutine slideLoopCoroutine;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            EndIntro();
        }
    }

    IEnumerator IntroSequence()
    {
        // Start looping slide 1
        slideLoopCoroutine = StartCoroutine(SlideLoop(slide1, slide1FPS));

        // Fade in from black
        yield return StartCoroutine(BlackFade(0));

        yield return new WaitForSeconds(3f);

        // Fade to black
        yield return StartCoroutine(BlackFade(1));

        // Switch to slide 2 while black
        StopCoroutine(slideLoopCoroutine);
        introText.text = "I am going to sweat to death";
        slideLoopCoroutine = StartCoroutine(SlideLoop(slide2, slide2FPS));

        // Fade in
        yield return StartCoroutine(BlackFade(0));

        yield return new WaitForSeconds(3f);

        // Fade to black
        yield return StartCoroutine(BlackFade(1));

        // Switch to slide 3 while black
        introText.text = "I need to grab a cold one";

        StopCoroutine(slideLoopCoroutine);
        slideLoopCoroutine = StartCoroutine(SlideLoop(slide3, slide3FPS));

        // Fade in
        yield return StartCoroutine(BlackFade(0));

        yield return new WaitForSeconds(3f);

        // Final fade to black
        yield return StartCoroutine(BlackFade(1));

        EndIntro();
    }

    IEnumerator SlideLoop(Sprite[] frames, float fps)
    {
        float frameDuration = 1f / fps;
        int frameIndex = 0;

        while (true)
        {
            introImage.sprite = frames[frameIndex];
            yield return new WaitForSeconds(frameDuration);

            frameIndex = (frameIndex + 1) % frames.Length;
        }
    }

    IEnumerator BlackFade(int targetAlpha)
    {
        float startAlpha = blackFade.color.a;
        float elapsed = 0f;

        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blackFadeDuration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            blackFade.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        blackFade.color = new Color(0, 0, 0, targetAlpha);
    }

    private void EndIntro()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
