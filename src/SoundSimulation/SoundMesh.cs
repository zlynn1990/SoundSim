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

        private List<FrequencyGenerator> _frequencyGenerators;

        public SoundMesh(int rows, int cols, float spacing)
        {
            _rows = rows;
            _cols = cols;
            _spacing = spacing;

            InitializeMesh();

            _frequencyGenerators = new List<FrequencyGenerator>
            {
                new FrequencyGenerator(_nodes[10, 10], 3000),
                new FrequencyGenerator(_nodes[50, 90], 4000)
            };
        }

        private void InitializeMesh()
        {
            _nodes = new MeshNode[_rows, _cols];

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

            // Initialize node neighbors (must be done after nodes are setup)
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    var neighbors = new List<MeshNode>();

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
                                neighbors.Add(_nodes[targetY, targetX]);
                            }
                        }
                    }

                    _nodes[y, x].Neighbors = neighbors.ToArray();
                }
            }
        }

        public void Simulate(int sampleRate)
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
                foreach (FrequencyGenerator generator in _frequencyGenerators)
                {
                    generator.Update(ElapsedSimulationTime);
                }

                // Sum all the forces on each node
                Parallel.For(0, _rows, y =>
                {
                    for (int x = 0; x < _cols; x++)
                    {
                        MeshNode node = _nodes[y, x];

                        if (node.Fixed) continue;

                        node.Force = Vector2.Zero;

                        foreach (MeshNode neighbor in node.Neighbors)
                        {
                            Vector2 difference = neighbor.Position - node.Position;

                            double length = difference.Length();

                            if (length > 0.1)
                            {
                                node.Force += difference * 1000000000;
                            }

                            node.Force += node.Velocity * -200f;
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

                ElapsedSimulationTime += _simulationDt;
            }
        }

        public RectangleF[] GetSnapshot()
        {
            var rectangles = new List<RectangleF>();

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    MeshNode node = _nodes[y, x];

                    rectangles.Add(new RectangleF((float)node.Position.X, (float)node.Position.Y, 7, 7));
                }
            }

            return rectangles.ToArray();
        }
    }
}
