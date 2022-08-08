using IFCtoRevit.ViewModels;
using System.Windows;

namespace IFCtoRevit.UI.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary> 
		public MainWindow()
		{
			DataContext = new MainWindowViewModel();
			InitializeComponent();
		}


		/// <summary>
		/// Gets or sets the current window.
		/// </summary>
		/// <value>The current window.</value>
		public static MainWindow CurrentWindow { get; set; }


		/// <summary>
		/// Handles the Click event of the btnClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DataContext = null;
			Close();
		}
	}
}
