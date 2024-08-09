using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public CharacterController playerController;

    public Camera playerCam;
    public Camera renderCam;
    public Transform playerBody;

    private float playerXrotation = 0;

    private Vector3 playerVelocity;

    public Transform groundCheck;
    public LayerMask groundMask;
    private bool isGrounded = true;

    private int currentSpeed = 12;
    [HideInInspector] public bool isSprinting = false;

    private bool isCrouching = false;
    private Vector3 offset;

    public Slider healthSlider;
    public Slider staminaSlider;

    private Coroutine decreaseStaminaCoroutine;
    private Coroutine refillStaminaCoroutine;
    private Coroutine swordAnimCoroutine;
    private Coroutine swordTrailCoroutine;

    public GameObject sword;
    public GameObject swordTrail;
    private bool hasAttacked = false;
    [HideInInspector] public bool canSwingSword = true;

    public GameObject net;
    [SerializeField] private ParticleSystem collectCreaturePS;
    [SerializeField] private ParticleSystem statGainPS;
    [SerializeField] private GameObject collectedCreature;
    [HideInInspector] public bool hasFoundCreature;

    public GameObject[] gear;
    private int currentGearIndex = 0;
    [HideInInspector] public bool canSwitch = true;
    [HideInInspector] public bool canSwingNet = true;

    private UI ui;

    [HideInInspector] public bool hasQueuedInput = false;

    [HideInInspector] public bool isPaused = false;
    [HideInInspector] public bool canMove = true;

    public GameObject speedLines;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ui = FindObjectOfType<UI>();

        for (int i = 0; i < gear.Length; i++)
        {
            gear[i].SetActive(i == currentGearIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPaused && canMove)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);

            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2;
            }

            if (Input.GetMouseButtonDown(0) && canSwingSword && canSwingNet)
            {
                Action();
            }
            else if (Input.GetMouseButtonDown(0) && !canSwingSword || Input.GetMouseButtonDown(0) && !canSwingNet)
            {
                hasQueuedInput = true;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                if (canSwitch)
                {
                    ScrollGear();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (currentGearIndex != 0)
                {
                    ScrollGear();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (currentGearIndex != 1)
                {
                    ScrollGear();
                }
            }

            if (Input.GetKeyDown(KeyCode.Slash))
            {
                GameOver();
            }

            CamLook();
            PlayerMove();

            Crouch();
        }
    }

    private void CamLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * 50 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 50 * Time.deltaTime;

        playerXrotation -= mouseY;
        playerXrotation = Mathf.Clamp(playerXrotation, -90, 90);

        renderCam.transform.localRotation = Quaternion.Euler(playerXrotation, 0, 0);
        playerCam.transform.localRotation = Quaternion.Euler(playerXrotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void PlayerMove()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = playerBody.right * moveX + playerBody.forward * moveZ;
        
        move.Normalize();

        playerController.Move(move * currentSpeed * Time.deltaTime);

        playerVelocity.y += -9.81f * Time.deltaTime;

        playerController.Move(playerVelocity * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift) && staminaSlider.value > 0)
        {
            if(moveX != 0 || moveZ != 0)
            {
                speedLines.SetActive(true);
                Sprint();
            }
            else
            {
                speedLines.SetActive(false);
                if (decreaseStaminaCoroutine != null)
                {
                    StopCoroutine(decreaseStaminaCoroutine);
                }
                Walk();
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            speedLines.SetActive(false);
            isCrouching = !isCrouching;
        }
        else
        {
            speedLines.SetActive(false);
            Walk();
        }

        if (moveX != 0 || moveZ != 0)
        {
            StartBobbing();
            sword.gameObject.GetComponent<Animator>().SetFloat("Speed", currentSpeed);
            net.gameObject.GetComponent<Animator>().SetFloat("Speed", currentSpeed);
        }
        else
        {
            StartCoroutine(StopBobbing(0.05f));
            sword.gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
            net.gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
        }
    }

    private void Sprint()
    {
        currentSpeed = 12;

        renderCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 78f, 20f * Time.deltaTime);
        playerCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 78f, 20f * Time.deltaTime);

        if (!isSprinting)
        {
            isSprinting = true;
            isCrouching = false;

            if (refillStaminaCoroutine != null)
            {
                StopCoroutine(refillStaminaCoroutine);
            }
            decreaseStaminaCoroutine = StartCoroutine(DecreaseStamina());
        }
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            playerController.height = playerController.height - 10f * Time.deltaTime;

            currentSpeed = 4;

            renderCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 50f, 20f * Time.deltaTime);
            playerCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 50f, 20f * Time.deltaTime);

            if(playerController.height <= 1f)
            {
                playerController.height = 1f;
            }
        }
        else
        {
            int layerMask = LayerMask.GetMask("Default");
            Ray ray = new Ray(playerCam.transform.position, playerCam.transform.up);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1f, layerMask))
            {
                if(playerController.height != 2)
                {
                    isCrouching = true;
                }
            }
            else
            {
                playerController.height = playerController.height + 10f * Time.deltaTime;

                if (playerController.height < 2f)
                {
                    playerBody.position = playerBody.position + offset * Time.deltaTime;
                }
                if (playerController.height >= 2f)
                {
                    playerController.height = 2f;
                }
            }
        }
    }

    private void Walk()
    {
        if(!isCrouching && !isSprinting)
        {
            currentSpeed = 8;

            renderCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 60f, 20f * Time.deltaTime);
            playerCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 60f, 20f * Time.deltaTime);
        }

        if (isSprinting)
        {
            isSprinting = false;
            isCrouching = false;

            if (decreaseStaminaCoroutine != null)
            {
                StopCoroutine(decreaseStaminaCoroutine);
            }
            refillStaminaCoroutine = StartCoroutine(RefillStamina());
        }
    }

    private void StartBobbing()
    {
        renderCam.gameObject.GetComponent<Animator>().Play("HeadBob");

        if (currentSpeed == 8)
        {
            renderCam.gameObject.GetComponent<Animator>().speed = 1;
            sword.gameObject.GetComponent<Animator>().speed = 0.8f;
            net.gameObject.GetComponent<Animator>().speed = 0.8f;
        }
        else if(currentSpeed == 12)
        {
            renderCam.gameObject.GetComponent<Animator>().speed = 1;
            sword.gameObject.GetComponent<Animator>().speed = 1;
            net.gameObject.GetComponent<Animator>().speed = 1;
        }
        else
        {
            renderCam.gameObject.GetComponent<Animator>().speed = 0.5f;
            sword.gameObject.GetComponent<Animator>().speed = 0.6f;
            net.gameObject.GetComponent<Animator>().speed = 0.6f;
        }
    }

    private IEnumerator StopBobbing(float duration)
    {
        float elapsedTime = 0f;

        Vector3 originalPos = new Vector3(0, 0.8f, 0);
        Vector3 currentPos = renderCam.transform.localPosition;

        renderCam.gameObject.GetComponent<Animator>().Play("New State");
        renderCam.gameObject.GetComponent<Animator>().enabled = false;

        while (elapsedTime < duration)
        {
            renderCam.transform.localPosition = Vector3.Lerp(currentPos, originalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderCam.transform.localPosition = originalPos;
        renderCam.gameObject.GetComponent<Animator>().enabled = true;
    }

    private IEnumerator DecreaseStamina()
    {
        while (isSprinting && staminaSlider.value > 0)
        {
            staminaSlider.value -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator RefillStamina()
    {
        yield return new WaitForSeconds(2);

        while (staminaSlider.value < staminaSlider.maxValue)
        {
            staminaSlider.value += 0.2f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator DamageShake()
    {
        if(!isPaused)
        {
            Quaternion pcInitialRotation = playerCam.transform.localRotation;
            Quaternion rcInitialRotation = renderCam.transform.localRotation;

            float elapsed = 0f;

            while (elapsed < 0.75f)
            {
                if(!isPaused)
                {
                    float yShake = Random.Range(-2f, 2f);
                    float zShake = Random.Range(-2f, 2f);

                    playerCam.transform.localRotation = pcInitialRotation * Quaternion.Euler(0, yShake, zShake);
                    renderCam.transform.localRotation = rcInitialRotation * Quaternion.Euler(0, yShake, zShake);

                    elapsed += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }

            playerCam.transform.localRotation = pcInitialRotation;
            renderCam.transform.localRotation = rcInitialRotation;
        }  
    }

    private void ScrollGear()
    {
        canSwitch = false;

        if (currentGearIndex == 0)
        {
            sword.gameObject.GetComponent<Animator>().Play("Sword_Pulldown");
        }
        else
        {
            net.gameObject.GetComponent<Animator>().Play("Net_Pulldown");
        }
    }

    public void SwitchGear()
    {
        gear[currentGearIndex].SetActive(false);

        currentGearIndex = (currentGearIndex + 1) % gear.Length;

        gear[currentGearIndex].SetActive(true);

        if(currentGearIndex == 0)
        {
            sword.gameObject.GetComponent<Animator>().Play("Sword_Pullup");
        }
        else
        {
            net.gameObject.GetComponent<Animator>().Play("Net_Pullup");
        }

        canSwitch = true;
    }

    public void Action()
    {
        sword.gameObject.GetComponent<Animator>().speed = 1;
        net.gameObject.GetComponent<Animator>().speed = 1;

        if (currentGearIndex == 0)
        {
            if (staminaSlider.value > 0)
            {
                if (!hasAttacked)
                {
                    if (swordAnimCoroutine != null)
                    {
                        StopCoroutine(swordAnimCoroutine);
                    }
                    swordAnimCoroutine = StartCoroutine(SwordAnimReset());
                    sword.gameObject.GetComponent<Animator>().Play("Sword_Swing");
                    sword.gameObject.GetComponent<SwordCollisionDetection>().shouldAttack = false;
                    swordTrail.SetActive(true);
                    canSwitch = false;
                }
                else
                {
                    if (swordAnimCoroutine != null)
                    {
                        StopCoroutine(swordAnimCoroutine);
                    }

                    sword.gameObject.GetComponent<Animator>().Play("Sword_Swing2");
                    sword.gameObject.GetComponent<SwordCollisionDetection>().shouldAttack = true;
                    swordTrail.SetActive(true);
                    canSwitch = false;
                }

                hasQueuedInput = false;
                canSwingSword = false;
                hasAttacked = !hasAttacked;
                ActionStamina();

                if(swordTrailCoroutine != null)
                {
                    StopCoroutine (swordTrailCoroutine);
                    swordTrailCoroutine = StartCoroutine(SwordTrailReset());
                }
                else
                {
                    swordTrailCoroutine = StartCoroutine(SwordTrailReset());
                }
            }
        }
        else
        {
            if(staminaSlider.value > 0)
            {
                hasQueuedInput = false;
                canSwingNet = false;
                canSwitch = false;
                CatchCreature();
                ActionStamina();
            }
        }
    }

    public void ActionStamina()
    {
        staminaSlider.value--;
        Sprint();
        Walk();
    }

    private IEnumerator SwordTrailReset()
    {
        yield return new WaitForSeconds(0.9f);
        SwordTrailDeactivate();
        canSwitch = true;
    }

    public void SwordTrailDeactivate()
    {
        swordTrail.SetActive(false);
        if(swordTrailCoroutine != null)
        {
            StopCoroutine(swordTrailCoroutine);
            swordTrailCoroutine = null;
        }
        canSwitch = true;
    }

    private IEnumerator SwordAnimReset()
    {
        yield return new WaitForSeconds(1.5f);
        hasAttacked = false;
        swordAnimCoroutine = null;
    }

    private void CatchCreature()
    {
        net.gameObject.GetComponent<Animator>().Play("Net_Swing");
    }

    public void CheckForCreature()
    {
        int layerMask = LayerMask.GetMask("AI");

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2.5f, layerMask))
        {
            if (hit.collider.CompareTag("Creature"))
            {
                GameObject creature = Instantiate(collectedCreature, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);
                Destroy(hit.collider.gameObject);
                StartCoroutine(ShrinkCreature(creature));
                hasFoundCreature = true;
                canMove = false;
            }
        }
    }

    private IEnumerator ShrinkCreature(GameObject creatureToShrink)
    {
        Vector3 startScale = creatureToShrink.transform.localScale;
        Vector3 endScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < 0.2f)
        {
            float t = elapsed / 0.2f;
            creatureToShrink.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        creatureToShrink.transform.localScale = endScale;

        ParticleSystem collecteffect = Instantiate(collectCreaturePS, creatureToShrink.transform.position, Quaternion.identity);

        Vector3 directionToPlayer = (transform.position - creatureToShrink.transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

        collecteffect.transform.rotation = lookRotation;

        ParticleSystem statgaineffect = Instantiate(statGainPS, transform.position, Quaternion.identity);

        hasFoundCreature = false;

        ui.creatureFound = true;
        ui.creatureFoundImage.gameObject.SetActive(true);
        ui.creatureImage.color = Color.white;
        StartCoroutine(ui.CatchCreature(new Vector3(200, 468, 0), new Vector3(200, 300, 0), true));

        Destroy(creatureToShrink);
        healthSlider.maxValue += 2;
        healthSlider.value = healthSlider.maxValue;

        StartCoroutine(CatchCreatureShake());
    }

    public void GameOver()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void DamageEffects()
    {
        if(!isPaused)
        {
            StartCoroutine(DamageShake());
            StartCoroutine(ui.DamageIndicator());
        }
    }

    public IEnumerator SwordAttackCameraShake(float duration, float magnitude)
    {
        if(!isPaused)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if(!isPaused)
                {
                    float percentComplete = elapsed / duration;
                    float damping = Mathf.Clamp01(1 - (percentComplete * 1));
                    transform.localPosition = originalPos + Random.insideUnitSphere * magnitude * damping;

                    elapsed += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
            transform.localPosition = originalPos;
        }
    }

    private IEnumerator CatchCreatureShake()
    {
        if(!isPaused)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < 0.2f)
            {
                if(!isPaused)
                {
                    Vector3 shakeOffset = Random.insideUnitSphere * 0.05f;
                    transform.localPosition = originalPos + shakeOffset;

                    elapsed += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }

            transform.localPosition = originalPos;
        }
    }

    public IEnumerator ChestOpenShake()
    {
        if(!isPaused)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < 0.2f)
            {
                if(!isPaused)
                {
                    float x = Random.Range(-1f, 1f) * 0.1f;
                    float y = Random.Range(-1f, 1f) * 0.1f;

                    transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

                    elapsed += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }

            transform.localPosition = originalPos;
        }
    }
}
