using System;
using System.Collections.Generic;
using System.Drawing;
using SoundSimulation.Generation;

namespace SoundSimulation
{
    class Speaker
    {
        private ISoundGenerator _generator;

        private List<MeshNode> _nodes;
        private List<Vector2> _initialNodePositions;

        private Brush _brush;

        public Speaker(ISoundGenerator generator, SoundMesh mesh, int meshColumn)
        {
            _generator = generator;

            _nodes = mesh.GetColumn(meshColumn);

            _initialNodePositions = new List<Vector2>();

            foreach (MeshNode node in _nodes)
            {
                _initialNodePositions.Add(new Vector2(node.Position.X, node.Position.Y));
            }

            _brush = new SolidBrush(Color.Green);
        }

        public void Update(double elapsedTime)
        {
            double amplitude = _generator.GetAmplitude(elapsedTime);

            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].Position.X = _initialNodePositions[i].X + amplitude * 50;
            }
        }

        public void Draw(Graphics graphics)
        {
            foreach (MeshNode node in _nodes)
            {
                graphics.FillRectangle(_brush, node.GetBounds());
            }
        }
    }
}
