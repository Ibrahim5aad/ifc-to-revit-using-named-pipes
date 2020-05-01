using System.Windows.Media.Imaging;


namespace IFCtoRevit
{

    public static class ResourceImage
    {

        public static BitmapImage GetIcon(string name)
        {
            // Create the resource reader stream.
            var stream = PluginAssembly.GetAssembly().GetManifestResourceStream(PluginAssembly.GetNamespace() + "Resources.Images.Icons." + name);

            var image = new BitmapImage();

            // Construct and return image.
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            // Return constructed BitmapImage.
            return image;
        }

    }
}
