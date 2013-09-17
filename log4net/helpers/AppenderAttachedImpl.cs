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

using log4net.spi;
using log4net.Appender;

namespace log4net.helpers
{
	/// <summary>
	/// A straightforward implementation of the <see cref="IAppenderAttachable"/> interface.
	/// </summary>
	public class AppenderAttachedImpl : IAppenderAttachable
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AppenderAttachedImpl"/> class.
		/// </summary>
		public AppenderAttachedImpl()
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods

		/// <summary>
		/// Calls the <see cref="IAppender.DoAppend" /> method on all 
		/// attached appenders.
		/// </summary>
		/// <param name="loggingEvent">The event being logged.</param>
		/// <returns>The number of appenders called.</returns>
		public int AppendLoopOnAppenders(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (m_appenderList == null) 
			{
				return 0;
			}

			foreach(IAppender appender in m_appenderList)
			{
				try
				{
					appender.DoAppend(loggingEvent);
				}
				catch(Exception ex)
				{
					LogLog.Error("AppenderAttachedImpl: Failed to append to appender [" + appender.Name + "]", ex);
				}
			}
			return m_appenderList.Count;
		}

		#endregion Public Instance Methods

		#region Implementation of IAppenderAttachable

		/// <summary>
		/// Attaches an appender.
		/// </summary>
		/// <param name="newAppender">The appender to add.</param>
		/// <remarks>
		/// If the appender is already in the list it won't be added again.
		/// </remarks>
		public void AddAppender(IAppender newAppender) 
		{
			// Null values for newAppender parameter are strictly forbidden.
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
	
			if (m_appenderList == null) 
			{
				m_appenderList = new AppenderCollection(1);
			}
			if (!m_appenderList.Contains(newAppender))
			{
				m_appenderList.Add(newAppender);
			}
		}

		/// <summary>
		/// Gets all attached appenders.
		/// </summary>
		/// <returns>
		/// A collection of attached appenders, or <c>null</c> if there
		/// are no attached appenders.
		/// </returns>
		public AppenderCollection Appenders 
		{
			get
			{
				if (m_appenderList == null)
				{
					// We must always return a valid collection
					return AppenderCollection.EmptyCollection;
				}
				else 
				{
					return m_appenderList;
				}
			}
		}

		/// <summary>
		/// Gets an attached appender with the specified name.
		/// </summary>
		/// <param name="name">The name of the appender to get.</param>
		/// <returns>
		/// The appender with the name specified, or <c>null</c> if no appender with the
		/// specified name is found.
		/// </returns>
		public IAppender GetAppender(string name) 
		{
			if (m_appenderList != null && name != null)
			{
				foreach(IAppender appender in m_appenderList)
				{
					if (name == appender.Name)
					{
						return appender;
					}
				}
			}
			return null;   
		}

		/// <summary>
		/// Removes all attached appenders.
		/// </summary>
		public void RemoveAllAppenders() 
		{
			if (m_appenderList != null) 
			{
				foreach(IAppender appender in m_appenderList)
				{
					try
					{
						appender.Close();
					}
					catch(Exception ex)
					{
						LogLog.Error("AppenderAttachedImpl: Failed to Close appender ["+appender.Name+"]", ex);
					}
				}
				m_appenderList.Clear();
				m_appenderList = null;	  
			}
		}

		/// <summary>
		/// Removes the specified appender from the list of attached appenders.
		/// </summary>
		/// <param name="appender">The appender to remove.</param>
		public void RemoveAppender(IAppender appender) 
		{
			if (appender != null && m_appenderList != null) 
			{
				m_appenderList.Remove(appender);	
			}
		}

		/// <summary>
		/// Removes the appender with the specified name from the list of appenders.
		/// </summary>
		/// <param name="name">The name of the appender to remove.</param>
		public void RemoveAppender(string name) 
		{
			RemoveAppender(GetAppender(name));
		}

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// Array of appenders
		/// </summary>
		private AppenderCollection m_appenderList;

		#endregion Private Instance Fields
	}
}
