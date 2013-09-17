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
using System.Text.RegularExpressions;

using log4net;
using log4net.spi;
using log4net.helpers;

namespace log4net.Filter
{
	/// <summary>
	/// Simple filter to match a string in the <see cref="NDC"/>
	/// </summary>
	/// <remarks>
	/// Simple filter to match a string in the <see cref="NDC"/>
	/// </remarks>
	public class NDCFilter : FilterSkeleton
	{
		#region Member Variables

		/// <summary>
		/// Flag to indicate the behaviour when we have a match
		/// </summary>
		private bool m_acceptOnMatch = true;

		/// <summary>
		/// The string to substring match against the message
		/// </summary>
		private string m_stringToMatch;

		/// <summary>
		/// A string regex to match
		/// </summary>
		private string m_stringRegexToMatch;

		/// <summary>
		/// A regex object to match (generated from m_stringRegexToMatch)
		/// </summary>
		private Regex m_regexToMatch;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public NDCFilter()
		{
		}

		#endregion

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialise and precompile the Regex if required
		/// </summary>
		override public void ActivateOptions() 
		{
			if (m_stringRegexToMatch != null)
			{
				m_regexToMatch = new Regex(m_stringRegexToMatch, RegexOptions.Compiled);
			}
		}

		#endregion

		/// <summary>
		/// The <see cref="AcceptOnMatch"/> property is a flag that determines
		/// the behaviour when a matching <see cref="Level"/> is found. If the
		/// flag is set to true then the filter will <see cref="FilterDecision.ACCEPT"/> the 
		/// logging event, otherwise it will <see cref="FilterDecision.DENY"/> the event.
		/// </summary>
		public bool AcceptOnMatch
		{
			get { return m_acceptOnMatch; }
			set { m_acceptOnMatch = value; }
		}

		/// <summary>
		/// The string that will be substring matched against
		/// the rendered message. If the message contains this
		/// string then the filter will match.
		/// </summary>
		public string StringToMatch
		{
			get { return m_stringToMatch; }
			set { m_stringToMatch = value; }
		}

		/// <summary>
		/// The regular expression pattern that will be matched against
		/// the rendered message. If the message matches this
		/// pattern then the filter will match.
		/// </summary>
		public string RegexToMatch
		{
			get { return m_stringRegexToMatch; }
			set { m_stringRegexToMatch = value; }
		}

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Check if this filter should allow the event to be logged
		/// </summary>
		/// <remarks>
		/// The <see cref="NDC"/> is matched against the <see cref="StringToMatch"/>.
		/// If the <see cref="StringToMatch"/> occurs as a substring within
		/// the message then a match will have occurred. If no match occurs
		/// this function will return <see cref="FilterDecision.NEUTRAL"/>
		/// allowing other filters to check the event. If a match occurs then
		/// the value of <see cref="AcceptOnMatch"/> is checked. If it is
		/// true then <see cref="FilterDecision.ACCEPT"/> is returned otherwise
		/// <see cref="FilterDecision.DENY"/> is returned.
		/// </remarks>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>see remarks</returns>
		override public FilterDecision Decide(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			string msg = loggingEvent.NestedContext;

			// Check if we have been setup to filter
			if (msg == null || (m_stringToMatch == null && m_regexToMatch == null))
			{
				// We cannot filter so allow the filter chain
				// to continue processing
				return FilterDecision.NEUTRAL;
			}
    
			// Firstly check if we are matching using a regex
			if (m_regexToMatch != null)
			{
				// Check the regex
				if (m_regexToMatch.Match(msg).Success == false)
				{
					// No match, continue processing
					return FilterDecision.NEUTRAL;
				} 

				// we've got a match
				if (m_acceptOnMatch) 
				{
					return FilterDecision.ACCEPT;
				} 
				return FilterDecision.DENY;
			}
			else if (m_stringToMatch != null)
			{
				// Check substring match
				if (msg.IndexOf(m_stringToMatch) == -1) 
				{
					// No match, continue processing
					return FilterDecision.NEUTRAL;
				} 

				// we've got a match
				if (m_acceptOnMatch) 
				{
					return FilterDecision.ACCEPT;
				} 
				return FilterDecision.DENY;
			}
			return FilterDecision.NEUTRAL;
		}

		#endregion
	}
}
