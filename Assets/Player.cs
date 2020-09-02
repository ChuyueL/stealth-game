using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 7;
    public float turnSpeed = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float inputAngle = 90 - Mathf.Atan2(inputDirection.z, inputDirection.x) * Mathf.Rad2Deg;

        StartCoroutine(MovePlayer(inputDirection));

        

        
    }

    IEnumerator MovePlayer(Vector3 inputDirection)
    {
        Vector3 velocity = speed * inputDirection;
        transform.Translate(velocity * Time.deltaTime, Space.World);
        yield return StartCoroutine(TurnToFace(inputDirection));
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
