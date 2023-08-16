using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{

    private const int LINEAR_SQUARE_VALUE = 10;
    private const int DIAGONAL_SQUARE_VALUE = 14; // aprox hypotenuse for a square triangle with LINEAR_SQUARE_VALUE sides

    /// <summary>
    /// Builds a path for the room, from the startGridPos to the endGridPos, and adds movement steps to the returned Stack.
    /// </summary>
    /// <param name="room">A Room object to get the prefered path</param>
    /// <param name="startGridPos">AStar start position</param>
    /// <param name="endGridPos">AStar end position</param>
    /// <returns>Null if no path is found</returns>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPos, Vector3Int endGridPos)
    {
        startGridPos -= (Vector3Int)room.templateLowerBounds;
        endGridPos -= (Vector3Int)room.templateLowerBounds;

        var openNodeList = new List<Node>();
        var closedNodeHashSet = new HashSet<Node>();

        var gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        var startNode = gridNodes.GetGridNode(startGridPos.x, startGridPos.y);
        var targetNode = gridNodes.GetGridNode(endGridPos.x, endGridPos.y);

        var endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, room.instantiatedRoom);

        if (endPathNode != null)
            return CreatePathStack(endPathNode, room);

        return null;
    }

    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        var movementPathStack = new Stack<Vector3>();

        var nextNode = targetNode;

        var cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            var worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x, nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }

    /// <summary>
    /// Find the shortest path
    /// </summary
    /// <returns>The end Node if a path has been found, else returns null</returns>
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();

            var currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            if (currentNode == targetNode)
                return currentNode;

            closedNodeHashSet.Add(currentNode);

            EvaluateCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);
        }

        return null;
    }

    private static void EvaluateCurrentNodeNeighbours(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        var currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        //Loop through all directions
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                if (validNeighbourNode != null)
                {
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighbourNode.gridPosition.x, validNeighbourNode.gridPosition.y];

                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + movementPenaltyForGridSpace;

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the distance int between nodeA and nodeB
    /// </summary>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return DIAGONAL_SQUARE_VALUE * dstY + LINEAR_SQUARE_VALUE * (dstX - dstY);
        return DIAGONAL_SQUARE_VALUE * dstX + LINEAR_SQUARE_VALUE * (dstY - dstX);
    }

    private static Node GetValidNodeNeighbour(int neighbourNodeXPos, int neighbourNodeYPos, GridNodes gridNodes, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        if (neighbourNodeXPos >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x
            || neighbourNodeXPos < 0
            || neighbourNodeYPos >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y
            || neighbourNodeYPos < 0)
        {
            return null;
        }

        var neighbourNode = gridNodes.GetGridNode(neighbourNodeXPos, neighbourNodeYPos);

        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighbourNodeXPos, neighbourNodeYPos];

        if (movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighbourNode))
            return null;

        return neighbourNode;
    }
}
