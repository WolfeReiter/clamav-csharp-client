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

		protected VirScanAgent()
		{
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
			//TODO: Put Log4Net here instead.
			Console.SetOut( Console.Error );
			Console.WriteLine( string.Format( "SCANNED {0}\nRESULT {1}", e.Item, e.Result ) );
		}

		public enum ThreadingModel
		{
			SyncronousSingleThead
			,AsyncronousThreadPool
			
		}
	}

	#region ScanCompleted delegate
	public delegate void ScanCompleted( ScanCompletedArgs e );
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
