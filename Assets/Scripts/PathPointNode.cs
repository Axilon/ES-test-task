public class PathPointNode
{
    public NoisePointsData PointData;
    public PathPointNode PreviousNode;
    public int PathLengthFromStart;
    public int PathLengthToEnd;
    public int FullPathLength => PathLengthFromStart + PathLengthToEnd;
}