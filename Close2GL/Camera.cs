using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;

namespace Close2GL
{
    class Camera
    {
        public bool TargetLock;

        private Vector3 position;
        private Vector3 target;
        private Vector3 up;
        private Quaternion rotation;

        public Vector3 Position {
            get { return position; }
            set {
                if (!TargetLock)
                    target += value - position;
                position = value;
            }
        }

        public Vector3 Target {
            get { return target; }
            set { target = value; }
        }

        public Vector3 Up {
            get { return up; }// Vector3.Cross(Direction, Left); }
            set { up = value; }
        }

        public Vector3 Direction {
            get { return (target - position).Normalized(); }
            set {
                float length = (target - position).Length;
                target = position + value.Normalized() * length;
            }
        }

        public Vector3 Left {
            get { return Vector3.Cross(up, Direction); }
        }

        public Quaternion Rotation {
            get { return rotation; }
            set {
                rotation = value.Normalized();
                Direction = Vector3.TransformVector(Direction, Matrix4.CreateFromQuaternion(rotation));
            }
        }

        public Matrix4 ViewMatrix {
            get { return Matrix4.LookAt(position, target, Up); }
        }

        public Camera() {
            position = Vector3.Zero;
            target = -Vector3.UnitZ;
            up = Vector3.UnitY;

            rotation = Quaternion.Identity;
            TargetLock = false;
        }

        public void MoveForward(float distance) {
            if (!TargetLock)
                target += distance * Direction;
            position += distance * Direction;
        }

        public void MoveBackward(float distance) {
            MoveForward(-distance);
        }

        public void MoveLeft(float distance) {
            if (!TargetLock)
                target += distance * Left;
            position += distance * Left;
        }

        public void MoveRight(float distance) {
            MoveLeft(-distance);
        }

        public void MoveUp(float distance) {
            Vector3 localUp = Vector3.Cross(Direction, Left);

            if (!TargetLock)
                target += distance * localUp;
            position += distance * localUp;
        }

        public void MoveDown(float distance) {
            MoveUp(-distance);
        }
    }
}
