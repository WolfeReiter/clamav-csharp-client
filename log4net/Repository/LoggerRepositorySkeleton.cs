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

using log4net.ObjectRenderer;
using log4net.spi;
using log4net.helpers;
using log4net.Plugin;

namespace log4net.Repository
{
	/// <summary>
	/// Skeleton implementation of the <see cref="ILoggerRepository"/> interface
	/// </summary>
	/// <remarks>
	/// Skeleton implementation of the <see cref="ILoggerRepository"/> interface.
	/// All <see cref="ILoggerRepository"/> types should extend this type.
	/// </remarks>
	public abstract class LoggerRepositorySkeleton : ILoggerRepository
	{
		#region Member Variables

		private string m_name;
		private RendererMap m_rendererMap;
		private PluginMap m_pluginMap;
		private LevelMap m_levelMap;
		private Level m_threshold;
		private bool m_configured;
		private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;
		private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;
		private PropertiesCollection m_properties;

		#endregion

		#region Constructors

		/// <summary>
		/// Default Construtor
		/// </summary>
		/// <remarks>
		/// Initialises the repository with default (empty) properties
		/// </remarks>
		protected LoggerRepositorySkeleton() : this(new PropertiesCollection())
		{
		}

		/// <summary>
		/// Construct the repository using specific properties
		/// </summary>
		/// <param name="properties">the properties to set for this repository</param>
		protected LoggerRepositorySkeleton(PropertiesCollection properties)
		{
			m_properties = properties;
			m_rendererMap = new RendererMap();
			m_pluginMap = new PluginMap(this);
			m_levelMap = new LevelMap();
			m_configured = false;

			AddBuiltinLevels();

			// Don't disable any levels by default.
			Threshold = Level.ALL;
		}

		#endregion

		#region Implementation of ILoggerRepository

		/// <summary>
		/// The name of the repository
		/// </summary>
		/// <value>
		/// The string name of the repository
		/// </value>
		virtual public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// The threshold for all events in this repository
		/// </summary>
		/// <value>
		/// The threshold for all events in this repository
		/// </value>
		/// <remarks>
		/// The threshold for all events in this repository
		/// </remarks>
		virtual public Level Threshold
		{
			get { return m_threshold; }
			set
			{ 
				if (value != null)
				{
					m_threshold = value; 
				}
				else
				{
					// Must not set threshold to null
					LogLog.Warn("LoggerRepositorySkeleton: Threshold cannot be set to null. Setting to ALL");
					m_threshold = Level.ALL;
				}
			}
		}

		/// <summary>
		/// RendererMap accesses the object renderer map for this repository.
		/// </summary>
		/// <value>
		/// RendererMap accesses the object renderer map for this repository.
		/// </value>
		/// <remarks>
		/// <para>RendererMap accesses the object renderer map for this repository.</para>
		/// 
		/// <para>The RendererMap holds a mapping between types and
		/// <see cref="IObjectRenderer"/> objects.</para>
		/// </remarks>
		virtual public RendererMap RendererMap
		{
			get { return m_rendererMap; }
		}

		/// <summary>
		/// The plugin map for this repository.
		/// </summary>
		/// <value>
		/// The plugin map for this repository.
		/// </value>
		virtual public PluginMap PluginMap
		{
			get { return m_pluginMap; }
		}

		/// <summary>
		/// Get the level map for the Repository.
		/// </summary>
		/// <remarks>
		/// <para>Get the level map for the Repository.</para>
		/// <para>The level map defines the mappings between
		/// level names and <see cref="Level"/> objects in
		/// this repository.</para>
		/// </remarks>
		virtual public LevelMap LevelMap
		{
			get { return m_levelMap; }
		}

		/// <summary>
		/// Check if the named logger exists in the repository. If so return
		/// its reference, otherwise returns <c>null</c>.
		/// </summary>
		/// <param name="name">The name of the logger to lookup</param>
		/// <returns>The Logger object with the name specified</returns>
		abstract public ILogger Exists(string name);

		/// <summary>
		/// Returns all the currently defined loggers in the repository as an Array
		/// </summary>
		/// <remarks>
		/// Returns all the currently defined loggers in the repository as an Array.
		/// The root logger is <b>not</b> included in the returned
		/// enumeration.
		/// </remarks>
		/// <returns>All the defined loggers</returns>
		abstract public ILogger[] GetCurrentLoggers();

		/// <summary>
		/// Return a new logger instance
		/// </summary>
		/// <remarks>
		/// <para>Return a new logger instance.</para>
		/// 
		/// <para>If a logger of that name already exists, then it will be
		/// returned.  Otherwise, a new logger will be instantiated and
		/// then linked with its existing ancestors as well as children.</para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve</param>
		/// <returns>The logger object with the name specified</returns>
		abstract public ILogger GetLogger(string name);

