using System;
using System.Collections.Generic;

namespace BoLang.Parser
{

public enum TokenType
{
	EOF,
	Let,
	Const,
	Fn,
	Identifier,
	OpenBrace,
	CloseBrace,
	Equals,
	Semicolon,
	Comma,
	Colon,
	Number,
	OpenParen,
	CloseParen,
	Dot
}

public class Token
{
	public TokenType Type { get; set; }
	public string Value { get; set; }
}

public class Parser
{
	private List<Token> tokens = new List<Token>();

	// Determines if the parsing is complete and the END OF FILE is reached.
	private bool NotEOF()
	{
		return tokens[0].Type != TokenType.EOF;
	}

	// Returns the currently available token
	private Token At()
	{
		return tokens[0];
	}

	// Returns the previous token and then advances the tokens list to the next value.
	private Token Eat()
	{
		var prev = tokens[0];
		tokens.RemoveAt(0);
		return prev;
	}

	// Returns the previous token and then advances the tokens list to the next value.
	// Also checks the type of expected token and throws if the values don't match.
	private Token Expect(TokenType type, string err)
	{
		var prev = tokens[0];
		tokens.RemoveAt(0);
		if (prev == null || prev.Type != type)
		{
			Console.Error.WriteLine($"Parser Error:\n{err} {prev} - Expecting: {type}");
			Environment.Exit(1);
		}

		return prev;
	}

	public Program ProduceAST(string sourceCode)
	{
		tokens = Tokenize(sourceCode);
		var program = new Program
		{
			Kind = "Program",
			Body = new List<Stmt>()
		};

		// Parse until end of file
		while (NotEOF())
		{
			program.Body.Add(ParseStmt());
		}

		return program;
	}

	// Handle complex statement types
	private Stmt ParseStmt()
	{
		// Skip to parse_expr
		switch (At().Type)
		{
			case TokenType.Let:
			case TokenType.Const:
				return ParseVarDeclaration();
			case TokenType.Fn:
				return ParseFnDeclaration();
			default:
				return ParseExpr();
		}
	}

	private Stmt ParseFnDeclaration()
	{
		Eat(); // Eat fn keyword
		var name = Expect(TokenType.Identifier, "Expected function name following fn keyword").Value;

		var args = ParseArgs();
		var parameters = new List<string>();
		foreach (var arg in args)
		{
			if (arg.Kind != "Identifier")
			{
				Console.WriteLine(arg);
				throw new Exception("Inside function declaration expected parameters to be of type string.");
			}

			parameters.Add(((Identifier)arg).Symbol);
		}

		Expect(TokenType.OpenBrace, "Expected function body following declaration");
		var body = new List<Stmt>();

		while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
		{
			body.Add(ParseStmt());
		}

		Expect(TokenType.CloseBrace, "Closing brace expected inside function declaration");

		var fn = new FunctionDeclaration
		{
			Body = body,
			Name = name,
			Parameters = parameters,
			Kind = "FunctionDeclaration"
		};

		return fn;
	}

	// LET IDENT;
	// ( LET | CONST ) IDENT = EXPR;
	private Stmt ParseVarDeclaration()
	{
		var isConstant = Eat().Type == TokenType.Const;
		var identifier = Expect(TokenType.Identifier, "Expected identifier name following let | const keywords.").Value;

		if (At().Type == TokenType.Semicolon)
		{
			Eat(); // Expect semicolon
			if (isConstant)
			{
				throw new Exception("Must assign value to constant expression. No value provided.");
			}

			return new VarDeclaration
			{
				Kind = "VarDeclaration",
				Identifier = identifier,
				Constant = false
			};
		}

		Expect(TokenType.Equals, "Expected equals token following identifier in var declaration.");

		var declaration = new VarDeclaration
		{
			Kind = "VarDeclaration",
			Value = ParseExpr(),
			Identifier = identifier,
			Constant = isConstant
		};

		Expect(TokenType.Semicolon, "Variable declaration statement must end with semicolon.");

		return declaration;
	}

