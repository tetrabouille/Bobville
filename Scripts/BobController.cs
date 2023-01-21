using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum BobState
{
    Turning,
    Walking
}

public class BobController : MonoBehaviour
{
    [SerializeField] PointController[] path;
    [SerializeField] float WALK_SPEED = 10f;
    [SerializeField] float ROTATION_SPEED = 4f;
    [SerializeField] float POINT_EPSILON = 0.8f;
    [SerializeField] float ROTATION_EPSILON = 0.01f;


    private int pathIndex = 0;
    private BobState state = BobState.Turning;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        if (path.Length <= pathIndex) pathIndex = 0;
        PointController currentPoint = path[pathIndex];
        switch (state)
        {
            case BobState.Turning:
                Turn(currentPoint);
                break;
            case BobState.Walking:
                Walk(currentPoint);
                break;
        }
    }

    void Turn(PointController currentPoint)
    {
        Vector3 target = new Vector3(
            currentPoint.transform.position.x - transform.position.x,
            transform.position.y,
            currentPoint.transform.position.z - transform.position.z
        );

        Vector3 smoothTarget = Vector3.RotateTowards(
            transform.forward,
            target,
            Time.deltaTime * ROTATION_SPEED,
            0
        );

        if (Vector3.Dot(Vector3.Normalize(target), transform.TransformDirection(Vector3.forward)) >= 1 - ROTATION_EPSILON)
        {
            transform.rotation = Quaternion.LookRotation(target);
            state = BobState.Walking;
            return;
        }

        Quaternion rotationTarget =
        transform.rotation = Quaternion.LookRotation(smoothTarget);
    }

    void Walk(PointController currentPoint)
    {
        Vector3 target = new Vector3(
            currentPoint.transform.position.x,
            transform.position.y,
            currentPoint.transform.position.z
        );
        Debug.Log(Vector3.Distance(transform.position, target));
        if (Vector3.Distance(transform.position, target) <= POINT_EPSILON)
        {
            state = BobState.Turning;
            pathIndex++;
            return;
        }
        transform.Translate(Vector3.forward * Time.deltaTime * WALK_SPEED);
    }
}
