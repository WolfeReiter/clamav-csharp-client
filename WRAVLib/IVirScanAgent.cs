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
		void Scan(DirectoryInfo dir, bool recurse);


		/// <summary>
		/// Scan a file or directry with or without recursion. Recursion only applies to diretories.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="recurse"></param>
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
}
