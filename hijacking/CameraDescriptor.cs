using Silk.NET.Maths;
namespace hijacking
{
    internal class CameraDescriptor
    {
        private const double AngleChangeStepSize = Math.PI / 180 * 5;
        private const float MoveSpeed = 0.9f;

        public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0.0f, 400.0f, 400.0f);

        public Vector3D<float> UpVector { get; private set; } = new Vector3D<float>(0, 1, 0);

        public Vector3D<float> Target { get; private set; } = new Vector3D<float>(0.0f, 0.0f, -1600.0f);

        public Vector3D<float> ForwardVector { get; private set; } = new Vector3D<float>(0,0,-1);

        public void MoveForward()
        {
            var direction = ForwardVector * MoveSpeed;
            Position += direction;
            Target += direction;
        }

        public void MoveBackward()
        {
            var direction = ForwardVector * MoveSpeed;
            Position -= direction;
            Target -= direction;
        }

        public void MoveRight()
        {
            //var right = Vector3D.Cross(UpVector, ForwardVector);
            //right = Vector3D.Normalize(right) * MoveSpeed;
            Position = new Vector3D<float>(Position.X + 10f, Position.Y, Position.Z);
            Target = new Vector3D<float>(Target.X + 10f, Target.Y, Target.Z);
        }

        public void MoveLeft()
        {
            Position = new Vector3D<float>(Position.X - 10f, Position.Y, Position.Z);
            Target = new Vector3D<float>(Target.X - 10f, Target.Y, Target.Z);
        }

        public void RotateLeft()
        {
            RotateAroundUpVector((float)AngleChangeStepSize);
        }

        public void RotateRight()
        {
            RotateAroundUpVector(-(float)AngleChangeStepSize);
        }

        private void RotateAroundUpVector(float angle)
        {
            var rotate = Matrix4X4.CreateFromAxisAngle(UpVector, angle);
            var forward = ForwardVector;
            forward = Vector3D.Transform(forward, rotate);
            Target = Position + forward;
        }

        public void RotateUp()
        {
            RotateAroundRightVector(-(float)AngleChangeStepSize);
        }

        public void RotateDown()
        {
            RotateAroundRightVector((float)AngleChangeStepSize);
        }

        private void RotateAroundRightVector(float angle)
        {
            var right = Vector3D.Cross(ForwardVector, UpVector);
            right = Vector3D.Normalize(right);
            var rotate = Matrix4X4.CreateFromAxisAngle(right, angle);
            var forward = ForwardVector;
            forward = Vector3D.Transform(forward, rotate);
            Target = Position + forward;

            // Update the UpVector to ensure it remains orthogonal to the ForwardVector
            UpVector = Vector3D.Normalize(Vector3D.Cross(right, forward));
        }
    }
}
