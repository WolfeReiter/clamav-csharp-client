using System;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// ByteScanArgs is a data transport structure that defines arguments for use with a Byte[] scan.
	/// </summary>
	public struct ByteScanArgs
	{
		private const string STR_ID_NULL = "ByteScanArgs.ID_NULL";

		private string _id;
		private byte[] _buff;
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="id">Byte[] buffer.</param>
		/// <param name="buff">Unique identifier for the buffer (eg. filename, url or database key, etc.)</param>
		public ByteScanArgs(string id, byte[] buff)
		{
			if(id==null)
				throw new ArgumentNullException( "id", ResourceManagers.Strings.GetString(STR_ID_NULL) );

			_buff = buff;
			_id	 = id;
		}
		/// <summary>
		/// Get-only. Byte[] buffer.
		/// </summary>
		public byte[] GetBuffer() 
		{ 
			if (_buff==null)
			  _buff = new Byte[0];
		
			return _buff;  
		}
		/// <summary>
		/// Get-only. Unique identifier for the buffer (eg. filename, url or database key, etc.)
		/// </summary>
		public string Id { get { return _id; } }

		/// <summary>
		/// Override base Equals method.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if( !(obj is ByteScanArgs) )
				return false;

			bool ideq = this.Id==((ByteScanArgs)obj).Id;
			if(!ideq)
				return false;

			return  this.GetBuffer()==((ByteScanArgs)obj).GetBuffer();
		}
		/// <summary>
		/// Override bas GetHashCode method.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			string hashstr = _id + GetBuffer().ToString();
			return hashstr.GetHashCode();
		}

		/// <summary>
		/// Equality comparison operator.
		/// </summary>
		/// <param name="args0"></param>
		/// <param name="args1"></param>
		/// <returns></returns>
		public static bool operator ==(ByteScanArgs args0, ByteScanArgs args1)
		{
			return args0.Equals(args1);
		}
		/// <summary>
		/// Inequality comparison operator.
		/// </summary>
		/// <param name="args0"></param>
		/// <param name="args1"></param>
		/// <returns></returns>
		public static bool operator !=(ByteScanArgs args0, ByteScanArgs args1)
		{
			return !args0.Equals(args1);
		}
	}
}
