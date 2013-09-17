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
using System.Runtime.Serialization;
using System.Collections;
#if (!NETCF)
using System.Security.Principal;
#endif

using log4net.helpers;
using log4net.Repository;

namespace log4net.spi
{
	/// <summary>
	/// Portable data structure used by <see cref="LoggingEvent"/>
	/// </summary>
	public struct LoggingEventData
	{
		#region Public Instance Fields

		/// <summary>
		/// The logger name.
		/// </summary>
		public string LoggerName;

		/// <summary>
		/// Level of logging event. Level cannot be Serializable
		/// because it is a flyweight.  Due to its special serialization it
		/// cannot be declared final either.
		/// </summary>
		public Level Level;

		/// <summary>
		/// The nested diagnostic context (NDC) of logging event.
		/// </summary>
		public string NDC;

		/// <summary>
		/// The local cache of the MDC dictionary
		/// </summary>
		public IDictionary MDC;

		/// <summary>
		/// The application supplied message of logging event.
		/// </summary>
		public string Message;

		/// <summary>
		/// The name of thread in which this logging event was generated
		/// </summary>
		public string ThreadName;

		/// <summary>
		/// The time the event was logged
		/// </summary>
		public DateTime TimeStamp;

		/// <summary>
		/// Location information for the caller.
		/// </summary>
		public LocationInfo LocationInfo;

		/// <summary>
		/// String representation of the user's windows name,
		/// like DOMAIN\username
		/// </summary>
		public string UserName;

		/// <summary>
		/// String representation of the current thread's principal identity.
		/// </summary>
		public string Identity;

		/// <summary>
		/// The string representation of the exception
		/// </summary>
		public string ExceptionString;

		/// <summary>
		/// String representation of the AppDomain.
		/// </summary>
		public string Domain;

		/// <summary>
		/// Additional event specific properties
		/// </summary>
		/// <remarks>
		/// A logger or an appender may attach additional
		/// properties to specific events. These properties
		/// have a string key and an object value.
		/// </remarks>
		public PropertiesCollection Properties;

		#endregion Public Instance Fields
	}

	/// <summary>
	/// Flags passed to the <see cref="LoggingEvent.Fix"/> property
	/// </summary>
	[Flags] public enum FixFlags
	{
		/// <summary>
		/// Fix the MDC
		/// </summary>
		Mdc = 0x01,

		/// <summary>
		/// Fix the NDC
		/// </summary>
		Ndc = 0x02,

		/// <summary>
		/// Fix the rendered message
		/// </summary>
		Message = 0x04,

		/// <summary>
		/// Fix the thread name
		/// </summary>
		ThreadName = 0x08,

		/// <summary>
		/// Fix the callers location information
		/// </summary>
		/// <remarks>
		/// CAUTION: Very slow to generate
		/// </remarks>
		LocationInfo = 0x10,

		/// <summary>
		/// Fix the callers windows user name
		/// </summary>
		/// <remarks>
		/// CAUTION: Slow to generate
		/// </remarks>
		UserName = 0x20,

		/// <summary>
		/// Fix the domain friendly name
		/// </summary>
		Domain = 0x40,

		/// <summary>
		/// Fix the callers principal name
		/// </summary>
		/// <remarks>
		/// CAUTION: May be slow to generate
		/// </remarks>
		Identity = 0x80,

		/// <summary>
		/// Fix the exception text
		/// </summary>
		Exception = 0x100,

		/// <summary>
		/// No fields fixed
		/// </summary>
		None = 0x0,

		/// <summary>
		/// All fields fixed
		/// </summary>
		All = 0xFFFFFFF,

		/// <summary>
		/// Partial fields fixed
		/// </summary>
		/// <remarks>
		/// <para>
		/// This set of partial fields gives good performance. The following fields are fixed:
		/// </para>
		/// <list type="bullet">
		/// <item><description><see cref="Mdc"/></description></item>
		/// <item><description><see cref="Ndc"/></description></item>
		/// <item><description><see cref="Message"/></description></item>
		/// <item><description><see cref="ThreadName"/></description></item>
		/// <item><description><see cref="Exception"/></description></item>
		/// <item><description><see cref="Domain"/></description></item>
		/// </list>
		/// </remarks>
		Partial = Mdc | Ndc | Message | ThreadName | Exception | Domain,
	}

