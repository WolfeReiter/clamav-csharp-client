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

using log4net.spi;
using log4net.helpers;

namespace log4net.Layout
{
	/// <summary>
	/// A flexible layout configurable with pattern string.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The goal of this class is to <see cref="PatternLayout.Format"/> a 
	/// <see cref="LoggingEvent"/> and return the results as a string. The results
	/// depend on the <i>conversion pattern</i>.
	/// </para>
	/// <para>
	/// The conversion pattern is closely related to the conversion
	/// pattern of the printf function in C. A conversion pattern is
	/// composed of literal text and format control expressions called
	/// <i>conversion specifiers</i>.
	/// </para>
	/// <para>
	/// <i>You are free to insert any literal text within the conversion
	/// pattern.</i>
	/// </para>
	/// <para>
	/// Each conversion specifier starts with a percent sign (%) and is
	/// followed by optional <i>format modifiers</i> and a <i>conversion
	/// character</i>. The conversion character specifies the type of
	/// data, e.g. logger, level, date, thread name. The format
	/// modifiers control such things as field width, padding, left and
	/// right justification. The following is a simple example.
	/// </para>
	/// <para>
	/// Let the conversion pattern be <b>"%-5p [%t]: %m%n"</b> and assume
	/// that the log4net environment was set to use a PatternLayout. Then the
	/// statements
	/// </para>
	/// <code>
	/// ILog log = LogManager.GetLogger(typeof(TestApp));
	/// log.Debug("Message 1");
	/// log.Warn("Message 2");   
	/// </code>
	/// <para>would yield the output</para>
	/// <code>
	/// DEBUG [main]: Message 1
	/// WARN  [main]: Message 2  
	/// </code>
	/// <para>
	/// Note that there is no explicit separator between text and
	/// conversion specifiers. The pattern parser knows when it has reached
	/// the end of a conversion specifier when it reads a conversion
	/// character. In the example above the conversion specifier
	/// <b>%-5p</b> means the level of the logging event should be left
	/// justified to a width of five characters.
	/// </para>
	/// <para>
	/// The recognized conversion characters are :
	/// </para>
	/// <list type="table">
	///     <listheader>
	///         <term>Conversion Character</term>
	///         <description>Effect</description>
	///     </listheader>
	///     <item>
	///         <term>a</term>
	///         <description>
	///				Used to output the frienly name of the AppDomain where the 
	///				logging event was generated. 
	///         </description>
	///     </item>
	///     <item>
	///         <term>c</term>
	///         <description>
	///             <para>
	///				Used to output the logger of the logging event. The
	/// 			logger conversion specifier can be optionally followed by
	/// 			<i>precision specifier</i>, that is a decimal constant in
	/// 			brackets.
	///             </para>
	/// 			<para>
	/// 			If a precision specifier is given, then only the corresponding
	/// 			number of right most components of the logger name will be
	/// 			printed. By default the logger name is printed in full.
	/// 			</para>
	/// 			<para>
	/// 			For example, for the logger name "a.b.c" the pattern
	/// 			<b>%c{2}</b> will output "b.c".
	/// 			</para>
	///         </description>
	///     </item>
	///     <item>
	///			<term>C</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the fully qualified class name of the caller
	/// 			issuing the logging request. This conversion specifier
	/// 			can be optionally followed by <i>precision specifier</i>, that
	/// 			is a decimal constant in brackets.
	/// 			</para>
	/// 			<para>
	/// 			If a precision specifier is given, then only the corresponding
	/// 			number of right most components of the class name will be
	/// 			printed. By default the class name is output in fully qualified form.
	/// 			</para>
	/// 			<para>
	/// 			For example, for the class name "log4net.Layout.PatternLayout", the
	/// 			pattern <b>%C{1}</b> will output "PatternLayout".
	/// 			</para>
	/// 			<para>
	/// 			<b>WARNING</b> Generating the caller class information is
	/// 			slow. Thus, it's use should be avoided unless execution speed is
	/// 			not an issue.
	/// 			</para>
	///			</description>
	///     </item>
	///     <item>
	///			<term>d</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the date of the logging event. The date conversion 
	/// 			specifier may be followed by a <i>date format specifier</i> enclosed 
	/// 			between braces. For example, <b>%d{HH:mm:ss,fff}</b> or
	/// 			<b>%d{dd MMM yyyy HH:mm:ss,fff}</b>.  If no date format specifier is 
	/// 			given then ISO8601 format is
	/// 			assumed (<see cref="log4net.DateFormatter.ISO8601DateFormatter"/>).
	/// 			</para>
	/// 			<para>
	/// 			The date format specifier admits the same syntax as the
	/// 			time pattern string of the <see cref="DateTime.ToString"/>.
	/// 			</para>
	/// 			<para>
	/// 			For better results it is recommended to use the log4net date
	/// 			formatters. These can be specified using one of the strings
	/// 			"ABSOLUTE", "DATE" and "ISO8601" for specifying 
	/// 			<see cref="log4net.DateFormatter.AbsoluteTimeDateFormatter"/>, 
	/// 			<see cref="log4net.DateFormatter.DateTimeDateFormatter"/> and respectively 
	/// 			<see cref="log4net.DateFormatter.ISO8601DateFormatter"/>. For example, 
	/// 			<b>%d{ISO8601}</b> or <b>%d{ABSOLUTE}</b>.
	/// 			</para>
	/// 			<para>
	/// 			These dedicated date formatters perform significantly
	/// 			better than <see cref="DateTime.ToString(string)"/>.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>F</term>
	///			<description>
	///				<para>
	///				Used to output the file name where the logging request was
	///				issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. It's use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>l</term>
	///			<description>
	/// 			<para>
	/// 			Used to output location information of the caller which generated
	/// 			the logging event.
	/// 			</para>
	/// 			<para>
	/// 			The location information depends on the CLI implementation but
	/// 			usually consists of the fully qualified name of the calling
	/// 			method followed by the callers source the file name and line
	/// 			number between parentheses.
	/// 			</para>
	/// 			<para>
	/// 			The location information can be very useful. However, it's
	/// 			generation is <b>extremely</b> slow. It's use should be avoided
	/// 			unless execution speed is not an issue.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>L</term>
	///			<description>
	///				<para>
	///				Used to output the line number from where the logging request
	///				was issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. It's use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>m</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the application supplied message associated with 
	/// 			the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>M</term>
	///			<description>
	///				<para>
	///				Used to output the method name where the logging request was
	///				issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. It's use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>n</term>
	///			<description>
	/// 			<para>
	/// 			Outputs the platform dependent line separator character or
	/// 			characters.
	/// 			</para>
	/// 			<para>
	/// 			This conversion character offers practically the same
	/// 			performance as using non-portable line separator strings such as
	/// 			"\n", or "\r\n". Thus, it is the preferred way of specifying a
	/// 			line separator.
	/// 			</para> 
	///			</description>
	///		</item>
	///		<item>
	///			<term>p</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the level of the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>P</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the an event specific property. The key to 
	/// 			lookup must be specified within braces and directly following the
	/// 			pattern specifier, e.g. <c>%X{user}</c> would include the value
	/// 			from the property that is keyed by the string 'user'. Each property value
	/// 			that is to be included in the log must be specified separately.
	/// 			Properties are added to events by loggers or appenders. By default
	/// 			no properties are defined.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>r</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the number of milliseconds elapsed since the start
	/// 			of the application until the creation of the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>t</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the name of the thread that generated the
	/// 			logging event. Uses the thread number if no name is available.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>u</term>
	///			<description>
	///				<para>
	///				Used to output the user name for the currently active user
	///				(Principal.Identity.Name).
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller information is
	///				extremely slow. It's use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>W</term>
	///			<description>
	///				<para>
	///				Used to output the WindowsIdentity for the currently
	///				active user.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller WindowsIdentity information is
	///				extremely slow. It's use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>x</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the NDC (nested diagnostic context) associated
	/// 			with the thread that generated the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>X</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the MDC (mapped diagnostic context) associated
	/// 			with the thread that generated the logging event. The key to lookup
	/// 			must be specified within braces and directly following the
	/// 			pattern specifier, e.g. <c>%X{user}</c> would include the value
	/// 			from the MDC that is keyed by the string 'user'. Each MDC value
	/// 			that is to be included in the log must be specified separately.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>%</term>
	///			<description>
	/// 			<para>
	/// 			The sequence %% outputs a single percent sign.
	/// 			</para>
	///			</description>
	///		</item>
	/// </list>
	/// <para>
	/// By default the relevant information is output as is. However,
	/// with the aid of format modifiers it is possible to change the
	/// minimum field width, the maximum field width and justification.
	/// </para>
	/// <para>
	/// The optional format modifier is placed between the percent sign
	/// and the conversion character.
	/// </para>
	/// <para>
	/// The first optional format modifier is the <i>left justification
	/// flag</i> which is just the minus (-) character. Then comes the
	/// optional <i>minimum field width</i> modifier. This is a decimal
	/// constant that represents the minimum number of characters to
	/// output. If the data item requires fewer characters, it is padded on
	/// either the left or the right until the minimum width is
	/// reached. The default is to pad on the left (right justify) but you
	/// can specify right padding with the left justification flag. The
	/// padding character is space. If the data item is larger than the
	/// minimum field width, the field is expanded to accommodate the
	/// data. The value is never truncated.
	/// </para>
	/// <para>
	/// This behaviour can be changed using the <i>maximum field
	/// width</i> modifier which is designated by a period followed by a
	/// decimal constant. If the data item is longer than the maximum
	/// field, then the extra characters are removed from the
	/// <i>beginning</i> of the data item and not from the end. For
	/// example, it the maximum field width is eight and the data item is
	/// ten characters long, then the first two characters of the data item
	/// are dropped. This behaviour deviates from the printf function in C
	/// where truncation is done from the end.
	/// </para>
	/// <para>
	/// Below are various format modifier examples for the logger
	/// conversion specifier.
	/// </para>
	/// <div class="tablediv">
	///		<table class="dtTABLE" cellspacing="0">
	///			<tr>
	///				<th>Format modifier</th>
	///				<th>left justify</th>
	///				<th>minimum width</th>
	///				<th>maximum width</th>
	///				<th>comment</th>
	///			</tr>
	///			<tr>
	///				<td align="center">%20c</td>
	///				<td align="center">false</td>
	///				<td align="center">20</td>
	///				<td align="center">none</td>
	///				<td>
	///					<para>
	///					Left pad with spaces if the logger name is less than 20
	///					characters long.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%-20c</td>
	///				<td align="center">true</td>
	///				<td align="center">20</td>
	///				<td align="center">none</td>
	///				<td>
	///					<para>
	///					Right pad with spaces if the logger 
	///					name is less than 20 characters long.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%.30c</td>
	///				<td align="center">NA</td>
	///				<td align="center">none</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Truncate from the beginning if the logger 
	///					name is longer than 30 characters.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%20.30c</td>
	///				<td align="center">false</td>
	///				<td align="center">20</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Left pad with spaces if the logger name is shorter than 20
	///					characters. However, if logger name is longer than 30 characters,
	///					then truncate from the beginning.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%-20.30c</td>
	///				<td align="center">true</td>
	///				<td align="center">20</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Right pad with spaces if the logger name is shorter than 20
	///					characters. However, if logger name is longer than 30 characters,
	///					then truncate from the beginning.
	///					</para>
	///				</td>
	///			</tr>
	///		</table>
	///	</div>
	/// </remarks>
	/// <example>
	/// This is essentially the TTCC layout
	/// <code><b>%r [%t] %-5p %c %x - %m\n</b></code>
	/// </example>
	/// <example>
	/// Similar to the TTCC layout except that the relative time is
	/// right padded if less than 6 digits, thread name is right padded if
	/// less than 15 characters and truncated if longer and the logger
	/// name is left padded if shorter than 30 characters and truncated if
	/// longer.
	/// <code><b>%-6r [%15.15t] %-5p %30.30c %x - %m\n</b></code>
	/// </example>
	public class PatternLayout : LayoutSkeleton
	{
		#region Constants

