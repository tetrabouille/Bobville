using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class AutoGraphController : MonoBehaviour
    {
        public GraphCore graphCore;

        // Start is called before the first frame update
        void Start()
        {
            List<MonoBehaviour> nodes = gameObject.GetComponentsInChildren<NodeController>().Cast<MonoBehaviour>().ToList();
            graphCore = new GraphCore(nodes);
        }

        void Update()
        {
            graphCore.DrawShortestGraph(Color.green, 0.1f);
        }

    }

}
