using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.IO.Pipes;
using Xbim.Ifc4.Interfaces;
using System.Windows.Forms;
using static IFCtoRevit.IFCLoader.Adapter;

namespace IFCtoRevit.IFCLoader
{
    public class Program
    {
        public static bool colsImport = false;
        public static bool beamsImport = false;
        public static bool incsImport = false;
        public static bool floorsImport = false;
        public static bool lvlsImport = false;

        [STAThread]
        public static void Main(string[] args)
        {
            if (args == null || args.Length < 1) return;

            string pipeWriteHandle = args[0];

            Application.EnableVisualStyles();
            LoaderWin main = new LoaderWin();
            Application.Run(main);


            //If Cancel button was clicked 
            if (LoaderWin.isCancelled) return;


            //Loading the file
            IFC_Loader.LoadFile(LoaderWin.FilePath);


            //Getting the IFC Elements
            IFC_Loader.GetIFCElements();

            List<IIfcColumn> cols = IFC_Loader.Columns;
            List<IIfcBuildingStorey> stors = IFC_Loader.Storeys;
            List<IIfcSlab> slabs = IFC_Loader.Floors;
            List<IIfcBeam> beamms = IFC_Loader.Beams;
            List<IIfcMemberStandardCase> incs = IFC_Loader.Inclines;

            List<IIfcColumn> Stcols = IFC_Loader.ColumnsSt;
            List<IIfcBeam> Stbeamms = IFC_Loader.BeamsSt;
            List<IIfcMemberStandardCase> Stbraces = IFC_Loader.Braces;



            List<Column> columns = columns = GetColumnsData(cols);
            List<double> storeys = GetStoreyLevels(stors);
            List<FloorSlab> floors = GetFloorsData(slabs);
            List<Beam> beams = GetBeamsData(beamms);
            List<Inclined> inlines = GetInclinesData(incs);

            List<ColumnSt> steelColumn = GetColumnsSteelData(Stcols);
            List<BeamSt> steelBeam = GetBeamsSteelData(Stbeamms);
            List<Brace> steelBrace = GetBracesData(Stbraces);


            //Serializers
            XmlSerializer columnSerializer = new XmlSerializer(typeof(List<Column>));
            XmlSerializer storeySerializer = new XmlSerializer(typeof(List<double>));
            XmlSerializer floorSerializer = new XmlSerializer(typeof(List<FloorSlab>));
            XmlSerializer beamsSerializer = new XmlSerializer(typeof(List<Beam>));
            XmlSerializer inclinesSerializer = new XmlSerializer(typeof(List<Inclined>));
            XmlSerializer steelColumnSerializer = new XmlSerializer(typeof(List<ColumnSt>));
            XmlSerializer steelBeamSerializer = new XmlSerializer(typeof(List<BeamSt>));
            XmlSerializer braceSerializer = new XmlSerializer(typeof(List<Brace>));


            StringWriter columnsXMLstring = new StringWriter();
            StringWriter storeyXMLstring = new StringWriter();
            StringWriter floorXMLstring = new StringWriter();
            StringWriter beamsXMLstring = new StringWriter();
            StringWriter inclinesXMLstring = new StringWriter();
            StringWriter steelColumnXMLstring = new StringWriter();
            StringWriter steelBeamXMLstring = new StringWriter();
            StringWriter braceXMLstring = new StringWriter();



            storeySerializer.Serialize(storeyXMLstring, storeys);

            if (floorsImport) floorSerializer.Serialize(floorXMLstring, floors);
            else floorSerializer.Serialize(floorXMLstring, new List<FloorSlab>());

            if (colsImport)
            {
                columnSerializer.Serialize(columnsXMLstring, columns);
                steelColumnSerializer.Serialize(steelColumnXMLstring, steelColumn);
            }
            else
            {
                columnSerializer.Serialize(columnsXMLstring, new List<Column>());
                steelColumnSerializer.Serialize(steelColumnXMLstring, new List<ColumnSt>());
            }

            if (beamsImport)
            {
                beamsSerializer.Serialize(beamsXMLstring, beams);
                steelBeamSerializer.Serialize(steelBeamXMLstring, steelBeam);
            }
            else
            {
                beamsSerializer.Serialize(beamsXMLstring, new List<Beam>());
                steelBeamSerializer.Serialize(steelBeamXMLstring, new List<BeamSt>());
            }

            if (incsImport)
            {
                inclinesSerializer.Serialize(inclinesXMLstring, inlines);
                braceSerializer.Serialize(braceXMLstring, steelBrace);
            }
            else
            {
                inclinesSerializer.Serialize(inclinesXMLstring, new List<Inclined>());
                braceSerializer.Serialize(braceXMLstring, new List<Brace>());
            }


            //Create an output client pipe to send the XML data to the plugin
            using (var pipeWrite = new AnonymousPipeClientStream(PipeDirection.Out, pipeWriteHandle))
            {
                try
                {
                    using (var sw = new StreamWriter(pipeWrite))
                    {
                        sw.AutoFlush = true;
                        // Send a 'sync message' and wait for the calling process to receive it
                        sw.WriteLine("SYNC");
                        pipeWrite.WaitForPipeDrain();

                        //Sending the XML serialized objects
                        sw.WriteLine(columnsXMLstring.ToString());
                        sw.WriteLine("COLSEND");
                        sw.WriteLine(inclinesXMLstring.ToString());
                        sw.WriteLine("INCSEND");
                        sw.WriteLine(beamsXMLstring.ToString());
                        sw.WriteLine("BEAMSEND");
                        sw.WriteLine(steelColumnXMLstring.ToString());
                        sw.WriteLine("COLSTEELSEND");
                        sw.WriteLine(braceXMLstring.ToString());
                        sw.WriteLine("BRACESEND");
                        sw.WriteLine(steelBeamXMLstring.ToString());
                        sw.WriteLine("BEAMSSTEELEND");
                        sw.WriteLine(storeyXMLstring.ToString());
                        sw.WriteLine("STORSEND");
                        sw.WriteLine(floorXMLstring.ToString());
                        sw.WriteLine("END");

                        columnsXMLstring.Close();
                        storeyXMLstring.Close();
                        beamsXMLstring.Close();
                        floorXMLstring.Close();
                        inclinesXMLstring.Close();
                        steelBeamXMLstring.Close();
                        steelColumnXMLstring.Close();
                        braceXMLstring.Close();
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

        }

    }
}