		/// <summary>
		/// Shutdown the repository
		/// </summary>
		/// <remarks>
		/// Shutdown the repository. Can be overriden in a subclass.
		/// This base class implementation notifies the <see cref="ShutdownEvent"/>
		/// listeners and all attached plugins of the shutdown event.
		/// </remarks>
		virtual public void Shutdown() 
		{
			// Shutdown attached plugins
			foreach(IPlugin plugin in PluginMap.AllPlugins)
			{
				plugin.Shutdown();
			}

			// Notify listeners
			FireShutdownEvent();
		}

		/// <summary>
		/// Reset the repositories configuration to a default state
		/// </summary>
		/// <remarks>
		/// <para>Reset all values contained in this instance to their
		/// default state.</para>
		/// 
		/// <para>Existing loggers are not removed. They are just reset.</para>
		/// 
		/// <para>This method should be used sparingly and with care as it will
		/// block all logging until it is completed.</para>
		/// </remarks>
		virtual public void ResetConfiguration() 
		{
			// Clear internal data structures
			m_rendererMap.Clear();
			m_levelMap.Clear();

			// Add the predefined levels to the map
			AddBuiltinLevels();

			Configured = false;

			// Notify listeners
			FireConfigurationResetEvent();
		}

		/// <summary>
		/// Log the logEvent through this repository.
		/// </summary>
		/// <param name="logEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// This method should not normally be used to log.
		/// The <see cref="ILog"/> interface should be used 
		/// for routine logging. This interface can be obtained
		/// using the <see cref="log4net.LogManager.GetLogger(string)"/> method.
		/// </para>
		/// <para>
		/// The <c>logEvent</c> is delivered to the appropriate logger and
		/// that logger is then responsible for logging the event.
		/// </para>
		/// </remarks>
		abstract public void Log(LoggingEvent logEvent);

		/// <summary>
		/// Flag indicates if this repository has been configured.
		/// </summary>
		/// <value>
		/// Flag indicates if this repository has been configured.
		/// </value>
		virtual public bool Configured 
		{ 
			get { return m_configured; }
			set { m_configured = value; }
		}

		/// <summary>
		/// Event to notify that the repository has been shutdown.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has been shutdown.
		/// </value>
		public event LoggerRepositoryShutdownEventHandler ShutdownEvent
		{
			add { m_shutdownEvent += value; }
			remove { m_shutdownEvent -= value; }
		}

		/// <summary>
		/// Event to notify that the repository has had its configuration reset.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has had its configuration reset.
		/// </value>
		public event LoggerRepositoryConfigurationResetEventHandler ConfigurationResetEvent
		{
			add { m_configurationResetEvent += value; }
			remove { m_configurationResetEvent -= value; }
		}

		/// <summary>
		/// Repository specific properties
		/// </summary>
		/// <remarks>
		/// These properties can be specified on a reporitory specific basis
		/// </remarks>
		public PropertiesCollection Properties 
		{ 
			get { return m_properties; } 
		}

		#endregion

		private void AddBuiltinLevels()
		{
			// Add the predefined levels to the map
			m_levelMap.Add(Level.OFF);

			// Unrecoverable errors
			m_levelMap.Add(Level.EMERGENCY);
			m_levelMap.Add(Level.FATAL);
			m_levelMap.Add(Level.ALERT); 

			// Recoverable errors
			m_levelMap.Add(Level.CRITICAL); 
			m_levelMap.Add(Level.SEVERE); 
			m_levelMap.Add(Level.ERROR); 
			m_levelMap.Add(Level.WARN);

			// Information
			m_levelMap.Add(Level.NOTICE); 
			m_levelMap.Add(Level.INFO); 

			// Debug
			m_levelMap.Add(Level.DEBUG);
			m_levelMap.Add(Level.FINE);
			m_levelMap.Add(Level.TRACE);
			m_levelMap.Add(Level.FINER);
			m_levelMap.Add(Level.VERBOSE);
			m_levelMap.Add(Level.FINEST);

			m_levelMap.Add(Level.ALL);
		}

		/// <summary>
		/// Adds an object renderer for a specific class. 
		/// </summary>
		/// <param name="classToRender">The type that will be rendered by the renderer supplied.</param>
		/// <param name="objectRenderer">The object renderer used to render the object.</param>
		virtual public void AddRenderer(Type classToRender, IObjectRenderer objectRenderer) 
		{
			if (classToRender == null)
			{
				throw new ArgumentNullException("classToRender");
			}
			if (objectRenderer == null)
			{
				throw new ArgumentNullException("objectRenderer");
			}

			m_rendererMap.Put(classToRender, objectRenderer);
		}

		/// <summary>
		/// Notify the registered listeners that the repository is shutting down
		/// </summary>
		protected void FireShutdownEvent()
		{
			if (m_shutdownEvent != null)
			{
				m_shutdownEvent(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Notify the registered listeners that the repository has had its configuration reset
		/// </summary>
		protected void FireConfigurationResetEvent()
		{
			if (m_configurationResetEvent != null)
			{
				m_configurationResetEvent(this, EventArgs.Empty);
			}
		}
	}
}