	// Handle expressions
	private Expr ParseExpr()
	{
		return ParseAssignmentExpr();
	}

	private Expr ParseAssignmentExpr()
	{
		var left = ParseObjectExpr();

		if (At().Type == TokenType.Equals)
		{
			Eat(); // Advance past equals
			var value = ParseAssignmentExpr();
			return new AssignmentExpr
			{
				Value = value,
				Assigne = left,
				Kind = "AssignmentExpr"
			};
		}

		return left;
	}

	private Expr ParseObjectExpr()
	{
		// { Prop[] }
		if (At().Type != TokenType.OpenBrace)
		{
			return ParseAdditiveExpr();
		}

		Eat(); // Advance past open brace.
		var properties = new List<Property>();

		while (NotEOF() && At().Type != TokenType.CloseBrace)
		{
			var key = Expect(TokenType.Identifier, "Object literal key expected").Value;

			// Allows shorthand key: pair -> { key, }
			if (At().Type == TokenType.Comma)
			{
				Eat(); // Advance past comma
				properties.Add(new Property { Key = key, Kind = "Property" });
				continue;
			}
			// Allows shorthand key: pair -> { key }
			else if (At().Type == TokenType.CloseBrace)
			{
				properties.Add(new Property { Key = key, Kind = "Property" });
				continue;
			}

			// { key: val }
			Expect(TokenType.Colon, "Missing colon following identifier in ObjectExpr");
			var value = ParseExpr();

			properties.Add(new Property { Kind = "Property", Value = value, Key = key });
			if (At().Type != TokenType.CloseBrace)
			{
				Expect(TokenType.Comma, "Expected comma or closing bracket following property");
			}
		}

		Expect(TokenType.CloseBrace, "Object literal missing closing brace.");
		return new ObjectLiteral { Kind = "ObjectLiteral", Properties = properties };
	}

	// Handle Addition & Subtraction Operations
	private Expr ParseAdditiveExpr()
	{
		var left = ParseMultiplicativeExpr();

		while (At().Value == "+" || At().Value == "-")
		{
			var operatorType = Eat().Value;
			var right = ParseMultiplicativeExpr();
			left = new BinaryExpr
			{
				Kind = "BinaryExpr",
				Left = left,
				Right = right,
				Operator = operatorType
			};
		}

		return left;
	}

	// Handle Multiplication, Division & Modulo Operations
	private Expr ParseMultiplicativeExpr()
	{
		var left = ParseCallMemberExpr();

		while (At().Value == "/" || At().Value == "*" || At().Value == "%")
		{
			var operatorType = Eat().Value;
			var right = ParseCallMemberExpr();
			left = new BinaryExpr
			{
				Kind = "BinaryExpr",
				Left = left,
				Right = right,
				Operator = operatorType
			};
		}

		return left;
	}

	// foo.x()()
	private Expr ParseCallMemberExpr()
	{
		var member = ParseMemberExpr();

		if (At().Type == TokenType.OpenParen)
		{
			return ParseCallExpr(member);
		}

		return member;
	}

	private Expr ParseCallExpr(Expr caller)
	{
		var callExpr = new CallExpr
		{
			Kind = "CallExpr",
			Caller = caller,
			Args = ParseArgs()
		};

		if (At().Type == TokenType.OpenParen)
		{
			callExpr = (CallExpr)ParseCallExpr(callExpr);
		}

		return callExpr;
	}

	private List<Expr> ParseArgs()
	{
		Expect(TokenType.OpenParen, "Expected open parenthesis");
		var args = At().Type == TokenType.CloseParen ? new List<Expr>() : ParseArgumentsList();

		Expect(TokenType.CloseParen, "Missing closing parenthesis inside arguments list");
		return args;
	}

