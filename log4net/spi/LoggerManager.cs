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
using System.Reflection;

using log4net.helpers;
using log4net.Repository;

namespace log4net.spi
{
	/// <summary>
	/// Static manager that controls the creation of repositories
	/// </summary>
	/// <remarks>
	/// <para>
	/// Static manager that controls the creation of repositories
	/// </para>
	/// <para>
	/// This class is used by the wrapper managers (e.g. <see cref="log4net.LogManager"/>)
	/// to provide access to the <see cref="ILogger"/> objects.
	/// </para>
	/// </remarks>
	public sealed class LoggerManager
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private constructor to prevent instances. Only static methods should be used.
		/// </summary>
		/// <remarks>
		/// Private constructor to prevent instances. Only static methods should be used.
		/// </remarks>
		private LoggerManager() 
		{
		}

		#endregion Private Instance Constructors

		#region Static Constructor

		/// <summary>
		/// Hook the shutdown event
		/// </summary>
		/// <remarks>
		/// On the full .NET runtime, the static constructor hooks up the 
		/// <c>AppDomain.ProcessExit</c> and <c>AppDomain.DomainUnload</c>> events. 
		/// These are used to shutdown the log4net system as the application exits.
		/// </remarks>
		static LoggerManager()
		{
			try
			{
				// Register the AppDomain events, note we have to do this with a
				// method call rather than directly here because the AppDomain
				// makes a LinkDemand which throws the execption during the JIT phase.
				RegisterAppDomainEvents();
			}
			catch(System.Security.SecurityException)
			{
				LogLog.Debug("Security Exception (ControlAppDomain LinkDemand) while trying "+
					"to register Shutdown handler with the AppDomain. LoggerManager.Shutdown() "+
					"will not be called automatically when the AppDomain exits. It must be called "+
					"programmatically.");
			}

			// Dump out our assembly version into the log if debug is enabled
			LogLog.Debug(GetVersionInfo());

			// Set the default repository selector
#if NETCF
			s_repositorySelector = new CompactRepositorySelector(typeof(log4net.Repository.Hierarchy.Hierarchy));
#else
			s_repositorySelector = new DefaultRepositorySelector(typeof(log4net.Repository.Hierarchy.Hierarchy));
#endif
		}

		/// <summary>
		/// Register for ProcessExit and DomainUnload events on the AppDomain
		/// </summary>
		/// <remarks>
		/// This needs to be in a seperate method because the events make
		/// a LinkDemand for the ControlAppDomain SecurityPermission. Because
		/// this is a LinkDemand it is demanded at JIT time. Therefore we cannot
		/// catch the exception in the method itself, we habe to catch it in the
		/// caller.
		/// </remarks>
		private static void RegisterAppDomainEvents()
		{
#if !NETCF
			// ProcessExit seems to be fired if we are part of the default domain
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

			// Otherwise DomainUnload is fired
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
#endif
		}

		#endregion Static Constructor

		#region Public Static Methods

