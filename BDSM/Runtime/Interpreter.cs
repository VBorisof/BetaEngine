using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Language;
using BDSM.Logging;
using BDSM.Parsing;
using BDSM.Scanning;
using BDSM.StandardLibrary;
using BDSM.Tokens;
using BDSM.Utilities;
using Beta.Common;

#nullable disable

namespace BDSM.Runtime;

public class Interpreter : ExpressionVisitor<object>, StatementVisitor<object>
{
    public Environment Globals { get; private set; } = new Environment();
    private Environment _environment;
    private readonly IBDSMLogger _logger;

    public BDSMEventHandlers EventHandlers { get; set; } = new BDSMEventHandlers();

    public Dictionary<Guid, Statement> AsyncCallbacks { get; } = [];

    public event EventHandler<RuntimeError> RuntimeError = (_, _) => { };

    public string RootDir { get; set; }

    public Interpreter(IBDSMLogger logger)
    {
        _environment = Globals;
        _logger = logger;

        RestartEnvironment();
    }

    public void DumpEnvironment()
    {
        Console.WriteLine("\n\n-------------------------------------");
        Console.WriteLine("GLOBAL ENV: ");
        Globals.Dump();
        Console.WriteLine("-------------------------------------\n\n");
    }

    public void RestartEnvironment()
    {
        _environment.Clear();
        Globals.Clear();

        Globals = new Environment();
        _environment = Globals;

        Globals.Define("clock", new ClockFunction());
        Globals.Define("echo", new EchoFunction());
        Globals.Define("rand", new RandomFunction());
        Globals.Define("dumpenv", new DumpFunction());

        Globals.Define("wait", new WaitFunction());
        Globals.Define("setscene", new SetSceneFunction());
        Globals.Define("setplayer", new SetPlayerFunction());
        Globals.Define("camzoom", new CamZoomFunction());
        Globals.Define("setcampos", new SetCamPosFunction());
        Globals.Define("playcinematic", new CinematicStartFunction());
        Globals.Define("fadein", new FadeInFunction());
        Globals.Define("fadeout", new FadeOutFunction());
        Globals.Define("closeup", new CloseupFunction());
        Globals.Define("playsong", new PlaySongFunction());
        Globals.Define("playsound", new PlaySoundFunction());
        Globals.Define("stopsong", new StopSongFunction());
        Globals.Define("playvideo", new PlayVideoFunction());
        Globals.Define("requestmainmenu", new RequestMainMenuFunction());
        Globals.Define("requeststartmenu", new RequestStartMenuFunction());
        Globals.Define("requesttutorial", new RequestTutorialFunction());
        Globals.Define("endgame", new EndGameFunction());
        Globals.Define("requeststateplaying", new RequestStatePlayingFunction());
        Globals.Define("interrupt", new InterruptFunction());
        Globals.Define("narrate", new NarrateFunction());
        Globals.Define("movecamto", new MoveCamToFunction());
        Globals.Define("autosave", new AutosaveFunction());
        Globals.Define("freespeech", new FreespeechFunction());
        Globals.Define("tip", new TipFunction());
        Globals.Define("notify", new NotifyFunction());
        EventHandlers.OnSetScene += (_, e) =>
        {
            Globals.Define("__scene", e.Scene);
        };
    }


    public void Interpret(Statement statement)
    {
        var context = ExecutionContext.Shared;
        try
        {
            if (statement != null)
            {
                Execute(statement, context);
            }
        }
        catch (RuntimeError error)
        {
            RuntimeError.Invoke(this, error);
        }
    }
    public void Interpret(List<Statement> statements)
    {
        var context = ExecutionContext.Shared;
        try
        {
            foreach (var statement in statements)
            {
                if (statement != null)
                {
                    Execute(statement, context);
                }
            }
        }
        catch (RuntimeError error)
        {
            RuntimeError.Invoke(this, error);
        }
    }

    public object VisitLiteralExpression(LiteralExpression literal, ExecutionContext context)
    {
        return literal.val;
    }

    public object VisitGroupingExpression(GroupingExpression grouping, ExecutionContext context)
    {
        return Evaluate(grouping.expr, context);
    }

    public object VisitUnaryExpression(UnaryExpression unary, ExecutionContext context)
    {
        var right = Evaluate(unary.expr, context);

