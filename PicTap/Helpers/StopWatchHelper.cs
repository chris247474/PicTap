using System;
using System.Threading;
using System.Threading.Tasks;

namespace PicTap
{
	public static class StopWatchHelper
	{
		static TimeSpan StartTime = new TimeSpan(0, 0, 0, 0, 0);

		public static bool IsTimerRunning()
		{
			if (StartTime.Days == 0 && StartTime.Hours == 0 && StartTime.Minutes == 0 && StartTime.Seconds == 0 && StartTime.Milliseconds == 0)
			{
				return false;
			}
			else {
				return true;
			}
		}

		public static void StartTimer()
		{
			StartTime = new TimeSpan(0);
			StartTime = DateTime.Now.TimeOfDay;
			Console.WriteLine("Started timer at {0}", StartTime);
		}

		public static TimeSpan StopTimer()
		{
			var timediff = IsTimerRunning() ? DateTime.Now.TimeOfDay.Subtract(StartTime) : new TimeSpan(0, 0, 0, 0, 0);
			Console.WriteLine("Stopping timer at {0}, call time was {1}", DateTime.Now.TimeOfDay, timediff);
			StartTime = new TimeSpan(0, 0, 0, 0, 0);
			return timediff;
		}

		public static async Task CountdownToAction(Action doThis) {
			
		}

	}
}

