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
using System.Globalization;
using System.Reflection;
using System.Xml;

using log4net.Appender;
using log4net.Layout;
using log4net.Filter;
using log4net.helpers;
using log4net.spi;
using log4net.ObjectRenderer;

namespace log4net.Repository.Hierarchy
{
	/// <summary>
	/// Initializes the log4net environment using a DOM tree.
	/// </summary>
	/// <remarks>
	/// Configures a <see cref="Hierarchy"/> using an XML DOM tree.
	/// </remarks>
	public class DOMHierarchyConfigurator
	{
		private enum ConfigUpdateMode {Merge, Overwrite};

		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DOMHierarchyConfigurator" /> class
		/// with the specified <see cref="Hierarchy" />.
		/// </summary>
		/// <param name="hierarchy">The hierarchy to build.</param>
		public DOMHierarchyConfigurator(Hierarchy hierarchy) 
		{
			m_hierarchy = hierarchy;
			m_appenderBag = new Hashtable();
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods

		/// <summary>
		/// Configures the log4net framework by parsing a DOM tree of XML elements.
		/// </summary>
		/// <param name="element">The root element to parse.</param>
		public void Configure(XmlElement element) 
		{
			if (element == null || m_hierarchy == null)
			{
				return;
			}

			string rootElementName = element.LocalName;

			if (rootElementName != CONFIGURATION_TAG)
			{
				LogLog.Error("DOMConfigurator: DOM element is - not a <" + CONFIGURATION_TAG + "> element.");
				return;
			}

			if (!LogLog.InternalDebugging)
			{
				// Look for a debug attribute to enable internal debug
				string debugAttrib = element.GetAttribute(INTERNAL_DEBUG_ATTR);
				LogLog.Debug("DOMConfigurator: "+INTERNAL_DEBUG_ATTR+" attribute [" + debugAttrib + "].");

				if (debugAttrib.Length>0 && debugAttrib != "null") 
				{	  
					LogLog.InternalDebugging = OptionConverter.ToBoolean(debugAttrib, true);
				}
				else 
				{
					LogLog.Debug("DOMConfigurator: Ignoring " + INTERNAL_DEBUG_ATTR + " attribute.");
				}

				string confDebug = element.GetAttribute(CONFIG_DEBUG_ATTR);
				if (confDebug.Length>0 && confDebug != "null")
				{	  
					LogLog.Warn("DOMConfigurator: The \"" + CONFIG_DEBUG_ATTR + "\" attribute is deprecated.");
					LogLog.Warn("DOMConfigurator: Use the \"" + INTERNAL_DEBUG_ATTR + "\" attribute instead.");
					LogLog.InternalDebugging = OptionConverter.ToBoolean(confDebug, true);
				}
			}

			// Default mode is merge
			ConfigUpdateMode configUpdateMode = ConfigUpdateMode.Merge;

			// Look for the config update attribute
			string configUpdateModeAttrib = element.GetAttribute(CONFIG_UPDATE_MODE_ATTR);
			if (configUpdateModeAttrib != null && configUpdateModeAttrib.Length > 0)
			{
				// Parse the attribute
				try
				{
					configUpdateMode = (ConfigUpdateMode)OptionConverter.ConvertStringTo(typeof(ConfigUpdateMode), configUpdateModeAttrib);
				}
				catch
				{
					LogLog.Error("DOMConfigurator: Invalid " + CONFIG_UPDATE_MODE_ATTR + " attribute value [" + configUpdateModeAttrib + "]");
				}
			}

#if (!NETCF)
			LogLog.Debug("DOMConfigurator: Configuration update mode [" + configUpdateMode.ToString(CultureInfo.InvariantCulture) + "].");
#else
			LogLog.Debug("DOMConfigurator: Configuration update mode [" + configUpdateMode.ToString() + "].");
#endif

			// Only reset configuration if overwrite flag specified
			if (configUpdateMode == ConfigUpdateMode.Overwrite)
			{
				// Reset to original unset configuration
				m_hierarchy.ResetConfiguration();
				LogLog.Debug("DOMConfigurator: Configuration reset before reading config.");
			}

			/* Building Appender objects, placing them in a local namespace
			   for future reference */

			/* Process all the top level elements */

			foreach (XmlNode currentNode in element.ChildNodes)
			{
				if (currentNode.NodeType == XmlNodeType.Element) 
				{
					XmlElement currentElement = (XmlElement)currentNode;

					if (currentElement.LocalName == LOGGER_TAG)
					{
						ParseLogger(currentElement);
					} 
					else if (currentElement.LocalName == CATEGORY_TAG)
					{
						// TODO: deprecated use of category
						ParseLogger(currentElement);
					} 
					else if (currentElement.LocalName == ROOT_TAG)
					{
						ParseRoot(currentElement);
					} 
					else if (currentElement.LocalName == RENDERER_TAG)
					{
						ParseRenderer(currentElement);
					}
					else if (currentElement.LocalName == APPENDER_TAG)
					{
						// We ignore appenders in this pass. They will
						// be found and loaded if they are referenced.
					}
					else
					{
						// Read the param tags and set properties on the hierarchy
						SetParameter(currentElement, m_hierarchy);
					}
				}
			}

			// Lastly set the hierarchy threshold
			string thresholdStr = element.GetAttribute(THRESHOLD_ATTR);
			LogLog.Debug("DOMConfigurator: Hierarchy Threshold [" + thresholdStr + "]");
			if (thresholdStr.Length > 0 && thresholdStr != "null") 
			{
				Level thresholdLevel = (Level) ConvertStringTo(typeof(Level), thresholdStr);
				if (thresholdLevel != null)
				{
					m_hierarchy.Threshold = thresholdLevel;
				}
				else
				{
					LogLog.Warn("DOMConfigurator: Unable to set hierarchy threshold using value [" + thresholdStr + "] (with acceptable conversion types)");
				}
			}

			// Done reading config
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Parse appenders by IDREF.
		/// </summary>
		/// <param name="appenderRef">The appender ref element.</param>
		/// <returns>The instance of the appender that the ref refers to.</returns>
		protected IAppender FindAppenderByReference(XmlElement appenderRef) 
		{	
			string appenderName = appenderRef.GetAttribute(REF_ATTR);

			IAppender appender = (IAppender)m_appenderBag[appenderName];
			if (appender != null) 
			{
				return appender;
			} 
			else 
			{
				// Find the element with that id
				XmlElement element = null;

				if (appenderName != null && appenderName.Length > 0)
				{
					foreach (XmlNode node in appenderRef.OwnerDocument.GetElementsByTagName(APPENDER_TAG))
					{
						if (((XmlElement)node).GetAttribute("name") == appenderName)
						{
							element = (XmlElement)node;
							break;
						}
					}
				}

				if (element == null) 
				{
					LogLog.Error("DOMConfigurator: No appender named [" + appenderName + "] could be found."); 
					return null;
				} 
				else
				{
					appender = ParseAppender(element);
					if (appender != null)
					{
						m_appenderBag[appenderName] = appender;
					}
					return appender;
				}
			} 
		}

		/// <summary>
		/// Parses an appender element.
		/// </summary>
		/// <param name="appenderElement">The appender element.</param>
		/// <returns>The appender instance or <c>null</c> when parsing failed.</returns>
		protected IAppender ParseAppender(XmlElement appenderElement) 
		{
			string appenderName = appenderElement.GetAttribute(NAME_ATTR);
			string typeName = appenderElement.GetAttribute(TYPE_ATTR);

			LogLog.Debug("DOMConfigurator: Loading Appender [" + appenderName + "] type: [" + typeName + "]");
			try 
			{
				IAppender appender = (IAppender)SystemInfo.GetTypeFromString(typeName, true, true).GetConstructor(SystemInfo.EmptyTypes).Invoke(BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture);
				appender.Name = appenderName;

				foreach (XmlNode currentNode in appenderElement.ChildNodes)
				{
					/* We're only interested in Elements */
					if (currentNode.NodeType == XmlNodeType.Element) 
					{
						XmlElement currentElement = (XmlElement)currentNode;

						// Look for the appender ref tag
						if (currentElement.LocalName == APPENDER_REF_TAG)
						{
							string refName = currentElement.GetAttribute(REF_ATTR);
							if (appender is IAppenderAttachable) 
							{
								IAppenderAttachable aa = (IAppenderAttachable) appender;
								LogLog.Debug("DOMConfigurator: Attaching appender named [" + refName + "] to appender named [" + appender.Name + "].");
								IAppender a = FindAppenderByReference(currentElement);
								if (a != null)
								{
									aa.AddAppender(a);
								}
							} 
							else 
							{
								LogLog.Error("DOMConfigurator: Requesting attachment of appender named ["+refName+ "] to appender named [" + appender.Name + "] which does not implement log4net.spi.IAppenderAttachable.");
							}
						}
						else
						{
							// For all other tags we use standard set param method
							SetParameter(currentElement, appender);
						}
					}
				}
				if (appender is IOptionHandler) 
				{
					((IOptionHandler) appender).ActivateOptions();
				}

				LogLog.Debug("DOMConfigurator: Created Appender [" + appenderName + "]");	
				return appender;
			}
				/* Yes, it's ugly.  But all of these exceptions point to the same problem: we can't create an Appender */
			catch (Exception oops) 
			{
				LogLog.Error("DOMConfigurator: Could not create Appender [" + appenderName + "] of type [" + typeName + "]. Reported error follows.", oops);
				return null;
			}
		}

		/// <summary>
		/// Parses a logger element.
		/// </summary>
		/// <param name="loggerElement">The logger element.</param>
		protected void ParseLogger(XmlElement loggerElement) 
		{
			// Create a new log4net.Logger object from the <logger> element.
			string loggerName = loggerElement.GetAttribute(NAME_ATTR);

			LogLog.Debug("DOMConfigurator: Retrieving an instance of log4net.Repository.Logger for logger [" + loggerName + "].");
			Logger log = m_hierarchy.GetLogger(loggerName) as Logger;

			// Setting up a logger needs to be an atomic operation, in order
			// to protect potential log operations while logger
			// configuration is in progress.
			lock(log) 
			{
				bool additivity = OptionConverter.ToBoolean(loggerElement.GetAttribute(ADDITIVITY_ATTR), true);
	
				LogLog.Debug("DOMConfigurator: Setting [" + log.Name + "] additivity to [" + additivity + "].");
				log.Additivity = additivity;
				ParseChildrenOfLoggerElement(loggerElement, log, false);
			}
		}

		/// <summary>
		/// Parses the root logger element.
		/// </summary>
		/// <param name="rootElement">The root element.</param>
		protected  void ParseRoot(XmlElement rootElement) 
		{
			Logger root = m_hierarchy.Root;
			// logger configuration needs to be atomic
			lock(root) 
			{	
				ParseChildrenOfLoggerElement(rootElement, root, true);
			}
		}

		/// <summary>
		/// Parses the children of a logger element.
		/// </summary>
		/// <param name="catElement">The category element.</param>
		/// <param name="log">The logger instance.</param>
		/// <param name="isRoot">Flag to indicate if the logger is the root logger.</param>
		protected void ParseChildrenOfLoggerElement(XmlElement catElement, Logger log, bool isRoot) 
		{
			// Remove all existing appenders from log. They will be
			// reconstructed if need be.
			log.RemoveAllAppenders();

			foreach (XmlNode currentNode in catElement.ChildNodes)
			{
				if (currentNode.NodeType == XmlNodeType.Element) 
				{
					XmlElement currentElement = (XmlElement) currentNode;
	
					if (currentElement.LocalName == APPENDER_REF_TAG)
					{
						IAppender appender = FindAppenderByReference(currentElement);
						string refName =  currentElement.GetAttribute(REF_ATTR);
						if (appender != null)
						{
							LogLog.Debug("DOMConfigurator: Adding appender named [" + refName + "] to logger [" + log.Name + "].");
							log.AddAppender(appender);
						}
						else 
						{
							LogLog.Error("DOMConfigurator: Appender named [" + refName + "] not found.");
						}
					} 
					else if (currentElement.LocalName == LEVEL_TAG || currentElement.LocalName == PRIORITY_TAG) 
					{
						ParseLevel(currentElement, log, isRoot);	
					} 
					else
					{
						SetParameter(currentElement, log);
					}
				}
			}
			if (log is IOptionHandler) 
			{
				((IOptionHandler) log).ActivateOptions();
			}
		}

		/// <summary>
		/// Parses an object renderer.
		/// </summary>
		/// <param name="element">The renderer element.</param>
		protected void ParseRenderer(XmlElement element) 
		{
			string renderingClassName = element.GetAttribute(RENDERING_TYPE_ATTR);
			string renderedClassName = element.GetAttribute(RENDERED_TYPE_ATTR);

			LogLog.Debug("DOMConfigurator: Rendering class [" + renderingClassName + "], Rendered class [" + renderedClassName + "].");
			IObjectRenderer renderer = (IObjectRenderer)OptionConverter.InstantiateByClassName(renderingClassName, typeof(IObjectRenderer), null);
			if (renderer == null) 
			{
				LogLog.Error("DOMConfigurator: Could not instantiate renderer [" + renderingClassName + "].");
				return;
			} 
			else 
			{
				try 
				{
					m_hierarchy.RendererMap.Put(SystemInfo.GetTypeFromString(renderedClassName, true, true), renderer);
				} 
				catch(Exception e) 
				{
					LogLog.Error("DOMConfigurator: Could not find class [" + renderedClassName + "].", e);
				}
			}
		}

		/// <summary>
		/// Parses a level element.
		/// </summary>
		/// <param name="element">The level element.</param>
		/// <param name="log">The logger object to set the level on.</param>
		/// <param name="isRoot">Flag to indicate if the logger is the root logger.</param>
		protected void ParseLevel(XmlElement element, Logger log, bool isRoot) 
		{
			string catName = log.Name;
			if (isRoot) 
			{
				catName = "root";
			}

			string priStr = element.GetAttribute(VALUE_ATTR);
			LogLog.Debug("DOMConfigurator: Logger [" + catName + "] Level string is [" + priStr + "].");
	
			if (INHERITED == priStr) 
			{
				if (isRoot) 
				{
					LogLog.Error("DOMConfigurator: Root level cannot be inherited. Ignoring directive.");
				} 
				else 
				{
					log.Level = null;
				}
			} 
			else 
			{
				log.Level = log.Hierarchy.LevelMap[priStr];
				if (log.Level == null)
				{
					LogLog.Error("DOMConfigurator: Undefined level [" + priStr + "] on Logger [" + log.Name + "].");
				}
			}
			LogLog.Debug("DOMConfigurator: Logger [" + catName + "] level set to [name=\"" + log.Level.Name + "\",value=" + log.Level.Value + "].");	
		}

		/// <summary>
		/// Sets a paramater on an object.
		/// </summary>
		/// <remarks>
		/// The parameter name must correspond to a writable property
		/// on the object. The value of the parameter is a string,
		/// therefore this function will attempt to set a string
		/// property first. If unable to set a string property it
		/// will inspect the property and its argument type. It will
		/// attempt to call a static method called 'Parse' on the
		/// type of the property. This method will take a single
		/// string argument and return a value that can be used to
		/// set the property.
		/// </remarks>
		/// <param name="element">The parameter element.</param>
		/// <param name="target">The object to set the parameter on.</param>
		protected void SetParameter(XmlElement element, object target) 
		{
			// Get the property name
			string name = element.GetAttribute(NAME_ATTR);

			// If the name attribute does not exist then use the name of the element
			if (element.LocalName != PARAM_TAG || name == null || name.Length == 0)
			{
				name = element.LocalName;
			}

			// Look for the property on the target object
			Type targetType = target.GetType();
			Type propertyType = null;

			PropertyInfo propInfo = null;
			MethodInfo methInfo = null;

			// Try to find a writable property
			propInfo = targetType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			if (propInfo != null && propInfo.CanWrite)
			{
				// found a property
				propertyType = propInfo.PropertyType;
			}
			else
			{
				propInfo = null;

				// look for a method with the signature Add<property>(type)

				methInfo = targetType.GetMethod("Add" + name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				if (methInfo != null && methInfo.IsPublic && !methInfo.IsStatic)
				{
					System.Reflection.ParameterInfo[] methParams = methInfo.GetParameters();
					if (methParams.Length == 1)
					{
						propertyType = methParams[0].ParameterType;
					}
					else
					{
						methInfo = null;
					}
				}
				else
				{
					methInfo = null;
				}
			}

			if (propertyType == null)
			{
				LogLog.Error("DOMConfigurator: Cannot find Property [" + name + "] to set object on [" + target.ToString() + "]");
			}
			else
			{
				if (element.GetAttributeNode(VALUE_ATTR) != null)
				{
					string propertyValue = element.GetAttribute(VALUE_ATTR);

					// Fixup embedded non-printable chars
					propertyValue = OptionConverter.ConvertSpecialChars(propertyValue);

#if !NETCF	
					try
					{
						// Expand environment variables in the string.
						propertyValue = OptionConverter.SubstVars(propertyValue, Environment.GetEnvironmentVariables());
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// unrestricted environment permission. If this occurs the expansion 
						// will be skipped with the following warning message.
						LogLog.Debug("DOMConfigurator: Security exception while trying to expand environment variables. Error Ignored. No Expansion.");
					}
#endif
					// Now try to convert the string value to an acceptable type
					// to pass to this property.

					object convertedValue = ConvertStringTo(propertyType, propertyValue);
					if (convertedValue != null)
					{
						if (propInfo != null)
						{
							// Got a converted result
							LogLog.Debug("DOMConfigurator: Setting Property [" + propInfo.Name + "] to " + convertedValue.GetType().Name + " value [" + convertedValue.ToString() + "]");

							// Pass to the property
							propInfo.SetValue(target, convertedValue, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
						}
						else if (methInfo != null)
						{
							// Got a converted result
							LogLog.Debug("DOMConfigurator: Setting Collection Property [" + methInfo.Name + "] to " + convertedValue.GetType().Name + " value [" + convertedValue.ToString() + "]");

							// Pass to the property
							methInfo.Invoke(target, BindingFlags.InvokeMethod, null, new object[] {convertedValue}, CultureInfo.InvariantCulture);
						}
					}
					else
					{
						LogLog.Warn("DOMConfigurator: Unable to set property [" + name + "] on object [" + target + "] using value [" + propertyValue + "] (with acceptable conversion types)");
					}
				}
				else
				{
					// No value specified
					Type defaultObjectType = null;
					if (propertyType.IsClass && !propertyType.IsAbstract)
					{
						defaultObjectType = propertyType;
					}

					object createdObject = CreateObjectFromXml(element, defaultObjectType, propertyType);

					if (createdObject == null)
					{
						LogLog.Error("DOMConfigurator: Failed to create object to set param: "+name);
					}
					else
					{
						if (propInfo != null)
						{
							// Got a converted result
							LogLog.Debug("DOMConfigurator: Setting Property ["+ propInfo.Name +"] to object ["+ createdObject +"]");

							// Pass to the property
							propInfo.SetValue(target, createdObject, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
						}
						else if (methInfo != null)
						{
							// Got a converted result
							LogLog.Debug("DOMConfigurator: Setting Collection Property ["+ methInfo.Name +"] to object ["+ createdObject +"]");

							// Pass to the property
							methInfo.Invoke(target, BindingFlags.InvokeMethod, null, new object[] {createdObject}, CultureInfo.InvariantCulture);
						}
					}
				}
			}
		}

		/// <summary>
		/// Converts a string value to a target type.
		/// </summary>
		/// <param name="type">The type of object to convert the string to.</param>
		/// <param name="value">The string value to use as the value of the object.</param>
		/// <returns>
		/// An object of type <paramref name="type"/> with value <paramref name="value"/> or 
		/// <c>null</c> when the conversion could not be performed.
		/// </returns>
		protected object ConvertStringTo(Type type, string value)
		{
			// Hack to allow use of Level in property
			if (type.IsAssignableFrom(typeof(Level)))
			{
				// Property wants a level
				return m_hierarchy.LevelMap[value];
			}
			return OptionConverter.ConvertStringTo(type, value);
		}

		/// <summary>
		/// Creates an object as specified in XML.
		/// </summary>
		/// <param name="element">The XML element that contains the definition of the object.</param>
		/// <param name="defaultTargetType">The object type to use if not explicitly specified.</param>
		/// <param name="typeConstraint">The type that the returned object must be or must inherit from.</param>
		/// <returns>The object or <c>null</c></returns>
		protected object CreateObjectFromXml(XmlElement element, Type defaultTargetType, Type typeConstraint) 
		{
			Type objectType = null;

			// Get the object type
			string objectTypeString = element.GetAttribute(TYPE_ATTR);
			if (objectTypeString == null || objectTypeString.Length == 0)
			{
				if (defaultTargetType == null)
				{
					LogLog.Error("DOMConfigurator: Object type not specified. Cannot create object.");
					return null;
				}
				else
				{
					// Use the default object type
					objectType = defaultTargetType;
				}
			}
			else
			{
				// Read the explicit object type
				try
				{
					objectType = SystemInfo.GetTypeFromString(objectTypeString, true, true);
				}
				catch(Exception ex)
				{
					LogLog.Error("DOMConfigurator: Failed to find type ["+objectTypeString+"]", ex);
					return null;
				}
			}

			bool requiresConversion = false;

			// Got the object type. Check that it meets the typeConstraint
			if (typeConstraint != null)
			{
				if (!typeConstraint.IsAssignableFrom(objectType))
				{
					// Check if there is an appropriate type converter
					if (OptionConverter.CanConvertTypeTo(objectType, typeConstraint))
					{
						requiresConversion = true;
					}
					else
					{
						LogLog.Error("DOMConfigurator: Object type ["+objectType.FullName+"] is not assignable to type ["+typeConstraint.FullName+"]. There are no acceptable type convertions.");
						return null;
					}
				}
			}

			// Look for the default constructor
			ConstructorInfo constInfo = objectType.GetConstructor(SystemInfo.EmptyTypes);
			if (constInfo == null)
			{
				LogLog.Error("DOMConfigurator: Failed to find default constructor for type [" + objectType.FullName + "]");
				return null;
			}

			// Call the constructor
			object createdObject = constInfo.Invoke(BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture);

			// Set any params on object
			foreach (XmlNode currentNode in element.ChildNodes)
			{
				if (currentNode.NodeType == XmlNodeType.Element) 
				{
					SetParameter((XmlElement)currentNode, createdObject);
				}
			}

			// Check if we need to call ActivateOptions
			if (createdObject is IOptionHandler)
			{
				((IOptionHandler) createdObject).ActivateOptions();
			}

			// Ok object should be initialised

			if (requiresConversion)
			{
				// Convert the object type
				return OptionConverter.ConvertTypeTo(createdObject, typeConstraint);
			}
			else
			{
				// The object is of the correct type
				return createdObject;
			}
		}

		#endregion Protected Instance Methods

		#region Private Static Fields

		// String constants used while parsing the XML data
		private const string CONFIGURATION_TAG			= "log4net";
		private const string RENDERER_TAG				= "renderer";
		private const string APPENDER_TAG 				= "appender";
		private const string APPENDER_REF_TAG 			= "appender-ref";  
		private const string PARAM_TAG					= "param";

		// TODO: Deprecate use of category tags
		private const string CATEGORY_TAG				= "category";
		// TODO: Deprecate use of priority tag
		private const string PRIORITY_TAG				= "priority";

		private const string LOGGER_TAG					= "logger";
		private const string NAME_ATTR					= "name";
		private const string TYPE_ATTR					= "type";
		private const string VALUE_ATTR					= "value";
		private const string ROOT_TAG					= "root";
		private const string LEVEL_TAG					= "level";
		private const string REF_ATTR					= "ref";
		private const string ADDITIVITY_ATTR			= "additivity";  
		private const string THRESHOLD_ATTR				= "threshold";
		private const string CONFIG_DEBUG_ATTR			= "configDebug";
		private const string INTERNAL_DEBUG_ATTR		= "debug";
		private const string CONFIG_UPDATE_MODE_ATTR	= "update";
		private const string RENDERING_TYPE_ATTR		= "renderingClass";
		private const string RENDERED_TYPE_ATTR			= "renderedClass";

		// flag used on the level element
		private const string INHERITED = "inherited";

		#endregion Private Static Fields

		#region Private Instance Fields

		/// <summary>
		/// key: appenderName, value: appender.
		/// </summary>
		private Hashtable m_appenderBag;

		/// <summary>
		/// The Hierarchy being configured.
		/// </summary>
		private readonly Hierarchy m_hierarchy;

		#endregion Private Instance Fields
	}
}