		/// <summary>
		/// Return the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <param name="domain">the domain to lookup in</param>
		/// <returns>Return the default <see cref="ILoggerRepository"/> instance</returns>
		/// <remarks>
		/// <para>Gets the <see cref="ILoggerRepository"/> for the domain specified
		/// by the <paramref name="domain"/> argument.</para>
		/// </remarks>
		public static ILoggerRepository GetLoggerRepository(string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return RepositorySelector.GetRepository(domain);
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		public static ILoggerRepository GetLoggerRepository(Assembly domainAssembly)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			return RepositorySelector.GetRepository(domainAssembly);
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>If the named logger exists (in the specified domain) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.</para>
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger found, or <c>null</c> if the named logger does not exist in the
		/// specified domain.
		/// </returns>
		public static ILogger Exists(string domain, string name) 
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return RepositorySelector.GetRepository(domain).Exists(name);
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>If the named logger exists (in the specified assembly's domain) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.</para>
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger found, or <c>null</c> if the named logger does not exist in the
		/// specified assembly's domain.
		/// </returns>
		public static ILogger Exists(Assembly domainAssembly, string name) 
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return RepositorySelector.GetRepository(domainAssembly).Exists(name);
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified domain.
		/// </summary>
		/// <param name="domain">The domain to lookup in.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILogger[] GetCurrentLoggers(string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return RepositorySelector.GetRepository(domain).GetCurrentLoggers();
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified assembly's domain.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILogger[] GetCurrentLoggers(Assembly domainAssembly)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			return RepositorySelector.GetRepository(domainAssembly).GetCurrentLoggers();
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieves a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>
		/// By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILogger GetLogger(string domain, string name)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return RepositorySelector.GetRepository(domain).GetLogger(name);
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieves a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>
		/// By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILogger GetLogger(Assembly domainAssembly, string name)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return RepositorySelector.GetRepository(domainAssembly).GetLogger(name);
		}	

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		/// <param name="type">The <paramref name="type"/> opf which the fullname will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILogger GetLogger(string domain, Type type) 
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return RepositorySelector.GetRepository(domain).GetLogger(type.FullName);
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="domainAssembly">the assembly to use to lookup the domain</param>
		/// <param name="type">The <paramref name="type"/> opf which the fullname will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILogger GetLogger(Assembly domainAssembly, Type type) 
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return RepositorySelector.GetRepository(domainAssembly).GetLogger(type.FullName);
		}	

		/// <summary>
		/// Shuts down the log4net system.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in all the
		/// default repositories.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>
		/// The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		public static void Shutdown() 
		{
			foreach(ILoggerRepository repository in GetAllRepositories())
			{
				repository.Shutdown();
			}
		}

		/// <summary>
		/// Shuts down the repository for the domain specified.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// repository for the <paramref name="domain"/> specified.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>
		/// The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		/// <param name="domain">The domain to shutdown.</param>
		public static void ShutdownRepository(string domain) 
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			RepositorySelector.GetRepository(domain).Shutdown();
		}

		/// <summary>
		/// Shuts down the repository for the domain specified.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// repository for the domain. The domain is looked up using
		/// the <paramref name="domainAssembly"/> specified.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>
		/// The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		public static void ShutdownRepository(Assembly domainAssembly) 
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			RepositorySelector.GetRepository(domainAssembly).Shutdown();
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Resets all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.DEBUG"/>. Moreover,
		/// message disabling is set its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		public static void ResetConfiguration(string domain) 
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			RepositorySelector.GetRepository(domain).ResetConfiguration();
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Resets all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.DEBUG"/>. Moreover,
		/// message disabling is set its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		public static void ResetConfiguration(Assembly domainAssembly) 
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			RepositorySelector.GetRepository(domainAssembly).ResetConfiguration();
		}

		/// <summary>
		/// Creates a domain with the specified name.
		/// </summary>
		/// <param name="domain">The name of the domain, this must be unique amongst domain.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the domain.</returns>
		/// <remarks>
		/// <para>
		/// Creates the default type of <see cref="ILoggerRepository"/> which is a
		/// <see cref="log4net.Repository.Hierarchy.Hierarchy"/> object.
		/// </para>
		/// <para>
		/// The <paramref name="domain"/> name must be unique. Domains cannot be redefined.
		/// An <see cref="Exception"/> will be thrown if the domain already exists.
		/// </para>
		/// </remarks>
		/// <exception cref="LogException">The specified domain already exists.</exception>
		public static ILoggerRepository CreateDomain(string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return RepositorySelector.CreateRepository(domain, null);
		}

		/// <summary>
		/// Creates a domain with the specified name and repository type.
		/// </summary>
		/// <param name="domain">The name of the domain, this must be unique to the domain.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the domain specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the domain.</returns>
		/// <remarks>
		/// <para>
		/// The <paramref name="domain"/> name must be unique. Domains cannot be redefined.
		/// An Exception will be thrown if the domain already exists.
		/// </para>
		/// </remarks>
		/// <exception cref="LogException">The specified domain already exists.</exception>
		public static ILoggerRepository CreateDomain(string domain, Type repositoryType)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (repositoryType == null)
			{
				throw new ArgumentNullException("repositoryType");
			}
			return RepositorySelector.CreateRepository(domain, repositoryType);
		}

		/// <summary>
		/// Creates a domain for the specified assembly and repository type.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to get the name of the domain.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the domain specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the domain.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetLoggerRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// </remarks>
		public static ILoggerRepository CreateDomain(Assembly domainAssembly, Type repositoryType)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			if (repositoryType == null)
			{
				throw new ArgumentNullException("repositoryType");
			}
			return RepositorySelector.CreateRepository(domainAssembly, repositoryType);
		}

		/// <summary>
		/// Gets the list of currently defined repositories.
		/// </summary>
		/// <returns>An array of all the known <see cref="ILoggerRepository"/> objects.</returns>
		public static ILoggerRepository[] GetAllRepositories()
		{
			return RepositorySelector.GetAllRepositories();
		}

		/// <summary>
		/// Gets or sets the repository selector used by the <see cref="LogManager" />.
		/// </summary>
		/// <value>
		/// The repository selector used by the <see cref="LogManager" />.
		/// </value>
		/// <remarks>
		/// <para>
		/// The repository selector (<see cref="IRepositorySelector"/>) is used by 
		/// the <see cref="LogManager"/> to create and select repositories 
		/// (<see cref="ILoggerRepository"/>).
		/// </para>
		/// <para>
		/// The caller to <see cref="LogManager"/> supplies either a string name 
		/// or an assembly (if not supplied the assembly is inferred using 
		/// <see cref="Assembly.GetCallingAssembly()"/>).
		/// </para>
		/// <para>
		/// This context is used by the selector to lookup a specific repository.
		/// </para>
		/// <para>
		/// For the full .NET Framework, the default repository is <c>DefaultRepositorySelector</c>;
		/// for the .NET Compact Framework <c>CompactRepositorySelector</c> is the default
		/// repository.
		/// </para>
		/// </remarks>
		public static IRepositorySelector RepositorySelector
		{
			get { return s_repositorySelector; }
			set { s_repositorySelector = value; }
		}

		#endregion Public Static Methods

		#region Private Static Methods

		/// <summary>
		/// Internal method to get pertinent version info.
		/// </summary>
		/// <returns>A string of version info.</returns>
		private static string GetVersionInfo()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			// Grab the currently executing assembly
			Assembly myAssembly = Assembly.GetExecutingAssembly();

			// Build Up message
			sb.Append("log4net assembly [").Append(myAssembly.FullName).Append("]. ");
			sb.Append("Loaded from [").Append(SystemInfo.AssemblyLocationInfo(myAssembly)).Append("]. ");
			sb.Append("(.NET Runtime [").Append(Environment.Version.ToString()).Append("]");
