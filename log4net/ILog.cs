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

namespace log4net
{
	/// <summary>
	/// The ILog interface is use by application to log messages into
	/// the log4net framework.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use the <see cref="LogManager"/> to obtain logger instances
	/// that implement this interface. The <see cref="LogManager.GetLogger"/>
	/// static method is used to get logger instances.
	/// </para>
	/// <para>
	/// This class contains methods for logging at different levels and also
	/// has properties for determining if those logging levels are
	/// enabled in the current configuration.
	/// </para>
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
	/// <seealso cref="LogManager"/>
	/// <seealso cref="LogManager.GetLogger"/>
	public interface ILog : ILoggerWrapper
	{
		/// <summary>
		/// Logs a message object with the <see cref="Level.DEBUG"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>DEBUG</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.DEBUG"/> level. If this logger is
		/// <c>DEBUG</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Debug(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Debug(object,Exception)"/>
		/// <seealso cref="IsDebugEnabled"/>
		void Debug(object message);
  
		/// <summary>
		/// Log a message object with the <see cref="Level.DEBUG"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <remarks>
		/// See the <see cref="Debug(object)"/> form for more detailed information.
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <param name="t">The exception to log, including its stack trace.</param>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		void Debug(object message, Exception t);

		/// <summary>
		/// Logs a message object with the <see cref="Level.INFO"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.INFO"/> level. If this logger is
		/// <c>INFO</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Info(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Info(object,Exception)"/>
		/// <seealso cref="IsInfoEnabled"/>
		void Info(object message);
  
		/// <summary>
		/// Logs a message object with the <c>INFO</c> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <remarks>
		/// See the <see cref="Info(object)"/> form for more detailed information.
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <param name="t">The exception to log, including its stack trace.</param>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="IsInfoEnabled"/>
		void Info(object message, Exception t);

		/// <summary>
		/// Log a message object with the <see cref="Level.WARN"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>WARN</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.WARN"/> level. If this logger is
		/// <c>WARN</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Warn(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Warn(object,Exception)"/>
		/// <seealso cref="IsWarnEnabled"/>
		void Warn(object message);
  
		/// <summary>
		/// Log a message object with the <see cref="Level.WARN"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <remarks>
		/// See the <see cref="Warn(object)"/> form for more detailed information.
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <param name="t">The exception to log, including its stack trace.</param>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="IsWarnEnabled"/>
		void Warn(object message, Exception t);

		/// <summary>
		/// Logs a message object with the <see cref="Level.ERROR"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>ERROR</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.ERROR"/> level. If this logger is
		/// <c>ERROR</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Error(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Error(object,Exception)"/>
		/// <seealso cref="IsErrorEnabled"/>
		void Error(object message);

		/// <summary>
		/// Log a message object with the <see cref="Level.ERROR"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <remarks>
		/// See the <see cref="Error(object)"/> form for more detailed information.
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <param name="t">The exception to log, including its stack trace.</param>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="IsErrorEnabled"/>
		void Error(object message, Exception t);

		/// <summary>
		/// Log a message object with the <see cref="Level.FATAL"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>FATAL</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.FATAL"/> level. If this logger is
		/// <c>FATAL</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Fatal(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Fatal(object,Exception)"/>
		/// <seealso cref="IsFatalEnabled"/>
		void Fatal(object message);
  
		/// <summary>
		/// Log a message object with the <see cref="Level.FATAL"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <remarks>
		/// See the <see cref="Fatal(object)"/> form for more detailed information.
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <param name="t">The exception to log, including its stack trace.</param>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="IsFatalEnabled"/>
		void Fatal(object message, Exception t);

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.DEBUG"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.DEBUG"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// This function is intended to lessen the computational cost of
		/// disabled log debug statements.
		/// </para>
		/// <para> For some ILog interface <c>log</c>, when you write:</para>
		/// <code>
		/// log.Debug("This is entry number: " + i );
		/// </code>
		/// <para>
		/// You incur the cost constructing the message, string construction and concatenation in
		/// this case, regardless of whether the message is logged or not.
		/// </para>
		/// <para>
		/// If you are worried about speed (who isn't), then you should write:
		/// </para>
		/// <code>
		/// if (log.IsDebugEnabled)
		/// { 
		///     log.Debug("This is entry number: " + i );
		/// }
		/// </code>
		/// <para>
		/// This way you will not incur the cost of parameter
		/// construction if debugging is disabled for <c>log</c>. On
		/// the other hand, if the <c>log</c> is debug enabled, you
		/// will incur the cost of evaluating whether the logger is debug
		/// enabled twice. Once in <see cref="IsDebugEnabled"/> and once in
		/// the <see cref="Debug"/>.  This is an insignificant overhead
		/// since evaluating a logger takes about 1% of the time it
		/// takes to actually log. This is the preferred style of logging.
		/// </para>
		/// <para>Alternatively if your logger is available statically then the is debug
		/// enabled state can be stored in a static variable like this:
		/// </para>
		/// <code>
		/// private static readonly bool isDebugEnabled = log.IsDebugEnabled;
		/// </code>
		/// <para>
		/// Then when you come to log you can write:
		/// </para>
		/// <code>
		/// if (isDebugEnabled)
		/// { 
		///     log.Debug("This is entry number: " + i );
		/// }
		/// </code>
		/// <para>
		/// This way the debug enabled state is only queried once
		/// when the class is loaded. Using a <c>private static readonly</c>
		/// variable is the most efficient because it is a run time constant
		/// and can be heavily optimised by the JIT compiler.
		/// </para>
		/// <para>
		/// Of course if you use a static readonly variable to
		/// hold the enabled state of the logger then you cannot
		/// change the enabled state at runtime to vary the logging
		/// that is produced. You have to decide if you need absolute
		/// speed or runtime flexibility.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug"/>
		bool IsDebugEnabled { get; }
  
		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.INFO"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.INFO"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Info"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsInfoEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.WARN"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.WARN"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Warn"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsWarnEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.ERROR"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.ERROR"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Error"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsErrorEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.FATAL"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.FATAL"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Fatal"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsFatalEnabled { get; }
	}
}
