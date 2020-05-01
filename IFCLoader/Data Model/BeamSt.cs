using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCtoRevit.IFCLoader
{
    [Serializable()]
    public class BeamSt
    {

        public Point Location { get; set; }
        public Point RefDirection { get; set; }
        public Point Axis { get; set; }
        public string Name { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double FlangeTh { get; set; }
        public double WebTh { get; set; }
    }

}
