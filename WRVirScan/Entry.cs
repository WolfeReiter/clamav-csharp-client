using System;
using System.Diagnostics;

namespace WolfeReiter.AntiVirus.ConsoleDemo
{
	/// <summary>
	/// Container for application entry point.
	/// </summary>
	public sealed class Entry
	{
		private Entry(){}
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		public static void Main(string[] args)
		{	
			log4net.Config.DOMConfigurator.Configure();
			try
			{
				ScanJob job = new ScanJob(  ScanArgs.Parse( args ) );
				job.Run();
			}
			catch(ArgumentException ex)
			{
				Debug.WriteLine(ex);
				Console.WriteLine(ex.Message);
			}
		}
	}
}