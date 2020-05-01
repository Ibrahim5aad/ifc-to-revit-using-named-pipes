using System;

namespace IFCtoRevit.IFCLoader
{
    [Serializable()]
    public class Inclined
    {
        public Point Location { get; set; }
        public Point RefDirection { get; set; }
        public Point Axis { get; set; }
        public double Length { get; set; }
        public string Name { get; set; }
        public double BottomLevel { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }

        //  public double TopLevel { get; set; }
    }
}