        switch (unary.op.TokenType)
        {
            case TokenType.Bang:
                return !EvaluationUtilities.IsTruthy(right);
            case TokenType.Minus:
                EvaluationUtilities.CheckNumberOperand(unary.op, right);
                return -(double)right;
        }

        return null;
    }

    public object VisitBinaryExpression(BinaryExpression binary, ExecutionContext context)
    {
        var left = Evaluate(binary.left, context);
        var right = Evaluate(binary.right, context);

        switch (binary.op.TokenType)
        {
            case TokenType.Minus:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                // TODO: Also evaluate expressions.
                return (double)left - (double)right;
            case TokenType.Slash:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left / (double)right;
            case TokenType.Star:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left * (double)right;
            case TokenType.Plus:
                if (left is double v && right is double v1)
                {
                    return v + v1;
                }
                if (left is string && right is string)
                {
                    return $"{left}{right}";
                }
                if (left is string && right is double)
                {
                    return $"{left}{right}";
                }
                if (left is string && right is object)
                {
                    return $"{left}{right}";
                }
                throw new RuntimeError(binary.op, "Operands must be numbers or strings.");
            case TokenType.Greater:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                EvaluationUtilities.CheckNumberOperands(binary.op, left, right);
                return (double)left <= (double)right;
            case TokenType.BangEqual:
                return !EvaluationUtilities.IsEqual(left, right);
            case TokenType.DoubleEqual:
                return EvaluationUtilities.IsEqual(left, right);
        }

        return null;
    }

    public object Evaluate(Expression e, ExecutionContext context)
    {
        if (e != null)
        {
            return e.Accept(this, context);
        }
        return null;
    }


    public object VisitBlockStatement(BlockStatement block, ExecutionContext context)
    {
        ExecuteBlock(block.statements, new Environment(_environment), context);
        return null;
    }

    public object VisitAsyncStatement(AsyncStatement async, ExecutionContext context)
    {
        var block = async.body as BlockStatement;
        var then = async.then as BlockStatement;
        var asyncContext = ExecutionContext.Async();
        VisitBlockStatement(block, asyncContext);

        if (then != null)
        {
            AsyncCallbacks[asyncContext.AsyncTag] = then;
        }

        return null;
    }

    public object RequestAsyncCallback(Guid asyncTag)
    {
        if (AsyncCallbacks.TryGetValue(asyncTag, out var value))
        {
            Interpret(value);

            AsyncCallbacks.Remove(asyncTag);
        }

        return null;
    }

    public void ExecuteBlock(List<Statement> statements, Environment env, ExecutionContext context)
    {
        var prev = _environment;
        try
        {
            _environment = env;

            foreach (var statement in statements)
            {
                Execute(statement, context);
            }
        }
        finally
        {
            _environment = prev;
        }
    }

    public object VisitExpressionStatement(ExpressionStatement expression, ExecutionContext context)
    {
        Evaluate(expression.expr, context);
        return null;
    }

    public object VisitPrintStatement(PrintStatement print, ExecutionContext context)
    {
        var val = Evaluate(print.expr, context);
        Console.WriteLine(EvaluationUtilities.Stringify(val));
        return null;
    }

    public object VisitVarStatement(VarStatement var, ExecutionContext context)
    {
        var val = Evaluate(var.initializer, context);
        _environment.Define(var.name.Lexeme, val);

        return null;
    }

    public object VisitVariableExpression(VariableExpression variable, ExecutionContext context)
    {
        return _environment.Get(variable.name);
    }

    public object VisitAssignExpression(AssignExpression assign, ExecutionContext context)
    {
        var val = Evaluate(assign.val, context);
        _environment.Assign(assign.name, val);
        return val;
    }

    private void Execute(Statement statement, ExecutionContext context)
    {
        statement.Accept(this, context);
    }

    public object VisitIfsStatement(IfsStatement ifs, ExecutionContext context)
    {
        if (EvaluationUtilities.IsTruthy(Evaluate(ifs.condition, context)))
        {
            Execute(ifs.thenBranch, context);
        }
        else if (ifs.elseBranch != null)
        {
            Execute(ifs.elseBranch, context);
        }

        return null;
    }

    public object VisitLogicalExpression(LogicalExpression logical, ExecutionContext context)
    {
        var left = Evaluate(logical.left, context);

        if (logical.op.TokenType == TokenType.Or)
        {
            if (EvaluationUtilities.IsTruthy(left))
            {
                return left;
            }
        }
        else
        {
            if (!EvaluationUtilities.IsTruthy(left))
            {
                return left;
            }
        }

