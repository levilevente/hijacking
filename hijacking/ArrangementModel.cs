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
        public Vector3D<float> roadPosition;

        private bool land = false;

        private Random r;
        
        public ArrangementModel()
        {
            airplaneTranslation = new Vector3D<float>(0, 0, 0);
            r = new Random();
            int r_z = r.Next(-3000, 3000);
            roadPosition = new Vector3D<float>(r_z, -1000, -18000);
            aircraftPosition = new Vector3D<float>[4];


            aircraftPosition[0] = new Vector3D<float>(700, 0, 700);
            aircraftPosition[1] = new Vector3D<float>(-700, 0, 700);
            aircraftPosition[2] = new Vector3D<float>(-700, 0, -700);
            aircraftPosition[3] = new Vector3D<float>(700, 0, -700);
        }

        public void AdvanceTime()
        {
            airplaneTranslation.Z -= movementSpeed;
            
            for (int i = 0; i < aircraftPosition.Length; i++)
            {
                aircraftPosition[i].Z -= movementSpeed;
            }

            if (aircraftPosition[0].X < roadPosition.X + 700)
            {
                aircraftPosition[0].X += 0.4f;
            }
            else if (aircraftPosition[0].X > roadPosition.X + 700)
            {
                aircraftPosition[0].X -= 0.4f;
            }
            
            if (aircraftPosition[1].X < roadPosition.X - 700)
            {
                aircraftPosition[1].X += 0.4f;
            }
            else if (aircraftPosition[1].X > roadPosition.X - 700)
            {
                aircraftPosition[1].X -= 0.4f;
            }
            
            if (aircraftPosition[2].X < roadPosition.X - 700)
            {
                aircraftPosition[2].X += 0.4f;
            }
            else if (aircraftPosition[2].X > roadPosition.X - 700)
            {
                aircraftPosition[2].X -= 0.4f;
            }
            
            if (aircraftPosition[3].X < roadPosition.X + 700)
            {
                aircraftPosition[3].X += 0.4f;
            }
            else if (aircraftPosition[3].X > roadPosition.X + 700)
            {
                aircraftPosition[3].X -= 0.4f;
            }
            
            if (land)
            {
                airplaneTranslation.Y -= 0.5f;
            }
            
            // if the airpalne position hit the road rane we stop the airplane
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
        
        public void setLand()
        {
            this.land = true;
        }
        
    }
}
