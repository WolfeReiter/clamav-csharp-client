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
using System.Globalization;
using System.Reflection;

using log4net.Appender;
using log4net.helpers;
using log4net.spi;
using log4net.Repository;

namespace log4net.spi
{
	/// <summary>
	/// The default implementation of the <see cref="IRepositorySelector"/> interface.
	/// </summary>
	/// <remarks>
	/// Uses attributes defined on the calling assembly to determine how to
	/// configure the hierarchy for the domain..
	/// </remarks>
	public class DefaultRepositorySelector : IRepositorySelector
	{
		#region Public Events

		/// <summary>
		/// Event to notify that a logger repository has been created.
		/// </summary>
		/// <value>
		/// Event to notify that a logger repository has been created.
		/// </value>
		public event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent 
		{
			add { m_loggerRepositoryCreatedEvent += value; }
			remove { m_loggerRepositoryCreatedEvent -= value; }
		}

		#endregion Public Events

		#region Public Instance Constructors

		/// <summary>
		/// Creates a new repository selector.
		/// </summary>
		/// <param name="defaultRepositoryType">The type of the repositories to create, must implement <see cref="ILoggerRepository"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="defaultRepositoryType"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="defaultRepositoryType"/> does not implement <see cref="ILoggerRepository"/>.</exception>
		public DefaultRepositorySelector(Type defaultRepositoryType)
		{
			if (defaultRepositoryType == null)
			{
				throw new ArgumentNullException("defaultRepositoryType");
			}

			// Check that the type is a repository
			if (! (typeof(ILoggerRepository).IsAssignableFrom(defaultRepositoryType)) )
			{
				throw new ArgumentOutOfRangeException("Parameter: defaultRepositoryType, Value: [" + defaultRepositoryType + "] out of range. Argument must implement the ILoggerRepository interface");
			}

			m_defaultRepositoryType = defaultRepositoryType;

			LogLog.Debug("DefaultRepositorySelector: defaultRepositoryType [" + m_defaultRepositoryType + "]");
		}

		#endregion Public Instance Constructors

		#region Implementation of IRepositorySelector

