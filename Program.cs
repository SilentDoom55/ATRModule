using System;

namespace ATRModule
{
    internal class Program
    {
        static int thermCount = 8;                                  // Number of thermometers (threads)
        static int interval = 10;                                   // Interval in minutes to find largest difference
        static int runTime = 600 + 1;                               // How long the simulation runs in minutes (+1)
        static int threadDelay = 50;                                // How long the threads wait between readings (60000 is accurate to real life situation)
        static int lowRead = -100;                                  // The lowest reading a thermometer can read
        static int highRead = 70;                                   // The highest reading a thermometer can read
        static Random rnd = new Random();                           // Random Number Generator for creating readings
        static List<int> highestTemps = new List<int>();            // List of the highest temperatures for a given hour
        static List<int> lowestTemps = new List<int>();             // List of the lowest temperatures for a given hour
        static int[,] currentTemps = new int[thermCount, runTime];  // List of all temperature readings
        static int[] lowTemps = new int[runTime];                   // List of all of the lowest temperatures for each minute
        static int[] highTemps = new int[runTime];                  // List of all of the highest temperatures for each minute
        static int tenMinuteInterval = -1;                          // The start point for the 10 minute interval, initialized at -1
        static int tenMinuteDif = -1;                               // The numerical difference for the interval, initialized at -1
        static object print = 0;                                    // Arbitrary object to lock printing and calculations

        static void Main(string[] args)
        {
            for(int i = 0; i < runTime; i++)
            {
                lowTemps[i] = highRead + 1;
                highTemps[i] = lowRead - 1;
            }
            Console.WriteLine("Starting");
            // Simulating a "Controller" for the thermometers, same as placing in main
            ThermometerController();
        }

        private static void ThermometerController()
        {
            Thread[] threads = new Thread[thermCount];

            // Creates and starts all thermometers
            for (int i = 0; i < thermCount; i++)
            {
                int tI = i;
                threads[i] = new Thread(new ThreadStart(() => Thermometer(tI)));
                threads[i].Start();
            }

            // Waits for thermometers to finish
            for (int i = 0; i < thermCount; i++)
                threads[i].Join();

            Console.WriteLine("Simulation Concluded");
        }

        // Main function each thermometer runs
        private static void Thermometer(int threadNum)
        {
            // Initializes the current time as the thread number
            // This is incremented by the number of threads so each thermometer only has to deal with 1/8 of the readings
            int currTime = 0;

            // Continues to loop until the simulation ends
            while (currTime < runTime)
            {
                // Gets a reading and then Adds that reading to the master list before sleeping
                int temp = GetRead();
                AddRead(temp, currTime, threadNum);
                Thread.Sleep(threadDelay);
                
                // If a thread lands on the hour mark, it is in charge of printing the report for the hour
                if (currTime % 60 == 0 && currTime != 0 && threadNum == 0)
                {
                    // Locks print to guarantee that only one thread is doing calculations at a time
                    lock (print)
                    {
                        // Performs various calculations and then prints the hourly report
                        GetHighLow(currTime - 60);
                        CheckHigh(currTime - 60);
                        CheckLow(currTime - 60);
                        CheckInterval(currTime - 60);
                        PrintHourReport(currTime);
                    }
                }
                // Increments the current time by the number of thermometers
                currTime++;
            }
            
        }

        // Helper function to get a random reading
        private static int GetRead()
        {
            return rnd.Next(lowRead, highRead + 1);
        }

        // Helper function to add a reading to the current Readings
        private static void AddRead(int temp, int currTime, int threadNum)
        {
            lock(currentTemps)
            {
                currentTemps[threadNum, currTime] = temp;
            }
        }

