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
using System.Text;
using System.Xml;
using System.IO;

using log4net.spi;
using log4net.helpers;

namespace log4net.Layout
{
	/// <summary>
	/// Layout that formats the log events as XML elements.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is an abstract class that must be subclassed by an implementation 
	/// to conform to a specific schema.
	/// </para>
	/// <para>
	/// Deriving classes must implement the <see cref="FormatXml"/> method.
	/// </para>
	/// </remarks>
	abstract public class XmlLayoutBase : LayoutSkeleton
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutBase" /> class
		/// with no location info.
		/// </summary>
		protected XmlLayoutBase() : this(false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutBase" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <paramref name="locationInfo" /> parameter determines whether 
		/// location information will be output by the layout. If 
		/// <paramref name="locationInfo" /> is set to <c>true</c>, then the 
		/// file name and line number of the statement at the origin of the log 
		/// statement will be output. 
		/// </para>
		/// <para>
		/// If you are embedding this layout within an SMTPAppender
		/// then make sure to set the <b>LocationInfo</b> option of that 
		/// appender as well.
		/// </para>
		/// </remarks>
		protected XmlLayoutBase(bool locationInfo)
		{
			m_locationInfo = locationInfo;
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets a value indicating whether to include location information in 
		/// the XML events.
		/// </summary>
		/// <value>
		/// <c>true</c> if location information should be included in the XML 
		/// events; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// If <see cref="LocationInfo" /> is set to <c>true</c>, then the file 
		/// name and line number of the statement at the origin of the log 
		/// statement will be output. 
		/// </para>
		/// <para>
		/// If you are embedding this layout within an SMTPAppender
		/// then make sure to set the <b>LocationInfo</b> option of that 
		/// appender as well.
		/// </para>
		/// </remarks>
		public bool LocationInfo
		{
			get { return m_locationInfo; }
			set { m_locationInfo = value; }
		}

		#endregion

		#region Implementation of IOptionHandler

		/// <summary>
		/// Does not do anything as options become effective immediately.
		/// </summary>
		override public void ActivateOptions() 
		{
			// nothing to do
		}

		#endregion Implementation of IOptionHandler

		#region Override implementation of LayoutSkeleton

		/// <summary>
		/// Gets the content type output by this layout. 
		/// </summary>
		/// <value>As this is the XML layout, the value is always "text/xml".</value>
		override public string ContentType
		{
			get { return "text/xml"; }
		}

		/// <summary>
		/// The XMLLayout does handle the exception contained within
		/// LoggingEvents. Thus, it returns <c>false</c>.
		/// </summary>
		override public bool IgnoresException
		{
			get { return false; }
		}

		/// <summary>
		/// Produces a formatted string.
		/// </summary>
		/// <param name="loggingEvent">The event being logged.</param>
		/// <returns>The formatted string.</returns>
		override public string Format(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			// Reset working string buffer
			if (m_sbuf.Capacity > XmlLayoutBase.MaximumCapacity) 
			{
				m_sbuf = new StringBuilder(XmlLayoutBase.BufferSize);
			} 
			else 
			{
				m_sbuf.Length = 0;
			}

			StringWriter writer = new StringWriter(m_sbuf, System.Globalization.CultureInfo.InvariantCulture);
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.None;
			xmlWriter.Namespaces = false;

			// Write the event to the writer
			FormatXml(xmlWriter, loggingEvent);

			xmlWriter.WriteWhitespace(SystemInfo.NewLine);
			xmlWriter.Close();

			return m_sbuf.ToString();
		}

		#endregion Override implementation of LayoutSkeleton

		#region Protected Instance Methods

		/// <summary>
		/// Does the actual writing of the XML.
		/// </summary>
		/// <param name="writer">The writer to use to output the event to.</param>
		/// <param name="loggingEvent">The event to write.</param>
		abstract protected void FormatXml(XmlWriter writer, LoggingEvent loggingEvent);

		#endregion Protected Instance Methods

		#region Private Instance Fields
  
		/// <summary>
		/// Output buffer appended to when <see cref="Format(LoggingEvent)" /> 
		/// is invoked.
		/// </summary>
		private StringBuilder m_sbuf = new StringBuilder(XmlLayoutBase.BufferSize);

		/// <summary>
		/// Flag to indicate if location information should be included in
		/// the XML events.
		/// </summary>
		private bool m_locationInfo = false;

		#endregion Private Instance Fields

		#region Protected Static Fields

		/// <summary>
		/// Initial buffer size.
		/// </summary>
		protected const int BufferSize = 256;

		/// <summary>
		/// Maximum buffer size before it is recycled.
		/// </summary>
		protected const int MaximumCapacity = 1024;

		#endregion Protected Static Fields
	}
}
