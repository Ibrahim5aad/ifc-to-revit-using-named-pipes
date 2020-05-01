using System;
using Autodesk.Revit.UI;

namespace IFCtoRevit.UI
{

    public static class RevitPushButton
    {
        #region public methods

        public static PushButton Create(RevitPushButtonDataModel data)
        {
            // The button name based on unique identifier.
            var btnDataName = Guid.NewGuid().ToString();

            // Sets the button data.
            var btnData = new PushButtonData(btnDataName, data.Label, PluginAssembly.GetAssemblyLocation(), data.CommandNamespacePath)
            {
                ToolTip = data.Tooltip,
                LargeImage = ResourceImage.GetIcon(data.IconImageName),
                ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName)
            };

            // Return created button and host it on panel provided in required data model.
            return data.Panel.AddItem(btnData) as PushButton;
        }

        #endregion
    }
}
