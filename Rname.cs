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
			flgDirectory = false,
			flgFirst = false,
			flgLast = false,
			flgTrim = false,
			flgAppend = false,
			flgExcise = false;
		
		int cntFirst = 1,
			cntLast = 1;
		
		List<string> listArgs = new List<string>();
		
		enmCase toCase = enmCase.None;
		
		enum enmCase
		{ None, Upper, Lower, Title }
		
		public static void Main (string[] args)
		{
			rName progInstance = new rName();
			progInstance.listArgs.AddRange(args);
			progInstance.Run();
		}
		
		public void Run()
		{
			bool processedFlags = false;
			
			if (listArgs.Count == 0)
				showSyntax();
			
			for ( ; (listArgs.Count > 0) && (listArgs[0].StartsWith("-")) ; listArgs.RemoveAt(0))
			{
				if (listArgs[0].StartsWith("--"))
				{
					listArgs[0] = listArgs[0].Remove(0, 2);
					
					switch (listArgs[0])
					{
						default: break;
						case "verbose": flgVerbose = true; break;
						case "recurse": flgRecurse = true; break;
						case "directory": flgDirectory = true; break;
						case "nofiles": flgFiles = false; break;
						case "first": flgFirst = true; break;
						case "last": flgLast = true; break;
						case "trim": flgTrim = true; break;
						case "upper": toCase = enmCase.Upper; break;
						case "lower": toCase = enmCase.Lower; break;
						case "title": toCase = enmCase.Title; break;
						case "append": flgAppend = true; break;
						case "excise": flgExcise = true; break;
						case "help": showHelp(); break;
					}
				}
				else if (!processedFlags && listArgs[0].StartsWith("-"))
				{
					processedFlags = true;
					
					listArgs[0] = listArgs[0].Remove(0, 1);
					
					for ( ; listArgs[0].Length > 0 ; listArgs[0] = listArgs[0].Remove(0, 1))
						switch (listArgs[0][0])
						{
							default: break;
							case 'v': flgVerbose = true; break;
							case 'r': flgRecurse = true; break;
							case 'd': flgDirectory = true; break;
							case 'D': flgDirectory = true; flgFiles = false; break;
							case 'U': toCase = enmCase.Upper; break;
							case 'L': toCase = enmCase.Lower; break;
							case 'T': toCase = enmCase.Title; break;	
							case 't': flgTrim = true; break;
							case 'a': flgAppend = true; break;
							case 'x': flgExcise = true; break;
							case 'h': showHelp(); break;
							
							case 'f': flgFirst = true;
								if ((listArgs[0].Length > 1) && (evalInt(listArgs[0].Substring(1, 1)) > -1))
								{
									for (int i = 1, j = 0; (listArgs[0].Length > i) && ((j = evalInt(listArgs[0].Substring(1, i))) > -1); i++)
										cntFirst = j; 
									listArgs[0] = listArgs[0].Remove(0, cntFirst.ToString().Length);
								}
								break;
							case 'l': flgLast = true; 
								if ((listArgs[0].Length > 1) && (evalInt(listArgs[0].Substring(1, 1)) > -1))
								{
									for (int i = 1, j = 0; (listArgs[0].Length > i) && ((j = evalInt(listArgs[0].Substring(1, i))) > -1); i++)
										cntLast = j; 
									listArgs[0] = listArgs[0].Remove(0, cntLast.ToString().Length);
								}
								break;
						}
				}
				else if (processedFlags)
					break;
			}
			
			if (listArgs.Count < 3)
			{
				Console.WriteLine("Error: Not enough arguments provided. Must include at least three arguments");
				Environment.Exit(0);
			}
				
			strOld = listArgs[0]; 
			strNew = listArgs[1];
			
			for (int i = 2; i < listArgs.Count; i++)
				fileTouch(listArgs[i]);
		}
		
		public int evalInt(string incString)
		{
			int tOut;
				
			if (int.TryParse(incString, out tOut))
				return tOut;
			
			return -1;
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
			string newFile = incFile.Name,
				oldFile = incFile.FullName;
			
			parseAll(ref newFile);
			
			newFile = Path.Combine(incFile.Directory.FullName, newFile);
				
			if (newFile == oldFile)
				return;
			
			if (!File.Exists(newFile))
				try 
			    {
					incFile.MoveTo(newFile);
				
					if (flgVerbose)
						Console.WriteLine(String.Format("{0} -> {1}", oldFile, newFile));
				}
				catch { Console.WriteLine(String.Format("Error: unable to rename, check permissions: {0}", incFile.FullName)); }
			else
				Console.WriteLine(String.Format("Error: file already exists: {0}", 
					Path.Combine(incFile.Directory.FullName, newFile)));
		}
		
		public void dirRename(DirectoryInfo incDir)
		{
			string newDir = incDir.Name,
				oldDir = incDir.FullName;
			
			parseAll(ref newDir);
			
			newDir = Path.Combine(incDir.Parent.FullName, newDir);
			
			if (newDir == oldDir)
				return;
			
			if (!Directory.Exists(newDir))
				try 
			    {
					incDir.MoveTo(newDir);
				
					if (flgVerbose)
						Console.WriteLine(String.Format("{0} -->> {1}", oldDir, newDir));
				}
				catch { Console.WriteLine(String.Format("Error: unable to rename, check permissions: {0}", incDir.FullName)); }
			else
				Console.WriteLine(String.Format("Error: directory already exists: {0}", 
					Path.Combine(incDir.Parent.FullName, newDir)));
		}
		
		public void parseAll(ref string incString)
		{
			if (flgAppend)
				incString = strOld + incString + strNew;
			else if (flgExcise)
			{
				int exBegin = evalInt(strOld), exEnd = evalInt(strNew);
				
				if ((exBegin < 0) || (exEnd < 0))
					Console.WriteLine("Arguments must be integers");
				else if ((exBegin + exEnd) >= incString.Length)
					Console.WriteLine(String.Format("Error: not enough letters in filename: {0}", incString));
				else
					incString = incString.Remove(incString.Length - exEnd, exEnd).Remove(0, exBegin);
			}
			else
			{
				if (flgFirst || flgLast)
					parseEnds(ref incString);
				else
					incString = incString.Replace(strOld, strNew);
			}
				
			parseTrim(ref incString);
			parseCase(ref incString);
		}
		
		public void parseEnds(ref string incString)
		{
			if (flgFirst)
				for (int i = 0; i < cntFirst; i++)
				{
					int iPos = incString.IndexOf(strOld);
					
					if (iPos > -1) 
						incString = incString.Remove(iPos, strOld.Length).Insert(iPos, strNew);
					else break;
				}
			
			if (flgLast)
				for (int i = 0; i < cntLast; i++)
				{
					int iPos = incString.LastIndexOf(strOld);
					
					if (iPos > -1) 
						incString = incString.Remove(iPos, strOld.Length).Insert(iPos, strNew);
					else break;
				}
		}
		
		public void parseTrim(ref string incString)
		{
			if (!flgTrim)
				return;
			
			string outString = "";
			List<string> splitString = new List<string>(incString.Split(new char[] {' '}));
			
			for (int i = 0; i < splitString.Count; i++)
				if (splitString[i].Length > 0)
				{
					outString += splitString[i].Trim();
					if (i < splitString.Count - 1)
						outString += " ";
				}
			
			incString = outString.Trim();
		}
		
		public void parseCase (ref string incString)
		{
			switch (toCase)
			{
				default: return;
				case enmCase.Upper: incString = caseUpper(incString); break;
				case enmCase.Lower: incString = caseLower(incString); break;
				case enmCase.Title: incString = caseTitle(incString); break;
			}
		}
		
		public string caseUpper(string incString)
		{ return incString.ToUpper(); }
		
		public string caseLower(string incString)
		{ return incString.ToLower(); }
		
		public string caseTitle(string incString)
		{
			string outString = "";
			List<string> splitString = new List<string>(incString.Split(new char[] {' '}));
			
			for (int i = 0; i < splitString.Count; i++)
				if (splitString[i].Length > 0)
					outString += splitString[i][0].ToString().ToUpper() + splitString[i].Remove(0, 1).ToLower() + " ";
			
			return outString.Trim();
		}
		
		public void showSyntax()
		{
			Console.WriteLine("Usage: rname [-vrdDULTtaxfl#] old-expr new-expr [filenames]");
			Console.WriteLine("       rname [-vrdD]ULTt x x [filenames]");
			Environment.Exit(0);
		}
		
		public void showHelp()
		{
			Console.WriteLine("Usage: rname [-vrdDULTtaxfl#] old-expr new-expr [filenames]");
			Console.WriteLine("       rname [-vrdD]ULTt x x [filenames]");
			Console.WriteLine("Renames part or the whole filename for multiple files.");
			Console.WriteLine("Portions of file names equaling the old expression (old-expr) are renamed ");
			Console.WriteLine("to the new expression (new-expr).");
			Console.WriteLine("");
			Console.WriteLine("Arguments can include:");
			Console.WriteLine("    -r, --recurse\trecurse through subdirectories");
			Console.WriteLine("    -v, --verbose\tprints additional output on actions");
			Console.WriteLine("    -d, --directory\talso parses and renames directories");
			Console.WriteLine("        --nofiles\texcludes files, to be used with --directories");
			Console.WriteLine("    -D	\t\trenames directories and not files (equal to");
			Console.WriteLine("       	\t\t--directories --nofiles)");
			Console.WriteLine("    -t, --trim\t\ttrims all extra white space in filename");
			Console.WriteLine("");
			Console.WriteLine("Append, Excise:");
			Console.WriteLine("Inserting, appending to, or excising from a filename");
			Console.WriteLine("    -a, --append\tadds the first expression to the beginning of the");
			Console.WriteLine("         \t\tfilename, appends second expression to the end of the");
			Console.WriteLine("         \t\tfilename, \"\" for null");
			Console.WriteLine("    -x, --excise\tremoves number of characters from beginning and");
			Console.WriteLine("         \t\tend of the filename, first expression is whole number");
			Console.WriteLine("         \t\tto remove from beginning, second expression is number");
			Console.WriteLine("         \t\tto remove from end (ie. rname -x 2 0 *)");
			Console.WriteLine("");
			Console.WriteLine("Case replacement:");
			Console.WriteLine("Changes case of words in all files selected, regardless of matching expression");
			Console.WriteLine("    -U, --upper\t\tconverts all selected files to UPPER case");
			Console.WriteLine("    -L, --lower\t\tconverts all selected files to lower case");
			Console.WriteLine("    -T, --title\t\tconverts all selected files to Title case");
			Console.WriteLine("");
			Console.WriteLine("Selective replacement:");
			Console.WriteLine("Defaults to 1 if no value is given, or if the long form switch is used");
			Console.WriteLine("    -f#, --first\treplace the first # of occurrences");
			Console.WriteLine("    -l#, --last\t\treplace the last # of occurrence");
			Console.WriteLine("");
			Console.WriteLine("Note: To distinguish between arguments and expressions, only one set of short");
			Console.WriteLine("arguments can be used (ie. correct: -vdtTf2 ; incorrect: -vdt -Tf2");
			Environment.Exit(0);
		}
	}
}

