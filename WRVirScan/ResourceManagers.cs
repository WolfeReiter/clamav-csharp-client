using System;
using System.Reflection;
using System.Resources;

namespace WolfeReiter.AntiVirus.ConsoleDemo
{
	/// <summary>
	/// ResourceManagers is a Factory generates ResourceManager instances in a singleton pattern.
	/// </summary>
	internal sealed class ResourceManagers
	{
		private const string BASE_STRINGS = "WolfeReiter.AntiVirus.ConsoleDemo.Strings";
		private static ResourceManager _stringmgr = null;

		private ResourceManagers(){}
		
		/// <summary>
		/// Singleton access to a ResourceManager for strings resources
		/// </summary>
		public static ResourceManager Strings
		{
			get
			{
				if (_stringmgr==null)
					_stringmgr = new ResourceManager( BASE_STRINGS, Assembly.GetExecutingAssembly() );
				return _stringmgr;
			}
		}
	}
}