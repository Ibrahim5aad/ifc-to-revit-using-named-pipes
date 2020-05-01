using Autodesk.Revit.UI;
using IFCtoRevit.UI;

namespace IFCtoRevit
{

    /// Setup whole plugins interface with tabs, panels, buttons,...
    public class SetupInterface
    {
        #region Constructor
        public SetupInterface()
        {

        }

        #endregion

        #region Public Methods

        public void Initialize(UIControlledApplication app)
        {
            // Create ribbon tab.
            string tabName = "ITI";
            app.CreateRibbonTab(tabName);

            // Create the ribbon panels.
            var annotateCommandsPanel = app.CreateRibbonPanel(tabName, "IFC");

            // Populate button data model.
            var TagWallButtonData = new RevitPushButtonDataModel
            {
                Label = "Import IFC",
                Panel = annotateCommandsPanel,
                Tooltip = "Import IFC files exported from CSI ETABS...",
                CommandNamespacePath = ImportIFC.GetPath(),
                IconImageName = "pushbutton.png",
                TooltipImageName = "tooltipimage.png"
            };

            // Create button from provided data.
            var TagWallButton = RevitPushButton.Create(TagWallButtonData);

        }

        #endregion
    }
}