		/// <summary>
		/// Gets the <see cref="ILoggerRepository"/> for the specified assembly.
		/// </summary>
		/// <param name="domainAssembly">The assembly use to lookup the <see cref="ILoggerRepository"/>.</param>
		/// <remarks>
		/// <para>
		/// The type of the <see cref="ILoggerRepository"/> created and the domain 
		/// to create can be overridden by specifying the <see cref="log4net.Config.DomainAttribute"/> 
		/// attribute on the <paramref name="assembly"/>.
		/// </para>
		/// <para>
		/// The default values are to use the <see cref="log4net.Repository.Hierarchy.Hierarchy"/> 
		/// implementation of the <see cref="ILoggerRepository"/> interface and to use the
		/// <see cref="AssemblyName.Name"/> as the name of the domain.
		/// </para>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be automatically configured using 
		/// any <see cref="log4net.Config.ConfiguratorAttribute"/> attributes defined on
		/// the <paramref name="assembly"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is null.</exception>
		/// <returns>The <see cref="ILoggerRepository"/> for the assembly</returns>
		public ILoggerRepository GetRepository(Assembly domainAssembly)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}
			return CreateRepository(domainAssembly, m_defaultRepositoryType);
		}

		/// <summary>
		/// Get the <see cref="ILoggerRepository"/> for the specified domain
		/// </summary>
		/// <param name="domain">the domain to use to lookup to the <see cref="ILoggerRepository"/></param>
		/// <returns>The <see cref="ILoggerRepository"/> for the domain</returns>
		/// <exception cref="ArgumentNullException">throw if <paramref name="domain"/> is null</exception>
		/// <exception cref="LogException">throw if the <paramref name="domain"/> does not exist</exception>
		public ILoggerRepository GetRepository(string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}

			lock(this)
			{
				// Lookup in map
				ILoggerRepository rep = m_domain2repositoryMap[domain] as ILoggerRepository;
				if (rep == null)
				{
					throw new LogException("Domain ["+domain+"] is NOT defined.");
				}
				return rep;
			}
		}

		/// <summary>
		/// Create a new repository for the assembly specified 
		/// </summary>
		/// <param name="domainAssembly">the assembly to use to create the domain to associate with the <see cref="ILoggerRepository"/>.</param>
		/// <param name="repositoryType">The type of repository to create, must implement <see cref="ILoggerRepository"/>.</param>
		/// <returns>The repository created.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// <para>
		/// The type of the <see cref="ILoggerRepository"/> created and
		/// the domain to create can be overridden by specifying the
		/// <see cref="log4net.Config.DomainAttribute"/> attribute on the 
		/// <paramref name="assembly"/>.  The default values are to use the 
		/// <paramref name="repositoryType"/> implementation of the 
		/// <see cref="ILoggerRepository"/> interface and to use the
		/// <see cref="AssemblyName.Name"/> as the name of the domain.
		/// </para>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be automatically
		/// configured using any <see cref="log4net.Config.ConfiguratorAttribute"/> 
		/// attributes defined on the <paramref name="domainAssembly"/>.
		/// </para>
		/// <para>
		/// If a repository for the <paramref name="domainAssembly"/> already exists
		/// that repository will be returned. An error will not be raised and that 
		/// repository may be of a different type to that specified in <paramref name="repositoryType"/>.
		/// Also the <see cref="log4net.Config.DomainAttribute"/> attribute on the
		/// assembly may be used to override the repository type specified in 
		/// <paramref name="repositoryType"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="domainAssembly"/> is null.</exception>
		public ILoggerRepository CreateRepository(Assembly domainAssembly, Type repositoryType)
		{
			return CreateRepository(domainAssembly, repositoryType, DEFAULT_DOMAIN_NAME, true);
		}

		/// <summary>
		/// Create a new repository for the assembly specified 
		/// </summary>
		/// <param name="domainAssembly">the assembly to use to create the domain to associate with the <see cref="ILoggerRepository"/>.</param>
		/// <param name="repositoryType">The type of repository to create, must implement <see cref="ILoggerRepository"/>.</param>
		/// <param name="domainName">The name to assign to the created repository</param>
		/// <param name="readAssemblyAttributes">Set to <c>true</c> to read and apply the assembly attributes</param>
		/// <returns>The repository created.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// <para>
		/// The type of the <see cref="ILoggerRepository"/> created and
		/// the domain to create can be overridden by specifying the
		/// <see cref="log4net.Config.DomainAttribute"/> attribute on the 
		/// <paramref name="assembly"/>.  The default values are to use the 
		/// <paramref name="repositoryType"/> implementation of the 
		/// <see cref="ILoggerRepository"/> interface and to use the
		/// <see cref="AssemblyName.Name"/> as the name of the domain.
		/// </para>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be automatically
		/// configured using any <see cref="log4net.Config.ConfiguratorAttribute"/> 
		/// attributes defined on the <paramref name="domainAssembly"/>.
		/// </para>
		/// <para>
		/// If a repository for the <paramref name="domainAssembly"/> already exists
		/// that repository will be returned. An error will not be raised and that 
		/// repository may be of a different type to that specified in <paramref name="repositoryType"/>.
		/// Also the <see cref="log4net.Config.DomainAttribute"/> attribute on the
		/// assembly may be used to override the repository type specified in 
		/// <paramref name="repositoryType"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="domainAssembly"/> is null.</exception>
		public ILoggerRepository CreateRepository(Assembly domainAssembly, Type repositoryType, string domainName, bool readAssemblyAttributes)
		{
			if (domainAssembly == null)
			{
				throw new ArgumentNullException("domainAssembly");
			}

			// If the type is not set then use the default type
			if (repositoryType == null)
			{
				repositoryType = m_defaultRepositoryType;
			}

			lock(this)
			{
				// Lookup in map
				ILoggerRepository rep = m_assembly2repositoryMap[domainAssembly] as ILoggerRepository;
				if (rep == null)
				{
					// Not found, therefore create
					LogLog.Debug("DefaultRepositorySelector: Creating repository for assembly [" + domainAssembly + "]");

					// Must specify defaults
					string domain = domainName;
					Type actualRepositoryType = repositoryType;

					if (readAssemblyAttributes)
					{
						// Get the domain and type from the assembly attributes
						GetInfoForAssembly(domainAssembly, ref domain, ref actualRepositoryType);
					}

					LogLog.Debug("DefaultRepositorySelector: Assembly [" + domainAssembly + "] using domain [" + domain + "] and repository type [" + actualRepositoryType + "]");

					// Lookup the domain in the map (as this may already be defined)
					rep = m_domain2repositoryMap[domain] as ILoggerRepository;
					if (rep == null)
					{

						// Create the repository
						rep = CreateRepository(domain, actualRepositoryType);

						if (readAssemblyAttributes)
						{
							// Look for aliasing attributes
							LoadAliases(domainAssembly, rep);

							// Look for plugins defined on the assembly
							LoadPlugins(domainAssembly, rep);

							// Configure the repository using the assembly attributes
							ConfigureRepository(domainAssembly, rep);
						}
					}
					else
					{
						LogLog.Debug("DefaultRepositorySelector: domain [" + domain + "] already exisits, using repository type [" + rep.GetType().FullName + "]");

						if (readAssemblyAttributes)
						{
							// Look for plugins defined on the assembly
							LoadPlugins(domainAssembly, rep);
						}
					}
					m_assembly2repositoryMap[domainAssembly] = rep;
				}
				return rep;
			}
		}

		/// <summary>
		/// Create a new repository for the domain specified
		/// </summary>
		/// <param name="domain">the domain to associate with the <see cref="ILoggerRepository"/></param>
		/// <param name="repositoryType">the type of repository to create, must implement <see cref="ILoggerRepository"/>.
		/// If this param is null then the default repository type is used.</param>
		/// <returns>the repository created</returns>
		/// <remarks>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetRepository(string)"/> with the
		/// same domain specified will return the same repository instance.
		/// </remarks>
		/// <exception cref="ArgumentNullException">throw if <paramref name="domain"/> is null</exception>
		/// <exception cref="LogException">throw if the <paramref name="domain"/> already exists</exception>
		public ILoggerRepository CreateRepository(string domain, Type repositoryType)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}

			// If the type is not set then use the default type
			if (repositoryType == null)
			{
				repositoryType = m_defaultRepositoryType;
			}

			lock(this)
			{
				ILoggerRepository rep = null;

				// First check that the domain does not exist
				rep = m_domain2repositoryMap[domain] as ILoggerRepository;
				if (rep != null)
				{
					throw new LogException("Domain [" + domain + "] is already defined. Domains cannot be redefined.");
				}
				else
				{
					// Lookup an alias before trying to create the new domain
					ILoggerRepository aliasedRepository = m_alias2repositoryMap[domain] as ILoggerRepository;
					if (aliasedRepository != null)
					{
						// Found an alias

						// Check repository type
						if (aliasedRepository.GetType() == repositoryType)
						{
							// Repository type is compatable
							LogLog.Debug("DefaultRepositorySelector: Aliasing domain [" + domain + "] to existing repository [" + aliasedRepository.Name + "]");
							rep = aliasedRepository;

							// Store in map
							m_domain2repositoryMap[domain] = rep;
						}
						else
						{
							// Invalid repository type for alias
							LogLog.Error("DefaultRepositorySelector: Failed to alias domain [" + domain + "] to existing repository ["+aliasedRepository.Name+"]. Requested repository type ["+repositoryType.FullName+"] is not compatable with existing type [" + aliasedRepository.GetType().FullName + "]");

							// We now drop through to create the domain without aliasing
						}
					}

					// If we could not find an alias
					if (rep == null)
					{
						LogLog.Debug("DefaultRepositorySelector: Creating repository for domain [" + domain + "] using type [" + repositoryType + "]");

						// Call the no arg constructor for the repositoryType
						rep = (ILoggerRepository) repositoryType.GetConstructor(SystemInfo.EmptyTypes).Invoke(BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture);

						// Set the name of the repository
						rep.Name = domain;

						// Store in map
						m_domain2repositoryMap[domain] = rep;

						// Notify listeners that the repository has been created
						FireLoggerRepositoryCreatedEvent(rep);
					}
				}

				return rep;
			}
		}

		/// <summary>
		/// Copy the list of <see cref="ILoggerRepository"/> objects
		/// </summary>
		/// <returns>an array of all known <see cref="ILoggerRepository"/> objects</returns>
		public ILoggerRepository[] GetAllRepositories()
		{
			lock(this)
			{
				ICollection reps = m_domain2repositoryMap.Values;
				ILoggerRepository[] all = new ILoggerRepository[reps.Count];
				reps.CopyTo(all, 0);
				return all;
			}
		}

		#endregion Implementation of IRepositorySelector

		#region Public Instance Methods

		/// <summary>
		/// Alias a domain to an existing repository.
		/// </summary>
		/// <param name="domain">The domain to alias.</param>
		/// <param name="repository">The repository that the domain is aliased to.</param>
		/// <remarks>
		/// <para>
		/// Aliases a domain to an existing repository.
		/// </para>
		/// <para>
		/// The domain specified will be aliased to the repository when created. 
		/// The domain must not already exist.
		/// </para>
		/// <para>
		/// When the domain is created it must utilise the same reporitory type as 
		/// the domain it is aliased to, otherwise the aliasing will fail.
		/// </para>
		/// </remarks>
		public void AliasRepository(string domain, ILoggerRepository repository) 
		{
			if (domain == null) 
			{
				throw new ArgumentNullException("domain");
			}
			if (repository == null) 
			{
				throw new ArgumentNullException("repository");
			}

			lock(this) 
			{
				// Check if the domain is already aliased
				if (m_alias2repositoryMap.Contains(domain)) 
				{
					// Check if this is a duplicate of the current alias
					if (repository != ((ILoggerRepository)m_alias2repositoryMap[domain])) 
					{
						// Cannot redefine existing alias
						throw new InvalidOperationException("Domain [" + domain + "] is already aliased to repository [" + ((ILoggerRepository)m_alias2repositoryMap[domain]).Name + "]. Aliases cannot be redefined.");
					}
				}
					// Check if the domain is already mapped to a repository
				else if (m_domain2repositoryMap.Contains(domain)) 
				{
					// Check if this is a duplicate of the current mapping
					if ( repository != ((ILoggerRepository)m_domain2repositoryMap[domain]) ) 
					{
						// Cannot define alias for already mapped domain
						throw new InvalidOperationException("Domain [" + domain + "] already exists and cannot be aliased to repository [" + repository.Name + "].");
					}
				}
				else 
				{
					// Set the alias
					m_alias2repositoryMap[domain] = repository;
				}
			}
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Notifies the registered listeners that the repository has been created.
		/// </summary>
		/// <param name="repository">The repository that has been created</param>
		protected void FireLoggerRepositoryCreatedEvent(ILoggerRepository repository) 
		{
			if (m_loggerRepositoryCreatedEvent != null) 
			{
				m_loggerRepositoryCreatedEvent(this, new LoggerRepositoryCreationEventArgs(repository));
			}
		}

		#endregion Protected Instance Methods

		#region Private Instance Methods

		/// <summary>
		/// Get the domain and repository type for the specified assembly
		/// </summary>
		/// <param name="assembly">the assembly that has a <see cref="log4net.Config.DomainAttribute"/></param>
		/// <param name="domain">in/out param to hold the domain to use for the assembly, caller should set this to the default value before calling</param>
		/// <param name="repositoryType">in/out param to hold the type of the repository to create for the domain, caller should set this to the default value before calling</param>
		private void GetInfoForAssembly(Assembly assembly, ref string domain, ref Type repositoryType)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}

			LogLog.Debug("DefaultRepositorySelector: Assembly [" + assembly.FullName + "] Loaded From [" + SystemInfo.AssemblyLocationInfo(assembly) + "]");

			// Look for the DomainAttribute on the assembly 
			object[] domainAttributes = Attribute.GetCustomAttributes(assembly, typeof(log4net.Config.DomainAttribute), false);
			if (domainAttributes == null || domainAttributes.Length == 0)
			{
				// This is not a problem, but its nice to know what is going on.
				LogLog.Debug("DefaultRepositorySelector: Assembly [" + assembly + "] does not have a DomainAttribute specified.");
			}
			else
			{
				if (domainAttributes.Length > 1)
				{
					LogLog.Error("DefaultRepositorySelector: Assembly [" + assembly + "] has multiple log4net.Config.DomainAttribute assembly attributes. Only using first occurrence.");
				}

				log4net.Config.DomainAttribute domAttr = domainAttributes[0] as log4net.Config.DomainAttribute;

				if (domAttr == null)
				{
					LogLog.Error("DefaultRepositorySelector: Assembly [" + assembly + "] has a DomainAttribute but it does not!.");
				}
				else
				{
					// If the Name property is set then override the default
					if (domAttr.Name != null)
					{
						domain = domAttr.Name;
					}

					// If the RepositoryType property is set then override the default
					if (domAttr.RepositoryType != null)
					{
						// Check that the type is a repository
						if (typeof(ILoggerRepository).IsAssignableFrom(domAttr.RepositoryType))
						{
							repositoryType = domAttr.RepositoryType;
						}
						else
						{
							LogLog.Error("DefaultRepositorySelector: Repository Type [" + domAttr.RepositoryType + "] must implement the ILoggerRepository interface.");
						}
					}
				}
			}
		}

		/// <summary>
		/// Configure the repository using information from the assembly
		/// </summary>
		/// <param name="assembly">The assembly containing <see cref="log4net.Config.ConfiguratorAttribute"/>
		/// attributes which define the configuration for the repository</param>
		/// <param name="repository">the repository to configure</param>
		private void ConfigureRepository(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}

			// Look for the Configurator attributes (eg. DOMConfiguratorAttribute) on the assembly
			object[] configAttributes = Attribute.GetCustomAttributes(assembly, typeof(log4net.Config.ConfiguratorAttribute), false);
			if (configAttributes != null && configAttributes.Length > 0)
			{
				// Delegate to the attribute the job of configuring the repository
				foreach(log4net.Config.ConfiguratorAttribute configAttr in configAttributes)
				{
					configAttr.Configure(assembly, repository);
				}
			}
		}

		/// <summary>
		/// Load the attribute defined plugins on the assembly
		/// </summary>
		/// <param name="assembly">the assembly that contains the attributes</param>
		/// <param name="repository">the repository to alias to</param>
		private void LoadPlugins(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}

			// Look for the PluginAttribute on the assembly
			object[] configAttributes = Attribute.GetCustomAttributes(assembly, typeof(log4net.Config.PluginAttribute), false);
			if (configAttributes != null && configAttributes.Length > 0)
			{
				foreach(log4net.Plugin.IPluginFactory configAttr in configAttributes)
				{
					try
					{
						// Create the plugin and add it to the repository
						repository.PluginMap.Add(configAttr.CreatePlugin());
					}
					catch(Exception ex)
					{
						LogLog.Error("DefaultRepositorySelector: Failed to create plugin. Attribute [" + configAttr.ToString() + "]", ex);
					}
				}
			}
		}

		/// <summary>
		/// Load the attribute defined aliases on the assembly
		/// </summary>
		/// <param name="assembly">the assembly that contains the attributes</param>
		/// <param name="repository">the repository to alias to</param>
		private void LoadAliases(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}

			// Look for the AliasDomainAttribute on the assembly
			object[] configAttributes = Attribute.GetCustomAttributes(assembly, typeof(log4net.Config.AliasDomainAttribute), false);
			if (configAttributes != null && configAttributes.Length > 0)
			{
				foreach(log4net.Config.AliasDomainAttribute configAttr in configAttributes)
				{
					try
					{
						AliasRepository(configAttr.Name, repository);
					}
					catch(Exception ex)
					{
						LogLog.Error("DefaultRepositorySelector: Failed to alias domain [" + configAttr.Name + "]", ex);
					}
				}
			}
		}

		#endregion Private Instance Methods

		#region Private Static Fields

		private const string DEFAULT_DOMAIN_NAME = "log4net-default-domain";

		#endregion Private Static Fields

		#region Private Instance Fields

		private IDictionary m_domain2repositoryMap = new Hashtable();
		private IDictionary m_assembly2repositoryMap = new Hashtable();
		private IDictionary m_alias2repositoryMap = new Hashtable();
		private Type m_defaultRepositoryType;

		private event LoggerRepositoryCreationEventHandler m_loggerRepositoryCreatedEvent;

		#endregion Private Instance Fields
	}
}
