using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WolfeReiter.AntiVirus;

namespace WolfeReiter.AntiVirus.ClamAV
{
	public class WRClamdScan
	{
		public const int CLAMD_PORT		 = 3310;
		public const string CLAMD_HOST	 = "localhost";
		public static void Main()
		{	
			//TODO: Implement switches rather than an infinite "scan this" loop
			log4net.Config.DOMConfigurator.Configure();
			WriteVersion();
			IVirScanAgent agent = new ClamdStreamAgent(CLAMD_HOST, CLAMD_PORT, true);
			Console.WriteLine(agent.Version);
			while (true)
			{
				Console.Write("File or directory to scan: ");
				string filestr = Console.ReadLine();
				if(filestr==null || filestr==string.Empty)
					return;
				else
				{
					FileSystemInfo file = new DirectoryInfo(filestr);
					if(!file.Exists)
						file = new FileInfo(filestr);
					agent.Scan( file, true );
				}
			}
		}

		public static void WriteVersion()
		{
			string title=null, /*descr=null, copyright=null,*/ version;
		
			Assembly assembly = Assembly.GetEntryAssembly();
		
			version = assembly.GetName().Version.ToString();
			object[] titles = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute),true);
//			object[] descrs = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute),true);
//			object[] copyrights = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute),true);
			if(titles.Length>0)
				title = ((AssemblyTitleAttribute)titles[0]).Title;
//			if(descrs.Length>0)
//				descr = ((AssemblyDescriptionAttribute)descrs[0]).Description;
//			if(copyrights.Length>0)
//				copyright = ((AssemblyCopyrightAttribute)copyrights[0]).Copyright;

//			Console.WriteLine(string.Format("{0} {1}\n{2}\n{3}\n", title, version, descr, copyright));
			Console.WriteLine(string.Format("{0} version {1}", title, version));
		}

	}
}