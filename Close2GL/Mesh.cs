using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Close2GL
{
    class TriangleFace
    {
        public Vector3[] v;
        public Vector3[] n;
        public Vector3 facenormal;
        public Color faceColor;

        public Vector3[] vColor;

        public TriangleFace(){
            v = new Vector3[3];
            n = new Vector3[3];
            facenormal = new Vector3();
            faceColor = Color.FromArgb(128, 128, 128);
        }
    }

    class Mesh
    {
        private const int MAX_MATERIAL_COUNT = 10;
        private int numtris;
        private TriangleFace[] tris;

        public TriangleFace[] Triangles {
            get { return tris; }
        }

        public Mesh(string file) {
            Vector3[] ambient = new Vector3[MAX_MATERIAL_COUNT],
                    diffuse = new Vector3[MAX_MATERIAL_COUNT],
                    specular = new Vector3[MAX_MATERIAL_COUNT];
            float[] shine = new float[MAX_MATERIAL_COUNT];

            int material_count = 0, i = 0, face = 0;
            int[] color_index = new int[3];

            string[] lines = File.ReadAllLines(file);

            foreach (string line in lines) {
                if (line.StartsWith("v0")){
                    string[] tokens = line.Split(new char[] { ' ' });
                    tris[face] = new TriangleFace();

                    tris[face].v[0].X = float.Parse(tokens[1]); tris[face].v[0].Y = float.Parse(tokens[2]); tris[face].v[0].Z = float.Parse(tokens[3]);
                    tris[face].n[0].X = float.Parse(tokens[4]); tris[face].n[0].Y = float.Parse(tokens[5]); tris[face].n[0].Z = float.Parse(tokens[6]);
                    color_index[0] = int.Parse(tokens[7]);
                }
                else if (line.StartsWith("v1")) {
                    string[] tokens = line.Split(new char[] { ' ' });

                    tris[face].v[1].X = float.Parse(tokens[1]); tris[face].v[1].Y = float.Parse(tokens[2]); tris[face].v[1].Z = float.Parse(tokens[3]);
                    tris[face].n[1].X = float.Parse(tokens[4]); tris[face].n[1].Y = float.Parse(tokens[5]); tris[face].n[1].Z = float.Parse(tokens[6]);
                    color_index[1] = int.Parse(tokens[7]);
                }
                else if (line.StartsWith("v2")) {
                    string[] tokens = line.Split(new char[] { ' ' });

                    tris[face].v[2].X = float.Parse(tokens[1]); tris[face].v[2].Y = float.Parse(tokens[2]); tris[face].v[2].Z = float.Parse(tokens[3]);
                    tris[face].n[2].X = float.Parse(tokens[4]); tris[face].n[2].Y = float.Parse(tokens[5]); tris[face].n[2].Z = float.Parse(tokens[6]);
                    color_index[2] = int.Parse(tokens[7]);
                }
                else if (line.StartsWith("face normal")) {
                    string[] tokens = line.Split(new char[] { ' ' });

                    tris[face].facenormal.X = float.Parse(tokens[2]); tris[face].facenormal.Y = float.Parse(tokens[3]);
                    tris[face].facenormal.Z = float.Parse(tokens[4]);

                    tris[face].faceColor = Color.FromArgb(
                            255 * (int)(diffuse[color_index[0]]).X,
                            255 * (int)(diffuse[color_index[0]]).Y,
                            255 * (int)(diffuse[color_index[0]]).Z);

                    

                    face++;
                }
                else if (line.StartsWith("ambient color")) {
                    string[] tokens = line.Split(new char[] { ' ' });
                    ambient[i].X = float.Parse(tokens[2]); ambient[i].Y = float.Parse(tokens[3]); ambient[i].Z = float.Parse(tokens[4]);
                }
                else if (line.StartsWith("diffuse color")) {
                    string[] tokens = line.Split(new char[] { ' ' });
                    diffuse[i].X = float.Parse(tokens[2]); diffuse[i].Y = float.Parse(tokens[3]); diffuse[i].Z = float.Parse(tokens[4]);
                }
                else if (line.StartsWith("specular color")) {
                    string[] tokens = line.Split(new char[] { ' ' });
                    specular[i].X = float.Parse(tokens[2]); specular[i].Y = float.Parse(tokens[3]); specular[i].Z = float.Parse(tokens[4]);
                }
                else if (line.StartsWith("material shine")) {
                    shine[i] = float.Parse(line.Split(new char[]{ ' ' })[2]);
                    i++;
                }
                else if (line.StartsWith("# triangles")) {
                    numtris = int.Parse(line.Split(new char[] { '=' })[1]);
                    tris = new TriangleFace[numtris];
                }
                else if (line.StartsWith("Material count")) material_count = int.Parse(line.Split(new char[] { '=' })[1]);

            }

            Normalize();
            Recenter();
        }

        private void Normalize() {
            float max = 0;
            float size = 3.0f;

            // Scan for highest values in each axis
            foreach (TriangleFace tri in tris)
                for (int i = 0; i < 3; i++) {
                    if (Math.Abs(tri.v[i].X) > max) max = Math.Abs(tri.v[i].X);
                    if (Math.Abs(tri.v[i].Y) > max) max = Math.Abs(tri.v[i].Y);
                    if (Math.Abs(tri.v[i].Z) > max) max = Math.Abs(tri.v[i].Z);
                }

            if (max == 0) return;

            float scale = max / size;

            foreach (TriangleFace tri in tris) {
                for (int i = 0; i < 3; i++) {
                    tri.v[i].X /= scale; tri.v[i].Y /= scale; tri.v[i].Z /= scale;
                }
            }
        }

        private void Recenter() {
            Vector3 avg = Vector3.Zero;

            foreach (TriangleFace tri in tris)
                for (int i = 0; i < 3; i++)
                    avg += tri.v[i];

            avg /= 3 * numtris;

            foreach (TriangleFace tri in tris)
                for (int i = 0; i < 3; i++)
                    tri.v[i] -= avg;
        }

        public void Render() {
            foreach (TriangleFace tri in tris)
                for (int i = 0; i < 3; i++) {
                    GL.Normal3(tri.n[i]);
                    GL.Vertex3(tri.v[i]);
                }
        }

        public void Render2(Close2GL gl) {
            foreach (TriangleFace tri in tris)
                for (int i = 0; i < 3; i++) {
                    gl.Normal(tri.n[i]);
                    gl.Vertex(tri.v[i]);
                }
        }
    }
}
