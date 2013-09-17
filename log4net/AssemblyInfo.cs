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

using System.Reflection;
using System.Runtime.CompilerServices;

#if (!SSCLI)
//
// log4net makes use of static methods which cannot be made com visible
//
[assembly: System.Runtime.InteropServices.ComVisible(false)]
#endif

//
// log4net is CLS compliant
//
[assembly: System.CLSCompliant(true)]

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//

#if (NET_1_0)
[assembly: AssemblyTitle("log4net for .NET Framework 1.0")]
#elif (NET_1_1)
[assembly: AssemblyTitle("log4net for .NET Framework 1.1")]
#elif (NETCF_1_0)
[assembly: AssemblyTitle("log4net for .NET Compact Framework 1.0")]
#elif (MONO_1_0)
[assembly: AssemblyTitle("log4net for Mono 1.0")]
#elif (SSCLI_1_0)
[assembly: AssemblyTitle("log4net for Shared Source CLI 1.0")]
#elif (NET)
[assembly: AssemblyTitle("log4net for .NET Framework")]
#elif (NETCF)
[assembly: AssemblyTitle("log4net for .NET Compact Framework")]
#elif (MONO)
[assembly: AssemblyTitle("log4net for Mono")]
#elif (SSCLI)
[assembly: AssemblyTitle("log4net for Shared Source CLI")]
#else
[assembly: AssemblyTitle("log4net")]
#endif

[assembly: AssemblyDescription("Logging Framework")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Neoworks Limited")]
[assembly: AssemblyProduct("log4net")]
[assembly: AssemblyCopyright("Copyright (C) 2001-2003 Neoworks Limited. All Rights Reserved.")]
[assembly: AssemblyTrademark("Copyright (C) 2001-2003 Neoworks Limited. All Rights Reserved.")]
[assembly: AssemblyDefaultAlias("log4net")]
[assembly: AssemblyCulture("")]		
		
//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
#if STRONG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\log4net.snk")]
#endif
// We do not use a CSP key for strong naming
// [assembly: AssemblyKeyName("")]

