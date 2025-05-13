using System;
using System.Collections.Generic;

namespace Bolang
{
	public enum TokenType
	{
		Number,
		Identifier,
		Let,
		BinaryOperator,
		Equals,
		OpenParen,
		CloseParen
	}

	public class Token
	{
		public string Value { get; set; }
		public TokenType Type { get; set; }

		public Token(string value, TokenType type)
		{
			Value = value;
			Type = type;
		}

		public override string ToString()
		{
			return $"value: \"{Value}\", type: {Type.ToString().ToLower()}";
		}
	}

	public static class Lexer
	{
		private static readonly Dictionary<string, TokenType> KEYWORDS = new()
		{
			{ "let", TokenType.Let }
		};

		private static Token CreateToken(string value, TokenType type)
		{
			return new Token(value, type);
		}

		public static List<Token> Tokenize(string sourceCode)
		{
			List<Token> tokens = new();
			List<char> src = new(sourceCode);

			while (src.Count > 0)
			{
				char current = src[0];

				if (current == '(')
				{
					tokens.Add(CreateToken(current.ToString(), TokenType.OpenParen));
					src.RemoveAt(0);
				}
				else if (current == ')')
				{
					tokens.Add(CreateToken(current.ToString(), TokenType.CloseParen));
					src.RemoveAt(0);
				}
				else if ("+-*/".Contains(current))
				{
					tokens.Add(CreateToken(current.ToString(), TokenType.BinaryOperator));
					src.RemoveAt(0);
				}
				else if (current == '=')
				{
					tokens.Add(CreateToken(current.ToString(), TokenType.Equals));
					src.RemoveAt(0);
				}
				else if (IsInt(current))
				{
					string num = "";
					while (src.Count > 0 && IsInt(src[0]))
					{
						num += src[0];
						src.RemoveAt(0);
					}
					tokens.Add(CreateToken(num, TokenType.Number));
				}
				else if (IsAlpha(current))
				{
					string ident = "";
					while (src.Count > 0 && IsAlpha(src[0]))
					{
						ident += src[0];
						src.RemoveAt(0);
					}

					if (KEYWORDS.ContainsKey(ident))
					{
						tokens.Add(CreateToken(ident, KEYWORDS[ident]));
					}
					else
					{
						tokens.Add(CreateToken(ident, TokenType.Identifier));
					}
				}
				else if (IsSkippable(current))
				{
					src.RemoveAt(0);
				}
				else
				{
					Console.WriteLine($"Unrecognized character found in source: {(int)current} '{current}'");
					Environment.Exit(1);
				}
			}

			return tokens;
		}

		private static bool IsAlpha(char c) => char.IsLetter(c);
		private static bool IsInt(char c) => char.IsDigit(c);
		private static bool IsSkippable(char c) => char.IsWhiteSpace(c);
	}
}
