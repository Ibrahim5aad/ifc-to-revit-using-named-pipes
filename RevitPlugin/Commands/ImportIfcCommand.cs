using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IFCtoRevit.Services;
using IFCtoRevit.UI.Windows;
using IFCtoRevit.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace IFCtoRevit
{

	[Transaction(TransactionMode.Manual)]
	public partial class ImportIfcCommand : IExternalCommand
	{

		/// <summary>
		/// Gets the path of the command.
		/// </summary>
		/// <returns></returns>
		public static string GetPath()
		{
			return typeof(ImportIfcCommand).Namespace + "." + nameof(ImportIfcCommand);
		}


		/// <summary>
		/// Executes the external command within Revit.
		/// </summary>
		/// <param name="commandData">An ExternalCommandData object which contains reference to Application and View
		/// needed by external command.</param>
		/// <param name="message">Error message can be returned by external command. This will be displayed only if the command status
		/// was "Failed".  There is a limit of 1023 characters for this message; strings longer than this will be truncated.</param>
		/// <param name="elements">Element set indicating problem elements to display in the failure dialog.  This will be used
		/// only if the command status was "Failed".</param>
		/// <returns>
		/// The result indicates if the execution fails, succeeds, or was canceled by user. If it does not
		/// succeed, Revit will undo any changes made by the external command.
		/// </returns>
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{

			UIDocument uidoc = commandData.Application.ActiveUIDocument;
			Document doc = uidoc.Document;
			MainWindowViewModel.RevitVersion = Convert.ToInt32(doc.Application.VersionName.Split(' ').Last());

			if (MainWindow.CurrentWindow == null)
			{
				MainWindow.CurrentWindow = new MainWindow();
				DocumentManager.Instance.Init(commandData.Application);
				MainWindow.CurrentWindow.Show();
			}
			else
			{
				if (MainWindow.CurrentWindow.WindowState == WindowState.Minimized)
					MainWindow.CurrentWindow.WindowState = WindowState.Normal;
				else
					MainWindow.CurrentWindow.Focus();
			}
			return Result.Succeeded;
		}

	}
}
