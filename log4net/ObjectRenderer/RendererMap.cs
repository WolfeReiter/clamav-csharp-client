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
	/// Map class objects to an <see cref="IObjectRenderer"/>.
	/// </summary>
	public class RendererMap
	{
		#region Member Variables

		private System.Collections.Hashtable m_map;
		private static IObjectRenderer s_defaultRenderer = new DefaultRenderer();

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public RendererMap() 
		{
			m_map = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		}

		#endregion

		/// <summary>
		/// Render <paramref name="obj"/> using the appropriate renderer.
		/// </summary>
		/// <remarks>
		/// <para>Find the appropriate renderer for the type of the
		/// <paramref name="obj"/> parameter. This is accomplished by calling the
		/// <see cref="Get(Type)"/> method. Once a renderer is found, it is
		/// applied on the object <paramref name="obj"/> and the result is returned
		/// as a <see cref="string"/>.</para>
		/// </remarks>
		/// <param name="obj">the object to render to a string</param>
		/// <returns>the string rendering of <paramref name="obj"/></returns>
		public string FindAndRender(object obj) 
		{
			if (obj == null)
			{
				return "(null)";
			}
			else 
			{
				try
				{
					return Get(obj.GetType()).DoRender(this, obj);
				}
				catch(Exception ex)
				{
					// Exception rendering the object
					log4net.helpers.LogLog.Error("RendererMap: Exception while rendering object of type ["+obj.GetType().FullName+"]", ex);

					// return default message
					return "<log4net.Error(Exception rendering object type ["+obj.GetType().FullName+"])>";
				}
			}
		}

		/// <summary>
		/// Gets the renderer for the specified object type
		/// </summary>
		/// <remarks>
		/// <param>Gets the renderer for the specified object type</param>
		/// 
		/// <param>Syntactic sugar method that calls <see cref="Get(Type)"/> 
		/// with the type of the object parameter.</param>
		/// </remarks>
		/// <param name="obj">the object to lookup the renderer for</param>
		/// <returns>the renderer for <paramref name="obj"/></returns>
		public IObjectRenderer Get(Object obj) 
		{
			if (obj == null) 
			{
				return null;
			}
			else
			{
				return Get(obj.GetType());
			}
		}
  
		/// <summary>
		/// Gets the renderer for the specified type
		/// </summary>
		/// <param name="type">the type to lookup the renderer for</param>
		/// <returns>the renderer for the specified type</returns>
		public IObjectRenderer Get(Type type) 
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			IObjectRenderer result = null;

			for(Type cur = type; cur != null; cur = cur.BaseType)
			{
				// Look for the specific type in the map
				result = (IObjectRenderer)m_map[cur];
				if (result != null) 
				{
					break;
				}

				// Search the type's interfaces
				result = SearchInterfaces(cur);
				if (result != null)
				{
					break;
				}
			}

			// if not set then use the default renderer
			if (result == null)
			{
				result = s_defaultRenderer;
			}

			return result;
		}  

		/// <summary>
		/// Internal function to recursively search interfaces
		/// </summary>
		/// <param name="type">the type to lookup the renderer for</param>
		/// <returns>the renderer for the specified type</returns>
		private IObjectRenderer SearchInterfaces(Type type) 
		{
			IObjectRenderer r = (IObjectRenderer)m_map[type];
			if (r != null) 
			{
				return r;
			} 
			else 
			{
				foreach(Type t in type.GetInterfaces())
				{
					r = SearchInterfaces(t);
					if (r != null)
					{
						return r; 
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Get the default renderer instance
		/// </summary>
		/// <returns>the default renderer</returns>
		public IObjectRenderer DefaultRenderer
		{
			get { return s_defaultRenderer; }
		}

		/// <summary>
		/// Clear the map of renderers
		/// </summary>
		public void Clear() 
		{
			m_map.Clear();
		}

		/// <summary>
		/// Register an <see cref="IObjectRenderer"/> for <paramref name="typeToRender"/>. 
		/// </summary>
		/// <param name="typeToRender">the type that will be rendered by <paramref name="renderer"/></param>
		/// <param name="renderer">the renderer for <paramref name="typeToRender"/></param>
		public void Put(Type typeToRender, IObjectRenderer renderer) 
		{
			if (typeToRender == null)
			{
				throw new ArgumentNullException("typeToRender");
			}
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			m_map[typeToRender] = renderer;
		}	
	}
}
