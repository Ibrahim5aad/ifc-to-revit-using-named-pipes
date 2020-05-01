using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.IO;
using IFCtoRevit.IFCLoader;
using System.Xml.Serialization;
using System.IO.Pipes;
using System.Diagnostics;
using System.Windows.Forms;
using IFCtoRevit.UI;
using System.Drawing;
using System.Reflection;



namespace IFCtoRevit
{

    [Transaction(TransactionMode.Manual)]
    public partial class ImportIFC : IExternalCommand
    {
        public static int revitVersion;

        List<FamilyInstance> colInstances;
        List<FamilyInstance> incInstances;
        List<FamilyInstance> beamInstances;
        List<Grid>[] grids;
        List<Level> levels;
        List<Floor> floors;
        List<FamilyInstance> colStInstances;
        List<FamilyInstance> BraceInstances;
        List<FamilyInstance> beamStInstances;


        int lvlNumber = 0;
        byte xGridName = 65; //ASCII code of 'A'
        int yGridName = 1;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            revitVersion = Convert.ToInt32(doc.Application.VersionName.Split(' ').Last());

            //The IFC Loader process
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var ifcLoader = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(assemblyFolder,"IFCLoader.exe"),
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            Application.EnableVisualStyles();
            MainWindow main = new MainWindow();

            Label lbl = new Label();
            lbl.Location = new System.Drawing.Point(90, 110);
            lbl.AutoSize = true;
            lbl.Font = new Font("Calibri", 12);
            lbl.Text = $"Revit Version: {revitVersion}";
            main.Controls.Add(lbl);

