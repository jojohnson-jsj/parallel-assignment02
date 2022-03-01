using System;
using System.Threading;
using System.Collections.Generic;


/* Overall Solution
   ----------------
	The solution I've chosen is extremely similar to the approach discussed in class for the Prisoners/Warden problem and is as
	follows:

	Of all of the guests, one is chosen as the leader. As guests are randomly sent into the labyrinth one at a time,

		If the guest is NOT the leader, they will NEVER replace the cupcake and:

			If the guest has not gotten a chance to eat a cupcake
				If there is a cupcake in the labyrinth 
					The guest will eat the cupcake and remember that they did
				Otherwise, if there is no cupcake in the labyrinth
					The guest will remember they have not gotten a chance to eat a cupcake

			If the guest has eaten a cupcake already then they will do nothing


		If the guest IS the leader:
			
			If the leader finds the cupcake missing when they visit, they will increase their personal count of how many guests
			have visited the labyrinth by 1. They will then replace the cupcake. If their count of the visited guests >= N 
			(where N = 10 in this case) then they will tell the Minotaur that all the guests have visited the labyrinth.

			If the leader finds a cupcake present when they visit, they will do nothing.
*/
namespace Minotaur01
{
	class MinotaurBirthday
	{
		private static Labyrinth maze = new Labyrinth();

		// This array (visited_freq) counts how many times an individual guest has visited the labyrinth. The purpose of this
		// array is NOT to determine whether all of the guests have visited the labyrinth for the sake of the riddle,
		// and it is not used for that funciton. Instead it is used to ensure that we don't continue to allow new guests
		// into the labyrinth repeatedly in perpetuity. Once all N guests have visited the labyrinth a minimum of N
		// times (where N = 10 in this case), the minotaur will stop allowing guests in.
		private static int[] visited_freq = new int[10];

		// This array is used for an individual guest to be able to "remember" whether they have visited the labyrinth or not.
		// It is never used by one guests to check whether any guest but themselves have visited.
		private static bool[] visited = new bool[10];

		// This dictionary maps numbers to individual threads for the sake of being able to updated the two arrays above.
		private static IDictionary<int, Thread> threadDictionary = new Dictionary<int, Thread>();

		// This lock is used in critical sections (the labyrinth itself) to ensure no two guests are there at once.
		private static Mutex mutexLock = new Mutex();

		// This counter is used by the leader to "remember" how many times they (the leader) have determined a guest has entered
		// the labyrinth. It is initalized to 1 because if the leader is keeping track of how many guests they've observed have 
		// entered the labyrinth using the solution provided above then that means they have also entered the labyrinth.
		private static int counter = 1;

		static void Main(string[] args)
		{
			Random rnd = new Random();

			// 10 threads are initially created to be the 10 guests.
			Thread guest0 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(0, guest0);
			guest0.Name = "leader/guest0";

			Thread guest1 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(1, guest1);
			guest1.Name = "guest1";

			Thread guest2 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(2, guest2);
			guest2.Name = "guest2";

			Thread guest3 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(3, guest3);
			guest3.Name = "guest3";

			Thread guest4 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(4, guest4);
			guest4.Name = "guest4";

			Thread guest5 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(5, guest5);
			guest5.Name = "guest5";

			Thread guest6 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(6, guest6);
			guest6.Name = "guest6";

			Thread guest7 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(7, guest7);
			guest7.Name = "guest7";

			Thread guest8 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(8, guest8);
			guest8.Name = "guest8";

			Thread guest9 = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
			threadDictionary.Add(9, guest9);
			guest9.Name = "guest9";

			Console.WriteLine("======CREATED 10 GUESTS======\n");

			int thread;

			// AllIsVisited checks the visited_freq array to check whether or not all N guests have visited the 
			// labyrinth a minimum of N times. It is NOT used by the guests to determine when to tell the minotaur
			// that they have all visited, and is instead just used here for the sake of preventing an infinite loop.
			// maze.AllGuestsEntered is set to false by default and switched to true when the leader believes all the 
			// guests have entered the labyrinth, at which point this loop will no longer be run.
			while (!AllVisited() && !maze.AllGuestsEntered)
			{
				// This chooses a random number thread from 0-9, inclusively.
				thread = rnd.Next(0, 10);

				// This increments the number of times the chosen thread has visited the labyrinth.
				visited_freq[thread]++;				

				// This uses the Dictionary to get the appropriate thread based on the number randomly chosen above.
				Thread guestEntering = threadDictionary[thread];
				string threadName = guestEntering.Name;

				Thread.Sleep(10);

				// The guest this thread corresponds to enters the labyrinth.
				guestEntering.Start(thread);

				// Once the execution of this thread has stopped, it will die an can never be restarted again. Therefore,
				// lines 105 - 107 create a brand new thread, give it the name of the thread that ran and died (e.g. "guest5"),
				// and update the dictionary accordingly for future use. Each thread dies after execution but the "guest" lives on.
				guestEntering = new Thread(new ParameterizedThreadStart(EnterLabyrinth));
				threadDictionary[thread] = guestEntering;
				guestEntering.Name = threadName;
			}

			// Thread.Sleep(50) here is used to ensure that the main thread will only print the below messages after all the other threads
			// have died. This is due to the nature of printing to the screen and how what is printing will not always be printed at the 
			// precisely correct time on its own.
			Thread.Sleep(50);
			if (maze.AllGuestsEntered)
			{
				Console.WriteLine("***ALL OF THE GUESTS HAVE ENTERED THE GRACIOUS MINOTAUR'S LABYRINTH!***");
				return;
			}
			else
			{
				Console.WriteLine(":( Failed to recognize that all the guests entered the maze :(");
			}
		}

