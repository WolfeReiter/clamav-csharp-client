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
	/// Interface for raw layout objects
	/// </summary>
	/// <remarks>
	/// <para>Interface used to format a <see cref="LoggingEvent"/>
	/// to an object.</para>
	/// 
	/// <para>This interface should not be confused with the
	/// <see cref="ILayout"/> interface. This interface is used in
	/// only certain specialised situations where a raw object is
	/// required rather than a formatted string. The <see cref="ILayout"/>
	/// is not generally usefull than this interface.</para>
	/// </remarks>
	public class Layout2RawLayoutAdapter : IRawLayout
	{
		#region Member Variables

		/// <summary>
		/// The layout to adapt
		/// </summary>
		private ILayout m_layout;

		#endregion

		#region Constructors

		/// <summary>
		/// Construst a new adapter
		/// </summary>
		/// <param name="layout">the layout to adapt</param>
		public Layout2RawLayoutAdapter(ILayout layout)
		{
			m_layout = layout;
		}

		#endregion

		#region Implementation of IRawLayout

		/// <summary>
		/// Format the logging event as an object.
		/// </summary>
		/// <param name="loggingEvent">The event to format</param>
		/// <returns>returns the formatted event</returns>
		/// <remarks>
		/// <para>Format the logging event as an object.</para>
		/// <para>Uses the <see cref="ILayout"/> object supplied to 
		/// the constructor to perform the formatting.</para>
		/// </remarks>
		virtual public object Format(LoggingEvent loggingEvent)
		{
			return m_layout.Format(loggingEvent);
		}

		#endregion
	}
}
