using UnityEngine;

namespace BobVille.Graph
{
    public class LinkController : MonoBehaviour
    {
        public NodeController nodeA;
        public NodeController nodeB;

        public LinkController(NodeController nodeA, NodeController nodeB)
        {
            this.nodeA = nodeA;
            this.nodeB = nodeB;
        }

        public float GetValue()
        {
            return GetDistance();
        }

        public float GetDistance()
        {
            if (nodeA == null || nodeB == null) return -1;
            return Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
        }

        public bool Contains(NodeController node)
        {
            return Object.ReferenceEquals(nodeA, node) ||
                Object.ReferenceEquals(nodeB, node);
        }
    }
}