using System;

namespace SoundSimulation
{
    class FrequencyGenerator
    {
        private readonly MeshNode _node;
        private readonly int _frequency;

        private readonly Vector2 _nodeStartingPosition;

        public FrequencyGenerator(MeshNode node, int frequency)
        {
            _node = node;
            _frequency = frequency;

            _nodeStartingPosition = new Vector2(node.Position.X, node.Position.Y);
        }

        public void Update(double elapsedTime)
        {
            _node.Position.X = _nodeStartingPosition.X + Math.Cos(2 * Math.PI * _frequency * elapsedTime) * 100;
            _node.Position.Y = _nodeStartingPosition.Y + Math.Sin(2 * Math.PI * _frequency * elapsedTime) * 100;
        }
    }
}
