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
using System.Collections;

namespace log4net.spi
{
	/// <summary>
	/// Defines the set of levels recognised by the system.
	/// </summary>
	/// <remarks>
	/// <para>Defines the set of levels recognised by the system.</para>
	/// 
	/// <para>The predefined set of levels recognised by the system are 
	/// <see cref="OFF"/>, <see cref="FATAL"/>, <see cref="ERROR"/>, 
	/// <see cref="WARN"/>, <see cref="INFO"/>, <see cref="DEBUG"/> and 
	/// <see cref="ALL"/>.</para>
	/// 
	/// <para>The Level class is sealed. You cannot extend this class.</para> 
	/// </remarks>
#if !NETCF
	[Serializable]
#endif
	sealed public class Level
	{
		#region Static Member Variables

		/// <summary>
		/// The <c>OFF</c> level designates a higher level than all the rest.
		/// </summary>
		public readonly static Level OFF = new Level(int.MaxValue, "OFF");

		/// <summary>
		/// The <c>EMERGENCY</c> level designates very severe error events. System unusable, emergencies.
		/// </summary>
		public readonly static Level EMERGENCY = new Level(120000, "EMERGENCY");

		/// <summary>
		/// The <c>FATAL</c> level designates very severe error events that will presumably lead the application to abort.
		/// </summary>
		public readonly static Level FATAL = new Level(110000, "FATAL");

		/// <summary>
		/// The <c>ALERT</c> level designates very severe error events. Take immediate action, alerts.
		/// </summary>
		public readonly static Level ALERT = new Level(100000, "ALERT");

		/// <summary>
		/// The <c>CRITICAL</c> level designates very severe error events. Critical condition, critical.
		/// </summary>
		public readonly static Level CRITICAL = new Level(90000, "CRITICAL");

		/// <summary>
		/// The <c>SEVERE</c> level designates very severe error events. Critical condition, critical.
		/// </summary>
		public readonly static Level SEVERE = new Level(80000, "SEVERE");

		/// <summary>
		/// The <c>ERROR</c> level designates error events that might still allow the application to continue running.
		/// </summary>
		public readonly static Level ERROR = new Level(70000, "ERROR");

		/// <summary>
		/// The <c>WARN</c> level designates potentially harmful situations.
		/// </summary>
		public readonly static Level WARN  = new Level(60000, "WARN");

		/// <summary>
		/// The <c>NOTICE</c> level designates informational messages that highlight the progress of the application at the highest level.
		/// </summary>
		public readonly static Level NOTICE  = new Level(50000, "NOTICE");

		/// <summary>
		/// The <c>INFO</c> level designates informational messages that highlight the progress of the application at coarse-grained level.
		/// </summary>
		public readonly static Level INFO  = new Level(40000, "INFO");

		/// <summary>
		/// The <c>DEBUG</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level DEBUG = new Level(30000, "DEBUG");

		/// <summary>
		/// The <c>FINE</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level FINE = new Level(30000, "FINE");

		/// <summary>
		/// The <c>TRACE</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level TRACE = new Level(20000, "TRACE");

		/// <summary>
		/// The <c>FINER</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level FINER = new Level(20000, "FINER");

		/// <summary>
		/// The <c>VERBOSE</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level VERBOSE = new Level(10000, "VERBOSE");

		/// <summary>
		/// The <c>FINEST</c> level designates fine-grained informational events that are most useful to debug an application.
		/// </summary>
		public readonly static Level FINEST = new Level(10000, "FINEST");

		/// <summary>
		/// The <c>ALL</c> level designates the lowest level possible.
		/// </summary>
		public readonly static Level ALL = new Level(int.MinValue, "ALL");

		#endregion

		#region Member Variables

		private int m_level;
		private string m_levelStr;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate a level object.
		/// </summary>
		/// <param name="level">integer value for this level, higher values represent more severe levels</param>
		/// <param name="levelName">the string name of this level</param>
		public Level(int level, string levelName) 
		{
			m_level = level;
			m_levelStr = string.Intern(levelName);
		}

		#endregion

		#region Override implementation of Object

		/// <summary>
		/// Returns the string representation of this level.
		/// </summary>
		/// <returns></returns>
		override public string ToString() 
		{
			return m_levelStr;
		}

