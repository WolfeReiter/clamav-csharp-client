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
using System.Collections;

namespace log4net.helpers.TypeConverters
{
	/// <summary>
	/// Register of type converters for specific types.
	/// </summary>
	public class ConverterRegistry
	{
		#region Internal Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConverterRegistry" /> class.
		/// </summary>
		internal ConverterRegistry() 
		{
			// Initialise the type2converter hashtable
			m_type2converter = new Hashtable();
		}

		#endregion Internal Instance Constructors

		#region Static Constructor

		/// <summary>
		/// Static constructor.
		/// </summary>
		/// <remarks>
		/// This constructor defines the intrinsic type converters
		/// </remarks>
		static ConverterRegistry()
		{
			// Create the registry
			s_registry = new ConverterRegistry();

			// Add predefined converters here
			AddConverter(typeof(bool), typeof(BooleanConverter));
			AddConverter(typeof(System.Text.Encoding), typeof(EncodingConverter));
		}

		#endregion Static Constructor

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the tye converter for a specific type.
		/// </summary>
		/// <value>
		/// The type converter for a specific type.
		/// </value>
		public IConvertFrom this[Type destinationType]
		{
			get { return (IConvertFrom) m_type2converter[destinationType]; }
			set { m_type2converter[destinationType] = value; }
		}

		#endregion Public Instance Properties

		#region Public Static Methods

		/// <summary>
		/// Adds a converter for a specific type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <param name="converter">The type converter to use to convert to the destination type.</param>
		public static void AddConverter(Type destinationType, IConvertFrom converter)
		{
			if (destinationType != null && converter != null)
			{
				s_registry[destinationType] = converter;
			}
		}

		/// <summary>
		/// Adds a converter for a specific type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <param name="converterType">The type of the type converter to use to convert to the destination type.</param>
		public static void AddConverter(Type destinationType, Type converterType)
		{
			AddConverter(destinationType, CreateConverterInstance(converterType));
		}

		/// <summary>
		/// Gets the type converter to use to convert values to the destination type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		public static IConvertFrom GetConverter(Type destinationType)
		{
			// TODO: Support inheriting type converters.
			// i.e. getting a type converter for a base of destinationType
			IConvertFrom converter = null;

			// Lookup in the static registry
			converter = s_registry[destinationType];

			if (converter == null)
			{
				// Lookup using attributes
				converter = GetConverterFromAttribute(destinationType);

				if (converter != null)
				{
					// Store in registry
					s_registry[destinationType] = converter;
				}
			}

			return converter;
		}
		
		/// <summary>
		/// Lookups the type converter to use as specified by the attributes on the 
		/// destination type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		private static IConvertFrom GetConverterFromAttribute(Type destinationType)
		{
			// Look for an attribute on the destination type
			object[] attributes = destinationType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
			if (attributes != null && attributes.Length > 0)
			{
				TypeConverterAttribute tcAttr = attributes[0] as TypeConverterAttribute;
				if (tcAttr != null)
				{
					Type converterType = SystemInfo.GetTypeFromString(destinationType, tcAttr.ConverterTypeName, false, true);
					return CreateConverterInstance(converterType);
				}
			}

			// Not found converter using attributes
			return null;
		}

		/// <summary>
		/// Creates the instance of the type converter.
		/// </summary>
		/// <param name="converterType">The type of the type converter.</param>
		/// <remarks>
		/// The type specified for the type converter must implement 
		/// the <see cref="IConvertFrom"/> interface and must have a public
		/// default (no argument) constructor.
		/// </remarks>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.</returns>
		private static IConvertFrom CreateConverterInstance(Type converterType)
		{
			if (converterType != null)
			{
				// Check type is a converter
				if (typeof(IConvertFrom).IsAssignableFrom(converterType))
				{
					// Create the type converter
					ConstructorInfo ci = converterType.GetConstructor(log4net.helpers.SystemInfo.EmptyTypes);
					if (ci != null)
					{
						return ci.Invoke(BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture) as IConvertFrom;
					}
				}
			}
			return null;
		}

		#endregion Public Static Methods

		#region Private Static Fields

		/// <summary>
		/// The singleton registry.
		/// </summary>
		private static ConverterRegistry s_registry;

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// Mapping from <see cref="Type" /> to type converter.
		/// </summary>
		private Hashtable m_type2converter;

		#endregion
	}
}
