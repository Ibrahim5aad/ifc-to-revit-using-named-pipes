using System.Reflection;


namespace IFCtoRevit
{
    public static class PluginAssembly
    {

        public static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        // Assembly Location
        public static string GetAssemblyLocation()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        //Assembly Namespace
        public static string GetNamespace()
        {
            return typeof(PluginAssembly).Namespace + ".";
        }


    }
}