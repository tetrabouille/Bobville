using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class GraphController : MonoBehaviour
    {
        public GraphCore graphCore;

        // Start is called before the first frame update
        void Start()
        {
            List<MonoBehaviour> nodes = gameObject.transform.Find("Nodes").GetComponentsInChildren<NodeController>().Cast<MonoBehaviour>().ToList();
            List<LinkController> links = gameObject.transform.Find("Links").GetComponentsInChildren<LinkController>().ToList();
            graphCore = new GraphCore(links.Select(link => link.link).ToList(), nodes);
        }

        // Update is called once per frame
        void Update()
        {
            graphCore.DrawShortestGraph(Color.green, 0.1f);

        }
    }
}