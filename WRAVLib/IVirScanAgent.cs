using System;
using System.IO;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// IVirScanAgent defines a public interface for virus scanning agents to implement.
	/// </summary>
	public interface IVirScanAgent
	{
		/// <summary>
		/// Scan a byte[] buffer for viruses.
		/// </summary>
		/// <param name="id">Identifier for the byte[]</param>
		/// <param name="buff">byte bag to be scanned</param>
		void Scan(string id, Byte[] buff);

		/// <summary>
		/// Scan a file for viruses, sending the result to the default async handler.
		/// </summary>
		/// <param name="file">file to be scanned</param>
		void Scan(FileInfo file);

		/// <summary>
		/// Scan a directory for viruses non-recursively. 
		/// </summary>
		/// <param name="dir">Directory to scan</param>
		void Scan(DirectoryInfo dir);

		/// <summary>
		/// Scan a directory for viruses with or without recursion.
		/// </summary>
		/// <param name="dir">Directory to scan</param>
		/// <param name="recurse">whether to recurse</param>
		void Scan(DirectoryInfo dir, bool recurse);

		/// <summary>
		/// Scan a file or directry with or without recursion. Recursion only applies to diretories.
		/// </summary>
		/// <param name="file">File or Directory to scan</param>
		/// <param name="recurse">whether to recurse</param>
		void Scan(FileSystemInfo file, bool recurse);

		/// <summary>
		/// Get version information.
		/// </summary>
		string Version {get;}

		/// <summary>
		/// Event to be fired when an item has been scanned.
		/// </summary>
		event ScanCompleted ItemScanCompleted;

		/// <summary>
		/// Event to be fired when a virus is found.
		/// </summary>
		event VirusFound VirusFound;
	}

	#region ScanCompleted, VirusFound delegates

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

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="item">String that can be used to uniquely identify the item being scanned (e.g. filename, database key etc.).</param>
		/// <param name="result">Result string from the virus scan engine.</param>
		public ScanCompletedArgs(string item, string result)
		{
			_item = item;
			_result = result;
		}

		/// <summary>
		/// Get-only. String that can be used to uniquely identify the item being scanned (e.g. filename, database key etc.).
		/// </summary>
		
		public string Item { get { return _item; } }
		/// <summary>
		/// Get-only. Result string from the virus scan engine.
		/// </summary>
		public string Result { get { return _result; } }
	}

	#endregion
}
