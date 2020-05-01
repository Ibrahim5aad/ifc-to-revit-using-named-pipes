using System;
using Autodesk.Revit.UI;



namespace IFCtoRevit.UI
{

    /// <summary>
    /// Represents Revit push button data model.
    /// </summary>
    public class RevitPushButtonDataModel
    {
        #region public methods

        public string Label { get; set; }


        public RibbonPanel Panel { get; set; }

        public string CommandNamespacePath { get; set; }

        public string Tooltip { get; set; }

        public string IconImageName { get; set; }


        public string TooltipImageName { get; set; }

        #endregion

        #region constructor


        public RevitPushButtonDataModel()
        {

        }

        #endregion
    }
}
