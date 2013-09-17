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
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace log4net.helpers
{
	/// <summary>
	/// Utility class for system specific information.
	/// </summary>
	public sealed class SystemInfo
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private constructor to prevent instances.
		/// </summary>
		private SystemInfo() 
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Properties

		/// <summary>
		/// Gets the system dependent line terminator.
		/// </summary>
		/// <value>
		/// The system dependent line terminator.
		/// </value>
		public static string NewLine
		{
			get
			{
#if NETCF
				return "\r\n";
#else
				return System.Environment.NewLine;
#endif
			}
		}

		/// <summary>
		/// Gets the base directory for this <see cref="AppDomain"/>.
		/// </summary>
		/// <value>The base directory path for the current <see cref="AppDomain"/>.</value>
		public static string ApplicationBaseDirectory
		{
			get 
			{
#if NETCF
				return System.IO.Path.GetDirectoryName(SystemInfo.EntryAssemblyLocation) + System.IO.Path.DirectorySeparatorChar;
#else
				return AppDomain.CurrentDomain.BaseDirectory;
#endif
			}
		}

		/// <summary>
		/// Gets the path to the configuration file for the current <see cref="AppDomain"/>.
		/// </summary>
		/// <value>The path to the configuration file for the current <see cref="AppDomain"/>.</value>
		/// <remarks>
		/// The .NET Compact Framework 1.0 does not have a concept of a configuration
		/// file. For this runtime, we use the entry assembly location as the root for
		/// the configuration file name.
		/// </remarks>
		public static string ConfigurationFileLocation
		{
			get 
			{
#if NETCF
				return SystemInfo.EntryAssemblyLocation+".config";
#else
				return System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
			}
		}

		/// <summary>
		/// Gets the path to the file that first executed in the current <see cref="AppDomain"/>.
		/// </summary>
		/// <value>The path to the entry assembly.</value>
		public static string EntryAssemblyLocation
		{
			get 
			{
#if NETCF
				return SystemInfo.NativeEntryAssemblyLocation;
#else
				return System.Reflection.Assembly.GetEntryAssembly().Location;
#endif
			}
		}

		/// <summary>
		/// Gets the ID of the current thread.
		/// </summary>
		/// <value>The ID of the current thread.</value>
		/// <remarks>
		/// <para>
		/// On the .NET framework, the <c>AppDomain.GetCurrentThreadId</c> method
		/// is used to obtain the thread ID for the current thread. This is the 
		/// operating system ID for the thread.
		/// </para>
		/// <para>
		/// On the .NET Compact Framework 1.0 it is not possible to get the 
		/// operating system thread ID for the current thread. The native method 
		/// <c>GetCurrentThreadId</c> is implemented inline in a header file
		/// and cannot be called.
		/// </para>
		/// </remarks>
		public static int CurrentThreadId
		{
			get 
			{
#if NETCF
				return System.Threading.Thread.CurrentThread.GetHashCode();
#else
				return AppDomain.GetCurrentThreadId();
#endif
			}
		}

		/// <summary>
		/// Get the host name or machine name for the current machine
		/// </summary>
		/// <value>
		/// The host name (<see cref="System.Net.Dns.GetHostName"/>) or
		/// the machine name (<c>Environment.MachineName</c>) for
		/// the current machine, or if neither of these are available
		/// then <c>NOT AVAILABLE</c> is returned.
		/// </value>
		public static string HostName
		{
			get
			{
				if (s_hostName == null)
				{

					// Get the DNS host name of the current machine
					try
					{
						// Lookup the host name
						s_hostName = System.Net.Dns.GetHostName();
					}
					catch
					{
						// We may get a security exception looking up the hostname
						// You must have Unrestricted DnsPermission to access resource
					}

					// Get the NETBIOS machine name of the current machine
					if (s_hostName == null || s_hostName.Length == 0)
					{
						try
						{
#if (!SSCLI && !NETCF)
							s_hostName = Environment.MachineName;
#endif
						}
						catch
						{
							// We may get a security exception looking up the machine name
							// You must have Unrestricted EnvironmentPermission to access resource
						}
					}

					// Couldn't find a value
					if (s_hostName == null || s_hostName.Length == 0)
					{
						s_hostName = "NOT AVAILABLE";
					}
				}
				return s_hostName;
			}
		}

		/// <summary>
		/// Get this application's friendly name
		/// </summary>
		/// <value>
		/// The friendly name of this application as a string
		/// </value>
		/// <remarks>
		/// <para>
		/// If available the name of the application is retrieved from
		/// the <c>AppDomain</c> using <c>AppDomain.CurrentDomain.FriendlyName</c>.
		/// </para>
		/// <para>Otherwise the file name of the entry assembly is used.</para>
		/// </remarks>
		public static string ApplicationFriendlyName
		{
			get
			{
				if (s_appFriendlyName == null)
				{
					try
					{
#if !NETCF
						s_appFriendlyName = AppDomain.CurrentDomain.FriendlyName;
#endif
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// some undefined set of SecurityPermission flags.
						LogLog.Debug("SystemInfo: Security exception while trying to get current domain friendly name. Error Ignored.");
					}

					if (s_appFriendlyName == null || s_appFriendlyName.Length == 0)
					{
						try
						{
							string assemblyLocation = SystemInfo.EntryAssemblyLocation;
							s_appFriendlyName = System.IO.Path.GetFileName(assemblyLocation);
						}
						catch(System.Security.SecurityException)
						{
							// Caller needs path discovery permission
						}
					}

					if (s_appFriendlyName == null || s_appFriendlyName.Length == 0)
					{
						s_appFriendlyName = "NOT AVAILABLE";
					}
				}
				return s_appFriendlyName;
			}
		}

		#endregion Public Static Properties

		#region Public Static Methods

		/// <summary>
		/// Gets the assembly location path for the specified assembly.
		/// </summary>
		/// <param name="myAssembly">The assembly to get the location for.</param>
		/// <remarks>
		/// This method does not guarantee to return the correct path
		/// to the assembly. If only tries to give an indication as to
		/// where the assembly was loaded from.
		/// </remarks>
		/// <returns>The location of the assembly.</returns>
		public static string AssemblyLocationInfo(Assembly myAssembly)
		{
#if NETCF
			return "Not supported on Microsoft .NET Compact Framework";
#else
			if (myAssembly.GlobalAssemblyCache)
			{
				return "Global Assembly Cache";
			}
			else
			{
				try
				{
					// This call requires FileIOPermission for access to the path
					// if we don't have permission then we just ignore it and
					// carry on.
					return myAssembly.Location;
				}
				catch(System.Security.SecurityException)
				{
					return "Location Permission Denied";
				}
			}
#endif
		}

		/// <summary>
		/// Gets the fully qualified name of the <see cref="Type" />, including 
		/// the name of the assembly from which the <see cref="Type" /> was 
		/// loaded.
		/// </summary>
		/// <param name="type">The <see cref="Type" /> to get the fully qualified name for.</param>
		/// <remarks>
		/// This is equivalent to the <c>Type.AssemblyQualifiedName</c> property,
		/// but this method works on the .NET Compact Framework 1.0 as well as
		/// the full .NET runtime.
		/// </remarks>
		/// <returns>The fully qualified name for the <see cref="Type" />.</returns>
		public static string AssemblyQualifiedName(Type type)
		{
			return type.FullName + ", " + type.Assembly.FullName;
		}

		/// <summary>
		/// Gets the short name of the <see cref="Assembly" />.
		/// </summary>
		/// <param name="myAssembly">The <see cref="Assembly" /> to get the name for.</param>
		/// <remarks>
		/// <para>
		/// The short name of the assembly is the <see cref="Assembly.FullName" /> 
		/// without the version, culture, or public key. i.e. it is just the 
		/// assembly's file name without the extension.
		/// </para>
		/// <para>
		/// Use this rather than Assembly.GetName().Name because that
		/// is not available on the Compact Framework.
		/// </para>
		/// <para>
		/// Because of a FileIOPermission security demand we cannot do
		/// the obvious Assembly.GetName().Name. We are allowed to get
		/// the <see cref="Assembly.FullName" /> of the assembly so we 
		/// start from there and strip out just the assembly name.
		/// </para>
		/// </remarks>
		/// <returns>The short name of the <see cref="Assembly" />.</returns>
		public static string AssemblyShortName(Assembly myAssembly)
		{
			string name = myAssembly.FullName;
			int offset = name.IndexOf(',');
			if (offset > 0)
			{
				name = name.Substring(0, offset);
			}
			return name.Trim();

			// TODO: Do we need to unescape the assembly name string? 
			// Doc says '\' is an escape char but has this already been 
			// done by the string loader?
		}

		/// <summary>
		/// Gets the file name portion of the <see cref="Assembly" />, including 
		/// the extension.
		/// </summary>
		/// <param name="myAssembly">The <see cref="Assembly" /> to get the file name for.</param>
		/// <returns>The file name of the assembly.</returns>
		public static string AssemblyFileName(Assembly myAssembly)
		{
#if NETCF
			// This is not very good because it assumes that only
			// the entry assembly can be an EXE. In fact multiple
			// EXEs can be loaded in to a process.

			string assemblyShortName = SystemInfo.AssemblyShortName(myAssembly);
			string entryAssemblyShortName = System.IO.Path.GetFileNameWithoutExtension(SystemInfo.EntryAssemblyLocation);

			if (string.Compare(assemblyShortName, entryAssemblyShortName, true) == 0)
			{
				// assembly is entry assembly
				return assemblyShortName + ".exe";
			}
			else
			{
				// assembly is not entry assembly
				return assemblyShortName + ".dll";
			}
#else
			return System.IO.Path.GetFileName(myAssembly.Location);
#endif
		}

		/// <summary>
		/// Loads the type specified in the type string.
		/// </summary>
		/// <param name="relativeType">A sibling type to use to load the type.</param>
		/// <param name="typeName">The name of the type to load.</param>
		/// <param name="throwOnError">Flag set to <c>true</c> to throw an exception if the type cannot be loaded.</param>
		/// <param name="ignoreCase"><c>true</c> to ignore the case of the type name; otherwise, <c>false</c></param>
		/// <remarks>
		/// <para>
		/// If the type name is fully qualified, i.e. if contains an assembly name in 
		/// the type name, the type will be loaded from the system using 
		/// <see cref="Type.GetType(string,bool)"/>.
		/// </para>
		/// <para>
		/// If the type name is not fully qualified, it will be loaded from the assembly
		/// containing the specified relative type.
		/// </para>
		/// </remarks>
		/// <returns>The type loaded or <c>null</c> if it could not be loaded.</returns>
		public static Type GetTypeFromString(Type relativeType, string typeName, bool throwOnError, bool ignoreCase)
		{
			return GetTypeFromString(relativeType.Assembly, typeName, throwOnError, ignoreCase);
		}

		/// <summary>
		/// Loads the type specified in the type string.
		/// </summary>
		/// <param name="typeName">The name of the type to load.</param>
		/// <param name="throwOnError">Flag set to <c>true</c> to throw an exception if the type cannot be loaded.</param>
		/// <param name="ignoreCase"><c>true</c> to ignore the case of the type name; otherwise, <c>false</c></param>
		/// <remarks>
		/// <para>
		/// If the type name is fully qualified, i.e. if contains an assembly name in 
		/// the type name, the type will be loaded from the system using 
		/// <see cref="Type.GetType(string,bool)"/>.
		/// </para>
		/// <para>
		/// If the type name is not fully qualified it will be loaded from the
		/// assembly that is directly calling this method.
		/// </para>
		/// </remarks>
		/// <returns>The type loaded or <c>null</c> if it could not be loaded.</returns>		
		public static Type GetTypeFromString(string typeName, bool throwOnError, bool ignoreCase)
		{
			return GetTypeFromString(Assembly.GetCallingAssembly(), typeName, throwOnError, ignoreCase);
		}

		/// <summary>
		/// Loads the type specified in the type string.
		/// </summary>
		/// <param name="relativeAssembly">An assembly to load the type from.</param>
		/// <param name="typeName">The name of the type to load.</param>
		/// <param name="throwOnError">Flag set to <c>true</c> to throw an exception if the type cannot be loaded.</param>
		/// <param name="ignoreCase"><c>true</c> to ignore the case of the type name; otherwise, <c>false</c></param>
		/// <remarks>
		/// <para>
		/// If the type name is fully qualified, i.e. if contains an assembly name in 
		/// the type name, the type will be loaded from the system using 
		/// <see cref="Type.GetType(string,bool)"/>.
		/// </para>
		/// <para>
		/// If the type name is not fully qualified it will be loaded from the specified
		/// assembly.
		/// </para>
		/// </remarks>
		/// <returns>The type loaded or <c>null</c> if it could not be loaded.</returns>
		public static Type GetTypeFromString(Assembly relativeAssembly, string typeName, bool throwOnError, bool ignoreCase)
		{
			// Check if the type name specifies the assembly name
			if(typeName.IndexOf(',') == -1)
			{
				//LogLog.Debug("SystemInfo: Loading type ["+typeName+"] from assembly ["+relativeAssembly.FullName+"]");
#if NETCF
				return relativeAssembly.GetType(typeName, throwOnError);
#else
				return relativeAssembly.GetType(typeName, throwOnError, ignoreCase);
#endif
			}
			else
			{
				// Includes assembly name
				//LogLog.Debug("SystemInfo: Loading type ["+typeName+"] from global Type");
#if NETCF
				return Type.GetType(typeName, throwOnError);
#else
				return Type.GetType(typeName, throwOnError, ignoreCase);
#endif
			}
		}


		/// <summary>
		/// Generate a new guid
		/// </summary>
		/// <returns>A new Guid</returns>
		public static Guid NewGuid()
		{
#if NETCF
				return PocketGuid.NewGuid();
#else
				return Guid.NewGuid();
#endif
		}

		#endregion Public Static Methods

		#region Private Static Methods

#if NETCF
		private static string NativeEntryAssemblyLocation 
		{
			get 
			{
				StringBuilder moduleName = null;

				IntPtr moduleHandle = GetModuleHandle(IntPtr.Zero);

				if (moduleHandle != IntPtr.Zero) 
				{
					moduleName = new StringBuilder(255);
					if (GetModuleFileName(moduleHandle, moduleName,	moduleName.Capacity) == 0) 
					{
						NativeError nativeError = NativeError.GetError(Marshal.GetLastWin32Error());
						throw new NotSupportedException(nativeError.ToString());
					}
				} 
				else 
				{
					NativeError nativeError = NativeError.GetError(Marshal.GetLastWin32Error());
					throw new NotSupportedException(nativeError.ToString());
				}

				return moduleName.ToString();
			}
		}

		[DllImport("CoreDll.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		private static extern IntPtr GetModuleHandle(IntPtr ModuleName);

		[DllImport("CoreDll.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		private static extern Int32 GetModuleFileName(
			IntPtr hModule,
			StringBuilder ModuleName,
			Int32 cch);

#endif

		#endregion Private Static Methods

		#region Public Static Fields

		/// <summary>
		/// Gets an empty array of types.
		/// </summary>
		/// <remarks>
		/// The <c>Type.EmptyTypes</c> field is not available on
		/// the .NET Compact Framework 1.0.
		/// </remarks>
		public static readonly Type[] EmptyTypes = new Type[0];

		#endregion Public Static Fields

		#region Private Static Fields

		/// <summary>
		/// Cache the host name for the current machine
		/// </summary>
		private static string s_hostName;

		/// <summary>
		/// Cache the application friendly name
		/// </summary>
		private static string s_appFriendlyName;

		#endregion

		#region Compact Framework Helper Classes
#if NETCF
		/// <summary>
		/// Generate GUIDs on the .NET Compact Framework.
		/// </summary>
		public class PocketGuid
		{
			// guid variant types
			private enum GuidVariant
			{
				ReservedNCS = 0x00,
				Standard = 0x02,
				ReservedMicrosoft = 0x06,
				ReservedFuture = 0x07
			}

			// guid version types
			private enum GuidVersion
			{
				TimeBased = 0x01,
				Reserved = 0x02,
				NameBased = 0x03,
				Random = 0x04
			}
         
			// constants that are used in the class
			private class Const
			{
				// number of bytes in guid
				public const int ByteArraySize = 16;
         
				// multiplex variant info
				public const int VariantByte = 8;
				public const int VariantByteMask = 0x3f;
				public const int VariantByteShift = 6;

				// multiplex version info
				public const int VersionByte = 7;
				public const int VersionByteMask = 0x0f;
				public const int VersionByteShift = 4;
			}

			// imports for the crypto api functions
			private class WinApi
			{
				public const uint PROV_RSA_FULL = 1;
				public const uint CRYPT_VERIFYCONTEXT = 0xf0000000;

				[DllImport("coredll.dll")] 
				public static extern bool CryptAcquireContext(
					ref IntPtr phProv, string pszContainer, string pszProvider,
					uint dwProvType, uint dwFlags);

				[DllImport("coredll.dll")] 
				public static extern bool CryptReleaseContext( 
					IntPtr hProv, uint dwFlags);

				[DllImport("coredll.dll")] 
				public static extern bool CryptGenRandom(
					IntPtr hProv, int dwLen, byte[] pbBuffer);
			}
      
			// all static methods
			private PocketGuid()
			{
			}
      
			/// <summary>
			/// Return a new System.Guid object.
			/// </summary>
			public static Guid NewGuid()
			{
				IntPtr hCryptProv = IntPtr.Zero;
				Guid guid = Guid.Empty;
         
				try
				{
					// holds random bits for guid
					byte[] bits = new byte[Const.ByteArraySize];

					// get crypto provider handle
					if (!WinApi.CryptAcquireContext(ref hCryptProv, null, null, 
						WinApi.PROV_RSA_FULL, WinApi.CRYPT_VERIFYCONTEXT))
					{
						throw new SystemException(
							"Failed to acquire cryptography handle.");
					}
      
					// generate a 128 bit (16 byte) cryptographically random number
					if (!WinApi.CryptGenRandom(hCryptProv, bits.Length, bits))
					{
						throw new SystemException(
							"Failed to generate cryptography random bytes.");
					}
            
					// set the variant
					bits[Const.VariantByte] &= Const.VariantByteMask;
					bits[Const.VariantByte] |= 
						((int)GuidVariant.Standard << Const.VariantByteShift);

					// set the version
					bits[Const.VersionByte] &= Const.VersionByteMask;
					bits[Const.VersionByte] |= 
						((int)GuidVersion.Random << Const.VersionByteShift);
         
					// create the new System.Guid object
					guid = new Guid(bits);
				}
				finally
				{
					// release the crypto provider handle
					if (hCryptProv != IntPtr.Zero)
						WinApi.CryptReleaseContext(hCryptProv, 0);
				}
         
				return guid;
			}
		}
#endif
		#endregion Compact Framework Helper Classes
	}
}
