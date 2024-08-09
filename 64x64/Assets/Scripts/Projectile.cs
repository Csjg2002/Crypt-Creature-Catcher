using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public GameObject enemyThrown;

    private PlayerController player;

    private List<GameObject> enemiesToAttack = new List<GameObject>();

    [HideInInspector] public bool shouldAttack = false;

    public GameObject deadSwitch;

    public GameObject bloodParticle;

    private GameObject chestToOpen;

    [HideInInspector] public EnemySpawner currentEncounter;
    [HideInInspector] public int enemiesRemaining;

    public GameObject healthPickup;
    public GameObject staminaPickup;

    private bool hasAttacked = false;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!hasAttacked)
        {
            hasAttacked = true;

            if (other.gameObject.tag == "Player")
            {
                if (player != null)
                {
                    if (!player.hasAttacked)
                    {
                        player.GetComponent<CharacterController>().Move(transform.forward += player.transform.forward * 0.2f * Time.deltaTime);

                        player.GetComponent<PlayerController>().DamageEffects();
                        player.GetComponent<PlayerController>().healthSlider.value--;

                        if (player.GetComponent<PlayerController>().healthSlider.value <= 0)
                        {
                            player.GetComponent<PlayerController>().GameOver();
                        }
                        else
                        {
                            Destroy(this.gameObject);
                        }
                    }
                    else
                    {
                        GameObject projectileObj = Instantiate(this.gameObject, new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z - 1), this.gameObject.transform.rotation);
                        Rigidbody projectileRB = projectileObj.GetComponent<Rigidbody>();
                        projectileObj.GetComponent<Projectile>().enemyThrown = null;
                        projectileRB.velocity = player.transform.forward * 8;
                        Destroy(projectileObj, 5f);
                        Destroy(this.gameObject);
                    }
                }
            }
            else if (other.gameObject.tag == "Enemy")
            {
                EnemyAI enemy = other.GetComponent<EnemyAI>();

                StartCoroutine(player.ProjectileReturnShake(0.2f, 0.1f, 1));

                enemy.GetComponentInParent<EnemyAI>().enemyHealth -= 2;

                enemy.GetComponentInParent<EnemyAI>().hasBeenAttacked = true;

                if (enemy.GetComponentInParent<EnemyAI>().enemyHealth <= 0)
                {
                    StartCoroutine(enemyHurtIndicator(enemy.enemyBody.gameObject, 0.15f));
                }
                else
                {
                    NavMeshAgent enemyAgent = enemy.GetComponentInParent<NavMeshAgent>();
                    if (enemyAgent != null)
                    {
                        enemyAgent.enabled = false;
                        enemy.transform.position += player.gameObject.transform.forward * 0.75f;
                        enemyAgent.enabled = true;
                    }

                    shouldAttack = false;
                    StartCoroutine(enemyHurtIndicator(enemy.enemyBody.gameObject, 0.15f));
                }

                StartCoroutine(enemyHurtIndicator(enemy.enemyBody.gameObject, 0.1f));
            }
            else
            {
                Destroy(this.gameObject);
            }
        }  
    }

    public IEnumerator enemyHurtIndicator(GameObject enemy, float stopTime)
    {
        Instantiate(bloodParticle, enemy.transform.position, enemy.transform.rotation);

        enemy.GetComponent<SpriteRenderer>().material.color = Color.red;

        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(stopTime);

        Time.timeScale = 1;

        if(enemy != null)
        {

            enemy.GetComponentInParent<EnemyAI>().hasBeenAttacked = false;
            enemy.GetComponent<SpriteRenderer>().material.color = Color.white;

            if (enemy.GetComponentInParent<EnemyAI>().enemyHealth <= 0)
            {
                enemiesRemaining--;
                Instantiate(deadSwitch, enemy.gameObject.transform.position, enemy.gameObject.transform.rotation);

                int pickupType = Random.Range(0, 7);

                switch (pickupType)
                {
                    case 0:
                        Instantiate(healthPickup, enemy.transform.position, Quaternion.identity);
                        break;
                    case 1:
                        Instantiate(staminaPickup, enemy.transform.position, Quaternion.identity);
                        break;
                    case 2:
                        Vector3 offset = Random.onUnitSphere;
                        offset.y = 0;
                        offset.Normalize();
                        offset *= 0.5f;

                        Instantiate(healthPickup, enemy.transform.position + offset, Quaternion.identity);
                        Instantiate(staminaPickup, enemy.transform.position - offset, Quaternion.identity);
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                }

                Destroy(enemy.transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}
