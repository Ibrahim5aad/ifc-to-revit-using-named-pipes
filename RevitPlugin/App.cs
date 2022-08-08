using Autodesk.Revit.UI;

namespace IFCtoRevit
{
	public class App : IExternalApplication
	{
		/// <summary>
		/// Will execute your tasks when Autodesk Revit starts.
		/// </summary>
		/// <param name="application">A handle to the application being started.</param>
		/// <returns>
		/// Indicates if the external application completes its work successfully.
		/// </returns>
		public Result OnStartup(UIControlledApplication application)
		{ 
			SetupInterface
				.Initialize(application);
			return Result.Succeeded;
		}


		/// <summary>
		/// Will excute your tasks when Autodesk Revit shuts down.
		/// </summary>
		/// <param name="application">A handle to the application being shut down.</param>
		/// <returns>
		/// Indicates if the external application completes its work successfully.
		/// </returns>
		public Result OnShutdown(UIControlledApplication application)
		{
			return Result.Succeeded;

		}


	}
}
