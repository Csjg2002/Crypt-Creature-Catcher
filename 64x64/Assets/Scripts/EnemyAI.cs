using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private GameObject player;

    private NavMeshAgent enemyAgent;

    private Vector3 destination;
    private bool isMoving = false;
    private bool isChasing = false;

    public float chaseDistance = 7f;
    public float chaseSpeed = 6f;
    public float normalSpeed = 3f;

    private Coroutine moveCoroutine;
    private Coroutine chaseCoroutine;
    private Coroutine attackCoroutine;

    private float distanceToPlayer;
    private bool hasDamagedPlayer = false;

    [HideInInspector] public float enemyHealth;
    [HideInInspector] public int attackCount = 0;

    public GameObject enemyBody;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = Random.Range(3, 6);
        player = FindObjectOfType<PlayerController>().gameObject;
        enemyAgent = GetComponent<NavMeshAgent>();
        StartCoroutine(MoveToNewLocation());
    }

    // Update is called once per frame

    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < chaseDistance)
        {
            if (!isChasing)
            {
                StartChasing();
            }
        }
        else
        {
            if (isChasing)
            {
                StopChasing();
            }
        }

        LookAtPlayer();
    }

    private void StartChasing()
    {
        isChasing = true;
        enemyAgent.speed = chaseSpeed;
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        if (chaseCoroutine == null)
        {
            chaseCoroutine = StartCoroutine(ChasePlayer());
        }
    }

    private void StopChasing()
    {
        isChasing = false;
        enemyAgent.speed = normalSpeed;
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
        }
        if (moveCoroutine == null)
        {
            moveCoroutine = StartCoroutine(MoveToNewLocation());
        }
    }

    private void LookAtPlayer()
    {
        Vector3 lookDirection = player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        enemyBody.transform.localRotation = lookRotation;
    }

    private IEnumerator MoveToNewLocation()
    {
        while (true)
        {
            if (!isMoving && !isChasing)
            {
                yield return StartCoroutine(FindNewLocation());
                isMoving = true;
                enemyAgent.SetDestination(destination);
                yield return new WaitForSeconds(Random.Range(3, 6));
            }
            yield return null;

            while (isMoving)
            {
                if (enemyAgent.pathPending || enemyAgent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                if (Vector3.Distance(transform.position, destination) < 1)
                {
                    Debug.Log("Reached destination");
                    isMoving = false;
                    yield return new WaitForSeconds(Random.Range(3, 6));
                }
                yield return null;
            }
        }
    }

    private IEnumerator FindNewLocation()
    {
        int retries = 0;
        bool foundValidLocation = false;

        while (retries < 10 && !foundValidLocation)
        {
            float randomX = Random.Range(-10, 10);
            float randomZ = Random.Range(-10, 10);
            destination = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                foundValidLocation = true;
            }
            else
            {
                retries++;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (!foundValidLocation)
        {
            destination = transform.position;
            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator ChasePlayer()
    {
        while (isChasing)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer > chaseDistance)
            {
                StopChasing();
                yield break;
            }

            if (distanceToPlayer > 1.5f && distanceToPlayer < chaseDistance)
            {
                enemyAgent.SetDestination(player.transform.position);
            }
            else
            {
                enemyAgent.SetDestination(transform.position);

                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(Attack());
                }
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        float attackCooldown = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < attackCooldown)
        {
            if (distanceToPlayer > 1.5f)
            {
                attackCoroutine = null;
                hasDamagedPlayer = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (distanceToPlayer <= 1.5f && !hasDamagedPlayer)
        {
            hasDamagedPlayer = true;

            player.GetComponent<PlayerController>().DamageEffects();

            player.GetComponent<PlayerController>().healthSlider.value--;

            if (player.GetComponent<PlayerController>().healthSlider.value <= 0)
            {
                player.GetComponent<PlayerController>().GameOver();
            }

            attackCoroutine = null;
        }
    }
}
