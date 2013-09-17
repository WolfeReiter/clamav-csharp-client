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

namespace log4net.Repository
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public interface IBasicRepositoryConfigurator
	{
		/// <summary>
		/// Initialise the log4net system using the specified appender
		/// </summary>
		/// <param name="appender">the appender to use to log all logging events</param>
		void Configure(log4net.Appender.IAppender appender);
	}
}
