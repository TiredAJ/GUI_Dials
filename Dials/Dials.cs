using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Forms.VisualStyles;

//to-do:
//cut down on member variables for each dial - Inheritance?
//navball-like dial?
//mid-start radial dials?

namespace Dials
{
    public class RadialDial
    {
        private Graphics G;
        private bool GAssigned = false;

        public Color LineColour;
        public Color BackgroundColour;
        public Color BorderColour;
        public float LineWidth;
        public float Angle;
        public float MaxVal;
        public DialShape DType;
        public DialOrientation DOrientation;
        public Rectangle Region;

        //constructor
        public RadialDial
        (
            Rectangle _Region, DialShape _DShape, DialOrientation _DOrientation,
            Color _LineColour, Color _BackgroundColour, Color _BorderColour, 
            float _LineWidth, float _MaxValue
        )
        {
            LineColour = _LineColour;
            BackgroundColour = _BackgroundColour;
            BorderColour = _BorderColour;
            LineWidth = _LineWidth;
            DType = _DShape;
            DOrientation = _DOrientation;
            Region = _Region;
            MaxVal = _MaxValue;

            //reduces edge-clipping with the border
            Region.Width -= (int)LineWidth;
            Region.Y += (int)LineWidth/2;
            Region.X += (int)LineWidth/2;
            Region.Height -= (int)LineWidth;

            //shenanigans for reducing wasted space
            //because the pie draws with space for a full ellipse, semi and
            //quarter dials leave a lot of empty space
            if (DType == DialShape.Quarter)
            {
                Region.Width *= 2;
                Region.Height *= 2;

                switch ((float)DOrientation)
                {
                    case 0:
                    {//moves dial from bottom-right to the visible region
                        Region.X -= Region.Width / 2;
                        Region.Y -= Region.Height / 2;
                        break;
                    }
                    case 90:
                    {//moves dial up to the visible region
                        Region.Y -= Region.Height / 2;
                        break;
                    }
                    case 270:
                    {//moves dial left to the visible region
                        Region.X -= Region.Width / 2;
                        break;
                    }
                }
            }
            else if (DType == DialShape.Semi)
            {
                //ensures the /working/ region is a square
                if (Region.Width > Region.Height)
                {Region.Height = Region.Width - (int)LineWidth;}
                else if (Region.Height > Region.Width)
                {Region.Width = Region.Height - (int)LineWidth;}
                
                //moves the dial either up or to the left so it's
                //in the visible region
                if (DOrientation == DialOrientation.NorthEast)
                {Region.X -= (Region.Width / 2) +1;}
                else if (DOrientation == DialOrientation.SouthEast)
                {Region.Y -= (Region.Height / 2) +1;}
            }
        }

        public void InitGraphics(PaintEventArgs e)
        {//passing the graphics object
            G = e.Graphics;

            //allows the object to be drawn
            GAssigned = true;

            //reduces aliasing
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        private void DrawPrep()
        {
            //checks the graphics object's been passed in
            if (!GAssigned)
            {throw new Exception("Please initialise graphics with .InitGraphics()");}

            //makes sure the input is within bounds
            if (Angle > (float)DType)
            {Angle = (float)DType;}
            else if (Angle < 0)
            {Angle = 0f;}

            //draws background
            G.FillPie
            (
                new SolidBrush(BackgroundColour), Region,
                (float)DOrientation, (float)DType
            );
        }

        public void Draw()
        {
            DrawPrep();

            //draws the needle
            G.DrawPie
            (
                new Pen(LineColour, LineWidth),
                Region,
                (float)DOrientation,
                Angle
            );
            
            //border
            G.DrawPie
            (
                new Pen(BorderColour, LineWidth),
                Region,
                (float)DOrientation,
                (float)DType
            );
        }

        public void FDraw()
        {
            DrawPrep();

            G.FillPie
            (
                new SolidBrush(LineColour),
                Region,
                (float)DOrientation,
                Angle
            );

            //border
            G.DrawPie
            (
                new Pen(BorderColour, LineWidth),
                Region,
                (float)DOrientation,
                (float)DType
            );
        }

        public void UpdateVal(float NewVal)
        {//calculates the angle from the inputted val
            Angle = (NewVal / MaxVal) * (float)DType;
        }

    }

    public class Tiltmeter
    {
        private Graphics G;
        private Point Left, Right;
        private float Angle;
        private bool GAssigned = false;

