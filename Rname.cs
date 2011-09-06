using System;
using System.IO;
using System.Collections.Generic;

namespace rname
{
	class rName
	{
		string strOld, strNew;
		
		bool flgVerbose = false, 
			flgRecurse = false,
			flgFiles = true,
			flgDirectory = false;
		
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
				if (listArgs[0].StartsWith("--"))
				{
					listArgs[0] = listArgs[0].Remove(0, 2);
					
					switch (listArgs[0].ToLower())
					{
						default: break;
						case "verbose": flgVerbose = true; break;
						case "recurse": flgRecurse = true; break;
						case "directory": flgDirectory = true; break;
						case "nofiles": flgFiles = false; break;
						case "help": showHelp(); break;
					}
				}
				else if (listArgs[0].StartsWith("-"))
				{
					listArgs[0] = listArgs[0].Remove(0, 1);
					
					while (listArgs[0].Length > 0)
					{
						switch (listArgs[0][0])
						{
							default: break;
							case 'v': flgVerbose = true; break;
							case 'r': flgRecurse = true; break;
							case 'd': flgDirectory = true; break;
							case 'D': flgDirectory = true; flgFiles = false; break;
							case 'h': showHelp(); break;
						}
						
						listArgs[0] = listArgs[0].Remove(0, 1);
					}
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
			{
				if (flgFiles)
					fileRename (new FileInfo(incFile));
			}
			else if (Directory.Exists(incFile))
			{
				if (flgRecurse)
				{
					foreach (string eachFile in Directory.GetFiles(incFile)) fileTouch(eachFile);
					foreach (string eachFile in Directory.GetDirectories(incFile)) fileTouch(eachFile);
				}
				
				if (flgDirectory)
					dirRename (new DirectoryInfo(incFile));
			}
						
		}
		
		public void fileRename(FileInfo incFile)
		{
			if (incFile.Name.Contains(strOld))
			{
				string newFile = Path.Combine(incFile.Directory.FullName, incFile.Name.Replace(strOld, strNew));
				
				if (!File.Exists(newFile))
					try 
				    {
						string oldFile = incFile.FullName;
						incFile.MoveTo(newFile);
					
						if (flgVerbose)
							Console.WriteLine(String.Format("{0} -> {1}", oldFile, newFile));
					}
					catch { Console.WriteLine(String.Format("Error: unable to rename, check permissions: {0}", incFile.FullName)); }
				else
					Console.WriteLine(String.Format("Error: file already exists: {0}", 
						Path.Combine(incFile.Directory.FullName, newFile)));
			}
		}
		
		public void dirRename(DirectoryInfo incDir)
		{
			if (incDir.Name.Contains(strOld))
			{
				string newDir = Path.Combine(incDir.Parent.FullName, incDir.Name.Replace(strOld, strNew));
				
				if (!Directory.Exists(newDir))
					try 
				    {
						string oldDir = incDir.FullName;
						incDir.MoveTo(newDir);
					
						if (flgVerbose)
							Console.WriteLine(String.Format("{0} -> {1}", oldDir, newDir));
					}
					catch { Console.WriteLine(String.Format("Error: unable to rename, check permissions: {0}", incDir.FullName)); }
				else
					Console.WriteLine(String.Format("Error: directory already exists: {0}", 
						Path.Combine(incDir.Parent.FullName, newDir)));
			}
		}
		
		public void showSyntax()
		{
			Console.WriteLine("Usage: rname [-vrdD] old-expr new-expr [filenames]");
			Environment.Exit(0);
		}
		
		public void showHelp()
		{
			Console.WriteLine("Usage: rname [-vrdD] old-expr new-expr [filenames]");
			Console.WriteLine("Renames part or the whole filename for multiple files.");
			Console.WriteLine("Portions of file names equaling the old expression (old-expr) are renamed ");
			Console.WriteLine("to the new expression (new-expr).");
			Console.WriteLine("");
			Console.WriteLine("Arguments can include:");
			Console.WriteLine("    -r, --recurse\t\trecurse through subdirectories");
			Console.WriteLine("    -v, --verbose\t\tprints additional output on actions");
			Console.WriteLine("    -d, --directory\t\talso parses and renames directories");
			Console.WriteLine("        --nofiles\t\texcludes files, to be used with --directories");
			Console.WriteLine("    -D	\t\t\trenames directories and not files (equal to");
			Console.WriteLine("       	\t\t\t--directories --nofiles)");
			Environment.Exit(0);
		}
	}
}