        // Condenses the main matrix to only the highest and lowest values for each minute
        public static void GetHighLow(int start)
        {
            // Locks relevant Lists
            lock(currentTemps)
            {
                lock(highTemps)
                {
                    lock(lowTemps)
                    {
                        // Loops through each minute and then each sensor for each minute
                        for (int i = start; i < start + 60; i++)
                        {
                            for(int j = 0; j < thermCount; j++)
                            {
                                // Gets the highest and lowest for the minute
                                if(currentTemps[j, i] > highTemps[i])
                                {
                                    highTemps[i] = currentTemps[j, i];
                                }

                                if (currentTemps[j, i] < lowTemps[i])
                                {
                                    lowTemps[i] = currentTemps[j, i];
                                }
                            }
                        }
                    }
                }
            }
        }
        // Helper function to Find the top 5 temperatures for a given start time
        private static void CheckHigh(int start)
        {
            // Locks relevant Lists
            lock(highestTemps)
            {
                lock(highTemps)
                { 
                    // Loops through an hour from the start position
                    for(int i = start; i < start + 60; i++)
                    {
                        // Makes sure that there is 5 temperatures there already
                        if (highestTemps.Count == 5)
                        {
                            // Loops through all of the highest temperatures
                            foreach (int highTemp in highestTemps)
                            {
                                // If the highTemp is higher than a highest temp, it removes it and adds the new temp
                                if (highTemps[i] > highTemp)
                                {
                                    highestTemps.Remove(highTemp);
                                    highestTemps.Add(highTemps[i]);
                                    // Sorts so that the lowest temp is always at index 0
                                    highestTemps.Sort();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // If there are not 5 highestTemps, it just adds it
                            highestTemps.Add(highTemps[i]);
                            highestTemps.Sort();
                        }
                    }
                }
            }
        }

        // Helper function to Find the bottom 5 temperatures for a given start time
        private static void CheckLow(int start)
        {
            // Locks relevant Lists
            lock (lowestTemps)
            {
                lock (lowTemps)
                {
                    // Loops through an hour from the start position
                    for (int i = start; i < start + 60; i++)
                    {
                        // Makes sure that there is 5 temperatures there already
                        if (lowestTemps.Count == 5)
                        {
                            // Loops through all of the lowest temperatures
                            foreach (int lowTemp in lowestTemps)
                            {
                                // If the currentTemp is lower than a low temp, it removes it and adds the new temp
                                if (lowTemps[i] < lowTemp)
                                {
                                    lowestTemps.Remove(lowTemp);
                                    lowestTemps.Add(lowTemps[i]);
                                    // Sorts and then Reverses so that the highest temp is always at index 0
                                    lowestTemps.Sort();
                                    lowestTemps.Reverse();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // If there are not 5 highestTemps, it just adds it
                            lowestTemps.Add(lowTemps[i]);
                            lowestTemps.Sort();
                            lowestTemps.Reverse();
                        }
                    }
                }
            }
        }

        // Helper function to find the interval with the largest difference
        private static void CheckInterval(int start)
        {
            // Locks relevant List
            lock (currentTemps)
            {
                // Loops from start for the next hour, minus the last 10 minutes
                for (int i = start; i < start + 60 - interval; i++)
                {
                    // Finds the largest difference and saves the value and minute value
                    int dif = Math.Abs(highTemps[i] - lowTemps[i + interval]);
                    int dif2 = Math.Abs(highTemps[i + interval] - lowTemps[i]);
                    if (dif > tenMinuteDif)
                    {
                        tenMinuteDif = dif;
                        tenMinuteInterval = i;
                    }
                    if (dif2 > tenMinuteDif)
                    {
                        tenMinuteDif = dif2;
                        tenMinuteInterval = i;
                    }
                }
            }
        }

        // Function to print out the hourly report given the current Time
        private static void PrintHourReport(int currTime)
        {
            // Locks the relevant List
            lock (highestTemps)
            {
                Console.WriteLine("Highest Temps for Hour {0}", (int)currTime / 60);
                foreach (int temp in highestTemps)
                {
                    Console.Write("{0}\t", temp);
                }
                Console.Write("\n");
                // Clears the list for the next hour
                highestTemps.Clear();
            }

            // Locks the relevant List
            lock (lowestTemps)
            {
                Console.WriteLine("Lowest Temps for Hour {0}", (int)currTime / 60);
                foreach (int temp in lowestTemps)
                {
                    Console.Write("{0}\t", temp);
                }
                Console.Write("\n");
                // Clears the list for the next hour
                lowestTemps.Clear();
            }

            Console.WriteLine("Largest {0} Minute Interval for Hour {1}", interval, (int)currTime / 60);
            Console.WriteLine("From {0} to {1} minutes, difference of {2}", tenMinuteInterval, tenMinuteInterval + interval, tenMinuteDif);
            // Resets variables for the next hour
            tenMinuteInterval = -1;
            tenMinuteDif = -1;

            Console.WriteLine("\n");
        }

    }
}