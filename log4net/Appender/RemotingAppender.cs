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

using log4net.Layout;
using log4net.spi;
using log4net.helpers;

namespace log4net.Appender
{
	/// <summary>
	/// Delivers logging events to a remote logging sink. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// This Appender is designed to deliver events to a remote sink. 
	/// That is any object that implements the <see cref="IRemoteLoggingSink"/>
	/// interface. It delivers the events using .NET remoting. The
	/// object to deliver events to is specified by setting the
	/// appenders <see cref="RemotingAppender.Sink"/> property.</para>
	/// <para>This appender sets the <c>hostname</c> property in the 
	/// <see cref="LoggingEvent.Properties"/> collection to the name of 
	/// the machine on which the event is logged.</para>
	/// </remarks>
	/// <seealso cref="IRemoteLoggingSink" />
	public class RemotingAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RemotingAppender" /> class.
		/// </summary>
		public RemotingAppender()
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the URL of the well-known object that will accept 
		/// the logging events.
		/// </summary>
		/// <value>
		/// The well-known URL of the remote sink.
		/// </value>
		public string Sink
		{
			get { return m_sinkUrl; }
			set { m_sinkUrl = value; }
		}

		#endregion Public Instance Properties

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialise the appender based on the options set
		/// </summary>
		override public void ActivateOptions() 
		{
			base.ActivateOptions();
			m_sinkObj = (IRemoteLoggingSink)Activator.GetObject(typeof(IRemoteLoggingSink), m_sinkUrl);
		}

		#endregion

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Send the contents of the buffer to the remote sink.
		/// </summary>
		/// <param name="events">The events to send.</param>
		override protected void SendBuffer(LoggingEvent[] events)
		{
			string hostName = SystemInfo.HostName;

			// Set the hostname
			foreach(LoggingEvent e in events)
			{
				if (e.Properties[LoggingEvent.HostNameProperty] == null)
				{
					e.Properties[LoggingEvent.HostNameProperty] = hostName;
				}
			}

			// Send the events
			m_sinkObj.LogEvents(events);
		}

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// The URL of the remote sink.
		/// </summary>
		private string m_sinkUrl;

		/// <summary>
		/// The local proxy (.NET remoting) for the remote logging sink.
		/// </summary>
		private IRemoteLoggingSink m_sinkObj;

		#endregion Private Instance Fields

		/// <summary>
		/// Interface used to deliver <see cref="LoggingEvent"/> objects to a remote sink.
		/// </summary>
		/// <remarks>
		/// This interface must be implemented by a remoting sink
		/// if the <see cref="RemotingAppender"/> is to be used
		/// to deliver logging events to the sink.
		/// </remarks>
		public interface IRemoteLoggingSink
		{
			/// <summary>
			/// Delivers logging events to the remote sink
			/// </summary>
			/// <param name="events">Array of events to log.</param>
			void LogEvents(LoggingEvent[] events);
		}
	}
}
