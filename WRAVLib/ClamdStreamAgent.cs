using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// ClamdStreamAgent is a concrete VirScanAgent that is a client to the clamd daemon using the STREAM protocol.
	/// </summary>
	public class ClamdStreamAgent : VirScanAgent, IVirScanAgent
	{
		private const string SCAN_VERB = "STREAM";
		private string	_host;
		private int		_port;
		private bool	_queueAsync = false;
		/// <summary>
		/// Ctor. Synchronous or async agent. 
		/// </summary>
		/// <remarks>Async is generally much faster when scanning several files but only appropriate for use with a 
		/// service or GUI application that has a long running main thread. A synchronous agent is appropriate when 
		/// the Scan() method should not exit until all scanning is complete such as with a syncronos scan as part 
		/// of an upload/download or console applciation.</remarks>
		/// <param name="host">computer running clamd</param>
		/// <param name="port">TCP port clamd listens on</param>
		/// <param name="useAsyncScanQueue">whether to queue each file to it's own thread for scanning</param>
		public ClamdStreamAgent(string host, int port, bool useAsyncScanQueue)
		{
			_host = host;
			_port = port;
			_queueAsync = useAsyncScanQueue;
		}
		/// <summary>
		/// Ctor. Constructs a syncronous scanning agent. This is best for console applications or single file scanning
		/// such as during an upload/download.
		/// </summary>
		/// <param name="host">computer running clamd</param>
		/// <param name="port">TCP port clamd listens on</param>
		public ClamdStreamAgent(string host, int port) : this(host, port, false){}

		protected void SendStream(byte[] buff, int streamPort)
		{
			using ( TcpClient tcpclient = new TcpClient(this.ClamdHost, streamPort ) )
			using ( NetworkStream netstream = tcpclient.GetStream() )
			using ( BinaryWriter writer = new BinaryWriter( netstream, Encoding.ASCII) )
			{	
				writer.Write( buff );
				writer.Flush();
				writer.Close();
				netstream.Close();
				tcpclient.Close();
			}
		}

		protected class ByteScanArgs
		{
			private string _id;
			private byte[] _buff;
			public ByteScanArgs(string id, byte[] buff)
			{
				_buff = buff;
				_id	 = id;
			}
			public byte[] Buff { get { return _buff; } }
			public string Id { get { return _id; } }
		}

		protected virtual void DoByteScan(object o)
		{
			if( !(o is ByteScanArgs) )
				throw new ArgumentException("DoByteScan requires a ClamdStreamAgent.ByteScanArgs struct as its parameter");

			ByteScanArgs scanArgs = o as ByteScanArgs;

			string outstr = null;
			using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, this.ClamdPort ) )
			using (NetworkStream netStream = tcpclient.GetStream() )
			using (StreamWriter outputStream = new StreamWriter(netStream) )
			using (StreamReader inputStream = new StreamReader(netStream) )
			{
				outputStream.WriteLine( SCAN_VERB );
				outputStream.Flush();
				
				string portstr = inputStream.ReadLine();
				string[] args = portstr.Split(' ');
				
				SendStream ( scanArgs.Buff, int.Parse (args[args.Length-1]) );
				
				outstr = inputStream.ReadLine();

				outputStream.Close();
				inputStream.Close();
				netStream.Close();
				tcpclient.Close();
			}
			
			this.OnItemScanCompleted( new ScanCompletedArgs( scanArgs.Id, outstr ) );
		}



		#region IVirScanAgent Members

		public override void Scan(string id, byte[] buff)
		{
			if(_queueAsync)
			{
				ByteScanArgs args = new ByteScanArgs(id,buff);
				ThreadPool.QueueUserWorkItem( new WaitCallback( this.DoByteScan ), args );
			}
			else
			{

				string outstr = null;
				using ( TcpClient tcpclient = new TcpClient( this.ClamdHost, this.ClamdPort ) )
				using (NetworkStream netStream = tcpclient.GetStream() )
				using (StreamWriter outputStream = new StreamWriter(netStream) )
				using (StreamReader inputStream = new StreamReader(netStream) )
				{
					outputStream.WriteLine( SCAN_VERB );
					outputStream.Flush();
				
					string portstr = inputStream.ReadLine();
					string[] args = portstr.Split(' ');
				
					SendStream ( buff, int.Parse (args[args.Length-1]) );
				
					outstr = inputStream.ReadLine();

					outputStream.Close();
					inputStream.Close();
					netStream.Close();
					tcpclient.Close();
				}
			
				this.OnItemScanCompleted( new ScanCompletedArgs( id, outstr ) );
			}
		}

		public override void Scan(FileInfo file)
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
			finally
			{
				if(inStream!=null)
					inStream.Close();
			}
		}

		public override void Scan(DirectoryInfo dir)
		{
			if( dir.Exists )
			{
				foreach( FileInfo file in dir.GetFiles() )
					Scan( file );
			}
			else
				this.OnItemScanCompleted( new ScanCompletedArgs( dir.FullName, "ERROR: The directory does not exist." ) );
		}

		public override void Scan(DirectoryInfo dir, bool recurse)
		{
			Scan ( dir );
			if(recurse)
			{			
				foreach( DirectoryInfo subdir in dir.GetDirectories() )
				{
					foreach( FileInfo file in subdir.GetFiles() )
					{
						Scan( file );
					}
					Scan( subdir, recurse );
				}
			}
		}

		public override void Scan(FileSystemInfo file, bool recurse)
		{
			if(file is FileInfo)
				Scan(file as FileInfo);
			else if (file is DirectoryInfo)
				Scan(file as DirectoryInfo, recurse);
			else
				throw new NotSupportedException( string.Format( "{0} is not a supported type of FileSystemInfo. Use FileInfo or DirectoryInfo.",file.GetType() ) );
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
