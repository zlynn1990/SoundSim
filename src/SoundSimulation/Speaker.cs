using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundSimulation.Generation;

namespace SoundSimulation
{
    class Speaker
    {
        private ISoundGenerator _generator;

        private List<MeshNode> _nodes;
        private List<Vector2> _initialNodePositions;

        public Speaker(ISoundGenerator generator, SoundMesh mesh, int meshColumn)
        {
            _generator = generator;

            _nodes = mesh.GetColumn(meshColumn);

            _initialNodePositions = new List<Vector2>();

            foreach (MeshNode node in _nodes)
            {
                _initialNodePositions.Add(new Vector2(node.Position.X, node.Position.Y));
            }
        }

        public void Update(double elapsedTime)
        {
            double amplitude = _generator.GetAmplitude(elapsedTime);

            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].Position.X = _initialNodePositions[i].X + amplitude * 50;
            }
        }
    }
}
