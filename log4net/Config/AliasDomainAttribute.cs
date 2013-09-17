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

namespace log4net.Config
{
	/// <summary>
	/// Assembly level attribute that specifies a domain to alias to this assembly's repository.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An assembly's logger repository is defined by its <see cref="DomainAttribute"/>,
	/// however this can be overidden by an assembly loaded before the target assembly.
	/// </para>
	/// <para>
	/// An assembly can alias another assembly's domain to its repository by
	/// specifying this attribute with the name of the target domain.
	/// </para>
	/// <para>
	/// This attribute can only be specified on the assembly and may be used
	/// as many times as nessasary to alias all the required domains.
	/// </para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly,AllowMultiple=true)]
	[Serializable]
	public sealed class AliasDomainAttribute : Attribute
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AliasDomainAttribute" /> class with 
		/// the specified domain to alias to this assembly's repository.
		/// </summary>
		/// <param name="name">The domain to alias to this assemby's repository.</param>
		public AliasDomainAttribute(string name)
		{
			Name = name;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the domain to alias to this assemby's repository.
		/// </summary>
		/// <value>
		/// The domain to alias to this assemby's repository.
		/// </value>
		public string Name
		{
			get { return m_name; }
			set { m_name = value ; }
		}

		#endregion Public Instance Properties

		#region Private Instance Fields

		private string m_name = null;

		#endregion Private Instance Fields
	}
}
