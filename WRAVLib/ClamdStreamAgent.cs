using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// ClamdStreamAgent is a concrete VirScanAgent that is a client to the clamd daemon using the STREAM protocol.
	/// </summary>
	public class ClamdStreamAgent : VirScanAgent, IVirScanAgent
	{
		private const string FOUND	   = "FOUND";
		private const string SCAN_VERB = "STREAM";
		private const string VER_VERB  = "VERSION";
		private string	_host;
		private int		_port;
		private VirScanAgent.ThreadingModel	_threadModel;
		private ILog _logger;
		private bool _verbose;

		/// <summary>
		/// Ctor. Synchronous or async agent. 
		/// </summary>
		/// <remarks>Async is generally much faster when scanning several files but only appropriate for use with a 
		/// service or GUI application that has a long running main thread. A synchronous agent is appropriate when 
		/// the Scan() method should not exit until all scanning is complete such as with a syncronos scan as part 
		/// of an upload/download or console applciation.</remarks>
		/// <param name="host">computer running clamd</param>
		/// <param name="port">TCP port clamd listens on</param>
		/// <param name="model">whether to queue each file to it's own thread for scanning</param>
		/// <param name="verbose">whether to use verbose logging or only log errors and viruses</param>
		public ClamdStreamAgent(string host, int port, VirScanAgent.ThreadingModel model, bool verbose )
		{
			_logger = LogManager.GetLogger(this.GetType());
			_host = host;
			_port = port;
			_threadModel = model;
			_verbose = verbose;
		}

		/// <summary>
		/// Ctor. Constructs a syncronous scanning agent. This is best for console applications or single file scanning
		/// such as during an upload/download.
		/// </summary>
		/// <param name="host">computer running clamd</param>
		/// <param name="port">TCP port clamd listens on</param>
		/// <param name="verbose">whether to use verbose logging or only log errors and viruses</param>
		public ClamdStreamAgent(string host, int port, bool verbose) : this(host, port, VirScanAgent.ThreadingModel.SyncronousSingleThead, verbose){}

		/// <summary>
		/// Sends a byte[] to a clamd port
		/// </summary>
		/// <param name="buff">byte[] to scan</param>
		/// <param name="streamPort">TCP port to send buffer for scanning</param>
		protected void SendStream(byte[] buff, int streamPort)
		{
			try
			{
				using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, streamPort ) )
				using ( NetworkStream netstream = tcpclient.GetStream() )
				using ( BinaryWriter writer = new BinaryWriter( netstream, Encoding.ASCII ) )
				{	
					writer.Write( buff );
					writer.Flush();
					writer.Close();
					netstream.Close();
					tcpclient.Close();
				}
			}
			catch(SocketException sex)
			{
				if(_logger.IsDebugEnabled)
					_logger.Error(sex);
				else
					_logger.Error(sex.Message);
			}
		}

		/// <summary>
		/// Inner class. Defines arguments for ByteScan and DoByteScan.
		/// </summary>
		protected class ByteScanArgs
		{
			private string _id;
			private byte[] _buff;
			/// <summary>
			/// CTOR.
			/// </summary>
			/// <param name="id">Byte[] buffer.</param>
			/// <param name="buff">Unique identifier for the buffer (eg. filename, url or database key, etc.)</param>
			public ByteScanArgs(string id, byte[] buff)
			{
				_buff = buff;
				_id	 = id;
			}
			/// <summary>
			/// Get-only. Byte[] buffer.
			/// </summary>
			public byte[] Buff { get { return _buff; } }
			/// <summary>
			/// Get-only. Unique identifier for the buffer (eg. filename, url or database key, etc.)
			/// </summary>
			public string Id { get { return _id; } }
		}

		/// <summary>
		/// WaitCallback method for performing a byte[] scan on an independent thread using System.Treading.QueueUserWorkItem.
		/// </summary>
		/// <exception cref="ArgumentException">Throws argument "o" is not an instance of ByteScanArgs.</exception>
		/// <param name="o">Instance of ByteScanArgs.</param>
		protected virtual void DoByteScan(object o)
		{
			if( !(o is ByteScanArgs) )
				throw new ArgumentException("DoByteScan requires a ClamdStreamAgent.ByteScanArgs object as its parameter");

			ByteScanArgs scanArgs = o as ByteScanArgs;
			ByteScan(scanArgs);
		}

		/// <summary>
		/// Method scans a byte[] by passing it to clamd over a dynamically allocated port. The real magic happens here. 
		/// The ItemScanCompleted event is raised whenever this method completes. If a virus is found, this method raises
		/// the VirusFound event.
		/// </summary>
		/// <param name="args">ByteScanArgs instance.</param>
		protected virtual void ByteScan(ByteScanArgs args)
		{
			if( args==null )
				throw new ArgumentNullException("args","Argument must not be null");

			string outstr = null;
			try
			{
				using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, this.ClamdPort ) )
				using (NetworkStream netStream = tcpclient.GetStream() )
				using (StreamWriter outputStream = new StreamWriter(netStream) )
				using (StreamReader inputStream = new StreamReader(netStream) )
				{
					outputStream.WriteLine( SCAN_VERB );
					outputStream.Flush();
				
					string portstr = inputStream.ReadLine();
					string[] daemonargs = portstr.Split(' ');
				
					SendStream ( args.Buff, int.Parse (daemonargs[daemonargs.Length-1]) );
				
					outstr = inputStream.ReadLine();

					outputStream.Close();
					inputStream.Close();
					netStream.Close();
					tcpclient.Close();
				}

				if(outstr!=null && outstr.IndexOf(FOUND)>-1)
					this.OnVirusFound( new ScanCompletedArgs( args.Id, outstr ) );
			}
			catch(SocketException sex)
			{
				if(_logger.IsDebugEnabled)
					_logger.Error(sex);
				else
					_logger.Error(sex.Message);

				outstr = sex.Message;
			}
			finally
			{
				this.OnItemScanCompleted( new ScanCompletedArgs( args.Id, outstr ) );
			}
		}

		#region IVirScanAgent Members

		/// <summary>
		/// Scan a byte[] bag.
		/// </summary>
		/// <param name="id">Unique identifier for the buffer (eg. filename, url or database key, etc.)</param>
		/// <param name="buff">byte[] bag to scan</param>
		public override void Scan(string id, byte[] buff)
		{
			ByteScanArgs args = new ByteScanArgs(id,buff);
			switch(_threadModel)
			{
				case VirScanAgent.ThreadingModel.AsyncronousThreadPool :
					ThreadPool.QueueUserWorkItem( new WaitCallback( this.DoByteScan ), args );
					break;
				case VirScanAgent.ThreadingModel.SyncronousSingleThead :
					ByteScan(args);			
					break;
				default :
					throw new NotSupportedException( string.Format("The threading model, {0}, is not supported by  ClamdStreamAgent.",_threadModel) );
			}
		}

		/// <summary>
		/// Scan a file.
		/// </summary>
		/// <param name="file">File to scan.</param>
		public override void Scan(FileInfo file)
		{
			if( file != null )
			{
				FileStream inStream = null;
				try
				{
					if(file.Exists)
					{
						inStream = file.OpenRead();
						byte[] buff = new byte[inStream.Length];
						inStream.Read(buff,0,buff.Length);
						Scan(file.FullName, buff);		
					}
					else
					{
						this.OnItemScanCompleted( new ScanCompletedArgs( file.FullName, "ERROR: The file does not exist." ) );
					}
				}
				catch(Exception ex)
				{
					if( (ex is SecurityException || ex is FileLoadException) )
						_logger.Info(ex.Message);	
					else if(_logger.IsDebugEnabled)
						_logger.Debug(ex);
					else
						_logger.Error(ex.Message);
				}
				finally
				{
					if(inStream!=null)
						inStream.Close();
				}
			}
		}

		/// <summary>
		/// Scan a directory non-recursively.
		/// </summary>
		/// <param name="dir">Directory to scan.</param>
		public override void Scan(DirectoryInfo dir)
		{
			if( dir != null )
			{
				if( dir.Exists )
				{
					try
					{
						foreach( FileInfo file in dir.GetFiles() )
							Scan( file );
					}
					catch(Exception ex)
					{
						if( (ex is SecurityException || ex is FileLoadException) )
							_logger.Info(ex.Message);	
						else if(_logger.IsDebugEnabled)
							_logger.Debug(ex);
						else
							_logger.Error(ex.Message);
					}
				}
				else
					this.OnItemScanCompleted( new ScanCompletedArgs( dir.FullName, "ERROR: The directory does not exist." ) );
			}
		}

		/// <summary>
		/// Scan a directory with optional recursion.
		/// </summary>
		/// <param name="dir">Directory to scan</param>
		/// <param name="recurse">Recurse when true.</param>
		public override void Scan(DirectoryInfo dir, bool recurse)
		{
			if( dir!= null )
			{
				Scan ( dir );
				if(recurse)
				{		
					try
					{
						foreach( DirectoryInfo subdir in dir.GetDirectories() )
						{
							try
							{
								foreach( FileInfo file in subdir.GetFiles() )
								{
									Scan( file );
								}
							}
							catch(Exception ex)
							{
								if( (ex is SecurityException || ex is FileLoadException) )
									_logger.Info(ex.Message);	
								else if(_logger.IsDebugEnabled)
									_logger.Debug(ex);
								else
									_logger.Error(ex.Message);
							}
							Scan( subdir, recurse );
						}
					}
					catch (Exception ex)
					{
						if( ex is SecurityException )
							_logger.Info(ex.Message);	
						else if(_logger.IsDebugEnabled)
							_logger.Debug(ex);
						else
							_logger.Error(ex.Message);
					}
				}
			}
		}

		/// <summary>
		/// Scan a FileSystem object (file or directory) with optional recursion.
		/// </summary>
		/// <param name="file">FileSystem object to scan</param>
		/// <param name="recurse">Recurse when true if FileSystem object is a directory. Otherwise this argument is ignored..</param>
		public override void Scan(FileSystemInfo file, bool recurse)
		{
			if( file!=null) 
			{
				if(file is FileInfo)
					Scan(file as FileInfo);
				else if (file is DirectoryInfo)
					Scan(file as DirectoryInfo, recurse);
				else
					throw new NotSupportedException( string.Format( "{0} is not a supported type of FileSystemInfo. Use FileInfo or DirectoryInfo.",file.GetType() ) );
			}
		}

		/// <summary>
		/// Get-only. Queries the clamd daemon for its version information.
		/// </summary>
		public string ClamdVersion
		{
			get
			{
				string portstr = null;
				try
				{
					using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, this.ClamdPort ) )
					using (NetworkStream netStream = tcpclient.GetStream() )
					using (StreamWriter outputStream = new StreamWriter(netStream) )
					using (StreamReader inputStream = new StreamReader(netStream) )
					{
						outputStream.WriteLine( VER_VERB );
						outputStream.Flush();
				
						portstr = inputStream.ReadLine();

						outputStream.Close();
						inputStream.Close();
						netStream.Close();
						tcpclient.Close();
					}
				}
				catch(SocketException sex)
				{
					if(_logger.IsDebugEnabled)
						_logger.Error(sex);
					else
						_logger.Error(sex.Message);

					portstr = sex.Message;
				}
				return portstr;
			}
		}

		/// <summary>
		/// Returns the version of WRAVLib and the scan engine.
		/// </summary>
		public override string Version
		{
			get
			{
				return string.Format("{0}\nScan Engine: {1}", AgentVersion, ClamdVersion);
			}
			
		}
		
		/// <summary>
		/// Handler for the ItemScanCompleted event. Overrides base behavior to implement verbose vs. non-verbose
		/// logging behavior.
		/// </summary>
		/// <param name="e"></param>
		protected override void ItemScanCompletedHandler( ScanCompletedArgs e )
		{
			if( _verbose || _logger.IsDebugEnabled || e.Result.IndexOf(FOUND)>-1 )
				_logger.Info( string.Format( "SCANNED {0} RESULT {1}", e.Item, e.Result ) );
		}

		#endregion

		/// <summary>
		/// Get-only. Host upon which the Clamd daemon is running.
		/// </summary>
		public string	ClamdHost { get { return _host; } }
		/// <summary>
		/// Get-only. Port upon which hte Clamd daemon is listening.
		/// </summary>
		public int		ClamdPort { get { return _port; } }

	}
}
