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

using log4net.helpers;
using log4net.spi;


namespace log4net.Repository.Hierarchy
{
	/// <summary>
	/// The <see cref="RootLogger" /> sits at the top of the logger hierarchy. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="RootLogger" /> is a regular <see cref="Logger" /> except 
	/// that it provides several guarantees.
	/// </para>
	/// <para>
	/// First, it cannot be assigned a <c>null</c>
	/// level. Second, since the root logger cannot have a parent, the
	/// <see cref="EffectiveLevel"/> property always returns the value of the
	/// level field without walking the hierarchy.
	/// </para>
	/// </remarks>
	public class RootLogger : Logger
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RootLogger" /> class with
		/// the specified logging level.
		/// </summary>
		/// <param name="level">The level to assign to the root logger.</param>
		/// <remarks>
		/// The root logger names itself as "root". However, the root
		/// logger cannot be retrieved by name.
		/// </remarks>
		public RootLogger(Level level) : base("root")
		{
			this.Level = level;
		}

		#endregion Public Instance Constructors

		#region Override implementation of Logger

		/// <summary>
		/// Gets the assigned level value without walking the logger hierarchy.
		/// </summary>
		/// <value>The assigned level value without walking the logger hierarchy.</value>
		override public Level EffectiveLevel 
		{
			get 
			{
				return base.Level;
			}
		}

		/// <summary>
		/// Gets or sets the assigned <see cref="Level"/>, if any, for the root
		/// logger.  
		/// </summary>
		/// <value>
		/// The <see cref="Level"/> of the root logger.
		/// </value>
		/// <summary>
		/// Setting the level of the root logger to a null reference
		/// may have catastrophic results. We prevent this here.
		/// </summary>
		override public Level Level
		{
			get { return base.Level; }
			set
			{
				if (value == null) 
				{
					LogLog.Error("RootLogger: You have tried to set a null level to root.", new LogException());
				}
				else 
				{
					base.Level = value;
				}
			}
		}

		#endregion Override implementation of Logger
	}
}
