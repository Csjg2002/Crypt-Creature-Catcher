using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDead : MonoBehaviour
{
    private GameObject player;
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().Play("Death");
        player = FindObjectOfType<PlayerController>().gameObject;
        StartCoroutine(FadeDeath());
    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector3 lookDirection = player.transform.position - this.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        this.transform.localRotation = lookRotation;
    }

    private IEnumerator FadeDeath()
    {
        yield return new WaitForSeconds(2);

        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        while (elapsedTime < 2)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / 2);
            color.a = alpha;
            spriteRenderer.color = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        spriteRenderer.color = color;

        Destroy(this.gameObject);
    }
}
