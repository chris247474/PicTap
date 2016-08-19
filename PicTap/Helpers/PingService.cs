using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Text;

namespace PicTap
{
	public static class PingService
	{
		public static long GetPingRate(string address) {
			try
			{
				var ping = new Ping();
				//IPAddress destIP = IPAddress.Parse(address);
				//IPAddress.TryParse(address, out destIP);
				PingOptions options = new PingOptions();
				options.DontFragment = true;
				string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
				byte[] buffer = Encoding.ASCII.GetBytes(data);
				int timeout = 120;
				var pingReply = ping.Send(address, timeout, buffer, options);
				Console.WriteLine("Ping status: {0}", pingReply.Status);
				return pingReply.RoundtripTime;
			}
			catch (Exception e){
				Console.WriteLine("PingService error: {0}", e.Message);
			}
			return 0;
		}

	}
}

