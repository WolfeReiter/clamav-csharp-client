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

using System;
using System.IO;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// IVirScanAgent defines a public interface for virus scanning agents to implement.
	/// </summary>
	public interface IVirusScanAgent
	{

		/// <summary>
		/// Scan a Stream for viruses.
		/// </summary>
		/// <param name="id">Identifier for the Stream</param>
		/// <param name="stream">Stream to be scanned</param>
		void Scan(string id, Stream stream);

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
		event ScanCompletedEventHandler ItemScanCompleted;

		/// <summary>
		/// Event to be fired when a virus is found.
		/// </summary>
		event ScanCompletedEventHandler VirusFound;
	}

	#region ScanCompleted, VirusFound delegates

	/// <summary>
	/// Delegate prototype to be called when a scan is completed.
	/// </summary>
	public delegate void ScanCompletedEventHandler( object sender, ScanCompletedEventArgs e );

	/// <summary>
	/// Arguments passed to the ScanCompleted and VirusFound delegates.
	/// </summary>
	public class ScanCompletedEventArgs : EventArgs
	{
		private string _item, _result;

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="item">String that can be used to uniquely identify the item being scanned (e.g. filename, database key etc.).</param>
		/// <param name="result">Result string from the virus scan engine.</param>
		public ScanCompletedEventArgs(string item, string result)
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
