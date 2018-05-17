using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

    public static GameObject[] InstantiateWalls(SimpleTransform[] simpleTransformsOfWalls, GameObject wallPref, string parent)
    {
        GameObject[] gameObjectsOfWalls = new GameObject[simpleTransformsOfWalls.Length];
        for (int i = 0; i < simpleTransformsOfWalls.Length; ++i)
        {
            if (simpleTransformsOfWalls[i] != null)
            {
                GameObject obj = Instantiate(wallPref, GameObject.Find(parent).transform);
                obj.name = simpleTransformsOfWalls[i].Id;
                obj.transform.localPosition = simpleTransformsOfWalls[i].LocalPosition;
                obj.transform.localRotation = simpleTransformsOfWalls[i].LocalRotation;
                obj.transform.localScale = simpleTransformsOfWalls[i].LocalScale;
                gameObjectsOfWalls[i] = obj;
            }
        }
        return gameObjectsOfWalls;
    }

    public static SimpleTransform[] GetWallsPositions(int size, int HEBP, int VEBP, float offset, string sId, string eId)
    {
        // Contains both horizontal & vertical walls
        SimpleTransform[] transforms = new SimpleTransform[2 * size * (size + 1)];

        // Transfer ebp to array
        char[] bits_hebp = Utils.ToEBPArray(size, size - 1, HEBP);
        char[] bits_vebp = Utils.ToEBPArray(size, size - 1, VEBP);

        // Index for VEBP

        // Horizontal walls (along z-axis)
        ConstructHorizontalWall(size, sId, eId, offset, transforms, bits_vebp);
        ConstructVerticalWall(size, sId, eId, offset, transforms, bits_hebp);

        return transforms;
    }

    public static SimpleTransform[] GetWallsPositions(int size, char[] HEBP, char[] VEBP, float offset, string sId, string eId)
    {
        // Contains both horizontal & vertical walls
        SimpleTransform[] transforms = new SimpleTransform[2 * size * (size + 1)];

        // Transfer ebp to array
        //char[] bits_hebp = Utils.ToEBPArray(size, size - 1, HEBP);
        //char[] bits_vebp = Utils.ToEBPArray(size, size - 1, VEBP);

        // Index for VEBP

        // Horizontal walls (along z-axis)
        ConstructHorizontalWall(size, sId, eId, offset, transforms, VEBP);
        ConstructVerticalWall(size, sId, eId, offset, transforms, HEBP);

        return transforms;
    }

    static void ConstructHorizontalWall(int size, string sId, string eId, float offset, SimpleTransform[] transforms, char[] bits_vebp)
    {
        int vIndex = 0;
        for (int i = 0; i < size + 1; ++i)
        {
            // Handling every row
            for (int j = 0; j < size; ++j)
            {
                // For the first row and last row
                if (i == 0 || i == size)
                {
                    // If not start or end, put the wall there
                    if (!sId.Equals("H_Wall_" + (i * size + j)) && !eId.Equals("H_Wall_" + (i * size + j)))
                    {
                        SimpleTransform transform = new SimpleTransform();
                        transform.Id = "H_Wall_" + (i * size + j);
                        transform.LocalPosition = new Vector3(i * offset + 0.5f, 0, j * offset);
                        transform.LocalRotation = Quaternion.identity;
                        transform.LocalScale = new Vector3(1, 1, 1);
                        transforms[i * size + j] = transform;
                    }
                }
                else
                {
                    // Rows between depends on HEBP
                    if (bits_vebp[vIndex] == '0')
                    {
                        SimpleTransform transform = new SimpleTransform();
                        transform.Id = "H_Wall_" + (i * size + j);
                        transform.LocalPosition = new Vector3(i * offset + 0.5f, 0, j * offset);
                        transform.LocalRotation = Quaternion.identity;
                        transform.LocalScale = new Vector3(1, 1, 1);
                        transforms[i * size + j] = transform;
                    }
                    vIndex++;
                }
            }
        }
    }

    static void ConstructVerticalWall(int size, string sId, string eId, float offset, SimpleTransform[] transforms, char[] bits_hebp)
    {
        int vsIdx = size + 1;
        // Index for HEBP
        int hIndex = 0;
        // Vertical walls (along x-axis)
        for (int i = 0; i < size + 1; ++i)
        {
            // Handling every column
            for (int j = 0; j < size; ++j)
            {
                // For the first column and last column
                if (i == 0 || i == size)
                {
                    // If not start or end, put the wall there
                    if (!sId.Equals("V_Wall_" + (i * size + j)) && !eId.Equals("V_Wall_" + (i * size + j)))
                    {
                        SimpleTransform transform = new SimpleTransform();
                        transform.Id = "V_Wall_" + (i * size + j);
                        transform.LocalPosition = new Vector3(j * offset, 0, i * offset + 0.5f);
                        transform.LocalRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                        transform.LocalScale = new Vector3(1, 1, 1);
                        transforms[vsIdx * size + j] = transform;
                    }
                }
                else
                {
                    // Columns between depends on VEBP
                    if (bits_hebp[hIndex] == '0')
                    {
                        SimpleTransform transform = new SimpleTransform();
                        transform.Id = "V_Wall_" + (i * size + j);
                        transform.LocalPosition = new Vector3(j * offset, 0, i * offset + 0.5f);
                        transform.LocalRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                        transform.LocalScale = new Vector3(1, 1, 1);
                        transforms[vsIdx * size + j] = transform;
                    }
                    hIndex++;
                }
            }
            vsIdx++;
        }
    }

    public static SimpleTransform[] GetNodesPositions(int size, float offset)
    {
        SimpleTransform[] transforms = new SimpleTransform[size * size];

        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                SimpleTransform transform = new SimpleTransform();
                transform.Id = "Node_" + (i * size + j);
                transform.LocalPosition = new Vector3((i + 0.5f) * offset, 0, (j + 0.5f) * offset);
                transform.LocalRotation = Quaternion.identity;
                transform.LocalScale = new Vector3(1, 1, 1);
                transforms[i * size + j] = transform;
            }
        }

        return transforms;
    }

    public static GameObject[] InstantiateNodes(SimpleTransform[] simpleTransformsOfNodes, ref GraphNode[] graphNodes, GameObject nodePref, string parent)
    {
        GameObject[] GameObjectsOfNodes = new GameObject[simpleTransformsOfNodes.Length];
        for (int i = 0; i < simpleTransformsOfNodes.Length; ++i)
        {
            GameObject obj = Instantiate(nodePref, GameObject.Find(parent).transform);
            obj.name = simpleTransformsOfNodes[i].Id;
            obj.transform.localPosition = simpleTransformsOfNodes[i].LocalPosition;
            obj.transform.localRotation = simpleTransformsOfNodes[i].LocalRotation;
            obj.transform.localScale = simpleTransformsOfNodes[i].LocalScale;
            GameObjectsOfNodes[i] = obj;

            graphNodes[i] = new GraphNode(simpleTransformsOfNodes[i].Id);
        }
        return GameObjectsOfNodes;
    }
}
