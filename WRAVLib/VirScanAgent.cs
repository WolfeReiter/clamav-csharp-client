using System;
using System.Collections;
using System.IO;

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

		#endregion

		/// <summary>
		/// Argument for a ByteScan WaitCallback abstract method.
		/// </summary>
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
			Console.SetOut( Console.Error );
			Console.WriteLine( string.Format( "SCANNED {0} RESULT {1}", e.Item, e.Result ) );
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
