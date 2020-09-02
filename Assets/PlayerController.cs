using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 7;
    public float smoothMoveTime = 0.1f; //time taken for smoothinputmagnitude to catch up with target input magnitude
    public float turnSpeed = 8;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float inputMagnitude = inputDirection.magnitude; //1 if any arrow key input, 0 otherwise
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        velocity = transform.forward * moveSpeed * smoothInputMagnitude;

        float targetAngle = 90 - Mathf.Atan2(inputDirection.z, inputDirection.x) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

    }

    void FixedUpdate()
    {
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
    }

    IEnumerator TurnToFace(Vector3 inputDirection)
    {
        float targetAngle = 90 - Mathf.Atan2(inputDirection.z, inputDirection.x) * Mathf.Rad2Deg;

        //> 0 would be dangerous as eulerangles may never exactly reach targetangle 
        //DeltaAngle will be -ve if turn is anticlockwise, hence the abs
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null; //wait for a frame
        }
    }
}
