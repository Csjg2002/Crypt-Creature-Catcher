using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private PlayerController player;
    public PauseGame pause;

    public Image fadeScreen;
    public Image damageScreen;

    public GameObject creatureBook;
    public Image creatureImage;
    public Image creatureFoundImage;
    [HideInInspector] public bool creatureFound = false;
    private bool creatureBookActive = false;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        pause = FindObjectOfType<PauseGame>();
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!creatureBookActive)
            {
                StartCoroutine(OpenCreatureBook(new Vector3(-200, 0, 0), new Vector3(8, 0, 0)));
                creatureBookActive = true;
                pause.StartPause();
            }
            else
            {
                StartCoroutine(OpenCreatureBook(transform.localPosition, new Vector3(-200, 0, 0)));
                creatureBookActive = false;
                pause.StopPause();
            }
        }
    }

    private IEnumerator FadeIn()
    {
        float startAlpha = fadeScreen.color.a;
        float elapsedTime = 0f;

        Color color = fadeScreen.color;
        color.a = 1f;
        fadeScreen.color = color;

        while (elapsedTime < 2)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / 2);
            color.a = alpha;
            fadeScreen.color = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        fadeScreen.color = color;
    }

    public IEnumerator DamageIndicator()
    {
        float elapsedTime = 0f;

        Color color = damageScreen.color;
        color.a = 1f;
        damageScreen.color = color;

        while (elapsedTime < 1)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2);
            color.a = alpha;
            damageScreen.color = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        damageScreen.color = color;
    }

    private IEnumerator OpenCreatureBook(Vector3 startPosition, Vector3 endPosition)
    {
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            creatureBook.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.unscaledDeltaTime;

            yield return null;
        }

        creatureBook.transform.position = endPosition;
    }
}
