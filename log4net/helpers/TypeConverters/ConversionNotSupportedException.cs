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
using System.Runtime.Serialization;

namespace log4net.helpers.TypeConverters
{
	/// <summary>
	/// Exception base type for conversion errors.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This type extends <see cref="ApplicationException"/>. It
	/// does not add any new functionality but does differentiate the
	/// type of exception being thrown.
	/// </para>
	/// </remarks>
#if !NETCF
	[Serializable]
#endif
	public class ConversionNotSupportedException : ApplicationException 
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </summary>
		public ConversionNotSupportedException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class
		/// with the specified message.
		/// </summary>
		/// <param name="message">A message to include with the exception.</param>
		public ConversionNotSupportedException(String message) : base(message) 
		{
		}

		
		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class
		/// with the specified message and inner exception.
		/// </summary>
		/// <param name="message">A message to include with the exception.</param>
		/// <param name="innerException">A nested exception to include.</param>
		public ConversionNotSupportedException(String message, Exception innerException) : base(message, innerException) 
		{
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

#if !NETCF
		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class 
		/// with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected ConversionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) 
		{
		}
#endif

		#endregion Protected Instance Constructors

		#region  Public Static Methods

		/// <summary>
		/// Creates a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </summary>
		/// <param name="destinationType">The conversion destination type.</param>
		/// <param name="sourceValue">The value to convert.</param>
		/// <returns>An instance of the <see cref="ConversionNotSupportedException" />.</returns>
		public static ConversionNotSupportedException Create(Type destinationType, object sourceValue)
		{
			if (sourceValue == null)
			{
				return new ConversionNotSupportedException("Cannot convert value [null] to type ["+destinationType+"]");
			}
			else
			{
				return new ConversionNotSupportedException("Cannot convert from type ["+sourceValue.GetType()+"] value ["+sourceValue+"] to type ["+destinationType+"]");
			}
		}

		#endregion  Public Static Methods
	}
}
