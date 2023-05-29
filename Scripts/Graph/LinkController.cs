using UnityEngine;

namespace BobVille.Graph
{
    public class LinkController : MonoBehaviour
    {
        [SerializeField] private NodeController nodeA;
        [SerializeField] private NodeController nodeB;
        [SerializeField] private bool oriented;

        public GraphLink link;

        public LinkController()
        {
        }

        private void Start()
        {
            this.link = new GraphLink(nodeA, nodeB, oriented);
        }
    }
}