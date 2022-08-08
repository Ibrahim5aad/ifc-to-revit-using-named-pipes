using Autodesk.Revit.DB;
using IFCtoRevit.Base;
using IFCtoRevit.IFCLoader;
using IFCtoRevit.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Serialization;

namespace IFCtoRevit.ViewModels
{
	public partial class MainWindowViewModel : ViewModelBase
	{
        #region Fields

        private Process _ifcLoader;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            RunModellingCommand = new RelayCommand(RunModelling);

            //The IFC Loader process
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _ifcLoader = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(assemblyFolder,"IFCLoader.exe"),
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
        }

        #endregion

        #region Commands
        public ICommand RunModellingCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Runs the modelling.
        /// </summary>
        private void RunModelling()
        {
            using (var pipeRead = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            {
                _ifcLoader.StartInfo.Arguments = pipeRead.GetClientHandleAsString();

                _ifcLoader.Start(); 

                //Lists of strings to recieve the XML data
                List<string> columnsResult = new List<string>();
                List<string> storeysResult = new List<string>();
                List<string> floorsResult = new List<string>();
                List<string> beamsResult = new List<string>();
                List<string> inclinesResult = new List<string>();

                List<string> columnsStResult = new List<string>();
                List<string> beamsStResult = new List<string>();
                List<string> bracesResult = new List<string>();


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
                    _ifcLoader.WaitForExit();
                    _ifcLoader.Close();

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
                void createLevels() => levels = CreateLevels(DocumentManager.Instance.CurrentDocument, storeys);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Levels", createLevels);

                //Creating Columns 
                void createColumns() => colInstances = CreateColumnInstance(DocumentManager.Instance.CurrentDocument, cols, "Concrete-Rectangular-Column", true, storeys);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Columns", createColumns);

                //Creating Inclined Columns 
                void createInclines() => incInstances = CreateInclinedInstance(DocumentManager.Instance.CurrentDocument, incss, "Concrete-Rectangular-Column", true, storeys);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Inclined Columns", createInclines);

                //Creating Beams
                void createBeams() => beamInstances = CreateBeamInstance(DocumentManager.Instance.CurrentDocument, beammms, "Concrete-Rectangular Beam");
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Beams", createBeams);

                //Creating steel Columns 
                void createSteelColumns() => colStInstances = CreateSteelColumnInstance(DocumentManager.Instance.CurrentDocument, StCol, "W Shapes-Column", true, storeys);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Columns", createSteelColumns);

                //Creating Braces 
                void createBraces() => BraceInstances = CreateBraceInstance(DocumentManager.Instance.CurrentDocument, StBrace, "L-Angle-Column", true, storeys);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Braces", createBraces);

                //Creating steel Beams
                void createSteelBeams() => beamStInstances = CreateSteelBeamInstance(DocumentManager.Instance.CurrentDocument, Stbeam, "W Shapes");
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create steel Beams", createSteelBeams);


                //Creating Grids
                List<FamilyInstance> allColumns = new List<FamilyInstance>();
                allColumns.AddRange(colInstances);
                allColumns.AddRange(colStInstances);
                allColumns.RemoveAll(col => col == null);
                List<XYZ> gridsIntersections = GetGridsIntersections(allColumns);
                List<double> xGridLocations = GetXGridsLocations(gridsIntersections);
                List<double> yGridLocations = GetYGridsLocations(gridsIntersections);

                void creatGrids() => grids = CreateGrids(DocumentManager.Instance.CurrentDocument, xGridLocations, yGridLocations);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Grids", creatGrids);


                //Creating Floors
                void createFloors() => floors = CreateFloor(DocumentManager.Instance.CurrentDocument, floorSlabs);
                InitTransaction(DocumentManager.Instance.CurrentDocument, "Create Floors", createFloors);
            }

        }


        /// <summary>
        /// Provides the required validation logic of
        /// any property you want to add validation for.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Validate(string propertyName, object value)
        {
            return true;
        }

        #endregion
    }
}
