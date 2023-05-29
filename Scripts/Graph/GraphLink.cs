using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class GraphLink
    {
        public NodeController nodeA;
        public NodeController nodeB;
        public Vector3 direction;
        private bool oriented;

        public GraphLink(NodeController nodeA, NodeController nodeB, bool oriented = false)
        {
            this.nodeA = nodeA;
            this.nodeB = nodeB;
            this.direction = nodeB.transform.position - nodeA.transform.position;
            this.oriented = oriented;
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

        public void SetOrientation(NodeController node)
        {
            if (Object.ReferenceEquals(nodeA, node)) return;
            this.nodeB = this.nodeA;
            this.nodeA = node;
            this.oriented = true;
        }

        public Ray GetRay()
        {
            return new Ray(nodeA.transform.position, direction);
        }
    }
}

