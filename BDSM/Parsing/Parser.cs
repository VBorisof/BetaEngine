using BDSM.Language;
using BDSM.Logging;
using BDSM.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace BDSM.Parsing;

public class Parser
{
    private readonly IBDSMLogger _logger;
    private List<Token> _tokens = [];
    private int _current;

    public Parser(IBDSMLogger logger)
    {
        _logger = logger;
    }

    public List<Statement?> Parse(List<Token> tokens)
    {
        // TODO: Newlines are broken so we won't have a clear way to separate
        // statements... Need to fix this.
        _tokens = tokens.Where(t => t.TokenType != TokenType.Newline).ToList();
        _current = 0;

        var statements = new List<Statement?>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private List<Statement> Block()
    {
        var statements = new List<Statement>();
        while (!PeekIsType(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl != null)
            {
                statements.Add(decl);
            }
        }

        Consume("Exprected `}` after block.", TokenType.RightBrace);
        return statements;
    }

    // 
    // declaration -> sceneDecl | actorDecl | flagsDecl | verbDecl | funDecl | varDecl
    private Statement? Declaration()
    {
        try
        {
            if (IsMatch(TokenType.Scene))
            {
                return SceneDeclaration();
            }
            if (IsMatch(TokenType.Actor))
            {
                return ActorDeclaration();
            }
            if (IsMatch(TokenType.Verb))
            {
                return VerbDeclaration();
            }
            if (IsMatch(TokenType.Fun))
            {
                return Function("function");
            }
            if (IsMatch(TokenType.Region))
            {
                return RegionDeclaration();
            }
            if (IsMatch(TokenType.Prop))
            {
                return PropDeclaration();
            }
            if (IsMatch(TokenType.Var))
            {
                return VarDeclaration();
            }
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    // 
    // scene -> "scene" IDENTIFIER "{" placement* variable* function* "}" 
    private SceneStatement SceneDeclaration()
    {
        var declName = Consume("Expect scene declaration name.", TokenType.Identifier);

        Consume("Expected `{` after scene declaration.", TokenType.LeftBrace);

        // TODO: Just shove all of this into a statement list and sort it 
        // out in the interpreter.
        List<FunctionStatement> functions = [];
        List<VarStatement> variables = [];
        List<RegionStatement> regions = [];
        List<PropStatement> props = [];
        while (!PeekIsType(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl is VarStatement)
            {
                variables.Add((VarStatement)decl);
            }
            if (decl is FunctionStatement)
            {
                functions.Add((decl as FunctionStatement)!);
            }
            if (decl is RegionStatement)
            {
                regions.Add((decl as RegionStatement)!);
            }
            if (decl is PropStatement)
            {
                props.Add((decl as PropStatement)!);
            }
        }

        Consume("Expected `}` after scene declaration.", TokenType.RightBrace);

        return new SceneStatement(declName, variables, functions, regions, props);
    }

    // 
    // actorDecl -> "actor" IDENTIFIER "{" verb* variable* function* "}"
    private ActorStatement ActorDeclaration()
    {
        var declName = Consume("Expect actor declaration name.", TokenType.Identifier);

        Consume("Expected `{` after actor declaration.", TokenType.LeftBrace);

        List<Statement> verbStatements = [];
        List<VarStatement> variables = [];
        List<FunctionStatement> functions = [];
        while (!PeekIsType(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl is VerbStatement)
            {
                verbStatements.Add(decl);
            }
            if (decl is VarStatement statement)
            {
                variables.Add(statement);
            }
            if (decl is FunctionStatement)
            {
                functions.Add((decl as FunctionStatement)!);
            }
        }

        Consume("Expected `}` after actor declaration.", TokenType.RightBrace);

        return new ActorStatement(declName, verbStatements, variables, functions);
    }

    //
    // verb -> "verb" IDENTIFIER "{" Statement* "}"
    private VerbStatement VerbDeclaration()
    {
        var name = Consume($"Expect verb name.", TokenType.Identifier);

        Token? item = null;
        if (IsMatch(TokenType.Identifier))
        {
            item = Previous();
        }

        Consume($"Expect verb body.", TokenType.LeftBrace);
        var body = Block();

        return new VerbStatement(name, item, body);
    }

    private FunctionStatement Function(string kind)
    {
        var name = Consume($"Expect {kind} name.", TokenType.Identifier);

        Consume($"Expect `(` after {kind} name.", TokenType.LeftParen);

        var parameters = new List<Token>();

        if (!PeekIsType(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    throw Error(Peek(), "Function can't have more than 255 parameters.");
                }

                parameters.Add(Consume("Expect parameter name.", TokenType.Identifier));
            }
            while (IsMatch(TokenType.Comma));
        }

        Consume("Expect `)` after parameters.", TokenType.RightParen);

        Consume($"Expect {kind} body.", TokenType.LeftBrace);
        var body = Block();

        return new FunctionStatement(name, parameters, body);
    }

    private RegionStatement RegionDeclaration()
    {
        var name = Consume($"Expect region name.", TokenType.Identifier);

        Consume($"Expect region body.", TokenType.LeftBrace);
        List<FunctionStatement> functions = [];
        while (!PeekIsType(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl is FunctionStatement)
            {
                functions.Add((decl as FunctionStatement)!);
            }
        }

        Consume("Expected `}` after region body.", TokenType.RightBrace);

        return new RegionStatement(name, functions);
    }

    private PropStatement PropDeclaration()
    {
        var declName = Consume($"Expect prop declaration name.", TokenType.Identifier);
        var name = Consume($"Expect prop name.", TokenType.String);

        Consume("Expected `{` after prop declaration.", TokenType.LeftBrace);

        List<VerbStatement> verbs = [];
        while (!PeekIsType(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl is VerbStatement statement)
            {
                verbs.Add(statement);
            }
        }

        Consume("Expected `}` after prop definition.", TokenType.RightBrace);

        return new PropStatement(declName, name, verbs);
    }

    private Statement VarDeclaration()
    {
        var name = Consume("Variable name expected.", TokenType.Identifier);

        Expression? initializer = null;
        if (IsMatch(TokenType.Equal))
        {
            initializer = Expression();
        }
        else
        {
            throw Error(Peek(), "Variable assignment expected.");
        }

        return new VarStatement(name, initializer);
    }

    private Statement ImportStatement()
    {
        var path = Consume("Expect import path.", TokenType.String);
        return new ImportStatement(path);
    }

    private Statement IfStatement()
    {
        Consume("Expect `(` after `if`.", TokenType.LeftParen);
        var condition = Expression();
        Consume("Expect `)` for `if`.", TokenType.RightParen);

        var thenBranch = Statement();
        Statement? elseBranch = null;
        if (IsMatch(TokenType.Else))
        {
            elseBranch = Statement();
        }

        return new IfsStatement(condition, thenBranch, elseBranch);
    }

    private Statement PrintStatement()
    {
        _logger.Debug("Try to get expression...");
        var expr = Expression();
        _logger.Debug("Try to consume...");
        //Consume(TokenType.Newline, "Expected newline after print");
        return new PrintStatement(expr);
    }

    private Statement ReturnStatement()
    {
        var keyword = Previous();
        var val = Expression();

        // Check for newline here...

        return new ReturnsStatement(keyword, val);
    }

    private Statement WhileStatement()
    {
        Consume("Expect `(` after `while`.", TokenType.LeftParen);
        var condition = Expression();
        Consume("Expect `)` for `while`.", TokenType.RightParen);

        var body = Statement();

        return new WhilesStatement(condition, body);
    }

    private Statement ForStatement()
    {
        Consume("Expect `(` after `for`.", TokenType.LeftParen);

        Statement? init = null;
        if (IsMatch(TokenType.Comma))
        {
            _logger.Debug("FOR: Match comma...");
            init = null;
        }
        else if (IsMatch(TokenType.Var))
        {
            _logger.Debug("FOR: Match vardecl...");
            init = VarDeclaration();
            Consume("Expect comma after for-statement initializer.", TokenType.Comma);
        }
        else
        {
            _logger.Debug("FOR: Match expression...");
            init = ExpressionStatement();
            Consume("Expect comma after for-statement initializer.", TokenType.Comma);
        }

        Expression? condition = null;
        if (!PeekIsType(TokenType.Comma))
        {
            condition = Expression();
        }
        Consume("Expect comma after for-statement condition.", TokenType.Comma);

        Expression? after = null;
        if (!PeekIsType(TokenType.RightParen))
        {
            after = Expression();
        }
        Consume("Expect `)` for `for`.", TokenType.RightParen);

        var body = Statement();
        if (after != null)
        {
            body = new BlockStatement(
                [body, new ExpressionStatement(after)]
            );
        }

        if (condition == null)
        {
            condition = new LiteralExpression(true);
        }

        body = new WhilesStatement(condition, body);

        if (init != null)
        {
            body = new BlockStatement([init, body]);
        }

        return body;
    }

    private Statement AsyncStatement()
    {
        var token = Peek();

        var body = Statement() as BlockStatement;
        if (body == null)
        {
            Error(token, "Expected async block declaration.");
        }

        Statement? then = null;
        if (IsMatch(TokenType.Then))
        {
            then = Statement() as BlockStatement;
            if (then == null)
            {
                Error(token, "Expected a block after `then`.");
            }
        }

        return new AsyncStatement(body, then);
    }

    private Statement ExpressionStatement()
    {
        var expr = Expression();
        if (expr != null)
        {
            //Consume(TokenType.Newline, "Expected newline after statement");
        }
        return new ExpressionStatement(expr);
    }

    //
    // expression -> assignment
    private Expression? Expression()
    {
        return Assignment();
    }

    //
    // assignment -> ( call "." )? IDENTIFIER "=" assignment | logic_or
    private Expression? Assignment()
    {
        var expr = LogicOr();

        if (IsMatch(TokenType.Equal))
        {
            var eq = Previous();
            var val = Assignment();

            if (expr is VariableExpression expression)
            {
                var name = expression.name;
                return new AssignExpression(name, val);
            }
            else if (expr is GetExpression get)
            {
                return new SetExpression(get.instance, get.name, val);
            }

            Error(eq, "Invalid assignment target.");
        }

        return expr;
    }

    // logic_or -> logic_and ("or" logic_and)*
    private Expression? LogicOr()
    {
        var expr = LogicAnd();

        while (IsMatch(TokenType.Or))
        {
            var op = Previous();
            var right = LogicAnd();
            expr = new LogicalExpression(expr, op, right);
        }

        return expr;
    }

    // logic_and -> equality ("and" equality)*
    private Expression? LogicAnd()
    {
        var expr = Equality();

        while (IsMatch(TokenType.And))
        {
            var op = Previous();
            var right = Equality();
            expr = new LogicalExpression(expr, op, right);
        }

        return expr;
    }

    //
    // equality -> comparison ( ("!=" | "==") comparison)* ;
    private Expression? Equality()
    {
        var expr = Comparison();
        while (IsMatch(TokenType.BangEqual, TokenType.DoubleEqual))
        {
            var op = Previous();
            var right = Comparison();

            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    //
    // comparison -> term ( (">" | ">=" | "<" | "<=") term )* ;
    private Expression? Comparison()
    {
        var expr = Term();
        while (IsMatch(
            TokenType.Greater, TokenType.GreaterEqual,
            TokenType.Less, TokenType.LessEqual)
        )
        {
            var op = Previous();
            var right = Term();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    //
    // term -> factor ( ("-" | "+") factor )* ;
    private Expression? Term()
    {
        var expr = Factor();
        while (IsMatch(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = Factor();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    //
    // factor -> unary ( ("/" | "*") unary )* | tuple;
    private Expression? Factor()
    {
        if (IsMatch(TokenType.LeftParen))
        {
            return Tuple();
        }

        var expr = Unary();
        while (IsMatch(TokenType.Slash, TokenType.Star))
        {
            var op = Previous();
            var right = Unary();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    //
    // tuple -> unary ( "," unary )* ;
    private Expression? Tuple()
    {
        var expr = Unary();

        var members = new List<Expression?> { expr };
        while (IsMatch(TokenType.Comma))
        {
            if (members.Count >= 10)
            {
                Error(Peek(), "Tuple cannot have more than 10 members.");
            }

            members.Add(Expression());
        }

        var paren = Consume("Expect `)` after tuple.", TokenType.RightParen);

        return new TupleExpression(paren, members);
    }

    //
    // unary -> ( "-" | "!" ) unary | call;
    private Expression? Unary()
    {
        if (IsMatch(TokenType.Minus, TokenType.Bang))
        {
            var op = Previous();
            var right = Unary();
            return new UnaryExpression(op, right);
        }

        return Call();
    }

    //
    // call -> primary ( "(" arguments? ")" )*
    private Expression? Call()
    {
        var expr = Primary();

        while (true)
        {
            if (IsMatch(TokenType.LeftParen))
            {
                expr = FinishCall(expr!);
            }
            else if (IsMatch(TokenType.Dot))
            {
                var name = Consume(
                    "Expected property name after `.`.",
                    TokenType.Identifier
                );
                expr = new GetExpression(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expression? FinishCall(Expression callee)
    {
        var args = new List<Expression?>();
        if (!PeekIsType(TokenType.RightParen))
        {
            do
            {
                if (args.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 args.");
                }
                args.Add(Expression());
            } while (IsMatch(TokenType.Comma));
        }

        var paren = Consume("Expect `)` after args.", TokenType.RightParen);

        return new CallExpression(callee, paren, args);
    }

    // 
    // primary -> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
    private Expression? Primary()
    {
        if (IsMatch(TokenType.True))
        {
            return new LiteralExpression(true);
        }
        if (IsMatch(TokenType.False))
        {
            return new LiteralExpression(false);
        }
        if (IsMatch(TokenType.Nil))
        {
            return new LiteralExpression(null);
        }
        if (IsMatch(TokenType.Newline))
        {
            return null;
        }

        if (IsMatch(TokenType.Number, TokenType.String))
        {
            return new LiteralExpression(Previous().Literal);
        }

        if (IsMatch(TokenType.Identifier))
        {
            return new VariableExpression(Previous());
        }

        if (IsMatch(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume("Expected `)` after expression", TokenType.RightParen);
            return new GroupingExpression(expr);
        }

        throw Error(Peek(), "Expected an expression");
    }

    private Statement Statement()
    {
        if (IsMatch(TokenType.Import))
        {
            return ImportStatement();
        }
        if (IsMatch(TokenType.If))
        {
            return IfStatement();
        }
        if (IsMatch(TokenType.Print))
        {
            return PrintStatement();
        }
        if (IsMatch(TokenType.Return))
        {
            return ReturnStatement();
        }
        if (IsMatch(TokenType.While))
        {
            return WhileStatement();
        }
        if (IsMatch(TokenType.For))
        {
            return ForStatement();
        }
        if (IsMatch(TokenType.Async))
        {
            return AsyncStatement();
        }
        if (IsMatch(TokenType.LeftBrace))
        {
            return new BlockStatement(Block());
        }
        return ExpressionStatement();
    }

    private bool IsMatch(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (PeekIsType(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(string message, params TokenType[] types)
    {
        foreach (var t in types)
        {
            if (PeekIsType(t))
            {
                return Advance();
            }
        }
        throw Error(Peek(), message);
    }


    private bool PeekIsType(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().TokenType == type;
    }
    private bool PeekNextIsType(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return PeekNext().TokenType == type;
    }
    private bool IsAtEnd()
    {
        return Peek().TokenType == TokenType.EOF;
    }
    private Token Peek()
    {
        return _tokens[_current];
    }
    private Token PeekNext()
    {
        if (IsAtEnd())
        {
            return Peek();
        }
        return _tokens[_current + 1];
    }
    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            ++_current;
            return Previous();
        }
        return Peek();
    }

    private ParseError Error(Token token, string message)
    {
        _logger.Error(token.Line, token.Column, token.Lexeme, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            switch (Peek().TokenType)
            {
                case TokenType.Import:
                case TokenType.If:
                case TokenType.Else:
                case TokenType.Scene:
                case TokenType.Actor:
                case TokenType.Entity:
                case TokenType.Verb:
                case TokenType.Animations:
                case TokenType.Wait:
                case TokenType.Nil:
                case TokenType.While:
                case TokenType.For:
                case TokenType.Async:
                case TokenType.Then:
                case TokenType.Region:
                case TokenType.Prop:
                    return;
            }

            Advance();
        }
    }
}