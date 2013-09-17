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
using System.Xml;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;

using log4net.Appender;
using log4net.helpers;
using log4net.Repository;

namespace log4net.Config
{
	/// <summary>
	/// Use this class to initialize the log4net environment using a DOM tree.
	/// </summary>
	/// <remarks>
	/// Configures a <see cref="ILoggerRepository"/> using an XML DOM tree.
	/// </remarks>
	public sealed class DOMConfigurator
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private constructor
		/// </summary>
		private DOMConfigurator() 
		{ 
		}

		#endregion Protected Instance Constructors

		#region Configure static methods

		/// <summary>
		/// Automatically configures the log4net system based on the 
		/// application's configuration settings.
		/// </summary>
		/// <remarks>
		/// Each application has a configuration file. This has the
		/// same name as the application with '.config' appended.
		/// This file is XML and calling this function prompts the
		/// configurator to look in that file for a section called
		/// <c>log4net</c> that contains the configuration data.
		/// </remarks>
		static public void Configure() 
		{
			Configure(LogManager.GetLoggerRepository(Assembly.GetCallingAssembly()));
		}

		/// <summary>
		/// Automatically configures the <see cref="ILoggerRepository"/> using settings
		/// stored in the application's configuration file.
		/// </summary>
		/// <remarks>
		/// Each application has a configuration file. This has the
		/// same name as the application with '.config' appended.
		/// This file is XML and calling this function prompts the
		/// configurator to look in that file for a section called
		/// <c>log4net</c> that contains the configuration data.
		/// </remarks>
		/// <param name="repository">The repository to configure.</param>
		static public void Configure(ILoggerRepository repository) 
		{
			LogLog.Debug("DOMConfigurator: configuring repository [" + repository.Name + "] using .config file section");

			try
			{
				LogLog.Debug("DOMConfigurator: Application config file is [" + SystemInfo.ConfigurationFileLocation + "]");
			}
			catch
			{
				// ignore error
				LogLog.Debug("DOMConfigurator: Application config file location unknown");
			}

#if NETCF
			// No config file reading stuff. Just go straight for the file
			Configure(repository, new FileInfo(SystemInfo.ConfigurationFileLocation));
#else
			try
			{
				ConfigureFromXML(repository, (XmlElement) System.Configuration.ConfigurationSettings.GetConfig("log4net"));
			}
			catch(System.Configuration.ConfigurationException confEx)
			{
				if (confEx.BareMessage.IndexOf("Unrecognized element") >= 0)
				{
					// Looks like the XML file is not valid
					LogLog.Error("DOMConfigurator: Failed to parse config file. Check your .config file is well formed XML.", confEx);
				}
				else
				{
					// This exception is typically due to the assembly name not being correctly specified in the section type.
					string configSectionStr = "<section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler," + Assembly.GetExecutingAssembly().FullName + "\" />";
					LogLog.Error("DOMConfigurator: Failed to parse config file. Is the <configSections> specified as: " + configSectionStr, confEx);
				}
			}
#endif
		}

