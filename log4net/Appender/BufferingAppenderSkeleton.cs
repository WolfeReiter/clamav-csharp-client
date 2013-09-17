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

using log4net.helpers;
using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Abstract base class implementation of <see cref="IAppender"/> that 
	/// buffers events in a fixed size buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This base class should be used by appenders that need to buffer a 
	/// number of events before logging them. For example the <see cref="ADONetAppender"/> 
	/// buffers events and then submits the entire contents of the buffer to 
	/// the underlying database in one go.
	/// </para>
	/// <para>
	/// Subclasses should override the <see cref="SendBuffer(LoggingEvent[])"/>
	/// method to deliver the buffered events.
	/// </para>
	/// <para>The BufferingAppenderSkeleton maintains a fixed size cyclic 
	/// buffer of events. The size of the buffer is set using 
	/// the <see cref="BufferSize"/> property.
	/// </para>
	/// <para>A <see cref="ITriggeringEventEvaluator"/> is used to inspect 
	/// each event as it arrives in the appender. If the <see cref="Evaluator"/> 
	/// triggers, then the current buffer is sent immediately 
	/// (see <see cref="SendBuffer(LoggingEvent[])"/>). Otherwise the event 
	/// is stored in the buffer. For example, an evaluator can be used to 
	/// deliver the events immediately when an ERROR event arrives.
	/// </para>
	/// <para>
	/// The buffering appender can be configured in a <see cref="Lossy"/> mode. 
	/// By default the appender is NOT lossy. When the buffer is full all 
	/// the buffered events are sent with <see cref="SendBuffer(LoggingEvent[])"/>.
	/// If the <see cref="Lossy"/> property is set to <c>true</c> then the 
	/// buffer will not be sent when it is full, and new events arriving 
	/// in the appender will overwrite the oldest event in the buffer. 
	/// In lossy mode the buffer will only be sent when the <see cref="Evaluator"/>
	/// triggers. This can be useful behaviour when you need to know about 
	/// ERROR events but not about events with a lower level, configure an 
	/// evaluator that will trigger when an ERROR event arrives, the whole 
	/// buffer will be sent which gives a history of events leading up to
	/// the ERROR event.
	/// </para>
	/// </remarks>
	public abstract class BufferingAppenderSkeleton : AppenderSkeleton
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferingAppenderSkeleton" /> class.
		/// </summary>
		protected BufferingAppenderSkeleton() : this(true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferingAppenderSkeleton" /> class.
		/// </summary>
		/// <param name="eventMustBeFixed">the events passed through this appender must be
		/// fixed by the time that they arrive in the derived class'<c>SendBuffer</c> method.</param>
		protected BufferingAppenderSkeleton(bool eventMustBeFixed) : base()
		{
			m_eventMustBeFixed = eventMustBeFixed;
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets a value that indicates whether the appender is lossy.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender is lossy, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// This appender uses a buffer to store logging events before 
		/// delivering them. A triggering event causes the whole buffer
		/// to be send to the remote sink. If the buffer overruns before
		/// a triggering event then logging events could be lost. Set
		/// <see cref="Lossy"/> to <c>false</c> to prevent logging events 
		/// from being lost.
		/// </para>
		/// <para>If <see cref="Lossy"/> is set to <c>true</c> then an
		/// <see cref="Evaluator"/> must be specified.</para>
		/// </remarks>
		public bool Lossy
		{
			get { return m_lossy; }
			set { m_lossy = value; }
		}

		/// <summary>
		/// Gets or sets the size of the cyclic buffer used to hold the 
		/// logging events.
		/// </summary>
		/// <value>
		/// The size of the cyclic buffer used to hold the logging events.
		/// </value>
		/// <remarks>
		/// The <see cref="BufferSize"/> option takes a positive integer
		/// representing the maximum number of logging events to collect in 
		/// a cyclic buffer. When the <see cref="BufferSize"/> is reached,
		/// oldest events are deleted as new events are added to the
		/// buffer. By default the size of the cyclic buffer is 512 events.
		/// </remarks>
		public int BufferSize
		{
			get { return m_bufferSize; }
			set { m_bufferSize = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ITriggeringEventEvaluator"/> that causes the 
		/// buffer to be sent immediately.
		/// </summary>
		/// <value>
		/// The <see cref="ITriggeringEventEvaluator"/> that causes the buffer to be
		/// sent immediately.
		/// </value>
		/// <remarks>
		/// <para>
		/// The evaluator will be called for each event that is appended to this 
		/// appender. If the evaluator triggers then the current buffer will 
		/// immediately be sent (see <see cref="SendBuffer(LoggingEvent[])"/>).
		/// </para>
		/// <para>If <see cref="Lossy"/> is set to <c>true</c> then an
		/// <see cref="Evaluator"/> must be specified.</para>
		/// </remarks>
		public ITriggeringEventEvaluator Evaluator
		{
			get { return m_evaluator; }
			set	{ m_evaluator = value; }
		}

		/// <summary>
		/// Gets or sets the value of the <see cref="ITriggeringEventEvaluator"/> to use.
		/// </summary>
		/// <value>
		/// The value of the <see cref="ITriggeringEventEvaluator"/> to use.
		/// </value>
		/// <remarks>
		/// <para>
		/// The evaluator will be called for each event that is discarded from this 
		/// appender. If the evaluator triggers then the current buffer will immediately 
		/// be sent (see <see cref="SendBuffer(LoggingEvent[])"/>).
		/// </para>
		/// </remarks>
		public ITriggeringEventEvaluator LossyEvaluator
		{
			get { return m_lossyEvaluator; }
			set	{ m_lossyEvaluator = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if only part of the logging event data
		/// should be fixed.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender should only fix part of the logging event 
		/// data, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Setting this property to <c>true</c> will cause only part of the
		/// event data to be fixed and serialised. This will improve performance.
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

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialise the appender based on the options set
		/// </summary>
		override public void ActivateOptions() 
		{
			base.ActivateOptions();

			// If the appender is in Lossy mode then we will
			// only send the buffer when the Evaluator triggers
			// therefore check we have an evaluator.
			if (m_lossy && m_evaluator == null)
			{
				ErrorHandler.Error("Appender ["+Name+"] is Lossy but has no Evaluator. The buffer will never be sent!"); 
			}

			m_cb = new CyclicBuffer(m_bufferSize);
		}

		#endregion Implementation of IOptionHandler

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Close this appender instance.
		/// </summary>
		/// <remarks>
		/// <para>Close this appender instance. If this appender is marked
		/// as not <see cref="Lossy"/> then the remaining events in 
		/// the buffer must be sent when the appender is closed.</para>
		/// </remarks>
		override public void OnClose() 
		{
			// If we are supposed to be non lossy then we had better
			// flush our buffer now
			if (m_cb != null && m_cb.Length > 0)
			{
				if (m_lossy)
				{
					if (m_lossyEvaluator != null)
					{
						foreach(LoggingEvent evnt in m_cb.PopAll())
						{
							if (m_lossyEvaluator.IsTriggeringEvent(evnt))
							{
								SendBuffer(new LoggingEvent[] { evnt } );
							}
						}
					}
				}
				else
				{
					SendBuffer(m_cb);
				}
			}
		}

		/// <summary>
		/// This method is called by the <see cref="AppenderSkeleton.DoAppend"/> method. 
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>Stores the <paramref name="loggingEvent"/> in the cyclic buffer.</para>
		/// 
		/// <para>The buffer will be sent (i.e. passed to the <see cref="SendBuffer"/> 
		/// method) if one of the following conditions is met:</para>
		/// 
		/// <list type="bullet">
		///		<item>
		///			<description>The cyclic buffer is full and this appender is
		///			marked as not lossy (see <see cref="Lossy"/>)</description>
		///		</item>
		///		<item>
		///			<description>An <see cref="Evaluator"/> is set and
		///			it is triggered for the <paramref name="loggingEvent"/>
		///			specified.</description>
		///		</item>
		/// </list>
		/// 
		/// <para>Before the event is stored in the buffer it is fixed
		/// (see <see cref="LoggingEvent.FixVolatileData()"/>) to ensure that
		/// any data referenced by the event will be valid when the buffer
		/// is processed.</para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// If the buffer size is set to 1 or less then the buffer will be
			// sent immediaty because there is not enough space in the buffer
			// to buffer up more than 1 event. Therefore as a special case
			// we don't use the buffer at all.
			if (m_bufferSize <= 1)
			{
				// Only send the event if we are in non lossy mode or the event is a triggering event
				if ((!m_lossy) || 
					(m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent)) || 
					(m_lossyEvaluator != null && m_lossyEvaluator.IsTriggeringEvent(loggingEvent)))
				{
					if (m_eventMustBeFixed)
					{
						// Derive class expects fixed events
						loggingEvent.Fix = this.Fix;
					}

					// Not buffering events, send immediatly
					SendBuffer(new LoggingEvent[] { loggingEvent } );
				}
			}
			else
			{
				// Because we are caching the LoggingEvent beyond the
				// lifetime of the Append() method we must fix any
				// volatile data in the event.
				loggingEvent.Fix = this.Fix;

				// catch the event discarded from the buffer
				LoggingEvent discardedLoggingEvent;

				// Add to the buffer, returns true if there is space remaining after the append
				bool space = m_cb.Append(loggingEvent, out discardedLoggingEvent);

				// Check if the discarded event should be logged
				if (discardedLoggingEvent != null && m_lossyEvaluator != null && m_lossyEvaluator.IsTriggeringEvent(discardedLoggingEvent))
				{
					SendBuffer(new LoggingEvent[] { discardedLoggingEvent } );
				}

				// If the buffer is full & not lossy then send the buffer, otherwise check if
				// the event will trigger the whole buffer to be sent
				if ((!space && !m_lossy) || (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent)) )
				{
					SendBuffer(m_cb);
				}
			}
		} 

		#endregion Override implementation of AppenderSkeleton

		#region Protected Instance Methods

		/// <summary>
		/// Sends the contents of the buffer.
		/// </summary>
		/// <param name="buffer">The buffer containing the events that need to be send.</param>
		/// <remarks>
		/// The subclass must override either <see cref="SendBuffer(CyclicBuffer)"/>
		/// or <see cref="SendBuffer(LoggingEvent[])"/>.
		/// </remarks>
		virtual protected void SendBuffer(CyclicBuffer buffer)
		{
			SendBuffer(buffer.PopAll());
		}

		/// <summary>
		/// Sends the events.
		/// </summary>
		/// <param name="events">The events that need to be send.</param>
		/// <remarks>
		/// The subclass must override either <see cref="SendBuffer(CyclicBuffer)"/>
		/// or <see cref="SendBuffer(LoggingEvent[])"/>.
		/// </remarks>
		virtual protected void SendBuffer(LoggingEvent[] events)
		{
		}

		#endregion Protected Instance Methods

		#region Private Static Fields

		/// <summary>
		/// The default buffer size.
		/// </summary>
		/// <remarks>
		/// The default size of the cyclic buffer used to store events.
		/// This is set to 512 by default.
		/// </remarks>
		private const int DEFAULT_BUFFER_SIZE = 512;

		#endregion Private Static Fields

		#region Private Instance Fields

		/// <summary>
		/// The size of the cyclic buffer used to hold the logging events.
		/// </summary>
		/// <remarks>
		/// Set to <see cref="DEFAULT_BUFFER_SIZE"/> by default.
		/// </remarks>
		private int m_bufferSize = DEFAULT_BUFFER_SIZE;

		/// <summary>
		/// The cyclic buffer used to store the logging events.
		/// </summary>
		private CyclicBuffer m_cb;

		/// <summary>
		/// The triggering event evaluator that causes the buffer to be sent immediately.
		/// </summary>
		/// <remarks>
		/// The object that is used to determine if an event causes the entire
		/// buffer to be sent immediately. This field can be <c>null</c>, which 
		/// indicates that event triggering is not to be done. The evaluator
		/// can be set using the <see cref="Evaluator"/> property. If this appender
		/// has the <see cref="m_lossy"/> (<see cref="Lossy"/> property) set to 
		/// <c>true</c> then an <see cref="Evaluator"/> must be set.
		/// </remarks>
		private ITriggeringEventEvaluator m_evaluator;

		/// <summary>
		/// Indicates if the appender should overwrite events in the cyclic buffer 
		/// when it becomes full, or if the buffer should be flushed when the 
		/// buffer is full.
		/// </summary>
		/// <remarks>
		/// If this field is set to <c>true</c> then an <see cref="Evaluator"/> must 
		/// be set.
		/// </remarks>
		private bool m_lossy = false;

		/// <summary>
		/// The triggering event evaluator filters discarded events.
		/// </summary>
		/// <remarks>
		/// The object that is used to determine if an event that is discarded should
		/// really be discarded or if it should be sent to the appenders. 
		/// This field can be <c>null</c>, which indicates that all discarded events will
		/// be discarded. 
		/// </remarks>
		private ITriggeringEventEvaluator m_lossyEvaluator;

		/// <summary>
		/// Value indicating which fields in the event should be fixed
		/// </summary>
		/// <remarks>
		/// By default all fields are fixed
		/// </remarks>
		private FixFlags m_fixFlags = FixFlags.All;

		/// <summary>
		/// The events delivered to the subclass must be fixed.
		/// </summary>
		private readonly bool m_eventMustBeFixed;

		#endregion Private Instance Fields
	}
}
