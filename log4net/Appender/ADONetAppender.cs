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
using System.Collections;
using System.Data;
using System.Reflection;

using log4net.helpers;
using log4net.Layout;
using log4net.spi;

namespace log4net.Appender
{
	/// <summary>
	/// Appender that logs to a database.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="ADONetAppender"/> appends logging events to a table within a
	/// database. The appender can be configured to specify the connection 
	/// string by setting the <see cref="ConnectionString"/> property. 
	/// The connection type (provider) can be specified by setting the <see cref="ConnectionType"/>
	/// property. For more information on database connection strings for
	/// your specific database see <a href="http://www.connectionstrings.com/">http://www.connectionstrings.com/</a>.
	/// </para>
	/// <para>
	/// Records are written into the database either using a prepared
	/// statement or a stored procedure. The <see cref="CommandType"/> property
	/// is set to <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>) to specify a prepared statement
	/// or to <see cref="System.Data.CommandType.StoredProcedure"/> (<c>System.Data.CommandType.StoredProcedure</c>) to specify a stored
	/// procedure.
	/// </para>
	/// <para>
	/// The prepared statement text or the name of the stored procedure
	/// must be set in the <see cref="CommandText"/> property.
	/// </para>
	/// <para>
	/// The prepared statement or stored procedure can take a number
	/// of parameters. Parameters are added using the <see cref="AddParameter"/>
	/// method. This adds a single <see cref="ADONetAppenderParameter"/> to the
	/// ordered list of parameters. The <see cref="ADONetAppenderParameter"/>
	/// type may be subclassed if required to provide database specific
	/// functionality. The <see cref="ADONetAppenderParameter"/> specifies
	/// the parameter name, database type, size, and how the value should
	/// be generated using a <see cref="ILayout"/>.
	/// </para>
	/// </remarks>
	/// <example>
	/// An example of a SQL Server table that could be logged to:
	/// <code>
	/// CREATE TABLE [dbo].[Log] ( 
	///   [ID] [int] IDENTITY (1, 1) NOT NULL ,
	///   [Date] [datetime] NOT NULL ,
	///   [Thread] [varchar] (255) NOT NULL ,
	///   [Level] [varchar] (20) NOT NULL ,
	///   [Logger] [varchar] (255) NOT NULL ,
	///   [Message] [varchar] (4000) NOT NULL 
	/// ) ON [PRIMARY]
	/// </code>
	/// </example>
	/// <example>
	/// An example configuration to log to the above table:
	/// <code>
	/// &lt;appender name="ADONetAppender_SqlServer" type="log4net.Appender.ADONetAppender" &gt;
	///   &lt;param name="ConnectionType" value="System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" /&gt;
	///   &lt;param name="ConnectionString" value="data source=GUINNESS;initial catalog=test_log4net;integrated security=false;persist security info=True;User ID=sa;Password=sql" /&gt;
	///   &lt;param name="CommandText" value="INSERT INTO Log ([Date],[Thread],[Level],[Logger],[Message]) VALUES (@log_date, @thread, @log_level, @logger, @message)" /&gt;
	///   &lt;param name="Parameter"&gt;
	///     &lt;param name="ParameterName" value="@log_date" /&gt;
	///     &lt;param name="DbType" value="DateTime" /&gt;
	///     &lt;param name="Layout" type="log4net.Layout.PatternLayout"&gt;
	///       &lt;param name="ConversionPattern" value="%d{yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}" /&gt;
	///     &lt;/param&gt;
	///   &lt;/param&gt;
	///   &lt;param name="Parameter"&gt;
	///     &lt;param name="ParameterName" value="@thread" /&gt;
	///     &lt;param name="DbType" value="String" /&gt;
	///     &lt;param name="Size" value="255" /&gt;
	///     &lt;param name="Layout" type="log4net.Layout.PatternLayout"&gt;
	///       &lt;param name="ConversionPattern" value="%t" /&gt;
	///     &lt;/param&gt;
	///   &lt;/param&gt;
	///   &lt;param name="Parameter"&gt;
	///     &lt;param name="ParameterName" value="@log_level" /&gt;
	///     &lt;param name="DbType" value="String" /&gt;
	///     &lt;param name="Size" value="50" /&gt;
	///     &lt;param name="Layout" type="log4net.Layout.PatternLayout"&gt;
	///       &lt;param name="ConversionPattern" value="%p" /&gt;
	///     &lt;/param&gt;
	///   &lt;/param&gt;
	///   &lt;param name="Parameter"&gt;
	///     &lt;param name="ParameterName" value="@logger" /&gt;
	///     &lt;param name="DbType" value="String" /&gt;
	///     &lt;param name="Size" value="255" /&gt;
	///     &lt;param name="Layout" type="log4net.Layout.PatternLayout"&gt;
	///       &lt;param name="ConversionPattern" value="%c" /&gt;
	///     &lt;/param&gt;
	///   &lt;/param&gt;
	///   &lt;param name="Parameter"&gt;
	///     &lt;param name="ParameterName" value="@message" /&gt;
	///     &lt;param name="DbType" value="String" /&gt;
	///     &lt;param name="Size" value="4000" /&gt;
	///     &lt;param name="Layout" type="log4net.Layout.PatternLayout"&gt;
	///       &lt;param name="ConversionPattern" value="%m" /&gt;
	///     &lt;/param&gt;
	///   &lt;/param&gt;
	/// &lt;/appender&gt;
	/// </code>
	/// </example>
	public class ADONetAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary> 
		/// Initializes a new instance of the <see cref="ADONetAppender" /> class.
		/// </summary>
		public ADONetAppender()
		{
			m_connectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			m_useTransactions = true;
			m_commandType = System.Data.CommandType.Text;
			m_parameters = new ArrayList();
		}

