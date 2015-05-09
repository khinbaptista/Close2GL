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

        public bool Finished { get { return (current - start).Length >= (end - start).Length; } }

        public Edge(Vector4 start, Vector4 end) {
            this.start = start; this.end = end;

            CalculateIncrements();
            Start();
        }

        public void Start() {
            current = start;
        }

        public void Next() {
            current += direction;
        }

        private void CalculateIncrements() {
            direction = (end - start).Normalized();
        }
    }
}
