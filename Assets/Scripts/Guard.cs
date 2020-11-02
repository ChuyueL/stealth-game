using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guard : MonoBehaviour
{
    public static event System.Action OnGuardHasSpottedPlayer;

    public Button killButton;

    public float speed = 5;
    public float waitTime = 0.3f;
    public float turnSpeed = 90; //90 degrees per second
    public float timeToSpotPlayer = 0.5f;

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    float viewAngle;
    float playerVisibleTimer;
    Color originalSpotlightColour;   

    public Transform pathHolder;
    Transform player;

    float interactableRadius = 1f;

    //gizmos are only visible in editor, not in final build of game
    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, 0.3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition); //to make a loop

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;

            //make sure waypoints height is same as guard's so guard does not 'sink into' ground
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z); 
        }

        StartCoroutine(FollowPath(waypoints));
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
        }
        else
        {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSpotlightColour, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if (playerVisibleTimer >= timeToSpotPlayer) //should never be > due to the Clamp but just in there for clarity
        {
            OnGuardHasSpottedPlayer?.Invoke();
        }

        
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            //guard's forward direction is at centre of viewAngle so divide by 2
            if (angleBetweenGuardAndPlayer < viewAngle/2f)
            {
                //linecast same as raycast but allows us to specify 2 pts to cast ray between
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnKillButton()
    {
        if (Vector3.Distance(transform.position, player.position) < interactableRadius)
        {
            Debug.Log("killed " + transform.name);
            Destroy(gameObject);
        }
    }
    
    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        //> 0 would be dangerous as eulerangles may never exactly reach targetangle 
        //DeltaAngle will be -ve if turn is anticlockwise, hence the abs
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null; //wait for a frame
        }
    }
    

    IEnumerator Move(Vector3 destination)
    {
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(waitTime);
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        //make sure guard is at first waypoint at start
        transform.position = waypoints[0];


        /*
        while (true)
        {
            foreach (Vector3 waypoint in waypoints)
            {
                yield return StartCoroutine(Move(waypoint));
            }
        }
        */

        //Sebastian's implementation
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {

            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint)); //wait until guard finishes turning
            }

            //wait for one frame between each iteration
            yield return null;
        }


        
    }
}
