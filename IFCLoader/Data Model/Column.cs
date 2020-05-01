using System;

namespace IFCtoRevit.IFCLoader
{
    [Serializable()]
    public class Column
    {

        public Point Location { get; set; }

        public string Name { get; set; }

        public double BottomLevel { get; set; }

        public double TopLevel { get; set; }

        public double Width { get; set; }

        public double Depth { get; set; }

        public Point RefDirection { get; set; }


    }

}
