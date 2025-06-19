using System; // Import the System namespace, which provides fundamental classes and base classes that define commonly used types
using System.Collections.Generic; // Import the System.Collections.Generic namespace, which provides classes for defining generic collections such as lists, dictionaries, and queues
using System.Linq; // Import the System.Linq namespace, which provides classes and methods for querying collections using Language-Integrated Query (LINQ)
using System.Text; // Import the System.Text namespace, which provides classes for manipulating strings and encoding character data
using System.Threading.Tasks; // Import the System.Threading.Tasks namespace, which provides types that simplify the work of writing concurrent and asynchronous code

namespace Project//name of namespace is project
{
    using System;//using system 

    namespace Project//name of namespace
    {
        public class SmartCandlestick : aCandlestick // Represents an advanced candlestick entry with additional properties and pattern detection.
        {
            
            public decimal Range => High - Low; // Range from high to low
            public decimal BodyRange => Math.Abs(Open - Close); // BodyRange ( absolute difference between Open and Close)
            public decimal TopPrice => Math.Max(Open, Close);   // TopPrice (higher of Open and Close)
            public decimal BottomPrice => Math.Min(Open, Close);    // BottomPrice (the lower of Open and Close)

            // UpperTail (the height of the upper tail, calculated as the difference between High and TopPrice)
            public decimal UpperTail => High - TopPrice;

            // LowerTail (the height of the lower tail, calculated as the difference between BottomPrice and Low)
            public decimal LowerTail => BottomPrice - Low;

            // Pattern Detection Properties
            public bool IsBullish => Close > Open;  // Determines if the candlestick is bullish (closing price is greater than opening price)
            public bool IsBearish => Close < Open;  // Determines if the candlestick is bearish (closing price is less than opening price)
            public bool IsNeutral => Close == Open;// Determines if the candlestick is neutral (open and close prices are equal)
            public SmartCandlestick() { }   // Constructors

            // This constructor initializes a new instance of the SmartCandlestick with the specified parameters.
            public SmartCandlestick(DateTime date, decimal open, decimal high, decimal low, decimal close, ulong volume)
                // Call the base class constructor with the provided parameters
                : base(date, open, high, low, close, volume) 
            { 
                //body of constructor is empty, as base constructor takes care of initialization
            }
            // Override the ToString method to provide a custom string representation of the SmartCandlestick object
            public override string ToString()
            {
                // Call the base class's ToString method to get the default string representation
                // and concatenate additional properties specific to the SmartCandlestick
                return base.ToString() +
                    $", Range: {Range}, BodyRange: {BodyRange}, " + // Include the Range and BodyRange properties in the string
                    $"TopPrice: {TopPrice}, BottomPrice: {BottomPrice}, " + // Include the TopPrice and BottomPrice properties
                    $"UpperTail: {UpperTail}, LowerTail: {LowerTail}"; // Include the UpperTail and LowerTail properties
            }
        }
    }
}