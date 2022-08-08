using Autodesk.Revit.UI;
using IFCtoRevit.UI;

namespace IFCtoRevit
{

    /// <summary>
    /// Setup whole plugins interface with tabs, panels, buttons,...
    /// </summary>
    public class SetupInterface
    { 
		#region Methods

		/// <summary>
		/// Initializes the specified application.
		/// </summary>
		/// <param name="app">The application.</param>
		public static void Initialize(UIControlledApplication app)
        {
            // Create ribbon tab.
            string tabName = "IFC to Revit";
            app.CreateRibbonTab(tabName);

            // Create the ribbon panels.
            var annotateCommandsPanel = app.CreateRibbonPanel(tabName, "IFC");

            // Populate button data model.
            var TagWallButtonData = new RevitPushButtonDataModel
            {
                Label = "Import IFC",
                Panel = annotateCommandsPanel,
                Tooltip = "Import IFC files exported from CSI ETABS...",
                CommandNamespacePath = ImportIfcCommand.GetPath(),
                IconImageName = "pushbutton.png",
                TooltipImageName = "tooltipimage.png"
            };

            // Create button from provided data.
            var TagWallButton = RevitPushButton.Create(TagWallButtonData);

        }

        #endregion
    }
}
