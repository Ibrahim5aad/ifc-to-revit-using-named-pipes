using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCtoRevit.IFCLoader
{
    [Serializable()]
    public class Beam
    {

        public string Name { get; set; }
        public double H { get; set; }
        public double B { get; set; }
        public double Length { get; set; }
        public Point Location { get; set; }
        public Point RefDirection { get; set; }
        public Point Axis { get; set; }
    }

}
