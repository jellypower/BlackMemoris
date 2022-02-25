using UnityEngine;
using System;
using System.Collections.Generic;
using Priority_Queue;
public class PathFinder : MonoBehaviour
{

    public NodeGridGenerator nodeGridInfo;
    [SerializeField] int priorityQueueMaxSize = 200;
    [SerializeField] bool optimizingPath = true;


    Vector2 gridStartPoint;
    Vector2 gridEndPoint;
    float cellSize;
    Vector2 collisionCheckSensorSize = new Vector2(1, 1);
    LayerMask layerTocheckCollide;


    new BoxCollider2D collider;

    int numCols;
    int numRows;


    Node[,] nodeGrid;

    Node goalNode;
    Node startNode;


    FastPriorityQueue<Node> openList;
    LinkedList<Node> closeList;

    LinkedList<Node> nodeOnPathList;

    LinkedList<Vector2> finalPath;

    bool isPathFound;


    void Awake()
    {



        gridStartPoint = nodeGridInfo.GridStartPoint;
        gridEndPoint = nodeGridInfo.GridEndPoint;
        cellSize = nodeGridInfo.CellSize;
        collisionCheckSensorSize = nodeGridInfo.CollisionCheckSensorSize;
        layerTocheckCollide = nodeGridInfo.LayerToCheckCollide;


        numCols = (int)((gridEndPoint.x - gridStartPoint.x) / cellSize + 0.5);
        numRows = (int)((gridEndPoint.y - gridStartPoint.y) / cellSize + 0.5);

       
        nodeGrid = nodeGridInfo.NodeGrid;

        
        collider = GetComponent<BoxCollider2D>();
        

        openList = new FastPriorityQueue<Node>(priorityQueueMaxSize);
        closeList = new LinkedList<Node>();


        nodeOnPathList = new LinkedList<Node>();
        finalPath = new LinkedList<Vector2>();




        goalNode = findNodeOnPosition(transform.position);

        

    }

    public Node[,] getNodeGrid()
    {
        return nodeGrid;
    }

 
    public LinkedList<Vector2> getShortestPath(Vector2 start, Vector2 goal, out long elapsedMiliSeconds)
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();



        getShortestPath(start, goal);



        watch.Stop();
        elapsedMiliSeconds = watch.ElapsedMilliseconds;


