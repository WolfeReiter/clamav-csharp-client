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
using System.Web.Mail;

using log4net.Layout;
using log4net.spi;
using log4net.helpers;

namespace log4net.Appender
{
	/// <summary>
	/// Send an e-mail when a specific logging event occurs, typically on errors 
	/// or fatal errors.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The number of logging events delivered in this e-mail depend on
	/// the value of <see cref="BufferingAppenderSkeleton.BufferSize"/> option. The
	/// <see cref="SMTPAppender"/> keeps only the last
	/// <see cref="BufferingAppenderSkeleton.BufferSize"/> logging events in its 
	/// cyclic buffer. This keeps memory requirements at a reasonable level while 
	/// still delivering useful application context.
	/// </para>
	/// <para>
	/// This appender sets the <c>hostname</c> property in the 
	/// <see cref="LoggingEvent.Properties"/> collection to the name of 
	/// the machine on which the event is logged.
	/// </para>
	/// </remarks>
	public class SMTPAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// </summary>
		public SMTPAppender()
		{	
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets a semicolon-delimited list of recipient e-mail addresses.
		/// </summary>
		/// <value>
		/// A semicolon-delimited list of e-mail addresses.
		/// </value>
		public string To 
		{
			get { return m_to; }
			set { m_to = value; }
		}

		/// <summary>
		/// Gets or sets the e-mail address of the sender.
		/// </summary>
		/// <value>
		/// The e-mail address of the sender.
		/// </value>
		public string From 
		{
			get { return m_from; }
			set { m_from = value; }
		}

		/// <summary>
		/// Gets or sets the subject line of the e-mail message.
		/// </summary>
		/// <value>
		/// The subject line of the e-mail message.
		/// </value>
		public string Subject 
		{
			get { return m_subject; }
			set { m_subject = value; }
		}
  
		/// <summary>
		/// Gets or sets the name of the SMTP relay mail server to use to send 
		/// the e-mail messages.
		/// </summary>
		/// <value>
		/// The name of the e-mail relay server. If SmtpServer is not set, the 
		/// name of the local SMTP server is used.
		/// </value>
		public string SMTPHost
		{
			get { return m_smtpHost; }
			set { m_smtpHost = value; }
		}

		/// <summary>
		/// Obselete
		/// </summary>
		/// <remarks>
		/// Use the BufferingAppenderSkeleton Fix methods instead 
		/// </remarks>
		[Obsolete("Use the BufferingAppenderSkeleton Fix methods")]
		public bool LocationInfo
		{
			get { return false; }
			set { ; }
		}

		#endregion Public Instance Properties

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Sends the contents of the cyclic buffer as an e-mail message.
		/// </summary>
		/// <param name="events">The logging events to send.</param>
		override protected void SendBuffer(LoggingEvent[] events) 
		{
			// Note: this code already owns the monitor for this
			// appender. This frees us from needing to synchronize on 'cb'.
			try 
			{	  
				StringBuilder sbuf = new StringBuilder();

				string t = Layout.Header;
				if (t != null)
				{
					sbuf.Append(t);
				}


				string hostName = SystemInfo.HostName;

				for(int i = 0; i < events.Length; i++) 
				{
					// Set the hostname property
					if (events[i].Properties[LoggingEvent.HostNameProperty] == null)
					{
						events[i].Properties[LoggingEvent.HostNameProperty] = hostName;
					}

					// Render the event and append the text to the buffer
					sbuf.Append(RenderLoggingEvent(events[i]));
				}

				t = Layout.Footer;
				if (t != null)
				{
					sbuf.Append(t);
				}

				MailMessage mailMessage = new MailMessage();
				mailMessage.Body = sbuf.ToString();
				mailMessage.From = m_from;
				mailMessage.To = m_to;
				mailMessage.Subject = m_subject;

				if (m_smtpHost != null && m_smtpHost.Length > 0)
				{
					SmtpMail.SmtpServer = m_smtpHost;
				}

				SmtpMail.Send(mailMessage);
			} 
			catch(Exception e) 
			{
				ErrorHandler.Error("Error occurred while sending e-mail notification.", e);
			}
		}

		#endregion Override implementation of BufferingAppenderSkeleton

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion Override implementation of AppenderSkeleton

		#region Private Instance Fields

		private string m_to;
		private string m_from;
		private string m_subject;
		private string m_smtpHost;

		#endregion Private Instance Fields
	}
}
