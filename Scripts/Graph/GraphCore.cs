using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class GraphCore
    {
        public List<GraphLink> links;
        private List<MonoBehaviour> nodes;
        private Dictionary<MonoBehaviour, List<MonoBehaviour>> graph;
        private Dictionary<MonoBehaviour, List<GraphLink>> shortestGraph;

        public GraphCore(List<GraphLink> links, List<MonoBehaviour> nodes)
        {
            this.links = links;
            this.nodes = nodes;
            this.graph = GetGraphFromLinks();
        }

        public GraphCore(List<MonoBehaviour> nodes)
        {
            this.nodes = nodes;
            ConstructGraphFromNodes();
            this.graph = GetGraphFromLinks();
        }

        private void ConstructGraphFromNodes()
        {
            Debug.Log("Constructing graph from nodes : " + this.nodes.Count());
            float MAX_DISTANCE = 2;
            this.links = new List<GraphLink>();
            this.nodes.ForEach(node =>
            {
                this.nodes.FindAll(otherNode =>
                {
                    return !Object.ReferenceEquals(node, otherNode) &&
                        Vector3.Distance(node.transform.position, otherNode.transform.position) < MAX_DISTANCE;
                }).ForEach(otherNode =>
                {
                    Debug.Log("Adding link between " + node.name + " and " + otherNode.name);
                    this.links.Add(new GraphLink(node, otherNode));
                });
            });
        }

        private Dictionary<MonoBehaviour, List<MonoBehaviour>> AddLinkToGraph(Dictionary<MonoBehaviour, List<MonoBehaviour>> currentGraph, GraphLink link)
        {
            if (!currentGraph[link.nodeA].Contains(link.nodeB)) currentGraph[link.nodeA].Add(link.nodeB);
            if (!currentGraph[link.nodeB].Contains(link.nodeA) && !link.IsOriented()) currentGraph[link.nodeB].Add(link.nodeA);
            return currentGraph;
        }

        private Dictionary<MonoBehaviour, List<MonoBehaviour>> GetGraphFromLinks()
        {
            return links.Aggregate(GetEmptyGraph(), (acc, link) =>
            {
                return AddLinkToGraph(acc, link);
            });
        }

        private Dictionary<MonoBehaviour, List<GraphLink>> GetEmptyShortestGraph()
        {
            Dictionary<MonoBehaviour, List<GraphLink>> newGraph =
                new Dictionary<MonoBehaviour, List<GraphLink>>();

            if (nodes == null) return newGraph;

            return nodes.Aggregate(new Dictionary<MonoBehaviour, List<GraphLink>>(), (acc, node) =>
            {
                acc[node] = new List<GraphLink>();
                return acc;
            });
        }

        private Dictionary<MonoBehaviour, List<MonoBehaviour>> GetEmptyGraph()
        {
            Dictionary<MonoBehaviour, List<MonoBehaviour>> newGraph =
                new Dictionary<MonoBehaviour, List<MonoBehaviour>>();

            if (nodes == null) return newGraph;

            return nodes.Aggregate(new Dictionary<MonoBehaviour, List<MonoBehaviour>>(), (acc, node) =>
            {
                acc[node] = new List<MonoBehaviour>();
                return acc;
            });
        }

        private Dictionary<MonoBehaviour, List<GraphLink>> AddLinkToShortestGraph(
            Dictionary<MonoBehaviour, List<GraphLink>> currentGraph,
            GraphLink shortestLink
        )
        {
            List<GraphLink> pathLinks = new List<GraphLink>(currentGraph[shortestLink.nodeA]);
            pathLinks.Add(shortestLink);
            currentGraph[shortestLink.nodeB] = pathLinks;
            return currentGraph;
        }

        public List<MonoBehaviour> GetPath(MonoBehaviour srcNode, List<MonoBehaviour> dstNodes)
        {
            shortestGraph = GetShortestGraph(srcNode, dstNodes);

            List<MonoBehaviour> path = new List<MonoBehaviour>();

            MonoBehaviour dstNode = dstNodes.Find((dstNode) => shortestGraph[dstNode].Count != 0);

            path.Add(srcNode);
            shortestGraph[dstNode].ForEach((shortestLink) =>
            {
                path.Add(shortestLink.nodeB);
            });

            return path;
        }

        private Dictionary<MonoBehaviour, List<GraphLink>> GetShortestGraph(MonoBehaviour srcNode, List<MonoBehaviour> dstNodes)
        {
            Dictionary<MonoBehaviour, List<GraphLink>> shortestGraph = GetEmptyShortestGraph();

            List<MonoBehaviour> openedListNodes = new List<MonoBehaviour>();
            List<MonoBehaviour> closedListNodes = new List<MonoBehaviour>();

            Dictionary<MonoBehaviour, float> distanceToNodes = new Dictionary<MonoBehaviour, float>();
            Dictionary<MonoBehaviour, float> heuristicOfNodes = new Dictionary<MonoBehaviour, float>();

            nodes.ForEach((node) =>
            {
                distanceToNodes[node] = -1;
                heuristicOfNodes[node] = -1;
            });

            openedListNodes.Add(srcNode);
            distanceToNodes[srcNode] = 0;

            while (openedListNodes.Count != 0)
            {
                MonoBehaviour closestNode = FindClosestNode(openedListNodes, distanceToNodes, heuristicOfNodes);
                openedListNodes.Remove(closestNode);
                closedListNodes.Add(closestNode);

                if (dstNodes.Find((dstNode) => Object.ReferenceEquals(dstNode, closestNode)) != null) break;

                GetSuccessorLinks(closestNode).ForEach((currentLink) =>
                {
                    if (heuristicOfNodes[currentLink.nodeB] == -1) heuristicOfNodes[currentLink.nodeB] = currentLink.GetHeuristic(dstNodes);
                    float currentDistance = distanceToNodes[currentLink.nodeA] + currentLink.GetValue();

                    if (
                        (closedListNodes.Contains(currentLink.nodeB) || openedListNodes.Contains(currentLink.nodeB)) &&
                        currentDistance >= distanceToNodes[currentLink.nodeB]
                    ) return;

                    closedListNodes.Remove(currentLink.nodeB);
                    if (!openedListNodes.Contains(currentLink.nodeB)) openedListNodes.Add(currentLink.nodeB);

                    shortestGraph = AddLinkToShortestGraph(shortestGraph, currentLink);
                    distanceToNodes[currentLink.nodeB] = currentDistance;
                });
            }

            return shortestGraph;
        }

        private MonoBehaviour FindClosestNode(
            List<MonoBehaviour> openedListNodes,
            Dictionary<MonoBehaviour, float> distanceToNodes,
            Dictionary<MonoBehaviour, float> heuristicOfNodes
        )
        {
            MonoBehaviour closestNode = openedListNodes[0];
            float bestF = distanceToNodes[closestNode] + heuristicOfNodes[closestNode];

            openedListNodes.ForEach((currentNode) =>
            {
                float currentF = distanceToNodes[currentNode] + heuristicOfNodes[currentNode];

                Debug.Log("currentNode: " + currentNode.name + " currentF: " + currentF);

                if (currentF < bestF)
                {
                    closestNode = currentNode;
                    bestF = currentF;
                }
            });

            Debug.Log("closestNode: " + closestNode.name + " bestF: " + bestF);

            return closestNode;
        }

        private List<GraphLink> GetSuccessorLinks(MonoBehaviour node)
        {
            return graph[node].Select((adjacentNode) =>
            {
                return new GraphLink(node, adjacentNode, true);
            }).ToList();
        }

        // debugs

        public void DrawGraph(Color color, float duration, Dictionary<MonoBehaviour, List<MonoBehaviour>> currentGraph = null)
        {
            if (currentGraph == null) currentGraph = graph;
            currentGraph.Keys.ToList().ForEach((currentNode) =>
            {
                List<MonoBehaviour> adjacentNodes = currentGraph[currentNode];
                adjacentNodes.ForEach((adjacentNode) =>
                {
                    Debug.DrawLine(currentNode.transform.position, adjacentNode.transform.position, color, duration);
                });
            });
        }

        public void DrawShortestGraph(Color color, float duration, Dictionary<MonoBehaviour, List<GraphLink>> currentGraph = null)
        {
            if (currentGraph == null) currentGraph = shortestGraph;
            if (currentGraph == null) return;

            currentGraph.Keys.ToList().ForEach((currentNode) =>
            {
                List<GraphLink> shortestLinks = currentGraph[currentNode];
                shortestLinks.ForEach((link) =>
                {
                    Debug.DrawLine(link.nodeA.transform.position, link.nodeB.transform.position, color, duration);
                });
            });
        }

        public void LogGraph(Dictionary<MonoBehaviour, List<MonoBehaviour>> currentGraph = null)
        {
            if (currentGraph == null) currentGraph = graph;
            currentGraph.Keys.ToList().ForEach((key) =>
            {
                List<MonoBehaviour> values = currentGraph[key];
                Debug.Log("key : ");
                Debug.Log(key.transform.name);

                Debug.Log("values : ");
                values.ForEach((value) =>
                {
                    Debug.Log(value.transform.name);
                });
            });
        }

    }

}