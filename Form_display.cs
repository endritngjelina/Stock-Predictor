using System; // Importing System namespace for basic functionality
using System.Collections.Generic; // Importing collections for list handling
using System.ComponentModel; // Importing for component model functionalities
using System.Data; // Importing data functionalities
using System.Drawing; // Importing drawing functionalities
using System.Linq; // Importing LINQ functionalities
using System.Text; // Importing text functionalities
using System.Threading.Tasks; // Importing threading and asynchronous task functionalities
using System.Windows.Forms; // Importing Windows Forms functionalities
using System.Reflection; // Importing Reflection namespace for metadata handling
using System.Runtime.InteropServices.ComTypes; // Importing for COM type definitions
using System.Security.Cryptography.X509Certificates; // Importing for handling X.509 certificates
using System.IO; // Importing for file and stream handling
using System.Diagnostics; // Importing for process management and debugging
using System.Windows.Forms.DataVisualization.Charting; // Importing for chart visualization in Windows Forms
using System.Windows.Forms.VisualStyles; // Importing for visual styling of Windows Forms controls
using static System.Net.Mime.MediaTypeNames; // Importing static members of MediaTypeNames for handling MIME types
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar; // Importing static members of VisualStyleElement.Rebar for UI styling
namespace Project //define the namespace for the Windows Forms application
{
    // Add this class definition
    public class Wave{  // Represents a wave in a price chart, used in technical analysis
    public int StartIndex { get; set; } // The index where the wave starts
    public int EndIndex { get; set; }   // The index where the wave ends
    public decimal StartPrice { get; set; } // Price at the start of the wave
    public decimal EndPrice { get; set; }   // Price at the end of the wave
    public bool IsUpWave { get; set; }  // True if the wave is upward, false if downward
    public string Type { get; set; }    // Type of wave (e.g., "Impulse", "Correction")
    public int ConfirmationCount { get; set; }  // Number of confirmation points for this wave
    public List<int> ConfirmationIndices { get; set; }  // Indices where confirmations occurred
    public List<FibonacciLevel> FibonacciLevels { get; set; }   // Fibonacci levels associated with the wave
    public string DisplayName => $"{StartIndex} to {EndIndex} - {Type} ({ConfirmationCount} confirmations)";    // Display name showing wave summary
    public Wave()// Constructor
    {
        ConfirmationIndices = new List<int>();// Initialize confirmation indices list
        FibonacciLevels = new List<FibonacciLevel>// Initialize Fibonacci levels list
        {
            new FibonacciLevel(0),  // Starting point of the wave (0%)
            new FibonacciLevel(23.6),   // Minor Fibonacci retracement level (23.6%)
            new FibonacciLevel(38.2),   // Common retracement level (38.2%)
            new FibonacciLevel(50.0),   // Midpoint retracement (50%), not official but widely used
            new FibonacciLevel(61.8),   // Golden ratio retracement level (61.8%)
            new FibonacciLevel(76.4),   // Deeper retracement level (76.4%)
            new FibonacciLevel(100) // Full retracement or extension (100%)
        };
    }
    public void CalculateFibonacciLevels()// Calculates price values for each Fibonacci level
    {
        decimal priceRange = Math.Abs(EndPrice - StartPrice);// Determine total price movement (absolute value)
        
        foreach (var level in FibonacciLevels)// Loop through each Fibonacci level
        {
            if (IsUpWave)// If the wave is upward
                level.Price = StartPrice + (priceRange * (decimal)(level.Percentage / 100.0));// Add proportion of range to start price
            else    // If the wave is downward
                level.Price = StartPrice - (priceRange * (decimal)(level.Percentage / 100.0));// Subtract proportion of range from start price
        }
    }
    }

    public class FibonacciLevel                                 // Represents a single Fibonacci level in a wave
{
    public double Percentage { get; set; }                  // Fibonacci percentage (e.g., 38.2, 61.8)
    public decimal Price { get; set; }                      // Calculated price level based on percentage
    public LineAnnotation LineAnnotation { get; set; }      // Visual line annotation on chart (optional)
    public TextAnnotation TextAnnotation { get; set; }      // Label or text annotation for the level

    public FibonacciLevel(double percentage)                // Constructor to initialize with percentage
    {
        Percentage = percentage;                            // Set the Fibonacci percentage
    }
}


    public class WaveAnnotations                                 // Holds chart annotations related to a wave
{
    public RectangleAnnotation Rectangle { get; set; }       // Rectangle annotation to highlight the wave area
    public LineAnnotation Diagonal { get; set; }             // Diagonal line annotation representing wave direction
}

    public partial class Form_display : Form    //partial public class initialization
    {
        // Add these at the class level with other member variables
        private void SuspendChartUpdates() => chart_candlesticks.SuspendLayout();   // Temporarily suspend chart layout updates
        private void ResumeChartUpdates() => chart_candlesticks.ResumeLayout();     // Resume chart layout updates after changes
        private bool isSelecting = false;                                            // Flag to indicate if user is currently selecting a wave
        private int startIndex = -1;                                                 // Index where selection started
        private int currentIndex = -1;                                               // Current index during selection
        private decimal startPrice = 0;                                              // Price at selection start
        private decimal currentPrice = 0;                                            // Current price during selection
        private List<Wave> waves = new List<Wave>();                                 // List of all identified waves
        private Wave selectedWave = null;                                            // Currently selected wave (if any)
        private decimal simulationStep = 5.0m; // Default step size in percentage
        private decimal minPricePercent = 0.0m;
        private decimal maxPricePercent = 100.0m;
        private int simulationDirection = 1; // 1 for increasing, -1 for decreasing
        List<aCandlestick> allCandlesticks; // List to hold all candlesticks read from the CSV file
        public Form_display() // Default constructor for Form_display, initializes the form's components.
        {
            InitializeComponent(); // Initialize form components.
        }

