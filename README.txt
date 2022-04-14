This code was written for .NET 6 and can be run by the command 'dotnet run Program.cs'

This program simulates 10 hours of a Mars Rover recieving temperature readings. To do this, a random number is generated for each sensor's reading and every 'hour' these results are compiled by one of the threads into a report that is printed to the screen. Very few calculations are actually done, just data management and after testing both manually and programmatically, I have concluded that this program is fully accurate in its results.

This program takes 8 readings every minute into a 2d array. By using a 2d array, there is no way for a sensor to overwrite another's reading, thus this is completely thread-safe.

Every hour, the results are compiled and calculated. First, a high and low list are created with the highest and lowest values from every minute. Then, these are used to find the top 5 highest and lowest temperatures for the hour. Then, it checks every 10 minute interval for the past hour to find the highest delta and saves the value and index (minute) for this value. These results are all then printed to the screen. As this takes only a few moments, the first thread created (thread 0) handles printing everything. This will not interfere with the ability to make readings as readings are only made every minute.

This program is also as efficient as I could make it, both time and space wise. The only time loss being from the Thread.Sleep() calls added to simulate the reading every 1 minute from the problem.

In addition, due to this program being written using C#, the built-in lock() command will automically cause any thread stuck behind a lock to wait until the lock is released before automically obtaining the lock itself and continuing. This means that there is always progress being made.

