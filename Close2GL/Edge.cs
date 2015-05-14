using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace Close2GL
{
    class Edge
    {
        public Vector4 start;
        public Vector4 current;
        public Vector4 end;
        public Vector4 direction;

        public Vector3 startColor;
        public Vector3 startNormal;
        public Vector2 startTexCoord;
        public Vector3 endColor;
        public Vector3 endNormal;
        public Vector2 endTexCoord;

        public bool Finished { get { return (current - start).LengthFast >= (end - start).LengthFast; } }

        public Edge(Vector4 start, Vector4 end, bool order = true) {
            this.start = start; this.end = end;

            if (order) Order();
            CalculateIncrements();
            Start();
        }

        private void Swap<T>(T a, T b) {
            T swap = a;
            a = b;
            b = swap;
        }

        public void Order() {
            if (start.Y > end.Y) {
                Swap<Vector4>(start, end);
                if (startColor != null) Swap<Vector3>(startColor, endColor);
                if (startNormal != null) Swap<Vector3>(startNormal, endNormal);
                if (startTexCoord != null) Swap<Vector2>(startTexCoord, endTexCoord);
            }
        }

        public void Start() {
            current = start + direction;
        }

        public void Next() {
            current += direction;
        }

        private void CalculateIncrements() {
            direction = (end - start).Normalized();
        }

        public int GetX(float y) {
            float dy = (end.Y - start.Y);
            if (dy == 0) return -1;

            float d = (end.X - start.X) / dy;
            float x = start.X + d * (y - start.Y);

            Vector2 result = new Vector2(x, y) - start.Xy;
            Vector2 original = end.Xy - start.Xy;

            if (result.LengthFast > original.LengthFast) return -1;// (int)Math.Round(end.X);

            return (int)Math.Round(x);
        }
    }
}
