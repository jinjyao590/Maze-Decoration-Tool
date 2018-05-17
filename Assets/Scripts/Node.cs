using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public string Id { get; set; }
    public Transform transform { set; get; }
    public Constant.NodeType Type { get; set; }

    // Neighbor objects from 4 directions
    public NeighborInfo Forward { get; set; }
    public NeighborInfo Backward { get; set; }
    public NeighborInfo Left { get; set; }
    public NeighborInfo Right { get; set; }

}