        return finalPath;


    }

    public LinkedList<Vector2> getShortestPath(Vector2 start, Vector2 goal)
    {
        isPathFound = false;
        finalPath.Clear();

        if (start.x <= gridStartPoint.x || start.x >= gridEndPoint.x ||
            start.y <= gridStartPoint.y || start.y >= gridEndPoint.y
            ) return finalPath;

        if (goal.x <= gridStartPoint.x || goal.x >= gridEndPoint.x ||
            goal.y <= gridStartPoint.y || goal.y >= gridEndPoint.y
            ) return finalPath;


        startNode = findNodeOnPosition(start);
        goalNode = findNodeOnPosition(goal);



        if (goalNode.isWall)
        {
            goalNode = findClosestWalkableNode(goal);
            
            Vector2 bias = (goal - goalNode.nodeCenter).normalized * nodeGridInfo.CellSize /2;
            
            goal = goalNode.nodeCenter;
            goal = goal + bias;
        }


        if (!Physics2D.BoxCast(
                gameObject.transform.position, collider.size, 0,
                goalNode.nodeCenter - start, Vector2.Distance(start, goal), layerTocheckCollide))
        {
            nodeOnPathList.AddFirst(startNode);
            nodeOnPathList.AddFirst(goalNode);
            isPathFound = true;
        }
        else
        {
            findPath(startNode, goalNode); // start A* + JPS algorithm

            excludeUselessWaypoints();
            if (optimizingPath) optimizeWaypoints();
        }

        if (isPathFound)
        {
            nodeOnPathList.RemoveFirst();

            while (nodeOnPathList.Count > 1)
            {
                finalPath.AddLast(nodeOnPathList.First.Value.nodeCenter);
                nodeOnPathList.RemoveFirst();
            }
            finalPath.AddLast(goal);
            nodeOnPathList.RemoveFirst();
        }

        return finalPath;




    }

    public LinkedList<Vector2> getPath()
    {

        return finalPath;
    }

    void initOpenList()
    {
        while(openList.Count > 0)
        {
            Node n= openList.Dequeue();
            n.parent = null;
            n.onOpenList = false;
        }

    }

    void initCloseList()
    {
        while(closeList.Count > 0)
        {
            LinkedListNode<Node> n = closeList.First;
            n.Value.parent = null;
            n.Value.onCloseList = false;

            closeList.RemoveFirst();
        }
        
    }


    
    void findPath(Node start, Node goal) // find path with a* algorithm
    {


        initCloseList();
        initOpenList();
        nodeOnPathList.Clear();

        if (start == null || goal == null || start == goal) return;

        openList.Enqueue(start, start.fCost);
        start.onOpenList = true;

        while (openList.Count > 0)
        {

            Node nodeToFind = openList.Dequeue();
            closeList.AddFirst(nodeToFind);
            nodeToFind.onCloseList = true;
            nodeToFind.onOpenList = false;// move node from open list to close list


            if (nodeToFind == goal)
            {
                isPathFound = true;
                break;
            }

            jump(nodeToFind);


        }


        makeWaypoints();
    }

    void makeWaypoints()
    {
        if (!isPathFound) return;

        Node iterNode = goalNode;

        do
        {
            nodeOnPathList.AddFirst(iterNode);
            iterNode = iterNode.parent;
        } while (iterNode != null);

    }

    void excludeUselessWaypoints()
    {
        if (nodeOnPathList.Count <= 2) return;
        LinkedListNode<Node> iter = nodeOnPathList.First;

        while(iter.Next.Next != null)
        {
            Node current = iter.Value;
            Node next = iter.Next.Value;
            Node nextnext= iter.Next.Next.Value;
            if ((current.XIndex < next.XIndex && next.XIndex < nextnext.XIndex
                || current.XIndex == next.XIndex && next.XIndex == nextnext.XIndex
                || current.XIndex > next.XIndex && next.XIndex > nextnext.XIndex) &&
                (current.YIndex < next.YIndex && next.YIndex < nextnext.YIndex
                || current.YIndex == next.YIndex && next.YIndex == nextnext.YIndex
                || current.YIndex > next.YIndex && next.YIndex > nextnext.YIndex))
            {
                nodeOnPathList.Remove(next);
                
            }
            else iter = iter.Next;
        }
    }


    Node current;
    Node next;
    Node nextnext;
    void optimizeWaypoints()
    {
        
        if (nodeOnPathList.Count <=2) return;

        LinkedListNode<Node> iter = nodeOnPathList.First;
        while (iter.Next.Next != null)
        {
            current = iter.Value;
            next = iter.Next.Value;
            nextnext = iter.Next.Next.Value;
            if (
                !Physics2D.BoxCast( current.nodeCenter, collider.size, 0,
                nextnext.nodeCenter - current.nodeCenter,
                Vector2.Distance(current.nodeCenter, nextnext.nodeCenter),
                layerTocheckCollide))
            {
                nodeOnPathList.Remove(next);
            }
            else iter = iter.Next;
        }

    }


        


    void jump(Node node) // find jump point from node variable and add jump point to open list.
    {
        Node parent;

        if (node.parent == null) parent = node;
        else parent = node.parent;

        if (parent.XIndex <= node.XIndex || parent.YIndex != node.YIndex)
            updateJumpPoints(jumpHorizontal(node, 1), node);

        if (parent.XIndex >= node.XIndex || parent.YIndex != node.YIndex)
            updateJumpPoints(jumpHorizontal(node, -1), node);

        if (parent.XIndex != node.XIndex || parent.YIndex <= node.YIndex)
            updateJumpPoints(jumpVertical(node, 1), node);

        if (parent.XIndex != node.XIndex || parent.YIndex >= node.YIndex)
            updateJumpPoints(jumpVertical(node, -1), node);


        if (parent.XIndex <= node.XIndex || parent.YIndex <= node.YIndex)
            updateJumpPoints(jumpDiagonal(node, 1, 1), node);

        if (parent.XIndex >= node.XIndex || parent.YIndex <= node.YIndex)
            updateJumpPoints(jumpDiagonal(node, -1, 1), node);

        if (parent.XIndex >= node.XIndex || parent.YIndex >= node.YIndex)
            updateJumpPoints(jumpDiagonal(node, -1, -1), node);

        if (parent.XIndex <= node.XIndex || parent.YIndex >= node.YIndex)
            updateJumpPoints(jumpDiagonal(node, 1, -1), node);


    }

    void updateJumpPoints(Node jumpEnd, Node jumpStart)
    {

        if (jumpEnd == null) return;

        if (jumpEnd.onOpenList || jumpEnd.onCloseList)
        {
            if (jumpEnd.gCost > jumpStart.gCost + Vector2.Distance(jumpStart.nodeCenter, jumpEnd.nodeCenter))
            {
                jumpEnd.parent = jumpStart;
                jumpEnd.gCost = jumpStart.gCost + Vector2.Distance(jumpEnd.nodeCenter, jumpStart.nodeCenter);

            }
            return;

        }
        else
        {
            jumpEnd.parent = jumpStart;
            jumpEnd.gCost = jumpStart.gCost + Vector2.Distance(jumpEnd.nodeCenter, jumpStart.nodeCenter);
            jumpEnd.hCost = Vector2.Distance(goalNode.nodeCenter, jumpEnd.nodeCenter); // update distance
            jumpEnd.onOpenList = true;
            openList.Enqueue(jumpEnd, jumpEnd.fCost);
        }

    }

    Node jumpHorizontal(Node start, int xDir)
    {
        int currentXDir = start.XIndex;
        int currentYDir = start.YIndex;
        Node currentNode;

        while (true)
        {
            currentXDir += xDir;
            
            if (!isWalkalbeAt(currentXDir, currentYDir)) return null;
            currentNode = nodeGrid[currentXDir, currentYDir];


            if (currentNode == goalNode)
            {

                return goalNode;
            }

            if ( isWalkalbeAt(currentXDir+xDir, currentYDir+1) && !isWalkalbeAt(currentXDir, currentYDir+1)
                || isWalkalbeAt(currentXDir+xDir, currentYDir-1) && !isWalkalbeAt(currentXDir, currentYDir-1))
            {

                return currentNode;
                
            }

        }
    }

    Node jumpVertical(Node start, int yDir)
    {

        int currentXDir = start.XIndex;
        int currentYDir = start.YIndex;
        Node currentNode;

        while (true)
        {

            currentYDir += yDir;
            
            if (!isWalkalbeAt(currentXDir, currentYDir)) return null;
            
            currentNode = nodeGrid[currentXDir, currentYDir];

            if (currentNode == goalNode)
            {

                return goalNode;
            }

            if (isWalkalbeAt(currentXDir + 1, currentYDir + yDir) && !isWalkalbeAt(currentXDir + 1, currentYDir)
                || isWalkalbeAt(currentXDir - 1, currentYDir + yDir) && !isWalkalbeAt(currentXDir - 1, currentYDir))
            {

                return currentNode;
            }
        }

    }

    Node jumpDiagonal(Node start, int xDir, int yDir) // if parent node and 
    {
        int currentXDir = start.XIndex;
        int currentYDir = start.YIndex;
        Node currentNode;

        while (true)
        {

            currentXDir += xDir;
            currentYDir += yDir;
            

            if (!isWalkalbeAt(currentXDir, currentYDir)) return null;

            currentNode = nodeGrid[currentXDir, currentYDir];

            if (currentNode == goalNode)
            {

                return goalNode;
            }

            if (isWalkalbeAt(currentXDir - xDir, currentYDir + yDir) && !isWalkalbeAt(currentXDir - xDir, currentYDir)
            || isWalkalbeAt(currentXDir + xDir, currentYDir - yDir) && !isWalkalbeAt(currentXDir, currentYDir - yDir)
                )
            {

                return currentNode;
            }

            Node temp;
            temp = jumpVertical(currentNode, yDir);
            if ( temp != null && !temp.onCloseList && !temp.onOpenList)
            {
                return currentNode;
            }
            temp = jumpHorizontal(currentNode, xDir);
            if (temp != null && !temp.onCloseList && !temp.onOpenList)
            {

                return currentNode;
            }
        }

    }

    bool isWalkalbeAt(int x, int y)
    {
        return 0 <= x && x < numCols && 0 <= y && y < numRows && !nodeGrid[x, y].isWall;
    }




    bool areTwoNodesStraight(Node node1, Node node2)
    {
        return node1.XIndex == node2.XIndex ||
            node1.YIndex == node2.YIndex;
    }

    Node findNodeOnPosition(Vector2 position)
    {
        if (nodeGrid == null) return null;
        
        if (position.x < gridStartPoint.x || position.y < gridStartPoint.y
            || position.x > gridEndPoint.x || position.y > gridEndPoint.y) return null;


        Vector2 relativePosition = position - gridStartPoint;

        int x = (int)(relativePosition.x / cellSize);
        int y = (int)(relativePosition.y / cellSize);

        return nodeGrid[x, y];
    }

    Node findClosestWalkableNode(Vector2 position)
    {


        Node origin = nodeGridInfo.findNodeOnPosition(position);
        Node[,] nodeGrid = nodeGridInfo.NodeGrid;

        int originX = origin.XIndex;
        int originY = origin.YIndex;

        int distance = 0;


        while (distance < 100) // 맨해튼디스턴스로 제일 가까운 walkable 위치 구하기
        {
            for (int i = 0; i <= distance; i++)
            {
                if (originY + distance < nodeGridInfo.NumRows)
                {
                    if (originX + i < nodeGridInfo.NumCols && !nodeGrid[originX + i, originY + distance].isWall)
                        return nodeGrid[originX + i, originY + distance];

                    if (originX - i >= 0 && !nodeGrid[originX - i, originY + distance].isWall)
                        return nodeGrid[originX - i, originY + distance];

                }

                if (originY - distance >= 0)
                {
                    if (originX + i < nodeGridInfo.NumCols && !nodeGrid[originX + i, originY - distance].isWall)
                        return nodeGrid[originX + i, originY - distance];

                    if (originX - i >= 0 && !nodeGrid[originX - i, originY - distance].isWall)
                        return nodeGrid[originX - i, originY - distance];

                }

                if (originX + distance < nodeGridInfo.NumCols)
                {
                    if (originY + i < nodeGridInfo.NumRows && !nodeGrid[originX + distance, originY + i].isWall)
                        return nodeGrid[originX + distance, originY + i];

                    if (originY - i < nodeGridInfo.NumRows && !nodeGrid[originX + distance, originY - i].isWall)
                        return nodeGrid[originX + distance, originY - i];
                }

                if (originX - distance >= 0)
                {
                    if (originY + i < nodeGridInfo.NumRows && !nodeGrid[originX - distance, originY + i].isWall)
                        return nodeGrid[originX - distance, originY + i];

                    if (originY - i < nodeGridInfo.NumRows && !nodeGrid[originX - distance, originY - i].isWall)
                        return nodeGrid[originX - distance, originY - i];
                }


            }

            distance++;
            
        }
        Debug.Log("PathFinder>findClosestWalkableNode: too far to find closest node");
        return null;

    }


    #region Draw On Gizmo Functions for DEBUG
