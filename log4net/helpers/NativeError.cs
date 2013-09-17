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
using System.Runtime.InteropServices;

namespace log4net.helpers 
{
	/// <summary>
	/// Represents a native error code and message.
	/// </summary>
	public class NativeError 
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Create an instance of the <see cref="NativeError" /> class with the specified 
		/// error number and message.
		/// </summary>
		/// <param name="number">The number of the native error.</param>
		/// <param name="message">The message of the native error.</param>
		protected NativeError(int number, string message) 
		{
			m_number = number;
			m_message = message;
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the number of the native error.
		/// </summary>
		/// <value>
		/// The number of the native error.
		/// </value>
		public int Number 
		{
			get { return m_number; }
		}

		/// <summary>
		/// Gets the message of the native error.
		/// </summary>
		/// <value>
		/// The message of the native error.
		/// </value>
		public string Message 
		{
			get { return m_message; }
		}

		#endregion Public Instance Properties

		#region Public Static Methods

		/// <summary>
		/// Create a new instance of the <see cref="NativeError" /> class.
		/// </summary>
		/// <param name="number">the error number for the native error</param>
		/// <returns>
		/// An instance of the <see cref="NativeError" /> class for the specified 
		/// error number.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The message for the specified error number is lookup up using the 
		/// native Win32 <c>FormatMessage</c> function.
		/// </para>
		/// </remarks>
		public static NativeError GetError(int number) 
		{
			return new NativeError(number, NativeError.GetErrorMessage(number));
		}

		/// <summary>
		/// Retrieves the message corresponding with a Win32 message identifier.
		/// </summary>
		/// <param name="messageId">Message identifier for the requested message.</param>
		/// <remarks>
		/// The message will be searched for in system message-table resource(s)
		/// using the native <c>FormatMessage</c> function.
		/// </remarks>
		/// <returns>
		/// The message corresponding with the specified message identifier.
		/// </returns>
		public static string GetErrorMessage(int messageId) 
		{
			// Win32 constants
			int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;	// The function should allocates a buffer large enough to hold the formatted message
			int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;		// Insert sequences in the message definition are to be ignored
			int FORMAT_MESSAGE_FROM_SYSTEM  = 0x00001000;		// The function should search the system message-table resource(s) for the requested message

			string lpMsgBuf = "";				// buffer that will receive the message
			IntPtr ptrlpSource = new IntPtr();	// Location of the message definition, will be ignored
			IntPtr prtArguments = new IntPtr();	// Pointer to array of values to insert, not supported as it requires unsafe code

			if (messageId != 0) 
			{
				// If the function succeeds, the return value is the number of TCHARs stored in the output buffer, excluding the terminating null character
				int messageSize = FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, ref ptrlpSource, messageId, 0, ref lpMsgBuf, 255, prtArguments);

				if (messageSize > 0) 
				{
					// Remove trailing null-terminating characters (\r\n) from the message
					lpMsgBuf = lpMsgBuf.TrimEnd(new char[] {'\r', '\n'});
				}
				else 
				{
					// A message could not be located.
					lpMsgBuf = null;
				}
			} 
			else 
			{
				lpMsgBuf = null;
			}

			return lpMsgBuf;
		}

		#endregion Public Static Methods

		#region Override Object Implementation

		/// <summary>
		/// Return error information string
		/// </summary>
		/// <returns>error information string</returns>
		public override string ToString() 
		{
			return string.Format(CultureInfo.InvariantCulture, "0x{0:x8}", this.Number) + (this.Message != null ? ": " + this.Message : "");
		}

		#endregion Override Object Implementation

		#region Stubs For Native Function Calls

		/// <summary>
		/// Formats a message string.
		/// </summary>
		/// <param name="dwFlags">Formatting options, and how to interpret the <paramref name="lpSource" /> parameter.</param>
		/// <param name="lpSource">Location of the message definition.</param>
		/// <param name="dwMessageId">Message identifier for the requested message.</param>
		/// <param name="dwLanguageId">Languager identifier for the requested message.</param>
		/// <param name="lpBuffer">If <paramref name="dwFlags" /> includes FORMAT_MESSAGE_ALLOCATE_BUFFER, the function allocates a buffer using the <c>LocalAlloc</c> function, and places the pointer to the buffer at the address specified in <paramref name="lpBuffer" />.</param>
		/// <param name="nSize">If the FORMAT_MESSAGE_ALLOCATE_BUFFER flag is not set, this parameter specifies the maximum number of TCHARs that can be stored in the output buffer. If FORMAT_MESSAGE_ALLOCATE_BUFFER is set, this parameter specifies the minimum number of TCHARs to allocate for an output buffer.</param>
		/// <param name="Arguments">Pointer to an array of values that are used as insert values in the formatted message.</param>
		/// <remarks>
		/// <para>
		/// The function requires a message definition as input. The message definition can come from a 
		/// buffer passed into the function. It can come from a message table resource in an 
		/// already-loaded module. Or the caller can ask the function to search the system's message 
		/// table resource(s) for the message definition. The function finds the message definition 
		/// in a message table resource based on a message identifier and a language identifier. 
		/// The function copies the formatted message text to an output buffer, processing any embedded 
		/// insert sequences if requested.
		/// </para>
		/// <para>
		/// To prevent the usage of unsafe code, this stub does not support inserting values in the formatted message.
		/// </para>
		/// </remarks>
		/// <returns>
		/// <para>
		/// If the function succeeds, the return value is the number of TCHARs stored in the output 
		/// buffer, excluding the terminating null character.
		/// </para>
		/// <para>
		/// If the function fails, the return value is zero. To get extended error information, 
		/// call <see cref="Marshal.GetLastWin32Error()" />.
		/// </para>
		/// </returns>
#if NETCF
		[DllImport("CoreDll.dll", SetLastError=true, CharSet=CharSet.Unicode)]
#else
		[DllImport("Kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
#endif
		private static extern int FormatMessage(
			int dwFlags, 
			ref IntPtr lpSource, 
			int dwMessageId,
			int dwLanguageId, 
			ref String lpBuffer, 
			int nSize,
			IntPtr Arguments);

		#endregion Stubs For Native Function Calls

		#region Private Instance Fields

		private int m_number;
		private string m_message;

		#endregion Member Variables
	}
}
