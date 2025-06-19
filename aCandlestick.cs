using System; //import System namespace for functionalities like DateTime
using System.Collections.Generic; //import the Collections.Generic namespace for using generic collections
using System.Linq; //import the LINQ namespace for querying collections
using System.Text; //import the Text namespace for string manipulation
using System.Threading.Tasks; //import the Tasks namespace for asynchronous programming

namespace Project //declare namespace Project to organize code
{
    public class aCandlestick //declare public class named aCandlestick
    {
        //properties of candlestick
        //get date and time for candlestick data
        public DateTime Date { get; set; } //property to store date and time of candlestick

        //open price for candlestick's period
        public decimal Open { get; set; } //property to store the opening price

        //highest price during time period
        public decimal High { get; set; } //property to store highest price

        //lowest price during time period
        public decimal Low { get; set; } //property to store lowest price

        //closing price at end of time period
        public decimal Close { get; set; } //property to store closing price

        //trading volume during time period
        public ulong Volume { get; set; } //property to store volume


        //constructors
        //default constructor initializing empty candlestick object
        public aCandlestick() //default constructor HAS no parameters
        {
            //properties will have default values
        }
        /// <param name="date">Timestamp for the candlestick data.</param> //parameter for date
        /// <param name="open">Opening price.</param> //parameter for opening price
        /// <param name="high">Highest price.</param> //parameter for highest price
        /// <param name="low">Lowest price.</param> //parameter  for lowest price
        /// <param name="close">Closing price.</param> //parameter for closing price
        /// <param name="volume">Trading volume.</param> //parameter for volume
        public aCandlestick(DateTime date, decimal open, decimal high, decimal low, decimal close, ulong volume) //constructor with parameters
        {
            Date = date; //assign date  to  date property
            Open = open; //assign open to open property
            High = high; //assign high  to high property
            Low = low; //assign low to low property
            Close = close; //assign close  to close property
            Volume = volume; //assign volume to volume property
        }

        /// <returns>format string with date, open, high, low, close, and volume data.</returns> //describes return value
        public override string ToString() //override ToString method to provide custom string representation
        {
            return $"Timestamp: {Date}, Open: {Open}, High: {High}, Low: {Low}, Close: {Close}, Volume: {Volume}";  //returns format string with the candlestick data
        }
    }
}