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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// Summary description for VirusScanAgent.
	/// </summary>
	public abstract class VirusScanAgent : IVirusScanAgent
	{
		#region resource name constants
		private const string STR_AGENT_VERSION					= "VirusScanAgent.AgentVersion";
		private const string STR_ITEM_SCAN_COMPLETED_HANDLER	= "VirusScanAgent.ItemScanCompletedHandler";
		#endregion
		
		private ILog _logger;

		/// <summary>
		/// Defalut Ctor. Creates an ILog instance and registers a default ItemScanCompeted handler.
		/// </summary>
		protected VirusScanAgent()
		{
			_logger = LogManager.GetLogger(this.GetType());

			this.ItemScanCompleted += new ScanCompletedEventHandler(ItemScanCompletedHandler);
		}

		#region IVirusScanAgent Members


		/// <summary>
		/// Scan a Stream for viruses.
		/// </summary>
		/// <param name="id">Identifier for the Stream</param>
		/// <param name="stream">Stream to be scanned</param>
		public abstract void Scan(string id, Stream stream);

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
		/// Return the version of the VirusScanAgent class implementation.
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

				return string.Format(CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_AGENT_VERSION ), title, version, descr, copyright);
			}
		}
		/// <summary>
		/// Event occurs when a virus is found.
		/// </summary>
		public event ScanCompletedEventHandler VirusFound;
		/// <summary>
		/// Raises the VirusFound event.
		/// </summary>
		/// <param name="e"></param>
		protected void OnVirusFound( ScanCompletedEventArgs e )
		{
			ScanCompletedEventHandler found = this.VirusFound;
			if(found!=null)
				found( this, e );
		}
		/// <summary>
		/// Event occurs when an item has been scanned and a result is available.
		/// </summary>
		public event ScanCompletedEventHandler ItemScanCompleted;
		/// <summary>
		/// Raises the ItemScanCompleted event.
		/// </summary>
		/// <param name="e"></param>
		protected void OnItemScanCompleted( ScanCompletedEventArgs e )
		{
			ScanCompletedEventHandler completed = ItemScanCompleted;
			if(completed!=null)
				completed( this, e );
		}

		/// <summary>
		/// Base handler for the ItemScanCompleted event. Logs every event to log4net.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void ItemScanCompletedHandler( object sender, ScanCompletedEventArgs e )
		{
			_logger.Info( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString( STR_ITEM_SCAN_COMPLETED_HANDLER ), e.Item, e.Result ) );
		}
		/// <summary>
		/// Preferred threading model.
		/// </summary>
		public enum ThreadingModel
		{
			/// <summary>
			/// Scans run on a syncronous thread and block each other. When the Scan method returns all scanning is complete.
			/// </summary>
			SynchronousSingleThread,
			/// <summary>
			/// Scans run in a thread pool. Each scan is queued to its own thread so that long-running scans do not block short scans.
			/// The Scan method may exit before scanning is complete.
			/// </summary>
			AsynchronousThreadPool
		}
	}
}
