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
        private SplitContainer splitContainer1;
        private GroupBox groupControls;
        private SplitContainer splitContainer2;
        private GroupBox groupOpenGL;
        private GLControl glControl1;
        private GroupBox groupClose2GL;
        private GLControl glControl2;
        private CheckBox checkDepth;
        private CheckBox checkCulling;
        private GroupBox groupFace;
        private RadioButton radioCW;
        private RadioButton radioCCW;
        private GroupBox groupRendermode;
        private RadioButton radioTriangles;
        private RadioButton radioWireframe;
        private RadioButton radioPoints;
        private GroupBox groupCamera;
        private GroupBox groupPivot;
        private Label label1;
        private NumericUpDown cameraSpeed;
        private CheckBox checkLockTarget;
        private CheckBox checkLight;
        private GroupBox groupLightPos;
        private RadioButton radioLocal;
        private RadioButton radioWorld;
        private GroupBox groupLightColor;
        private NumericUpDown numericLightColorB;
        private Label label4;
        private NumericUpDown numericLightColorG;
        private Label label5;
        private NumericUpDown numericLightColorR;
        private Label label6;
        private NumericUpDown numericLightPosZ;
        private Label labelLightPosZ;
        private NumericUpDown numericLightPosY;
        private Label labelLightPosY;
        private NumericUpDown numericLightPosX;
        private Label labelLightPosX;
        private GroupBox groupProj;
        private OpenFileDialog openFileDialog1;
        private NumericUpDown projLeft;
        private Label labelLeft;
        private NumericUpDown projTop;
        private Label labelTop;
        private NumericUpDown projBottom;
        private Label labelBottom;
        private NumericUpDown projRight;
        private Label labelRight;
        private NumericUpDown projFar;
        private Label labelFar;
        private NumericUpDown projNear;
        private Label labelNear;
        private Button buttonReset;
        private GroupBox groupCameraPosition;
        private NumericUpDown cameraPosZ;
        private Label labelCamPosZ;
        private NumericUpDown cameraPosY;
        private Label labelCamPosY;
        private NumericUpDown cameraPosX;
        private Label labelCamPosX;
        private GroupBox groupCameraTarget;
        private NumericUpDown cameraTargetZ;
        private Label labelCamTargetZ;
        private NumericUpDown cameraTargetY;
        private Label labelCamTargetY;
        private NumericUpDown cameraTargetX;
        private Label labelCamTargetX;
        private GroupBox groupCameraUp;
        private NumericUpDown cameraUpZ;
        private Label labelCamUpZ;
        private NumericUpDown cameraUpY;
        private Label labelCamUpY;
        private NumericUpDown cameraUpX;
        private Label labelCamUpX;
        private GroupBox groupMesh;
        private GroupBox groupMeshColor;
        private NumericUpDown meshColorB;
        private Label labelMeshColorB;
        private NumericUpDown meshColorG;
        private Label labelMeshColorG;
        private NumericUpDown meshColorR;
        private Label labelMeshColorR;
        private Button buttonLoadMesh;

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

            gl.Perspective(projLeftX, projRightX, projBottomY, projTopY, projNearZ, projFarZ);
            gl.Viewport(glControl2.Width, glControl2.Height);
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
                GL.Vertex3(-10.0f, -5.0f, 0.0f);
                GL.Vertex3(0.0f, 5.0f, 0.0f);
                GL.Vertex3(10.0f, -5.0f, 0.0f);
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
                gl.Vertex(new Vector3(-10.0f, -5.0f, 0.0f));
                gl.Vertex(new Vector3(0.0f, 5.0f, 0.0f));
                gl.Vertex(new Vector3(10.0f, -5.0f, 0.0f));
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

            glControl1.MakeCurrent();
            if (wireframe)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            gl.RenderWireframe(wireframe);

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

        private void InitializeComponent() {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupControls = new System.Windows.Forms.GroupBox();
            this.groupProj = new System.Windows.Forms.GroupBox();
            this.projFar = new System.Windows.Forms.NumericUpDown();
            this.labelFar = new System.Windows.Forms.Label();
            this.projNear = new System.Windows.Forms.NumericUpDown();
            this.labelNear = new System.Windows.Forms.Label();
            this.projTop = new System.Windows.Forms.NumericUpDown();
            this.labelTop = new System.Windows.Forms.Label();
            this.projBottom = new System.Windows.Forms.NumericUpDown();
            this.labelBottom = new System.Windows.Forms.Label();
            this.projRight = new System.Windows.Forms.NumericUpDown();
            this.labelRight = new System.Windows.Forms.Label();
            this.projLeft = new System.Windows.Forms.NumericUpDown();
            this.labelLeft = new System.Windows.Forms.Label();
            this.groupLightColor = new System.Windows.Forms.GroupBox();
            this.numericLightColorB = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericLightColorG = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericLightColorR = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.groupLightPos = new System.Windows.Forms.GroupBox();
            this.numericLightPosZ = new System.Windows.Forms.NumericUpDown();
            this.labelLightPosZ = new System.Windows.Forms.Label();
            this.numericLightPosY = new System.Windows.Forms.NumericUpDown();
            this.labelLightPosY = new System.Windows.Forms.Label();
            this.numericLightPosX = new System.Windows.Forms.NumericUpDown();
            this.labelLightPosX = new System.Windows.Forms.Label();
            this.checkLight = new System.Windows.Forms.CheckBox();
            this.groupCamera = new System.Windows.Forms.GroupBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.groupPivot = new System.Windows.Forms.GroupBox();
            this.radioLocal = new System.Windows.Forms.RadioButton();
            this.radioWorld = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.cameraSpeed = new System.Windows.Forms.NumericUpDown();
            this.checkLockTarget = new System.Windows.Forms.CheckBox();
            this.groupRendermode = new System.Windows.Forms.GroupBox();
            this.radioTriangles = new System.Windows.Forms.RadioButton();
            this.radioWireframe = new System.Windows.Forms.RadioButton();
            this.radioPoints = new System.Windows.Forms.RadioButton();
            this.groupFace = new System.Windows.Forms.GroupBox();
            this.radioCCW = new System.Windows.Forms.RadioButton();
            this.radioCW = new System.Windows.Forms.RadioButton();
            this.checkCulling = new System.Windows.Forms.CheckBox();
            this.checkDepth = new System.Windows.Forms.CheckBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupOpenGL = new System.Windows.Forms.GroupBox();
            this.glControl1 = new OpenTK.GLControl();
            this.groupClose2GL = new System.Windows.Forms.GroupBox();
            this.glControl2 = new OpenTK.GLControl();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupCameraPosition = new System.Windows.Forms.GroupBox();
            this.cameraPosZ = new System.Windows.Forms.NumericUpDown();
            this.labelCamPosZ = new System.Windows.Forms.Label();
            this.cameraPosY = new System.Windows.Forms.NumericUpDown();
            this.labelCamPosY = new System.Windows.Forms.Label();
            this.cameraPosX = new System.Windows.Forms.NumericUpDown();
            this.labelCamPosX = new System.Windows.Forms.Label();
            this.groupCameraTarget = new System.Windows.Forms.GroupBox();
            this.cameraTargetZ = new System.Windows.Forms.NumericUpDown();
            this.labelCamTargetZ = new System.Windows.Forms.Label();
            this.cameraTargetY = new System.Windows.Forms.NumericUpDown();
            this.labelCamTargetY = new System.Windows.Forms.Label();
            this.cameraTargetX = new System.Windows.Forms.NumericUpDown();
            this.labelCamTargetX = new System.Windows.Forms.Label();
            this.groupCameraUp = new System.Windows.Forms.GroupBox();
            this.cameraUpZ = new System.Windows.Forms.NumericUpDown();
            this.labelCamUpZ = new System.Windows.Forms.Label();
            this.cameraUpY = new System.Windows.Forms.NumericUpDown();
            this.labelCamUpY = new System.Windows.Forms.Label();
            this.cameraUpX = new System.Windows.Forms.NumericUpDown();
            this.labelCamUpX = new System.Windows.Forms.Label();
            this.groupMesh = new System.Windows.Forms.GroupBox();
            this.groupMeshColor = new System.Windows.Forms.GroupBox();
            this.meshColorB = new System.Windows.Forms.NumericUpDown();
            this.labelMeshColorB = new System.Windows.Forms.Label();
            this.meshColorG = new System.Windows.Forms.NumericUpDown();
            this.labelMeshColorG = new System.Windows.Forms.Label();
            this.meshColorR = new System.Windows.Forms.NumericUpDown();
            this.labelMeshColorR = new System.Windows.Forms.Label();
            this.buttonLoadMesh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupControls.SuspendLayout();
            this.groupProj.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projFar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projNear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projLeft)).BeginInit();
            this.groupLightColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorR)).BeginInit();
            this.groupLightPos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosX)).BeginInit();
            this.groupCamera.SuspendLayout();
            this.groupPivot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraSpeed)).BeginInit();
            this.groupRendermode.SuspendLayout();
            this.groupFace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupOpenGL.SuspendLayout();
            this.groupClose2GL.SuspendLayout();
            this.groupCameraPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosX)).BeginInit();
            this.groupCameraTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetX)).BeginInit();
            this.groupCameraUp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpX)).BeginInit();
            this.groupMesh.SuspendLayout();
            this.groupMeshColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorR)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupControls);
            this.splitContainer1.Panel1.Tag = "panelMenu";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Tag = "panelGraphics";
            this.splitContainer1.Size = new System.Drawing.Size(1504, 899);
            this.splitContainer1.SplitterDistance = 191;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupControls
            // 
            this.groupControls.AutoSize = true;
            this.groupControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupControls.Controls.Add(this.groupMesh);
            this.groupControls.Controls.Add(this.groupProj);
            this.groupControls.Controls.Add(this.groupLightColor);
            this.groupControls.Controls.Add(this.groupLightPos);
            this.groupControls.Controls.Add(this.checkLight);
            this.groupControls.Controls.Add(this.groupCamera);
            this.groupControls.Controls.Add(this.groupRendermode);
            this.groupControls.Controls.Add(this.groupFace);
            this.groupControls.Controls.Add(this.checkCulling);
            this.groupControls.Controls.Add(this.checkDepth);
            this.groupControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControls.Location = new System.Drawing.Point(0, 0);
            this.groupControls.Name = "groupControls";
            this.groupControls.Size = new System.Drawing.Size(1504, 191);
            this.groupControls.TabIndex = 1;
            this.groupControls.TabStop = false;
            this.groupControls.Text = "Graphics Controls";
            // 
            // groupProj
            // 
            this.groupProj.Controls.Add(this.projFar);
            this.groupProj.Controls.Add(this.labelFar);
            this.groupProj.Controls.Add(this.projNear);
            this.groupProj.Controls.Add(this.labelNear);
            this.groupProj.Controls.Add(this.projTop);
            this.groupProj.Controls.Add(this.labelTop);
            this.groupProj.Controls.Add(this.projBottom);
            this.groupProj.Controls.Add(this.labelBottom);
            this.groupProj.Controls.Add(this.projRight);
            this.groupProj.Controls.Add(this.labelRight);
            this.groupProj.Controls.Add(this.projLeft);
            this.groupProj.Controls.Add(this.labelLeft);
            this.groupProj.Location = new System.Drawing.Point(240, 16);
            this.groupProj.Name = "groupProj";
            this.groupProj.Size = new System.Drawing.Size(613, 48);
            this.groupProj.TabIndex = 8;
            this.groupProj.TabStop = false;
            this.groupProj.Text = "Projection";
            // 
            // projFar
            // 
            this.projFar.DecimalPlaces = 2;
            this.projFar.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.projFar.Location = new System.Drawing.Point(537, 18);
            this.projFar.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.projFar.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.projFar.Name = "projFar";
            this.projFar.Size = new System.Drawing.Size(64, 20);
            this.projFar.TabIndex = 11;
            this.projFar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projFar.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.projFar.ValueChanged += new System.EventHandler(this.projFar_ValueChanged);
            // 
            // labelFar
            // 
            this.labelFar.AutoSize = true;
            this.labelFar.Location = new System.Drawing.Point(512, 21);
            this.labelFar.Name = "labelFar";
            this.labelFar.Size = new System.Drawing.Size(22, 13);
            this.labelFar.TabIndex = 10;
            this.labelFar.Text = "Far";
            // 
            // projNear
            // 
            this.projNear.DecimalPlaces = 3;
            this.projNear.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.projNear.Location = new System.Drawing.Point(437, 19);
            this.projNear.Maximum = new decimal(new int[] {
            19999,
            0,
            0,
            196608});
            this.projNear.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.projNear.Name = "projNear";
            this.projNear.Size = new System.Drawing.Size(64, 20);
            this.projNear.TabIndex = 9;
            this.projNear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projNear.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.projNear.ValueChanged += new System.EventHandler(this.projNear_ValueChanged);
            // 
            // labelNear
            // 
            this.labelNear.AutoSize = true;
            this.labelNear.Location = new System.Drawing.Point(406, 22);
            this.labelNear.Name = "labelNear";
            this.labelNear.Size = new System.Drawing.Size(30, 13);
            this.labelNear.TabIndex = 8;
            this.labelNear.Text = "Near";
            // 
            // projTop
            // 
            this.projTop.DecimalPlaces = 3;
            this.projTop.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.projTop.Location = new System.Drawing.Point(333, 19);
            this.projTop.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.projTop.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
            this.projTop.Name = "projTop";
            this.projTop.Size = new System.Drawing.Size(64, 20);
            this.projTop.TabIndex = 7;
            this.projTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projTop.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.projTop.ValueChanged += new System.EventHandler(this.ProjectionBounds_Changed);
            // 
            // labelTop
            // 
            this.labelTop.AutoSize = true;
            this.labelTop.Location = new System.Drawing.Point(308, 22);
            this.labelTop.Name = "labelTop";
            this.labelTop.Size = new System.Drawing.Size(26, 13);
            this.labelTop.TabIndex = 6;
            this.labelTop.Text = "Top";
            // 
            // projBottom
            // 
            this.projBottom.DecimalPlaces = 3;
            this.projBottom.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.projBottom.Location = new System.Drawing.Point(237, 19);
            this.projBottom.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.projBottom.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
            this.projBottom.Name = "projBottom";
            this.projBottom.Size = new System.Drawing.Size(64, 20);
            this.projBottom.TabIndex = 5;
            this.projBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projBottom.Value = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.projBottom.ValueChanged += new System.EventHandler(this.ProjectionBounds_Changed);
            // 
            // labelBottom
            // 
            this.labelBottom.AutoSize = true;
            this.labelBottom.Location = new System.Drawing.Point(198, 22);
            this.labelBottom.Name = "labelBottom";
            this.labelBottom.Size = new System.Drawing.Size(40, 13);
            this.labelBottom.TabIndex = 4;
            this.labelBottom.Text = "Bottom";
            // 
            // projRight
            // 
            this.projRight.DecimalPlaces = 3;
            this.projRight.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.projRight.Location = new System.Drawing.Point(130, 19);
            this.projRight.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.projRight.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
            this.projRight.Name = "projRight";
            this.projRight.Size = new System.Drawing.Size(64, 20);
            this.projRight.TabIndex = 3;
            this.projRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projRight.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.projRight.ValueChanged += new System.EventHandler(this.ProjectionBounds_Changed);
            // 
            // labelRight
            // 
            this.labelRight.AutoSize = true;
            this.labelRight.Location = new System.Drawing.Point(99, 22);
            this.labelRight.Name = "labelRight";
            this.labelRight.Size = new System.Drawing.Size(32, 13);
            this.labelRight.TabIndex = 2;
            this.labelRight.Text = "Right";
            // 
            // projLeft
            // 
            this.projLeft.DecimalPlaces = 3;
            this.projLeft.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.projLeft.Location = new System.Drawing.Point(32, 18);
            this.projLeft.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.projLeft.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
            this.projLeft.Name = "projLeft";
            this.projLeft.Size = new System.Drawing.Size(64, 20);
            this.projLeft.TabIndex = 1;
            this.projLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.projLeft.Value = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.projLeft.ValueChanged += new System.EventHandler(this.ProjectionBounds_Changed);
            // 
            // labelLeft
            // 
            this.labelLeft.AutoSize = true;
            this.labelLeft.Location = new System.Drawing.Point(8, 21);
            this.labelLeft.Name = "labelLeft";
            this.labelLeft.Size = new System.Drawing.Size(25, 13);
            this.labelLeft.TabIndex = 0;
            this.labelLeft.Text = "Left";
            // 
            // groupLightColor
            // 
            this.groupLightColor.Controls.Add(this.numericLightColorB);
            this.groupLightColor.Controls.Add(this.label4);
            this.groupLightColor.Controls.Add(this.numericLightColorG);
            this.groupLightColor.Controls.Add(this.label5);
            this.groupLightColor.Controls.Add(this.numericLightColorR);
            this.groupLightColor.Controls.Add(this.label6);
            this.groupLightColor.Location = new System.Drawing.Point(245, 96);
            this.groupLightColor.Name = "groupLightColor";
            this.groupLightColor.Size = new System.Drawing.Size(88, 88);
            this.groupLightColor.TabIndex = 7;
            this.groupLightColor.TabStop = false;
            this.groupLightColor.Text = "Light color";
            // 
            // numericLightColorB
            // 
            this.numericLightColorB.DecimalPlaces = 3;
            this.numericLightColorB.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.numericLightColorB.Location = new System.Drawing.Point(24, 64);
            this.numericLightColorB.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericLightColorB.Name = "numericLightColorB";
            this.numericLightColorB.Size = new System.Drawing.Size(54, 20);
            this.numericLightColorB.TabIndex = 11;
            this.numericLightColorB.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "B";
            // 
            // numericLightColorG
            // 
            this.numericLightColorG.DecimalPlaces = 3;
            this.numericLightColorG.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.numericLightColorG.Location = new System.Drawing.Point(24, 40);
            this.numericLightColorG.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericLightColorG.Name = "numericLightColorG";
            this.numericLightColorG.Size = new System.Drawing.Size(54, 20);
            this.numericLightColorG.TabIndex = 9;
            this.numericLightColorG.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "G";
            // 
            // numericLightColorR
            // 
            this.numericLightColorR.DecimalPlaces = 3;
            this.numericLightColorR.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.numericLightColorR.Location = new System.Drawing.Point(24, 16);
            this.numericLightColorR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericLightColorR.Name = "numericLightColorR";
            this.numericLightColorR.Size = new System.Drawing.Size(54, 20);
            this.numericLightColorR.TabIndex = 7;
            this.numericLightColorR.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "R";
            // 
            // groupLightPos
            // 
            this.groupLightPos.Controls.Add(this.numericLightPosZ);
            this.groupLightPos.Controls.Add(this.labelLightPosZ);
            this.groupLightPos.Controls.Add(this.numericLightPosY);
            this.groupLightPos.Controls.Add(this.labelLightPosY);
            this.groupLightPos.Controls.Add(this.numericLightPosX);
            this.groupLightPos.Controls.Add(this.labelLightPosX);
            this.groupLightPos.Location = new System.Drawing.Point(136, 96);
            this.groupLightPos.Name = "groupLightPos";
            this.groupLightPos.Size = new System.Drawing.Size(104, 88);
            this.groupLightPos.TabIndex = 5;
            this.groupLightPos.TabStop = false;
            this.groupLightPos.Text = "Light position";
            // 
            // numericLightPosZ
            // 
            this.numericLightPosZ.DecimalPlaces = 3;
            this.numericLightPosZ.Location = new System.Drawing.Point(24, 64);
            this.numericLightPosZ.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericLightPosZ.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.numericLightPosZ.Name = "numericLightPosZ";
            this.numericLightPosZ.Size = new System.Drawing.Size(70, 20);
            this.numericLightPosZ.TabIndex = 5;
            this.numericLightPosZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelLightPosZ
            // 
            this.labelLightPosZ.AutoSize = true;
            this.labelLightPosZ.Location = new System.Drawing.Point(8, 67);
            this.labelLightPosZ.Name = "labelLightPosZ";
            this.labelLightPosZ.Size = new System.Drawing.Size(14, 13);
            this.labelLightPosZ.TabIndex = 4;
            this.labelLightPosZ.Text = "Z";
            // 
            // numericLightPosY
            // 
            this.numericLightPosY.DecimalPlaces = 3;
            this.numericLightPosY.Location = new System.Drawing.Point(24, 40);
            this.numericLightPosY.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericLightPosY.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.numericLightPosY.Name = "numericLightPosY";
            this.numericLightPosY.Size = new System.Drawing.Size(70, 20);
            this.numericLightPosY.TabIndex = 3;
            this.numericLightPosY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelLightPosY
            // 
            this.labelLightPosY.AutoSize = true;
            this.labelLightPosY.Location = new System.Drawing.Point(7, 43);
            this.labelLightPosY.Name = "labelLightPosY";
            this.labelLightPosY.Size = new System.Drawing.Size(14, 13);
            this.labelLightPosY.TabIndex = 2;
            this.labelLightPosY.Text = "Y";
            // 
            // numericLightPosX
            // 
            this.numericLightPosX.DecimalPlaces = 3;
            this.numericLightPosX.Location = new System.Drawing.Point(24, 16);
            this.numericLightPosX.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericLightPosX.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.numericLightPosX.Name = "numericLightPosX";
            this.numericLightPosX.Size = new System.Drawing.Size(70, 20);
            this.numericLightPosX.TabIndex = 1;
            this.numericLightPosX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelLightPosX
            // 
            this.labelLightPosX.AutoSize = true;
            this.labelLightPosX.Location = new System.Drawing.Point(7, 19);
            this.labelLightPosX.Name = "labelLightPosX";
            this.labelLightPosX.Size = new System.Drawing.Size(14, 13);
            this.labelLightPosX.TabIndex = 0;
            this.labelLightPosX.Text = "X";
            // 
            // checkLight
            // 
            this.checkLight.AutoSize = true;
            this.checkLight.Location = new System.Drawing.Point(136, 72);
            this.checkLight.Name = "checkLight";
            this.checkLight.Size = new System.Drawing.Size(63, 17);
            this.checkLight.TabIndex = 6;
            this.checkLight.Text = "Lighting";
            this.checkLight.UseVisualStyleBackColor = true;
            // 
            // groupCamera
            // 
            this.groupCamera.Controls.Add(this.groupCameraUp);
            this.groupCamera.Controls.Add(this.groupCameraTarget);
            this.groupCamera.Controls.Add(this.groupCameraPosition);
            this.groupCamera.Controls.Add(this.buttonReset);
            this.groupCamera.Controls.Add(this.groupPivot);
            this.groupCamera.Controls.Add(this.label1);
            this.groupCamera.Controls.Add(this.cameraSpeed);
            this.groupCamera.Controls.Add(this.checkLockTarget);
            this.groupCamera.Location = new System.Drawing.Point(342, 71);
            this.groupCamera.Name = "groupCamera";
            this.groupCamera.Size = new System.Drawing.Size(511, 113);
            this.groupCamera.TabIndex = 4;
            this.groupCamera.TabStop = false;
            this.groupCamera.Text = "Camera";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(9, 76);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(168, 30);
            this.buttonReset.TabIndex = 4;
            this.buttonReset.Text = "Reset camera position";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // groupPivot
            // 
            this.groupPivot.Controls.Add(this.radioLocal);
            this.groupPivot.Controls.Add(this.radioWorld);
            this.groupPivot.Location = new System.Drawing.Point(110, 11);
            this.groupPivot.Name = "groupPivot";
            this.groupPivot.Size = new System.Drawing.Size(65, 56);
            this.groupPivot.TabIndex = 3;
            this.groupPivot.TabStop = false;
            this.groupPivot.Text = "Pivot";
            // 
            // radioLocal
            // 
            this.radioLocal.AutoSize = true;
            this.radioLocal.Location = new System.Drawing.Point(8, 35);
            this.radioLocal.Name = "radioLocal";
            this.radioLocal.Size = new System.Drawing.Size(51, 17);
            this.radioLocal.TabIndex = 3;
            this.radioLocal.Text = "Local";
            this.radioLocal.UseVisualStyleBackColor = true;
            // 
            // radioWorld
            // 
            this.radioWorld.AutoSize = true;
            this.radioWorld.Checked = true;
            this.radioWorld.Location = new System.Drawing.Point(8, 16);
            this.radioWorld.Name = "radioWorld";
            this.radioWorld.Size = new System.Drawing.Size(53, 17);
            this.radioWorld.TabIndex = 1;
            this.radioWorld.TabStop = true;
            this.radioWorld.Text = "World";
            this.radioWorld.UseVisualStyleBackColor = true;
            this.radioWorld.CheckedChanged += new System.EventHandler(this.radioWorld_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Speed";
            // 
            // cameraSpeed
            // 
            this.cameraSpeed.DecimalPlaces = 3;
            this.cameraSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.cameraSpeed.Location = new System.Drawing.Point(51, 46);
            this.cameraSpeed.Name = "cameraSpeed";
            this.cameraSpeed.Size = new System.Drawing.Size(52, 20);
            this.cameraSpeed.TabIndex = 1;
            this.cameraSpeed.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.cameraSpeed.ValueChanged += new System.EventHandler(this.cameraSpeed_ValueChanged);
            // 
            // checkLockTarget
            // 
            this.checkLockTarget.AutoSize = true;
            this.checkLockTarget.Location = new System.Drawing.Point(9, 22);
            this.checkLockTarget.Name = "checkLockTarget";
            this.checkLockTarget.Size = new System.Drawing.Size(80, 17);
            this.checkLockTarget.TabIndex = 0;
            this.checkLockTarget.Text = "Lock target";
            this.checkLockTarget.UseVisualStyleBackColor = true;
            this.checkLockTarget.CheckedChanged += new System.EventHandler(this.checkLockTarget_CheckedChanged);
            // 
            // groupRendermode
            // 
            this.groupRendermode.Controls.Add(this.radioTriangles);
            this.groupRendermode.Controls.Add(this.radioWireframe);
            this.groupRendermode.Controls.Add(this.radioPoints);
            this.groupRendermode.Location = new System.Drawing.Point(8, 16);
            this.groupRendermode.Name = "groupRendermode";
            this.groupRendermode.Size = new System.Drawing.Size(224, 48);
            this.groupRendermode.TabIndex = 3;
            this.groupRendermode.TabStop = false;
            this.groupRendermode.Text = "Render mode";
            // 
            // radioTriangles
            // 
            this.radioTriangles.AutoSize = true;
            this.radioTriangles.Location = new System.Drawing.Point(150, 19);
            this.radioTriangles.Name = "radioTriangles";
            this.radioTriangles.Size = new System.Drawing.Size(68, 17);
            this.radioTriangles.TabIndex = 2;
            this.radioTriangles.Text = "Triangles";
            this.radioTriangles.UseVisualStyleBackColor = true;
            this.radioTriangles.CheckedChanged += new System.EventHandler(this.RenderMode_Changed);
            // 
            // radioWireframe
            // 
            this.radioWireframe.AutoSize = true;
            this.radioWireframe.Location = new System.Drawing.Point(71, 19);
            this.radioWireframe.Name = "radioWireframe";
            this.radioWireframe.Size = new System.Drawing.Size(73, 17);
            this.radioWireframe.TabIndex = 1;
            this.radioWireframe.Text = "Wireframe";
            this.radioWireframe.UseVisualStyleBackColor = true;
            this.radioWireframe.CheckedChanged += new System.EventHandler(this.RenderMode_Changed);
            // 
            // radioPoints
            // 
            this.radioPoints.AutoSize = true;
            this.radioPoints.Checked = true;
            this.radioPoints.Location = new System.Drawing.Point(6, 19);
            this.radioPoints.Name = "radioPoints";
            this.radioPoints.Size = new System.Drawing.Size(54, 17);
            this.radioPoints.TabIndex = 0;
            this.radioPoints.TabStop = true;
            this.radioPoints.Text = "Points";
            this.radioPoints.UseVisualStyleBackColor = true;
            this.radioPoints.CheckedChanged += new System.EventHandler(this.RenderMode_Changed);
            // 
            // groupFace
            // 
            this.groupFace.Controls.Add(this.radioCCW);
            this.groupFace.Controls.Add(this.radioCW);
            this.groupFace.Location = new System.Drawing.Point(8, 118);
            this.groupFace.Name = "groupFace";
            this.groupFace.Size = new System.Drawing.Size(123, 66);
            this.groupFace.TabIndex = 2;
            this.groupFace.TabStop = false;
            this.groupFace.Text = "Front face";
            // 
            // radioCCW
            // 
            this.radioCCW.AutoSize = true;
            this.radioCCW.Location = new System.Drawing.Point(8, 40);
            this.radioCCW.Name = "radioCCW";
            this.radioCCW.Size = new System.Drawing.Size(112, 17);
            this.radioCCW.TabIndex = 1;
            this.radioCCW.Text = "Counter-clockwise";
            this.radioCCW.UseVisualStyleBackColor = true;
            // 
            // radioCW
            // 
            this.radioCW.AutoSize = true;
            this.radioCW.Checked = true;
            this.radioCW.Location = new System.Drawing.Point(8, 16);
            this.radioCW.Name = "radioCW";
            this.radioCW.Size = new System.Drawing.Size(73, 17);
            this.radioCW.TabIndex = 0;
            this.radioCW.TabStop = true;
            this.radioCW.Text = "Clockwise";
            this.radioCW.UseVisualStyleBackColor = true;
            this.radioCW.CheckedChanged += new System.EventHandler(this.radioCW_CheckedChanged);
            // 
            // checkCulling
            // 
            this.checkCulling.AutoSize = true;
            this.checkCulling.Location = new System.Drawing.Point(8, 95);
            this.checkCulling.Name = "checkCulling";
            this.checkCulling.Size = new System.Drawing.Size(105, 17);
            this.checkCulling.TabIndex = 1;
            this.checkCulling.Text = "Backface culling";
            this.checkCulling.UseVisualStyleBackColor = true;
            this.checkCulling.CheckedChanged += new System.EventHandler(this.checkCulling_CheckedChanged);
            // 
            // checkDepth
            // 
            this.checkDepth.AutoSize = true;
            this.checkDepth.Location = new System.Drawing.Point(8, 72);
            this.checkDepth.Name = "checkDepth";
            this.checkDepth.Size = new System.Drawing.Size(85, 17);
            this.checkDepth.TabIndex = 0;
            this.checkDepth.Text = "Depth buffer";
            this.checkDepth.UseVisualStyleBackColor = true;
            this.checkDepth.CheckedChanged += new System.EventHandler(this.checkDepth_CheckedChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupOpenGL);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupClose2GL);
            this.splitContainer2.Size = new System.Drawing.Size(1504, 704);
            this.splitContainer2.SplitterDistance = 753;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // groupOpenGL
            // 
            this.groupOpenGL.AutoSize = true;
            this.groupOpenGL.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupOpenGL.Controls.Add(this.glControl1);
            this.groupOpenGL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupOpenGL.Location = new System.Drawing.Point(0, 0);
            this.groupOpenGL.Name = "groupOpenGL";
            this.groupOpenGL.Size = new System.Drawing.Size(753, 704);
            this.groupOpenGL.TabIndex = 1;
            this.groupOpenGL.TabStop = false;
            this.groupOpenGL.Text = "OpenGL";
            // 
            // glControl1
            // 
            this.glControl1.AutoSize = true;
            this.glControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(3, 16);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(747, 685);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControls_KeyDown);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // groupClose2GL
            // 
            this.groupClose2GL.AutoSize = true;
            this.groupClose2GL.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupClose2GL.Controls.Add(this.glControl2);
            this.groupClose2GL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupClose2GL.Location = new System.Drawing.Point(0, 0);
            this.groupClose2GL.Name = "groupClose2GL";
            this.groupClose2GL.Size = new System.Drawing.Size(747, 704);
            this.groupClose2GL.TabIndex = 1;
            this.groupClose2GL.TabStop = false;
            this.groupClose2GL.Text = "Close2GL";
            // 
            // glControl2
            // 
            this.glControl2.AutoSize = true;
            this.glControl2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.glControl2.BackColor = System.Drawing.Color.Black;
            this.glControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl2.Location = new System.Drawing.Point(3, 16);
            this.glControl2.Name = "glControl2";
            this.glControl2.Size = new System.Drawing.Size(741, 685);
            this.glControl2.TabIndex = 1;
            this.glControl2.VSync = false;
            this.glControl2.Load += new System.EventHandler(this.glControl2_Load);
            this.glControl2.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl2_Paint);
            this.glControl2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControls_KeyDown);
            this.glControl2.Resize += new System.EventHandler(this.glControl2_Resize);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "cow_up.in";
            this.openFileDialog1.Filter = "Mesh files|*.in|Text files|*.txt";
            this.openFileDialog1.Title = "Open mesh file";
            // 
            // groupCameraPosition
            // 
            this.groupCameraPosition.Controls.Add(this.cameraPosZ);
            this.groupCameraPosition.Controls.Add(this.labelCamPosZ);
            this.groupCameraPosition.Controls.Add(this.cameraPosY);
            this.groupCameraPosition.Controls.Add(this.labelCamPosY);
            this.groupCameraPosition.Controls.Add(this.cameraPosX);
            this.groupCameraPosition.Controls.Add(this.labelCamPosX);
            this.groupCameraPosition.Location = new System.Drawing.Point(182, 11);
            this.groupCameraPosition.Name = "groupCameraPosition";
            this.groupCameraPosition.Size = new System.Drawing.Size(103, 94);
            this.groupCameraPosition.TabIndex = 5;
            this.groupCameraPosition.TabStop = false;
            this.groupCameraPosition.Text = "Position";
            // 
            // cameraPosZ
            // 
            this.cameraPosZ.DecimalPlaces = 3;
            this.cameraPosZ.Location = new System.Drawing.Point(26, 68);
            this.cameraPosZ.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraPosZ.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraPosZ.Name = "cameraPosZ";
            this.cameraPosZ.Size = new System.Drawing.Size(70, 20);
            this.cameraPosZ.TabIndex = 11;
            this.cameraPosZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraPosZ.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.cameraPosZ.ValueChanged += new System.EventHandler(this.CameraPosition_Changed);
            // 
            // labelCamPosZ
            // 
            this.labelCamPosZ.AutoSize = true;
            this.labelCamPosZ.Location = new System.Drawing.Point(10, 71);
            this.labelCamPosZ.Name = "labelCamPosZ";
            this.labelCamPosZ.Size = new System.Drawing.Size(14, 13);
            this.labelCamPosZ.TabIndex = 10;
            this.labelCamPosZ.Text = "Z";
            // 
            // cameraPosY
            // 
            this.cameraPosY.DecimalPlaces = 3;
            this.cameraPosY.Location = new System.Drawing.Point(26, 44);
            this.cameraPosY.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraPosY.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraPosY.Name = "cameraPosY";
            this.cameraPosY.Size = new System.Drawing.Size(70, 20);
            this.cameraPosY.TabIndex = 9;
            this.cameraPosY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraPosY.ValueChanged += new System.EventHandler(this.CameraPosition_Changed);
            // 
            // labelCamPosY
            // 
            this.labelCamPosY.AutoSize = true;
            this.labelCamPosY.Location = new System.Drawing.Point(9, 47);
            this.labelCamPosY.Name = "labelCamPosY";
            this.labelCamPosY.Size = new System.Drawing.Size(14, 13);
            this.labelCamPosY.TabIndex = 8;
            this.labelCamPosY.Text = "Y";
            // 
            // cameraPosX
            // 
            this.cameraPosX.DecimalPlaces = 3;
            this.cameraPosX.Location = new System.Drawing.Point(26, 20);
            this.cameraPosX.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraPosX.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraPosX.Name = "cameraPosX";
            this.cameraPosX.Size = new System.Drawing.Size(70, 20);
            this.cameraPosX.TabIndex = 7;
            this.cameraPosX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraPosX.ValueChanged += new System.EventHandler(this.CameraPosition_Changed);
            // 
            // labelCamPosX
            // 
            this.labelCamPosX.AutoSize = true;
            this.labelCamPosX.Location = new System.Drawing.Point(9, 23);
            this.labelCamPosX.Name = "labelCamPosX";
            this.labelCamPosX.Size = new System.Drawing.Size(14, 13);
            this.labelCamPosX.TabIndex = 6;
            this.labelCamPosX.Text = "X";
            // 
            // groupCameraTarget
            // 
            this.groupCameraTarget.Controls.Add(this.cameraTargetZ);
            this.groupCameraTarget.Controls.Add(this.labelCamTargetZ);
            this.groupCameraTarget.Controls.Add(this.cameraTargetY);
            this.groupCameraTarget.Controls.Add(this.labelCamTargetY);
            this.groupCameraTarget.Controls.Add(this.cameraTargetX);
            this.groupCameraTarget.Controls.Add(this.labelCamTargetX);
            this.groupCameraTarget.Location = new System.Drawing.Point(290, 12);
            this.groupCameraTarget.Name = "groupCameraTarget";
            this.groupCameraTarget.Size = new System.Drawing.Size(103, 94);
            this.groupCameraTarget.TabIndex = 6;
            this.groupCameraTarget.TabStop = false;
            this.groupCameraTarget.Text = "Target";
            // 
            // cameraTargetZ
            // 
            this.cameraTargetZ.DecimalPlaces = 3;
            this.cameraTargetZ.Location = new System.Drawing.Point(26, 68);
            this.cameraTargetZ.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraTargetZ.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraTargetZ.Name = "cameraTargetZ";
            this.cameraTargetZ.Size = new System.Drawing.Size(70, 20);
            this.cameraTargetZ.TabIndex = 11;
            this.cameraTargetZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraTargetZ.ValueChanged += new System.EventHandler(this.CameraTarget_Changed);
            // 
            // labelCamTargetZ
            // 
            this.labelCamTargetZ.AutoSize = true;
            this.labelCamTargetZ.Location = new System.Drawing.Point(10, 71);
            this.labelCamTargetZ.Name = "labelCamTargetZ";
            this.labelCamTargetZ.Size = new System.Drawing.Size(14, 13);
            this.labelCamTargetZ.TabIndex = 10;
            this.labelCamTargetZ.Text = "Z";
            // 
            // cameraTargetY
            // 
            this.cameraTargetY.DecimalPlaces = 3;
            this.cameraTargetY.Location = new System.Drawing.Point(26, 44);
            this.cameraTargetY.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraTargetY.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraTargetY.Name = "cameraTargetY";
            this.cameraTargetY.Size = new System.Drawing.Size(70, 20);
            this.cameraTargetY.TabIndex = 9;
            this.cameraTargetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraTargetY.ValueChanged += new System.EventHandler(this.CameraTarget_Changed);
            // 
            // labelCamTargetY
            // 
            this.labelCamTargetY.AutoSize = true;
            this.labelCamTargetY.Location = new System.Drawing.Point(9, 47);
            this.labelCamTargetY.Name = "labelCamTargetY";
            this.labelCamTargetY.Size = new System.Drawing.Size(14, 13);
            this.labelCamTargetY.TabIndex = 8;
            this.labelCamTargetY.Text = "Y";
            // 
            // cameraTargetX
            // 
            this.cameraTargetX.DecimalPlaces = 3;
            this.cameraTargetX.Location = new System.Drawing.Point(26, 20);
            this.cameraTargetX.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraTargetX.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraTargetX.Name = "cameraTargetX";
            this.cameraTargetX.Size = new System.Drawing.Size(70, 20);
            this.cameraTargetX.TabIndex = 7;
            this.cameraTargetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraTargetX.ValueChanged += new System.EventHandler(this.CameraTarget_Changed);
            // 
            // labelCamTargetX
            // 
            this.labelCamTargetX.AutoSize = true;
            this.labelCamTargetX.Location = new System.Drawing.Point(9, 23);
            this.labelCamTargetX.Name = "labelCamTargetX";
            this.labelCamTargetX.Size = new System.Drawing.Size(14, 13);
            this.labelCamTargetX.TabIndex = 6;
            this.labelCamTargetX.Text = "X";
            // 
            // groupCameraUp
            // 
            this.groupCameraUp.Controls.Add(this.cameraUpZ);
            this.groupCameraUp.Controls.Add(this.labelCamUpZ);
            this.groupCameraUp.Controls.Add(this.cameraUpY);
            this.groupCameraUp.Controls.Add(this.labelCamUpY);
            this.groupCameraUp.Controls.Add(this.cameraUpX);
            this.groupCameraUp.Controls.Add(this.labelCamUpX);
            this.groupCameraUp.Location = new System.Drawing.Point(400, 13);
            this.groupCameraUp.Name = "groupCameraUp";
            this.groupCameraUp.Size = new System.Drawing.Size(103, 94);
            this.groupCameraUp.TabIndex = 7;
            this.groupCameraUp.TabStop = false;
            this.groupCameraUp.Text = "Up";
            // 
            // cameraUpZ
            // 
            this.cameraUpZ.DecimalPlaces = 3;
            this.cameraUpZ.Location = new System.Drawing.Point(26, 68);
            this.cameraUpZ.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraUpZ.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraUpZ.Name = "cameraUpZ";
            this.cameraUpZ.Size = new System.Drawing.Size(70, 20);
            this.cameraUpZ.TabIndex = 11;
            this.cameraUpZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraUpZ.ValueChanged += new System.EventHandler(this.CameraUp_Changed);
            // 
            // labelCamUpZ
            // 
            this.labelCamUpZ.AutoSize = true;
            this.labelCamUpZ.Location = new System.Drawing.Point(10, 71);
            this.labelCamUpZ.Name = "labelCamUpZ";
            this.labelCamUpZ.Size = new System.Drawing.Size(14, 13);
            this.labelCamUpZ.TabIndex = 10;
            this.labelCamUpZ.Text = "Z";
            // 
            // cameraUpY
            // 
            this.cameraUpY.DecimalPlaces = 3;
            this.cameraUpY.Location = new System.Drawing.Point(26, 44);
            this.cameraUpY.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraUpY.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraUpY.Name = "cameraUpY";
            this.cameraUpY.Size = new System.Drawing.Size(70, 20);
            this.cameraUpY.TabIndex = 9;
            this.cameraUpY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraUpY.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cameraUpY.ValueChanged += new System.EventHandler(this.CameraUp_Changed);
            // 
            // labelCamUpY
            // 
            this.labelCamUpY.AutoSize = true;
            this.labelCamUpY.Location = new System.Drawing.Point(9, 47);
            this.labelCamUpY.Name = "labelCamUpY";
            this.labelCamUpY.Size = new System.Drawing.Size(14, 13);
            this.labelCamUpY.TabIndex = 8;
            this.labelCamUpY.Text = "Y";
            // 
            // cameraUpX
            // 
            this.cameraUpX.DecimalPlaces = 3;
            this.cameraUpX.Location = new System.Drawing.Point(26, 20);
            this.cameraUpX.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.cameraUpX.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
            this.cameraUpX.Name = "cameraUpX";
            this.cameraUpX.Size = new System.Drawing.Size(70, 20);
            this.cameraUpX.TabIndex = 7;
            this.cameraUpX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraUpX.ValueChanged += new System.EventHandler(this.CameraUp_Changed);
            // 
            // labelCamUpX
            // 
            this.labelCamUpX.AutoSize = true;
            this.labelCamUpX.Location = new System.Drawing.Point(9, 23);
            this.labelCamUpX.Name = "labelCamUpX";
            this.labelCamUpX.Size = new System.Drawing.Size(14, 13);
            this.labelCamUpX.TabIndex = 6;
            this.labelCamUpX.Text = "X";
            // 
            // groupMesh
            // 
            this.groupMesh.Controls.Add(this.buttonLoadMesh);
            this.groupMesh.Controls.Add(this.groupMeshColor);
            this.groupMesh.Location = new System.Drawing.Point(861, 16);
            this.groupMesh.Name = "groupMesh";
            this.groupMesh.Size = new System.Drawing.Size(633, 168);
            this.groupMesh.TabIndex = 9;
            this.groupMesh.TabStop = false;
            this.groupMesh.Text = "Mesh";
            // 
            // groupMeshColor
            // 
            this.groupMeshColor.Controls.Add(this.meshColorB);
            this.groupMeshColor.Controls.Add(this.labelMeshColorB);
            this.groupMeshColor.Controls.Add(this.meshColorG);
            this.groupMeshColor.Controls.Add(this.labelMeshColorG);
            this.groupMeshColor.Controls.Add(this.meshColorR);
            this.groupMeshColor.Controls.Add(this.labelMeshColorR);
            this.groupMeshColor.Location = new System.Drawing.Point(12, 64);
            this.groupMeshColor.Name = "groupMeshColor";
            this.groupMeshColor.Size = new System.Drawing.Size(88, 88);
            this.groupMeshColor.TabIndex = 8;
            this.groupMeshColor.TabStop = false;
            this.groupMeshColor.Text = "Color";
            // 
            // meshColorB
            // 
            this.meshColorB.DecimalPlaces = 3;
            this.meshColorB.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.meshColorB.Location = new System.Drawing.Point(24, 64);
            this.meshColorB.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.meshColorB.Name = "meshColorB";
            this.meshColorB.Size = new System.Drawing.Size(54, 20);
            this.meshColorB.TabIndex = 11;
            this.meshColorB.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.meshColorB.ValueChanged += new System.EventHandler(this.meshColor_Changed);
            // 
            // labelMeshColorB
            // 
            this.labelMeshColorB.AutoSize = true;
            this.labelMeshColorB.Location = new System.Drawing.Point(7, 67);
            this.labelMeshColorB.Name = "labelMeshColorB";
            this.labelMeshColorB.Size = new System.Drawing.Size(14, 13);
            this.labelMeshColorB.TabIndex = 10;
            this.labelMeshColorB.Text = "B";
            // 
            // meshColorG
            // 
            this.meshColorG.DecimalPlaces = 3;
            this.meshColorG.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.meshColorG.Location = new System.Drawing.Point(24, 40);
            this.meshColorG.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.meshColorG.Name = "meshColorG";
            this.meshColorG.Size = new System.Drawing.Size(54, 20);
            this.meshColorG.TabIndex = 9;
            this.meshColorG.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.meshColorG.ValueChanged += new System.EventHandler(this.meshColor_Changed);
            // 
            // labelMeshColorG
            // 
            this.labelMeshColorG.AutoSize = true;
            this.labelMeshColorG.Location = new System.Drawing.Point(6, 44);
            this.labelMeshColorG.Name = "labelMeshColorG";
            this.labelMeshColorG.Size = new System.Drawing.Size(15, 13);
            this.labelMeshColorG.TabIndex = 8;
            this.labelMeshColorG.Text = "G";
            // 
            // meshColorR
            // 
            this.meshColorR.DecimalPlaces = 3;
            this.meshColorR.Increment = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.meshColorR.Location = new System.Drawing.Point(24, 16);
            this.meshColorR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.meshColorR.Name = "meshColorR";
            this.meshColorR.Size = new System.Drawing.Size(54, 20);
            this.meshColorR.TabIndex = 7;
            this.meshColorR.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.meshColorR.ValueChanged += new System.EventHandler(this.meshColor_Changed);
            // 
            // labelMeshColorR
            // 
            this.labelMeshColorR.AutoSize = true;
            this.labelMeshColorR.Location = new System.Drawing.Point(6, 19);
            this.labelMeshColorR.Name = "labelMeshColorR";
            this.labelMeshColorR.Size = new System.Drawing.Size(15, 13);
            this.labelMeshColorR.TabIndex = 6;
            this.labelMeshColorR.Text = "R";
            // 
            // buttonLoadMesh
            // 
            this.buttonLoadMesh.Location = new System.Drawing.Point(12, 22);
            this.buttonLoadMesh.Name = "buttonLoadMesh";
            this.buttonLoadMesh.Size = new System.Drawing.Size(87, 28);
            this.buttonLoadMesh.TabIndex = 9;
            this.buttonLoadMesh.Text = "Load mesh";
            this.buttonLoadMesh.UseVisualStyleBackColor = true;
            this.buttonLoadMesh.Click += new System.EventHandler(this.buttonLoadMesh_Click);
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(1504, 899);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainWindow";
            this.Text = "INF01009 - Computer Graphics - Khin Baptista, 217443";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupControls.ResumeLayout(false);
            this.groupControls.PerformLayout();
            this.groupProj.ResumeLayout(false);
            this.groupProj.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projFar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projNear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projLeft)).EndInit();
            this.groupLightColor.ResumeLayout(false);
            this.groupLightColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightColorR)).EndInit();
            this.groupLightPos.ResumeLayout(false);
            this.groupLightPos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLightPosX)).EndInit();
            this.groupCamera.ResumeLayout(false);
            this.groupCamera.PerformLayout();
            this.groupPivot.ResumeLayout(false);
            this.groupPivot.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraSpeed)).EndInit();
            this.groupRendermode.ResumeLayout(false);
            this.groupRendermode.PerformLayout();
            this.groupFace.ResumeLayout(false);
            this.groupFace.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupOpenGL.ResumeLayout(false);
            this.groupOpenGL.PerformLayout();
            this.groupClose2GL.ResumeLayout(false);
            this.groupClose2GL.PerformLayout();
            this.groupCameraPosition.ResumeLayout(false);
            this.groupCameraPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPosX)).EndInit();
            this.groupCameraTarget.ResumeLayout(false);
            this.groupCameraTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraTargetX)).EndInit();
            this.groupCameraUp.ResumeLayout(false);
            this.groupCameraUp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cameraUpX)).EndInit();
            this.groupMesh.ResumeLayout(false);
            this.groupMeshColor.ResumeLayout(false);
            this.groupMeshColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.meshColorR)).EndInit();
            this.ResumeLayout(false);

        }

    }
}
