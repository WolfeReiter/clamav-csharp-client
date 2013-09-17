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
using System.Globalization;

using log4net.Layout;
using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Appends logging events to the console.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ConsoleAppender appends log events to the standard output stream
	/// or the error output stream using a layout specified by the 
	/// user.
	/// </para>
	/// <para>
	/// By default, all output is written to the console's standard output stream.
	/// The <see cref="Target"/> property can be set to direct the output to the
	/// error stream.
	/// </para>
	/// <para>
	/// NOTE: This appender writes each message to the <c>System.Console.Out</c> or 
	/// <c>System.Console.Error</c> that is set at the time the event is appended.
	/// Therefore it is possible to progamatically redirect the output of this appender 
	/// (for example NUnit does this to capture program ouput). While this is the desired
	/// behaviour of this appender it may have security implications in your application. 
	/// </para>
	/// </remarks>
	public class ConsoleAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsoleAppender" /> class.
		/// </summary>
		/// <remarks>
		/// The instance of the <see cref="ConsoleAppender" /> class is set up to write 
		/// to the standard output stream.
		/// </remarks>
		public ConsoleAppender() 
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsoleAppender" /> class
		/// with the specified layout.
		/// </summary>
		/// <param name="layout">the layout to use for this appender</param>
		/// <remarks>
		/// The instance of the <see cref="ConsoleAppender" /> class is set up to write 
		/// to the standard output stream.
		/// </remarks>
		public ConsoleAppender(ILayout layout) : this(layout, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsoleAppender" /> class
		/// with the specified layout.
		/// </summary>
		/// <param name="layout">the layout to use for this appender</param>
		/// <param name="writeToErrorStream">flag set to <c>true</c> to write to the console error stream</param>
		/// <remarks>
		/// When <paramref name="writeToErrorStream" /> is set to <c>true</c>, output is written to
		/// the standard error output stream.  Otherwise, output is written to the standard
		/// output stream.
		/// </remarks>
		public ConsoleAppender(ILayout layout, bool writeToErrorStream) 
		{
			Layout = layout;
			m_writeToErrorStream = writeToErrorStream;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Target is the value of the console output stream.
		/// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
		/// </summary>
		/// <value>
		/// Target is the value of the console output stream.
		/// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
		/// </value>
		virtual public string Target
		{
			get { return m_writeToErrorStream ? CONSOLE_ERR : CONSOLE_OUT; }
			set
			{
				string v = value.Trim();
				
				if (string.Compare(CONSOLE_ERR, v, true, CultureInfo.InvariantCulture) == 0) 
				{
					m_writeToErrorStream = true;
				} 
				else 
				{
					m_writeToErrorStream = false;
				}
			}
		}

		#endregion Public Instance Properties

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This method is called by the <see cref="AppenderSkeleton.DoAppend"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes the event to the console.
		/// </para>
		/// <para>
		/// The format of the output will depend on the appender's layout.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
#if NETCF
			// Write to the output stream
			Console.Write(RenderLoggingEvent(loggingEvent));
#else
			if (m_writeToErrorStream)
			{
				// Write to the error stream
				Console.Error.Write(RenderLoggingEvent(loggingEvent));
			}
			else
			{
				// Write to the output stream
				Console.Write(RenderLoggingEvent(loggingEvent));
			}
#endif
		}

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion Override implementation of AppenderSkeleton

		#region Public Static Fields

		/// <summary>
		/// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console 
		/// standard output stream.
		/// </summary>
		public const string CONSOLE_OUT = "Console.Out";

		/// <summary>
		/// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console 
		/// standard error output stream.
		/// </summary>
		public const string CONSOLE_ERR = "Console.Error";

		#endregion Public Static Fields

		#region Private Instances Fields

		private bool m_writeToErrorStream = false;

		#endregion Private Instances Fields
	}
}
