using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace IFCtoRevit
{
    class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            SetupInterface @interface = new SetupInterface();
            @interface.Initialize(application);

            return Result.Succeeded;
        }


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;

        }


    }
}