#if (!SSCLI)
            sb.Append(" on ").Append(Environment.OSVersion.ToString());
#endif
            sb.Append(")");
			return sb.ToString();
		}

#if (!NETCF)
		/// <summary>
		/// Called when the <see cref="AppDomain.DomainUnload"/> event fires
		/// </summary>
		/// <param name="sender">the <see cref="AppDomain"/> that is exiting</param>
		/// <param name="e">null</param>
		/// <remarks>
		/// <para>Called when the <see cref="AppDomain.DomainUnload"/> event fires.</para>
		/// 
		/// <para>When the event is triggered the log4net system is <see cref="Shutdown()"/>.</para>
		/// </remarks>
		private static void OnDomainUnload(object sender, EventArgs e)
		{
			Shutdown();
		}

		/// <summary>
		/// Called when the <see cref="AppDomain.ProcessExit"/> event fires
		/// </summary>
		/// <param name="sender">the <see cref="AppDomain"/> that is exiting</param>
		/// <param name="e">null</param>
		/// <remarks>
		/// <para>Called when the <see cref="AppDomain.ProcessExit"/> event fires.</para>
		/// 
		/// <para>When the event is triggered the log4net system is <see cref="Shutdown()"/>.</para>
		/// </remarks>
		private static void OnProcessExit(object sender, EventArgs e)
		{
			Shutdown();
		}
#endif

		#endregion Private Static Methods

		#region Private Static Fields

		/// <summary>
		/// Initialise the default repository selector
		/// </summary>
		private static IRepositorySelector s_repositorySelector;

		#endregion Private Static Fields
	}
}
