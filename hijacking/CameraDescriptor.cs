using Silk.NET.Maths;
namespace hijacking
{
    internal class CameraDescriptor
    {
        private const double AngleChangeStepSize = Math.PI / 180 * 5;
        private float MoveSpeed = 0.9f;
        private float TurningSpeed = 0.9f;
        private static readonly Vector3D<float> PointOfView = new Vector3D<float>(0f, 915f, 2747f);
        private static readonly Vector3D<float> landingVector3D = new Vector3D<float>(0f, 0.5f, 0f);
        
        private bool landing = false;
        private bool colidingWithRoad = false;
        private bool colidingWithFighterJets = false;
        // TPV :
        public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0.0f, 1000.0f, 2000.0f);
        // FPV:
        // public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0.0f, 29f, -747f);

        // repulo elore megy z -
        //repulo hata megy z +
        //repulo balra megy x -
        //repulo jobbra megy x +
        
        public Vector3D<float> UpVector { get; private set; } = new Vector3D<float>(0, 1, 0);

        public Vector3D<float> Target { get; private set; } = new Vector3D<float>(0.0f, 0.0f, -1600.0f);

        public Vector3D<float> ForwardVector { get; private set; } = new Vector3D<float>(0,0,-1);

        public void MoveForward()
        {
            if (colidingWithFighterJets)
            {
                return;
            }

            if (colidingWithRoad)
            {
                MoveSpeed *= 0.9981f;
                var direction = ForwardVector * MoveSpeed;
                Position += direction;
                Target += direction;
                return;
            }
            else
            {
                var direction = ForwardVector * MoveSpeed;
                Position += direction;
                Target += direction;
                if (landing)
                {
                    Position -= landingVector3D;
                    Target -= landingVector3D;
                }    
            }
            
        }

        public void MoveRight()
        {
            if (colidingWithRoad || colidingWithFighterJets)
            {
                return;
            }
            Position = new Vector3D<float>(Position.X + TurningSpeed, Position.Y, Position.Z);
            Target = new Vector3D<float>(Target.X + TurningSpeed, Target.Y, Target.Z);
        }

        public void MoveLeft()
        {
            if (colidingWithRoad || colidingWithFighterJets)
            {
                return;
            }
            Position = new Vector3D<float>(Position.X - TurningSpeed, Position.Y, Position.Z);
            Target = new Vector3D<float>(Target.X - TurningSpeed, Target.Y, Target.Z);
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
            var forward = Target - Position;
            var rotate = Matrix4X4.CreateFromAxisAngle(UpVector, angle);
            forward = Vector3D.Transform(forward, rotate);
            Target = Position + forward;
        }
        
        public void RotateUp()
        {
            RotateAroundRightVector((float)AngleChangeStepSize);
        }

        public void RotateDown()
        {
            RotateAroundRightVector(-(float)AngleChangeStepSize);
        }

        private void RotateAroundRightVector(float angle)
        {
            var right = Vector3D.Cross(ForwardVector, UpVector);
            right = Vector3D.Normalize(right);
            var rotate = Matrix4X4.CreateFromAxisAngle(right, angle);

            var forward = Target - Position;

            forward = Vector3D.Transform(forward, rotate);

            Target = Position + forward;
        }

        public void SetTPV()
        {
            Position += PointOfView;
        }
        
        public void SetFPV()
        {
            Position -= PointOfView;
        }
        
        public void SetLanding()
        {
           landing = true;
        }
        
        public void SetColidingWithRoad()
        {
            colidingWithRoad = true;
        }
        
        public void SetColidingWithFighterJets()
        {
            colidingWithFighterJets = true;
        }
        
        public void setMovementSpeed(float speed)
        {
            MoveSpeed = speed;
        }
        
        public void setTurningSpeed(float speed)
        {
            TurningSpeed = speed;
        }
    }
}
