using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    private bool isSprinting = false;

    private bool isCrouching = false;
    private Vector3 offset;

    public Slider healthSlider;
    public Slider staminaSlider;

    private Coroutine decreaseStaminaCoroutine;
    private Coroutine refillStaminaCoroutine;

    public GameObject sword;
    [HideInInspector] public bool isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Screen.SetResolution(400,400,false);
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2;
        }

        if (Input.GetKey(KeyCode.LeftShift) && staminaSlider.value > 0)
        {
            Sprint();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }
        else
        {
            Walk();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Action();
        }

        CamLook();
        PlayerMove();

        Crouch();
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

        if (moveX != 0 || moveZ != 0)
        {
            StartBobbing();
            sword.gameObject.GetComponent<Animator>().SetFloat("Speed", currentSpeed);
        }
        else
        {
            StartCoroutine(StopBobbing(0.05f));
            sword.gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
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
            currentSpeed = 4;

            renderCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 50f, 20f * Time.deltaTime);
            playerCam.fieldOfView = Mathf.Lerp(renderCam.fieldOfView, 50f, 20f * Time.deltaTime);

            playerController.height = playerController.height - 10f * Time.deltaTime;

            if(playerController.height <= 1f)
            {
                playerController.height = 1f;
            }
        }
        else
        {
            playerController.height = playerController.height + 10f * Time.deltaTime;

            if(playerController.height < 2f)
            {
                playerBody.position = playerBody.position + offset * Time.deltaTime;
            }
            if (playerController.height >= 2f)
            {
                playerController.height = 2f;
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
        }
        else if(currentSpeed == 12)
        {
            renderCam.gameObject.GetComponent<Animator>().speed = 1f;
            sword.gameObject.GetComponent<Animator>().speed = 1;
        }
        else
        {
            renderCam.gameObject.GetComponent<Animator>().speed = 0.5f;
            sword.gameObject.GetComponent<Animator>().speed = 0.6f;
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
            staminaSlider.value += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Action()
    {
        sword.gameObject.GetComponent<Animator>().speed = 1;
        sword.gameObject.GetComponent<Animator>().Play("Sword_Swing");
        isAttacking = true;
    }

    private void CatchCreature()
    {
        int layerMask = LayerMask.GetMask("AI");

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2.5f, layerMask))
        {
            if (hit.collider.CompareTag("Creature"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
