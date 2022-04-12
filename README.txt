This code was written for .NET 6 and can be run by the command 'dotnet run Program.cs'

This program simulates 10 hours of a Mars Rover recieving temperature readings. To do this, a random number is generated for each sensor's reading and every 'hour' these results are compiled by one of the threads into a report that is printed to the screen. Very few calculations are actually done, just data management and after testing both manually and programmatically, I have concluded that this program is fully accurate in its results.

This program is also as efficient as I could make it, both time and space wise. The only time loss being from the Thread.Sleep() calls added to simulate the reading every 1 minute from the problem.

In addition, due to this program being written using C#, the built-in lock() command will automically cause any thread stuck behind a lock to wait until the lock is released before automically obtaining the lock itself and continuing. This means that there is always progress being made.