		#endregion // Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the database connection string that is used to connect to 
		/// the database.
		/// </summary>
		/// <value>
		/// The database connection string used to connect to the database.
		/// </value>
		/// <remarks>
		/// <para>
		/// The connections string is specific to the connection type.
		/// See <see cref="ConnectionType"/> for more information.
		/// </para>
		/// </remarks>
		/// <example>Connection string for MS Access via ODBC:
		/// <code>"DSN=MS Access Database;UID=admin;PWD=;SystemDB=C:\\data\\System.mdw;SafeTransactions = 0;FIL=MS Access;DriverID = 25;DBQ=C:\\data\\train33.mdb"</code>
		/// </example>
		/// <example>Another connection string for MS Access via ODBC:
		/// <code>"Driver={Microsoft Access Driver (*.mdb)};DBQ=C:\\Work\\cvs_root\\log4net-1.2\\access.mdb;UID=;PWD=;"</code>
		/// </example>
		/// <example>Connection string for MS Access via OLE DB:
		/// <code>"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Work\\cvs_root\\log4net-1.2\\access.mdb;User Id=;Password=;"</code>
		/// </example>
		public string ConnectionString
		{
			get { return m_connectionString; }
			set { m_connectionString = value; }
		}

		/// <summary>
		/// Gets or sets the type name of the <see cref="IDbConnection"/> connection
		/// that should be created.
		/// </summary>
		/// <value>
		/// The type name of the <see cref="IDbConnection"/> connection.
		/// </value>
		/// <remarks>
		/// <para>
		/// The type name of the ADO.NET provider to use.
		/// </para>
		/// <para>
		/// The default is to use the OLE DB provider.
		/// </para>
		/// </remarks>
		/// <example>Use the OLE DB Provider. This is the default value.
		/// <code>System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</code>
		/// </example>
		/// <example>Use the MS SQL Server Provider. 
		/// <code>System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</code>
		/// </example>
		/// <example>Use the ODBC Provider. 
		/// <code>Microsoft.Data.Odbc.OdbcConnection,Microsoft.Data.Odbc,version=1.0.3300.0,publicKeyToken=b77a5c561934e089,culture=neutral</code>
		/// This is an optional package that you can download from 
		/// <a href="http://msdn.microsoft.com/downloads">http://msdn.microsoft.com/downloads</a> 
		/// search for <b>ODBC .NET Data Provider</b>.
		/// </example>
		public string ConnectionType
		{
			get { return m_connectionType; }
			set { m_connectionType = value; }
		}

