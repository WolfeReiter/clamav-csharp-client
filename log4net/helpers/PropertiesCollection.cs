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
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace log4net.helpers
{
	/// <summary>
	/// String keyed object map.
	/// </summary>
	/// <remarks>
	/// Only member objects that are serializable will
	/// be serialized allong with this collection.
	/// </remarks>
#if NETCF
	public class PropertiesCollection
#else
	[Serializable] public sealed class PropertiesCollection : ISerializable
#endif
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesCollection" /> class.
		/// </summary>
		public PropertiesCollection()
		{
		}

		#endregion Public Instance Constructors

		#region Private Instance Constructors

#if !NETCF
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesCollection" /> class 
		/// with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <remarks>
		/// Because this class is sealed the serialization constructor is private.
		/// </remarks>
		private PropertiesCollection(SerializationInfo info, StreamingContext context)
		{
			foreach(SerializationEntry entry in info)
			{
				m_ht[entry.Name] = entry.Value;
			}
		}
#endif

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the key names.
		/// </summary>
		/// <value>An array of key names.</value>
		/// <returns>An array of all the keys.</returns>
		public string[] GetKeys()
		{
			string[] keys = new String[m_ht.Count];
			m_ht.Keys.CopyTo(keys, 0);
			return keys;
		}

		/// <summary>
		/// Gets or sets the value of the  property with the specified key.
		/// </summary>
		/// <value>
		/// The value of the property with the specified key.
		/// </value>
		/// <param name="key">The key of the property to get or set.</param>
		/// <remarks>
		/// The property value will only be serialized if it is serializable.
		/// If it cannot be serialized it will be silently ignored if
		/// a serialization operation is performed.
		/// </remarks>
		public object this[string key]
		{
			get { return m_ht[key]; }
			set { m_ht[key] = value; }
		}

		#endregion Public Instance Properties

		#region Implementation of ISerializable

#if !NETCF
		/// <summary>
		/// Serializes this object into the <see cref="SerializationInfo" /> provided.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination for this serialization.</param>
#if !MONO
		[System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter=true)]
#endif
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach(DictionaryEntry entry in m_ht)
			{
				// If value is serializable then we add it to the list
				if (entry.Value.GetType().IsSerializable)
				{
					info.AddValue(entry.Key as string, entry.Value);
				}
			}
		}
#endif

		#endregion Implementation of ISerializable

		#region Private Instance Fields

		private Hashtable m_ht = new Hashtable();

		#endregion Private Instance Fields
	}
}

