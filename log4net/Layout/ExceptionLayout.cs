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
	/// A Layout that renders only the Exception text from the logging event
	/// </summary>
	/// <remarks>
	/// <para>A Layout that renders only the Exception text from the logging event</para>
	/// <para>This Layout should only be used with appenders that utilise multiple
	/// layouts (e.g. <see cref="log4net.Appender.ADONetAppender"/>).</para>
	/// </remarks>
	public class ExceptionLayout : LayoutSkeleton
	{
		#region Constructors

		/// <summary>
		/// Constructs a ExceptionLayout
		/// </summary>
		/// <remarks>
		/// </remarks>
		public ExceptionLayout()
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
		/// The ExceptionLayout only handles the exception. Thus, it returns <c>false</c>.
		/// </summary>
		/// <value>
		/// The ExceptionLayout only handles the exception. Thus, it returns <c>false</c>.
		/// </value>
		/// <remarks>
		/// The ExceptionLayout only handles the exception. Thus, it returns <c>false</c>.
		/// </remarks>
		override public bool IgnoresException
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the exception text from the logging event
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>the formatted string</returns>
		override public string Format(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			return loggingEvent.GetExceptionStrRep();
		}

		#endregion
	}
}
