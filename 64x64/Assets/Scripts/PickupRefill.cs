using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRefill : MonoBehaviour
{
    private PlayerController player;

    public enum PickupType{health, stamina}
    public PickupType type;
    public SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject gainPS;

    private bool shouldMove = false;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        StartCoroutine(TimeToDespawn());
    }

    // Update is called once per frame
    void Update()
    {
        if (type == PickupType.health)
        {
            if (player.healthSlider.value < player.healthSlider.maxValue)
            {
                if (shouldMove)
                {
                    Vector3 direction = (player.transform.position - transform.position).normalized;
                    transform.position += direction * 4 * Time.deltaTime;
                }
            }
        }
        else
        {
            if (player.staminaSlider.value < player.staminaSlider.maxValue)
            {
                if (shouldMove)
                {
                    Vector3 direction = (player.transform.position - transform.position).normalized;
                    transform.position += direction * 4 * Time.deltaTime;
                }
            }
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= 0.2f)
        {
            Collect();
        }
    }

    private IEnumerator TimeToDespawn()
    {
        yield return new WaitForSeconds(3);

        spriteRenderer.enabled = false;

        for (int i = 0; i < 10; i++)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        shouldMove = true;
    }

    private void Collect()
    {
        if (type == PickupType.health)
        {
            if(!player.isHealing)
            {
                if (player.healthSlider.value < player.healthSlider.maxValue)
                {
                    StopAllCoroutines();
                    player.healthSlider.value++;

                    GameObject gaineffect = Instantiate(gainPS, player.transform.position, Quaternion.identity);

                    gaineffect.transform.parent = player.gameObject.transform;
                    player.isHealing = true;

                    Destroy(this.gameObject);
                }
            }
        }
        else
        {
            if(!player.isResting)
            {
                if (player.staminaSlider.value < player.staminaSlider.maxValue)
                {
                    StopAllCoroutines();
                    player.staminaSlider.value++;

                    GameObject gaineffect = Instantiate(gainPS, player.transform.position, Quaternion.identity);

                    gaineffect.transform.parent = player.gameObject.transform;
                    player.isResting = true;

                    Destroy(this.gameObject);
                }
            }
        }
    }
}
