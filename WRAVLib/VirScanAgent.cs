using log4net;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// Summary description for VirScanAgent.
	/// </summary>
	public abstract class VirScanAgent : IVirScanAgent
	{
		private ILog _logger;

		/// <summary>
		/// Defalut Ctor. Creates an ILog instance and registers a default ItemScanCompeted handler.
		/// </summary>
		protected VirScanAgent()
		{
			_logger = LogManager.GetLogger(this.GetType());

			this.ItemScanCompleted += new ScanCompleted(ItemScanCompletedHandler);
		}

		#region IVirScanAgent Members

		/// <summary>
		/// Scan a byte[] buffer for viruses.
		/// </summary>
		/// <param name="id">Identifier for the byte[]</param>
		/// <param name="buff">byte bag to be scanned</param>
		public abstract void Scan(string id, Byte[] buff);

		/// <summary>
		/// Scan a file for viruses, sending the result to the default async handler.
		/// </summary>
		/// <param name="file">file to be scanned</param>
		public abstract void Scan(FileInfo file);

		/// <summary>
		/// Scan a directory for viruses non-recursively. 
		/// </summary>
		/// <param name="dir">Directory to scan</param>
		public abstract void Scan(DirectoryInfo dir);

		/// <summary>
		/// Scan a directory for viruses non-recursively. 
		/// </summary>
		/// <param name="dir">Directory to scan</param>
		/// <param name="recurse">Whether to recurse</param>
		public abstract void Scan(DirectoryInfo dir, bool recurse);

		/// <summary>
		/// Scan a directory for viruses with or without recursion.
		/// </summary>
		/// <param name="file">File or Directory to scan</param>
		/// <param name="recurse">whether to recurse</param>
		public abstract void Scan(FileSystemInfo file, bool recurse);

		/// <summary>
		/// Get version information.
		/// </summary>
		public virtual string Version
		{
			get
			{
				return AgentVersion;
			}
		}

		#endregion
		
		/// <summary>
		/// Return the version of the VirScanAgent class implementation.
		/// </summary>
		public string AgentVersion
		{
			get
			{
				string title=null, descr=null, copyright=null, version;
	
				Assembly assembly = Assembly.GetExecutingAssembly();
	
				version = assembly.GetName().Version.ToString();
				object[] titles = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute),true);
				object[] descrs = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute),true);
				object[] copyrights = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute),true);
				if(titles.Length>0)
					title = ((AssemblyTitleAttribute)titles[0]).Title;
				if(descrs.Length>0)
					descr = ((AssemblyDescriptionAttribute)descrs[0]).Description;
				if(copyrights.Length>0)
					copyright = ((AssemblyCopyrightAttribute)copyrights[0]).Copyright;

				return string.Format("{0} version {1} \n{2}\n{3}", title, version, descr, copyright);
			}
		}
		/// <summary>
		/// Event occurs when a virus is found.
		/// </summary>
		public event VirusFound VirusFound;
		/// <summary>
		/// Raises the VirusFound event.
		/// </summary>
		/// <param name="e"></param>
		protected void OnVirusFound( ScanCompletedArgs e )
		{
			VirusFound found = this.VirusFound;
			if(found!=null)
				found( e );
		}
		/// <summary>
		/// Event occurs when an item has been scanned and a result is available.
		/// </summary>
		public event ScanCompleted ItemScanCompleted;
		/// <summary>
		/// Raises the ItemScanCompleted event.
		/// </summary>
		/// <param name="e"></param>
		protected void OnItemScanCompleted( ScanCompletedArgs e )
		{
			ScanCompleted completed = ItemScanCompleted;
			if(completed!=null)
				completed( e );
		}

		/// <summary>
		/// Base handler for the ItemScanCompleted event. Logs every event to log4net.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void ItemScanCompletedHandler( ScanCompletedArgs e )
		{
			_logger.Info( string.Format( "SCANNED {0}\nRESULT {1}", e.Item, e.Result ) );
			//Console.SetOut( Console.Error );
			//Console.WriteLine( string.Format( "SCANNED {0}\nRESULT {1}", e.Item, e.Result ) );
		}
		/// <summary>
		/// Preferred threading model.
		/// </summary>
		public enum ThreadingModel
		{
			/// <summary>
			/// Scans run on a syncronous thread and block each other. When the Scan method returns all scanning is complete.
			/// </summary>
			SyncronousSingleThead,
			/// <summary>
			/// Scans run in a thread pool. Each scan is queued to its own thread so that long-running scans do not block short scans.
			/// The Scan method may exit before scanning is complete.
			/// </summary>
			AsyncronousThreadPool
		}
	}
}
