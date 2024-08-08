using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private PlayerController player;

    public Image fadeScreen;
    public Image damageScreen;

    public GameObject creatureBook;
    private bool creatureBookActive = false;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if(!creatureBookActive)
                {
                    StartCoroutine(OpenCreatureBook(new Vector3(0, 1, 1), new Vector3(1, 1, 1)));
                    creatureBookActive = true;
                }
                else
                {
                    StartCoroutine(OpenCreatureBook(transform.localScale, new Vector3(0, 1, 1)));
                    creatureBookActive = false;
                }
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

    private IEnumerator OpenCreatureBook(Vector3 startScale, Vector3 endScale)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 0.3f)
        {

            float t = elapsedTime / 0.3f;

            creatureBook.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        creatureBook.transform.localScale = endScale;
    }
}
