using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
    [SerializeField] private RawImage _renderer;
    [Range(0,1)]
    [SerializeField] private float _whiteColorSafeZone = 0.75f;
    
    [Header("Noise Settings")]
    [SerializeField] private int _width = 200;
    [SerializeField] private int _height = 200;
    [SerializeField] private int _scale = 30;
    [SerializeField] private int _xOffset = 0;
    [SerializeField] private int _yOffset = 0;

    private NoisePointsData[,] _noisePointsDatas;
    
    
    private Texture2D _noiseTexture;

    private float _lastTimeUpdate;

     void Start()
     {
        PreSetScreenValues();
        GenerateNoise();
        GenerateLightning();
    }

    /*private void Update()
    {
        if (!(Time.time - _lastTimeUpdate >.1f)) return;
        _lastTimeUpdate = Time.time;
        GenerateNoise();
        GenerateLightning();
    }*/

    private void PreSetScreenValues()
    {
        if(_renderer != null) return;
            _renderer = GetComponent<RawImage>();
    }

    private void GenerateNoise()
    {
        _noisePointsDatas = new NoisePointsData[_width, _height];
        _noiseTexture = new Texture2D(_width, _height);

        for (int y = _height-1; y >= 0; y--)
        {
            for (int x = _width - 1; x >= 0 ; x--)
            {
                var samp = CalculateNoisePointColor(x, y);
                _noiseTexture.SetPixel(x,y,new Color(samp,samp,samp,samp));
                
                if (samp < _whiteColorSafeZone)
                {
                    _noisePointsDatas[x, y] = new NoisePointsData(){X = x, Y = y, IsActive = true};
                }
            }
        }
        _noiseTexture.Apply();
        _renderer.texture = _noiseTexture;
    }

    private float CalculateNoisePointColor(int x, int y)
    {
        var xCoord = (float) x / _width * _scale + _xOffset;
        var yCoord = (float) y / _height * _scale + _yOffset;
        
        return Mathf.PerlinNoise(xCoord, yCoord);/*
        var pixelColor = new Color(samp,samp,samp,samp);

        if (samp < 0.65f)
        {
            _noisePointsDatas[x, y] = new NoisePointsData(){X = x, Y = y, IsActive = true};
        }
        
        return pixelColor;*/
    }

    private void GenerateDefaultTexture()
    {
        _noiseTexture = new Texture2D(_width, _height);
        
        for (int y = _height-1; y >= 0; y--)
        {
            for (int x = _width - 1; x >= 0 ; x--)
            {
                _noiseTexture.SetPixel(x,y,Color.black);
            }
        }
        
        _noiseTexture.Apply();
        _renderer.texture = _noiseTexture;
    }

    private async void GenerateLightning()
    {
        var points = await GetStartEndPoints();
        if (points.Item1 == null || points.Item2 == null)
        {
            GenerateLightning();
            return;
        }
        
        var path =  await GreedySearchPath(points.Item1, points.Item2);
        if(path == null) return;
        foreach (var pointsData in path)
        {
            _noiseTexture.SetPixel(pointsData.X,pointsData.Y,Color.red);
        }
        _noiseTexture.Apply();
    }

    private async Task<(NoisePointsData,NoisePointsData)> GetStartEndPoints()
    {
        var randomStartWidth = Random.Range(0, _width);
        var randomEndWidth = Random.Range(0, _width);
        
        var result = await Task.Run(() =>
        {
            NoisePointsData startPoint;
            NoisePointsData endPoint;

            while (true)
            {
                if(_noisePointsDatas[_height - 1, randomStartWidth]== null) continue;
                startPoint = _noisePointsDatas[randomStartWidth, _height - 1];
                break;
            }
        
            while (true)
            {
                if(_noisePointsDatas[0, randomEndWidth]== null) continue;
                endPoint = _noisePointsDatas[randomEndWidth, 0];
                break;
            }
            return (startPoint, endPoint);
        });
        
        
        return result;
    }

    private async Task<List<NoisePointsData>> GreedySearchPath(NoisePointsData startPoint, NoisePointsData endPoint)
    {
        var result = await Task.Run(() =>
        {
            var openNodes = new List<PathPointNode>();
            var closedNodes = new List<PathPointNode>();

            var startNode = new PathPointNode
            {
                PointData = startPoint,
                PathLengthFromStart = 0,
                PathLengthToEnd = CalculateDistanceBetweenPoints(startPoint, endPoint),
                PreviousNode = null
            };

            openNodes.Add(startNode);

            while (openNodes.Count > 0)
            {
                var lastNode = openNodes.OrderBy(node => node.FullPathLength).First();

                if (lastNode.PointData == endPoint)
                {
                    return lastNode;
                }

                openNodes.Remove(lastNode);
                closedNodes.Add(lastNode);

                foreach (var node in GetNodeNeighbors(lastNode, endPoint))
                {
                    if(closedNodes.Count(n => n.PointData == node.PointData) > 0) continue;
                    var openNode = openNodes.FirstOrDefault(n =>
                        n.PointData == node.PointData);
                    // Шаг 8.
                    if (openNode == null)
                        openNodes.Add(node);
                    else if (openNode.PathLengthFromStart > node.PathLengthFromStart)
                    {
                        // Шаг 9.
                        openNode.PreviousNode = lastNode;
                        openNode.PathLengthFromStart = node.PathLengthFromStart;
                    }
                }
            }
            return null;
        });

        return GenerateNodePath(result);
    }

    private int CalculateDistanceBetweenPoints(NoisePointsData startPoint, NoisePointsData endPoint)
    {
        return Mathf.Abs(startPoint.X - endPoint.X) + Mathf.Abs(startPoint.Y - endPoint.Y);
    }

    private List<NoisePointsData> GenerateNodePath(PathPointNode node)
    {
        var path = new List<NoisePointsData>();
        var curNode = node;
        while (curNode.PreviousNode != null)
        {
            path.Add(curNode.PointData);
            curNode = curNode.PreviousNode;
        }

        return path;
    }

    private List<PathPointNode> GetNodeNeighbors(PathPointNode node, NoisePointsData target)
    {
        var neighbors = new List<PathPointNode>();
        NoisePointsData[] points = new NoisePointsData[4];
        points[0] = new NoisePointsData {X = node.PointData.X + 1, Y = node.PointData.Y};
        points[1] = new NoisePointsData {X = node.PointData.X - 1, Y = node.PointData.Y};
        points[2] = new NoisePointsData {X = node.PointData.X, Y = node.PointData.Y + 1};
        points[3] = new NoisePointsData {X = node.PointData.X, Y = node.PointData.Y - 1};

        foreach (var point in points)
        {
            if(point.X == node.PointData.X && point.Y == node.PointData.Y) continue;
            if (point.X < 0 || point.X >= _width) continue;
            if (point.Y < 0 || point.Y >= _height) continue;
            if (_noisePointsDatas[point.X, point.Y] == null) continue;
            neighbors.Add(new PathPointNode
            {
                PreviousNode = node,
                PointData = _noisePointsDatas[point.X, point.Y],
                PathLengthFromStart = node.PathLengthFromStart + 1,
                PathLengthToEnd = CalculateDistanceBetweenPoints(point, target)
            });
        }

        return neighbors.OrderBy(n => n.FullPathLength).ToList();
    }
}
