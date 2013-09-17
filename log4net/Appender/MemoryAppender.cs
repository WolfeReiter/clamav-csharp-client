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
using System.Collections;

using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Stores logging events in an array.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The memory appender stores all the logging events
	/// that are appended in an in-memory array.
	/// </para>
	/// <para>
	/// Use the <see cref="Events"/> property to get
	/// the current list of events that have been appended.
	/// </para>
	/// <para>
	/// Use the <see cref="Clear()"/> method to clear the
	/// current list of events.
	/// </para>
	/// </remarks>
	public class MemoryAppender : AppenderSkeleton
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryAppender" /> class.
		/// </summary>
		public MemoryAppender() : base()
		{
			m_eventsList = new ArrayList();
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the events that have been logged.
		/// </summary>
		/// <value>
		/// The events that have been logged.
		/// </value>
		virtual public LoggingEvent[] Events
		{
			get { return (LoggingEvent[])m_eventsList.ToArray(typeof(LoggingEvent)); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether only part of the logging event 
		/// data should be fixed.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender should only fix part of the logging event 
		/// data, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Setting this property to <c>true</c> will cause only part of the event 
		/// data to be fixed and stored in the appender, hereby improving performace. 
		/// </para>
		/// <para>
		/// See <see cref="LoggingEvent.FixVolatileData(bool)"/> for more information.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		virtual public bool OnlyFixPartialEventData
		{
			get { return (Fix == FixFlags.Partial); }
			set 
			{ 
				if (value)
				{
					Fix = FixFlags.Partial;
				}
				else
				{
					Fix = FixFlags.All;
				}
			}
		}

		/// <summary>
		/// Gets or sets a the fields that will be fixed in the event
		/// </summary>
		virtual public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { m_fixFlags = value; }
		}

		#endregion Public Instance Properties

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This method is called by the <see cref="AppenderSkeleton.DoAppend"/> method. 
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>Stores the <paramref name="loggingEvent"/> in the events list.</para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// Because we are caching the LoggingEvent beyond the
			// lifetime of the Append() method we must fix any
			// volatile data in the event.
			loggingEvent.Fix = this.Fix;

			m_eventsList.Add(loggingEvent);
		} 

		#endregion Override implementation of AppenderSkeleton

		#region Public Instance Methods

		/// <summary>
		/// Clear the list of events
		/// </summary>
		/// <remarks>
		/// Clear the list of events
		/// </remarks>
		virtual public void Clear()
		{
			m_eventsList.Clear();
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The list of events that have been appended.
		/// </summary>
		private ArrayList m_eventsList;

		/// <summary>
		/// Value indicating which fields in the event should be fixed
		/// </summary>
		/// <remarks>
		/// By default all fields are fixed
		/// </remarks>
		private FixFlags m_fixFlags = FixFlags.All;

		#endregion Private Instance Fields
	}
}
