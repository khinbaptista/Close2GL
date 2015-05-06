using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Close2GL
{
    class Close2GL
    {
        private Matrix4 modelview;
        private Matrix4 projection;
        private Matrix4 mvp;
        private Matrix4 viewport;

        float left, right, top, bottom, near, far;

        private Vector3 cameraDirection;
        private bool culling;
        private FrontFaceDirection face;

        private bool begun;
        private List<Vector4> primitive;

        public Matrix4 Modelview {
            get { return modelview; }
            set { modelview = value; }
        }

        public Matrix4 Projection {
            get { return projection; }
            set { projection = value; }
        }

        public Matrix4 ModelviewProjection {
            get { return modelview * projection; }
        }

        public Close2GL() {
            modelview = Matrix4.Identity;
            projection = Matrix4.Identity;
            viewport = Matrix4.Identity;
            mvp = Matrix4.Identity;
            begun = false;
        }

        public void ResetModelview() { modelview = Matrix4.Identity; }

        public void LookAt(Vector3 eye, Vector3 target, Vector3 up) {
            Vector3 direction = (eye - target).Normalized(); // right-hand coordinate system: invert the direction // n
            Vector3 right = Vector3.Cross(up, direction).Normalized();  // u
            Vector3 realUp = Vector3.Cross(direction, right).Normalized();  // v

            // implement the Matrix4.LookAt equivalent using eye, target and realUp
            Matrix4 lookat = Matrix4.Identity;
            lookat.Row0 = new Vector4(right, -Vector3.Dot(eye, right));
            lookat.Row1 = new Vector4(realUp, -Vector3.Dot(eye, realUp));
            lookat.Row2 = new Vector4(direction, -Vector3.Dot(eye, direction));
            lookat.Row3 = new Vector4(0, 0, 0, 1);

            lookat.Transpose();
            modelview *= lookat;
            
            UpdateMVP();

            cameraDirection = (target - eye).Normalized();
        }

        public void Translate(Vector3 translation) {
            Matrix4 T = Matrix4.Identity;

            T.M14 = translation.X;
            T.M24 = translation.Y;
            T.M34 = translation.Z;

            modelview *= T;
        }

        public void RotateX(float angle) {

        }

        public void RotateY(float angle) {

        }

        public void RotateZ(float angle) {

        }

        public void Rotate(Vector3 axis, float angle) {

        }

        public void Scale(float scale) {
            Matrix4 S = Matrix4.Identity;

            S.M11 = scale;
            S.M22 = scale;
            S.M33 = scale;

            modelview *= S;
        }

        public void Scale(float Sx, float Sy, float Sz) {
            Matrix4 S = Matrix4.Identity;

            S.M11 = Sx;
            S.M22 = Sy;
            S.M33 = Sz;

            modelview *= S;
        }

        public void BackfaceCulling(bool value, FrontFaceDirection face = FrontFaceDirection.Cw) {
            culling = value;
            this.face = face;
        }

        public void Perspective(float left, float right, float bottom, float top, float near, float far) {
            this.left = left; this.right = right;
            this.bottom = bottom; this.top = top;
            this.near = near; this.far = far;
            
            projection = Matrix4.Identity;

            projection.M11 = 2.0f * near / (right - left);
            projection.M22 = 2.0f * near / (top - bottom);
            projection.M13 = (right + left) / (right - left);
            projection.M23 = (top + bottom) / (top - bottom);
            projection.M33 = -(far + near) / (far - near);
            projection.M43 = -1.0f;
            projection.M34 = -2.0f * far * near / (far - near);
            projection.M44 = 0.0f;

            projection.Transpose();
            UpdateMVP();

            //Matrix4 m = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
            //Matrix4 m = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, near, far);
            //Matrix4 m = Matrix4.CreateOrthographicOffCenter(-(right - left) / 2 / 0f, (right - left) / 2, -(top - bottom) / 2.0f, (top - bottom) / 2.0f, near, far);
            //GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity(); GL.MultMatrix(ref m); GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Viewport(int width, int height) {
            viewport = Matrix4.Identity;
            viewport.M11 = width / 2.0f; viewport.M22 = height / 2.0f;
            viewport.M14 = width / 2.0f; viewport.M24 = height / 2.0f;

            viewport.Transpose();
        }

        private void UpdateMVP() {
            mvp = Matrix4.Mult(modelview, projection);
        }

        public void ViewportResized(int width, int height) {

        }

        public void Begin(PrimitiveType rendermode) {
            begun = true;

            if (rendermode == PrimitiveType.Triangles)
                primitive = new List<Vector4>(3);
            else if (rendermode == PrimitiveType.Points)
                primitive = new List<Vector4>(1);

            GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();

            GL.Begin(rendermode);
        }

        public void Vertex(Vector3 vertex) {
            if (!begun) return;

            Vector4 vh = new Vector4(vertex, 1);
            primitive.Add(vh);

            if (primitive.Count == primitive.Capacity) {
                bool discard = false;

                if (culling && primitive.Capacity >= 3) {
                    Vector3 normal = Vector3.Zero;
                    Vector3 edge1 = Vector3.Zero;
                    Vector3 edge2 = Vector3.Zero;

                    if (face == FrontFaceDirection.Cw){
                        edge1 = new Vector3(primitive[1] - primitive[0]);
                        edge2 = new Vector3(primitive[2] - primitive[0]);
                    }
                    else if (face == FrontFaceDirection.Ccw) {
                        edge1 = new Vector3(primitive[2] - primitive[0]);
                        edge2 = new Vector3(primitive[1] - primitive[0]);
                    }

                    normal = Vector3.Cross(edge1, edge2);
                    normal.Normalize();
                    if (Vector3.Dot(normal, cameraDirection) < 0)
                        discard = true;
                }

                foreach (Vector4 v in primitive) {
                    if (discard) break;

                    Vector4 transformed = Vector4.Transform(v, mvp);

                    if (transformed.X < -transformed.Z || transformed.X > transformed.Z ||
                        transformed.Y < -transformed.Z || transformed.Y > transformed.Z || transformed.Z < near || transformed.Z > far)
                        discard = true;

                    if (discard) break;

                    transformed /= transformed.W;

                    transformed = Vector4.Transform(transformed, viewport);

                    if (!discard)
                        GL.Vertex4(transformed);
                }

                primitive = new List<Vector4>(primitive.Capacity);
            }

        }

        public void Normal(Vector3 normal) {
            if (!begun) return;

            //
        }

        public void End() {
            GL.End();
            begun = false;
        }

        public Vector3[] Transform(Vector3[] vertices) {
            Vector4[] vh = new Vector4[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                vh[i] = new Vector4(vertices[i], 1.0f);

            vh = Transform(vh);
            Vector3[] transformed = new Vector3[vh.Length];

            for (int i = 0; i < vh.Length; i++)
                transformed[i] = new Vector3(vh[i]);

            return transformed;
        }

        public Vector4[] Transform(Vector4[] vertices) {
            Matrix4 MVP = ModelviewProjection;
            Vector4[] transformed = new Vector4[vertices.Length];

            for (int i = 0; i < vertices.Length; i++) {
                transformed[i] = Vector4.Transform(vertices[i], MVP);
                transformed[i] /= transformed[i].W;
                transformed[i] = Vector4.Transform(transformed[i], viewport);
            }

            return transformed;
        }
    }
}
