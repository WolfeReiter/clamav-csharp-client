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
	/// Users should implement this interface to implement customised logging
	/// event filtering.
	/// </summary>
	/// <remarks>
	/// <para>Users should implement this interface to implement customized logging
	/// event filtering. Note that <see cref="log4net.Repository.Hierarchy.Logger"/> and 
	/// <see cref="log4net.Appender.AppenderSkeleton"/>, the parent class of all standard
	/// appenders, have built-in filtering rules. It is suggested that you
	/// first use and understand the built-in rules before rushing to write
	/// your own custom filters.</para>
	/// 
	/// <para>This abstract class assumes and also imposes that filters be
	/// organized in a linear chain. The <see cref="Decide"/>
	/// method of each filter is called sequentially, in the order of their 
	/// addition to the chain.</para>
	/// 
	/// <para>The <see cref="Decide"/> method must return one
	/// of the integer constants <see cref="FilterDecision.DENY"/>, 
	/// <see cref="FilterDecision.NEUTRAL"/> or <see cref="FilterDecision.ACCEPT"/>.</para>
	/// 
	/// <para>If the value <see cref="FilterDecision.DENY"/> is returned, then the log event is dropped 
	/// immediately without consulting with the remaining filters. </para>
	/// 
	/// <para>If the value <see cref="FilterDecision.NEUTRAL"/> is returned, then the next filter
	/// in the chain is consulted. If there are no more filters in the
	/// chain, then the log event is logged. Thus, in the presence of no
	/// filters, the default behaviour is to log all logging events.</para>
	/// 
	/// <para>If the value <see cref="FilterDecision.ACCEPT"/> is returned, then the log
	/// event is logged without consulting the remaining filters. </para>
	/// 
	/// <para>The philosophy of log4net filters is largely inspired from the
	/// Linux ipchains.</para>
	/// </remarks>
	public interface IFilter : IOptionHandler
	{
		/// <summary>
		/// Decide if the logging event should be logged through an appender.
		/// </summary>
		/// <param name="loggingEvent">The LoggingEvent to decide upon</param>
		/// <returns>The decision of the filter</returns>
		/// <remarks>
		/// <para>If the decision is <see cref="FilterDecision.DENY"/>, then the event will be
		/// dropped. If the decision is <see cref="FilterDecision.NEUTRAL"/>, then the next
		/// filter, if any, will be invoked. If the decision is <see cref="FilterDecision.ACCEPT"/> then
		/// the event will be logged without consulting with other filters in
		/// the chain.</para>
		/// </remarks>
		FilterDecision Decide(LoggingEvent loggingEvent);

		/// <summary>
		/// Property to get and set the next filter in the filter
		/// chain of responsibility.
		/// </summary>
		/// <value>
		/// The next filter in the chain
		/// </value>
		/// <remarks>
		/// Filters are typically composed into chains. This property allows the next filter in 
		/// the chain to be accessed.
		/// </remarks>
		IFilter Next { get; set; }
	}

	/// <summary>
	/// The return result from <see cref="IFilter.Decide"/>
	/// </summary>
	/// <remarks>
	/// The return result from <see cref="IFilter.Decide"/>
	/// </remarks>
	public enum FilterDecision : int
	{
		/// <summary>
		/// The log event must be dropped immediately without 
		/// consulting with the remaining filters, if any, in the chain.
		/// </summary>
		DENY = -1,
  
		/// <summary>
		/// This filter is neutral with respect to the log event. 
		/// The remaining filters, if any, should be consulted for a final decision.
		/// </summary>
		NEUTRAL = 0,

		/// <summary>
		/// The log event must be logged immediately without 
		/// consulting with the remaining filters, if any, in the chain.
		/// </summary>
		ACCEPT = 1,
	}

}
