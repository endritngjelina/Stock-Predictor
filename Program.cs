// Import necessary namespaces
using System; // Provides basic functionality, including data types and exceptions
using System.Collections.Generic; // Provides classes for defining generic collections
using System.Linq; // Provides classes and methods for querying collections
using System.Threading.Tasks; // Provides types for asynchronous programming
using System.Windows.Forms; // Provides classes for creating Windows-based applications

//namespace for the project
namespace Project
{
    // Define the Program class as static, meaning it cannot be instantiated
    internal static class Program
    {
        /// The main entry point for the application.
        [STAThread] // Indicates that the COM threading model for the application is single-threaded apartment
        static void Main()
        {
            // Enable visual styles for the application, allowing for themed controls
            Application.EnableVisualStyles();
            
            // Set default text rendering for controls to be compatible with older versions
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Start the application with the specified form (Form_display)
            Application.Run(new Form_display());
        }
    }
}