	/// <summary>
	/// The internal representation of logging events. 
	/// </summary>
	/// <remarks>
	/// <para>When an affirmative decision is made to log then a 
	/// <see cref="LoggingEvent"/> instance is created. This instance 
	/// is passed around to the different log4net components.</para>
	/// 
	/// <para>This class is of concern to those wishing to extend log4net.</para>
	/// 
	/// <para>Some of the values in instances of <see cref="LoggingEvent"/>
	/// are considered volatile, that is the values are correct at the
	/// time the event is delivered to appenders, but will not be consistent
	/// at any time afterwards. If an event is to be stored and then processed
	/// at a later time these volatile values must be fixed by calling
	/// <see cref="FixVolatileData()"/>. There is a performance penalty
	/// for incurred by calling <see cref="FixVolatileData()"/> but it
	/// is essential to maintaining data consistency.</para>
	/// </remarks>
#if !NETCF
	[Serializable]
#endif
	public class LoggingEvent 
#if !NETCF
		: ISerializable
#endif
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class
		/// from the supplied parameters.
		/// </summary>
		/// <param name="fullNameOfLoggerClass">Fully qualified classname of the logger.</param>
		/// <param name="repository">The repository this event is logged in.</param>
		/// <param name="loggerName">The name of the logger of this event.</param>
		/// <param name="level">The level of this event.</param>
		/// <param name="message">The message of this event.</param>
		/// <param name="exception">The exception for this event.</param>
		/// <remarks>
		/// <para>
		/// Except <see cref="TimeStamp"/>, <see cref="Level"/> and <see cref="LoggerName"/>, 
		/// all fields of <c>LoggingEvent</c> are filled when actually needed. Call
		/// <see cref="FixVolatileData()"/> to cache all data locally
		/// to prevent inconsistencies.
		/// </para>
		/// <para>This method is called by the log4net framework
		/// to create a logging event.
		/// </para>
		/// </remarks>
		public LoggingEvent(string fullNameOfLoggerClass, log4net.Repository.ILoggerRepository repository, string loggerName, Level level, object message, Exception exception) 
		{
			m_fqnOfLoggerClass = fullNameOfLoggerClass;
			m_message = message;
			m_repository = repository;
			m_thrownException = exception;

			m_data.LoggerName = loggerName;
			m_data.Level = level;
			m_data.TimeStamp = DateTime.Now;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// using specific data.
		/// </summary>
		/// <param name="data">Data used to initialise the logging event.</param>
		/// <remarks>
		/// <para>
		/// This constructor is provided to allow a <see cref="LoggingEvent" />
		/// to be created independently of the log4net framework. This can
		/// be useful if you require a custom serialization scheme.
		/// </para>
		/// <para>
		/// Use the <see cref="GetLoggingEventData"/> method to obtain an 
		/// instance of the <see cref="LoggingEventData"/> class.</para>
		/// </remarks>
		public LoggingEvent(LoggingEventData data)
		{
			m_data = data;
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

#if !NETCF

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected LoggingEvent(SerializationInfo info, StreamingContext context) 
		{
			m_data.LoggerName = info.GetString("LoggerName");

			// Note we are desrialising the whole level object. That is the
			// name and the value. This value is correct for the source 
			// hierarchy but may not be for the target hierarchy that this
			// event may be relogged into. If it is to be relogged it may
			// be nessasary to relookup the level based only on the name.
			m_data.Level = (Level)info.GetValue("Level", typeof(Level));

			m_data.NDC = info.GetString("NDC");
			m_data.MDC = (IDictionary) info.GetValue("MDC", typeof(IDictionary));
			m_data.Message = info.GetString("Message");
			m_data.ThreadName = info.GetString("ThreadName");
			m_data.TimeStamp = info.GetDateTime("TimeStamp");
			m_data.LocationInfo = (LocationInfo) info.GetValue("LocationInfo", typeof(LocationInfo));
			m_data.UserName = info.GetString("UserName");
			m_data.ExceptionString = info.GetString("ExceptionString");
			m_data.Properties = (PropertiesCollection) info.GetValue("Properties", typeof(PropertiesCollection));
			m_data.Domain = info.GetString("Domain");
			m_data.Identity = info.GetString("Identity");
		}

#endif

		#endregion Protected Instance Constructors

		#region Public Instance Properties
	
		/// <summary>
		/// Gets the time when the application started, in milliseconds elapsed 
		/// since 01.01.1970.
		/// </summary>
		/// <value>
		/// The time when the application started, in milliseconds elapsed 
		/// since 01.01.1970.
		/// </value>
		public static DateTime StartTime
		{
			get { return s_startTime; }
		}

		/// <summary>
		/// Gets the <see cref="Level" /> of the logging event.
		/// </summary>
		/// <value>
		/// The <see cref="Level" /> of the logging event.
		/// </value>
		public Level Level
		{
			get { return m_data.Level; } 
		}

		/// <summary>
		/// Gets the time of the logging event.
		/// </summary>
		/// <value>
		/// The time of the logging event.
		/// </value>
		public DateTime TimeStamp
		{
			get { return m_data.TimeStamp; }
		}

		/// <summary>
		/// Gets the name of the logger that logged the event.
		/// </summary>
		/// <value>
		/// The name of the logger that logged the event.
		/// </value>
		public string LoggerName
		{
			get { return m_data.LoggerName; }
		}

		/// <summary>
		/// Gets the location information for this logging event.
		/// </summary>
		/// <value>
		/// The location information for this logging event.
		/// </value>
		/// <remarks>
		/// The collected information is cached for future use.
		/// </remarks>
		public LocationInfo LocationInformation
		{
			get
			{
				if (m_data.LocationInfo == null) 
				{
					m_data.LocationInfo = new LocationInfo(m_fqnOfLoggerClass);
				}
				return m_data.LocationInfo;
			}
		}

		/// <summary>
		/// Gets the text of the <see cref="NDC"/>.
		/// </summary>
		/// <value>
		/// The text of the <see cref="NDC"/>.
		/// </value>
		public string NestedContext
		{
			get
			{
				if (m_data.NDC == null) 
				{
					m_data.NDC = NDC.Get();
					if (m_data.NDC == null)
					{
						m_data.NDC = "";
					}
				}
				return m_data.NDC; 
			}
		}

		/// <summary>
		/// Get the MDC dictionary.
		/// </summary>
		public IDictionary MappedContext
		{
			get
			{
				if (m_data.MDC == null)
				{
					// This creates a live copy of the MDC
					m_data.MDC = MDC.GetMap();
				}
				return m_data.MDC;
			}
		}

		/// <summary>
		/// Gets the message object used to initialise this event.
		/// </summary>
		/// <value>
		/// The message object used to initialise this event.
		/// </value>
		/// <remarks>
		/// Gets the message object used to initialise this event.
		/// Note that this event may not have a valid message object.
		/// If the event is serialised the message object will not 
		/// be transferred. To get the text of the message the
		/// <see cref="RenderedMessage"/> property must be used 
		/// not this property.
		/// If there is no defined message object for this event then
		/// null will be returned.
		/// </remarks>
		public object MessageObject
		{
			get 
			{ 
				return m_message;
			}
		}  

		/// <summary>
		/// Gets the message, rendered through the <see cref="ILoggerRepository.RendererMap" />.
		/// </summary>
		/// <value>
		/// The message rendered through the <see cref="ILoggerRepository.RendererMap" />.
		/// </value>
		/// <remarks>
		/// The collected information is cached for future use.
		/// </remarks>
		public string RenderedMessage
		{
			get 
			{ 
				if (m_data.Message == null)
				{
					if (m_message == null)
					{
						m_data.Message = "";
					}
					else if (m_message is string)
					{
						m_data.Message = (m_message as string);
					}
					else
					{
						m_data.Message = m_repository.RendererMap.FindAndRender(m_message);
					}
				}
				return m_data.Message; 
			}
		}  

		/// <summary>
		/// Gets the name of the current thread.  
		/// </summary>
		/// <value>
		/// The name of the current thread, or the thread ID when 
		/// the name is not available.
		/// </value>
		/// <remarks>
		/// The collected information is cached for future use.
		/// </remarks>
		public string ThreadName
		{
			get
			{
				if (m_data.ThreadName == null)
				{
#if NETCF
					// Get thread ID only
					m_data.ThreadName = SystemInfo.CurrentThreadId.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
#else
					m_data.ThreadName = System.Threading.Thread.CurrentThread.Name;
					if (m_data.ThreadName == null || m_data.ThreadName.Length == 0)
					{
						// The thread name is not available. Therefore we
						// go the the AppDomain to get the ID of the 
						// current thread. (Why don't Threads know their own ID?)
						try
						{
							m_data.ThreadName = SystemInfo.CurrentThreadId.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
						}
						catch(System.Security.SecurityException)
						{
							// This security exception will occur if the caller does not have 
							// some undefined set of SecurityPermission flags.
							LogLog.Debug("LoggingEvent: Security exception while trying to get current thread ID. Error Ingnored. Empty thread name.");

							m_data.ThreadName = "";
						}
					}
#endif
				}
				return m_data.ThreadName;
			}
		}

		/// <summary>
		/// Gets the name of the current user.
		/// </summary>
		/// <value>
		/// The name of the current user, or <c>NOT AVAILABLE</c> when the 
		/// underlying runtime has no support for retrieving the name of the 
		/// current user.
		/// </value>
		/// <remarks>
		/// <para>
		/// Calls <c>WindowsIdentity.GetCurrent().Name</c> to get the name of
		/// the current windows user.
		/// </para>
		/// <para>
		/// To improve performance, we could cache the string representation of 
		/// the name, and reuse that as long as the identity stayed constant.  
		/// Once the identity changed, we would need to re-assign and re-render 
		/// the string.
		/// </para>
		/// <para>
		/// However, the <c>WindowsIdentity.GetCurrent()</c> call seems to 
		/// return different objects every time, so the current implementation 
		/// doesn't do this type of caching.
		/// </para>
		/// <para>
		/// Timing for these operations:
		/// </para>
		/// <code>
		/// 00:00:00.2031250 sec, 10000 loops, WindowsIdentity.GetCurrent()
		/// 00:00:08.0468750 sec, 10000 loops, WindowsIdentity.GetCurrent().Name
		/// </code>
		/// <para>
		/// This means we could speed things up almost 40 times by caching the 
		/// value of the <c>WindowsIdentity.GetCurrent().Name</c> property, since 
		/// this takes (8.04-0.20) = 7.84375 seconds.
		/// </para>
		/// </remarks>
		public string UserName
		{
			get
			{
				if (m_data.UserName == null) 
				{
#if (NETCF || SSCLI)
					// On compact framework there's no notion of current Windows user
					m_data.UserName = "NOT AVAILABLE";
#else
					try
					{
						WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
						if (windowsIdentity != null && windowsIdentity.Name != null)
						{
							m_data.UserName = windowsIdentity.Name;
						}
						else
						{
							m_data.UserName = "";
						}
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// some undefined set of SecurityPermission flags.
						LogLog.Debug("LoggingEvent: Security exception while trying to get current windows identity. Error Ignored. Empty user name.");

						m_data.UserName = "";
					}
#endif
				}
				return m_data.UserName;
			}
		}

		/// <summary>
		/// Gets the identity of the current thread principal.
		/// </summary>
		/// <value>
		/// The string name of the identity of the current thread principal.
		/// </value>
		/// <remarks>
		/// <para>
		/// Calls <c>System.Threading.Thread.CurrentPrincipal.Identity.Name</c> to get
		/// the name of the current thread principal.
		/// </para>
		/// </remarks>
		public string Identity
		{
			get
			{
				if (m_data.Identity == null)
				{
#if (NETCF || SSCLI)
					// On compact framework there's no notion of current thread principals
					m_data.Identity = "NOT AVAILABLE";
#else
					try
					{
						if (System.Threading.Thread.CurrentPrincipal != null && 
							System.Threading.Thread.CurrentPrincipal.Identity != null &&
							System.Threading.Thread.CurrentPrincipal.Identity.Name != null)
						{
							m_data.Identity = System.Threading.Thread.CurrentPrincipal.Identity.Name;
						}
						else
						{
							m_data.Identity = "";
						}
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// some undefined set of SecurityPermission flags.
						LogLog.Debug("LoggingEvent: Security exception while trying to get current thread principal. Error Ignored. Empty identity name.");

						m_data.Identity = "";
					}
#endif
				}
				return m_data.Identity;
			}
		}

		/// <summary>
		/// Gets the AppDomain friendly name.
		/// </summary>
		public string Domain
		{
			get 
			{ 
				if (m_data.Domain == null)
				{
					m_data.Domain = SystemInfo.ApplicationFriendlyName;
				}
				return m_data.Domain; 
			}
		}

		/// <summary>
		/// Gets additional event specific properties.
		/// </summary>
		/// <value>
		/// Additional event specific properties.
		/// </value>
		/// <remarks>
		/// A logger or an appender may attach additional
		/// properties to specific events. These properties
		/// have a string key and an object value.
		/// </remarks>
		public PropertiesCollection Properties
		{
			get 
			{ 
				if (m_data.Properties == null)
				{
					m_data.Properties = new PropertiesCollection();
				}
				return m_data.Properties; 
			}
		}

		/// <summary>
		/// The fixed fields in this event
		/// </summary>
		/// <value>
		/// The set of fields that are fixed in this event
		/// </value>
		/// <remarks>
		/// <para>
		/// Fields will not be fixed if they have previously been fixed.
		/// It is not possible to 'unfix' a field.
		/// </para>
		/// </remarks>
		public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { this.FixVolatileData(value); }
		}

		#endregion Public Instance Properties

		#region Implementation of ISerializable

#if !NETCF

		/// <summary>
		/// Serializes this object into the <see cref="SerializationInfo" /> provided.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination for this serialization.</param>
		/// <remarks>
		/// <para>
		/// The data in this event must be fixed before it can be serialised.
		/// </para>
		/// <para>
		/// The <see cref="FixVolatileData"/> method must be called during the
		/// <see cref="log4net.Appender.IAppender.DoAppend"/> method call if this event 
		/// is to be used outside that method.
		/// </para>
		/// </remarks>
#if !MONO
		[System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter=true)]
#endif
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// The caller must call FixVolatileData before this object
			// can be serialised.

