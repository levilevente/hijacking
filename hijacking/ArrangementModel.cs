using Silk.NET.Maths;

namespace hijacking
{
    internal class ArrangementModel
    {
        private static float leftBound = -100f;
        private static float rightBound = 100f;

        private static float movementSpeed = 0.9f;

        public Vector3D<float> airplaneTranslation;
        public Vector3D<float>[] aircraftPosition;
        
        public ArrangementModel()
        {
            airplaneTranslation = new Vector3D<float>(0, 0, 0);
            aircraftPosition = new Vector3D<float>[4];
            
            for (int i = 0; i < aircraftPosition.Length; i++)
            {
                aircraftPosition[i] = new Vector3D<float>(0, 0, 0);
            }

            aircraftPosition[0] = new Vector3D<float>(100, 100, 100);
            aircraftPosition[1] = new Vector3D<float>(-100, 100, 100);
            aircraftPosition[2] = new Vector3D<float>(100, -100, 100);
            aircraftPosition[3] = new Vector3D<float>(-100, -100, 100);
        }

        public void AdvanceTime()
        {
            airplaneTranslation.Z -= movementSpeed;
            if (airplaneTranslation.Z > 10000)
            {
                airplaneTranslation.Z = -10000;
            }
        }
        
        public void TurnLeft()
        {
            airplaneTranslation.X -= 10.0f;
            if (airplaneTranslation.X < -10000)
            {
                airplaneTranslation.X = 10000;
            }
        }
        
        public void TurnRight()
        {
            airplaneTranslation.X += 10.0f;
            if (airplaneTranslation.X > 10000)
            {
                airplaneTranslation.X = -10000;
            }
        }
        
    }
}
