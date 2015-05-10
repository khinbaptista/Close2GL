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

        public bool Finished { get { return (current - start).LengthFast >= (end - start).LengthFast; } }

        public Edge(Vector4 start, Vector4 end, bool order = true) {
            this.start = start; this.end = end;

            if (order) Order();
            CalculateIncrements();
            Start();
        }

        public void Order() {
            if (start.Y > end.Y) {
                Vector4 swap = end;
                end = start;
                start = swap;
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
            float d = (end.Y - start.Y) / (end.X - start.X);
            float x = (y - start.Y + d * start.X) / d;

            return (int)Math.Round(x);
        }
    }
}
