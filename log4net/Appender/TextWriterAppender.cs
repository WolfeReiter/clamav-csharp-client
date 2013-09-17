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
using System.IO;

using log4net.helpers;
using log4net.Layout;
using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Sends logging events to a <see cref="TextWriter"/>.
	/// </summary>
	/// <remarks>
	/// An Appender that writes to a <see cref="TextWriter"/>.
	/// </remarks>
	public class TextWriterAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterAppender" /> class.
		/// </summary>
		public TextWriterAppender() 
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterAppender" /> class and
		/// sets the output destination to a new <see cref="StreamWriter"/> initialized 
		/// with the specified <see cref="Stream"/>.
		/// </summary>
		/// <param name="layout">The layout to use with this appender.</param>
		/// <param name="os">The <see cref="Stream"/> to output to.</param>
		public TextWriterAppender(ILayout layout, Stream os) : this(layout, new StreamWriter(os))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterAppender" /> class and sets
		/// the output destination to the specified <see cref="StreamWriter" />.
		/// </summary>
		/// <param name="layout">The layout to use with this appender</param>
		/// <param name="writer">The <see cref="TextWriter" /> to output to</param>
		/// <remarks>
		/// The <see cref="TextWriter" /> must have been previously opened.
		/// </remarks>
		public TextWriterAppender(ILayout layout, TextWriter writer) 
		{
			Layout = layout;
			Writer = writer;
		}

		#endregion

		#region Public Instance Properties

		/// <summary>
		/// Gets or set whether the appender will flush at the end 
		/// of each append operation.
		/// </summary>
		/// <value>
		/// <para>
		/// The default behaviour is to flush at the end of each 
		/// append operation.
		/// </para>
		/// <para>
		/// If this option is set to <c>false</c>, then the underlying 
		/// stream can defer persisting the logging event to a later 
		/// time.
		/// </para>
		/// </value>
		/// <remarks>
		/// Avoiding the flush operation at the end of each append results in
		/// a performance gain of 10 to 20 percent. However, there is safety
		/// trade-off involved in skipping flushing. Indeed, when flushing is
		/// skipped, then it is likely that the last few log events will not
		/// be recorded on disk when the application exits. This is a high
		/// price to pay even for a 20% performance gain.
		/// </remarks>
		public bool ImmediateFlush 
		{
			get { return m_immediateFlush; }
			set { m_immediateFlush = value; }
		}

		/// <summary>
		/// Sets the <see cref="TextWriter"/> where the log output will go.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The specified <see cref="TextWriter"/> must be open and writable.
		/// </para>
		/// <para>
		/// The <see cref="TextWriter"/> will be closed when the appender 
		/// instance is closed.
		/// </para>
		/// <para>
		/// <b>Note:</b> Logging to an unopened <see cref="TextWriter"/> will fail.
		/// </para>
		/// </remarks>
		virtual public TextWriter Writer 
		{
			set 
			{
				lock(this) 
				{
					Reset();
					m_qtw = new QuietTextWriter(value, ErrorHandler);
					WriteHeader();
				}
			}
		}

		#endregion Public Instance Properties

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This method determines if there is a sense in attempting to append.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method checked if an output target has been set and if a
		/// layout has been set. 
		/// </para>
		/// </remarks>
		/// <returns><c>false</c> if any of the preconditions fail.</returns>
		override protected bool PreAppendCheck() 
		{
			if (!base.PreAppendCheck()) 
			{
				return false;
			}

			if (m_qtw == null) 
			{
				ErrorHandler.Error("No output stream or file set for the appender named ["+ Name +"].");
				return false;
			}
			if (m_qtw.Closed) 
			{
				ErrorHandler.Error("Output stream for appender named ["+ Name +"] has been closed.");
				return false;
			}
	
			return true;
		}

		/// <summary>
		/// This method is called by the <see cref="AppenderSkeleton.DoAppend"/>
		/// method. 
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes a log statement to the output stream if the output stream exists 
		/// and is writable.  
		/// </para>
		/// <para>
		/// The format of the output will depend on the appender's layout.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			m_qtw.Write(RenderLoggingEvent(loggingEvent));

			if (m_immediateFlush) 
			{
				m_qtw.Flush();
			} 
		} 

		/// <summary>
		/// Close this appender instance. The underlying stream or writer is also closed.
		/// </summary>
		/// <remarks>
		/// Closed appenders cannot be reused.
		/// </remarks>
		override public void OnClose() 
		{
			lock(this)
			{
				Reset();
			}
		}

		/// <summary>
		/// Gets or set the <see cref="IErrorHandler"/> and the underlying 
		/// <see cref="QuietTextWriter"/>, if any, for this appender. 
		/// </summary>
		/// <value>
		/// The <see cref="IErrorHandler"/> for this appender.
		/// </value>
		override public IErrorHandler ErrorHandler
		{
			get { return base.ErrorHandler; }
			set
			{
				lock(this)
				{
					if (value == null) 
					{
						LogLog.Warn("TextWriterAppender: You have tried to set a null error-handler.");
					} 
					else 
					{
						base.ErrorHandler = value;
						if (m_qtw != null) 
						{
							m_qtw.ErrorHandler = value;
						}
					}	
				}
			}
		}

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion Override implementation of AppenderSkeleton

		#region Protected Instance Methods

		/// <summary>
		/// Writes the footer and closes the underlying <see cref="TextWriter"/>.
		/// </summary>
		virtual protected void WriteFooterAndCloseWriter()
		{
			WriteFooter();
			CloseWriter();
		}

		/// <summary>
		/// Closes the underlying <see cref="TextWriter"/>.
		/// </summary>
		virtual protected void CloseWriter() 
		{
			if (m_qtw != null) 
			{
				try 
				{
					m_qtw.Close();
				} 
				catch(Exception e) 
				{
					ErrorHandler.Error("Could not close writer ["+m_qtw+"]", e); 
					// do need to invoke an error handler
					// at this late stage
				}
			}
		}

		/// <summary>
		/// Clears internal references to the underlying <see cref="TextWriter" /> 
		/// and other variables.
		/// </summary>
		/// <remarks>
		/// Subclasses can override this method for an alternate closing behaviour.
		/// </remarks>
		virtual protected void Reset() 
		{
			WriteFooterAndCloseWriter();
			m_qtw = null;
		}

		/// <summary>
		/// Writes a footer as produced by the embedded layout's <see cref="ILayout.Footer"/> property.
		/// </summary>
		virtual protected void WriteFooter() 
		{
			if (Layout != null && m_qtw != null && !m_qtw.Closed) 
			{
				string f = Layout.Footer;
				if (f != null)
				{
					m_qtw.Write(f);
				}
			}
		}

		/// <summary>
		/// Writes a header produced by the embedded layout's <see cref="ILayout.Header"/> property.
		/// </summary>
		virtual protected void WriteHeader() 
		{
			if (Layout != null && m_qtw != null && !m_qtw.Closed) 
			{
				string h = Layout.Header;
				if (h != null)
				{
					m_qtw.Write(h);
				}
			}
		}	

		#endregion Protected Instance Methods

		#region Protected Instance Fields

		/// <summary>
		/// This is the <see cref="log4net.helpers.QuietTextWriter"/> where logging events
		/// will be written to. 
		/// </summary>
		protected QuietTextWriter m_qtw;

		#endregion Protected Instance Fields

		#region Private Instance Fields

		/// <summary>
		/// Immediate flush means that the underlying <see cref="TextWriter" /> 
		/// or output stream will be flushed at the end of each append operation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Immediate flush is slower but ensures that each append request is 
		/// actually written. If <see cref="ImmediateFlush"/> is set to
		/// <c>false</c>, then there is a good chance that the last few
		/// logging events are not actually persisted if and when the application 
		/// crashes.
		/// </para>
		/// <para>
		/// The default value is <c>true</c>.
		/// </para>
		/// </remarks>
		private bool m_immediateFlush = true;

		#endregion Private Instance Fields
	}
}
