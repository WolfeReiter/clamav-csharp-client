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

namespace log4net.DateFormatter
{
	/// <summary>
	/// Interface to abstract the rendering of a <see cref="DateTime"/>
	/// instance into a string.
	/// </summary>
	public interface IDateFormatter
	{
		/// <summary>
		/// Formats the specified date as a string.
		/// </summary>
		/// <param name="dateToFormat">The date to format.</param>
		/// <param name="buffer">The string builder to write to.</param>
		/// <returns>The string builder passed.</returns>
		StringBuilder FormatDate(DateTime dateToFormat, StringBuilder buffer);
	}
}
