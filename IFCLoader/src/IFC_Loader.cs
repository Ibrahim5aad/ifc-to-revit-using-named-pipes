using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;


namespace IFCtoRevit.IFCLoader
{
    public static class IFC_Loader
    {
        static IfcStore _ifcModel;

        static List<IIfcColumn> _columns;
        static List<IIfcMemberStandardCase> _inclines;   
        static List<IIfcBeam> _beams;
        static List<IIfcSlab> _floors;
        static List<IIfcBuildingStorey> _bStoreys;

        static List<IIfcColumn> _columnsSt;
        static List<IIfcMemberStandardCase> _braces;   
        static List<IIfcBeam> _beamsSt;

        public static bool LoadFile(string filePath, string fileName)
        {
            _ifcModel = IfcStore.Open($"{filePath}{fileName}.ifc");
            if (_ifcModel != null) return true; else return false;
        }

        public static bool LoadFile(string fileLocation)
        {
            _ifcModel = IfcStore.Open($"{fileLocation}");
            if (_ifcModel != null) return true; else return false;
        }

        public static void GetIFCElements()
        {
            _bStoreys =    _ifcModel.Instances.OfType<IIfcBuildingStorey>().ToList();

            _floors =      _ifcModel.Instances.OfType<IIfcSlab>().ToList();

            _columns = _ifcModel.Instances.OfType<IIfcColumn>().Where(col => {
                            if ((col.Representation.Representations.FirstOrDefault()
                            .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                            .SweptArea is IIfcRectangleProfileDef) return true; else return false; }).ToList();


            _columnsSt = _ifcModel.Instances.OfType<IIfcColumn>().Where(col => {
                            if ((col.Representation.Representations.FirstOrDefault()
                            .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                            .SweptArea is IIfcIShapeProfileDef) return true; else return false; }).ToList();


            _beams = _ifcModel.Instances.OfType<IIfcBeam>().Where(beam => {
                            if ((beam.Representation.Representations.FirstOrDefault()
                            .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                            .SweptArea is IIfcRectangleProfileDef) return true; else return false; }).ToList();

            _beamsSt = _ifcModel.Instances.OfType<IIfcBeam>().Where(beam => {
                            if ((beam.Representation.Representations.FirstOrDefault()
                            .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                            .SweptArea is IIfcIShapeProfileDef) return true; else return false;}).ToList();

            _inclines = _ifcModel.Instances.OfType<IIfcMemberStandardCase>().Where(inc => {
                        if ((inc.Representation.Representations.FirstOrDefault()
                        .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea is IIfcRectangleProfileDef) return true; else return false; }).ToList();


            _braces = _ifcModel.Instances.OfType<IIfcMemberStandardCase>().Where(brace => {
                        if ((brace.Representation.Representations.FirstOrDefault()
                        .Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea is IIfcLShapeProfileDef) return true; else return false; }).ToList();

        }

        public static List<IIfcBuildingStorey> Storeys
        {
            get { return _bStoreys; }
        }

        public static List<IIfcSlab> Floors
        {
            get { return _floors; }
        }

        public static List<IIfcColumn> Columns
        {
            get { return _columns; }
        }

        public static List<IIfcBeam> Beams
        {
            get { return _beams; }
        }

        public static List<IIfcMemberStandardCase> Inclines     
        {
            get { return _inclines; }
        }

        public static List<IIfcColumn> ColumnsSt
        {
            get { return _columnsSt; }
        }

        public static List<IIfcBeam> BeamsSt
        {
            get { return _beamsSt; }
        }

        public static List<IIfcMemberStandardCase> Braces     
        {
            get { return _braces; }
        }

    }
}
