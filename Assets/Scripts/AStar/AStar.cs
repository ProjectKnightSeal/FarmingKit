using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("Tiles & Tilemap References")]
    [Header("Options")]
    [SerializeField] private bool observeMovementPenalties = true;

    [Range(0, 20)]
    [SerializeField] private int pathMovementPenalty = 0;
    [Range(0, 20)]
    [SerializeField] private int defaultMovementPenalty = 0;

    private GridNodes gridNodes;
    private Node startNode;
    private Node targetNode;
    private int gridWidth;
    private int gridHeight;
    private int originX;
    private int originY;

    private List<Node> openNodeList;
    private HashSet<Node> closedNodeList;

    private bool pathFound = false;

    //Builds a path for the given sceneName, from the startGridPosition to the endGridPosition, and adds movement steps to the passed in npcMovementStack. Also returns true if path found or false if no path found.
    public bool BuildPath(SceneName sceneName,Vector2Int startGridPosition,Vector2Int endGridPosition,Stack<NPCMovementStep> npcMovementStepStack)
    {
        if (PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, endGridPosition))
        {
            if (FindShortestPath())
            {
                UpdatePathOnNPCMovementStepStack(sceneName, npcMovementStepStack);

                return true;
            }
        }

        return false;
    }

    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
    {
        Node nextNode = targetNode;

        while(nextNode != null)
        {
            NPCMovementStep npcMovementStep = new NPCMovementStep();

            npcMovementStep.sceneName = sceneName;
            npcMovementStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);

            npcMovementStepStack.Push(npcMovementStep);

            nextNode = nextNode.parentNode;
        }
    }

    //Returns true if a path has been found
    private bool FindShortestPath()
    {
        //Add start node to open list
        openNodeList.Add(startNode);

        //Loop through open node list until empty
        while (openNodeList.Count > 0)
        {
            //Sprt List
            openNodeList.Sort();

            //current node = the node in the open list with the lowest fCost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            //add current node to the closed list
            closedNodeList.Add(currentNode);

            //if the current node = target node
            //     then finish

            if(currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            //evaluate fcost for each neighbour of the current node
            EvaluateCurrentNodeNeighbours(currentNode);
        }

        if (pathFound)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        //Loop through all directions
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);

                if (validNeighbourNode != null)
                {
                    //Calculate new gcost for neighbour
                    int newCostToNeighbour;

                    if (observeMovementPenalties)
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + validNeighbourNode.movementPenalty;
                    }
                    else
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                    }

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
}
