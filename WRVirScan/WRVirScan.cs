using System;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// Container for application entry point.
	/// </summary>
	public class WRClamdScan
	{
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		public static void Main(string[] args)
		{	
			log4net.Config.DOMConfigurator.Configure();
			ScanJob job = new ScanJob(  ScanArgs.Parse( args ) );
			job.Run();
		}
	}
}