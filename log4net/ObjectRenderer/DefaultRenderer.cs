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

using log4net.helpers;

namespace log4net.ObjectRenderer
{
	/// <summary>
	/// The default Renderer renders objects by calling their <see cref="Object.ToString"/> method.
	/// </summary>
	/// <remarks>
	/// <para>The default renderer supports rendering objects to strings as follows:</para>
	/// 
	/// <list type="table">
	///		<listheader>
	///			<term>Value</term>
	///			<description>Rendered String</description>
	///		</listheader>
	///		<item>
	///			<term><c>null</c></term>
	///			<description><para>"(null)"</para></description>
	///		</item>
	///		<item>
	///			<term><see cref="Array"/></term>
	///			<description>
	///			<para>For a one dimensional array this is the
	///			array type name, an open brace, followed by a comma
	///			separated list of the elements (using the appropriate
	///			renderer), followed by a close brace. For example:
	///			<c>int[] {1, 2, 3}</c>.</para>
	///			<para>If the array is not one dimensional the 
	///			<c>Array.ToString()</c> is returned.</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="Exception"/></term>
	///			<description>
	///			<para>Renders the exception type, message
	///			and stack trace. Any nested exception is also rendered.</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>other</term>
	///			<description>
	///			<para><c>Object.ToString()</c></para>
	///			</description>
	///		</item>
	/// </list>
	/// 
	/// <para>The <see cref="DefaultRenderer"/> serves as a good base class 
	/// for renderers that need to provide special handling of exception
	/// types. The <see cref="RenderException"/> method is used to render
	/// the exception and its nested exceptions, however the <see cref="RenderExceptionMessage"/>
	/// method is called just to render the exceptions message. This method
	/// can be overridden is a subclass to provide additional information
	/// for some exception types. See <see cref="RenderException"/> for
	/// more information.</para>
	/// </remarks>
	public class DefaultRenderer : IObjectRenderer
	{
		private static readonly string NewLine = SystemInfo.NewLine;

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// Default constructor
		/// </remarks>
		public DefaultRenderer()
		{
		}

		#endregion

		#region Implementation of IObjectRenderer

		/// <summary>
		/// Render the object <paramref name="obj"/> to a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="obj">The object to render</param>
		/// <returns>the object rendered as a string</returns>
		/// <remarks>
		/// <para>Render the object <paramref name="obj"/> to a 
		/// string.</para>
		/// 
		/// <para>The <paramref name="rendererMap"/> parameter is
		/// provided to lookup and render other objects. This is
		/// very useful where <paramref name="obj"/> contains
		/// nested objects of unknown type. The <see cref="RendererMap.FindAndRender"/>
		/// method can be used to render these objects.</para>
		/// 
		/// <para>The default renderer supports rendering objects to strings as follows:</para>
		/// 
		/// <list type="table">
		///		<listheader>
		///			<term>Value</term>
		///			<description>Rendered String</description>
		///		</listheader>
		///		<item>
		///			<term><c>null</c></term>
		///			<description>
		///			<para>"(null)"</para>
		///			</description>
		///		</item>
		///		<item>
		///			<term><see cref="Array"/></term>
		///			<description>
		///			<para>For a one dimensional array this is the
		///			array type name, an open brace, followed by a comma
		///			separated list of the elements (using the appropriate
		///			renderer), followed by a close brace. For example:
		///			<c>int[] {1, 2, 3}</c>.</para>
		///			<para>If the array is not one dimensional the 
		///			<c>Array.ToString()</c> is returned.</para>
		///			
		///			<para>The <see cref="RenderArray"/> method is called
		///			to do the actual array rendering. This method can be
		///			overridden in a subclass to provide different array
		///			rendering.</para>
		///			</description>
		///		</item>
		///		<item>
		///			<term><see cref="Exception"/></term>
		///			<description>
		///			<para>Renders the exception type, message
		///			and stack trace. Any nested exception is also rendered.</para>
		///			
		///			<para>The <see cref="RenderException"/> method is called
		///			to do the actual exception rendering. This method can be
		///			overridden in a subclass to provide different exception
		///			rendering.</para>
		///			</description>
		///		</item>
		///		<item>
		///			<term>other</term>
		///			<description>
		///			<para><c>Object.ToString()</c></para>
		///			</description>
		///		</item>
		/// </list>
		/// </remarks>
		virtual public string DoRender(RendererMap rendererMap, object obj) 
		{
			if (rendererMap == null)
			{
				throw new ArgumentNullException("rendererMap");
			}

			if (obj == null)
			{
				return "(null)";
			}

			if (obj is Array)
			{
				return RenderArray(rendererMap, (Array)obj);
			}
			else if (obj is Exception)
			{
				return RenderException(rendererMap, (Exception)obj);
			}
			else
			{
				return obj.ToString();
			}
		}