		// This method iterates through visited_freq. If any of the N guests have not visited the labyrinth at least N
		// times, it returns false. Otherwise, if all of the guests have visited >= N times, it returns true.
		private static bool AllVisited()
		{
			for (int i = 0; i < 10; i++)
			{
				if (visited_freq[i] < 10)
					return false;
			}

			return true;
		}

		// This function simulates a guest's experience in the labyrinth and can only be accessed by one thread/guest at a 
		// time accordingly.
		static void EnterLabyrinth(object cur_thread)
		{
			if (maze.AllGuestsEntered)
				return;

			mutexLock.WaitOne();

			Console.WriteLine("\n" + Thread.CurrentThread.Name + " has entered the labyrinth");
			Console.WriteLine("================================");

			// If the current thread is the leading thread (guest1)
			if (Thread.CurrentThread.Name.Equals("leader/guest0"))
			{
				if (maze.CheckCupcakeIsMissing())
				{
					counter++;
					Console.WriteLine("+The leader found the cupcake missing, so now they know that " + counter + " guests have visited the labyrinth (including themselves)");
					maze.ReplaceOrLeaveCupcake();
				}
				else
				{
					Console.WriteLine("+The leader found the cupcake present");
					Console.WriteLine("+The leader has left the cupcake alone");
				}				

				if (counter >= 10)
					maze.AllGuestsEntered = true;
			}

			// For all other non-leader guests:
			else
			{
				if (!visited[(int)cur_thread])
				{
					Console.WriteLine("-This was either the first time this guest has entered the Labyrinth, or they weren't able to eat a cupcake the last time ");
					if (maze.CheckCupcakeIsMissing())
						Console.WriteLine("*This guest finds the cupcake missing so they do nothing");
					else
						maze.EatCupcake();
				}
				else
				{
					Console.WriteLine("-This was not the first time this guest entered the labyrinth and the last time they ate a cupcake");
					Console.WriteLine("*This guest does nothing");
				}
			}

			Console.WriteLine("-" + Thread.CurrentThread.Name + " has exited the labyrinth\n");
			mutexLock.ReleaseMutex();
		}
	}

	// This class serves as the mechanism by which all guests interact with the 1 maze and its cupcake. It 
	// has a bool, "cupcake", that is set to on or off by guests depending on the action they take and it is
	// set to true by default in accordance to the problem definition. It also has a public bool,
	// AllGuestsEntered, that is set to false by default but switched to true once the leader/guest0 determines
	// all of the guests have been able to visit the labyrinth at least once.
	class Labyrinth
	{
		private bool cupcake;
		public bool AllGuestsEntered = false;

		public Labyrinth()
		{
			this.cupcake = true;
		}

		public void EatCupcake()
		{
			this.cupcake = false;
			Console.WriteLine("-The guest has eaten a cupcake");
		}

		public void ReplaceOrLeaveCupcake()
		{
			this.cupcake = true;
			Console.WriteLine("+The leader has replaced the cupcake");
		}

		public bool CheckCupcakeIsMissing()
		{
			if (!this.cupcake)
				return true;

			return false;
		}
	}
}
