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

using log4net.spi;

namespace log4net.helpers
{
	/// <summary>
	/// Abstract class that provides the formatting functionality that 
	/// derived classes need.
	/// </summary>
	/// <remarks>
	/// Conversion specifiers in a conversion patterns are parsed to
	/// individual PatternConverters. Each of which is responsible for
	/// converting a logging event in a converter specific manner.
	/// </remarks>
	public abstract class PatternConverter
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternConverter" /> class.
		/// </summary>
		protected PatternConverter() 
		{  
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternConverter" /> class 
		/// with the specified formatting info object.
		/// </summary>
		/// <param name="formattingInfo">The formatting info object to use.</param>
		protected PatternConverter(FormattingInfo formattingInfo) 
		{
			m_min = formattingInfo.Min;
			m_max = formattingInfo.Max;
			m_leftAlign = formattingInfo.LeftAlign;
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// the next patter converter in the chain
		/// </summary>
		public PatternConverter Next
		{
			get { return m_next; }
			set { m_next = value; }
		}

		#endregion Public Instance Properties

		#region Protected Abstract Methods

		/// <summary>
		/// Derived pattern converters must override this method in order to
		/// convert conversion specifiers in the correct way.
		/// </summary>
		/// <param name="loggingEvent">The <see cref="LoggingEvent" /> on which the pattern converter should be executed.</param>
		/// <returns></returns>
		abstract protected string Convert(LoggingEvent loggingEvent);

		#endregion Protected Abstract Methods

		#region Public Instance Methods

		/// <summary>
		/// A template method for formatting in a converter specific way.
		/// </summary>
		/// <param name="buffer"><see cref="StringBuilder" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">The <see cref="LoggingEvent" /> on which the pattern converter should be executed.</param>
		virtual public void Format(StringBuilder buffer, LoggingEvent loggingEvent) 
		{
			string s = Convert(loggingEvent);

			if (s == null) 
			{
				if (0 < m_min)
				{
					SpacePad(buffer, m_min);
				}
				return;
			}

			int len = s.Length;

			if (len > m_max)
			{
				buffer.Append(s.Substring(len - m_max));
			}
			else if (len < m_min) 
			{
				if (m_leftAlign) 
				{	
					buffer.Append(s);
					SpacePad(buffer, m_min - len);
				}
				else 
				{
					SpacePad(buffer, m_min - len);
					buffer.Append(s);
				}
			}
			else
			{
				buffer.Append(s);
			}
		}	

		static readonly string[] SPACES = {	" ", "  ", "    ", "        ",			// 1,2,4,8 spaces
											"                ",						// 16 spaces
											"                                " };	// 32 spaces

		/// <summary>
		/// Fast space padding method.
		/// </summary>
		/// <param name="buffer"><see cref="StringBuilder" /> to which the spaces will be appended.</param>
		/// <param name="length">The number of spaces to be padded.</param>
		public void SpacePad(StringBuilder buffer, int length) 
		{
			while(length >= 32) 
			{
				buffer.Append(SPACES[5]);
				length -= 32;
			}
    
			for(int i = 4; i >= 0; i--) 
			{	
				if ((length & (1<<i)) != 0) 
				{
					buffer.Append(SPACES[i]);
				}
			}
		}	

		#endregion Public Instance Methods

		#region Private Instance Fields

		private PatternConverter m_next;
		private int m_min = -1;
		private int m_max = 0x7FFFFFFF;
		private bool m_leftAlign = false;

		#endregion Private Instance Fields
	}
}
