/*
** Copyright (c) 2004 Brian A. Reiter <breiter@wolfereiter.com> and WolfeReiter, LLC
**
** This software is provided 'as-is', without any express or implied warranty. In no 
** event will the authors be held liable for any damages arising from the use of 
** this software.
**
** Permission is granted to anyone to use this software for any purpose, including 
** commercial applications, and to alter it and redistribute it freely, subject to 
** the following restrictions:
**
**    1. The origin of this software must not be misrepresented; you must not claim 
**       that you wrote the original software. If you use this software in a product,
**       an acknowledgment in the product documentation would be appreciated but is 
**       not required.
**
**    2. Altered source versions must be plainly marked as such, and must not be 
**       misrepresented as being the original software.
**
**    3. This notice may not be removed or altered from any source distribution.
*/

using log4net;
using System;
using System.Globalization;
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
	/// ClamdStreamAgent is a concrete VirusScanAgent that is a client to the clamd daemon using the STREAM protocol.
	/// </summary>
	public class ClamdStreamAgent : VirusScanAgent, IVirusScanAgent
	{
		#region resource name constants
		private const string STR_DO_BYTE_SCAN_ARG_EX				= "ClamdStreamAgent.DoByteScan_ArgumentException";
		private const string STR_SCAN_NOT_SUPPORTED_EX_THREADING	= "ClamdStreamAgent.Scan_NotSupportedException-ThreadingModel";
		private const string STR_SCAN_ERR_FILE_NOT_EXIST			= "ClamdStreamAgent.Scan_NotFileExists";
		private const string STR_SCAN_ERR_DIR_NOT_EXIST				= "ClamdStreamAgent.Scan_NotDirectoryExists";
		private const string STR_SCAN_FILESYSTEMINFO_NONT_SUPPORTED	= "ClamdStreamAgent.Scan_NotSupportedException-FileSystemInfo";
		private const string STR_VERSION							= "ClamdStreamAgent.Version";
		private const string STR_ITEM_SCAN_COMPLETED_HANDLER		= "ClamdStreamAgent.ItemScanCompletedHandler";
		#endregion

		private const string FOUND	   = "FOUND";
		private const string SCAN_VERB = "STREAM";
		private const string VER_VERB  = "VERSION";
		
		private const int	 MAX_NET_RETRY = 5;
		private static int   _netRetryCount = 0;
		
		private string	_host;
		private int		_port;
		private VirusScanAgent.ThreadingModel	_threadModel;
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
		public ClamdStreamAgent(string host, int port, VirusScanAgent.ThreadingModel model, bool verbose )
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
		public ClamdStreamAgent(string host, int port, bool verbose) : this(host, port, VirusScanAgent.ThreadingModel.SynchronousSingleThread, verbose){}

		/// <summary>
		/// Sends a byte[] to a clamd port.
		/// </summary>
		/// <exception cref="SocketException">Throws if the socket is not available.</exception>
		/// <exception cref="IOException">Throws if there is an error writing to the socket.</exception>
		/// <param name="stream">Stream to scan</param>
		/// <param name="streamPort">TCP port to send buffer for scanning</param>
		protected void SendStream(Stream stream, int streamPort)
		{
			const int TCP_CHUNK = 1460;
			byte[] buff = new byte[TCP_CHUNK];

			using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, streamPort ) )
			using ( NetworkStream netstream = tcpclient.GetStream() )
			{	
				_logger.Debug("Initial stream.Length: " + stream.Length);
				for(long i=0; stream.Position < stream.Length; i+=TCP_CHUNK)
				{
					stream.Position = i;
					_logger.Debug("stream.Position: " + stream.Position);
					if( (stream.Length - stream.Position) < TCP_CHUNK)
						buff = new byte[Convert.ToInt32( stream.Length-stream.Position )];
						
					stream.Read( buff, 0, buff.Length );
					netstream.Write( buff, 0, buff.Length );
					netstream.Flush();
				}
				netstream.Close();
				tcpclient.Close();
			}
		}

		/// <summary>
		/// WaitCallback method for performing a byte[] scan on an independent thread using System.Treading.QueueUserWorkItem.
		/// </summary>
		/// <exception cref="ArgumentException">Throws argument "o" is not an instance of StreamScanArgs.</exception>
		/// <param name="o">Instance of StreamScanArgs.</param>
		protected virtual void DoStreamScan(object o)
		{
			if( !(o is StreamScanArgs) )
				throw new ArgumentException( ResourceManagers.Strings.GetString(STR_DO_BYTE_SCAN_ARG_EX) );

			StreamScanArgs scanArgs = (StreamScanArgs)o;
			StreamScan(scanArgs);
		}

		/// <summary>
		/// Method scans a byte[] by passing it to clamd over a dynamically allocated port. The real magic happens here. 
		/// The ItemScanCompleted event is raised whenever this method completes. If a virus is found, this method raises
		/// the VirusFound event.
		/// </summary>
		/// <param name="args">StreamScanArgs instance.</param>
		protected virtual void StreamScan(StreamScanArgs args)
		{
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
				
					SendStream ( args.Stream, int.Parse (daemonargs[daemonargs.Length-1], CultureInfo.InvariantCulture ) );
				
					outstr = inputStream.ReadLine();

					outputStream.Close();
					inputStream.Close();
					netStream.Close();
					tcpclient.Close();
				}

				if(outstr!=null && outstr.IndexOf(FOUND)>-1)
					this.OnVirusFound( new ScanCompletedEventArgs( args.Id, outstr ) );
				
				this.OnItemScanCompleted( new ScanCompletedEventArgs( args.Id, outstr ) );
				//reset retry counter
				_netRetryCount = 0;
			}
			catch(Exception ex)
			{
				if(ex is SocketException || ex is IOException)
				{
					if (_netRetryCount < MAX_NET_RETRY)
					{
						//increment retry count and try again
						_netRetryCount++;
						StreamScan( args );
					}
					else
					{
						//reset retry count log error and return
						_netRetryCount = 0;
						if(_logger.IsDebugEnabled)
							_logger.Error(ex);
						else
							_logger.Error(ex.Message);
					}
				}
				else
					throw;
			}

		}

		#region IVirusScanAgent Members

		/// <summary>
		/// Scan a byte[] bag.
		/// </summary>
		/// <param name="id">Unique identifier for the buffer (eg. filename, url or database key, etc.)</param>
		/// <param name="stream">Stream to scan</param>
		public override void Scan(string id, Stream stream)
		{
			StreamScanArgs args = new StreamScanArgs(id,stream);
			switch(_threadModel)
			{
				case VirusScanAgent.ThreadingModel.AsynchronousThreadPool :
					ThreadPool.QueueUserWorkItem( new WaitCallback( this.DoStreamScan ), args );
					break;
				case VirusScanAgent.ThreadingModel.SynchronousSingleThread :
					StreamScan(args);			
					break;
				default :
					throw new NotSupportedException( string.Format(CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString(STR_SCAN_NOT_SUPPORTED_EX_THREADING), _threadModel) );
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
				try
				{
					if(file.Exists)
					{
						using( Stream stream = file.OpenRead() )
							Scan(file.FullName, stream);		
					}
					else
					{
						this.OnItemScanCompleted( new ScanCompletedEventArgs( file.FullName, ResourceManagers.Strings.GetString(STR_SCAN_ERR_FILE_NOT_EXIST) ) );
					}
				}
				catch(Exception ex)
				{
					if( (ex is UnauthorizedAccessException || ex is SecurityException || ex is FileLoadException || ex is IOException) )
						_logger.Error(ex.Message);	
					else if(_logger.IsDebugEnabled)
						_logger.Error(ex);
					else
						throw;
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
						if( ex is UnauthorizedAccessException || ex is SecurityException || ex is FileLoadException )
							_logger.Error(ex.Message);	
						else if(_logger.IsDebugEnabled)
							_logger.Error(ex);
						else
							throw;
					}
				}
				else
					this.OnItemScanCompleted( new ScanCompletedEventArgs( dir.FullName, ResourceManagers.Strings.GetString(STR_SCAN_ERR_DIR_NOT_EXIST) ) );
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
								if( ex is UnauthorizedAccessException || ex is SecurityException )
									_logger.Error(ex.Message);	
								else if(_logger.IsDebugEnabled)
									_logger.Error(ex);
								else
									throw;
							}
							Scan( subdir, recurse );
						}
					}
					catch (Exception ex)
					{
						if( ex is UnauthorizedAccessException || ex is SecurityException )
							_logger.Error(ex.Message);	
						else if(_logger.IsDebugEnabled)
							_logger.Error(ex);
						else
							throw;
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
					throw new NotSupportedException( string.Format(  CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString(STR_SCAN_FILESYSTEMINFO_NONT_SUPPORTED), file.GetType() ) );
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
				return string.Format(CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString(STR_VERSION), AgentVersion, ClamdVersion);
			}
			
		}
		
		/// <summary>
		/// Handler for the ItemScanCompleted event. Overrides base behavior to implement verbose vs. non-verbose
		/// logging behavior.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void ItemScanCompletedHandler( object sender, ScanCompletedEventArgs e )
		{
			if( _verbose || _logger.IsDebugEnabled || e.Result.IndexOf(FOUND)>-1 )
				_logger.Info( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString(STR_ITEM_SCAN_COMPLETED_HANDLER), e.Item, e.Result ) );
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
