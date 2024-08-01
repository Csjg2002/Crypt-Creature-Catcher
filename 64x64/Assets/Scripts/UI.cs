using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image fadeScreen;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
