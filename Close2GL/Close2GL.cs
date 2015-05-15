using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
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
        private int vpW, vpH;

        float left, right, top, bottom, near, far;

        private Vector3 cameraPosition;
        private Vector3 cameraDirection;
        private bool culling;
        private FrontFaceDirection face;

        private bool begun;
        private PrimitiveType mode;
        private bool wireframe;

        private bool useLight;
        private Vector3 lightPosition;
        private Vector3 lightColor;
        private Vector3 ambientLight;

        private Vector3 materialColor;
        private Vector3 materialAmbient;
        private Vector3 materialSpecular;
        private float materialShininess;

        private List<Vector4> vertices;
        private List<Vector3> normals;
        private List<Vector3> faceNormals;
        private List<Vector3> colors;
        private List<Vector2> textureCoordinates;

        private bool hasColors;
        private bool hasNormals;
        private bool hasTexture;

        private bool smooth;
        

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
            smooth = false;

            useLight = false;
            lightPosition = Vector3.Zero;
            lightColor = new Vector3(0.5f, 0.5f, 0.5f);
            ambientLight = new Vector3(0.5f, 0.5f, 0.5f);
            
            materialColor = new Vector3(0.5f, 0.5f, 0.5f);
            materialAmbient = new Vector3(0.5f, 0.5f, 0.5f);
            materialSpecular = new Vector3(0.5f, 0.5f, 0.5f);

            materialShininess = 30f;
        }

        public void Color(Vector3 color) {
            materialColor = color;
        }

        public void EnableLight(bool value = true) {
            useLight = value;
        }

        public void Light(Vector3 position, Vector3 color, Vector3 ambient) {
            lightPosition = position;
            lightColor = color;
            ambientLight = ambient;
        }

        public void Material(Vector3 color, Vector3 ambient, Vector3 specular, float shininess) {
            materialColor = color;
            materialAmbient = ambient;
            materialSpecular = specular;
            materialShininess = shininess;
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
            cameraPosition = eye;
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

        public void RenderWireframe(bool value) {
            wireframe = value;
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

            vpW = width;
            vpH = height;
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
            faceNormals = new List<Vector3>();
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

        public void FaceNormal(Vector3 normal) {
            if (!begun) throw new Exception("Tried to load normal before call to Begin()");

            faceNormals.Add(normal);
        }

        public void TextureCoordinate(Vector2 textureCoordinate) {
            if (!begun) throw new Exception("Tried to load texture coordinate before call to Begin()");

            textureCoordinates.Add(textureCoordinate);
        }

        public void SetSmooth(bool value) {
            smooth = value;
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
            Vector3[] colorList = ShadeVertices();
            MapToViewport();
            Raster(colorList);

            begun = false;
        }

        #region New transformation methods

        private void CullBackfaces() {
            int index = 0;

            Vector3 edge1;
            Vector3 edge2;
            Vector3 normal;

            faceNormals = new List<Vector3>();

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

                faceNormals.Add(normal);
                if (Vector3.Dot(normal, cameraDirection) < 0)
                    DiscardFace(index);
                else
                    index += 3;
            }
        }

        private void TransformMVP() {
            for (int index = 0; index < vertices.Count; index++) {
                vertices[index] = Vector4.Transform(vertices[index], mvp);
                if (hasNormals)
                    normals[index] = Vector3.TransformNormal(normals[index], mvp);
            }
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
            for (int index = 0; index < vertices.Count; index++)
                vertices[index] = Vector4.Transform(vertices[index], viewport);
        }

        private void Raster(Vector3[] colors) {
            Vector3[] colorBuffer = new Vector3[vpW * vpH];

            // Raster vertices as points
            if (mode == PrimitiveType.Points || mode == PrimitiveType.Triangles)
                foreach (Vector4 v in vertices)
                    colorBuffer[FindBufferPosition(v.X, v.Y)] = materialColor;

            if (mode == PrimitiveType.Triangles) {
                Edge[] edges = new Edge[3];

                for (int index = 0; index + 2 < vertices.Count; index += 3) { // index + 2 <= vertices.Count - 1
                    int[] ordered = OrderByY(index);

                    edges[0] = new Edge(vertices[ordered[0]], vertices[ordered[1]]);
                    edges[1] = new Edge(vertices[ordered[0]], vertices[ordered[2]]);
                    edges[2] = new Edge(vertices[ordered[1]], vertices[ordered[2]]);

                    //Vector3[] vertexColors = ColorVertices(index / 3, ordered[0], ordered[1], ordered[2]);

                    // Raster lines connecting vertices into triangles
                    for (int ei = 0; ei < 3; ei++) {

                        while (!edges[ei].Finished) {
                            colorBuffer[FindBufferPosition(edges[ei].current)] = InterpolateColor(ordered[0], ordered[1], ordered[2],
                                                                                        colors, edges[ei].current.Xyz);
                            edges[ei].Next();
                        }
                    }

                    // Fill the triangles if required
                    if (!wireframe) {
                        int scanline = (int)Math.Round(edges[0].start.Y);// +1;
                        int startX, endX, currentX;
                        int index1 = 0, index2 = 1;

                        for (int pair = 0; pair < 2; pair++) {
                            do {
                                startX = edges[index1].GetX(scanline);
                                endX = edges[index2].GetX(scanline);

                                if (startX > endX) {
                                    Swap(ref startX, ref endX);
                                    Swap(ref index1, ref index2);
                                }

                                if (startX <= 0 || endX <= 0) { scanline++; continue; }

                                currentX = startX;

                                while (currentX < endX) {
                                    colorBuffer[FindBufferPosition(currentX, scanline)] = InterpolateColor(ordered[0], ordered[1], ordered[2],
                                                                                           colors, new Vector3(currentX, scanline, 0));
                                    currentX++;
                                }

                                scanline++;
                            } while (scanline < edges[index1].end.Y && scanline < edges[index2].end.Y);

                            if (scanline >= edges[index1].end.Y) index1 = 2;
                            else index2 = 2;
                        }

                    }
                }

                
            }

            GL.DrawPixels<Vector3>(vpW, vpH, PixelFormat.Rgb, PixelType.Float, colorBuffer);
        }

        private Vector3[] ShadeVertices() {
            Vector3[] colors;

            Vector3 V, L, N, R;
            float dotNL;
            Vector3 ambientComponent = ambientLight * materialAmbient;

            if (smooth) {
                colors = new Vector3[vertices.Count];

                for (int i = 0; i < colors.Length; i++) {
                    N = normals[i].Normalized();
                    V = (cameraPosition - vertices[i].Xyz).Normalized();
                    L = (lightPosition - vertices[i].Xyz).Normalized();
                    dotNL = Vector3.Dot(N, L);
                    R = 2 * dotNL * N - L;
                    float dotVR = Vector3.Dot(V, R);
                    float spec = (float)Math.Pow(dotVR, materialShininess);

                    colors[i] = ambientComponent + lightColor * (materialColor * dotNL + materialSpecular * spec);
                }
            }
            else {
                colors = new Vector3[vertices.Count / 3];

                for (int i = 0; i + 2 < colors.Length; i++) {
                    N = faceNormals[i].Normalized();
                    V = (cameraPosition - vertices[i * 3].Xyz).Normalized();
                    L = (lightPosition - vertices[i * 3].Xyz).Normalized();
                    dotNL = Vector3.Dot(N, L);
                    R = 2 * dotNL * N - L;
                    float dotVR = Vector3.Dot(V, R);
                    float spec = (float)Math.Pow(dotVR, materialShininess);

                    colors[i] = ambientComponent + lightColor * (materialColor * dotNL + materialSpecular * spec);
                }
            }

            return colors;
        }
        
        private Vector3 InterpolateColor(int index0, int index1, int index2, Vector3[] colors, Vector3 fragment) {
            if (!smooth || colors.Length == 1) return colors[index0 / 3];
            
            Vector3 full = vertices[index1].Xyz - vertices[index0].Xyz;
            Vector3 part = fragment - vertices[index0].Xyz;
            float alpha0 = Vector3.Dot(full.Normalized(), part.Normalized());

            full = vertices[index2].Xyz - vertices[index0].Xyz;
            float alpha1 = Vector3.Dot(full.Normalized(), part.Normalized());

            Vector3 result0 = Vector3.Lerp(colors[index0], colors[index1], alpha0);
            Vector3 result1 = Vector3.Lerp(colors[index0], colors[index2], alpha1);

            if (alpha0 > alpha1)
                return Vector3.Lerp(result1, result0, alpha1 / alpha0);
            return Vector3.Lerp(result0, result1, alpha0 / alpha1);
        }

        private Vector3 FindBarycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
            // As seen at
            // http://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
            
            Vector3 result = Vector3.Zero;

            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;

            result.Y = (d11 * d20 - d01 * d21) / denom;
            result.Z = (d00 * d21 - d01 * d20) / denom;
            result.X = 1.0f - result.X - result.Z;

            return result;
        }

        private void Swap(ref int a, ref int b) {
            int swap = a;
            a = b;
            b = swap;
        }

        private int[] OrderByY(int startIndex) {
            int first = 0, second = 0, third = 0;

            first = startIndex;
            second = startIndex + 1;
            third = startIndex + 2;

            if (vertices[first].Y > vertices[second].Y) Swap(ref first, ref second);
            if (vertices[second].Y > vertices[third].Y) {
                Swap(ref second, ref third);
                if (vertices[first].Y > vertices[second].Y) Swap(ref first, ref second);
            }

            return new int[] { first, second, third };
        }

        private int FindBufferPosition(Vector4 v) {
            return FindBufferPosition(v.X, v.Y);
        }

        private int FindBufferPosition(float x, float y) {
            return (int)(vpW * Math.Round(y) + Math.Round(x));
        }

        private Vector3 CalculateColor(Vector4 v) {
            return materialColor;
        }

        private void DiscardFace(int startIndex) {
            vertices.RemoveRange(startIndex, 3);
            if (hasNormals) normals.RemoveRange(startIndex, 3);
            if (hasTexture) textureCoordinates.RemoveRange(startIndex, 3);
            if (faceNormals.Count > 0) faceNormals.RemoveAt(startIndex / 3);
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
