using System;
using System.IO;
using System.Collections.Generic;

namespace rname
{
	class rName
	{
		string strOld, strNew;
		
		bool flgVerbose = false, 
			flgRecurse = false;
		
		List<string> listFiles = new List<string>(),
			listArgs = new List<string>();
		
		
		public static void Main (string[] args)
		{
			rName progInstance = new rName();
			progInstance.listArgs.AddRange(args);
			progInstance.Run();
		}
		
		public void Run()
		{
			if (listArgs.Count == 0)
				showSyntax();
			
			while (listArgs[0].StartsWith("-"))
			{
				while (listArgs[0].StartsWith("-")) 
					listArgs[0] = listArgs[0].Remove(0, 1);
				
				switch (listArgs[0].ToLower())
				{
					default: break;
					case "v": case "verbose": flgVerbose = true; break;
					case "r": case "recurse": flgRecurse = true; break;
					case "h": case "help": showHelp(); break;
				}
				
				listArgs.RemoveAt(0);
			}
			
			if (listArgs.Count < 3)
			{
				Console.WriteLine("Error: Not enough arguments provided. Must include at least three arguments");
				Environment.Exit(0);
			}
			
			strOld = listArgs[0]; strNew = listArgs[1];
			for (int i = 2; i < listArgs.Count; i++)
				listFiles.Add(listArgs[i]);
			
			foreach (string eachFile in listFiles)
				fileTouch(eachFile);
		}
		
		public void fileTouch(string incFile)
		{
			if (File.Exists(incFile))
					fileRename (new FileInfo(incFile));
				else if (Directory.Exists(incFile))
					if (flgRecurse)
					{
						foreach (string eachFile in Directory.GetFiles(incFile)) fileTouch(eachFile);
						foreach (string eachFile in Directory.GetDirectories(incFile)) fileTouch(eachFile);
					}
						
		}
		public void fileRename(FileInfo incFile)
		{
			if (incFile.Name.Contains(strOld))
			{
				string newFile = incFile.Name.Replace(strOld, strNew);
				
				if (!File.Exists(newFile))
					try 
				    {
						string oldFile = incFile.FullName;
						incFile.MoveTo(Path.Combine(incFile.Directory.FullName, newFile));
					
						if (flgVerbose)
							Console.WriteLine(String.Format("{0} -> {1}", oldFile, newFile));
					}
					catch { Console.WriteLine(String.Format("Error: unable to rename, check permissions: {0}", incFile.FullName)); }
				else
					Console.WriteLine(String.Format("Error: file already exists: {0}", 
						Path.Combine(incFile.Directory.FullName, newFile)));
			}
		}
		
		public void showSyntax()
		{
			Console.WriteLine("Usage: rname [-v] [-r] old-expr new-expr [filenames]");
			Environment.Exit(0);
		}
		
		public void showHelp()
		{
			Console.WriteLine("Usage: rname [-v] [-r] old-expr new-expr [filenames]");
			Console.WriteLine("Renames part or the whole filename for multiple files.");
			Console.WriteLine("Portions of file names equaling the old expression (old-expr) are renamed ");
			Console.WriteLine("to the new expression (new-expr).");
			Console.WriteLine("");
			Console.WriteLine("Arguments can include:");
			Console.WriteLine("    -r, --recurse\t\trecurse through subdirectories");
			Console.WriteLine("    -v, --verbose\t\tprints additional output on actions");
			Environment.Exit(0);
		}
	}
}

