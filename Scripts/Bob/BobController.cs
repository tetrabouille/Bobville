using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using BobVille.Graph;

namespace BobVille.Bob
{
    public class BobController : MonoBehaviour
    {
        [SerializeField] private float WALK_SPEED = 10f;
        [SerializeField] private float ROTATION_SPEED = 4f;
        [SerializeField] private float WALK_EPSILON = 0.8f;
        [SerializeField] private float WALK_TIME = 10f;
        [SerializeField] private float ROTATION_EPSILON = 0.01f;
        [SerializeField] private float WORK_TIME = 5f;
        [SerializeField] private float WORK_SPEED = 1f;

        private MonoBehaviour currentNode;
        private ActionState actionState = ActionState.MoveOnPath;
        private MoveOnPathState moveOnPathState = MoveOnPathState.Turning;
        private List<MonoBehaviour> path;
        private AutoGraphController graphController;
        private int pathIndex = 0;
        private float currentWorkTime = 0f;
        private float currentWalkTime = 0f;
        private List<MonoBehaviour> nodes;
        private Rigidbody rb;

        // Start is called before the first frame update
        void Start()
        {
            graphController = GameObject.FindObjectsOfType<AutoGraphController>().Single((g) => g.tag == "Graph");
            nodes = graphController.GetComponentsInChildren<NodeController>().Cast<MonoBehaviour>().ToList();
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {

            TakeAction();

        }

        void TakeAction()
        {
            rb.angularVelocity = Vector3.zero;

            switch (actionState)
            {
                case ActionState.Work:
                    Work();
                    break;
                case ActionState.MoveOnPath:
                    MoveOnPath();
                    break;
            }
        }

        private void Work()
        {
            currentWorkTime += WORK_SPEED * Time.deltaTime;
            transform.Rotate(Vector3.up * ROTATION_SPEED * Time.deltaTime * 5);

            if (currentWorkTime > WORK_TIME)
            {
                currentWorkTime = 0;
                ChangeActionState(ActionState.MoveOnPath);
                ChangeMoveOnPathState(MoveOnPathState.Turning);
            }
        }

        private void MoveOnPath()
        {
            if (path == null)
            {
                pathIndex = 0;
                if (this.currentNode == null) this.currentNode = nodes[Random.Range(0, nodes.Count)];

                MonoBehaviour randomTarget = nodes[Random.Range(0, nodes.Count)];
                while (randomTarget == this.currentNode) randomTarget = nodes[Random.Range(0, nodes.Count)];

                path = graphController.graphCore.GetPath(this.currentNode, new List<MonoBehaviour>() { randomTarget });
            }

            if (path.Count <= pathIndex || path.Count == 0)
            {
                pathIndex = 0;
                ChangeActionState(ActionState.Work);
                path = null;
                return;
            }

            this.currentNode = path[pathIndex];
            switch (moveOnPathState)
            {
                case MoveOnPathState.Turning:
                    TurnOnPath(this.currentNode);
                    break;
                case MoveOnPathState.Walking:
                    WalkOnPath(this.currentNode);
                    break;
            }
        }

        private void ChangeActionState(ActionState newState)
        {
            Debug.Log($"action state : ${newState}");
            ChangeMoveOnPathState(MoveOnPathState.Idle);
            actionState = newState;
        }

        private void ChangeMoveOnPathState(MoveOnPathState newState)
        {
            Debug.Log($"move on path state : ${newState}");
            moveOnPathState = newState;
        }

        private void TurnOnPath(MonoBehaviour currentNode)
        {
            Vector3 targetVector = new Vector3(
                currentNode.transform.position.x - transform.position.x,
                0,
                currentNode.transform.position.z - transform.position.z
            );

            Vector3 targetPosition = new Vector3(
                currentNode.transform.position.x,
                transform.position.y,
                currentNode.transform.position.z
            );

            if (isOnTarget(targetPosition))
            {
                pathIndex++;
                return;
            }

            Debug.DrawRay(transform.position, targetVector, Color.green, 0.1f);

            Vector3 smoothTarget = Vector3.RotateTowards(
                new Vector3(transform.forward.x, 0, transform.forward.z),
                targetVector,
                Time.deltaTime * ROTATION_SPEED,
                0
            );

            Debug.DrawRay(transform.position, smoothTarget, Color.red, 0.1f);

            if (isLookingAtTarget(targetVector))
            {
                transform.rotation = Quaternion.LookRotation(targetVector);
                ChangeMoveOnPathState(MoveOnPathState.Walking);
                return;
            }

            transform.rotation = Quaternion.LookRotation(smoothTarget);
        }

        private void WalkOnPath(MonoBehaviour currentNode)
        {
            Vector3 target = new Vector3(
                currentNode.transform.position.x,
                transform.position.y,
                currentNode.transform.position.z
            );
            if (isOnTarget(target))
            {
                ChangeMoveOnPathState(MoveOnPathState.Turning);
                pathIndex++;
                return;
            }

            Debug.DrawLine(transform.position, target, Color.green, 0.1f);

            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * WALK_SPEED);
        }

        private bool isOnTarget(Vector3 target)
        {
            return Vector3.Distance(transform.position, target) <= WALK_EPSILON;
        }

        private bool isLookingAtTarget(Vector3 target)
        {
            return Vector3.Dot(Vector3.Normalize(target), transform.TransformDirection(Vector3.forward)) >= 1 - ROTATION_EPSILON;
        }
    }
}