        public Rectangle Region = new Rectangle();
        public float LineWidth;
        public Color LineColour, BackgroundColour;
        public float CurrentVal = 0;
        public float MaxVal;

        //constructor
        public Tiltmeter
        (
            Rectangle _Region, Color _BackgroundColour, Color _LineColour,
            float _LineWidth, float _MaxVal
        )
        {
            Region = _Region;
            LineColour = _LineColour;
            BackgroundColour = _BackgroundColour;
            MaxVal = _MaxVal;
            LineWidth = _LineWidth;

            //ensures working region is a square
            if (Region.Width != Region.Height)
            {Region.Height = Region.Width;}

            CalculatePoints(CurrentVal);
        }

        public void InitGraphics(PaintEventArgs e)
        {//passing the graphics object
            G = e.Graphics;

            //allows the object to be drawn
            GAssigned = true;

            //reduces aliasing
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        public void UpdateVal(float NewVal) 
        {//calls for re-calculation on a new inputted value
            CalculatePoints(NewVal);
        }

        private void CalculatePoints(float Value)
        {//calculates the position of the needle's ends
            CurrentVal = -Value;

            //makes the angle relative (proportional) to the boundaries
            Angle = (CurrentVal / MaxVal) * 90;

            //calculates the right end of the needle w/ trig functions
            Right = new Point
            (
                (int)(Math.Cos((Math.PI / 180) * Angle) * Region.Width / 2) + Region.Width / 2,
                (int)(Math.Sin((Math.PI / 180) * Angle) * Region.Width / 2) + Region.Height / 2
            );

            //calculates the left end of the needle w/ trig functions
            Left = new Point
            (
                (int)(Math.Cos((Math.PI / 180) * (Angle + 180)) * Region.Width / 2) + Region.Width / 2,
                (int)(Math.Sin((Math.PI / 180) * (Angle + 180)) * Region.Width / 2) + Region.Height / 2
            );
        }

        public void Draw()
        {
            //checks the graphics object's been passed in
            if (!GAssigned)
            {throw new Exception("Please initialise graphics with .InitGraphics()");}

            //draws a line between the calculated points
            G.DrawLine(new Pen(LineColour, LineWidth), Left, Right);
        }
    }

    public class BarChart
    {
        private Graphics G;
        private bool GAssigned = false;

        public float MaxVal, MinVal, CurrentVal = 0, Height;
        public Rectangle Region, InnerRegion;
        public Color BackgroundColour, FillColour;

        //constructor
        public BarChart
        (
            Rectangle _Region, Color _FillColour, Color _BackgroundColour,
            float _MaxVal, float _MinVal
        )
        {
            MaxVal = _MaxVal;
            MinVal = _MinVal;
            Region = _Region;
            FillColour = _FillColour;

            //sets up the inner region (the actual bar)
            InnerRegion.Height = 0;
            InnerRegion.Width = _Region.Width;
            InnerRegion.X = 0;
            InnerRegion.Y = _Region.Height;

            Height = Region.Height;
            BackgroundColour = _BackgroundColour;

            //default value
            UpdateVal(0);
        }

        public void InitGraphics(PaintEventArgs e)
        {//passing the graphics object
            G = e.Graphics;

            //allows the object to be drawn
            GAssigned = true;

            //reduces aliasing
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        public void UpdateVal(float _Val)
        {//new inputted value
            CurrentVal = _Val;

            //calculates the new height for the inner region (bar)
            Height = (CurrentVal / MaxVal) * Region.Height;
        }

        public void Draw() 
        {
            //checks the graphics object's been passed in
            if (!GAssigned)
            {throw new Exception("Please initialise graphics with .InitGraphics()");}

            //draws the background
            G.FillRectangle(new SolidBrush(BackgroundColour), Region);

            //draws the bar
            //a little cheaty, but I'll fix it. It currently draws a rectangle
            //of the same size as the background, but moves it up and down - not happy
            G.FillRectangle
            (
                new SolidBrush(FillColour), new Rectangle
                (
                    InnerRegion.X, (Region.Height - (int)Height),
                    InnerRegion.Width, Region.Height
                )
            );
        }
    }

    //enum for the dial shape-types
    public enum DialShape
    {
        Quarter = 90,
        Semi = 180,
        ThreeQuarter = 270
    }

    //enum for the dial orientations
    public enum DialOrientation
    {
        SouthEast = 0,
        SouthWest = 90,
        Default   = 180,
        NorthWest = 180,
        NorthEast = 270
    }
}