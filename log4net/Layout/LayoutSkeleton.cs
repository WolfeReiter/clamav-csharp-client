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

using log4net;
using log4net.spi;

namespace log4net.Layout
{
	/// <summary>
	/// Extend this abstract class to create your own log layout format.
	/// </summary>
	/// <remarks>
	/// <para>This is the base implementation of the <see cref="ILayout"/>
	/// interface. Most layout objects should extend this class.</para>
	/// </remarks>
	public abstract class LayoutSkeleton : ILayout, IOptionHandler
	{
		#region Member Variables

		/// <summary>
		/// The header text
		/// </summary>
		/// <remarks>
		/// <para>See <see cref="Header"/> for more information.</para>
		/// </remarks>
		private string m_header = null;

		/// <summary>
		/// The footer text
		/// </summary>
		/// <remarks>
		/// <para>See <see cref="Footer"/> for more information.</para>
		/// </remarks>
		private string m_footer = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Empty default constructor
		/// </summary>
		/// <remarks>
		/// Empty default constructor
		/// </remarks>
		protected LayoutSkeleton()
		{
		}

		#endregion

		#region Implementation of IOptionHandler

		/// <summary>
		/// Activate the options that were previously set with calls to option setters.
		/// </summary>
		/// <remarks>
		/// <para>This allows deferred activation of the options once all
		/// options have been set. This is required for components which have
		/// related options that remain ambiguous until all are set.</para>
		/// 
 		/// <para>This method must be implemented by the subclass.</para>
		/// </remarks>
		abstract public void ActivateOptions();

		#endregion

		#region Implementation of ILayout

		/// <summary>
		/// Implement this method to create your own layout format.
		/// </summary>
		/// <param name="loggingEvent">The event to format</param>
		/// <returns>returns the formatted event</returns>
		/// <remarks>
		/// <para>This method is called by an appender to format
		/// the <paramref name="loggingEvent"/> as a string.</para>
		/// 
 		/// <para>This method must be implemented by the subclass.</para>
		/// </remarks>
		abstract public	string Format(LoggingEvent loggingEvent);

		/// <summary>
		/// The content type output by this layout. 
		/// </summary>
		/// <value>The content type is <c>"text/plain"</c></value>
		/// <remarks>
		/// <para>The content type output by this layout.</para>
		/// <para>This base class uses the value <c>"text/plain"</c>.
		/// To change this value a subclass must override this
		/// property.</para>
		/// </remarks>
		virtual public string ContentType
		{
			get { return "text/plain"; }
		}

		/// <summary>
		/// The header for the layout format.
		/// </summary>
		/// <value>the layout header</value>
		/// <remarks>
		/// <para>The Header text will be appended before any logging events
		/// are formatted and appended.</para>
		/// </remarks>
		virtual public string Header
		{
			get { return m_header; }
			set { m_header = value; }
		}

		/// <summary>
		/// The footer for the layout format.
		/// </summary>
		/// <value>the layout footer</value>
		/// <remarks>
		/// <para>The Footer text will be appended after all the logging events
		/// have been formatted and appended.</para>
		/// </remarks>
		virtual public string Footer
		{
			get { return m_footer; }
			set { m_footer = value; }
		}

		/// <summary>
		/// Flag indicating if this layout handle exceptions
		/// </summary>
		/// <value><c>false</c> if this layout handles exceptions</value>
		/// <remarks>
		/// <para>If this layout handles the exception object contained within
		/// <see cref="LoggingEvent"/>, then the layout should return
		/// <c>false</c>. Otherwise, if the layout ignores the exception
		/// object, then the layout should return <c>true</c>.</para>
		/// 
		/// <para>This method must be implemented by the subclass.</para>
		/// </remarks>
		abstract public	bool IgnoresException { get; }

		#endregion
	}
}