            //Create an input server pipe to recieve the XML data from the IFC Loader
            using (var pipeRead = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            {
                ifcLoader.StartInfo.Arguments = pipeRead.GetClientHandleAsString();
                void startLoader(object o, EventArgs e) => ifcLoader.Start();
                main.loaderBtn.Click += startLoader;
                Application.Run(main);


                //Lists of strings to recieve the XML data
                List<string> columnsResult = new List<string>();
                List<string> storeysResult = new List<string>();
                List<string> floorsResult = new List<string>();
                List<string> beamsResult = new List<string>();
                List<string> inclinesResult = new List<string>();

                List<string> columnsStResult = new List<string>();
                List<string> beamsStResult = new List<string>();
                List<string> bracesResult = new List<string>();

                if (main.isCancelled == true)
                {
                    ifcLoader.Close();
                    return Result.Cancelled;
                }
                pipeRead.DisposeLocalCopyOfClientHandle();

                try
                {
                    using (var sr = new StreamReader(pipeRead))
                    {
                        string temp;
                        // Wait for 'sync message' from the other process
                        do
                        {
                            temp = sr.ReadLine();
                        } while (temp == null || !temp.StartsWith("SYNC"));

                        // Read Column data until 'COLSEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("COLSEND"))
                        {
                            columnsResult.Add(temp);
                        }

                        // Read Inclines data until 'INCSEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("INCSEND"))
                        {
                            inclinesResult.Add(temp);
                        }

                        // Read Beams data until 'BEAMSEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("BEAMSEND"))
                        {
                            beamsResult.Add(temp);
                        }

                        // Read I-Column data until 'COLSTEELSEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("COLSTEELSEND"))
                        {
                            columnsStResult.Add(temp);
                        }

                        // Read Braces data until 'BRACESEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("BRACESEND"))
                        {
                            bracesResult.Add(temp);
                        }

                        // Read I-Beams data until 'BEAMSSTEELEND message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("BEAMSSTEELEND"))
                        {
                            beamsStResult.Add(temp);
                        }

                        // Read Storeys data until 'END message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("STORSEND"))
                        {
                            storeysResult.Add(temp);
                        }

                        // Read Floors data until 'END message' from the other process
                        while ((temp = sr.ReadLine()) != null && !temp.StartsWith("END"))
                        {
                            floorsResult.Add(temp);

                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ifcLoader.WaitForExit();
                    ifcLoader.Close();

                }


                //Joining XML data lines
                string columnsXMLData = String.Join(Environment.NewLine, columnsResult.ToArray());
                string beamsXMLData = String.Join(Environment.NewLine, beamsResult.ToArray());
                string inclinesXMLData = String.Join(Environment.NewLine, inclinesResult.ToArray());
                string storeysXMLData = String.Join(Environment.NewLine, storeysResult.ToArray());
                string floorsXMLData = String.Join(Environment.NewLine, floorsResult.ToArray());

                string bracesXMLData = String.Join(Environment.NewLine, bracesResult.ToArray());
                string beamsStXMLData = String.Join(Environment.NewLine, beamsStResult.ToArray());
                string columnsStXMLData = String.Join(Environment.NewLine, columnsStResult.ToArray());


                //XML Deserializers
                XmlSerializer columnsDeserializer = new XmlSerializer(typeof(List<Column>));
                XmlSerializer beamsDeserializer = new XmlSerializer(typeof(List<Beam>));
                XmlSerializer inclinesDeserializer = new XmlSerializer(typeof(List<Inclined>));
                XmlSerializer storeysDeserializer = new XmlSerializer(typeof(List<double>));
                XmlSerializer floorsDeserializer = new XmlSerializer(typeof(List<FloorSlab>));

                XmlSerializer columnsStDeserializer = new XmlSerializer(typeof(List<ColumnSt>));
                XmlSerializer beamsStDeserializer = new XmlSerializer(typeof(List<BeamSt>));
                XmlSerializer bracesDeserializer = new XmlSerializer(typeof(List<Brace>));


                //String Readers streams
                StringReader colsStringReader = new StringReader(columnsXMLData);
                StringReader beamsStringReader = new StringReader(beamsXMLData);
                StringReader inclinesStringReader = new StringReader(inclinesXMLData);
                StringReader storsStringReader = new StringReader(storeysXMLData);
                StringReader floorsStringReader = new StringReader(floorsXMLData);

                StringReader columnsStStringReader = new StringReader(columnsStXMLData);
                StringReader beamsStStringReader = new StringReader(beamsStXMLData);
                StringReader bracesStringReader = new StringReader(bracesXMLData);



                //Lists of required data

                List<Column> cols = columnsDeserializer.Deserialize(colsStringReader) as List<Column>;
                List<Inclined> incss = inclinesDeserializer.Deserialize(inclinesStringReader) as List<Inclined>;
                List<Beam> beammms = beamsDeserializer.Deserialize(beamsStringReader) as List<Beam>;
                List<double> storeys = storeysDeserializer.Deserialize(storsStringReader) as List<double>;
                List<FloorSlab> floorSlabs = floorsDeserializer.Deserialize(floorsStringReader) as List<FloorSlab>;

                List<ColumnSt> StCol = columnsStDeserializer.Deserialize(columnsStStringReader) as List<ColumnSt>;
                List<BeamSt> Stbeam = beamsStDeserializer.Deserialize(beamsStStringReader) as List<BeamSt>;
                List<Brace> StBrace = bracesDeserializer.Deserialize(bracesStringReader) as List<Brace>;


                //Disposing resources and closing the string readers 
                colsStringReader.Dispose();
                inclinesStringReader.Dispose();
                beamsStringReader.Dispose();
                storsStringReader.Dispose();
                floorsStringReader.Dispose();

                columnsStStringReader.Dispose();
                beamsStStringReader.Dispose();
                bracesStringReader.Dispose();


                floorsStringReader.Close();
                storsStringReader.Close();
                beamsStringReader.Close();
                inclinesStringReader.Close();
                colsStringReader.Close();

                columnsStStringReader.Close();
                beamsStStringReader.Close();
                bracesStringReader.Close();


                //Creating Levels 
                void createLevels() => levels = CreateLevels(doc, storeys);
                InitTransaction(doc, "Create Levels", createLevels);

                //Creating Columns 
                void createColumns() => colInstances = CreateColumnInstance(doc, cols, "Concrete-Rectangular-Column", true, storeys);
                InitTransaction(doc, "Create Columns", createColumns);

                //Creating Inclined Columns 
                void createInclines() => incInstances = CreateInclinedInstance(doc, incss, "Concrete-Rectangular-Column", true, storeys);
                InitTransaction(doc, "Create Inclined Columns", createInclines);

                //Creating Beams
                void createBeams() => beamInstances = CreateBeamInstance(doc, beammms, "Concrete-Rectangular Beam");
                InitTransaction(doc, "Create Beams", createBeams);

                //Creating steel Columns 
                void createSteelColumns() => colStInstances = CreateSteelColumnInstance(doc, StCol, "W Shapes-Column", true, storeys);
                InitTransaction(doc, "Create Columns", createSteelColumns);

                //Creating Braces 
                void createBraces() => BraceInstances = CreateBraceInstance(doc, StBrace, "L-Angle-Column", true, storeys);
                InitTransaction(doc, "Create Braces", createBraces);

                //Creating steel Beams
                void createSteelBeams() => beamStInstances = CreateSteelBeamInstance(doc, Stbeam, "W Shapes");
                InitTransaction(doc, "Create steel Beams", createSteelBeams);


                //Creating Grids
                List<FamilyInstance> allColumns = new List<FamilyInstance>();
                allColumns.AddRange(colInstances);
                allColumns.AddRange(colStInstances);
                allColumns.RemoveAll(col => col == null);
                List<XYZ> gridsIntersections = GetGridsIntersections(allColumns);
                List<double> xGridLocations = GetXGridsLocations(gridsIntersections);
                List<double> yGridLocations = GetYGridsLocations(gridsIntersections);

                void creatGrids() => grids = CreateGrids(doc, xGridLocations, yGridLocations);
                InitTransaction(doc, "Create Grids", creatGrids);


                //Creating Floors
                void createFloors() => floors = CreateFloor(doc, floorSlabs);
                InitTransaction(doc, "Create Floors", createFloors);
            }

            return Result.Succeeded;
        }

    }
}
