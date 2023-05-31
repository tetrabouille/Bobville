using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BobVille.Graph
{
    public class GraphCore
    {
        public List<GraphLink> links;
        private List<NodeController> nodes;
        private Dictionary<NodeController, List<NodeController>> graph;
        private Dictionary<NodeController, List<GraphLink>> shortestGraph;

        public GraphCore(List<GraphLink> links, List<NodeController> nodes)
        {
            this.links = links;
            this.nodes = nodes;
            this.graph = GetGraphFromLinks();
        }

        public GraphCore(List<NodeController> nodes)
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

        private Dictionary<NodeController, List<NodeController>> AddLinkToGraph(Dictionary<NodeController, List<NodeController>> currentGraph, GraphLink link)
        {
            if (!currentGraph[link.nodeA].Contains(link.nodeB)) currentGraph[link.nodeA].Add(link.nodeB);
            if (!currentGraph[link.nodeB].Contains(link.nodeA) && !link.IsOriented()) currentGraph[link.nodeB].Add(link.nodeA);
            return currentGraph;
        }

        private Dictionary<NodeController, List<NodeController>> GetGraphFromLinks()
        {
            return links.Aggregate(GetEmptyGraph(), (acc, link) =>
            {
                return AddLinkToGraph(acc, link);
            });
        }

        private Dictionary<NodeController, List<GraphLink>> GetEmptyShortestGraph()
        {
            Dictionary<NodeController, List<GraphLink>> newGraph =
                new Dictionary<NodeController, List<GraphLink>>();

            if (nodes == null) return newGraph;

            return nodes.Aggregate(new Dictionary<NodeController, List<GraphLink>>(), (acc, node) =>
            {
                acc[node] = new List<GraphLink>();
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

        private Dictionary<NodeController, List<GraphLink>> AddLinkToShortestGraph(
            Dictionary<NodeController, List<GraphLink>> currentGraph,
            GraphLink shortestLink
        )
        {
            List<GraphLink> pathLinks = new List<GraphLink>(currentGraph[shortestLink.nodeA]);
            pathLinks.Add(shortestLink);
            currentGraph[shortestLink.nodeB] = pathLinks;
            return currentGraph;
        }

        public List<NodeController> GetPath(NodeController srcNode, NodeController dstNode)
        {
            shortestGraph = GetShortestGraph(srcNode, dstNode);

            List<NodeController> path = new List<NodeController>();

            path.Add(srcNode);
            shortestGraph[dstNode].ForEach((shortestLink) =>
            {
                path.Add(shortestLink.nodeB);
            });

            return path;
        }

        private Dictionary<NodeController, List<GraphLink>> GetShortestGraph(NodeController srcNode, NodeController dstNode)
        {
            Dictionary<NodeController, List<GraphLink>> shortestGraph = GetEmptyShortestGraph();

            List<NodeController> openedListNodes = new List<NodeController>();
            List<NodeController> closedListNodes = new List<NodeController>();

            Dictionary<NodeController, float> distanceToNodes = new Dictionary<NodeController, float>();
            Dictionary<NodeController, float> heuristicOfNodes = new Dictionary<NodeController, float>();

            nodes.ForEach((node) =>
            {
                distanceToNodes[node] = -1;
                heuristicOfNodes[node] = -1;
            });

            openedListNodes.Add(srcNode);
            distanceToNodes[srcNode] = 0;

            while (openedListNodes.Count != 0)
            {
                NodeController closestNode = FindClosestNode(openedListNodes, distanceToNodes, heuristicOfNodes);
                openedListNodes.Remove(closestNode);
                closedListNodes.Add(closestNode);

                if (Object.ReferenceEquals(closestNode, dstNode)) break;

                GetSuccessorLinks(closestNode).ForEach((currentLink) =>
                {
                    if (heuristicOfNodes[currentLink.nodeB] == -1) heuristicOfNodes[currentLink.nodeB] = currentLink.GetHeuristic(dstNode);
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

        private NodeController FindClosestNode(
            List<NodeController> openedListNodes,
            Dictionary<NodeController, float> distanceToNodes,
            Dictionary<NodeController, float> heuristicOfNodes
        )
        {
            NodeController closestNode = openedListNodes[0];
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

        private List<GraphLink> GetSuccessorLinks(NodeController node)
        {
            return graph[node].Select((adjacentNode) =>
            {
                return new GraphLink(node, adjacentNode, true);
            }).ToList();
        }

        // debugs

        public void DrawGraph(Color color, float duration, Dictionary<NodeController, List<NodeController>> currentGraph = null)
        {
            if (currentGraph == null) currentGraph = graph;
            currentGraph.Keys.ToList().ForEach((currentNode) =>
            {
                List<NodeController> adjacentNodes = currentGraph[currentNode];
                adjacentNodes.ForEach((adjacentNode) =>
                {
                    Debug.DrawLine(currentNode.transform.position, adjacentNode.transform.position, color, duration);
                });
            });
        }

        public void DrawShortestGraph(Color color, float duration, Dictionary<NodeController, List<GraphLink>> currentGraph = null)
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

        public void LogGraph(Dictionary<NodeController, List<NodeController>> currentGraph = null)
        {
            if (currentGraph == null) currentGraph = graph;
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

    }

}