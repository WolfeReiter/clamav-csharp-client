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

namespace log4net.Filter
{
	/// <summary>
	/// This filter drops all <see cref="LoggingEvent"/>. 
	/// </summary>
	/// <remarks>
	/// You can add this filter to the end of a filter chain to
	/// switch from the default "accept all unless instructed otherwise"
	/// filtering behaviour to a "deny all unless instructed otherwise"
	/// behaviour.	
	/// </remarks>
	public class DenyAllFilter : FilterSkeleton
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public DenyAllFilter()
		{
		}

		#endregion

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Always returns the integer constant <see cref="FilterDecision.DENY"/>
		/// </summary>
		/// <remarks>
		/// Ignores the event being logged and just returns
		/// <see cref="FilterDecision.DENY"/>. This can be used to change the default filter
		/// chain behaviour from <see cref="FilterDecision.ACCEPT"/> to <see cref="FilterDecision.DENY"/>. This filter
		/// should only be used as the last filter in the chain
		/// as any further filters will be ignored!
		/// </remarks>
		/// <param name="loggingEvent">the LoggingEvent to filter</param>
		/// <returns>Always returns <see cref="FilterDecision.DENY"/></returns>
		override public FilterDecision Decide(LoggingEvent loggingEvent) 
		{
			return FilterDecision.DENY;
		}

		#endregion
	}
}
