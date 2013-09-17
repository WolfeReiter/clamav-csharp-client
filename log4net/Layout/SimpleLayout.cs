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
using System.Text;

using log4net.spi;
using log4net.helpers;

namespace log4net.Layout
{
	/// <summary>
	/// A very simple layout
	/// </summary>
	/// <remarks>
	/// SimpleLayout consists of the level of the log statement,
	/// followed by " - " and then the log message itself. For example,
	/// <code>
	/// DEBUG - Hello world
	/// </code>
	/// </remarks>
	public class SimpleLayout : LayoutSkeleton
	{
		#region Constants

		/// <summary>
		/// Initial buffer size
		/// </summary>
  		protected const int BUF_SIZE = 256;

		/// <summary>
		/// Maximum buffer size before it is recycled
		/// </summary>
		protected const int MAX_CAPACITY = 1024;

		#endregion

		#region Member Variables
  
		/// <summary>
		/// output buffer appended to when Format() is invoked
		/// </summary>
		private StringBuilder m_sbuf = new StringBuilder(BUF_SIZE);
  
		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a SimpleLayout
		/// </summary>
		/// <remarks>
		/// </remarks>
		public SimpleLayout()
		{
		}

		#endregion
  
		#region Implementation of IOptionHandler

		/// <summary>
		/// Does not do anything as options become effective immediately.
		/// </summary>
		override public void ActivateOptions() 
		{
			// nothing to do.
		}

		#endregion

		#region Override implementation of LayoutSkeleton

		/// <summary>
		/// The SimpleLayout does not handle the exception contained within
		/// LoggingEvents. Thus, it returns <c>true</c>.
		/// </summary>
		override public bool IgnoresException
		{
			get { return true; }
		}

		/// <summary>
		/// Produces a formatted string.
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>the formatted string</returns>
		override public string Format(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			// Reset working string buffer
			if (m_sbuf.Capacity > MAX_CAPACITY) 
			{
				m_sbuf = new StringBuilder(BUF_SIZE);
			} 
			else 
			{
				m_sbuf.Length = 0;
			}

			m_sbuf
				.Append(loggingEvent.Level)
				.Append(" - ")
				.Append(loggingEvent.RenderedMessage)
				.Append(SystemInfo.NewLine);

			return m_sbuf.ToString();
		}

		#endregion
	}
}
