public class Constant {

    public enum NodeType
    {
        Default,
        Start,
        End,
        DeadEnd,
        SolutionPath
    };

    public enum NeighborType
    {
        Wall,
        Node,
        Empty
    };

    public enum DistanceType
    {
        DistFromStart,
        DistFromEnd,
        DistFromDeadEnd,
        DistFromSolutionPath
    }

    public enum CurveDepType
    {
        ColorH,
        ColorS,
        ColorV,
        ColorAlpha,
        Size,
        Rotation,
        Amount,
        PosAmount,
        NegAmount
    }

    public enum CurveIndepType
    {
        PN_XY,
        PN_ZY,
        SF_Start,
        SF_End,
        SF_DeadEnd,
        SF_SolutionPath
    }

    public enum CurveOperationType
    {
        Add,
        Sub,
        Mul,
        Div,
        Mod
    }

    public enum DirectionType
    {
        Forward,
        Backward,
        Left,
        Right
    }

    public enum FunctionType
    {
        Curve_Function,
        Multiple_3_Add_1,
        On_Solution_Path
    }
}
