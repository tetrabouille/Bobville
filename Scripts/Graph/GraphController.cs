using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class GraphController : MonoBehaviour
    {
        public List<LinkController> links;
        private List<NodeController> nodes;
        private Dictionary<NodeController, List<NodeController>> graph;

        // Start is called before the first frame update
        void Start()
        {
            nodes = gameObject.transform.Find("Nodes").GetComponentsInChildren<NodeController>().ToList();
            links = gameObject.transform.Find("Links").GetComponentsInChildren<LinkController>().ToList();
            graph = GetGraphFromLinks();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                DrawGraph(graph, Color.blue, 0.1f);
            }
        }

        private void DrawGraph(Dictionary<NodeController, List<NodeController>> currentGraph, Color color, float duration)
        {
            currentGraph.Keys.ToList().ForEach((currentNode) =>
            {
                List<NodeController> adjacentNodes = currentGraph[currentNode];
                adjacentNodes.ForEach((adjacentNode) =>
                {
                    Debug.DrawLine(currentNode.transform.position, adjacentNode.transform.position, color, duration);
                });
            });
        }

        private void DrawShortestGraph(Dictionary<NodeController, List<LinkController>> currentGraph, Color color, float duration)
        {
            currentGraph.Keys.ToList().ForEach((currentNode) =>
            {
                List<LinkController> shortestLinks = currentGraph[currentNode];
                shortestLinks.ForEach((link) =>
                {
                    Debug.DrawLine(link.nodeA.transform.position, link.nodeB.transform.position, color, duration);
                });
            });
        }

        private void LogGraph(Dictionary<NodeController, List<NodeController>> currentGraph)
        {
            currentGraph.Keys.ToList().ForEach((key) =>
            {
                List<NodeController> values = currentGraph[key];
                Debug.Log("key : ");
                Debug.Log(key.transform.name);

                Debug.Log("values : ");
                values.ForEach((value) =>
                {
                    Debug.Log(value.transform.name);
                });
            });
        }

        private Dictionary<NodeController, List<NodeController>> GetGraphFromLinks()
        {
            return links.Aggregate(GetEmptyGraph(), (acc, link) =>
            {
                return AddLinkToGraph(acc, link);
            });
        }

        private Dictionary<NodeController, List<NodeController>> AddLinkToGraph(Dictionary<NodeController, List<NodeController>> currentGraph, LinkController link)
        {
            if (!currentGraph[link.nodeA].Contains(link.nodeB)) currentGraph[link.nodeA].Add(link.nodeB);
            if (!currentGraph[link.nodeB].Contains(link.nodeA)) currentGraph[link.nodeB].Add(link.nodeA);
            return currentGraph;
        }

        private Dictionary<NodeController, List<LinkController>> AddLinkToShortestGraph(
            Dictionary<NodeController, List<LinkController>> currentGraph,
            LinkController shortestLink,
            NodeController adjacentNode,
            NodeController previousNode
            )
        {
            shortestLink.SetPreviousNext(adjacentNode);
            List<LinkController> pathLinks = new List<LinkController>(currentGraph[previousNode]);
            pathLinks.Add(shortestLink);
            currentGraph[adjacentNode] = pathLinks;
            return currentGraph;
        }

        private Dictionary<NodeController, List<LinkController>> GetEmptyShortestGraph()
        {
            Dictionary<NodeController, List<LinkController>> newGraph =
                new Dictionary<NodeController, List<LinkController>>();

            if (nodes == null) return newGraph;

            return nodes.Aggregate(new Dictionary<NodeController, List<LinkController>>(), (acc, node) =>
            {
                acc[node] = new List<LinkController>();
                return acc;
            });
        }

        private Dictionary<NodeController, List<NodeController>> GetEmptyGraph()
        {
            Dictionary<NodeController, List<NodeController>> newGraph =
                new Dictionary<NodeController, List<NodeController>>();

            if (nodes == null) return newGraph;

            return nodes.Aggregate(new Dictionary<NodeController, List<NodeController>>(), (acc, node) =>
            {
                acc[node] = new List<NodeController>();
                return acc;
            });
        }

        private LinkController GetLinkFromNodes(NodeController nodeA, NodeController nodeB)
        {
            return links.Find((link) =>
            {
                return link.Contains(nodeA) && link.Contains(nodeB);
            });
        }

        public List<NodeController> GetPath(NodeController srcNode, NodeController dstNode)
        {
            Dictionary<NodeController, List<LinkController>> shortestGraph = GetShortestGraph(srcNode);

            DrawShortestGraph(shortestGraph, Color.green, 3);

            List<NodeController> path = new List<NodeController>();

            path.Add(srcNode);
            shortestGraph[dstNode].ForEach((shortestLink) =>
            {
                path.Add(shortestLink.nextNode);
            });

            return path;
        }

        private Dictionary<NodeController, List<LinkController>> GetShortestGraph(NodeController initialNode)
        {
            Dictionary<NodeController, List<LinkController>> shortestGraph = GetEmptyShortestGraph();
            Dictionary<NodeController, bool> visitedNodes = new Dictionary<NodeController, bool>();
            Dictionary<NodeController, float> distanceToNodes = new Dictionary<NodeController, float>();

            nodes.ForEach((node) =>
            {
                visitedNodes[node] = false;
                distanceToNodes[node] = -1;
            });

            visitedNodes[initialNode] = true;
            distanceToNodes[initialNode] = 0;

            List<LinkController> candidateLinks = GetNextCandidateLinks(visitedNodes);
            if (candidateLinks.Count == 0) return shortestGraph;

            while (candidateLinks.Count != 0)
            {
                LinkController shortestLink = null;
                float shortestDistance = -1;

                candidateLinks.ForEach((currentLink) =>
                {
                    NodeController adjacentNode = GetAdjacentNode(visitedNodes, currentLink);
                    NodeController currentNode = GetCurrentNode(visitedNodes, currentLink);

                    float linkDistance = currentLink.GetValue();
                    float currentDistance = distanceToNodes[currentNode];
                    float previousDistance = distanceToNodes[adjacentNode];

                    float newDistance = currentDistance == -1 ? linkDistance : currentDistance + linkDistance;

                    if (previousDistance == -1) distanceToNodes[adjacentNode] = newDistance;
                    else if (newDistance < previousDistance) distanceToNodes[adjacentNode] = newDistance;

                    if (shortestDistance == -1 || newDistance < shortestDistance)
                    {
                        shortestDistance = newDistance;
                        shortestLink = currentLink;
                    }
                });
                if (shortestLink == null) break;

                NodeController adjacentNode = GetAdjacentNode(visitedNodes, shortestLink);
                NodeController currentNode = GetCurrentNode(visitedNodes, shortestLink);
                visitedNodes[adjacentNode] = true;

                shortestGraph = AddLinkToShortestGraph(shortestGraph, shortestLink, adjacentNode, currentNode);
                candidateLinks = GetNextCandidateLinks(visitedNodes);
            }


            return shortestGraph;
        }

        private NodeController GetAdjacentNode(Dictionary<NodeController, bool> visitedNodes, LinkController link)
        {
            if (!visitedNodes[link.nodeA]) return link.nodeA;
            return link.nodeB;
        }

        private NodeController GetCurrentNode(Dictionary<NodeController, bool> visitedNodes, LinkController link)
        {
            if (visitedNodes[link.nodeA] == true) return link.nodeA;
            return link.nodeB;
        }

        private List<LinkController> GetNextCandidateLinks(Dictionary<NodeController, bool> visitedNodes)
        {
            return visitedNodes.Aggregate(
                new List<LinkController>(),
                (acc, nodePair) =>
                {
                    if (!nodePair.Value) return acc;

                    NodeController node = nodePair.Key;
                    graph[node].ForEach((adjacentNode) =>
                    {
                        if (visitedNodes[adjacentNode]) return;
                        acc.Add(GetLinkFromNodes(node, adjacentNode));
                    });

                    return acc;
                }
            );
        }
    }
}