		#endregion

		/// <summary>
		/// Render the array argument into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="array">the array to render</param>
		/// <returns>the string representation of the array</returns>
		/// <remarks>
		/// <para>For a one dimensional array this is the
		///	array type name, an open brace, followed by a comma
		///	separated list of the elements (using the appropriate
		///	renderer), followed by a close brace. For example:
		///	<c>int[] {1, 2, 3}</c>.</para>
		///	<para>If the array is not one dimensional the 
		///	<c>Array.ToString()</c> is returned.</para>
		/// </remarks>
		virtual protected string RenderArray(RendererMap rendererMap, Array array)
		{
			if (array.Rank != 1)
			{
				return array.ToString();
			}
			else
			{
				StringBuilder buffer = new StringBuilder(array.GetType().Name + " {");
				int len = array.Length;

				if (len > 0)
				{
					buffer.Append(rendererMap.FindAndRender(array.GetValue(0)));
					for(int i=1; i<len; i++)
					{
						buffer.Append(", ").Append(rendererMap.FindAndRender(array.GetValue(i)));
					}
				}
				return buffer.Append("}").ToString();
			}
		}

		/// <summary>
		/// Render the exception into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="ex">the exception to render</param>
		/// <returns>the string representation of the exception</returns>
		/// <remarks>
		/// <para>Renders the exception type, message, and stack trace. Any nested
		/// exceptions are also rendered.</para>
		/// 
		/// <para>The <see cref="RenderExceptionMessage(RendererMap,Exception)"/>
		/// method is called to render the Exception's message into a string. This method
		/// can be overridden to change the behaviour when rendering
		/// exceptions. To change or extend only the message that is
		/// displayed override the <see cref="RenderExceptionMessage(RendererMap,Exception)"/>
		/// method instead.</para>
		/// </remarks>
		virtual protected string RenderException(RendererMap rendererMap, Exception ex)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("Exception: ")
				.Append(ex.GetType().FullName)
				.Append(NewLine)
				.Append("Message: ")
				.Append(RenderExceptionMessage(rendererMap, ex))
				.Append(NewLine);

#if !NETCF
			if (ex.Source != null && ex.Source.Length > 0)
			{
				sb.Append("Source: ").Append(ex.Source).Append(NewLine);
			}
			if (ex.StackTrace != null && ex.StackTrace.Length > 0)
			{
				sb.Append(ex.StackTrace).Append(NewLine);
			}
#endif
			if (ex.InnerException != null)
			{
				sb.Append(NewLine)
					.Append("Nested Exception")
					.Append(NewLine)
					.Append(NewLine)
					.Append(RenderException(rendererMap, ex.InnerException))
					.Append(NewLine);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Render the exception message into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="ex">the exception to get the message from and render</param>
		/// <returns>the string representation of the exception message</returns>
		/// <remarks>
		/// <para>This method is called to render the exception's message into
		/// a string. This method should be overridden to extend the information
		/// that is rendered for a specific exception.</para>
		/// 
		/// <para>See <see cref="RenderException"/> for more information.</para>
		/// </remarks>
		virtual protected string RenderExceptionMessage(RendererMap rendererMap, Exception ex)
		{
			return ex.Message;
		}

	}
}
