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

using log4net.spi;

namespace log4net.helpers
{
	/// <summary>
	/// QuietTextWriter does not throw exceptions when things go wrong. 
	/// Instead, it delegates error handling to its <see cref="IErrorHandler"/>.
	/// </summary>
	public class QuietTextWriter
	{
		#region Public Instance Constructors

		/// <summary>
		/// Create a new QuietTextWriter using a writer and error handler
		/// </summary>
		/// <param name="writer">the writer to actually write to</param>
		/// <param name="errorHandler">the error handler to report error to</param>
		public QuietTextWriter(TextWriter writer, IErrorHandler errorHandler) 
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (errorHandler == null)
			{
				throw new ArgumentNullException("errorHandler");
			}

			m_writer = writer;
			ErrorHandler = errorHandler;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the error handler that all errors are 
		/// passed to.
		/// </summary>
		/// <value>
		/// The error handler that all errors are passed to.
		/// </value>
		public IErrorHandler ErrorHandler
		{
			get { return m_errorHandler; }
			set
			{
				if (value == null)
				{
					// This is a programming error on the part of the enclosing appender.
					throw new ArgumentNullException("value");
				}
				m_errorHandler = value;
			}
		}	

		/// <summary>
		/// Gets a value indicating whether this writer is closed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this writer is closed, otherwise <c>false</c>.
		/// </value>
		public bool Closed
		{
			get { return m_closed; }
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties

		/// <summary>
		/// Gets the underlying <see cref="TextWriter" />.
		/// </summary>
		/// <value>
		/// The underlying <see cref="TextWriter" />.
		/// </value>
		protected TextWriter Writer 
		{
			get { return this.m_writer; }
		}

		#endregion Protected Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Writes a string to the output.
		/// </summary>
		/// <param name="value">The string data to write to the output.</param>
		virtual public void Write(string value) 
		{
			try 
			{
				this.Writer.Write(value);
			} 
			catch(Exception e) 
			{
				m_errorHandler.Error("Failed to write [" + value + "].", e, ErrorCodes.WriteFailure);
			}
		}

		/// <summary>
		/// Flushes any buffered output.
		/// </summary>
		public void Flush() 
		{
			try 
			{
				this.Writer.Flush();
			} 
			catch(Exception e) 
			{
				m_errorHandler.Error("Failed to flush writer.", e, ErrorCodes.FlushFailure);
			}	
		}

		/// <summary>
		/// Closes the underlying output writer.
		/// </summary>
		public void Close()
		{
			m_closed = true;
			this.Writer.Close();
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The error handler instance to pass all errors to
		/// </summary>
		private IErrorHandler m_errorHandler;

		/// <summary>
		/// The instance of the underlying TextWriter used for output
		/// </summary>
		private TextWriter m_writer;

		/// <summary>
		/// Flag to indicate if this writer is closed
		/// </summary>
		private bool m_closed = false;

		#endregion Private Instance Fields
	}
}
