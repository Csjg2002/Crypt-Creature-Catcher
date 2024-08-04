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
    public float zigzagAmount = 1f;
    public float zigzagFrequency = 5f;

    private Coroutine moveCoroutine;
    private Coroutine chaseCoroutine;
    private Coroutine attackCoroutine;

    private float distanceToPlayer;
    private bool hasDamagedPlayer = false;

    [HideInInspector] public float enemyHealth;
    [HideInInspector] public int attackCount = 0;

    public GameObject enemyBody;
    
    [HideInInspector] public bool hasBeenAttacked = false;

    public GameObject deadSwitch;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = Random.Range(3, 7);
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

        float speed = enemyAgent.velocity.magnitude;
        enemyBody.GetComponent<Animator>().SetFloat("Speed", speed);
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
        hasDamagedPlayer = false;

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
        Vector3 lookDirection = player.transform.position - enemyBody.transform.position;
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
        Vector3 lastPosition = transform.position;
        float stuckThreshold = 1f;

        while (isChasing)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer > chaseDistance)
            {
                yield break;
            }

            if (distanceToPlayer > 1.5f && distanceToPlayer < chaseDistance)
            {
                Vector3 targetPosition = player.transform.position;
                Vector3 directionToPlayer = (targetPosition - transform.position).normalized;

                float time = Time.time * zigzagFrequency;
                Vector3 zigzagOffset = new Vector3(Mathf.Sin(time) * zigzagAmount,0,Mathf.Cos(time) * zigzagAmount
                );

                Vector3 newDestination = targetPosition + zigzagOffset;

                if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    newDestination = hit.position;
                }
                else
                {
                    newDestination = targetPosition;
                }

                enemyAgent.SetDestination(newDestination);

                if (Vector3.Distance(transform.position, lastPosition) < stuckThreshold)
                {
                    newDestination = targetPosition + zigzagOffset * 0.5f;
                    enemyAgent.SetDestination(newDestination);
                }

                lastPosition = transform.position;
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
        if (hasBeenAttacked) yield break;

        enemyBody.GetComponent<Animator>().Play("Attack");

        float attackCooldown = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < attackCooldown)
        {
            if (distanceToPlayer > 1.5f || !isChasing || hasBeenAttacked)
            {
                hasDamagedPlayer = false;
                attackCoroutine = null;
                StopChasing();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (distanceToPlayer <= 1.5f && isChasing && !hasDamagedPlayer && !hasBeenAttacked)
        {
            hasDamagedPlayer = true;

            player.GetComponent<CharacterController>().Move(enemyBody.transform.forward += player.transform.forward * 0.2f * Time.deltaTime);


            player.GetComponent<PlayerController>().DamageEffects();
            player.GetComponent<PlayerController>().healthSlider.value--;

            if (player.GetComponent<PlayerController>().healthSlider.value <= 0)
            {
                player.GetComponent<PlayerController>().GameOver();
            }
        }

        attackCoroutine = null;
        StopChasing();
    }

    public void Death()
    {
        Instantiate(deadSwitch, enemyBody.gameObject.transform.position, enemyBody.gameObject.transform.rotation);
        Destroy(this.gameObject);
    }
}