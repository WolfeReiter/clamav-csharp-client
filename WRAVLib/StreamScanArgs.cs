using System;
using System.IO;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// StreamScanArgs is a data transport structure that defines arguments for use with a Byte[] scan.
	/// </summary>
	public struct StreamScanArgs
	{
		private const string STR_ID_NULL = "StreamScanArgs.ID_NULL";

		private string _id;
		private Stream _stream;
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="id">Byte[] buffer.</param>
		/// <param name="stream">Unique identifier for the buffer (eg. filename, url or database key, etc.)</param>
		public StreamScanArgs(string id, Stream stream)
		{
			if(id==null)
				throw new ArgumentNullException( "id", ResourceManagers.Strings.GetString(STR_ID_NULL) );

			_stream = stream;
			_id	 = id;
		}
		/// <summary>
		/// Get-only. Stream.
		/// </summary>
		public Stream Stream 
		{ 
			get
			{
				if (_stream==null)
					_stream = new MemoryStream( new byte[0] );
		
				return _stream;  
			}
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
			if( !(obj is StreamScanArgs) )
				return false;

			bool ideq = this.Id==((StreamScanArgs)obj).Id;
			if(!ideq)
				return false;

			return  this.Stream==((StreamScanArgs)obj).Stream;
		}
		/// <summary>
		/// Override bas GetHashCode method.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			string hashstr = _id + Stream.ToString();
			return hashstr.GetHashCode();
		}

		/// <summary>
		/// Equality comparison operator.
		/// </summary>
		/// <param name="args0"></param>
		/// <param name="args1"></param>
		/// <returns></returns>
		public static bool operator ==(StreamScanArgs args0, StreamScanArgs args1)
		{
			return args0.Equals(args1);
		}
		/// <summary>
		/// Inequality comparison operator.
		/// </summary>
		/// <param name="args0"></param>
		/// <param name="args1"></param>
		/// <returns></returns>
		public static bool operator !=(StreamScanArgs args0, StreamScanArgs args1)
		{
			return !args0.Equals(args1);
		}
	}
}