#if DEBUG
    bool drawOnGizmo;

    Color goalGizmoColor = new Color(1, 1, 0, 0.5f);
    void OnDrawGizmos()
    {
        if (drawOnGizmo)
        {
            drawObjectPositionNode();
            drawNodeOnGizmo(goalNode, goalGizmoColor);
            //drawGridLine();
            //drawObstacles();
            drawShortestPath();
            //drawFinalWaypoints();
            //drawFinalNodes();
            //drawClosedNodes();

        }


    }

    void drawShortestPath()
    {
        if (finalPath != null && finalPath.Count > 0)
        {
            drawLineBetweenNodes(transform.position, finalPath.First.Value);


            for (LinkedListNode<Vector2> iter = finalPath.First; iter.Next != null; iter = iter.Next)
            {
                drawLineBetweenNodes(iter.Value, iter.Next.Value);
            }
        }
    }

    void drawFinalNodes()
    {
        if (openList != null)
            foreach (Node n in nodeOnPathList)
            {
                drawNodeOnGizmo(n, Color.red);
            }
    }

    void drawFinalWaypoints()
    {
        if (nodeOnPathList != null && nodeOnPathList.Count > 1)
        {
            
            for (LinkedListNode<Node> iter = nodeOnPathList.First; iter.Next != null; iter = iter.Next)
            {
                drawLineBetweenNodes(iter.Value.nodeCenter, iter.Next.Value.nodeCenter);
            }
        }

    }

    void drawLineBetweenNodes(Vector2 start, Vector2 end)
    {
        Gizmos.color = Color.white;

        Gizmos.DrawLine(start, end);
    }

    Color playerGizmoColor = new Color(0, 1, 1, 0.5f);
    void drawObjectPositionNode()
    {
        if (transform != null)
            drawNodeOnGizmo(findNodeOnPosition(transform.position), playerGizmoColor);
    }

    

    void drawClosedNodes()
    {
        if (closeList != null)
            foreach (Node n in closeList)
            {
                if (n.parent != null)
                {
                    drawNodeOnGizmo(n, new Color(0, 0, 0, 0.5f));
                    drawLineBetweenNodes(n.nodeCenter, n.parent.nodeCenter);
                }
            }

    }



    void drawNodeOnGizmo(Node node, Color gizmoColor)
    {
        if (node == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(node.nodeCenter, new Vector2(cellSize, cellSize));
    }

#endif
    #endregion

}