		/// <summary>
		/// Gets or sets the command text that is used to insert logging events
		/// into the database.
		/// </summary>
		/// <value>
		/// The command text used to insert logging events into the database.
		/// </value>
		/// <remarks>
		/// <para>
		/// Either the text of the prepared statement or the
		/// name of the stored procedure to execute to write into
		/// the database.
		/// </para>
		/// <para>
		/// The <see cref="CommandType"/> property determines if
		/// this text is a prepared statement or a stored procedure.
		/// </para>
		/// </remarks>
		public string CommandText
		{
			get { return m_commandText; }
			set { m_commandText = value; }
		}

		/// <summary>
		/// Gets or sets the command type to execute.
		/// </summary>
		/// <value>
		/// The command type to execute.
		/// </value>
		/// <remarks>
		/// <para>
		/// This value may be either <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>) to specify
		/// that the <see cref="CommandText"/> is a prepared statement to execute, 
		/// or <see cref="System.Data.CommandType.StoredProcedure"/> (<c>System.Data.CommandType.StoredProcedure</c>) to specify that the
		/// <see cref="CommandText"/> property is the name of a stored procedure
		/// to execute.
		/// </para>
		/// <para>
		/// The default value is <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>).
		/// </para>
		/// </remarks>
		public CommandType CommandType
		{
			get { return m_commandType; }
			set { m_commandType = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether transactions should be used
		/// to insert logging events in the database.
		/// </summary>
		/// <value>
		/// <c>true</c> if transactions should be used to insert logging events in
		/// the database, otherwisr <c>false</c>. The default value is <c>true</c>.
		/// </value>
		public bool UseTransactions
		{
			get { return m_useTransactions; }
			set { m_useTransactions = value; }
		}

		#endregion // Public Instance Properties

		#region Protected Instance Properties

		/// <summary>
		/// Gets or sets the underlying <see cref="IDbConnection" />.
		/// </summary>
		/// <value>
		/// The underlying <see cref="IDbConnection" />.
		/// </value>
		/// <remarks>
		/// <see cref="ADONetAppender" /> creates a <see cref="IDbConnection" /> to insert 
		/// logging events into a database.  Classes deriving from <see cref="ADONetAppender" /> 
		/// can use this property to get or set this <see cref="IDbConnection" />.  Use the 
		/// underlying <see cref="IDbConnection" /> returned from <see cref="Connection" /> if 
		/// you require access beyond that which <see cref="ADONetAppender" /> provides.
		/// </remarks>
		protected IDbConnection Connection 
		{
			get { return this.m_dbConnection; }
			set { this.m_dbConnection = value; }
		}

		#endregion // Protected Instance Properties

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialise the appender based on the options set
		/// </summary>
		override public void ActivateOptions() 
		{
			base.ActivateOptions();
			InitializeDatabaseConnection();

			// Are we using a command object
			if (m_commandText != null && m_commandText.Length > 0)
			{
				m_usePreparedCommand = true;

				// Create the command object
				InitializeDatabaseCommand();
			}
			else
			{
				m_usePreparedCommand = false;
			}
		}

		#endregion

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Override the parent method to close the database
		/// </summary>
		override public void OnClose() 
		{
			base.OnClose();
			if (m_dbCommand != null)
			{
				m_dbCommand.Dispose();
				m_dbCommand = null;
			}
			if (m_dbConnection != null)
			{
				m_dbConnection.Close();
				m_dbConnection = null;
			}
		}

		#endregion

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Inserts the events into the database.
		/// </summary>
		/// <param name="events">The events to insert into the database.</param>
		override protected void SendBuffer(LoggingEvent[] events)
		{
			// Check that the connection exists and is open
			if (m_dbConnection != null && m_dbConnection.State == ConnectionState.Open)
			{
				if (m_useTransactions)
				{
					// Create transaction
					// NJC - Do this on 2 lines because it can confuse the debugger
					IDbTransaction dbTran = null;
					dbTran = m_dbConnection.BeginTransaction();
					try
					{
						SendBuffer(dbTran, events);

						// commit transaction
						dbTran.Commit();
					}
					catch(Exception ex)
					{
						// rollback the transaction
						try
						{
							dbTran.Rollback();
						}
						catch(Exception)
						{
						}

						// Can't insert into the database. That's a bad thing
						ErrorHandler.Error("Exception while writing to database", ex);
					}
				}
				else
				{
					// Send without transaction
					SendBuffer(null, events);
				}
			}
		}

		#endregion // Override implementation of BufferingAppenderSkeleton

		#region Public Instance Methods

		/// <summary>
		/// Adds a parameter to the command.
		/// </summary>
		/// <param name="parameter">The parameter to add to the command.</param>
		/// <remarks>
		/// <para>
		/// Adds a parameter to the ordered list of command parameters.
		/// </para>
		/// </remarks>
		public void AddParameter(ADONetAppenderParameter parameter)
		{
			m_parameters.Add(parameter);
		}


		#endregion // Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Writes the events to the database using the transaction specified.
		/// </summary>
		/// <param name="dbTran">The transaction that the events will be executed under.</param>
		/// <param name="events">The array of events to insert into the database.</param>
		/// <remarks>
		/// The transaction argument can be <c>null</c> if the appender has been
		/// configured not to use transactions. See <see cref="UseTransactions"/>
		/// property for more information.
		/// </remarks>
		virtual protected void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
		{
			if (m_usePreparedCommand) 
			{
				// Send buffer using the prepared command object

				if (m_dbCommand != null)
				{
					if (dbTran != null)
					{
						m_dbCommand.Transaction = dbTran;
					}

					// run for all events
					foreach(LoggingEvent e in events)
					{
						// Set the parameter values
						foreach(ADONetAppenderParameter param in m_parameters)
						{
							param.FormatValue(m_dbCommand, e);
						}

						// Execute the query
						m_dbCommand.ExecuteNonQuery();
					}
				}
			}
			else
			{
				// create a new command
				using(IDbCommand dbCmd = m_dbConnection.CreateCommand())
				{
					if (dbTran != null)
					{
						dbCmd.Transaction = dbTran;
					}

					// run for all events
					foreach(LoggingEvent e in events)
					{
						// Get the command text from the Layout
						string logStatement = GetLogStatement(e);

						LogLog.Debug("ADOAppender: LogStatement ["+logStatement+"]");

						dbCmd.CommandText = logStatement;
						dbCmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Formats the log message into database statement text.
		/// </summary>
		/// <param name="logEvent">The event being logged.</param>
		/// <remarks>
		/// This method can be overridden by subclasses to provide 
		/// more control over the format of the database statement.
		/// </remarks>
		/// <returns>
		/// Text that can be passed to a <see cref="System.Data.IDbCommand"/>.
		/// </returns>
		virtual protected string GetLogStatement(LoggingEvent logEvent)
		{
			if (Layout == null)
			{
				ErrorHandler.Error("ADOAppender: No Layout specified.");
				return "";
			}
			else
			{
				return Layout.Format(logEvent);
			}
		}

		/// <summary>
		/// Connects to the database.
		/// </summary>		
		virtual protected void InitializeDatabaseConnection()
		{
			try
			{
				// Create the connection object
				m_dbConnection = (IDbConnection)Activator.CreateInstance(ResolveConnectionType());
			
				// Set the connection string
				m_dbConnection.ConnectionString = m_connectionString;

				// Open the database connection
				m_dbConnection.Open();
			}
			catch (System.Exception e)
			{
				// Sadly, your connection string is bad.
				ErrorHandler.Error("Could not open database connection [" + m_connectionString + "]", e);	 
			}
		}

		/// <summary>
		/// Retrieves the class type of the ADO.NET provider.
		/// </summary>
		virtual protected Type ResolveConnectionType()
		{
			try
			{
				return Type.GetType(m_connectionType, true);
			}
			catch(Exception ex)
			{
				ErrorHandler.Error("Failed to load connection type ["+m_connectionType+"]", ex);
				throw;
			}
		}

		/// <summary>
		/// Prepares the database command and initialize the parameters.
		/// </summary>
		virtual protected void InitializeDatabaseCommand()
		{
			try
			{
				// Create the command object
				m_dbCommand = m_dbConnection.CreateCommand();
		
				// Set the command string
				m_dbCommand.CommandText = m_commandText;

				// Set the command type
				m_dbCommand.CommandType = m_commandType;
			}
			catch (System.Exception e)
			{
				ErrorHandler.Error("Could not create database command ["+m_commandText+"]", e);	 

				if (m_dbCommand != null)
				{
					try
					{
						m_dbCommand.Dispose();
					}
					catch
					{
					}
					m_dbCommand = null;
				}
			}

			if (m_dbCommand != null)
			{
				try
				{
					foreach(ADONetAppenderParameter param in m_parameters)
					{
						try
						{
							param.Prepare(m_dbCommand);
						}
						catch(System.Exception e)
						{
							ErrorHandler.Error("Could not add database command parameter ["+param.ParameterName+"]", e);	 
							throw;
						}
					}
				}
				catch
				{
					try
					{
						m_dbCommand.Dispose();
					}
					catch
					{
					}
					m_dbCommand = null;
				}
			}

			if (m_dbCommand != null)
			{
				try
				{
					// Prepare the command statement.
					m_dbCommand.Prepare();
				}
				catch (System.Exception e)
				{
					ErrorHandler.Error("Could not prepare database command ["+m_commandText+"]", e);
					try
					{
						m_dbCommand.Dispose();
					}
					catch
					{
					}
					m_dbCommand = null;
				}
			}
		}

		#endregion // Protected Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The <see cref="IDbConnection" /> that will be used
		/// to insert logging events into a database.
		/// </summary>
		private IDbConnection m_dbConnection;

		/// <summary>
		/// The database command.
		/// </summary>
		private IDbCommand m_dbCommand;

		/// <summary>
		/// Flag to indicate if we are using a command object
		/// </summary>
		private bool m_usePreparedCommand;

		/// <summary>
		/// Database connection string.
		/// </summary>
		private string m_connectionString;

		/// <summary>
		/// String type name of the <see cref="IDbConnection"/> type name.
		/// </summary>
		private string m_connectionType;

		/// <summary>
		/// The text of the command.
		/// </summary>
		private string m_commandText;

		/// <summary>
		/// The command type.
		/// </summary>
		private CommandType m_commandType;

		/// <summary>
		/// Incicats whether to use Utransactions when writing to the 
		/// database.
		/// </summary>
		private bool m_useTransactions;

		/// <summary>
		/// The list of <see cref="ADONetAppenderParameter"/> objects.
		/// </summary>
		/// <remarks>
		/// The list of <see cref="ADONetAppenderParameter"/> objects.
		/// </remarks>
		private ArrayList m_parameters;

		#endregion // Private Instance Fields
	}

	/// <summary>
	/// Parameter type used by the <see cref="ADONetAppender"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class provides the basic database parameter properties
	/// as defined by the <see cref="System.Data.IDbDataParameter"/> interface.
	/// </para>
	/// <para>This type can be subclassed to provide database specific
	/// functionality. The two methods that are called externally are
	/// <see cref="Prepare"/> and <see cref="FormatValue"/>.
	/// </para>
	/// </remarks>
	public class ADONetAppenderParameter
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ADONetAppenderParameter" />
		/// class.
		/// </summary>
		public ADONetAppenderParameter()
		{
			m_precision = 0;
			m_scale = 0;
			m_size = 0;
		}

		#endregion // Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the name of this parameter.
		/// </summary>
		/// <value>
		/// The name of this parameter.
		/// </value>
		public string ParameterName
		{
			get { return m_parameterName; }
			set { m_parameterName = value; }
		}

		/// <summary>
		/// Gets or sets the database type for this parameter.
		/// </summary>
		/// <value>
		/// The database type for this parameter.
		/// </value>
		public DbType DbType
		{
			get { return m_dbType; }
			set { m_dbType = value; }
		}

		/// <summary>
		/// Gets or sets the precision for this parameter.
		/// </summary>
		/// <value>
		/// The precision for this parameter.
		/// </value>
		public byte Precision 
		{
			get { return m_precision; } 
			set { m_precision = value; }
		}

		/// <summary>
		/// Gets or sets the scale for this parameter.
		/// </summary>
		/// <value>
		/// The scale for this parameter.
		/// </value>
		public byte Scale 
		{
			get { return m_scale; }
			set { m_scale = value; }
		}

		/// <summary>
		/// Gets or sets the size for this parameter.
		/// </summary>
		/// <value>
		/// The size for this parameter.
		/// </value>
		public int Size 
		{
			get { return m_size; }
			set { m_size = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="IRawLayout"/> to use to 
		/// render the logging event into an object for this 
		/// parameter.
		/// </summary>
		/// <value>
		/// The <see cref="IRawLayout"/> used to render the
		/// logging event into an object for this parameter.
		/// </value>
		public IRawLayout Layout
		{
			get { return m_layout; }
			set { m_layout = value; }
		}

		#endregion // Public Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Prepare the specified database command object.
		/// </summary>
		/// <param name="command">The command to prepare.</param>
		/// <remarks>
		/// <para>
		/// Prepares the database command object by adding
		/// this parameter to its collection of parameters.
		/// </para>
		/// </remarks>
		virtual public void Prepare(IDbCommand command)
		{
			// Create a new parameter
			IDbDataParameter param = command.CreateParameter();

			// Set the parameter properties
			param.ParameterName = m_parameterName;
			param.DbType = m_dbType;
			
			if (m_precision != 0)
			{
				param.Precision = m_precision;
			}
			if (m_scale != 0)
			{
				param.Scale = m_scale;
			}
			if (m_size != 0)
			{
				param.Size = m_size;
			}

			// Add the parameter to the collection of params
			command.Parameters.Add(param);
		}

		/// <summary>
		/// Renders the logging event and set the parameter value in the command.
		/// </summary>
		/// <param name="command">The command containing the parameter.</param>
		/// <param name="loggingEvent">The event to be rendered.</param>
		/// <remarks>
		/// <para>
		/// Renders the logging event using this parameters layout
		/// object. Sets the value of the parameter on the command object.
		/// </para>
		/// </remarks>
		virtual public void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
		{
			// Lookup the parameter
			IDbDataParameter param = (IDbDataParameter)command.Parameters[m_parameterName];

			param.Value = Layout.Format(loggingEvent);
		}

		#endregion // Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The name of this parameter.
		/// </summary>
		private string m_parameterName;

		/// <summary>
		/// The database type for this parameter.
		/// </summary>
		private DbType m_dbType;

		/// <summary>
		/// The precision for this parameter.
		/// </summary>
		private byte m_precision;

		/// <summary>
		/// The scale for this parameter.
		/// </summary>
		private byte m_scale;

		/// <summary>
		/// The size for this parameter.
		/// </summary>
		private int m_size;

		/// <summary>
		/// The <see cref="IRawLayout"/> to use to render the
		/// logging event into an object for this parameter.
		/// </summary>
		private IRawLayout m_layout;

		#endregion // Private Instance Fields
	}
}
