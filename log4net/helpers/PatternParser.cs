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

using log4net.Layout;
using log4net.spi;
using log4net.DateFormatter;

namespace log4net.helpers
{
	/// <summary>
	/// Most of the work of the <see cref="PatternLayout"/> class
	/// is delegated to the PatternParser class.
	/// </summary>
	public class PatternParser
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternParser" /> class 
		/// with the specified pattern string.
		/// </summary>
		/// <param name="pattern">The pattern to parse.</param>
		public PatternParser(string pattern) 
		{
			m_pattern = pattern;
			m_patternLength =  pattern.Length;	
			m_state = LITERAL_STATE;
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods

		/// <summary>
		/// Parses the pattern into a chain of pattern converters.
		/// </summary>
		/// <returns>The head of a chain of pattern converters.</returns>
		public PatternConverter Parse() 
		{
			char c;
			m_index = 0;
			while(m_index < m_patternLength) 
			{
				c = m_pattern[m_index++];
				switch(m_state) 
				{
					case LITERAL_STATE: 
						// In literal state, the last char is always a literal.
						if (m_index == m_patternLength)
						{
							m_currentLiteral.Append(c);
							continue;
						}
						if (c == ESCAPE_CHAR) 
						{	  
							// peek at the next char. 
							switch(m_pattern[m_index])
							{
								case ESCAPE_CHAR:
									m_currentLiteral.Append(c);
									m_index++; // move pointer
									break;

								case 'n':
									m_currentLiteral.Append(SystemInfo.NewLine);
									m_index++; // move pointer
									break;

								default:
									if (m_currentLiteral.Length != 0)
									{
										AddToList(new LiteralPatternConverter(m_currentLiteral.ToString()));
									}
									m_currentLiteral.Length = 0;
									m_currentLiteral.Append(c); // append %
									m_state = CONVERTER_STATE;
									m_formattingInfo.Reset();
									break;
							}
						}
						else
						{
							m_currentLiteral.Append(c);
						}
						break;

					case CONVERTER_STATE:
						m_currentLiteral.Append(c);
					switch(c) 
					{
						case '-':
							m_formattingInfo.LeftAlign = true;
							break;

						case '.':
							m_state = DOT_STATE;
							break;

						default:
							if (c >= '0' && c <= '9') 
							{
								m_formattingInfo.Min = c - '0';
								m_state = MIN_STATE;
							}
							else 
							{
								FinalizeConverter(c);		
							}
							break;
					} // switch
						break;

					case MIN_STATE:
						m_currentLiteral.Append(c);
						if (c >= '0' && c <= '9') 
						{
							m_formattingInfo.Min = (m_formattingInfo.Min * 10) + (c - '0');
						}
						else if (c == '.')
						{
							m_state = DOT_STATE;
						}
						else 
						{
							FinalizeConverter(c);
						}
						break;

					case DOT_STATE:
						m_currentLiteral.Append(c);
						if (c >= '0' && c <= '9') 
						{
							m_formattingInfo.Max = c - '0';
							m_state = MAX_STATE;
						}
						else 
						{
							LogLog.Error("PatternParser: Error occurred in position ["+m_index+"].\n Was expecting digit, instead got char ["+c+"].");
							m_state = LITERAL_STATE;
						}
						break;

					case MAX_STATE:
						m_currentLiteral.Append(c);
						if (c >= '0' && c <= '9') 
						{
							m_formattingInfo.Max = (m_formattingInfo.Max * 10) + (c - '0');
						}
						else 
						{
							FinalizeConverter(c);
							m_state = LITERAL_STATE;
						}
						break;
				} // switch
			} // while

			if (m_currentLiteral.Length != 0) 
			{
				AddToList(new LiteralPatternConverter(m_currentLiteral.ToString()));
			}
			return m_head;
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Extracts the option from the pattern at the current index.
		/// </summary>
		/// <remarks>
		/// The option is the section of the pattern between '{' and '}'.
		/// </remarks>
		/// <returns>
		/// The option if the current index of the parse is at the start 
		/// of the option, otherwise <c>null</c>.
		/// </returns>
		protected string ExtractOption() 
		{
			if ((m_index < m_patternLength) && (m_pattern[m_index] == '{')) 
			{
				int end = m_pattern.IndexOf('}', m_index);	
				if (end > m_index) 
				{
					string r = m_pattern.Substring(m_index + 1, end - m_index - 1);
					m_index = end + 1;
					return r;
				}
			}
			return null;
		}

		/// <summary>
		/// The option is expected to be in decimal and positive. In case of error, zero is returned.
		/// </summary>
		/// <returns>The option as a number.</returns>
		protected int ExtractPrecisionOption() 
		{
			string opt = ExtractOption();
			int r = 0;
			if (opt != null) 
			{
				try 
				{
					r = int.Parse(opt, System.Globalization.NumberFormatInfo.InvariantInfo);
					if (r <= 0) 
					{
						LogLog.Error("PatternParser: Precision option ["+opt+"] isn't a positive integer.");
						r = 0;
					}
				}	  
				catch (Exception e) 
				{
					LogLog.Error("PatternParser: Logger option ["+opt+"] not a decimal integer.", e);
				}	  
			}
			return r;	
		}
	  
		/// <summary>
		/// Internal method that works on a single option in the
		/// pattern
		/// </summary>
		/// <param name="c">The option specifier.</param>
		protected void FinalizeConverter(char c) 
		{
			PatternConverter pc = null;
			switch(c) 
			{
				case 'a':
					pc = new BasicPatternConverter(m_formattingInfo, APPDOMAIN_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'c':
					pc = new LoggerPatternConverter(m_formattingInfo, ExtractPrecisionOption());	
					m_currentLiteral.Length = 0;
					break;	 

				case 'C':
					pc = new ClassNamePatternConverter(m_formattingInfo, ExtractPrecisionOption());
					m_currentLiteral.Length = 0;
					break;

				case 'd':
					string dateFormatStr = AbsoluteTimeDateFormatter.ISO8601_DATE_FORMAT;
					IDateFormatter df;
					string dOpt = ExtractOption();
					if (dOpt != null)
					{
						dateFormatStr = dOpt;
					}
	  
					if (string.Compare(dateFormatStr, AbsoluteTimeDateFormatter.ISO8601_DATE_FORMAT, true, System.Globalization.CultureInfo.InvariantCulture) == 0) 
					{
						df = new ISO8601DateFormatter();
					}
					else if (string.Compare(dateFormatStr, AbsoluteTimeDateFormatter.ABS_TIME_DATE_FORMAT, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					{
						df = new AbsoluteTimeDateFormatter();
					}
					else if (string.Compare(dateFormatStr, AbsoluteTimeDateFormatter.DATE_AND_TIME_DATE_FORMAT, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					{
						df = new DateTimeDateFormatter();
					}
					else 
					{
						try 
						{
							df = new SimpleDateFormatter(dateFormatStr);
						}
						catch (Exception e) 
						{
							LogLog.Error("PatternParser: Could not instantiate SimpleDateFormatter with ["+dateFormatStr+"]", e);
							df = new ISO8601DateFormatter();
						}	
					}
					pc = new DatePatternConverter(m_formattingInfo, df);
					m_currentLiteral.Length = 0;
					break;

				case 'F':
					pc = new LocationPatternConverter(m_formattingInfo, FILE_LOCATION_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'l':
					pc = new LocationPatternConverter(m_formattingInfo, FULL_LOCATION_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'L':
					pc = new LocationPatternConverter(m_formattingInfo, LINE_LOCATION_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'm':
					pc = new BasicPatternConverter(m_formattingInfo, MESSAGE_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'M':
					pc = new LocationPatternConverter(m_formattingInfo, METHOD_LOCATION_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'p':
					pc = new BasicPatternConverter(m_formattingInfo, LEVEL_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'P':
					pc = new PropertyPatternConverter(m_formattingInfo, this.ExtractOption());
					m_currentLiteral.Length = 0;
					break;

				case 'r':
					pc = new BasicPatternConverter(m_formattingInfo, RELATIVE_TIME_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 't':
					pc = new BasicPatternConverter(m_formattingInfo, THREAD_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'u':
					pc = new BasicPatternConverter(m_formattingInfo, IDENTITY_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'W':
					pc = new BasicPatternConverter(m_formattingInfo, USERNAME_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'x':
					pc = new BasicPatternConverter(m_formattingInfo, NDC_CONVERTER);
					m_currentLiteral.Length = 0;
					break;

				case 'X':
					pc = new MDCPatternConverter(m_formattingInfo, this.ExtractOption());
					m_currentLiteral.Length = 0;
					break;

				default:
					LogLog.Error("PatternParser: Unexpected char ["+c+"] at position ["+m_index+"] in conversion pattern.");
					pc = new LiteralPatternConverter(m_currentLiteral.ToString());
					m_currentLiteral.Length = 0;
					break;
			}
			AddConverter(pc);
		}

		/// <summary>
		/// Resets the internal state of the parser and adds the specified pattern converter 
		/// to the chain.
		/// </summary>
		/// <param name="pc">The pattern converter to add.</param>
		protected void AddConverter(PatternConverter pc) 
		{
			m_currentLiteral.Length = 0;

			// Add the pattern converter to the list.
			AddToList(pc);

			// Next pattern is assumed to be a literal.
			m_state = LITERAL_STATE;

			// Reset formatting info
			m_formattingInfo.Reset();
		}

		#endregion Protected Instance Methods

		#region Private Instance Methods

		/// <summary>
		/// Adds a pattern converter to the chain.
		/// </summary>
		/// <param name="pc">The converter to add.</param>
		private void AddToList(PatternConverter pc) 
		{
			if (m_head == null) 
			{
				m_head = m_tail = pc;
			}
			else 
			{
				m_tail.Next = pc;
				m_tail = pc;	
			}
		}

		#endregion Private Instance Methods			

		#region Private Static Fields

		private const char ESCAPE_CHAR = '%';
  
		private const int LITERAL_STATE = 0;
		private const int CONVERTER_STATE = 1;
		private const int MINUS_STATE = 2;
		private const int DOT_STATE = 3;
		private const int MIN_STATE = 4;
		private const int MAX_STATE = 5;

		private const int FULL_LOCATION_CONVERTER = 1000;
		private const int METHOD_LOCATION_CONVERTER = 1001;
		private const int CLASS_LOCATION_CONVERTER = 1002;
		private const int LINE_LOCATION_CONVERTER = 1003;
		private const int FILE_LOCATION_CONVERTER = 1004;	

		private const int RELATIVE_TIME_CONVERTER = 2000;
		private const int THREAD_CONVERTER = 2001;
		private const int LEVEL_CONVERTER = 2002;
		private const int NDC_CONVERTER = 2003;
		private const int MESSAGE_CONVERTER = 2004;

		private const int USERNAME_CONVERTER = 3000;
		private const int IDENTITY_CONVERTER = 3001;
		private const int APPDOMAIN_CONVERTER = 3002;

		#endregion Private Static Fields

		#region Private Instance Fields

		private int m_state;

		/// <summary>
		/// the literal being parsed
		/// </summary>
		private StringBuilder m_currentLiteral = new StringBuilder(32);

		/// <summary>
		/// the total length of the pattern
		/// </summary>
		private int m_patternLength;

		/// <summary>
		/// the current index into the pattern
		/// </summary>
		private int m_index;

		/// <summary>
		/// The first pattern converter in the chain
		/// </summary>
		private PatternConverter m_head;

		/// <summary>
		///  the last pattern converter in the chain
		/// </summary>
		private PatternConverter m_tail;

		/// <summary>
		/// the formatting info object
		/// </summary>
		private FormattingInfo m_formattingInfo = new FormattingInfo();

		/// <summary>
		/// The pattern
		/// </summary>
		private string m_pattern;

		#endregion Private Instance Fields

		// ---------------------------------------------------------------------
		//					  PatternConverters
		// ---------------------------------------------------------------------
	
		/// <summary>
		/// Basic pattern converter
		/// </summary>
		internal class BasicPatternConverter : PatternConverter 
		{
			private int m_type;
	
			/// <summary>
			/// Construct the pattern converter with formatting info and type
			/// </summary>
			/// <param name="formattingInfo">the formatting info</param>
			/// <param name="type">the type of pattern</param>
			internal BasicPatternConverter(FormattingInfo formattingInfo, int type) : base(formattingInfo)
			{
				m_type = type;
			}

			/// <summary>
			/// To the conversion
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the result of converting the pattern</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				switch(m_type) 
				{
					case RELATIVE_TIME_CONVERTER: 
						return TimeDifferenceInMillis(LoggingEvent.StartTime, loggingEvent.TimeStamp).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
					case THREAD_CONVERTER:
						return loggingEvent.ThreadName;
					case LEVEL_CONVERTER:
						return loggingEvent.Level.ToString();
					case NDC_CONVERTER:  
						return loggingEvent.NestedContext;
					case MESSAGE_CONVERTER: 
						return loggingEvent.RenderedMessage;
					case USERNAME_CONVERTER:
						return loggingEvent.UserName;
					case IDENTITY_CONVERTER:
						return loggingEvent.Identity;
					case APPDOMAIN_CONVERTER:
						return loggingEvent.Domain;

					default: 
						return null;
				}
			}

			/// <summary>
			/// Internal method to get the time difference between two DateTime objects
			/// </summary>
			/// <param name="start">start time</param>
			/// <param name="end">end time</param>
			/// <returns>the time difference in milliseconds</returns>
			private static long TimeDifferenceInMillis(DateTime start, DateTime end)
			{
				return ((end.Ticks - start.Ticks) / TimeSpan.TicksPerMillisecond);
			}
		}

		/// <summary>
		/// Pattern converter for literal instances in the pattern
		/// </summary>
		internal class LiteralPatternConverter : PatternConverter 
		{
			private string m_literal;
  
			/// <summary>
			/// Constructor, takes the literal string
			/// </summary>
			/// <param name="strValue"></param>
			internal LiteralPatternConverter(string strValue) 
			{
				m_literal = strValue;
			}

			/// <summary>
			/// Override the formatting behaviour to ignore the FormattingInfo
			/// because we have a literal instead.
			/// </summary>
			/// <param name="buffer">the builder to write to</param>
			/// <param name="loggingEvent">the event being logged</param>
			override public void Format(StringBuilder buffer, LoggingEvent loggingEvent) 
			{
				buffer.Append(m_literal);
			}
	
			/// <summary>
			/// Convert this pattern into the rendered message
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the literal</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				return m_literal;
			}
		}

		/// <summary>
		/// Mapped Diagnostic pattern converter
		/// </summary>
		internal class MDCPatternConverter : PatternConverter 
		{
			private string m_key;
	
			/// <summary>
			/// Construct the pattern converter with formatting info and MDC key
			/// </summary>
			/// <param name="formattingInfo">the formatting info</param>
			/// <param name="key">the MDC key to emit</param>
			internal MDCPatternConverter(FormattingInfo formattingInfo, string key) : base(formattingInfo)
			{
				m_key = key;
			}

			/// <summary>
			/// To the conversion
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the result of converting the pattern</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				return loggingEvent.LookupMappedContext(m_key);
			}
		}

		/// <summary>
		/// Property pattern converter
		/// </summary>
		internal class PropertyPatternConverter : PatternConverter 
		{
			private string m_key;
	
			/// <summary>
			/// Construct the pattern converter with formatting info and event property key
			/// </summary>
			/// <param name="formattingInfo">the formatting info</param>
			/// <param name="key">the property key to emit</param>
			internal PropertyPatternConverter(FormattingInfo formattingInfo, string key) : base(formattingInfo)
			{
				m_key = key;
			}

			/// <summary>
			/// To the conversion
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the result of converting the pattern</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				object prop = loggingEvent.Properties[m_key];
				if (prop != null)
				{
					return prop.ToString();
				}
				return null;
			}
		}
		/// <summary>
		/// Date pattern converter, uses a <see cref="IDateFormatter"/> to format the date
		/// </summary>
		internal class DatePatternConverter : PatternConverter 
		{
			private IDateFormatter m_df;
			private DateTime m_date;
	
			/// <summary>
			/// Construct the converter with formatting info and a
			/// <see cref="IDateFormatter"/> to format the date
			/// </summary>
			/// <param name="formattingInfo">the formatting info</param>
			/// <param name="df">the date formatter</param>
			internal DatePatternConverter(FormattingInfo formattingInfo, IDateFormatter df) : base(formattingInfo)
			{
				m_df = df;	  
			}

			/// <summary>
			/// Convert the pattern into the rendered message
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns></returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				m_date = loggingEvent.TimeStamp;
				string converted = null;
				try 
				{
					converted = m_df.FormatDate(m_date, new StringBuilder()).ToString();
				}
				catch (Exception ex) 
				{
					LogLog.Error("PatternParser: Error occurred while converting date.", ex);
				}
				return converted;
			}
		}

		/// <summary>
		/// Converter to include event location information
		/// </summary>
		internal class LocationPatternConverter : PatternConverter 
		{
			private int m_type;
	
			/// <summary>
			/// Construct the converter with formatting information and
			/// the type of location information required.
			/// </summary>
			/// <param name="formattingInfo"></param>
			/// <param name="type"></param>
			internal LocationPatternConverter(FormattingInfo formattingInfo, int type) : base(formattingInfo)
			{
				m_type = type;
			}
	
			/// <summary>
			/// Convert the pattern to the rendered message
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the relevant location information</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				LocationInfo locationInfo = loggingEvent.LocationInformation;
				switch(m_type) 
				{
					case FULL_LOCATION_CONVERTER: 
						return locationInfo.FullInfo;

					case METHOD_LOCATION_CONVERTER: 
						return locationInfo.MethodName;

					case LINE_LOCATION_CONVERTER: 
						return locationInfo.LineNumber;

					case FILE_LOCATION_CONVERTER: 
						return locationInfo.FileName;

					default: 
						return null;
				}
			}
		}

		/// <summary>
		/// Converter to deal with '.' separated strings
		/// </summary>
		internal abstract class NamedPatternConverter : PatternConverter 
		{
			protected int m_precision;
	
			/// <summary>
			/// Construct a converter with formatting info and a precision
			/// argument. The precision is the number of '.' separated sections
			/// to return, starting from the end of the string and working 
			/// towards to the start.
			/// </summary>
			/// <param name="formattingInfo">the formatting info</param>
			/// <param name="precision">the precision</param>
			internal NamedPatternConverter(FormattingInfo formattingInfo, int precision) : base(formattingInfo)
			{
				m_precision =  precision;	  
			}

			/// <summary>
			/// Overridden by subclasses to get the fully qualified name before the
			/// precision is applied to it.
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the fully qualified name</returns>
			abstract protected string GetFullyQualifiedName(LoggingEvent loggingEvent);
	
			/// <summary>
			/// Convert the pattern to the rendered message
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the precision of the fully qualified name specified</returns>
			override protected string Convert(LoggingEvent loggingEvent) 
			{
				string n = GetFullyQualifiedName(loggingEvent);
				if (m_precision <= 0)
				{
					return n;
				}
				else 
				{
					int len = n.Length;

					// We subtract 1 from 'len' when assigning to 'end' to avoid out of
					// bounds exception in return r.substring(end+1, len). This can happen if
					// precision is 1 and the logger name ends with a dot. 
					int end = len -1 ;
					for(int i = m_precision; i > 0; i--) 
					{	  
						end = n.LastIndexOf('.', end-1);
						if (end == -1)
						{
							return n;
						}
					}
					return n.Substring(end+1, len-end-1);
				}	  
			}
		}
  
		/// <summary>
		/// Pattern converter for the class name
		/// </summary>
		internal class ClassNamePatternConverter : NamedPatternConverter 
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="formattingInfo">formatting info</param>
			/// <param name="precision">namespace depth precision</param>
			internal ClassNamePatternConverter(FormattingInfo formattingInfo, int precision) : base(formattingInfo, precision)
			{
			}
	
			/// <summary>
			/// Gets the fully qualified name of the class
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the class name</returns>
			override protected string GetFullyQualifiedName(LoggingEvent loggingEvent) 
			{
				return loggingEvent.LocationInformation.ClassName;
			}
		}
  
		/// <summary>
		/// Converter for logger name
		/// </summary>
		internal class LoggerPatternConverter : NamedPatternConverter 
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="formattingInfo">formatting info</param>
			/// <param name="precision">logger hierarchy depth precision</param>
			internal LoggerPatternConverter(FormattingInfo formattingInfo, int precision) : base(formattingInfo, precision)
			{
			}
	
			/// <summary>
			/// Gets the fully qualified name of the logger
			/// </summary>
			/// <param name="loggingEvent">the event being logged</param>
			/// <returns>the logger name</returns>
			override protected string GetFullyQualifiedName(LoggingEvent loggingEvent) 
			{
				return loggingEvent.LoggerName;
			}
		}  	
	}
}
