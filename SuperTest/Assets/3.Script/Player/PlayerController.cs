using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    public float speed = 10f;
    public float rotationSpeed = 15f;

    [Header("Joystick Settings")]
    public RectTransform backGround;
    public RectTransform handle;
    //public Canvas canvas;
    public float handleRange = 1f;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;
    private Vector2 firstTouch;

    private Transform cameraTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if(Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            firstTouch = Input.mousePosition;
            backGround.gameObject.SetActive(true);
            backGround.position = firstTouch;
            handle.anchoredPosition = Vector2.zero;
        }
        else if(Input.GetMouseButton(0))
        {
            Vector2 currentTouchPos = Input.mousePosition;
            Vector2 direction = currentTouchPos - firstTouch;

            float bgRadius = (backGround.rect.width / 2f);
            float distance = Mathf.Min(direction.magnitude, bgRadius);
            inputVector = direction.normalized * (distance / bgRadius);
            handle.anchoredPosition = inputVector * bgRadius * handleRange;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            backGround.gameObject.SetActive(false);
            inputVector = Vector2.zero;
        }
    }
    private void FixedUpdate()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * inputVector.y + camRight * inputVector.x).normalized;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
