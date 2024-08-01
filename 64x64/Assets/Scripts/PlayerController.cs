using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Camera playerCam;
    public Transform playerBody;

    private float playerXrotation = 0;

    private Vector3 playerVelocity;

    public Transform groundCheck;
    public LayerMask groundMask;
    private bool isGrounded = true;

    private int currentSpeed = 12;
    private bool isSprinting;

    public Slider healthSlider;
    public Slider staminaSlider;

    private Coroutine decreaseStaminaCoroutine;
    private Coroutine refillStaminaCoroutine;

    public GameObject sword;

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
            currentSpeed = 24;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 78f, 20f * Time.deltaTime);

            if (!isSprinting)
            {
                isSprinting = true;
                if (refillStaminaCoroutine != null)
                {
                    StopCoroutine(refillStaminaCoroutine);
                }
                decreaseStaminaCoroutine = StartCoroutine(DecreaseStamina());
            }
        }
        else
        {
            currentSpeed = 12;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 60f, 20f * Time.deltaTime);

            if (isSprinting)
            {
                isSprinting = false;
                if (decreaseStaminaCoroutine != null)
                {
                    StopCoroutine(decreaseStaminaCoroutine);
                }
                refillStaminaCoroutine = StartCoroutine(RefillStamina());
            }
        }

        CamLook();
        PlayerMove();
    }

    private void CamLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * 80 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 80 * Time.deltaTime;

        playerXrotation -= mouseY;
        playerXrotation = Mathf.Clamp(playerXrotation, -90, 90);

        playerCam.transform.localRotation = Quaternion.Euler(playerXrotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void PlayerMove()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = playerBody.right * moveX + playerBody.forward * moveZ;
        
        move.Normalize();

        GetComponent<CharacterController>().Move(move * currentSpeed * Time.deltaTime);

        playerVelocity.y += -9.81f * Time.deltaTime;

        GetComponent<CharacterController>().Move(playerVelocity * Time.deltaTime);

        if (moveX > 0 || moveZ > 0)
        {
            StartBobbing();
        }
        else
        {
            StartCoroutine(StopBobbing(0.05f));
        }
    }

    private void StartBobbing()
    {
        playerCam.gameObject.GetComponent<Animator>().Play("HeadBob");

        if (currentSpeed == 12)
        {
            playerCam.gameObject.GetComponent<Animator>().speed = 1;
        }
        else
        {
            playerCam.gameObject.GetComponent<Animator>().speed = 1.5f;
        }
    }

    private IEnumerator StopBobbing(float duration)
    {
        float elapsedTime = 0f;

        Vector3 originalPos = new Vector3(0, 0.8f, 0);
        Vector3 currentPos = playerCam.transform.localPosition;

        playerCam.gameObject.GetComponent<Animator>().Play("New State");
        playerCam.gameObject.GetComponent<Animator>().enabled = false;

        while (elapsedTime < duration)
        {
            playerCam.transform.localPosition = Vector3.Lerp(currentPos, originalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerCam.transform.localPosition = originalPos;
        playerCam.gameObject.GetComponent<Animator>().enabled = true;
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
}
