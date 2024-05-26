using Silk.NET.Maths;

namespace hijacking
{
    internal class ArrangementModel
    {
        private static float movementSpeed = 0.9f;
        private static float planeSpeed = 0.9f;

        public Vector3D<float> airplaneTranslation;
        public Vector3D<float>[] aircraftPosition;
        public Vector3D<float> roadPosition;

        private bool land = false;

        private Random r;

        private bool colidingWithRoad = false;
        private int colidingWithFighterJets = -1;
        
        public ArrangementModel()
        {
            airplaneTranslation = new Vector3D<float>(0, 0, 0);
            r = new Random();
            int r_x = r.Next(-3000, 3000);
            roadPosition = new Vector3D<float>(r_x, -1000, -18000);
            aircraftPosition = new Vector3D<float>[4];
            
            aircraftPosition[0] = new Vector3D<float>(700, 0, 700);
            aircraftPosition[1] = new Vector3D<float>(-700, 0, 700);
            aircraftPosition[2] = new Vector3D<float>(-700, 0, -700);
            aircraftPosition[3] = new Vector3D<float>(700, 0, -700);
        }

        public void AdvanceTime()
        {
            if (colidingWithFighterJets != -1)
            {
                airplaneTranslation.Y -= movementSpeed;
                aircraftPosition[colidingWithFighterJets].Y -= movementSpeed;
                
                for (int i = 0; i < aircraftPosition.Length; i++)
                {
                    if (i != colidingWithFighterJets)
                    {
                        aircraftPosition[i].Z -= movementSpeed;
                    }
                }
                return;
            }

            if (colidingWithRoad)
            {
                moveFighterJets();
                airplaneTranslation.Z -= planeSpeed;
                planeSpeed *= 0.9991f;
                return;
            }
            
            airplaneTranslation.Z -= planeSpeed;
            
            moveFighterJets();
            
            if (land)
            {
                airplaneTranslation.Y -= 0.5f;
            }
        }
        
        public void TurnLeft()
        {
            if (colidingWithFighterJets != -1)
            {
                return;
            }
            airplaneTranslation.X -= 10.0f;
            if (airplaneTranslation.X < -10000)
            {
                airplaneTranslation.X = 10000;
            }
        }
        
        public void TurnRight()
        {
            if (colidingWithFighterJets != -1)
            {
                return;
            }
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
        
        public void setColidingWithFighterJet(int numberOfJet)
        {
            this.colidingWithFighterJets = numberOfJet;
        }

        public int getColifingWithFighterJet()
        {
            return this.colidingWithFighterJets;
        }
        
        public void setColidingWithRoad()
        {
            this.colidingWithRoad = true;
        }
        
        public bool getColidingWithRoad()
        {
            return this.colidingWithRoad;
        }
        
        private void moveFighterJets()
        {
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
        }
    }
}
