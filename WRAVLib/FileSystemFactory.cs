using System;
using System.Globalization;
using System.IO;
using System.Security;
using log4net;

namespace WolfeReiter.AntiVirus
{
	/// <summary>
	/// FileSystemFactory generates instances FileSystemInfo object from a file or directory name.
	/// </summary>
	public sealed class FileSystemFactory
	{
		#region resource name constants
		private const string STR_CREATE_INSTANCE_NULL_FILE_ARG	= "FileSystemFactory.CreateInstance_Debug-NullEmptyFileStr";
		private const string STR_CREATE_INSTANCE_NOT_FOUND_ERR	= "FileSystemFactory.CreateInstance_Error-NotFound";

		#endregion
		private FileSystemFactory(){}

		/// <summary>
		/// Generates a FileSystemInfo object from a file or directory name.
		/// </summary>
		/// <remarks>Performs modest validity checking. But does not guarantee that the file or directory will
		/// exist.</remarks>
		/// <param name="fileSystemItem">File or directory name</param>
		/// <returns>FileSystemInfo object or null if the string was invalid</returns>
		public static FileSystemInfo CreateInstance(string fileSystemItem)
		{
			ILog log = LogManager.GetLogger(typeof(FileSystemFactory));
			FileSystemInfo fileSystemInfo = null;

			if(fileSystemItem==null || fileSystemItem.Length==0)
			{
				log.Debug( ResourceManagers.Strings.GetString(STR_CREATE_INSTANCE_NULL_FILE_ARG) );
				return null;
			}
			else
			{
				try
				{
					fileSystemInfo = new DirectoryInfo( fileSystemItem );
					if( !fileSystemInfo.Exists )
						fileSystemInfo = new FileInfo( fileSystemItem );
					if( fileSystemInfo.Exists)
                        return fileSystemInfo;
					log.Error( string.Format( CultureInfo.CurrentCulture, ResourceManagers.Strings.GetString(STR_CREATE_INSTANCE_NOT_FOUND_ERR), fileSystemItem ) );
					return null;
				}
				catch(Exception ex)
				{
					if(log.IsDebugEnabled)
						log.Error(ex);
					else
						log.Error(ex.Message);
					//AgumentException, PathTooLongExcpetion and SecurityException are expected and shouldn't kill the application.
					if( ex is ArgumentException || ex is PathTooLongException || ex is SecurityException )
						return null;
					//anything else is truly catastrophic.
					throw ex;
				}
			}
		}
	}
}
