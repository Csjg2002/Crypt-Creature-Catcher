using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    public Sprite[] bloodParticles;
    public SpriteRenderer bloodRenderer;

    // Start is called before the first frame update
    void Start()
    {
        int bp = Random.Range(0, bloodParticles.Length);
        bloodRenderer.sprite = bloodParticles[bp];
        StartCoroutine(End());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator End()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(this.gameObject);
    }
}
