using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType { melee, ranged }
    public EnemyType enemyType;

    private GameObject player;

    private NavMeshAgent enemyAgent;

    private Vector3 destination;

    //Melee
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

    //Ranged
    private bool isFleeing = false;

    public float fleeDistance = 10f;
    public float fleeSpeed = 12f;

    private Coroutine fleeCoroutine;

    [HideInInspector] public GameObject enemyBody;

    [SerializeField] private float timer = 5;
    private float projectileTime;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject projectileSpawnPoint;

    [HideInInspector] public float enemyHealth;
    [HideInInspector] public int attackCount = 0;

    [HideInInspector] public bool hasBeenAttacked = false;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = Random.Range(3, 6);
        enemyBody = GetComponentInChildren<Animator>().gameObject;
        player = FindObjectOfType<PlayerController>().gameObject;
        Physics.IgnoreCollision(GetComponentInChildren<Collider>(), player.GetComponent<CharacterController>());
        enemyAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame

    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (enemyType == EnemyType.melee)
        {

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
        }
        else
        {
            if (distanceToPlayer < fleeDistance)
            {
                if (!isFleeing)
                {
                    StartFleeing();
                }
            }
            else
            {
                if (isFleeing)
                {
                    StopFleeing();
                }
            }

            ReadyUpAttack();
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
                Vector3 zigzagOffset = new Vector3(Mathf.Sin(time) * zigzagAmount, 0, Mathf.Cos(time) * zigzagAmount
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
                if (attackCoroutine == null && isChasing && !hasBeenAttacked)
                {
                    enemyAgent.SetDestination(transform.position);
                    enemyBody.GetComponent<Animator>().Play("Attack");
                }
            }

            yield return null;
        }
    }

    public void StartAttack()
    {
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        if (hasBeenAttacked) yield break;

        float attackCooldown = 0.8f;
        float elapsedTime = 0f;

        while (elapsedTime < attackCooldown)
        {
            if (distanceToPlayer > 2f || !isChasing || hasBeenAttacked)
            {
                attackCoroutine = null;
                StopChasing();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (distanceToPlayer <= 1.5f && isChasing && !hasBeenAttacked)
        {
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

    //Ranged
    private void StartFleeing()
    {
        isFleeing = true;
        enemyAgent.speed = fleeSpeed;
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        fleeCoroutine = StartCoroutine(FleeFromPlayer());
    }

    private void StopFleeing()
    {
        isFleeing = false;
        enemyAgent.speed = normalSpeed;
        if (fleeCoroutine != null)
        {
            StopCoroutine(fleeCoroutine);
            fleeCoroutine = null;
        }

        if (moveCoroutine == null)
        {
            moveCoroutine = StartCoroutine(MoveToNewLocation());
        }
    }

    private IEnumerator FleeFromPlayer()
    {
        while (isFleeing)
        {
            Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
            Vector3 potentialDestination = transform.position + directionAwayFromPlayer.normalized * fleeDistance;

            if (NavMesh.SamplePosition(potentialDestination, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                destination = hit.position;
                enemyAgent.SetDestination(destination);
            }
            else
            {
                destination = transform.position;
                enemyAgent.SetDestination(destination);
                yield return new WaitForSeconds(2);
            }

            if (enemyAgent.pathPending || enemyAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            yield return null;
        }
    }

    private void ReadyUpAttack()
    {
        projectileTime -= Time.deltaTime;

        if (projectileTime > 0)
        {
            return;
        }

        projectileTime = timer;

        enemyBody.GetComponent<Animator>().Play("Attack");
    }

    public void ShootAtPlayer()
    {
        GameObject projectileObj = Instantiate(projectilePrefab, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
        Rigidbody projectileRB = projectileObj.GetComponent<Rigidbody>();
        projectileObj.GetComponent<Projectile>().enemyThrown = this.gameObject;
        projectileRB.velocity = enemyBody.transform.forward * 8;
        Destroy(projectileObj, 5f);
    }
}