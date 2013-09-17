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
using log4net.Layout;
using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Buffers events and then forwards them to attached appenders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The events are buffered in this appender until conditions are
	/// met to allow the appender to deliver the events to the attached 
	/// appenders. See <see cref="BufferingAppenderSkeleton"/> for the
	/// conditions that cause the buffer to be sent.
	/// </para>
	/// <para>The forwarding appender can be used to specify different 
	/// thresholds and filters for the same appender at different locations 
	/// within the hierarchy.
	/// </para>
	/// </remarks>
	public class BufferingForwardingAppender : BufferingAppenderSkeleton, IAppenderAttachable
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferingForwardingAppender" /> class.
		/// </summary>
		public BufferingForwardingAppender()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Closes the appender and releases resources.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Releases any resources allocated within the appender such as file handles, 
		/// network connections, etc.
		/// </para>
		/// <para>
		/// It is a programming error to append to a closed appender.
		/// </para>
		/// </remarks>
		override public void OnClose()
		{
			// Remove all the attached appenders
			lock(this)
			{
				// Delegate to base, which will flush buffers
				base.OnClose();

				if (m_aai != null)
				{
					m_aai.RemoveAllAppenders();
				}
			}
		}

		#endregion Override implementation of AppenderSkeleton

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Send the events.
		/// </summary>
		/// <param name="events">The events that need to be send.</param>
		/// <remarks>
		/// <para>
		/// Forwards the events to the attached appenders.
		/// </para>
		/// </remarks>
		override protected void SendBuffer(LoggingEvent[] events)
		{
			// Pass the logging event on to the attached appenders
			if (m_aai != null)
			{
				// Send each event one at a time
				foreach(LoggingEvent e in events)
				{
					m_aai.AppendLoopOnAppenders(e);
				}
			}
		}

		#endregion Override implementation of BufferingAppenderSkeleton

		#region Implementation of IAppenderAttachable

		/// <summary>
		/// Adds an <see cref="IAppender" /> to the list of appenders of this
		/// instance.
		/// </summary>
		/// <param name="newAppender">The <see cref="IAppender" /> to add to this appender.</param>
		/// <remarks>
		/// <para>
		/// If the specified <see cref="IAppender" /> is already in the list of
		/// appenders, then it won't be added again.
		/// </para>
		/// </remarks>
		virtual public void AddAppender(IAppender newAppender) 
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			lock(this)
			{
				if (m_aai == null) 
				{
					m_aai = new log4net.helpers.AppenderAttachedImpl();
				}
				m_aai.AddAppender(newAppender);
			}
		}

		/// <summary>
		/// Gets the appenders contained in this appender as an 
		/// <see cref="System.Collections.ICollection"/>.
		/// </summary>
		/// <remarks>
		/// If no appenders can be found, then an <see cref="EmptyCollection"/> 
		/// is returned.
		/// </remarks>
		/// <returns>
		/// A collection of the appenders in this appender.
		/// </returns>
		virtual public AppenderCollection Appenders
		{
			get
			{
				lock(this)
				{
					if (m_aai == null)
					{
						return AppenderCollection.EmptyCollection;
					}
					else 
					{
						return m_aai.Appenders;
					}
				}
			}
		}

		/// <summary>
		/// Looks for the appender with the specified name.
		/// </summary>
		/// <param name="name">The name of the appender to lookup.</param>
		/// <returns>
		/// The appender with the specified name, or <c>null</c>.
		/// </returns>
		virtual public IAppender GetAppender(string name) 
		{
			lock(this)
			{
				if (m_aai == null || name == null)
				{
					return null;
				}

				return m_aai.GetAppender(name);
			}
		}

		/// <summary>
		/// Removes all previously added appenders from this appender.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is useful when re-reading configuration information.
		/// </para>
		/// </remarks>
		virtual public void RemoveAllAppenders() 
		{
			lock(this)
			{
				if (m_aai != null) 
				{
					m_aai.RemoveAllAppenders();
					m_aai = null;
				}
			}
		}

		/// <summary>
		/// Removes the specified appender from the list of appenders.
		/// </summary>
		/// <param name="appender">The appender to remove.</param>
		virtual public void RemoveAppender(IAppender appender) 
		{
			lock(this)
			{
				if (appender != null && m_aai != null) 
				{
					m_aai.RemoveAppender(appender);
				}
			}
		}

		/// <summary>
		/// Removes the appender with the specified name from the list of appenders.
		/// </summary>
		/// <param name="name">The name of the appender to remove.</param>
		virtual public void RemoveAppender(string name) 
		{
			lock(this)
			{
				if (name != null && m_aai != null)
				{
					m_aai.RemoveAppender(name);
				}
			}
		}
  
		#endregion Implementation of IAppenderAttachable

		#region Private Instance Fields

		/// <summary>
		/// Implementation of the <see cref="IAppenderAttachable"/> interface
		/// </summary>
		private AppenderAttachedImpl m_aai;

		#endregion Private Instance Fields
	}
}
