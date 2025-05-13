using System;
using System.IO;
using Bolang;

class Program
{
	static void Main()
	{
		Console.WriteLine("Type 'run bolang' to run the system");
		string input = Console.ReadLine()?.Trim().ToLower();

		if (input == "run bolang")
		{
			string filePath = "C:\\xampp\\htdocs\\Projects\\BoLang\\BoLang\\BoLang\\code.txt"; // fix later and make it use relative path instead

			if (File.Exists(filePath))
			{
				string code = File.ReadAllText(filePath);
				var tokens = Lexer.Tokenize(code);

				foreach (var token in tokens)
				{
					Console.WriteLine(token);
				}
			}
			else
			{
				Console.WriteLine("Error: code.txt not found.");
			}
		}
		else
		{
			Console.WriteLine("Command not recognized. Exiting...");
		}
	}
}
