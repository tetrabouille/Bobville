using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using BobVille.Graph;

namespace BobVille.Bob
{
    public class BobController : MonoBehaviour
    {
        [SerializeField] NodeController srcNode;
        [SerializeField] NodeController dstNode;

        [SerializeField] float WALK_SPEED = 10f;
        [SerializeField] float ROTATION_SPEED = 4f;
        [SerializeField] float WALK_EPSILON = 0.8f;
        [SerializeField] float ROTATION_EPSILON = 0.01f;
        [SerializeField] float WORK_TIME = 5f;

        private ActionState actionState = ActionState.MoveOnPath;
        private MoveOnPathState moveOnPathState = MoveOnPathState.Turning;
        private List<NodeController> path;
        private GraphController graphController;
        private int pathIndex = 0;
        private float currentWorkTime = 0f;

        // Start is called before the first frame update
        void Start()
        {
            graphController = GameObject.FindObjectsOfType<GraphController>().Single((g) => g.tag == "Graph");
        }

        // Update is called once per frame
        void Update()
        {
            if (path == null) path = graphController.GetPath(srcNode, dstNode);

            TakeAction();
        }

        void TakeAction()
        {
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

        }

        private void MoveOnPath()
        {
            if (path == null || path.Count == 0) return;

            if (path.Count <= pathIndex) pathIndex = 0;
            NodeController currentNode = path[pathIndex];
            switch (moveOnPathState)
            {
                case MoveOnPathState.Turning:
                    TurnOnPath(currentNode);
                    break;
                case MoveOnPathState.Walking:
                    WalkOnPath(currentNode);
                    break;
            }
        }

        private void TurnOnPath(NodeController currentNode)
        {
            Vector3 target = new Vector3(
                currentNode.transform.position.x - transform.position.x,
                0,
                currentNode.transform.position.z - transform.position.z
            );

            Debug.DrawRay(transform.position, target, Color.red, 0.1f);

            Vector3 smoothTarget = Vector3.RotateTowards(
                new Vector3(transform.forward.x, 0, transform.forward.z),
                target,
                Time.deltaTime * ROTATION_SPEED,
                0
            );

            Debug.DrawRay(transform.position, smoothTarget, Color.red, 0.1f);

            if (Vector3.Dot(Vector3.Normalize(target), transform.TransformDirection(Vector3.forward)) >= 1 - ROTATION_EPSILON)
            {
                transform.rotation = Quaternion.LookRotation(target);
                moveOnPathState = MoveOnPathState.Walking;
                return;
            }

            Quaternion rotationTarget =
            transform.rotation = Quaternion.LookRotation(smoothTarget);
        }

        private void WalkOnPath(NodeController currentNode)
        {
            Vector3 target = new Vector3(
                currentNode.transform.position.x,
                transform.position.y,
                currentNode.transform.position.z
            );
            if (Vector3.Distance(transform.position, target) <= WALK_EPSILON)
            {
                moveOnPathState = MoveOnPathState.Turning;
                pathIndex++;
                return;
            }

            Debug.DrawRay(transform.position, target, Color.red, 0.1f);

            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * WALK_SPEED);
        }
    }
}