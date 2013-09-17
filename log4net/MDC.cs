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

namespace log4net
{
	/// <summary>
	/// Implementation of Mapped Diagnostic Contexts.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The MDC class is similar to the <see cref="NDC"/> class except that it is
	/// based on a map instead of a stack. It provides <i>mapped
	/// diagnostic contexts</i>. A <i>Mapped Diagnostic Context</i>, or
	/// MDC in short, is an instrument for distinguishing interleaved log
	/// output from different sources. Log output is typically interleaved
	/// when a server handles multiple clients near-simultaneously.
	/// </para>
	/// <para>
	/// The MDC is managed on a per thread basis.
	/// </para>
	/// </remarks>
	public sealed class MDC
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MDC" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private MDC()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Methods

		/// <summary>
		/// Gets the context identified by the <paramref name="key" /> parameter.
		/// </summary>
		/// <remarks>
		/// If the <paramref name="key" /> parameter does not look up to a
		/// previously defined context then <c>null</c> will be returned.
		/// </remarks>
		/// <param name="key">The key to lookup in the MDC.</param>
		/// <returns>The string value held for the key, or a <c>null</c> reference if no corresponding value is found.</returns>
		public static string Get(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return GetMap()[key] as string;
		}

		/// <summary>
		/// Puts a context value (the <paramref name="val" /> parameter) as identified
		/// with the <paramref name="key" /> parameter into the current thread's
		/// context map.
		/// </summary>
		/// <remarks>
		/// If a value is already defined for the <paramref name="key" />
		/// specified then the value will be replaced.  If the <paramref name="val" /> 
		/// is specified as <c>null</c> then the key value mapping will be removed.
		/// </remarks>
		/// <param name="key">The key to store the value under.</param>
		/// <param name="value">The value to store.</param>
		public static void Set(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (value == null)
			{
				GetMap().Remove(key);
			}
			else
			{
				GetMap()[key] = value;
			}
		}

		/// <summary>
		/// Removes the key value mapping for the key specified.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		public static void Remove(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			Set(key, null);
		}

		/// <summary>
		/// Clear all entries in the MDC
		/// </summary>
		public static void Clear()
		{
			Hashtable map = (Hashtable)System.Threading.Thread.GetData(s_slot);
			if (map != null)
			{
				map.Clear();
			}
		}

		#endregion Public Static Methods

		#region Internal Static Methods

		/// <summary>
		/// Gets the map on this thread.
		/// </summary>
		/// <returns>The map on the current thread.</returns>
		internal static IDictionary GetMap()
		{
			Hashtable map = (Hashtable)System.Threading.Thread.GetData(s_slot);
			if (map == null)
			{
				map = new Hashtable();
				System.Threading.Thread.SetData(s_slot, map);
			}
			return map;
		}

		/// <summary>
		/// Gets a readonly copy of the map on this thread.
		/// </summary>
		/// <returns>A readonly copy of the map on the current thread.</returns>
		internal static IDictionary CopyMap()
		{
			Hashtable map = (Hashtable)System.Threading.Thread.GetData(s_slot);
			if (map == null)
			{
				return log4net.helpers.EmptyDictionary.Instance;
			}

			// Return a copy of the map
			return (IDictionary)map.Clone();
		}

		#endregion Internal Static Methods

		#region Private Static Fields

		/// <summary>
		/// The thread local data slot to use for context information.
		/// </summary>
		private readonly static LocalDataStoreSlot s_slot = System.Threading.Thread.AllocateDataSlot();

		#endregion Private Static Fields
	}
}
