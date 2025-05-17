using System;
using BoLang.Parser;
using BoLang.AST;

class Program
{
	static void Main()
	{
		var parser = new Parser();
		Console.WriteLine("\nRepl v0.1");

		// Run REPL loop
		while (true)
		{
			Console.Write("> ");
			var input = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(input) || input.Contains("exit"))
			{
				Environment.Exit(0);
			}

			try
			{
				ProgramNode program = parser.ProduceAST(input);
				PrintAST(program);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}

	static void PrintAST(ProgramNode program)
	{
		Console.WriteLine("AST:");
		foreach (var stmt in program.Body)
		{
			PrintNode(stmt, 1);
		}
	}

	static void PrintNode(Stmt node, int indent)
	{
		var prefix = new string(' ', indent * 2);

		switch (node)
		{
			case BinaryExpr bin:
				Console.WriteLine($"{prefix}BinaryExpr (op: {bin.Operator})");
				PrintNode(bin.Left, indent + 1);
				PrintNode(bin.Right, indent + 1);
				break;

			case NumericLiteral num:
				Console.WriteLine($"{prefix}NumericLiteral: {num.Value}");
				break;

			case Identifier id:
				Console.WriteLine($"{prefix}Identifier: {id.Symbol}");
				break;

			default:
				Console.WriteLine($"{prefix}Unknown node: {node.GetType().Name}");
				break;
		}
	}
}
