using System;
using System.Globalization;
using System.Reflection;

namespace WolfeReiter.AntiVirus.ConsoleDemo
{
	/// <summary>
	/// ScanJob manages a virus scanning job.
	/// </summary>
	public class ScanJob
	{
		#region resource name constants
		private const string STR_RUN_DEBUG_MSG		= "ScanJob.Run_DebugMessage";
		private const string STR_HELP_TEMPLATE		= "ScanJob.WriteHelp";
		private const string STR_WRITE_VERSION		= "ScanJob.WriteVersion";
		private const string STR_TIMESPAN_TEMPLATE	= "ScanJob.WriteStats_TimeSpan";
		private const string STR_STATS_TEMPLATE		= "ScanJob.WriteStats_Template";
		#endregion

		private ScanArgs _args;
		private int _scancount, _vircount;
		private IVirusScanAgent _agent;

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <exception cref="ArgumentNullException">Throws when args is null.</exception>
		/// <param name="args">ScanArgs. Cannot be null.</param>
		public ScanJob(ScanArgs args)
		{
			if(args==null)
				throw new ArgumentNullException("args");

			_args		= args;
			_scancount	= 0;
			_vircount	= 0;

			_agent = new ClamdStreamAgent(args.Host, args.Port, args.Verbose);
			_agent.ItemScanCompleted += new ScanCompletedEventHandler(_agent_ItemScanCompleted);
			_agent.VirusFound		 += new ScanCompletedEventHandler(_agent_VirusFound);
		}

		/// <summary>
		/// Runs the job with prepared arguments.
		/// </summary>
		public void Run()
		{
			DateTime start = DateTime.Now;

			if( _args.ShowHelp || _args.FileSystem==null )
				WriteHelp();
			if ( _args.ShowVersion )
				WriteVersion( _agent );
			if( !_args.ShowHelp && _args.FileSystem!=null )
			{
				_agent.Scan( _args.FileSystem, _args.Recurse );
				WriteStats( start );
			}
        
#if DEBUG 
			//handy when using Visual Studio debug. Keeps the console from closing.
			Console.WriteLine( ResourceManagers.Strings.GetString(STR_RUN_DEBUG_MSG) );
			Console.ReadLine();
#endif
		}

		#region public properties
		/// <summary>
		/// Get/Set. ScanArgs. Most useful for running consecutive jobs with a single set of counters.
		/// </summary>
		public ScanArgs Args
		{
			get { return _args; }
			set { _args = value; }
		}

		/// <summary>
		/// Get-only. Total number of files that have been scanned as part of this ScanJob.
		/// </summary>
		public int FileCount
		{
			get { return _scancount; } 
		}

		/// <summary>
		/// Get-only. Total number of viruses that have been found as part of this ScanJob.
		/// </summary>
		public int VirusCount
		{
			get { return _vircount; } 
		}
		#endregion

		#region IVirusScanAgent event handlers
		/// <summary>
		/// Handles the _agent.ItemScanCompleted event to increment a counter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _agent_ItemScanCompleted(object sender, ScanCompletedEventArgs e)
		{
			_scancount++;
		}
		/// <summary>
		/// Handles the _agent.VirusFound event to increment a counter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _agent_VirusFound(object sender, ScanCompletedEventArgs e)
		{
			_vircount++;
		}
		#endregion
		
		#region Write-info-to-console helper methods
		/// <summary>
		/// Writes help to to the console standard out.
		/// </summary>
		public static void WriteHelp()
		{
			Console.WriteLine( string.Format (CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_HELP_TEMPLATE )
				,ScanArgs.FLAG_HELP_SHORT		,ScanArgs.FLAG_HELP_LONG
				,ScanArgs.FLAG_VERSION_SHORT	,ScanArgs.FLAG_VERSION_LONG
				,ScanArgs.FLAG_VERBOSE_SHORT	,ScanArgs.FLAG_VERBOSE_LONG
				,ScanArgs.FLAG_RECURSE_SHORT	,ScanArgs.FLAG_RECURSE_LONG
				,ScanArgs.FLAG_CLAMD_HOST_SHORT	,ScanArgs.FLAG_CLAMD_HOST_LONG	,ScanArgs.CLAMD_HOST
				,ScanArgs.FLAG_CLAMD_PORT_SHORT	,ScanArgs.FLAG_CLAMD_PORT_LONG	,ScanArgs.CLAMD_PORT
				) );
		}
		/// <summary>
		/// Writes the console client version and _agent version to the console standard out.
		/// </summary>
		/// <param name="agent">IVirusScanAgent instance to call Version upon.</param>
		public static void WriteVersion(IVirusScanAgent agent)
		{
			string title=null, version;
		
			Assembly assembly = Assembly.GetEntryAssembly();
	
			version = assembly.GetName().Version.ToString();
			object[] titles = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute),true);
			if(titles.Length>0)
				title = ((AssemblyTitleAttribute)titles[0]).Title;

			Console.WriteLine( string.Format(CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_WRITE_VERSION ), title, version) );
			Console.WriteLine( agent.Version );
			Console.WriteLine();
		}

		/// <summary>
		/// Writes file scan statistics to the console.
		/// </summary>
		private void WriteStats(DateTime startTime)
		{
			TimeSpan elapsed = DateTime.Now - startTime;

			string timestr = string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_TIMESPAN_TEMPLATE ), elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
			Console.WriteLine( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_STATS_TEMPLATE ), _scancount, _vircount, timestr ) );
		}
		#endregion
	}
}