		/// <summary>
		/// Default pattern string for log output. 
		/// Currently set to the string <b>"%m%n"</b> 
		/// which just prints the application supplied	message. 
		/// </summary>
		public const string DEFAULT_CONVERSION_PATTERN ="%m%n";

		/// <summary>
		/// A conversion pattern equivalent to the TTCCLayout. Current value is <b>%r [%t] %p %c %x - %m%n</b>.
		/// </summary>
		public const string TTCC_CONVERSION_PATTERN = "%r [%t] %p %c %x - %m%n";

		/// <summary>
		/// Initial buffer size
		/// </summary>
  		protected const int BUF_SIZE = 256;

		/// <summary>
		/// Maximum buffer size before it is recycled
		/// </summary>
		protected const int MAX_CAPACITY = 1024;

		#endregion

		#region Member Variables
  
		/// <summary>
		/// output buffer appended to when Format() is invoked
		/// </summary>
		private StringBuilder m_sbuf = new StringBuilder(BUF_SIZE);
  
		/// <summary>
		/// the pattern
		/// </summary>
		private string m_pattern;
  
		/// <summary>
		/// the head of the pattern converter chain
		/// </summary>
		private PatternConverter m_head;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a PatternLayout using the DEFAULT_LAYOUT_PATTERN
		/// </summary>
		/// <remarks>
		/// The default pattern just produces the application supplied message.
		/// </remarks>
		public PatternLayout() : this(DEFAULT_CONVERSION_PATTERN)
		{
		}

