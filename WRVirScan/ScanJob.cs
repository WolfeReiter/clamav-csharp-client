using System;
using System.Reflection;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// ScanJob manages a virus scanning job.
	/// </summary>
	public class ScanJob
	{
		private ScanArgs _args;
		private int _scancount, _vircount;
		private IVirScanAgent _agent;

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
			_agent.ItemScanCompleted += new ScanCompleted(_agent_ItemScanCompleted);
			_agent.VirusFound		 += new VirusFound(_agent_VirusFound);
		}

		/// <summary>
		/// Runs the job with prepared arguments.
		/// </summary>
		public void Run()
		{
			if( _args.ShowHelp || _args.FileSystem==null )
				WriteHelp();
			if ( _args.ShowVer )
				WriteVersion( _agent );
			if( !_args.ShowHelp && _args.FileSystem!=null )
			{
				_agent.Scan( _args.FileSystem, _args.Recurse );
				WriteStats();
			}
        
#if DEBUG 
			//handy when using Visual Studio debug. Keeps the console from closing.
			Console.WriteLine("Press ENTER or RETURN to exit.");
			Console.ReadLine();
#endif
		}

		#region public properties
		/// <summary>
		/// Get/Set. ScanArgs. Most useful for running consecutive jobs with a single set of counters.
		/// </summary>
		public ScanArgs args
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

		#region IVirScanAgent event handlers
		/// <summary>
		/// Handles the _agent.ItemScanCompleted event to increment a counter.
		/// </summary>
		/// <param name="e"></param>
		private void _agent_ItemScanCompleted(ScanCompletedArgs e)
		{
			_scancount++;
		}
		/// <summary>
		/// Handles the _agent.VirusFound event to increment a counter.
		/// </summary>
		/// <param name="e"></param>
		private void _agent_VirusFound(ScanCompletedArgs e)
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
			Console.WriteLine( string.Format (@"
Usage: [switches] fileOrDirName
switches:
	{0} or {1}		show help
	{2} or {3}		show version info
	{4} or {5}		be verbose
	{6} or {7}		recurse
	{8} or {9}	host ex: {8} {10}
	(default host is {10}) 
	{11} or {12}		tcp port ex: {11} {13}
	(default port is {13}) 
"
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
		/// <param name="_agent">IVirScanAgent instance to call Version upon.</param>
		public static void WriteVersion(IVirScanAgent _agent)
		{
			string title=null, version;
		
			Assembly assembly = Assembly.GetEntryAssembly();
	
			version = assembly.GetName().Version.ToString();
			object[] titles = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute),true);
			if(titles.Length>0)
				title = ((AssemblyTitleAttribute)titles[0]).Title;

			Console.WriteLine( string.Format("{0} version {1}", title, version) );
			Console.WriteLine( _agent.Version );
			Console.WriteLine();
		}

		/// <summary>
		/// Writes file scan statistics to the console.
		/// </summary>
		private void WriteStats()
		{
			const string STATS = @"
************************
** FILES SCANNED: {0}
** VIRUSES FOUND: {1}
************************
";
			Console.WriteLine( string.Format( STATS, _scancount, _vircount ) );
		}
		#endregion
	}
}