	private List<Expr> ParseArgumentsList()
	{
		var args = new List<Expr> { ParseAssignmentExpr() };

		while (At().Type == TokenType.Comma && Eat() != null)
		{
			args.Add(ParseAssignmentExpr());
		}

		return args;
	}

	private Expr ParseMemberExpr()
	{
		var obj = ParsePrimaryExpr();

		while (At().Type == TokenType.Dot || At().Type == TokenType.OpenBracket)
		{
			var operatorType = Eat();
			Expr property;
			bool computed;

			// non-computed values aka obj.expr
			if (operatorType.Type == TokenType.Dot)
			{
				computed = false;
				// get identifier
				property = ParsePrimaryExpr();
				if (property.Kind != "Identifier")
				{
					throw new Exception("Cannot use dot operator without right hand side being an identifier");
				}
			}
			else
			{
				// This allows obj[computedValue]
				computed = true;
				property = ParseExpr();
				Expect(TokenType.CloseBracket, "Missing closing bracket in computed value.");
			}

			obj = new MemberExpr
			{
				Kind = "MemberExpr",
				Object = obj,
				Property = property,
				Computed = computed
			};
		}

		return obj;
	}

	// Orders of precedence
	// Assignment
	// Object
	// AdditiveExpr
	// MultiplicativeExpr
	// Call
	// Member
	// PrimaryExpr

	// Parse Literal Values & Grouping Expressions
	private Expr ParsePrimaryExpr()
	{
		var tk = At().Type;

		// Determine which token we are currently at and return literal value
		switch (tk)
		{
			case TokenType.Identifier:
				return new Identifier { Kind = "Identifier", Symbol = Eat().Value };

			case TokenType.Number:
				return new NumericLiteral
				{
					Kind = "NumericLiteral",
					Value = float.Parse(Eat().Value)
				};

			case TokenType.OpenParen:
				Eat(); // Eat the opening paren
				var value = ParseExpr();
				Expect(TokenType.CloseParen, "Unexpected token found inside parenthesized expression. Expected closing parenthesis.");
				return value;

			default:
				Console.Error.WriteLine("Unexpected token found during parsing!" + At());
				Environment.Exit(1);
				return null;
		}
	}

	// Tokenizer (mock implementation)
	private List<Token> Tokenize(string source)
	{
		// Implement a simple tokenizer here
		return new List<Token>();
	}
}

public class Program
{
	public string Kind { get; set; }
	public List<Stmt> Body { get; set; }
}

public class Stmt { }

public class VarDeclaration : Stmt
{
	public string Kind { get; set; }
	public string Identifier { get; set; }
	public bool Constant { get; set; }
	public Expr Value { get; set; }
}

public class FunctionDeclaration : Stmt
{
	public string Kind { get; set; }
	public string Name { get; set; }
	public List<string> Parameters { get; set; }
	public List<Stmt> Body { get; set; }
}

public class Expr { }

public class AssignmentExpr : Expr
{
	public Expr Value { get; set; }
	public Expr Assigne { get; set; }
	public string Kind { get; set; }
}

public class ObjectLiteral : Expr
{
	public string Kind { get; set; }
	public List<Property> Properties { get; set; }
}

public class BinaryExpr : Expr
{
	public string Kind { get; set; }
	public Expr Left { get; set; }
	public Expr Right { get; set; }
	public string Operator { get; set; }
}

public class CallExpr : Expr
{
	public string Kind { get; set; }
	public Expr Caller { get; set; }
	public List<Expr> Args { get; set; }
}

public class Property
{
	public string Kind { get; set; }
	public string Key { get; set; }
	public Expr Value { get; set; }
}

public class Identifier : Expr
{
	public string Kind { get; set; }
	public string Symbol { get; set; }
}

public class NumericLiteral : Expr
{
	public string Kind { get; set; }
	public float Value { get; set; }
}

public class MemberExpr : Expr
{
	public string Kind { get; set; }
	public Expr Object { get; set; }
	public Expr Property { get; set; }
	public bool Computed { get; set; }
}
}