			info.AddValue("LoggerName", m_data.LoggerName);
			info.AddValue("Level", m_data.Level);
			info.AddValue("NDC", m_data.NDC);
			info.AddValue("MDC", m_data.MDC);
			info.AddValue("Message", m_data.Message);
			info.AddValue("ThreadName", m_data.ThreadName);
			info.AddValue("TimeStamp", m_data.TimeStamp);
			info.AddValue("LocationInfo", m_data.LocationInfo);
			info.AddValue("UserName", m_data.UserName);
			info.AddValue("ExceptionString", m_data.ExceptionString);
			info.AddValue("Properties", m_data.Properties);
			info.AddValue("Domain", m_data.Domain);
			info.AddValue("Identity", m_data.Identity);
		}

#endif

		#endregion Implementation of ISerializable

		#region Public Instance Methods

		/// <summary>
		/// Gets the portable data for this <see cref="LoggingEvent" />.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A new <see cref="LoggingEvent"/> can be constructed using a
		/// <see cref="LoggingEventData"/> instance.</para>
		/// </remarks>
		/// <returns>The <see cref="LoggingEventData"/> for this event.</returns>
		public LoggingEventData GetLoggingEventData()
		{
			Fix = FixFlags.Partial;
			return m_data;
		}

		/// <summary>
		/// Looks up the specified key in the <see cref="MDC"/>.
		/// </summary>
		/// <param name="key">The key to lookup.</param>
		/// <returns>
		/// The value associated with the key, or <c>null</c> if the key was not found.
		/// </returns>
		public string LookupMappedContext(string key)
		{
			if (m_data.MDC == null)
			{
				// This creates a live copy of the MDC
				m_data.MDC = MDC.GetMap();
			}
			return m_data.MDC[key] as string;
		}

		/// <summary>
		/// Returns this event's exception's rendered using the 
		/// <see cref="ILoggerRepository.RendererMap" />.
		/// </summary>
		/// <returns>
		/// This event's exception's rendered using the <see cref="ILoggerRepository.RendererMap" />.
		/// </returns>
		public string GetExceptionStrRep() 
		{
			if (m_data.ExceptionString == null)
			{
				if (m_thrownException != null)
				{
					// Render exception using the repositories renderer map
					m_data.ExceptionString = m_repository.RendererMap.FindAndRender(m_thrownException);
				}
				else
				{
					m_data.ExceptionString = "";
				}
			}
			return m_data.ExceptionString;
		}

		/// <summary>
		/// Fixs instance fields that hold volatile data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some of the values in instances of <see cref="LoggingEvent"/>
		/// are considered volatile, that is the values are correct at the
		/// time the event is delivered to appenders, but will not be consistent
		/// at any time afterwards. If an event is to be stored and then processed
		/// at a later time these volatile values must be fixed by calling
		/// <see cref="FixVolatileData()"/>. There is a performance penalty
		/// incurred by calling <see cref="FixVolatileData()"/> but it
		/// is essential to maintaining data consistency.
		/// </para>
		/// <para>
		/// Calling <see cref="FixVolatileData()"/> is equivalent to
		/// calling <see cref="FixVolatileData(bool)"/> passing the parameter
		/// <c>false</c>.
		/// </para>
		/// <para>
		/// See <see cref="FixVolatileData(bool)"/> for more
		/// information.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		public void FixVolatileData()
		{
			Fix = FixFlags.All;
		}

		/// <summary>
		/// Fixes instance fields that hold volatile data.
		/// </summary>
		/// <param name="fastButLoose">Set to <c>true</c> to not fix data that takes a long time to fix.</param>
		/// <remarks>
		/// <para>
		/// Some of the values in instances of <see cref="LoggingEvent"/>
		/// are considered volatile, that is the values are correct at the
		/// time the event is delivered to appenders, but will not be consistent
		/// at any time afterwards. If an event is to be stored and then processed
		/// at a later time these volatile values must be fixed by calling
		/// <see cref="FixVolatileData()"/>. There is a performance penalty
		/// for incurred by calling <see cref="FixVolatileData()"/> but it
		/// is essential to maintaining data consistency.
		/// </para>
		/// <para>
		/// The <paramref name="fastButLoose"/> param controls the data that
		/// is fixed. Some of the data that can be fixed takes a long time to 
		/// generate, therefore if you do not require those settings to be fixed
		/// they can be ignored by setting the <paramref name="fastButLoose"/> param
		/// to <c>true</c>. This setting will ignore the <see cref="LocationInformation"/>
		/// and <see cref="UserName"/> settings.
		/// </para>
		/// <para>
		/// Set <paramref name="fastButLoose"/> to <c>false</c> to ensure that all 
		/// settings are fixed.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		public void FixVolatileData(bool fastButLoose)
		{
			if (fastButLoose)
			{
				Fix = FixFlags.Partial;
			}
			else
			{
				Fix = FixFlags.All;
			}
		}

		/// <summary>
		/// Fix the fields specified by the <see cref="FixFlags"/> parameter
		/// </summary>
		/// <param name="flags">the fields to fix</param>
		/// <remarks>
		/// Only fields specified in the <paramref name="flags"/> will be fixed.
		/// Fields will not be fixed if they have previously been fixed.
		/// It is not possible to 'unfix' a field.
		/// </remarks>
		protected void FixVolatileData(FixFlags flags)
		{
			// determine the flags that we are actually fixing
			FixFlags updateFlags = (FixFlags)((flags ^ m_fixFlags) & flags);

			if (updateFlags > 0)
			{
				if ((updateFlags & FixFlags.Mdc) != 0)
				{
					// Force the MDC to be cached
					CacheMappedContext();

					m_fixFlags |= FixFlags.Mdc;
				}
				if ((updateFlags & FixFlags.Ndc) != 0)
				{
					// Force the NDC to be cached
					string tmp = this.NestedContext;

					m_fixFlags |= FixFlags.Ndc;
				}
				if ((updateFlags & FixFlags.Message) != 0)
				{
					// Force the message to be rendered
					string tmp = this.RenderedMessage;

					m_fixFlags |= FixFlags.Message;
				}
				if ((updateFlags & FixFlags.Message) != 0)
				{
					// Force the message to be rendered
					string tmp = this.RenderedMessage;

					m_fixFlags |= FixFlags.Message;
				}
				if ((updateFlags & FixFlags.ThreadName) != 0)
				{
					// Grab the thread name
					string tmp = this.ThreadName;

					m_fixFlags |= FixFlags.ThreadName;
				}

				if ((updateFlags & FixFlags.LocationInfo) != 0)
				{
					// Force the location information to be loaded
					LocationInfo lo = this.LocationInformation;

					m_fixFlags |= FixFlags.LocationInfo;
				}
				if ((updateFlags & FixFlags.UserName) != 0)
				{
					// Grab the user name
					string tmp = this.UserName;

					m_fixFlags |= FixFlags.UserName;
				}
				if ((updateFlags & FixFlags.Domain) != 0)
				{
					// Grab the domain name
					string tmp = this.Domain;

					m_fixFlags |= FixFlags.Domain;
				}
				if ((updateFlags & FixFlags.Identity) != 0)
				{
					// Grab the identity
					string tmp = this.Identity;

					m_fixFlags |= FixFlags.Identity;
				}

				if ((updateFlags & FixFlags.Exception) != 0)
				{
					// Force the exception text to be loaded
					string tmp = GetExceptionStrRep();

					m_fixFlags |= FixFlags.Exception;
				}
			}
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Creates a cached copy of the <see cref="MDC" />.
		/// </summary>
		protected void CacheMappedContext()
		{
			// Copy the MDC dictionary
			if (m_data.MDC == null || m_data.MDC.IsReadOnly == false)
			{
				m_data.MDC = MDC.CopyMap();
			}
		}

		#endregion Public Instance Methods

		#region Private Static Fields

		/// <summary>
		/// Stores the time when this class is loaded.
		/// </summary>
		/// <remarks>
		/// This is used to provide times relative to the
		/// application start.
		/// </remarks>
		private static readonly DateTime s_startTime = DateTime.Now;

		#endregion Private Static Fields

		#region Private Instance Fields

		/// <summary>
		/// The internal logging event data.
		/// </summary>
		private LoggingEventData m_data;

		/// <summary>
		/// The fully qualified classname of the calling 
		/// logger class.
		/// </summary>
		private readonly string m_fqnOfLoggerClass;

		/// <summary>
		/// The application supplied message of logging event.
		/// </summary>
		private readonly object m_message;

		/// <summary>
		/// The exception that was thrown.
		/// </summary>
		/// <remarks>
		/// This is not serialised. The string representation
		/// is serialised instead.
		/// </remarks>
		private readonly Exception m_thrownException;

		/// <summary>
		/// The repository that generated the logging event
		/// </summary>
		/// <remarks>
		/// This is not serialised.
		/// </remarks>
		private readonly ILoggerRepository m_repository = null;

		/// <summary>
		/// The fix state for this event
		/// </summary>
		/// <remarks>
		/// These flags indicate which fields have been fixed.
		/// Not serialised.
		/// </remarks>
		private FixFlags m_fixFlags = FixFlags.None;

		#endregion Private Instance Fields

		#region Constants

		/// <summary>
		/// The key into the <see cref="Properties"/> map for the host name value.
		/// </summary>
		public const string HostNameProperty = "log4net:HostName";

		/// <summary>
		/// The key into the <see cref="Properties"/> map for the host name value.
		/// </summary>
		public const string IdentityProperty = "log4net:Identity";

		/// <summary>
		/// The key into the <see cref="Properties"/> map for the host name value.
		/// </summary>
		public const string UserNameProperty = "log4net:UserName";

		#endregion
	}
}
