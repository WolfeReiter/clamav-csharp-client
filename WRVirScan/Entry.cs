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
using System.Diagnostics;

namespace WolfeReiter.AntiVirus.ConsoleDemo
{
	/// <summary>
	/// Container for application entry point.
	/// </summary>
	public sealed class Entry
	{
		private Entry(){}
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		public static void Main(string[] args)
		{	
			log4net.Config.DOMConfigurator.Configure();
			try
			{
				ScanJob job = new ScanJob(  ScanArgs.Parse( args ) );
				job.Run();
			}
			catch(ArgumentException ex)
			{
				Debug.WriteLine(ex);
				Console.WriteLine(ex.Message);
			}
		}
	}
}