		/// <summary>
		/// Override Equals to compare the levels of
		/// Level objects. Defers to base class if
		/// the target object is not a Level.
		/// </summary>
		/// <param name="o">The object to compare against</param>
		/// <returns>true if the objects are equal</returns>
		override public bool Equals(object o)
		{
			if (o != null && o is Level)
			{
				return m_level == ((Level)o).m_level;
			}
			else
			{
				return base.Equals(o);
			}
		}

		/// <summary>
		/// Returns a hash code that is suitable for use in a hashtree etc
		/// </summary>
		/// <returns>the hash of this object</returns>
		override public int GetHashCode()
		{
			return m_level;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Operator greater than that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is greater than the right hand side</returns>
		public static bool operator > (Level l, Level r)
		{
			return l.m_level > r.m_level;
		}

		/// <summary>
		/// Operator less than that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is less than the right hand side</returns>
		public static bool operator < (Level l, Level r)
		{
			return l.m_level < r.m_level;
		}

		/// <summary>
		/// Operator greater than or equal that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is greater than or equal to the right hand side</returns>
		public static bool operator >= (Level l, Level r)
		{
			return l.m_level >= r.m_level;
		}

		/// <summary>
		/// Operator less than or equal that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is less than or equal to the right hand side</returns>
		public static bool operator <= (Level l, Level r)
		{
			return l.m_level <= r.m_level;
		}

		/// <summary>
		/// Operator equals that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is equal to the right hand side</returns>
		public static bool operator == (Level l, Level r)
		{
			if (((object)l) != null && ((object)r) != null)
			{
				return l.m_level == r.m_level;
			}
			else
			{
				return ((object)l) == ((object)r);
			}
		}

		/// <summary>
		/// Operator not equals that compares Levels
		/// </summary>
		/// <param name="l">left hand side</param>
		/// <param name="r">right hand side</param>
		/// <returns>true if left hand side is not equal to the right hand side</returns>
		public static bool operator != (Level l, Level r)
		{
			return !(l==r);
		}

		#endregion

		/// <summary>
		/// The name of this level
		/// </summary>
		/// <value>
		/// The name of this level
		/// </value>
		/// <remarks>
		/// The name of this level. Readonly.
		/// </remarks>
		public string Name
		{
			get { return m_levelStr; }
		}

		/// <summary>
		/// The Value of this level
		/// </summary>
		/// <value>
		/// The Value of this level
		/// </value>
		/// <remarks>
		/// The Value of this level. Readonly.
		/// </remarks>
		public int Value
		{
			get { return m_level; }
		}

		/// <summary>
		/// Compares two specified <see cref="Level"/> values.
		/// </summary>
		/// <param name="l">A <see cref="Level"/></param>
		/// <param name="r">A <see cref="Level"/></param>
		/// <returns>A signed number indicating the relative values of <c>l</c> and <c>r</c>.</returns>
		/// <remarks>
		/// Less than zero: <c>l</c> is less than <c>r</c>. 
		/// Zero: <c>l</c> and <c>r</c> are equal. 
		/// Greater than zero: <c>l</c> is greater than <c>r</c>. 
		/// </remarks>
		public static int Compare(Level l, Level r)
		{
			if (l == null && r == null)
			{
				return 0;
			}
			if (l == null)
			{
				return -1;
			}
			if (r == null)
			{
				return 1;
			}

			return l.m_level - r.m_level;
		}

		/// <summary>
		/// Compares this instance to a specified <see cref="Object"/>
		/// </summary>
		/// <param name="r">An <see cref="Object"/> or a null reference</param>
		/// <returns>A signed number indicating the relative values of this instance and <c>r</c>.</returns>
		/// <remarks>
		/// Less than zero: this instance is less than <c>r</c>. 
		/// Zero: this instance and <c>r</c> are equal. 
		/// Greater than zero: this instance is greater than <c>r</c>. 
		/// Any instance of <see cref="Level"/>, regardless of its value, 
		/// is considered greater than a null reference.
		/// </remarks>
		public int CompareTo(object r)
		{
			if (r is Level)
			{
				return Compare(this, (Level)r);
			}
			throw new ArgumentOutOfRangeException("Parameter: r, Value: ["+r+"] out of range. Expected instance of Level");
		}
	}
}
