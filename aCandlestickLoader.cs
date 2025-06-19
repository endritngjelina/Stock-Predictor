//import required namespaces
using System; //offers classes and base classes for .NET applications
using System.Collections.Generic; //offers collections such as List<T>
using System.Globalization; //offers parsing dates and numbers
using System.IO; //offers file and data stream handling 
using System.Linq; //offers query capabilities on collections
using System.Text; //offers encoding and string manipulation
using System.Threading.Tasks; //offers support for asynchronous programming
using System.Windows.Forms; //offers UI elements for Windows Forms applications

//declare namespace to group related classes
namespace Project//name of namespace is project
{
    public class ACandlestickLoader//public class candlestickloader
    {
        /// <param name="filePath">path to CSV file containing candlestick data.</param>
        /// <returns>list of aCandlestick objects populated with data from CSV file.</returns>
        static public List<aCandlestick> LoadFromCsv(string filePath)//static list for candlesticks
        {
            
            var candlesticks = new List<aCandlestick>();    //list to store candlestick entry from the CSV file
            try //try-catch block to handle exceptions during file reading
            {
                using (var reader = new StreamReader(filePath))//declare a StreamReader object to read CSV file
                {
                    reader.ReadLine();//read header line
                    char[] delimiters = {',', '\\', '"'};   //setting delimiters to split CSV lines
                    while (!reader.EndOfStream)//read line from the file til reach end
                    {
                        var line = reader.ReadLine();//reading line from CSV file
                        if (!string.IsNullOrEmpty(line))    //continue only if line is not empty or null
                        {
                            
                            var values = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);//split line into array of values based on delimiters
                            if (values.Length != 6)     //validate column count; expects exactly 6 columns
                                throw new FormatException("Unexpected Number of Columns in CSV file");  //assume CSV has 6 columns: Time, Open, High, Low, Close, Volume

                            var candlestick = new aCandlestick  //create and populate an aCandlestick object with parsed values
                            {
                                
                                Date = DateTime.ParseExact(values[0], "yyyy-MM-dd", CultureInfo.InvariantCulture),  //parse the date string from the first column into a DateTime object using "yyyy-MM-dd" format
                                Open = Math.Round(100 * decimal.Parse(values[1], CultureInfo.InvariantCulture))/100,    //parse and round open price two decimal places
                                High = Math.Round(100 * decimal.Parse(values[2], CultureInfo.InvariantCulture)) / 100,  //parse and round high price two decimal places
                                Low = Math.Round(100 * decimal.Parse(values[3], CultureInfo.InvariantCulture))/100, //parse and round low price two decimal places
                                Close = Math.Round(100 * decimal.Parse(values[4], CultureInfo.InvariantCulture))/ 100,  //parse and round close price two decimal places
                                Volume = ulong.Parse(values[5], CultureInfo.InvariantCulture)   //parse volume from sixth column and convert unsigned long integer
                            };
                            candlesticks.Add(candlestick);  //add parsed candlestick data to list
                        }
                    }
                }
            }
            catch (Exception ex)    //catch and handle exceptions that occur during file reading or parsing
            {
                MessageBox.Show($"An error occurred while loading candlesticks: {ex.Message}"); //display error message if issues arise
            }
            return candlesticks;    //return populated list of candlestick objects
        }
    }
}
