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

        public GraphCore(List<GraphLink> links, List<NodeController> nodes)
        {
            this.links = links;
            this.nodes = nodes;
            this.graph = GetGraphFromLinks();
        }

        private Dictionary<NodeController, List<NodeController>> AddLinkToGraph(Dictionary<NodeController, List<NodeController>> currentGraph, GraphLink link)
        {
            if (!currentGraph[link.nodeA].Contains(link.nodeB)) currentGraph[link.nodeA].Add(link.nodeB);
            if (!currentGraph[link.nodeB].Contains(link.nodeA)) currentGraph[link.nodeB].Add(link.nodeA);
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
            Dictionary<NodeController, List<GraphLink>> shortestGraph = GetShortestGraph(srcNode);

            DrawShortestGraph(shortestGraph, Color.green, 3);

            List<NodeController> path = new List<NodeController>();

            path.Add(srcNode);
            shortestGraph[dstNode].ForEach((shortestLink) =>
            {
                path.Add(shortestLink.nodeB);
            });

            return path;
        }

        private Dictionary<NodeController, List<GraphLink>> GetShortestGraph(NodeController initialNode)
        {
            Dictionary<NodeController, List<GraphLink>> shortestGraph = GetEmptyShortestGraph();
            Dictionary<NodeController, bool> visitedNodes = new Dictionary<NodeController, bool>();
            Dictionary<NodeController, float> distanceToNodes = new Dictionary<NodeController, float>();

            nodes.ForEach((node) =>
            {
                visitedNodes[node] = false;
                distanceToNodes[node] = -1;
            });

            visitedNodes[initialNode] = true;
            distanceToNodes[initialNode] = 0;

            List<GraphLink> candidateLinks = GetNextCandidateLinks(visitedNodes);
            if (candidateLinks.Count == 0) return shortestGraph;

            while (candidateLinks.Count != 0)
            {
                GraphLink shortestLink = null;
                float shortestDistance = -1;

                candidateLinks.ForEach((currentLink) =>
                {
                    float linkDistance = currentLink.GetValue();
                    float currentDistance = distanceToNodes[currentLink.nodeA];
                    float previousDistance = distanceToNodes[currentLink.nodeB];

                    float newDistance = currentDistance == -1 ? linkDistance : currentDistance + linkDistance;

                    if (previousDistance == -1) distanceToNodes[currentLink.nodeB] = newDistance;
                    else if (newDistance < previousDistance) distanceToNodes[currentLink.nodeB] = newDistance;

                    if (shortestDistance == -1 || newDistance < shortestDistance)
                    {
                        shortestDistance = newDistance;
                        shortestLink = currentLink;
                    }
                });
                if (shortestLink == null) break;

                visitedNodes[shortestLink.nodeB] = true;

                shortestGraph = AddLinkToShortestGraph(shortestGraph, shortestLink);
                candidateLinks = GetNextCandidateLinks(visitedNodes);
            }

            return shortestGraph;
        }

        private List<GraphLink> GetNextCandidateLinks(Dictionary<NodeController, bool> visitedNodes)
        {
            return visitedNodes.Aggregate(
                new List<GraphLink>(),
                (acc, nodePair) =>
                {
                    if (!nodePair.Value) return acc;

                    NodeController node = nodePair.Key;
                    graph[node].ForEach((adjacentNode) =>
                    {
                        if (visitedNodes[adjacentNode]) return;
                        acc.Add(new GraphLink(node, adjacentNode, true));
                    });

                    return acc;
                }
            );
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

        static public void DrawShortestGraph(Dictionary<NodeController, List<GraphLink>> currentGraph, Color color, float duration)
        {
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