        return Evaluate(logical.right, context);
    }

    public object VisitWhilesStatement(WhilesStatement whiles, ExecutionContext context)
    {
        while (EvaluationUtilities.IsTruthy(Evaluate(whiles.condition, context)))
        {
            Execute(whiles.body, context);
        }

        return null;
    }

    public object VisitCallExpression(CallExpression call, ExecutionContext context)
    {
        var callee = Evaluate(call.callee, context);

        var args = call.arguments.Select(a => Evaluate(a, context)).ToList();

        if (callee is not ICallable)
        {
            throw new RuntimeError(call.paren, "Unexpected function call.");
        }

        var func = (ICallable)callee;
        if (call.arguments.Count != func.Arity())
        {
            throw new RuntimeError(
                call.paren,
                $"Expected {func.Arity()} args but got {call.arguments.Count}."
            );
        }
        return func.Call(this, args, context);
    }

    public object VisitTupleExpression(TupleExpression tuple, ExecutionContext context)
    {
        return tuple.members.Select(m => Evaluate(m, context)).ToList();
    }

    public object VisitFunctionStatement(FunctionStatement function, ExecutionContext context)
    {
        var func = new Function(function);
        _environment.Define(function.name.Lexeme, func);
        return null;
    }

    public object VisitReturnsStatement(ReturnsStatement returns, ExecutionContext context)
    {
        object val = null;
        if (returns.value != null)
        {
            val = Evaluate(returns.value, context);
        }

        throw new Return(val);
    }

    public object VisitActorStatement(ActorStatement actor, ExecutionContext context)
    {
        // Need to convert these to something callable from game code...
        var verbs = actor.verbs.Select(v => (VerbStatement)v).ToList();

        var a = new BDSMActor(
            actor.declName.Lexeme,
            verbs
        );

        foreach (var f in actor.functions)
        {
            var func = new Function(f);
            a.AddMethod(f.name, func);
        }
        foreach (var v in actor.vars)
        {
            a.AddField(v.name, Evaluate(v.initializer, context));
        }

        _environment.Define(actor.declName.Lexeme, a);

        EventHandlers.OnDefineActor(this, new DefineActorEventArgs(context, a));

        return null;
    }

    public object VisitVerbStatement(VerbStatement verb, ExecutionContext context)
    {
        ExecuteBlock(verb.statements, _environment, context);
        return null;
    }

    public object VisitSceneStatement(SceneStatement sceneStatement, ExecutionContext context)
    {
        var declName = sceneStatement.declName.Lexeme;

        var regions = new List<BDSMRegion>();
        foreach (var region in sceneStatement.regions)
        {
            var bdsmRegion = new BDSMRegion(region.declName.Lexeme);
            foreach (var f in region.functions)
            {
                var func = new Function(f);
                bdsmRegion.AddMethod(f.name, func);
            }
            regions.Add(bdsmRegion);
        }
        var scene = new BDSMScene(declName, regions, sceneStatement.props);

        foreach (var f in sceneStatement.functions)
        {
            var func = new Function(f);
            scene.AddMethod(f.name, func);
        }
        foreach (var v in sceneStatement.vars)
        {
            scene.AddField(v.name, Evaluate(v.initializer, context));
        }

        _environment.Define(declName, scene);

        EventHandlers.OnDefineScene(this, new DefineSceneEventArgs(context, scene));

        return null;
    }

    public object VisitGetExpression(GetExpression get, ExecutionContext context)
    {
        var obj = Evaluate(get.instance, context);
        if (obj is Instance instance)
        {
            return instance.Get(get.name);
        }

        throw new RuntimeError(
            get.name,
            $"Try to access a non-instance field. Was {obj.GetType()} instead."
        );
    }

    public object VisitSetExpression(SetExpression set, ExecutionContext context)
    {
        var obj = Evaluate(set.obj, context);
        if (obj is not Instance)
        {
            throw new RuntimeError(set.name, "Try to set a non-instance field.");
        }

        var val = Evaluate(set.val, context);

        ((Instance)obj).SetField(set.name, val);

        return val;
    }

    public object VisitImportStatement(ImportStatement import, ExecutionContext context)
    {
        var fileName = RootDir + "/" + (string)import.what.Literal;

