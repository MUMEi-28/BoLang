namespace BoLang.AST
{
	public enum NodeType
	{
		// Statements
		Program,
		VarDeclaration,
		FunctionDeclaration,

		// Expressions
		AssignmentExpr,
		MemberExpr,
		CallExpr,

		// Literals
		Property,
		ObjectLiteral,
		NumericLiteral,
		Identifier,
		BinaryExpr
	}

	public interface IStmt
	{
		NodeType Kind { get; }
	}

	public interface IExpr : IStmt { }

	public class ProgramNode : IStmt
	{
		public NodeType Kind => NodeType.Program;
		public List<IStmt> Body { get; }

		public ProgramNode(List<IStmt> body)
		{
			Body = body;
		}
	}

	public class VarDeclaration : IStmt
	{
		public NodeType Kind => NodeType.VarDeclaration;
		public bool Constant { get; set; }
		public string Identifier { get; set; }
		public IExpr? Value { get; set; }

		public VarDeclaration(string identifier, bool constant, IExpr? value = null)
		{
			Identifier = identifier;
			Constant = constant;
			Value = value;
		}
	}

	public class FunctionDeclaration : IStmt
	{
		public NodeType Kind => NodeType.FunctionDeclaration;
		public string Name { get; set; }
		public List<string> Parameters { get; set; }
		public List<IStmt> Body { get; set; }

		public FunctionDeclaration(string name, List<string> parameters, List<IStmt> body)
		{
			Name = name;
			Parameters = parameters;
			Body = body;
		}
	}

	public class AssignmentExpr : IExpr
	{
		public NodeType Kind => NodeType.AssignmentExpr;
		public IExpr Assigne { get; set; }
		public IExpr Value { get; set; }

		public AssignmentExpr(IExpr assigne, IExpr value)
		{
			Assigne = assigne;
			Value = value;
		}
	}

	public class BinaryExpr : IExpr
	{
		public NodeType Kind => NodeType.BinaryExpr;
		public IExpr Left { get; set; }
		public IExpr Right { get; set; }
		public string Operator { get; set; }

		public BinaryExpr(IExpr left, IExpr right, string op)
		{
			Left = left;
			Right = right;
			Operator = op;
		}
	}

	public class CallExpr : IExpr
	{
		public NodeType Kind => NodeType.CallExpr;
		public IExpr Caller { get; set; }
		public List<IExpr> Args { get; set; }

		public CallExpr(IExpr caller, List<IExpr> args)
		{
			Caller = caller;
			Args = args;
		}
	}

	public class MemberExpr : IExpr
	{
		public NodeType Kind => NodeType.MemberExpr;
		public IExpr Object { get; set; }
		public IExpr Property { get; set; }
		public bool Computed { get; set; }

		public MemberExpr(IExpr obj, IExpr property, bool computed)
		{
			Object = obj;
			Property = property;
			Computed = computed;
		}
	}

	public class Identifier : IExpr
	{
		public NodeType Kind => NodeType.Identifier;
		public string Symbol { get; set; }

		public Identifier(string symbol)
		{
			Symbol = symbol;
		}
	}

	public class NumericLiteral : IExpr
	{
		public NodeType Kind => NodeType.NumericLiteral;
		public double Value { get; set; }

		public NumericLiteral(double value)
		{
			Value = value;
		}
	}

	public class Property : IExpr
	{
		public NodeType Kind => NodeType.Property;
		public string Key { get; set; }
		public IExpr? Value { get; set; }

		public Property(string key, IExpr? value = null)
		{
			Key = key;
			Value = value;
		}
	}

	public class ObjectLiteral : IExpr
	{
		public NodeType Kind => NodeType.ObjectLiteral;
		public List<Property> Properties { get; set; }

		public ObjectLiteral(List<Property> properties)
		{
			Properties = properties;
		}
	}
}
