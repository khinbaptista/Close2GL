using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Close2GL
{
    public partial class MainWindow : Form
    {
        bool loadedOpenGL;
        bool loadedClose2GL;

        bool meshLoaded;
        bool inputing;

        bool depthBuffer;
        bool backfaceCulling;
        FrontFaceDirection face;
        PrimitiveType mode;
        bool local;
        float projLeftX, projRightX, projTopY, projBottomY, projNearZ, projFarZ;
        float speed;

        Close2GL gl;
        Camera camera;
        Mesh mesh;

        Vector3 meshColor;

        public MainWindow() {
            InitializeComponent();

            loadedOpenGL = false;
            loadedClose2GL = false;
            meshLoaded = false;
            inputing = false;

            depthBuffer = checkDepth.Checked;
            backfaceCulling = checkCulling.Checked;
            face = FrontFaceDirection.Cw;
            mode = PrimitiveType.Points;
            local = false;
            projLeftX = (float)projLeft.Value;
            projRightX = (float)projRight.Value;
            projTopY = (float)projTop.Value;
            projBottomY = (float)projBottom.Value;
            projNearZ = (float)projNear.Value;
            projFarZ = (float)projFar.Value;
            speed = (float)cameraSpeed.Value;

            camera = new Camera();
            camera.Position = VectorFromControl(cameraPosX, cameraPosY, cameraPosZ);
            camera.Target = VectorFromControl(cameraTargetX, cameraTargetY, cameraTargetZ);
            camera.Up = VectorFromControl(cameraUpX, cameraUpY, cameraUpZ);
            meshColor = VectorFromControl(meshColorR, meshColorG, meshColorB);

            gl = new Close2GL();
        }

        #region Event handling

        private void glControl1_Load(object sender, EventArgs e) {
            loadedOpenGL = true;

            glControl1.MakeCurrent();
            SetProjection(glControl1);
            GL.ClearColor(Color.Black);
        }

        private void glControl2_Load(object sender, EventArgs e) {
            loadedClose2GL = true;

            glControl2.MakeCurrent();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl2.Width, 0, glControl2.Height, projNearZ, projFarZ);
            GL.Viewport(0, 0, glControl2.Width, glControl2.Height);

            gl.Perspective(-1, 1, -1, 1, projNearZ, projFarZ);
            gl.Viewport(glControl2.Width, glControl2.Height);
            //GL.ClearColor(Color.AliceBlue);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e) {
            if (!loadedOpenGL) return;

            glControl1.MakeCurrent();
            Matrix4 view = camera.ViewMatrix;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MultMatrix(ref view);
            
            GL.Color3(meshColor);
            GL.Begin(mode);
            if (!meshLoaded) {
                GL.Vertex3(-10.0f, -10.0f, 0.0f);
                GL.Vertex3(0.0f, 5.0f, 0.0f);
                GL.Vertex3(10.0f, -10.0f, 0.0f);
            }
            else {
                mesh.Render();
            }

            GL.End();

            glControl1.SwapBuffers();
        }

        private void glControl2_Paint(object sender, PaintEventArgs e) {
            if (!loadedClose2GL) return;

            if (inputing) HandleInput();

            glControl2.MakeCurrent();
            Matrix4 view = camera.ViewMatrix;

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            gl.ResetModelview();
            gl.LookAt(camera.Position, camera.Target, camera.Up);


            gl.Color(meshColor);
            gl.Begin(mode);

            if (!meshLoaded) {
                gl.Vertex(new Vector3(-10.0f, -10.0f, 0.0f));
                gl.Vertex(new Vector3(0.0f, 5.0f, 0.0f));
                gl.Vertex(new Vector3(10.0f, -10.0f, 0.0f));
            }
            else
                mesh.Render2(gl);

            gl.End();

            glControl2.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e) {
            if (!loadedOpenGL) return;

            glControl1.MakeCurrent();

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            SetProjection(glControl1);
        }

        private void glControl2_Resize(object sender, EventArgs e) {
            if (!loadedClose2GL) return;

            glControl2.MakeCurrent();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl2.Width, 0, glControl2.Height, projNearZ, projFarZ);
            GL.Viewport(0, 0, glControl2.Width, glControl2.Height);

            gl.Viewport(glControl2.Width, glControl2.Height);
        }

        private void glControls_KeyDown(object sender, KeyEventArgs e) {
            inputing = true;

            glControl1.Invalidate();
            glControl2.Invalidate();
        }

        #endregion

        private void HandleInput() {
            bool inputed = false;
            KeyboardState k = Keyboard.GetState();

            if (k.IsKeyDown(Key.W)) {
                inputed = true;
                if (local) camera.MoveForward(speed);
                else camera.Position += new Vector3(0.0f, 0.0f, -speed);
                UpdateCameraControls();
            }
            else if (k.IsKeyDown(Key.S)) {
                inputed = true;
                if (local) camera.MoveBackward(speed);
                else camera.Position += new Vector3(0.0f, 0.0f, speed);
                UpdateCameraControls();
            }

            if (k.IsKeyDown(Key.A)) {
                inputed = true;
                if (local) camera.MoveLeft(speed);
                else camera.Position += new Vector3(-speed, 0.0f, 0.0f);
                UpdateCameraControls();
            }
            else if (k.IsKeyDown(Key.D)) {
                inputed = true;
                if (local) camera.MoveRight(speed);
                else camera.Position += new Vector3(speed, 0.0f, 0.0f);
                UpdateCameraControls();
            }

            if (k.IsKeyDown(Key.Q)) {
                inputed = true;
                if (local) camera.MoveUp(speed);
                else camera.Position += new Vector3(0.0f, speed, 0.0f);
                UpdateCameraControls();
            }
            else if (k.IsKeyDown(Key.E)) {
                inputed = true;
                if (local) camera.MoveDown(speed);
                else camera.Position += new Vector3(0.0f, -speed, 0.0f);
                UpdateCameraControls();
            }

            if (k.IsKeyDown(Key.Escape)) {
                mesh = null;
                meshLoaded = false;
                glControl1.Invalidate(); glControl2.Invalidate();
            }


            if (!inputed)
                inputing = false;

            //glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void SetProjection(GLControl control) {
            Matrix4 projection = Matrix4.CreatePerspectiveOffCenter(projLeftX, projRightX, projBottomY, projTopY, projNearZ, projFarZ);

            control.MakeCurrent();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MultMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);

            control.Invalidate();
        }

        #region Update control values

        private Vector3 VectorFromControl(NumericUpDown x, NumericUpDown y, NumericUpDown z) {
            return new Vector3((float)x.Value, (float)y.Value, (float)z.Value);
        }

        private void UpdateControlFromVector(Vector3 value, NumericUpDown x, NumericUpDown y, NumericUpDown z) {
            x.Value = (decimal)value.X; y.Value = (decimal)value.Y; z.Value = (decimal)value.Z;
        }

        private void UpdateCameraPositionControls() {
            UpdateControlFromVector(camera.Position, cameraPosX, cameraPosY, cameraPosZ);
        }

        private void UpdateCameraTargetControls() {
            UpdateControlFromVector(camera.Target, cameraTargetX, cameraTargetY, cameraTargetZ);
        }

        private void UpdateCameraUpControls() {
            UpdateControlFromVector(camera.Up, cameraUpX, cameraUpY, cameraUpZ);
        }

        private void UpdateCameraControls() {
            UpdateCameraPositionControls();
            UpdateCameraTargetControls();
            UpdateCameraUpControls();
        }

        private void checkDepth_Click(object sender, EventArgs e) {
            depthBuffer = checkDepth.Checked;
        }

        #endregion

        #region Control values changed by user

        private void checkDepth_CheckedChanged(object sender, EventArgs e) {
            depthBuffer = checkDepth.Checked;

            glControl1.MakeCurrent();
            if (depthBuffer) {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);
            }
            else GL.Disable(EnableCap.DepthTest);

            glControl2.MakeCurrent();
            if (depthBuffer) {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);
            }
            else GL.Disable(EnableCap.DepthTest);

            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void checkCulling_CheckedChanged(object sender, EventArgs e) {
            backfaceCulling = checkCulling.Checked;

            glControl1.MakeCurrent();
            if (backfaceCulling) {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(face);
            }
            else GL.Disable(EnableCap.CullFace);


            gl.BackfaceCulling(backfaceCulling, face);

            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void radioCW_CheckedChanged(object sender, EventArgs e) {
            if (radioCW.Checked) face = FrontFaceDirection.Cw;
            else face = FrontFaceDirection.Ccw;

            glControl1.MakeCurrent();
            if (backfaceCulling)
                GL.FrontFace(face);

            glControl2.MakeCurrent();
            if (backfaceCulling) gl.BackfaceCulling(true, face);

            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void RenderMode_Changed(object sender, EventArgs e) {
            bool wireframe = false;
            
            if (radioPoints.Checked) {
                mode = PrimitiveType.Points;
                wireframe = false;
            }
            else if (radioWireframe.Checked) {
                mode = PrimitiveType.Triangles;
                wireframe = true;
            }
            else if (radioTriangles.Checked) {
                mode = PrimitiveType.Triangles;
                wireframe = false;
            }

            if (wireframe) {
                glControl1.MakeCurrent(); GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                glControl2.MakeCurrent(); GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else {
                glControl1.MakeCurrent(); GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                glControl2.MakeCurrent(); GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            glControl1.Invalidate();
            glControl2.Invalidate();
        }

        private void radioWorld_CheckedChanged(object sender, EventArgs e) {
            local = radioLocal.Checked;
        }

        private void checkLockTarget_CheckedChanged(object sender, EventArgs e) {
            camera.TargetLock = checkLockTarget.Checked;
        }

        private void CameraPosition_Changed(object sender, EventArgs e) {
            camera.Position = VectorFromControl(cameraPosX, cameraPosY, cameraPosZ);
            
            if (!camera.TargetLock)
                UpdateCameraTargetControls();

            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void CameraTarget_Changed(object sender, EventArgs e) {
            camera.Target = VectorFromControl(cameraTargetX, cameraTargetY, cameraTargetZ);
            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void CameraUp_Changed(object sender, EventArgs e) {
            camera.Up = VectorFromControl(cameraUpX, cameraUpY, cameraUpZ);
            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void ProjectionBounds_Changed(object sender, EventArgs e) {
            projLeftX = (float)projLeft.Value;
            projRightX = (float)projRight.Value;
            projTopY = (float)projTop.Value;
            projBottomY = (float)projBottom.Value;

            SetProjection(glControl1);
            gl.Perspective(projLeftX, projRightX, projBottomY, projTopY, projNearZ, projFarZ);

            glControl1.Invalidate();
            glControl2.Invalidate();
        }

        private void projNear_ValueChanged(object sender, EventArgs e) {
            projNearZ = (float)projNear.Value;

            SetProjection(glControl1);
            gl.Perspective(projLeftX, projRightX, projBottomY, projTopY, projNearZ, projFarZ);
            glControl2.Invalidate();
        }

        private void projFar_ValueChanged(object sender, EventArgs e) {
            projFarZ = (float)projFar.Value;
            
            SetProjection(glControl1);
            gl.Perspective(projLeftX, projRightX, projBottomY, projTopY, projNearZ, projFarZ);
            glControl2.Invalidate();
        }

        private void buttonReset_Click(object sender, EventArgs e) {
            camera = new Camera();
            camera.TargetLock = checkLockTarget.Checked;
            UpdateCameraControls();
        }

        private void cameraSpeed_ValueChanged(object sender, EventArgs e) {
            speed = (float)cameraSpeed.Value;
        }

        private void buttonLoadMesh_Click(object sender, EventArgs e) {
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Filter = "Input files|*.in";
            openFileDialog1.FileName = "";
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel) return;

            mesh = new Mesh(openFileDialog1.FileName);
            meshLoaded = true;
            glControl1.Invalidate(); glControl2.Invalidate();
        }

        private void meshColor_Changed(object sender, EventArgs e) {
            meshColor = VectorFromControl(meshColorR, meshColorG, meshColorB);
            glControl1.Invalidate(); glControl2.Invalidate();
        }

        #endregion

    }
}
