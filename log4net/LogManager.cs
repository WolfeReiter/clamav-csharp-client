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
using System.Reflection;

using log4net.spi;
using log4net.Repository;

namespace log4net
{
	/// <summary>
	/// This is the class used by client applications to bind to logger
	/// instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// See the <see cref="ILog"/> interface for more details.
	/// </para>
	/// <para>
	/// log4net uses NUnit 2.0 to provide internal unit testing.
	/// To run the tests you will need a copy of NUnit 2.0. Then
	/// run the following command:
	/// </para>
	/// <code>nunit-console.exe /assembly:&lt;log4net assembly&gt;</code>
	/// </remarks>
	/// <example>Simple example of logging messages
	/// <code>
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.Info("Application Start");
	/// log.Debug("This is a debug message");
	/// 
	/// if (log.IsDebugEnabled)
	/// {
	///		log.Debug("This is another debug message");
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="ILog"/>
	public sealed class LogManager
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LogManager" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private LogManager()
		{
		}

		#endregion Private Instance Constructors

		#region Type Specific Manager Methods

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the named logger exists (in the default hierarchy) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>The logger found, or <c>null</c> if no logger could be found.</returns>
		public static ILog Exists(string name) 
		{
			return Exists(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the named logger exists (in the specified domain) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger found, or <c>null</c> if the logger doesn't exist in the specified 
		/// domain.
		/// </returns>
		public static ILog Exists(string domain, string name) 
		{
			return WrapLogger(LoggerManager.Exists(domain, name));
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger, or <c>null</c> if the logger doesn't exist in the specified
		/// assembly's domain.
		/// </returns>
		public static ILog Exists(Assembly domainAssembly, string name) 
		{
			return WrapLogger(LoggerManager.Exists(domainAssembly, name));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the default domain.
		/// </summary>
		/// <remarks>
		/// <para>The root logger is <b>not</b> included in the returned array.</para>
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers()
		{
			return GetCurrentLoggers(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified domain.
		/// </summary>
		/// <param name="domain">The domain to lookup in.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers(string domain)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(domain));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified assembly's domain.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers(Assembly domainAssembly)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(domainAssembly));
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
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string name)
		{
			return GetLogger(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieve a logger named as the <paramref name="name"/>
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
		public static ILog GetLogger(string domain, string name)
		{
			return WrapLogger(LoggerManager.GetLogger(domain, name));
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieve a logger named as the <paramref name="name"/>
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
		public static ILog GetLogger(Assembly domainAssembly, string name)
		{
			return WrapLogger(LoggerManager.GetLogger(domainAssembly, name));
		}	

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Type type) 
		{
			return GetLogger(Assembly.GetCallingAssembly(), type.FullName);
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string domain, Type type) 
		{
			return WrapLogger(LoggerManager.GetLogger(domain, type));
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Assembly domainAssembly, Type type) 
		{
			return WrapLogger(LoggerManager.GetLogger(domainAssembly, type));
		}

		#endregion Type Specific Manager Methods

		#region Domain & Repository Manager Methods

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
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		public static void Shutdown() 
		{
			LoggerManager.Shutdown();
		}

		/// <summary>
		/// Shuts down the default repository.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// default repository.
		/// </para>
		/// <para>Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		public static void ShutdownRepository() 
		{
			ShutdownRepository(Assembly.GetCallingAssembly());
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
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		/// <param name="domain">The domain to shutdown.</param>
		public static void ShutdownRepository(string domain) 
		{
			LoggerManager.ShutdownRepository(domain);
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
			LoggerManager.ShutdownRepository(domainAssembly);
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
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		public static void ResetConfiguration() 
		{
			ResetConfiguration(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.DEBUG"/>. Moreover,
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="domain">The domain to lookup in.</param>
		public static void ResetConfiguration(string domain) 
		{
			LoggerManager.ResetConfiguration(domain);
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.DEBUG"/>. Moreover,
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		public static void ResetConfiguration(Assembly domainAssembly) 
		{
			LoggerManager.ResetConfiguration(domainAssembly);
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <returns>The <see cref="ILoggerRepository"/> instance for the default domain.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the domain specified
		/// by the callers assembly (<see cref="Assembly.GetCallingAssembly()"/>).
		/// </para>
		/// </remarks>
		public static ILoggerRepository GetLoggerRepository()
		{
			return GetLoggerRepository(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <param name="domain">The domain to lookup in.</param>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the domain specified
		/// by the <paramref name="domain"/> argument.
		/// </para>
		/// </remarks>
		public static ILoggerRepository GetLoggerRepository(string domain)
		{
			return LoggerManager.GetLoggerRepository(domain);
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <param name="domainAssembly">The assembly to use to lookup the domain.</param>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the domain specified
		/// by the <paramref name="domainAssembly"/> argument.
		/// </para>
		/// </remarks>
		public static ILoggerRepository GetLoggerRepository(Assembly domainAssembly)
		{
			return LoggerManager.GetLoggerRepository(domainAssembly);
		}

		/// <summary>
		/// Creates a domain with the specified repository type.
		/// </summary>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the domain specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the domain.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetLoggerRepository()"/> will return 
		/// the same repository instance.
		/// </para>
		/// </remarks>
		public static ILoggerRepository CreateDomain(Type repositoryType)
		{
			return CreateDomain(Assembly.GetCallingAssembly(), repositoryType);
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
			return LoggerManager.CreateDomain(domain);
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
		/// An <see cref="Exception"/> will be thrown if the domain already exists.
		/// </para>
		/// </remarks>
		/// <exception cref="LogException">The specified domain already exists.</exception>
		public static ILoggerRepository CreateDomain(string domain, Type repositoryType)
		{
			return LoggerManager.CreateDomain(domain, repositoryType);
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
			return LoggerManager.CreateDomain(domainAssembly, repositoryType);
		}

		/// <summary>
		/// Gets the list of currently defined repositories.
		/// </summary>
		/// <returns>An array of all the known <see cref="ILoggerRepository"/> objects.</returns>
		public static ILoggerRepository[] GetAllRepositories()
		{
			return LoggerManager.GetAllRepositories();
		}

		#endregion Domain & Repository Manager Methods

		#region Extension Handlers

		/// <summary>
		/// Looks up the wrapper object for the logger specified.
		/// </summary>
		/// <param name="logger">The logger to get the wrapper for.</param>
		/// <returns>The wrapper for the logger specified.</returns>
		public static ILog WrapLogger(ILogger logger)
		{
			return (ILog)s_wrapperMap[logger];
		}

		/// <summary>
		/// Looks up the wrapper objects for the loggers specified.
		/// </summary>
		/// <param name="loggers">The loggers to get the wrappers for.</param>
		/// <returns>The wrapper objects for the loggers specified.</returns>
		public static ILog[] WrapLoggers(ILogger[] loggers)
		{
			ILog[] results = new ILog[loggers.Length];
			for(int i=0; i<loggers.Length; i++)
			{
				results[i] = WrapLogger(loggers[i]);
			}
			return results;
		}

		/// <summary>
		/// Create the <see cref="ILoggerWrapper"/> objects used by
		/// this manager.
		/// </summary>
		/// <param name="logger">The logger to wrap.</param>
		/// <returns>The wrapper for the logger specified.</returns>
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
		{
			return new LogImpl(logger);
		}

		#endregion

		#region Private Static Fields

		/// <summary>
		/// The wrapper map to use to hold the <see cref="LogImpl"/> objects.
		/// </summary>
		private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));

		#endregion Private Static Fields
	}
}
