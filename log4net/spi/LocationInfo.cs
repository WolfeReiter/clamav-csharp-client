#region Copyright
//
// This framework is based on log4j see http://jakarta.apache.org/log4j
// Copyright (C) The Apache Software Foundation. All rights reserved.
//
// This software is published under the terms of the Apache Software
// License version 1.1, a copy of which has been included with this
// distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.Diagnostics;

using log4net.helpers;

namespace log4net.spi
{
	/// <summary>
	/// The internal representation of caller location information.
	/// </summary>
#if !NETCF
	[Serializable]
#endif
	public class LocationInfo
	{
		#region Constants

		/// <summary>
		/// When location information is not available the constant
		/// <c>NA</c> is returned. Current value of this string
		/// constant is <b>?</b>.
		/// </summary>
		private const string NA = "?";

		#endregion

		#region Member Variables

		private string m_className;
		private string m_fileName;
		private string m_lineNumber;
		private string m_methodName;
		private string m_fullInfo;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate location information based on the current thread
		/// </summary>
		/// <param name="fullNameOfCallingClass">the fully name of the calling class (not assembly qualified)</param>
		public LocationInfo(string fullNameOfCallingClass) 
		{
			// Initialise all fields
			m_className = NA;
			m_fileName = NA;
			m_lineNumber = NA;
			m_methodName = NA;
			m_fullInfo = NA;

#if !NETCF
			try
			{
				StackTrace st = new StackTrace(true);
				int frameIndex = 0;

				// skip frames not from fqnOfCallingClass
				while (frameIndex < st.FrameCount)
				{
					StackFrame frame = st.GetFrame(frameIndex);
					if (frame.GetMethod().DeclaringType.FullName == fullNameOfCallingClass)
					{
						break;
					}
					frameIndex++;
				}

				// skip frames from fqnOfCallingClass
				while (frameIndex < st.FrameCount)
				{
					StackFrame frame = st.GetFrame(frameIndex);
					if (frame.GetMethod().DeclaringType.FullName != fullNameOfCallingClass)
					{
						break;
					}
					frameIndex++;
				}

				if (frameIndex < st.FrameCount)
				{
					// now frameIndex is the first 'user' caller frame
					StackFrame locationFrame = st.GetFrame(frameIndex);

					m_className = locationFrame.GetMethod().DeclaringType.FullName;
					m_fileName = locationFrame.GetFileName();
					m_lineNumber = locationFrame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
					m_methodName =  locationFrame.GetMethod().Name;
					m_fullInfo =  m_className+'.'+m_methodName+'('+m_fileName+':'+m_lineNumber+')';
				}
			}
			catch(System.Security.SecurityException)
			{
				// This security exception will occur if the caller does not have 
				// some undefined set of SecurityPermission flags.
				LogLog.Debug("LocationInfo: Security exception while trying to get caller stack frame. Error Ingnored. Location Information Not Available.");
			}
#endif
		}

		/// <summary>
		/// Create LocationInfo with specified data
		/// </summary>
		/// <param name="className">the fully qualified class name</param>
		/// <param name="methodName">the method name</param>
		/// <param name="fileName">the file name</param>
		/// <param name="lineNumber">the line number of the method within the file</param>
		public LocationInfo(string className, string methodName, string fileName, string lineNumber)
		{
			m_className = className;
			m_fileName = fileName;
			m_lineNumber = lineNumber;
			m_methodName = methodName;
			m_fullInfo = m_className+'.'+m_methodName+'('+m_fileName+':'+m_lineNumber+')';
		}

		#endregion

		/// <summary>
		/// Return the fully qualified class name of the caller making the logging request.
		/// </summary>
		/// <returns></returns>
		public string ClassName
		{
			get { return m_className; }
		}

		/// <summary>
		/// Return the file name of the caller.
		/// </summary>
		/// <returns></returns>
		public string FileName
		{
			get { return m_fileName; }
		}

		/// <summary>
		/// Returns the line number of the caller.
		/// </summary>
		/// <returns></returns>
		public string LineNumber
		{
			get { return m_lineNumber; }
		}

		/// <summary>
		/// Returns the method name of the caller.
		/// </summary>
		/// <returns></returns>
		public string MethodName
		{
			get { return m_methodName; }
		}

		/// <summary>
		/// All available caller information, in the format
		/// <c>fully.qualified.classname.of.caller.methodName(Filename:line)</c>
		/// </summary>
		/// <returns></returns>
		public string FullInfo
		{
			get { return m_fullInfo; }
		}
	}
}