        if (!Globals.IsFileImported(fileName))
        {
            var source = "";
            try
            {
                source = FileLoader.ReadAllFromFile(fileName);
            }
            catch (IOException)
            {
                throw new RuntimeError(import.what, $"Cannot import `{fileName}`: File not found.");
            }
            var scanner = new Scanner(_logger);
            var tokens = scanner.ScanTokens(source);

            _logger.Debug($"TOKENS: \n  {string.Join("\n  ", tokens)}");

            if (!scanner.IsSuccess)
            {
                _logger.Info("Please fix syntax errors.");
                throw new RuntimeError(import.what, $"Cannot import `{fileName}`: Syntax errors.");
            }

            try
            {
                var parser = new Parser(_logger);
                var expression = parser.Parse(tokens);
                if (expression == null)
                {
                    throw new RuntimeError(import.what, $"Cannot import `{fileName}`: Parse errors.");
                }

                Interpret(expression);
            }
            catch (ParseError)
            {
                _logger.Info("Please fix parsing errors.");
                throw new RuntimeError(import.what, $"Cannot import `{fileName}`: Parse errors.");
            }

            Globals.SetFileImported(fileName);
        }

        return null;
    }

    public object VisitRegionStatement(RegionStatement region, ExecutionContext context)
    {
        //new BDSMRegion(region.declName);

        return null;
    }

    public object VisitPropStatement(PropStatement prop, ExecutionContext context)
    {
        // TODO: Ouch.. Fix this.
        //throw new NotImplementedException();
        return null;
    }

    public void EnterScene(string sceneName)
    {
        var scene = (BDSMScene)Globals.Get(sceneName);

        // Reset regions.
        foreach (var region in scene.Regions)
        {
            region.SetField(BDSMRegion.IsEnteredFieldName, false);
        }

        var callbackDefined = scene.TryGetMethod("onEnter", out var onEnter);

        if (callbackDefined)
        {
            onEnter.Call(this, null, ExecutionContext.Shared);
        }

        scene.SetField("timesEntered", (double)scene.Get("timesEntered") + 1);
    }

    public void TryCallUpdate(string declName)
    {
        if (_environment.Get(declName) is Instance instance)
        {
            var updateDefined = instance.TryGetMethod("update", out var update);

            if (updateDefined)
            {
                update.Call(this, null, ExecutionContext.Actor(declName));
            }
        }
    }

    public void SetSceneVariable(string name)
    {
        Globals.Define("__scene", (BDSMScene)_environment.Get(name));
    }

    public bool IsAbleToExit(string name, int startIndex)
    {
        var scene = (BDSMScene)_environment.Get(name);
        var hasExitMethod = scene.TryGetMethod("exit", out var exitFunc);
        if (!hasExitMethod)
        {
            return true;
        }

        if (exitFunc.Arity() != 1)
        {
            throw new RuntimeError(null, $"scene {name}: exit() must accept exactly one integer parameter.");
        }

        var result = exitFunc.Call(this, [(double)startIndex], ExecutionContext.Shared);

        return (bool)result;
    }

    public void EnterSceneRegion(string sceneName, string regionName)
    {
        var scene = (BDSMScene)_environment.Get(sceneName);
        var region = scene.Regions.Single(r => regionName == r.DeclName);
        region.SetField(BDSMRegion.IsEnteredFieldName, true);

        var hasOnEnterMethod = region.TryGetMethod("onEnter", out var onEnter);

        if (!hasOnEnterMethod)
        {
            return;
        }

        // TODO: Execution context?
        onEnter.Call(this, [], ExecutionContext.Shared);
    }

    public void ExitSceneRegion(string sceneName, string regionName)
    {
        var scene = (BDSMScene)_environment.Get(sceneName);
        var region = scene.Regions.Single(r => regionName == r.DeclName);
        region.SetField(BDSMRegion.IsEnteredFieldName, false);
    }

    public void SetSceneRegionEntered(string sceneName, string regionName, bool isEntered)
    {
        var scene = (BDSMScene)_environment.Get(sceneName);
        var region = scene.Regions.Single(r => regionName == r.DeclName);
        region.SetField(BDSMRegion.IsEnteredFieldName, isEntered);
    }

    public void CallDefaultVerbHandler(string verb)
    {
        var defaultHandlerFunction = (Function)_environment.Get($"default{verb}");

        defaultHandlerFunction.Call(this, [], ExecutionContext.Shared);
    }
}