		/// <summary>
		/// Configures log4net using a <c>log4net</c> element
		/// </summary>
		/// <remarks>
		/// Loads the log4net configuration from the XML element
		/// supplied as <paramref name="element"/>.
		/// </remarks>
		/// <param name="element">The element to parse.</param>
		static public void Configure(XmlElement element) 
		{
			ConfigureFromXML(LogManager.GetLoggerRepository(Assembly.GetCallingAssembly()), element);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified XML 
		/// element.
		/// </summary>
		/// <remarks>
		/// Loads the log4net configuration from the XML element
		/// supplied as <paramref name="element"/>.
		/// </remarks>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="element">The element to parse.</param>
		static public void Configure(ILoggerRepository repository, XmlElement element) 
		{
			LogLog.Debug("DOMConfigurator: configuring repository [" + repository.Name + "] using XML element");

			ConfigureFromXML(repository, element);
		}

		/// <summary>
		/// Configures log4net using the specified configuration file.
		/// </summary>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the log4net configuration data.
		/// </para>
		/// <para>
		/// The log4net configuration file can possible be specified in the application's
		/// configuration file (either <c>MyAppName.exe.config</c> for a
		/// normal application on <c>Web.config</c> for an ASP.NET application).
		/// </para>
		/// <example>
		/// The following example configures log4net using a configuration file, of which the 
		/// location is stored in the application's configuration file :
		/// </example>
		/// <code lang="C#">
		/// using log4net.Config;
		/// using System.IO;
		/// using System.Configuration;
		/// 
		/// ...
		/// 
		/// DOMConfigurator.Configure(new FileInfo(ConfigurationSettings.AppSettings["log4net-config-file"]));
		/// </code>
		/// <para>
		/// In the <c>.config</c> file, the path to the log4net can be specified like this :
		/// </para>
		/// <code>
		/// &lt;configuration&gt;
		///		&lt;appSettings&gt;
		///			&lt;add key="log4net-config-file" value="log.config"/&gt;
		///		&lt;/appSettings&gt;
		///	&lt;/configuration&gt;
		/// </code>
		/// </remarks>
		static public void Configure(FileInfo configFile)
		{
			Configure(LogManager.GetLoggerRepository(Assembly.GetCallingAssembly()), configFile);
		}

		/// <summary>
		/// Configures log4net using the specified configuration file.
		/// </summary>
		/// <param name="configStream">A stream to load the XML configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration data must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the log4net configuration data.
		/// </para>
		/// <para>
		/// Note that this method will NOT close the stream parameter.
		/// </para>
		/// </remarks>
		static public void Configure(Stream configStream)
		{
			Configure(LogManager.GetLoggerRepository(Assembly.GetCallingAssembly()), configStream);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
		/// file.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The log4net configuration file can possible be specified in the application's
		/// configuration file (either <c>MyAppName.exe.config</c> for a
		/// normal application on <c>Web.config</c> for an ASP.NET application).
		/// </para>
		/// <example>
		/// The following example configures log4net using a configuration file, of which the 
		/// location is stored in the application's configuration file :
		/// </example>
		/// <code lang="C#">
		/// using log4net.Config;
		/// using System.IO;
		/// using System.Configuration;
		/// 
		/// ...
		/// 
		/// DOMConfigurator.Configure(new FileInfo(ConfigurationSettings.AppSettings["log4net-config-file"]));
		/// </code>
		/// <para>
		/// In the <c>.config</c> file, the path to the log4net can be specified like this :
		/// </para>
		/// <code>
		/// &lt;configuration&gt;
		///		&lt;appSettings&gt;
		///			&lt;add key="log4net-config-file" value="log.config"/&gt;
		///		&lt;/appSettings&gt;
		///	&lt;/configuration&gt;
		/// </code>
		/// </remarks>
		static public void Configure(ILoggerRepository repository, FileInfo configFile)
		{
			LogLog.Debug("DOMConfigurator: configuring repository [" + repository.Name + "] using file [" + configFile + "]");

			if (configFile == null)
			{
				LogLog.Error("DOMConfigurator: Configure called with null 'configFile' parameter");
			}
			else
			{
				// Have to use File.Exists() rather than configFile.Exists()
				// because configFile.Exists() caches the value, not what we want.
				if (File.Exists(configFile.FullName))
				{
					// Open the file for reading
					FileStream fs = null;
					
					// Try hard to open the file
					for(int retry = 5; --retry >= 0; )
					{
						try
						{
							fs = configFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
							break;
						}
						catch(IOException ex)
						{
							if (retry == 0)
							{
								LogLog.Error("DOMConfigurator: Failed to open XML config file [" + configFile.Name + "]", ex);

								// The stream cannot be valid
								fs = null;
							}
							System.Threading.Thread.Sleep(250);
						}
					}

					if (fs != null)
					{
						try
						{
							// Load the configuration from the stream
							Configure(repository, fs);
						}
						finally
						{
							// Force the file closed whatever happens
							fs.Close();
						}
					}
				}
				else
				{
					LogLog.Debug("DOMConfigurator: config file [" + configFile.FullName + "] not found. Configuration unchanged.");
				}
			}
		}


		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
		/// file.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configStream">The stream to load the XML configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration data must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// Note that this method will NOT close the stream parameter.
		/// </para>
		/// </remarks>
		static public void Configure(ILoggerRepository repository, Stream configStream)
		{
			LogLog.Debug("DOMConfigurator: configuring repository [" + repository.Name + "] using stream");

			if (configStream == null)
			{
				LogLog.Error("DOMConfigurator: Configure called with null 'configStream' parameter");
			}
			else
			{
				// Load the config file into a DOM
				XmlDocument doc = new XmlDocument();
				try
				{
#if (NETCF || MONO)
					// Create a text reader for the file stream
					XmlTextReader xmlReader = new XmlTextReader(configStream);
#else
					// Create a validating reader arround a text reader for the file stream
					XmlValidatingReader xmlReader = new XmlValidatingReader(new XmlTextReader(configStream));

					// Specify that the reader should not perform validation, but that it should
					// expand entity refs.
					xmlReader.ValidationType = ValidationType.None;
					xmlReader.EntityHandling = EntityHandling.ExpandEntities;
#endif
					
					// load the data into the dom
					doc.Load(xmlReader);
				}
				catch(Exception ex)
				{
					LogLog.Error("DOMConfigurator: Error while loading XML configuration", ex);

					// The document is invalid
					doc = null;
				}

				if (doc != null)
				{
					LogLog.Debug("DOMConfigurator: loading XML configuration");

					// Configure using the 'log4net' element
					XmlNodeList configNodeList = doc.GetElementsByTagName("log4net");
					if (configNodeList.Count == 0)
					{
						LogLog.Debug("DOMConfigurator: XML configuration does not contain a <log4net> element. Configuration Aborted.");
					}
					else if (configNodeList.Count > 1)
					{
						LogLog.Error("DOMConfigurator: XML configuration contains [" + configNodeList.Count + "] <log4net> elements. Only one is allowed. Configuration Aborted.");
					}
					else
					{
						ConfigureFromXML(repository, configNodeList[0] as XmlElement);
					}
				}
			}
		}

		#endregion Configure static methods

		#region ConfigureAndWatch static methods

#if (!NETCF && !SSCLI)

		/// <summary>
		/// Configures log4net using the file specified, monitors the file for changes 
		/// and reloads the configuration if a change is detected.
		/// </summary>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The configuration file will be monitored using a <see cref="FileSystemWatcher"/>
		/// and depends on the behaviour of that class.
		/// </para>
		/// <para>
		/// For more information on how to configure log4net using
		/// a separate configuration file, see <see cref="Configure(FileInfo)"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="Configure(FileInfo)"/>
		static public void ConfigureAndWatch(FileInfo configFile)
		{
			ConfigureAndWatch(LogManager.GetLoggerRepository(Assembly.GetCallingAssembly()), configFile);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the file specified, 
		/// monitors the file for changes and reloads the configuration if a change 
		/// is detected.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The configuration file will be monitored using a <see cref="FileSystemWatcher"/>
		/// and depends on the behaviour of that class.
		/// </para>
		/// <para>
		/// For more information on how to configure log4net using
		/// a separate configuration file, see <see cref="Configure(FileInfo)"/>.
		/// </para>
		/// </remarks>
		/// <seealso cfer="Configure(FileInfo)"/>
		static public void ConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
		{
			LogLog.Debug("DOMConfigurator: configuring repository [" + repository.Name + "] using file [" + configFile + "] watching for file updates");

			if (configFile == null)
			{
				LogLog.Error("DOMConfigurator: ConfigureAndWatch called with null 'configFile' parameter");
			}
			else
			{
				// Configure log4net now
				Configure(repository, configFile);

				try
				{
					// Create a watch handler that will reload the
					// configuration whenever the config file is modified.
					new ConfigureAndWatchHandler(repository, configFile);
				}
				catch(Exception ex)
				{
					LogLog.Error("Failed to initialise configuration file watcher for file ["+configFile.FullName+"]", ex);
				}
			}
		}
#endif

		#endregion ConfigureAndWatch static methods

		#region ConfigureAndWatchHandler

#if (!NETCF && !SSCLI)
		/// <summary>
		/// Class used to watch config files.
		/// </summary>
		/// <remarks>
		/// Uses the <see cref="FileSystemWatcher"/> to monitor
		/// changes to a specified file. Because multiple change notifications
		/// may be raised when the file is modified, a timer is used to
		/// compress the notifications into a single event. The timer
		/// waits for <see cref="TimoutMillis"/> time before delivering
		/// the event notification. If any further <see cref="FileSystemWatcher"/>
		/// change notifications arrive while the timer is waiting it
		/// is reset and waits again for <see cref="TimoutMillis"/> to
		/// elaps.
		/// </remarks>
		private sealed class ConfigureAndWatchHandler
		{
			/// <summary>
			/// Holds the FileInfo used to configure the DOMConfigurator
			/// </summary>
			private FileInfo m_configFile;

			/// <summary>
			/// Holds the repository being configured.
			/// </summary>
			private ILoggerRepository m_repository;

			/// <summary>
			/// The timer used to compress the notification events.
			/// </summary>
			private Timer m_timer;

			/// <summary>
			/// The default amount of time to wait after receiving notification
			/// before reloading the config file.
			/// </summary>
			private const int TimoutMillis = 500;

			/// <summary>
			/// Initializes a new instance of the <see cref="ConfigureAndWatchHandler" /> class.
			/// </summary>
			/// <param name="repository">The repository to configure.</param>
			/// <param name="configFile">The configuration file to watch.</param>
			internal ConfigureAndWatchHandler(ILoggerRepository repository, FileInfo configFile)
			{
				m_repository = repository;
				m_configFile = configFile;

				// Create a new FileSystemWatcher and set its properties.
				FileSystemWatcher watcher = new FileSystemWatcher();

				watcher.Path = m_configFile.DirectoryName;
				watcher.Filter = m_configFile.Name;

				// Set the notification filters
				watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;

				// Add event handlers. OnChanged will do for all event handlers that fire a FileSystemEventArgs
				watcher.Changed += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
				watcher.Created += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
				watcher.Deleted += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
				watcher.Renamed += new RenamedEventHandler(ConfigureAndWatchHandler_OnRenamed);

				// Begin watching.
				watcher.EnableRaisingEvents = true;

				// Create the timer that will be used to deliver events. Set as disabled
				m_timer = new Timer(new TimerCallback(OnWhatchedFileChange), null, Timeout.Infinite, Timeout.Infinite);
			}

			/// <summary>
			/// Event handler used by <see cref="ConfigureAndWatchHandler"/>.
			/// </summary>
			/// <param name="source">The <see cref="FileSystemWatcher"/> firing the event.</param>
			/// <param name="e">The argument indicates the file that caused the event to be fired.</param>
			/// <remarks>
			/// This handler reloads the configuration from the file when the event is fired.
			/// </remarks>
			private void ConfigureAndWatchHandler_OnChanged(object source, FileSystemEventArgs e)
			{
				LogLog.Debug("ConfigureAndWatchHandler: "+e.ChangeType+" [" + m_configFile.FullName + "]");

				// Deliver the event in TimoutMillis time
				// timer will fire only once
				m_timer.Change(TimoutMillis, Timeout.Infinite);
			}

			/// <summary>
			/// Event handler used by <see cref="ConfigureAndWatchHandler"/>.
			/// </summary>
			/// <param name="source">The <see cref="FileSystemWatcher"/> firing the event.</param>
			/// <param name="e">The argument indicates the file that caused the event to be fired.</param>
			/// <remarks>
			/// This handler reloads the configuration from the file when the event is fired.
			/// </remarks>
			private void ConfigureAndWatchHandler_OnRenamed(object source, RenamedEventArgs e)
			{
				LogLog.Debug("ConfigureAndWatchHandler: " + e.ChangeType + " [" + m_configFile.FullName + "]");

				// Deliver the event in TimoutMillis time
				// timer will fire only once
				m_timer.Change(TimoutMillis, Timeout.Infinite);
			}

			/// <summary>
			/// Called by the timer when the configuration has been updated.
			/// </summary>
			/// <param name="state">null</param>
			private void OnWhatchedFileChange(object state)
			{
				DOMConfigurator.Configure(m_repository, m_configFile);
			}
		}
#endif

		#endregion ConfigureAndWatchHandler

		#region Private Static Methods

		/// <summary>
		/// Configures the specified repository using a <c>log4net</c> element.
		/// </summary>
		/// <param name="repository">The hierarchy to configure.</param>
		/// <param name="element">The element to parse.</param>
		/// <remarks>
		/// <para>
		/// Loads the log4net configuration from the XML element
		/// supplied as <paramref name="element"/>.
		/// </para>
		/// <para>
		/// This method is ultimately called by one of the Configure methods 
		/// to load the configuration from an <see cref="XmlElement"/>.
		/// </para>
		/// </remarks>
		static private void ConfigureFromXML(ILoggerRepository repository, XmlElement element) 
		{
			if (element == null)
			{
				LogLog.Error("DOMConfigurator: ConfigureFromXML called with null 'element' parameter");
			}
			else if (repository == null)
			{
				LogLog.Error("DOMConfigurator: ConfigureFromXML called with null 'repository' parameter");
			}
			else
			{
				LogLog.Debug("DOMConfigurator: Configuring Repository [" + repository.Name + "]");

				// Copy the xml data into the root of a new document
				// this isolates the xml config data from the rest of
				// the document
				XmlDocument newDoc = new XmlDocument();
				XmlElement newElement = (XmlElement) newDoc.AppendChild(newDoc.ImportNode(element, true));

				// Check the type of the repository
				if (repository is IDOMRepositoryConfigurator)
				{
					// Pass the configurator the config element
					((IDOMRepositoryConfigurator) repository).Configure(newElement);
				}
				else
				{
					LogLog.Warn("Repository [" + repository + "] does not support the DOMConfigurator");
				}
			}
		}

		#endregion Private Static Methods
	}
}

