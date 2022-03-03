using System;
using System.Threading;
using System.Collections.Generic;

namespace Minotaur02
{
	class MinotaurVase
	{
		// This dictionary will be used to map the specific thread to an index in a visited array via its name.
		private static IDictionary<string, int> dictionary = new Dictionary<string, int>();

		// This lock ensures that only one guest will ever be viewing the vase at a time.
		private static Mutex showRoomLock = new Mutex();

		// This bool is the sign at the showroom door informing a guest of whether or not they may enter.
		private static bool isAvailable = true;
		private static bool[] visited = new bool[5];

		static void Main(string[] args)
		{			
			Thread guest0 = new Thread(new ThreadStart(RoamCastle));
			dictionary.Add("guest0", 0);
			guest0.Name = "guest0";

			Thread guest1 = new Thread(new ThreadStart(RoamCastle));
			dictionary.Add("guest1", 1);
			guest1.Name = "guest1";

			Thread guest2 = new Thread(new ThreadStart(RoamCastle));
			dictionary.Add("guest2", 2);
			guest2.Name = "guest2";

			Thread guest3 = new Thread(new ThreadStart(RoamCastle));
			dictionary.Add("guest3", 3);
			guest3.Name = "guest3";

			Thread guest4 = new Thread(new ThreadStart(RoamCastle));
			dictionary.Add("guest4", 4);
			guest4.Name = "guest4";


			Console.WriteLine("======CREATED 5 GUESTS======\n");

			// All 5 guests start off by roaming around the castle.
			guest0.Start();
			guest1.Start();
			guest2.Start();
			guest3.Start();
			guest4.Start();

			Console.WriteLine("\nAll guests are roaming the castle\n");

			// If any of the guests have yet to visit the vase, I want the main thread to sleep so that
			// it'll only print line 60 once all the guests are done.			
			while (!AllVisited())
			{
				Thread.Sleep(20);
			}

			Console.WriteLine("\n\n***ALL OF THE GUESTS HAVE VIEWED THE GENEROUS MINOTAUR'S VASE!***");
		}

		private static void ViewVase()
		{
			Console.WriteLine("\n\n" + "+++" + Thread.CurrentThread.Name + " is VIEWING the vase+++");
			Thread.Sleep(20);
			Console.WriteLine("\n" + "+++" + Thread.CurrentThread.Name + " is DONE viewing the vase+++\n");

			// Once the guest is done, they will set the sign to AVAILABLE again.
			isAvailable = true;
		}

		private static void RoamCastle()
		{
			// This bool indicates whether this specific guest has viewed the vase or not. Until the guest has
			// viewed the vase, they will periodically check the sign at the door to see if it is available. Once
			// they've viewed the vase, they will be done.
			bool viewedVase = false;

			Random rnd = new Random();

			int sleepTime;

			while (!viewedVase)
			{
				bool start = false;

				// The guest will roam the castle for a random period of time before deciding to check whether they can view
				// the vase.
				sleepTime = rnd.Next(10, 41);

				Thread.Sleep(sleepTime);

				// After the guest is done roaming, they will check to see if the sign is AVAILABLE.
				start = CheckSign();

				// If the sign was AVAILABLE, the guest will enter the showroom, which is locked to ensure only one guest at a time enters.
				// They will also mark that they have viewed the vase so that they will not continue trying to visit the showroom in
				// perpetuity. 
				if (start)
				{
					Console.WriteLine("\n" + "The sign is AVAILABLE, so " + Thread.CurrentThread.Name + " will go view the vase");
					showRoomLock.WaitOne();
					ViewVase();
					showRoomLock.ReleaseMutex();
					viewedVase = true;

					visited[dictionary[Thread.CurrentThread.Name]] = true;
				}
				// If it's BUSY, the guest will just continue to roam the castle again
				else
				{
					Console.WriteLine("\n" + "The sign is BUSY, so " + Thread.CurrentThread.Name + " will continue roaming the castle");
				}
			}
		}

		// AllVisited() is just checking to see when all of the guests have viewed the vase so that the main thread can print out the completion
		// text.
		private static bool AllVisited()
		{
			for (int i = 0; i < 5; i++)
			{
				if (!visited[i])
					return false;
			}

			return true;
		}

		// CheckSign() returns the state of isAvailable when the method was called. If isAvailable was true, it sets it to false to ensure that
		// no more than one thread ever read isAvailable as true. If a thread reads it as true, it means that it's going to go view the vase
		// and it should therefore be set to false for the next thread that decides to check it.
		private static bool CheckSign()
		{
			if (isAvailable)
			{
				isAvailable = false;
				return true;
			}				

			return false;
		}
	}
}