		/// <summary>
		/// Constructs a PatternLayout using the supplied conversion pattern
		/// </summary>
		/// <param name="pattern">the pattern to use</param>
		public PatternLayout(string pattern) 
		{
			m_pattern = pattern;
			m_head = CreatePatternParser((pattern == null) ? DEFAULT_CONVERSION_PATTERN : pattern).Parse();
		}

		#endregion
  
		/// <summary>
		/// The <b>ConversionPattern</b> option. This is the string which
		/// controls formatting and consists of a mix of literal content and
		/// conversion specifiers.
		/// </summary>
		public string ConversionPattern
		{
			get { return m_pattern;	}
			set
			{
				m_pattern = value;
				m_head = CreatePatternParser(m_pattern).Parse();
			}
		}

		/// <summary>
		/// Returns PatternParser used to parse the conversion string. Subclasses
		/// may override this to return a subclass of PatternParser which recognize
		/// custom conversion characters.
		/// </summary>
		/// <param name="pattern">the pattern to parse</param>
		/// <returns></returns>
		virtual protected PatternParser CreatePatternParser(string pattern) 
		{
			return new PatternParser(pattern);
		}
  
		#region Implementation of IOptionHandler

		/// <summary>
		/// Does not do anything as options become effective immediately.
		/// </summary>
		override public void ActivateOptions() 
		{
			// nothing to do.
		}

		#endregion

		#region Override implementation of LayoutSkeleton

		/// <summary>
		/// The PatternLayout does not handle the exception contained within
		/// LoggingEvents. Thus, it returns <c>true</c>.
		/// </summary>
		override public bool IgnoresException
		{
			get { return true; }
		}

		/// <summary>
		/// Produces a formatted string as specified by the conversion pattern.
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>the formatted string</returns>
		override public string Format(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			// Reset working string buffer
			if (m_sbuf.Capacity > MAX_CAPACITY) 
			{
				m_sbuf = new StringBuilder(BUF_SIZE);
			} 
			else 
			{
				m_sbuf.Length = 0;
			}
    
			PatternConverter c = m_head;

			// loop through the chain of pattern converters
			while(c != null) 
			{
				c.Format(m_sbuf, loggingEvent);
				c = c.Next;
			}
			return m_sbuf.ToString();
		}

		#endregion
	}
}
