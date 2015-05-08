﻿using System;
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
        private Vector2 viewportDimensions;

        float left, right, top, bottom, near, far;

        private Vector3 cameraDirection;
        private bool culling;
        private FrontFaceDirection face;

        private bool begun;
        private PrimitiveType mode;

        private List<Vector4> vertices;
        private List<Vector3> normals;
        private List<Vector2> textureCoordinates;

        private List<Vector4> inViewport;

        private bool hasNormals;
        private bool hasTexture;

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
            Vector3 direction = (eye - target).Normalized(); // right-hand coordinate system: invert the direction
            Vector3 right = Vector3.Cross(up, direction).Normalized();
            Vector3 realUp = Vector3.Cross(direction, right).Normalized();

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

        #region Basic Geometric Transformations
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
        #endregion

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
        }

        public void Viewport(int width, int height) {
            viewport = Matrix4.Identity;
            viewport.M11 = width / 2.0f; viewport.M22 = height / 2.0f;
            viewport.M14 = width / 2.0f; viewport.M24 = height / 2.0f;

            viewport.Transpose();

            viewportDimensions = new Vector2(width, height);
        }

        private void UpdateMVP() {
            mvp = Matrix4.Mult(modelview, projection);
        }

        public void Begin(PrimitiveType rendermode) {
            if (begun) throw new Exception("Tried to call Begin() a second time before calling End().");
            begun = true;

            mode = rendermode;

            vertices = new List<Vector4>();
            normals = new List<Vector3>();
            textureCoordinates = new List<Vector2>();
        }

        public void Vertex(Vector3 vertex) {
            if (!begun) throw new Exception("Tried to load vertex before call to Begin()");

            Vector4 vh = new Vector4(vertex, 1);
            vertices.Add(vh);
            
        }

        public void Normal(Vector3 normal) {
            if (!begun) throw new Exception("Tried to load normal before call to Begin()");

            normals.Add(normal);
        }

        public void TextureCoordinate(Vector2 textureCoordinate) {
            if (!begun) throw new Exception("Tried to load texture coordinate before call to Begin()");

            textureCoordinates.Add(textureCoordinate);
        }

        public void End() {
            if (!begun) throw new Exception("Tried to call End() before call to Begin()");

            hasNormals = normals.Count > 0;
            hasTexture = textureCoordinates.Count > 0;

            if (culling && mode == PrimitiveType.Triangles)
                CullBackfaces();

            TransformMVP();
            SimpleClipping();
            PerspectiveDivision();
            MapToViewport();
            Raster();

            begun = false;
        }

        #region New transformation methods

        private void CullBackfaces() {
            int index = 0;

            Vector3 edge1;
            Vector3 edge2;
            Vector3 normal;

            while (index + 2 <= vertices.Count - 1) {
                edge1 = Vector3.Zero;
                edge2 = Vector3.Zero;
                normal = Vector3.Zero;

                edge1 = (face == FrontFaceDirection.Cw) ?
                        new Vector3(vertices[index + 1] - vertices[index]) :
                        new Vector3(vertices[index + 2] - vertices[index]);

                edge2 = (face == FrontFaceDirection.Cw) ?
                        new Vector3(vertices[index + 2] - vertices[index]) :
                        new Vector3(vertices[index + 1] - vertices[index]);

                normal = Vector3.Cross(edge1, edge2).Normalized();

                if (Vector3.Dot(normal, cameraDirection) < 0)
                    DiscardFace(index);
                else
                    index += 3;
                    
            }
        }

        private void TransformMVP() {
            for (int index = 0; index < vertices.Count; index++)
                vertices[index] = Vector4.Transform(vertices[index], mvp);
        }

        private void SimpleClipping() {
            int face = 0;

            while (face + 2 <= vertices.Count - 1) {
                bool faceDiscarded = false;

                for (int vertex = 0; vertex < 3; vertex++) {
                    int index = face + vertex;
                    if (Math.Abs(vertices[index].X) > Math.Abs(vertices[index].Z) ||
                        Math.Abs(vertices[index].Y) > Math.Abs(vertices[index].Z) ||
                        vertices[index].Z < near || vertices[index].Z > far)

                        faceDiscarded = true;
                }

                if (faceDiscarded)
                    DiscardFace(face);
                else
                    face += 3;
            }
        }

        private void PerspectiveDivision() {
            for (int index = 0; index < vertices.Count; index++)
                vertices[index] /= vertices[index].W;
        }

        private void MapToViewport() {
            inViewport = new List<Vector4>();

            for (int index = 0; index < vertices.Count; index++)
                inViewport.Add(Vector4.Transform(vertices[index], viewport));
        }

        private void Raster() {
            GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();

            GL.Begin(mode);
            
            foreach (Vector4 v in vertices)
                GL.Vertex4(v);

            GL.End();
        }

        private void DiscardFace(int startIndex) {
            vertices.RemoveRange(startIndex, 3);
            if (hasNormals) normals.RemoveRange(startIndex, 3);
            if (hasTexture) textureCoordinates.RemoveRange(startIndex, 3);
        }

        #endregion


        #region Old transformation functions
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
        #endregion

    }
}
