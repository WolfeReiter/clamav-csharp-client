using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace WolfeReiter.AntiVirus.ConsoleDemo
{
	/// <summary>
	/// ScanArgs is a helper class that translates the arguments passed on the command-line to something more useful.
	/// </summary>
	public class ScanArgs
	{

		#region resource name constants
		private const string STR_PARSE_ARGUMENT_EXCEPTION_HOSTNAME		= "ScanArgs.Parse.ArgumentException-HostName";
		private const string STR_PARSE_ARGUMENT_EXCEPTION_PORT			= "ScanArgs.Parse.ArgumentException-Port";
		private const string STR_PARSE_ARGUMENT_EXCEPTION_PORT_MISSING	= "ScanArgs.Parse.ArgumentException-PortMissing";
		private const string STR_PARSE_ARGUMENT_EXCEPTION_UNKNOWN		= "ScanArgs.Parse.ArgumentException-Unknown";
		#endregion
		
		#region constants for defaults and switches
		/// <summary>
		/// Short hostname switch.
		/// </summary>
		public const string FLAG_CLAMD_HOST_SHORT	= "-c";
		/// <summary>
		/// Long hostname switch.
		/// </summary>
		public const string FLAG_CLAMD_HOST_LONG	= "--clamd-host";
		/// <summary>
		/// Short port number switch.
		/// </summary>
		public const string FLAG_CLAMD_PORT_SHORT	= "-p";
		/// <summary>
		/// Long port number switch.
		/// </summary>
		public const string FLAG_CLAMD_PORT_LONG	= "--port";
		/// <summary>
		/// Short version switch.
		/// </summary>
		public const string FLAG_VERSION_SHORT		= "-V";
		/// <summary>
		/// Long version switch.
		/// </summary>
		public const string FLAG_VERSION_LONG		= "--version";
		/// <summary>
		/// Short verbose switch.
		/// </summary>
		public const string FLAG_VERBOSE_SHORT		= "-v";
		/// <summary>
		/// Long verbose switch.
		/// </summary>
		public const string FLAG_VERBOSE_LONG		= "--verbose";
		/// <summary>
		/// Short help switch.
		/// </summary>
		public const string FLAG_HELP_SHORT			= "-h";
		/// <summary>
		/// Long help switch.
		/// </summary>
		public const string FLAG_HELP_LONG			= "--help";
		/// <summary>
		/// Short recurse switch.
		/// </summary>
		public const string FLAG_RECURSE_SHORT		= "-r";
		/// <summary>
		/// Long recurse switch.
		/// </summary>
		public const string FLAG_RECURSE_LONG		= "--recurse";
		/// <summary>
		/// Default clamd host.
		/// </summary>
		public const string CLAMD_HOST				= "localhost";
		/// <summary>
		/// Default clamd port.
		/// </summary>
		public const int    CLAMD_PORT				= 3310;
		#endregion

		private FileSystemInfo _filesystem;
		private bool _showHelp, _showVer, _recurse, _verbose;
		private string _host;
		private int _port;

		private ScanArgs() : this( null, false, false, false, false, CLAMD_HOST, CLAMD_PORT){}
		private	ScanArgs(FileSystemInfo filesystem,	bool showHelp, bool showVer, bool recurse, bool verbose, string host, int port)
		{
			_filesystem	= filesystem;
			_showHelp	= showHelp;
			_showVer	= showVer;
			_recurse	= recurse;
			_verbose	= verbose;
			_host		= host;
			_port		= port;
		}

		/// <summary>
		/// Parse a string[] of command-line arguments into a ScanArgs object.
		/// </summary>
		/// <exception cref="ArgumentException">Throws when an invalid switch is passed.</exception>
		/// <param name="args">string[] of command-line arguments</param>
		/// <returns></returns>
		public static ScanArgs Parse(string[] args)
		{
			ScanArgs sargs = new ScanArgs();
			
			if ( args.Length==0 )
				sargs.ShowHelp = true;

			for(int i=0; i<args.Length; i++)
			{
				//check if last arg is file or directory name (or at least not a switch starting with "-"
				if( i==args.Length-1 && !args[i].StartsWith("-") )
					sargs.FileSystem = FileSystemFactory.CreateInstance( args[i] );
				else
				{
					switch( args[i] )
					{
						case FLAG_CLAMD_HOST_SHORT :
							goto case FLAG_CLAMD_HOST_LONG;
						case FLAG_CLAMD_HOST_LONG :
							if( args.Length<i+1 && !args[i+1].StartsWith("-") )
							{
								sargs.Host = args[++i];								
							}
							else
							{
								throw new ArgumentException( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_PARSE_ARGUMENT_EXCEPTION_HOSTNAME ), FLAG_CLAMD_HOST_SHORT, FLAG_CLAMD_HOST_LONG ) );
							}
							break;
						case FLAG_CLAMD_PORT_SHORT :
							goto case FLAG_CLAMD_HOST_LONG;
						case FLAG_CLAMD_PORT_LONG :
							if( args.Length<i+1 && !args[i+1].StartsWith("-") )
							{
								try
								{
									sargs.Port = int.Parse( args[++i], CultureInfo.InvariantCulture );
								}
								catch (FormatException ex)
								{
									throw new ArgumentException( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_PARSE_ARGUMENT_EXCEPTION_PORT ), ex.Message ) );
								}
								catch (OverflowException ex)
								{
									throw new ArgumentException( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_PARSE_ARGUMENT_EXCEPTION_PORT ), ex.Message ) );
								}
							}
							else
							{
								throw new ArgumentException( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_PARSE_ARGUMENT_EXCEPTION_PORT_MISSING ), FLAG_CLAMD_PORT_SHORT, FLAG_CLAMD_PORT_LONG ) );
							}
							break;

						case FLAG_VERSION_SHORT :
							goto case FLAG_VERSION_LONG;
						case FLAG_VERSION_LONG :
							sargs.ShowVersion = true;
							break;

						case FLAG_HELP_SHORT :
							goto case FLAG_HELP_LONG;
						case FLAG_HELP_LONG :
							sargs.ShowHelp = true;
							break;

						case FLAG_RECURSE_SHORT :
							goto case FLAG_RECURSE_LONG;
						case FLAG_RECURSE_LONG :
							sargs.Recurse = true;
							break;

						case FLAG_VERBOSE_SHORT :
							goto case FLAG_VERBOSE_LONG;
						case FLAG_VERBOSE_LONG :
							sargs.Verbose = true;
							break;

						default :
							throw new ArgumentException( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_PARSE_ARGUMENT_EXCEPTION_UNKNOWN ), args[1] ) );
					}
				}
			}
			return sargs;
		}

		#region properties

		/// <summary>
		/// Get-set. Host running clamd.
		/// </summary>
		public string Host
		{
			get { return _host; }
			set { _host = value; }
		}

		/// <summary>
		/// Get-Set. Clamd TCP port.
		/// </summary>
		public int Port
		{
			get { return _port; }
			set { _port = value; }
		}
		
		/// <summary>
		/// Get-Set. Whether to show help. When true a scan never happens.
		/// </summary>
		public bool ShowHelp
		{
			get { return _showHelp; }
			set { _showHelp = value; }
		}

		/// <summary>
		/// Get-Set. Whehter to show version information.
		/// </summary>
		public bool ShowVersion
		{
			get { return _showVer; }
			set { _showVer = value; }
		}
		
		/// <summary>
		/// Get-Set. Whether to be verbose.
		/// </summary>
		public bool Verbose
		{
			get { return _verbose; }
			set { _verbose = value; }
		}

		/// <summary>
		/// Get-Set. Whether to use recursive file system scan.
		/// </summary>
		public bool Recurse
		{
			get { return _recurse; }
			set { _recurse = value; }
		}

		/// <summary>
		/// Get-Set. Directory or file to scan.
		/// </summary>
		public FileSystemInfo FileSystem
		{
			get { return _filesystem; }
			set { _filesystem = value; }
		}
		#endregion
	}
}
