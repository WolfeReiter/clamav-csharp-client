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

namespace log4net.ObjectRenderer
{
	/// <summary>
	/// Implement this interface in order to render objects as strings
	/// </summary>
	/// <remarks>
	/// <para>Certain types require special case conversion to
	/// string form. This conversion is done by an object renderer.
	/// Object renderers implement the <see cref="IObjectRenderer"/>
	/// interface.</para>
	/// </remarks>
	public interface IObjectRenderer
	{
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
		/// </remarks>
		string DoRender(RendererMap rendererMap, object obj);
	}
}
