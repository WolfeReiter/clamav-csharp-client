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

		public abstract void Scan(string id, Byte[] buff);

		public abstract void Scan(FileInfo file);

		public abstract void Scan(DirectoryInfo dir);

		public abstract void Scan(DirectoryInfo dir, bool recurse);

		public abstract void Scan(FileSystemInfo file, bool recurse);

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
			SyncronousSingleThead
			,AsyncronousThreadPool
		}
	}

	#region ScanCompleted delegate
	/// <summary>
	/// Delegate prototype to be called when a scan is completed.
	/// </summary>
	public delegate void ScanCompleted( ScanCompletedArgs e );
	/// <summary>
	/// Delegate prototype to be called when a virus is found.
	/// </summary>
	public delegate void VirusFound( ScanCompletedArgs e);
	/// <summary>
	/// Arguments passed to the ScanCompleted and VirusFound delegates.
	/// </summary>
	public class ScanCompletedArgs
	{
		private string _item, _result;
		public ScanCompletedArgs(string item, string result)
		{
			_item = item;
			_result = result;
		}
		public string Item { get { return _item; } }
		
		public string Result { get { return _result; } }
	}
	#endregion
}
