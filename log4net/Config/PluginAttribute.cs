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
using System.Globalization;
using System.Reflection;

using log4net.spi;
using log4net.helpers;
using log4net.Plugin;

namespace log4net.Config
{
	/// <summary>
	/// Assembly level attribute that specifies a plugin to attach to 
	/// the repository.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly,AllowMultiple=true)]
	[Serializable]
	public sealed class PluginAttribute : Attribute, IPluginFactory
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PluginAttribute" /> class
		/// with the specified type.
		/// </summary>
		/// <param name="type">The type of plugin to create.</param>
		public PluginAttribute(string type)
		{
			m_type = type;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the type name for the plugin.
		/// </summary>
		/// <value>
		/// The type name for the plugin.
		/// </value>
		public string Type
		{
			get { return m_type; }
			set { m_type = value ; }
		}

		#endregion Public Instance Properties

		#region Implementation of IPluginFactory

		/// <summary>
		/// Creates the plugin object defined by this attribute.
		/// </summary>
		/// <remarks>
		/// Creates the instance of the <see cref="IPlugin"/> object as 
		/// specified by this attribute.
		/// </remarks>
		/// <returns>The plugin object.</returns>
		public IPlugin CreatePlugin()
		{
			// Get the plugin object type
			System.Type pluginType = SystemInfo.GetTypeFromString(this.Type, true, true);

			// Check that the type is a plugin
			if (!(typeof(IPlugin).IsAssignableFrom(pluginType)))
			{
				throw new LogException("Plugin type [" + this.Type + "] does not implement log4net.IPlugin interface");
			}

			// Create an instance of the plugin using the default constructor
			IPlugin plugin = (IPlugin) pluginType.GetConstructor(SystemInfo.EmptyTypes).Invoke(BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture);

			return plugin;
		}

		#endregion Implementation of IPluginFactory

		#region Override implementation of Object

		/// <summary>
		/// Returns a representation of the properties of this object.
		/// </summary>
		/// <remarks>
		/// Overrides base class <see cref="Object.ToString()" /> method to 
		/// return a representation of the properties of this object.
		/// </remarks>
		/// <returns>A representation of the properties of this object</returns>
		override public string ToString()
		{
			return "PluginAttribute[Type=" + this.Type + "]";
		}

		#endregion Override implementation of Object

		#region Private Instance Fields

		private string m_type = null;

		#endregion Private Instance Fields
	}
}