        // Add mouse event handlers to enable rubber banding
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);                                                       // Call base class's OnLoad to ensure proper initialization
            this.chart_candlesticks.MouseDown += Chart_candlesticks_MouseDown;   // Attach handler for mouse button press on the chart
            this.chart_candlesticks.MouseMove += Chart_candlesticks_MouseMove;   // Attach handler for mouse movement on the chart
            this.chart_candlesticks.MouseUp += Chart_candlesticks_MouseUp;       // Attach handler for mouse button release on the chart

        }

        private void Chart_candlesticks_MouseDown(object sender, MouseEventArgs e)// Handles mouse move event on the candlestick chart
        {
            if (allCandlesticks == null || allCandlesticks.Count == 0)// Exit if not in selection mode or no candlesticks are available
                return; //return statement

            
            var hitTestResult = chart_candlesticks.HitTest(e.X, e.Y);// Convert mouse coordinates to chart values
            
            if (hitTestResult.ChartElementType == ChartElementType.DataPoint)// Check if the mouse is over a data point
            {
                isSelecting = true;
                startIndex = hitTestResult.PointIndex;
                
                // Check if this is a peak or valley
                // Check if the start index corresponds to an extreme point (peak or valley)
            if (IsExtremePoint(startIndex))
            {
                // If the start point is valid (peak or valley), set the start price and initialize the current index and price
                startPrice = GetPriceAtIndex(startIndex);
                currentIndex = startIndex;
                currentPrice = startPrice;
            }
            else
            {
                // If the start point is not an extreme, stop the selection and show a message to the user
                isSelecting = false;
                MessageBox.Show("Please start from a peak or valley point.");//error message
            }

            }
        }

        // Event handler for mouse movement over the candlestick chart
        private void Chart_candlesticks_MouseMove(object sender, MouseEventArgs e)
        {
            // If not selecting or no candlesticks available, exit the method
            if (!isSelecting || allCandlesticks == null || allCandlesticks.Count == 0)
                return;

            // Convert the mouse coordinates to chart values (hit test)
            var hitTestResult = chart_candlesticks.HitTest(e.X, e.Y);
            
            // Check if the mouse is over a data point (candlestick)
            if (hitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                // Update the current index and price based on mouse position
                currentIndex = hitTestResult.PointIndex;
                currentPrice = GetPriceAtIndex(currentIndex);
                
                // Draw the wave visualization in real-time based on the selected points
                DrawWaveVisualization();
            }
        }

        // Event handler for mouse button release (mouse up) on the candlestick chart
        private void Chart_candlesticks_MouseUp(object sender, MouseEventArgs e)
        {
            // If not selecting or no candlesticks available, exit the method
            if (!isSelecting || allCandlesticks == null || allCandlesticks.Count == 0)
                return;

            // Stop the selection process
            isSelecting = false;
            
            // Finalize the wave selection if the end index is different from the start index
            if (currentIndex != startIndex)
            {
                // Create a new wave based on the selected start and end points
                Wave wave = CreateWave(startIndex, currentIndex, startPrice, currentPrice);
                
                // If the wave is valid, add it to the list and update the combo box
                if (wave != null)
                {
                    waves.Add(wave);  // Add the wave to the waves list
                    comboBox_waves.Items.Add(wave);  // Add the wave to the combo box
                    comboBox_waves.SelectedIndex = comboBox_waves.Items.Count - 1;  // Select the newly added wave
                }
            }
        }


        private bool IsExtremePoint(int index)//function to find if its extreme point
        {
            // Check if the list of candlesticks is null, empty, or if the index is out of bounds (first or last element)
            if (allCandlesticks == null || allCandlesticks.Count == 0 || 
                index <= 0 || index >= allCandlesticks.Count - 1)
                return false;  // Return false if any of the conditions are true as the point can't be extreme

            var previous = allCandlesticks[index - 1];  // Get the previous candlestick
            var current = allCandlesticks[index];  // Get the current candlestick
            var next = allCandlesticks[index + 1];  // Get the next candlestick

            // Check if the current point is a peak (local maximum)
            if (current.High > previous.High && current.High > next.High)
                return true;  // Return true if the current candlestick is a peak

            // Check if the current point is a valley (local minimum)
            if (current.Low < previous.Low && current.Low < next.Low)
                return true;  // Return true if the current candlestick is a valley

            return false;  // Return false if neither peak nor valley
        }


        private decimal GetPriceAtIndex(int index)//fuctino to get price at an index
        {
            if (allCandlesticks == null || allCandlesticks.Count == 0 ||  // Check if the list of all candlesticks is null or empty
                index < 0 || index >= allCandlesticks.Count)  // Check if the index is out of bounds (negative or exceeds the list size)
                return 0;  // Return 0 if any of the above conditions are true, as it's not a valid candlestick

            var candlestick = allCandlesticks[index];  // Get the candlestick at the specified index

            // If it's a peak, use the high price
            if (index > 0 && index < allCandlesticks.Count - 1)  // Ensure the index is valid (not the first or last element)
            {
                var previous = allCandlesticks[index - 1];  // Get the previous candlestick
                var next = allCandlesticks[index + 1];  // Get the next candlestick
                
                if (candlestick.High > previous.High && candlestick.High > next.High)  // Check if the current candlestick's high is higher than both the previous and next candlesticks' highs
                    return candlestick.High;  // Return the high price if the current candlestick is a peak (local maximum)
            }

            
            // If it's a valley, use the low price
            if (index > 0 && index < allCandlesticks.Count - 1)  // Ensure the index is valid (not the first or last element)
            {
                var previous = allCandlesticks[index - 1];  // Get the previous candlestick
                var next = allCandlesticks[index + 1];  // Get the next candlestick
                
                if (candlestick.Low < previous.Low && candlestick.Low < next.Low)  // Check if the current candlestick's low is lower than both the previous and next candlesticks' lows
                    return candlestick.Low;  // Return the low price if the current candlestick is a valley (local minimum)
            }

            
            // Otherwise, use the close price
            return candlestick.Close;
        }

        private void DrawWaveVisualization()//fucntion to draw waves
        {
            // Clear previous annotations
            chart_candlesticks.Annotations.Clear();
            
            if (startIndex < 0 || currentIndex < 0 || startIndex == currentIndex)//conditional if start index less than 0 or curr index less that 0
                return;//return statement
            
            // Create a temporary wave for visualization
            Wave wave = CreateWave(startIndex, currentIndex, startPrice, currentPrice);  // Create a new wave object using the given start and current indices, and their corresponding prices

            if (wave != null)  // Check if the wave was successfully created (not null)
            {
                DisplayWave(wave);  // If the wave is valid, display it on the chart
            }

        }

        private Wave CreateWave(int startIdx, int endIdx, decimal startPrice, decimal endPrice)//fucntion for creating waves
        {
            if (allCandlesticks == null || allCandlesticks.Count == 0)  // Check if the list of all candlesticks is null or empty
            {
                return null;  // Return null to exit the method if there are no candlesticks available for processing
            }

    
            if (!IsExtremePoint(endIdx))  // Check if the end index does not correspond to an extreme point (peak or valley)
            {
                MessageBox.Show("End point must be an extreme (peak/valley).");  // Show a message box to inform the user that the end point must be an extreme point
                return null;  // Return null to exit the method and prevent further processing if the condition is not met
            }
            Wave wave = new Wave  // Create a new instance of the Wave class
            {
                StartIndex = startIdx,  // Set the start index of the wave (where the wave starts)
                EndIndex = endIdx,  // Set the end index of the wave (where the wave ends)
                StartPrice = startPrice,  // Set the start price of the wave (price at the start index)
                EndPrice = endPrice,  // Set the end price of the wave (price at the end index)
                IsUpWave = endPrice > startPrice  // Determine if the wave is upward (IsUpWave is true if the end price is higher than the start price)
            };
            wave.Type = wave.IsUpWave ? "Valley to Peak" : "Peak to Valley";  // Set the wave type: "Valley to Peak" for an upward wave, and "Peak to Valley" for a downward wave
            wave.CalculateFibonacciLevels();  // Call the method to calculate Fibonacci levels for the wave based on its price range
            CalculateConfirmations(wave);  // Call the method to calculate the number of confirmations for the wave
            return wave;////return statement
        }

        private void CalculateConfirmations(Wave wave)//function to calculate confirmations
        {
            wave.ConfirmationIndices.Clear();//call function to clear confirmation indicies
            
            if (allCandlesticks == null || allCandlesticks.Count == 0)//condintional if theres no candlestick
                return;//return statement
            
            // Define the Fibonacci levels (in percentage)
            decimal[] fibLevels = { 0m, 23.6m, 38.2m, 50.0m, 61.8m, 76.4m, 100m };
            
            // Calculate price range
            decimal priceRange = Math.Abs(wave.EndPrice - wave.StartPrice);
            
            // For each candle between start and end
            int minIdx = Math.Min(wave.StartIndex, wave.EndIndex);//get minimum of wave
            int maxIdx = Math.Max(wave.StartIndex, wave.EndIndex);//maximum on wave index
            
            for (int i = minIdx; i <= maxIdx; i++)//forloop for candle stick indexing
            {
                var candle = allCandlesticks[i];//candlestick array
                
                // For each Fibonacci level
                foreach (decimal fibLevel in fibLevels)
                {
                    decimal fibPrice;//initinalize decimal for fibonacci
                    
                    // Calculate the price at this Fibonacci level
                    if (wave.IsUpWave)  // Check if the wave is an upward wave
                    {
                        fibPrice = wave.StartPrice + (priceRange * fibLevel / 100m);  // For upward wave, calculate the Fibonacci level price by adding the percentage of price range to the start price
                    }
                    else  // If the wave is a downward wave
                    {
                        fibPrice = wave.StartPrice - (priceRange * fibLevel / 100m);  // For downward wave, calculate the Fibonacci level price by subtracting the percentage of price range from the start price
                    }
                    // Check if the candle crosses this level
                    if ((candle.Low <= fibPrice && candle.High >= fibPrice) &&
                        i != wave.StartIndex && i != wave.EndIndex)
                    {
                        
                        if (!wave.ConfirmationIndices.Contains(i))// This is a confirmation - add to list if not already there
                        {
                            wave.ConfirmationIndices.Add(i);//add to confirmation
                        }
                    }
                }
            }
            
            wave.ConfirmationCount = wave.ConfirmationIndices.Count;//calli function to confirm wave count
        }

        private void DisplayWave(Wave wave)//function to display waves
        {
            chart_candlesticks.Annotations.Clear();
    
            if (wave == null || allCandlesticks == null || allCandlesticks.Count == 0)//conditional is wave is null and so are the count of candlesticks
                return;//return statement

            // Draw rectangle
            RectangleAnnotation rect = new RectangleAnnotation  // Create a new rectangle annotation to represent the wave area
            {
                AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the rectangle (chart's OHLC area)
                AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the rectangle (chart's OHLC area)
                IsSizeAlwaysRelative = false,  // Set the rectangle size to be absolute rather than relative to chart's size
                X = Math.Min(wave.StartIndex, wave.EndIndex),  // Set the X position to the minimum of the start and end indexes
                Y = (double)Math.Max(wave.StartPrice, wave.EndPrice),  // Set the Y position to the maximum of the start and end prices
                Width = Math.Abs(wave.EndIndex - wave.StartIndex),  // Set the width of the rectangle as the absolute difference between the start and end indexes
                Height = (double)Math.Abs(wave.EndPrice - wave.StartPrice),  // Set the height of the rectangle as the absolute difference between the start and end prices
                LineColor = wave.IsUpWave ? Color.Green : Color.Red,  // Set the line color: green for upward wave, red for downward wave
                LineWidth = 2,  // Set the line width to 2 pixels
                BackColor = Color.FromArgb(30, wave.IsUpWave ? Color.LightGreen : Color.LightPink),  // Set the background color with transparency: light green for upward wave, light pink for downward wave
                Visible = true,  // Make the rectangle visible on the chart
                Tag = "Wave"  // Set the tag to "Wave" to identify the annotation type
            };

            chart_candlesticks.Annotations.Add(rect);  // Add the previously created rectangle annotation to the chart's annotations collection

            LineAnnotation diag = new LineAnnotation  // Create a new line annotation to represent the diagonal (wave) line
            {
                AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the line (chart's OHLC area)
                AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the line (chart's OHLC area)
                IsSizeAlwaysRelative = false,  // Set the line size to be absolute rather than relative to chart's size
                X = wave.StartIndex,  // Set the starting X position (start index of the wave)
                Y = (double)wave.StartPrice,  // Set the starting Y position (start price of the wave)
                Width = wave.EndIndex - wave.StartIndex,  // Set the width of the line based on the distance between the start and end indexes
                Height = (double)(wave.EndPrice - wave.StartPrice),  // Set the height of the line based on the difference between the start and end prices
                LineColor = wave.IsUpWave ? Color.DarkGreen : Color.DarkRed,  // Set the line color: dark green for upward wave, dark red for downward wave
                LineWidth = 2,  // Set the line width to 2 pixels
                Visible = true,  // Make the line visible on the chart
                Tag = "Wave"  // Set the tag to "Wave" to identify the annotation type
            };

            chart_candlesticks.Annotations.Add(diag);  // Add the created diagonal line annotation to the chart's annotations collection

            
            // Draw Fibonacci levels
            foreach (var level in wave.FibonacciLevels)
            {
                Color fibColor = level.Percentage == 0 || level.Percentage == 100 ? 
                    Color.Blue : Color.FromArgb(150, 0, 0, 255); // give color based on level percentage
                
                // Horizontal line
                HorizontalLineAnnotation line = new HorizontalLineAnnotation  // Create a new horizontal line annotation to represent the Fibonacci level
                {
                    AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the line (chart's OHLC area)
                    AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the line (chart's OHLC area)
                    Y = (double)level.Price,  // Position the line at the calculated Fibonacci price level
                    LineColor = fibColor,  // Set the line color based on the Fibonacci level's color
                    LineWidth = level.Percentage == 0 || level.Percentage == 100 ? 2 : 1,  // Make the line width 2 for the 0% and 100% levels, 1 for others
                    IsInfinitive = true,  // Make the line extend infinitely across the chart
                    Visible = true,  // Make the line visible on the chart
                    Tag = "Fibonacci"  // Set the tag to "Fibonacci" to identify the annotation type
                };
                chart_candlesticks.Annotations.Add(line);  // Add the created horizontal line annotation to the chart's annotations collection
                level.LineAnnotation = line;  // Associate the line annotation with the Fibonacci level object for later reference

                
                // Text label
                TextAnnotation label = new TextAnnotation  // Create a new text annotation to display the Fibonacci level and its price
                {
                    AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the label (chart's OHLC area)
                    AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the label (chart's OHLC area)
                    AnchorX = wave.EndIndex + 1.5,  // Position the label slightly to the right of the wave's end index
                    Y = (double)level.Price,  // Position the label at the calculated Fibonacci price level
                    Text = $"{level.Percentage}% ({level.Price:F2})",  // Set the label text to display the Fibonacci percentage and the corresponding price, formatted to 2 decimal places
                    ForeColor = fibColor,  // Set the text color based on the Fibonacci level's color
                    Font = new Font("Arial", 8),  // Set the font to Arial, size 8
                    Visible = true,  // Make the label visible on the chart
                    Tag = "Fibonacci"  // Set the tag to "Fibonacci" to identify the annotation type
                };
                chart_candlesticks.Annotations.Add(label);  // Add the created text annotation to the chart's annotations collection
                level.TextAnnotation = label;  // Associate the text annotation with the Fibonacci level object for later reference

            }
            
            // Draw confirmations
            DisplayConfirmations(wave);
            
            // Confirmation count
            TextAnnotation countText = new TextAnnotation  // Create a new text annotation to display the confirmation count for the wave
            {
                AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the label (chart's OHLC area)
                AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the label (chart's OHLC area)
                X = wave.StartIndex + (wave.EndIndex - wave.StartIndex) / 2,  // Position the label in the horizontal center of the wave (midpoint between start and end index)
                Y = (double)(Math.Max(wave.StartPrice, wave.EndPrice) * 1.05m),  // Position the label slightly above the higher of the start and end prices of the wave
                Text = $"Confirmations: {wave.ConfirmationCount}",  // Display the confirmation count in the label text
                ForeColor = Color.DarkBlue,  // Set the text color to dark blue
                Font = new Font("Arial", 10, FontStyle.Bold),  // Set the font to Arial, size 10, bold
                Visible = true,  // Make the text label visible on the chart
                Tag = "Wave"  // Set the tag to "Wave" to identify the annotation type
            };
            chart_candlesticks.Annotations.Add(countText);  // Add the created text annotation to the chart's annotations collection
        }


        private void DisplayFibonacciLines(Wave wave)//funtion to display fiibonacci lines
        {
            if (wave == null)//if theres no wave
                return;//return statement
            
            // Define the Fibonacci levels (in percentage)
            decimal[] fibLevels = { 0m, 23.6m, 38.2m, 50.0m, 61.8m, 76.4m, 100m };  // Array defining the common Fibonacci retracement levels (in percentage) to be used for price analysis
            Color[] fibColors = { Color.Blue, Color.Purple, Color.Green, Color.Orange, Color.Brown, Color.Magenta, Color.Red };  // Array defining colors to represent each Fibonacci level on the chart, corresponding to the levels in fibLevels

            
            // Calculate price range
            decimal priceRange = Math.Abs(wave.EndPrice - wave.StartPrice);
            
            for (int i = 0; i < fibLevels.Length; i++)// Add Fibonacci lines
            {
                decimal fibPrice;//initialize fibonaci price
                
                // Calculate the price at this Fibonacci level
                if (wave.IsUpWave)  // Check if the wave is an upward trend
                    fibPrice = wave.StartPrice + (priceRange * fibLevels[i] / 100m);  // For an upward wave, calculate the Fibonacci price level by adding the price range percentage to the start price
                else  // If the wave is a downward trend
                    fibPrice = wave.StartPrice - (priceRange * fibLevels[i] / 100m);  // For a downward wave, calculate the Fibonacci price level by subtracting the price range percentage from the start price

                
                // Create a horizontal line annotation for this Fibonacci level
                HorizontalLineAnnotation line = new HorizontalLineAnnotation  // Create a new horizontal line annotation for the Fibonacci price level
                {
                    AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the line (chart's OHLC area)
                    AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the line (chart's OHLC area)
                    ClipToChartArea = "ChartArea_OHLC",  // Ensure the line is clipped to the specified chart area
                    Y = (double)fibPrice,  // Set the Y position of the line to the calculated Fibonacci price level
                    LineColor = fibColors[i],  // Set the color of the line based on the Fibonacci level's color
                    LineWidth = 1,  // Set the width of the line to 1 pixel
                    IsInfinitive = true,  // Make the line extend infinitely across the chart
                    Visible = true  // Make the line visible on the chart
                };
                chart_candlesticks.Annotations.Add(line);  // Add the created horizontal line annotation to the chart's annotations collection
                // Add label for this Fibonacci level
                TextAnnotation label = new TextAnnotation  // Create a new text annotation object to display Fibonacci level information
                {
                    AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the label (chart's OHLC area)
                    AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the label (chart's OHLC area)
                    AnchorX = Math.Min(wave.StartIndex, wave.EndIndex),  // Set the X position of the label based on the smaller of the start or end index of the wave
                    Y = (double)fibPrice,  // Set the Y position of the label to the calculated Fibonacci price level
                    Text = $"{fibLevels[i]}% - {fibPrice:F2}",  // Set the label text to display the Fibonacci level and its corresponding price, formatted to 2 decimal places
                    ForeColor = fibColors[i],  // Set the text color of the label based on the Fibonacci level's color
                    Font = new Font("Arial", 8),  // Set the font of the label to Arial with size 8
                    Visible = true  // Make the label visible on the chart
                };
                chart_candlesticks.Annotations.Add(label);  // Add the created text annotation to the chart's annotations collection
            }
        }

        private void DisplayConfirmations(Wave wave)//display confirmations fuction
        {
            if (wave == null) //condiitonal if no wave
                return;//return statement
            
            // For each confirmation point, add a marker
            foreach (int idx in wave.ConfirmationIndices)  // Iterate through each index in the wave's confirmation indices
            {
                var candle = allCandlesticks[idx];  // Retrieve the candlestick data at the current index (idx)

                // Create an ellipse annotation to mark this confirmation on the chart
                EllipseAnnotation ellipse = new EllipseAnnotation
                {
                    AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX,  // Set the X-axis for the ellipse (chart's OHLC area)
                    AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY,  // Set the Y-axis for the ellipse (chart's OHLC area)
                    X = idx,  // Set the X position of the ellipse based on the candlestick index
                    Y = (double)candle.Close,  // Set the Y position of the ellipse based on the candlestick's closing price
                    Width = 0.5,  // Set a fixed width for the ellipse
                    Height = (double)(candle.High - candle.Low),  // Set the height of the ellipse to match the range between high and low prices
                    LineColor = Color.Green,  // Set the line color of the ellipse to green
                    BackColor = Color.FromArgb(50, Color.Green),  // Set a semi-transparent green background color for the ellipse
                    Visible = true  // Make the ellipse visible on the chart
                };
                chart_candlesticks.Annotations.Add(ellipse);  // Add the created ellipse annotation to the chart
            }
        }
        private void button_startStop_Click(object sender, EventArgs e)  // Event handler for the Start/Stop button click
{
            if (selectedWave == null)  // Check if no wave is selected
            {
                MessageBox.Show("Please select a wave first.");  // Show a message to the user if no wave is selected
                return;  // Exit the method without performing any actions
            }

            if (timer_simulation.Enabled)  // Check if the simulation timer is currently running
            {
                // Stop the simulation if it's running
                timer_simulation.Stop();  // Stop the simulation timer
                button_startStop.Text = "Start";  // Change the button text to "Start"
                button_increase.Enabled = true;  // Enable the increase button after stopping the simulation
                button_decrease.Enabled = true;  // Enable the decrease button after stopping the simulation
            }
            else
            {
                // Start simulation
                // Parse input values
                if (!decimal.TryParse(textBox_minPricePercent.Text, out minPricePercent) ||
                    !decimal.TryParse(textBox_maxPricePercent.Text, out maxPricePercent) ||
                    !decimal.TryParse(textBox_stepSize.Text, out simulationStep))
                {
                    MessageBox.Show("Please enter valid numeric values for min price, max price, and step size.");//error message display
                    return;//return statement
                }

                if (minPricePercent < 0 || maxPricePercent > 100)//conditional is min price % less than 0 or max price is greater than 100%
                {
                    MessageBox.Show("Min/Max must be between 0% and 100%.");//error message
                    return;//return statement
                }

                // Validate input values
                if (minPricePercent >= maxPricePercent)//condiitonal is min price % is greater than or equal to max price %
                {
                    MessageBox.Show("Min price must be less than max price.");//error message
                    return;//return statement
                }

                if (simulationStep <= 0)//conditional is step is less or equal to 0
                {
                    MessageBox.Show("Step size must be greater than zero.");//error message
                    return;//return statement
                }

                // Initialize simulation
                simulationDirection = 1;
                button_startStop.Text = "Stop";//initialize to stop
                button_increase.Enabled = false;//initialize to false
                button_decrease.Enabled = false;//initialize to false
                
                // Start the timer
                timer_simulation.Start();
            }
        }

        private void ClearWaveAnnotations()  // Method to clear wave and Fibonacci annotations from the chart
        {
            // Find all annotations in the chart where the tag is either "Wave" or "Fibonacci"
            var annotationsToRemove = chart_candlesticks.Annotations
                .Where(a => a.Tag != null && (a.Tag.ToString() == "Wave" || a.Tag.ToString() == "Fibonacci"))//to string to seperate 
                .ToList();  // Store the matching annotations in a list for removal

            // Iterate through each annotation to remove
            foreach (var annotation in annotationsToRemove)
            {
                chart_candlesticks.Annotations.Remove(annotation);  // Remove the annotation from the chart
            }
        }


        private void timer_simulation_Tick(object sender, EventArgs e)//event handler for ticker
        {
            if (selectedWave == null) return;//conditional is theres no selected wave
    
            SuspendChartUpdates();//call fuction to suspend chat updates
            
            // Calculate new end price
            decimal priceRange = Math.Abs(selectedWave.EndPrice - selectedWave.StartPrice);  // Calculate the absolute price range between the start and end prices of the selected wave
            decimal currentPercent = ((currentPrice - selectedWave.StartPrice) / priceRange) * 100m;  // Calculate the current percentage position within the price range, based on the current price
            currentPercent += simulationDirection * simulationStep;  // Adjust the current percentage by the simulation step, considering the direction of the simulation (positive or negative)
                        
            
            if (currentPercent > maxPricePercent)// Boundary checks
            {
                currentPercent = maxPricePercent; //current percent equality
                simulationDirection = -1; // Reverse direction
            }
            else if (currentPercent < minPricePercent)//conditional is its not equal
            {
                currentPercent = minPricePercent;//equality for current percent and minimum percentage
                simulationDirection = 1; // Reverse direction
            }
            currentPrice = selectedWave.StartPrice + (priceRange * currentPercent / 100m);// Update current price
            
            // Create temp wave for visualization
           Wave tempWave = new Wave                               // Create a new Wave object called tempWave
            {
                StartIndex = selectedWave.StartIndex,              // Set the start index of tempWave from the selected wave
                EndIndex = selectedWave.EndIndex,                  // Set the end index of tempWave from the selected wave
                StartPrice = selectedWave.StartPrice,              // Set the start price of tempWave from the selected wave
                EndPrice = currentPrice,                           // Set the end price of tempWave to the current price
                IsUpWave = selectedWave.IsUpWave,                  // Set whether it's an upward wave based on the selected wave
                ConfirmationCount = selectedWave.ConfirmationCount, // Set the confirmation count of tempWave from the selected wave
                ConfirmationIndices = selectedWave.ConfirmationIndices // Set the confirmation indices of tempWave from the selected wave
            };

            tempWave.CalculateFibonacciLevels();   // Calculate the Fibonacci levels for the tempWave object based on its start and end prices
            DisplayWave(tempWave);                  // Display the tempWave on the chart or UI for visualization
            ResumeChartUpdates();                  // Resume chart updates after modifying or adding new data (e.g., tempWave)
        }

        private void button_increase_Click(object sender, EventArgs e)//event handler for increasingly button clicked
        {
            if (selectedWave == null)//if theres no selected waves
            {
                MessageBox.Show("Please select a wave first.");//error message display
                return;//return statement
            }
            if (!decimal.TryParse(textBox_stepSize.Text, out simulationStep))// Parse step size
            {
                MessageBox.Show("Please enter a valid numeric value for step size.");//error message
                return;//return statement
            }
            decimal priceRange = Math.Abs(selectedWave.StartPrice - selectedWave.EndPrice);// Calculate price range
            decimal currentPercent;// Calculate current percentage based on wave direction
            if (selectedWave.IsUpWave)// conditional is selected wave is up
            {
            currentPercent = ((currentPrice - selectedWave.StartPrice) / priceRange) * 100m;  // If the wave is upward, calculate the percentage by comparing the current price to the start price, relative to the price range
            }
            else
            {
                currentPercent = ((selectedWave.StartPrice - currentPrice) / priceRange) * 100m;  // If the wave is downward, calculate the percentage by comparing the start price to the current price, relative to the price range
            }
            currentPercent += simulationStep;// Increase by step size
            if (currentPercent > 100m)// Ensure we don't exceed max price
            {
                currentPercent = 100m;// Ensure we can equal max price
            }
            
            
            if (selectedWave.IsUpWave)// Calculate new price based on percentage
            {
            currentPrice = selectedWave.StartPrice + (priceRange * currentPercent / 100m);  // If the wave is upward, calculate the current price by adding the proportional price range to the start price
            }
            else
            {
                currentPrice = selectedWave.StartPrice - (priceRange * currentPercent / 100m);  // If the wave is downward, calculate the current price by subtracting the proportional price range from the start price
            }

            
            
            Wave tempWave = CreateWave(selectedWave.StartIndex, selectedWave.EndIndex, selectedWave.StartPrice, currentPrice);// Update the wave visualization
            if (tempWave != null)            // Check if tempWave is not null (i.e., it exists and is valid)
            {
                DisplayWave(tempWave);        // Call DisplayWave method to visualize or process the tempWave if it's valid
            }
        }

        private void button_decrease_Click(object sender, EventArgs e)//function to decrease click
        {
            if (selectedWave == null)//conditional if there is no selected waves
            {
                MessageBox.Show("Please select a wave first.");//error message displayed
                return;//return statement
            }
            
            
            if (!decimal.TryParse(textBox_stepSize.Text, out simulationStep))// Parse step size
            {
                MessageBox.Show("Please enter a valid numeric value for step size.");//error message
                return;
            }
            decimal priceRange = Math.Abs(selectedWave.StartPrice - selectedWave.EndPrice);// Calculate price range
            decimal currentPercent;// Calculate current percentage based on wave direction
            if (selectedWave.IsUpWave)  // Check if the selected wave is an upward wave
            {
                currentPercent = ((currentPrice - selectedWave.StartPrice) / priceRange) * 100m;  // Calculate the current percentage for upward wave based on price range
            }
            else
            {
                currentPercent = ((selectedWave.StartPrice - currentPrice) / priceRange) * 100m;  // Calculate the current percentage for downward wave based on price range
            }

            currentPercent -= simulationStep;// Decrease by step size
            
            if (currentPercent < 0m)// Ensure we don't go below min price
            {
                currentPercent = 0m;//if it equals min price
            }
            
            if (selectedWave.IsUpWave) // Calculate new price based on percentage
            {
            currentPrice = selectedWave.StartPrice + (priceRange * currentPercent / 100m);  // If wave is upward, calculate the price by adding the proportional range to the start price
            }
            else
            {
                currentPrice = selectedWave.StartPrice - (priceRange * currentPercent / 100m);  // If wave is downward, calculate the price by subtracting the proportional range from the start price
            }

            Wave tempWave = CreateWave(selectedWave.StartIndex, selectedWave.EndIndex, selectedWave.StartPrice, currentPrice);// Update the wave visualization
            if (tempWave != null)// Check if tempWave is not null
            {
                DisplayWave(tempWave);// Call DisplayWave method to visualize or process the tempWave
            }
        }

        private void comboBox_waves_SelectedIndexChanged(object sender, EventArgs e)// Event handler for when the selected index of the combo box changes
        {
            if (comboBox_waves.SelectedIndex >= 0)// Check if a valid item is selected in the combo box 
            {
                selectedWave = (Wave)comboBox_waves.SelectedItem;// Retrieve the selected wave from the combo box and cast it to the Wave type
                currentPrice = selectedWave.EndPrice;// Set the current price to the end price of the selected wav
                DisplayWave(selectedWave);// Call DisplayWave method to visualize or process the selected wave
                
                // Update simulation controls
                textBox_minPricePercent.Text = "0";// Set the minimum price percentage text box to 0 (default value)
                textBox_maxPricePercent.Text = "100";// Set the maximum price percentage text box to 100 (default value)
                textBox_stepSize.Text = simulationStep.ToString();// Set the step size text box to the string representation of simulationStep
            }
        }
        /// Overloaded constructor for Form_display, setting it up as a child form of Form.
        /// <param name="data_source_path">Path to the CSV file containing candlestick data.</param>
        /// <param name="startDate">Start date filter for the candlesticks.</param>
        /// <param name="endDate">End date filter for the candlesticks.</param>
        public Form_display(String data_source_path, DateTime startDate, DateTime endDate)// Constructor to initialize the display form with data and date range
        {
            InitializeComponent(); // Initialize form components.
            ResumeLayout(false); // Suspend layout logic to do some changes in the form.

            // Extract just the file name from the full path for display.
            String filename = Path.GetFileName(data_source_path);

            // Set the form's title to the filename for easy identification.
            Text = filename;
            dateTimePicker_startDate.Value = startDate; // Set the start date filter.
            dateTimePicker_endDate.Value = endDate; // Set the end date filter.

            // Load all candlesticks from the specified CSV file.
            allCandlesticks = ACandlestickLoader.LoadFromCsv(data_source_path);

            // Check if the data is already ordered from recent to oldest by comparing the dates of the first two items.
            if (allCandlesticks.Count > 1 && allCandlesticks[0].Date > allCandlesticks[1].Date)
            {
                // If it is in descending order (recent to oldest), reverse it to display oldest to recent.
                allCandlesticks.Reverse();
            }

            // Update the form’s contents based on the specified date range passed to the constructor.
            // This method resumes the layout and shows the form.
            // This method was implemented for reusability as the same code executes when a user changes the dates for displaying a form in the current form
            update_content(startDate, endDate);
        }
        // Event handler for the update button click event to refresh displayed content.
        private void button_update_Click(object sender, EventArgs e)
        {
            update_content((DateTime)dateTimePicker_startDate.Value, (DateTime)dateTimePicker_endDate.Value);    // Update the content based on the current date filter range of this from based on the date pickers
        }
        /// Event handler for the Load Stock button click event. Opens a file dialog
        /// to allow the user to select a CSV file containing stock data.
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event data containing event-specific information.</param>
        private void button_load_Click(object sender, EventArgs e)
        {
            DialogResult r = openFileDialog_load.ShowDialog();  // Displaying the file dialog as show dialog so it is the window with permament focus until terminated.
        }
        /// Event handler triggered after a file is selected in the open file dialog. 
        /// <param name="sender">The file dialog that triggered the event.</param>
        /// <param name="e">Provides event metadata and status.</param>
        private void openFileDialog_load_FileOk(object sender, CancelEventArgs e)//private constructer for open file dialogue
        {
            // Retrieve the file path of the selected file
            String fullPath = openFileDialog_load.FileName;

            // Gets the start date desired
            DateTime startDate = dateTimePicker_startDate.Value;
            // Gets the end date desired
            DateTime endDate = dateTimePicker_endDate.Value;

            // Setting the name of the current form to the text file name. Denoting MAIN as it is the principal form
            Text = "MAIN FORM: " +Path.GetFileName(fullPath);
            // Load all candlesticks from the specified CSV file.
            allCandlesticks = ACandlestickLoader.LoadFromCsv(fullPath);

            // Check if the data is already ordered from recent to oldest by comparing the dates of the first two items.
            if (allCandlesticks.Count > 1 && allCandlesticks[0].Date > allCandlesticks[1].Date)
            {
                // If it is in descending order (recent to oldest), reverse it to display oldest to recent.
                allCandlesticks.Reverse();
            }

            // This method resumes the layout and shows the form.
            update_content(startDate, endDate);
            for (int i = 1; i < openFileDialog_load.FileNames.Length; i++)// instantiate forms for the different filenames selected
            {
                // creating the new form with parameters. We are invoking the overloaded contructor
                Form_display newForm = new Form_display(openFileDialog_load.FileNames[i], startDate, endDate);
                newForm.Show(); // show the form
            }
        }

        /// This function filters the list of candlesticks passes as argument by returning a filtered list containting only the candlesticks within the startingDate and endingDate datetimes.
        /// <param name="candlesticks">List of Candlesticks</param>
        /// <param name="startingDate">Start Date</param>
        /// <param name="endingDate">End Date</param>
        /// <returns></returns>
        public static List<aCandlestick> filterCandlesticks(List<aCandlestick> candlesticks, DateTime startingDate, DateTime endingDate)//list fo candlestick
        {
            var filteredCandlesticks = new List<aCandlestick>();    // Create a list to hold candlesticks within the date range.
            foreach (var candlestick in candlesticks)   // Iterate through each candlestick in the list.
            {
                if (candlestick.Date >= startingDate && candlestick.Date <= endingDate) // If the candlestick's date is within the range, add it to the filtered list.
                {
                    filteredCandlesticks.Add(candlestick);  // Adding the candlestick to the filtered list
                }
            }
            return filteredCandlesticks;    // Returning the filtered list
        }

        /// Adjusts the chart's minimum and maximum Y-axis values based on the candlestick data range and receives a boundlist of Candlestick
        private void normalizeChart(BindingList<aCandlestick> bound_List)
        {
            if (bound_List.Count == 0) return;   // if no there are no candlesticks in the boundlist then we return to avoid any running time issue.
            
            // Get the minimum and maximum candlestick prices for Y - axis scaling.
            // We use arrow functuoins and the .Min, .Max methods from boudnlist
            double min = (double)bound_List.Min(cs => cs.Low); // gets the minimum 
            double max = (double)bound_List.Max(cs => cs.High); // gets the maximum

            // Adding some margin to the data so the candlestics Graphical representation does not start right at the y axis
            min = Math.Floor(0.98 * min); // removing 2% from the minimum
            max = Math.Ceiling(1.02 * max); // adding 2% to the maximum

            // Set the chart's Y-axis minimum and maximum values
            chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY.Minimum = min; // set the minimum
            chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY.Maximum = max; // set the maximum

            // Calculate and set the interval based on the range for better visibility
            chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY.Interval = Math.Ceiling((max - min) / 10); // Divide by 10 for a smoother interval
        }

        private void update_content(DateTime startingDate, DateTime endingDate)  // Updates the content displayed in the chart based on the specified date range.
        {
            List<aCandlestick> filteredCandlesticks = filterCandlesticks(allCandlesticks, startingDate, endingDate);    // Filter the candlesticks to match the selected date range.
            BindingList<aCandlestick> bound_List = new BindingList<aCandlestick>(filteredCandlesticks);  // Creating a binding list using the filtered candlesticks. This is will be the datasource
            chart_candlesticks.DataSource = bound_List;  // Set the datasource of the chart in the form display to our boundlist of candlesticks
            normalizeChart(bound_List);  // Divide chart range by 10 splits for a smoother interval
            chart_candlesticks.Annotations.Clear(); // Clear previous annotations to avoid duplicating them
            identify_peaks_and_valleys(bound_List);  // identify peaks and valleys but set them to be hidden
            ResumeLayout(true); // Resume Layout of the form as we have done all changes. If Layout was already Resumed this command does not affect.
            this.Show();    // Show the form
            waves.Clear();  // Clear the list of waves (reset the waves collection)
            comboBox_waves.Items.Clear();   // Clear the items in the waves combo box (reset the UI list)
            selectedWave = null;    // Reset the selected wave (no wave is selected now)
        }
        
        /// <param name="filteredCandlesticks">bound_list (filtered)</param>
        private void identify_peaks_and_valleys(BindingList<aCandlestick> filteredCandlesticks) // Identifies peaks and valleys in the filtered candlestick list
        {
            // Loop through each candlestick in the list, starting from the second element and ending before the last element (to allow comparison with neighboring elements)
            for (int i = 1; i < filteredCandlesticks.Count - 1; i++)
            {
                var current = filteredCandlesticks[i];  // Set 'current' to the candlestick at index i, which is the one being evaluated.
                var previous = filteredCandlesticks[i - 1]; // Set 'previous' to the candlestick before the current one, for comparison purposes.
                var next = filteredCandlesticks[i + 1]; // Set 'next' to the candlestick after the current one, for comparison purposes.
                // Check if the current candlestick represents a peak by comparing its high value with the high values of the previous and next candlesticks.
                if (current.High > previous.High && current.High > next.High)
                {
                    add_Text_Annotation(chart_candlesticks, "Peak", current.High, i, Color.Green);    // Create a hidden annotation for a peak
                    add_Line_Annotation(chart_candlesticks, current.High, i, Color.Green);    // Draw a horizontal red line at the valley (hidden)
                }
                else if (current.Low < previous.Low && current.Low < next.Low)  // Check for a valley: Current low is less than both previous and next lows
                {
                    add_Text_Annotation(chart_candlesticks, "Valley", current.Low, i, Color.Red); // Create a hidden annotation for a valley
                    add_Line_Annotation(chart_candlesticks, current.Low, i, Color.Red);   // Draw a horizontal red line at the valley (hidden)
                }
            }
        }
        /// <param name="chart">The chart where the annotation will be applied.</param>
        /// <param name="text">The label for the annotation, such as "Peak" or "Valley".</param>
        /// <param name="price">The price level at which the annotation should be placed.</param>
        /// <param name="index">The x-axis position corresponding to the annotation.</param>
        /// <param name="color">The color of the annotation text.</param>
        private void add_Text_Annotation(Chart chart, string text, decimal price, int index, Color color) //function to add text annotations
        {
            var newAnnotation = new TextAnnotation  // Create a new text annotation for marking a peak or valley point.
            {
                Text = text,            // Set the annotation text (e.g., "Peak" or "Valley").
                ForeColor = color,      // Set the color of the annotation text.
                Font = new Font("Arial", 8),    // Set the font style and size for the annotation text.
                TextStyle = TextStyle.Frame,    // Define the annotation style as framed text.
                Alignment = System.Drawing.ContentAlignment.TopCenter,  // Set the alignment of the annotation text to be centered at the top.
                AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX, // Link the annotation to the X-axis of the specified chart area.
                AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY, // Link the annotation to the Y-axis of the specified chart area.
                AnchorX = index + 1, // Set the anchor position on the X-axis slightly offset to align with the candlestick.
                Y = (double)price, // Set the position on the Y-axis to the specified price level.
                Visible = false // Make the annotation hidden initially, to be shown when needed.
            };

            // Adjust the position slightly above the price for peak annotations.
            if (text == "Peak")newAnnotation.Y += newAnnotation.Y * (0.02);// show the annotation 2% above the actual peak
            
            // Adjust the position slightly below the price for valley annotations.
            else if (text == "Valley")
            {
                // show the annotation 2% below the actual peak
                newAnnotation.Y -= newAnnotation.Y * (0.02); ;
            }
            // Add the configured annotation to the chart's annotations collection.
            chart_candlesticks.Annotations.Add(newAnnotation);
        }
        /// Adds a hidden horizontal line annotation to the chart to mark a specific price level, such as the price of a peak or valley.
        /// <param name="chart">The chart where the annotation will be added.</param>
        /// <param name="price">The price level at which to place the line.</param>
        /// <param name="index">The x-axis index position for the annotation.</param>
        /// <param name="color">The color of the line annotation.</param>
        private void add_Line_Annotation(Chart chart, decimal price, int index, Color color) // Adds a line annotation to the chart
        {
            var newLineAnnotation = new HorizontalLineAnnotation // new horizontal line annotation to mark the price level.
            {
                AxisX = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisX, //link the line annotation to the X-axis of the specified chart area.
                AxisY = chart_candlesticks.ChartAreas["ChartArea_OHLC"].AxisY, //link the line annotation to the Y-axis of the specified chart area.
                ClipToChartArea = "ChartArea_OHLC",     // Restrict the annotation to be drawn within the specified chart area.
                Y = (double)price,                      //set the Y position of the line to the specified price level.
                LineColor = color,                      //set the color of the line annotation.
                LineWidth = 1,                          //set the width of the line.
                IsInfinitive = true,                    //make the line infinite, stretching horizontally across the entire chart.
                Visible = false                         //start with the annotation hidden; it can be shown when necessary.
            };
            chart_candlesticks.Annotations.Add(newLineAnnotation);// Add the configured line annotation to the chart's annotations collection.
        }
        private void Form_display_Load(object sender, EventArgs e)  
        {
            // Event handler triggered when the form loads
        }
        private void dataGridView_candlesticks_CellContentClick(object sender, DataGridViewCellEventArgs e) 
        {
            // Event handler triggered when a cell in the candlestick data grid is clicked
        }
        private void chart1_Click(object sender, EventArgs e)
        {
            // Event handler triggered when the chart is clicked
        }
        private void dateTimePicker_startDate_ValueChanged(object sender, EventArgs e)
        {
            // Event handler triggered when the start date in the date picker is changed
        }
        private void label1_Click(object sender, EventArgs e)
        {
            // Event handler triggered when label1 is clicked
        }
        private void label2_Click(object sender, EventArgs e)
        {
            // Event handler triggered when label2 is clicked
        }
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event data.</param>
        private void button_showPeaks_Click(object sender, EventArgs e) // Event handler triggered when the "Show Peaks" button is clicked
        {
            foreach (var annotation in chart_candlesticks.Annotations)// Iterate through each annotation in the chart's annotation collection
            {
                // Check if the annotation is a peak by its color or label
                if (annotation is TextAnnotation textAnnotation && textAnnotation.Text == "Peak")textAnnotation.Visible = !textAnnotation.Visible; // Toggle visibility
                
                // Check if the annotation is a peak line annotation by its color
                else if (annotation is LineAnnotation lineAnnotation && lineAnnotation.LineColor == Color.Green)lineAnnotation.Visible = !lineAnnotation.Visible; // Toggle visibility
                
            }
        }
        /// Toggles the visibility of annotations that mark valleys on the chart.
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event data.</param>
        private void button_showValleys_Click(object sender, EventArgs e)   //private void function to display valleys
        {
            foreach (var annotation in chart_candlesticks.Annotations)// Iterate through each annotation in the chart's annotation collection
            {
                // Check if the annotation is a valley text annotation by verifying the text label
                if (annotation is TextAnnotation textAnnotation && textAnnotation.Text == "Valley")textAnnotation.Visible = !textAnnotation.Visible; // Toggle visibility
                // Check if the annotation is a valley line annotation by its color
                else if (annotation is LineAnnotation lineAnnotation && lineAnnotation.LineColor == Color.Red)lineAnnotation.Visible = !lineAnnotation.Visible; // Toggle the visibility of the valley line annotation
            }
        }
    }
}