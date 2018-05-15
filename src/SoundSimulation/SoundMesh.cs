using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace SoundSimulation
{
    class MeshNode
    {
        public bool Fixed;

        public Vector2 Position;

        public Vector2 Velocity;

        public Vector2 Force;

        public MeshNode[] Neighbors;

        public double[] InitialNeighborDistances;

        public RectangleF GetBounds()
        {
            return new RectangleF((float)Position.X, (float)Position.Y, 5, 5);
        }
    }

    class SoundMesh
    {
        public double ElapsedSimulationTime { get; private set; }

        private int _rows;
        private int _cols;
        private float _spacing;

        private MeshNode[,] _nodes;

        private bool _isActive;
        private Thread _updateThread;

        private double _simulationDt;

        private List<Speaker> _speakers;
        private List<Microphone> _microphones;

        private Brush _brush;

        public SoundMesh(int rows, int cols, float spacing)
        {
            _rows = rows;
            _cols = cols;
            _spacing = spacing;

            _speakers = new List<Speaker>();
            _microphones = new List<Microphone>();

            _nodes = new MeshNode[_rows, _cols];

            _brush = new SolidBrush(Color.White);

            if (_rows == 1)
            {
                Initialize1DMesh();
            }
            else
            {
                Initialize2DMesh();
            }

            ConnectMesh();
        }

        private void Initialize1DMesh()
        {
            // Initialize all nodes and fix any nodes on the edges
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    _nodes[y, x] = new MeshNode
                    {
                        Fixed = x == 0 || x == _cols - 1,
                        Position = new Vector2(x * _spacing, y * _spacing + 60),
                        Velocity = Vector2.Zero,
                        Force = Vector2.Zero
                    };
                }
            }
        }

        private void Initialize2DMesh()
        {
            // Initialize all nodes and fix any nodes on the edges
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    _nodes[y, x] = new MeshNode
                    {
                        Fixed = y == 0 || x == 0 || y == _rows - 1 || x == _cols - 1,
                        Position = new Vector2(x * _spacing, y * _spacing + 60),
                        Velocity = Vector2.Zero,
                        Force = Vector2.Zero
                    };
                }
            }
        }

        private void ConnectMesh()
        {
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    var currentNode = _nodes[y, x];

                    var neighbors = new List<MeshNode>();
                    var initialDistances = new List<double>();

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            // Skip the current node
                            if (dy == 0 && dx == 0) continue;

                            int targetY = y + dy;
                            int targetX = x + dx;

                            // Find all in range neighbors
                            if (y + dy >= 0 && y + dy < _rows &&
                                x + dx >= 0 && x + dx < _cols)
                            {
                                var targetNode = _nodes[targetY, targetX];

                                Vector2 difference = currentNode.Position - targetNode.Position;

                                neighbors.Add(targetNode);
                                initialDistances.Add(difference.Length());
                            }
                        }
                    }

                    currentNode.Neighbors = neighbors.ToArray();
                    currentNode.InitialNeighborDistances = initialDistances.ToArray();
                }
            }

        }

        public void AddSpeaker(Speaker speaker)
        {
            _speakers.Add(speaker);
        }

        public void AddMicrophone(Microphone microphone)
        {
            _microphones.Add(microphone);
        }

        public MeshNode GetNode(int row, int col)
        {
            return _nodes[row, col];
        }

        public List<MeshNode> GetColumn(int col)
        {
            var nodes = new List<MeshNode>();

            for (int x = 0; x < _rows; x++)
            {
                nodes.Add(_nodes[x, col]);
            }

            return nodes;
        }

        public void Simulate(uint sampleRate)
        {
            ElapsedSimulationTime = 0;
            _simulationDt = 1.0 / sampleRate;

            _isActive = true;
            _updateThread = new Thread(Update);
            _updateThread.Start();
        }

        public void Stop()
        {
            _isActive = false;
            _updateThread.Join(1000);
        }

        private void Update()
        {
            while (_isActive)
            {
                // Sum all the forces on each node
                Parallel.For(0, _rows, y =>
                {
                    for (int x = 0; x < _cols; x++)
                    {
                        MeshNode node = _nodes[y, x];

                        if (node.Fixed) continue;

                        node.Force.X = 0;
                        node.Force.Y = 0;

                        for (var i = 0; i < node.Neighbors.Length; i++)
                        {
                            Vector2 difference = node.Neighbors[i].Position - node.Position;

                            double length = difference.Length();

                            double displacement = length - node.InitialNeighborDistances[i];

                            if (displacement > 0.001)
                            {
                                Vector2 normal = difference / length;

                                Vector2 force = normal * displacement * 100000000.0f;

                                node.Force += force;
                            }

                            node.Force += node.Velocity * -50.0f;
                        }
                    }
                });

                // Integrate the force into accelerate, velocity, and position
                Parallel.For(0, _rows, y =>
                {
                    for (int x = 0; x < _cols; x++)
                    {
                        MeshNode node = _nodes[y, x];

                        if (node.Fixed) continue;

                        node.Velocity += node.Force * _simulationDt;
                        node.Position += node.Velocity * _simulationDt;
                    }
                });

                foreach (Speaker speaker in _speakers)
                {
                    speaker.Update(ElapsedSimulationTime);
                }

                foreach (Microphone microphone in _microphones)
                {
                    microphone.Update(ElapsedSimulationTime);
                }

                ElapsedSimulationTime += _simulationDt;
            }
        }

        public void Draw(Graphics graphics)
        {
            var rectangles = new List<RectangleF>();

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    MeshNode node = _nodes[y, x];

                    rectangles.Add(node.GetBounds());
                }
            }

            graphics.FillRectangles(_brush, rectangles.ToArray());
        }
    }
}
