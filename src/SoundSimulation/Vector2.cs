using System;

namespace SoundSimulation
{
    public class Vector2
    {
        public static Vector2 Zero { get { return new Vector2(0.0, 0.0); } }

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2() { }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        public double Cross(Vector2 v)
        {
            return X * v.Y - Y * v.X;
        }

        public double Distance(Vector2 other)
        {
            double diffX = other.X - X;
            double diffY = other.Y - Y;

            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 a, double b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }
}
