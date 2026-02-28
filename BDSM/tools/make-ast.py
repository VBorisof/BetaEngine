import sys

## Utility for generating abstract syntax trees.

class Type:
    def __init__(self, baseClass:str, className: str, fields: str):
        self.baseClass = baseClass
        self.className = className
        ## Split by commas and strip of extra space
        self.fields = list(map(lambda x: x.strip(' '), fields.split(',')))

    def print(self):
        # Class declaration
        print(f"public class {self.className}{self.baseClass} : {self.baseClass}")
        print(f"{{")

        # Constructor
        print(f"    public {self.className}{self.baseClass}({', '.join(self.fields)})")
        print("    {")
        for field in self.fields:
            field_name = field.split(' ')[1]
            print(f"        this.{field_name} = {field_name};")

        print("    }")
        print()

        # Fields
        for field in self.fields:
            print (f"    public {field};")

        print()

        # Visitor
        print(f"    public override T Accept<T>({self.baseClass}Visitor<T> visitor, ExecutionContext context)")
        print("    {")
        print(f"        return visitor.Visit{self.className}{self.baseClass}(this, context);")
        print("    }")
        print(f"}}")

def print_visitor(visitor_type: str, types: list[Type]):
    print(f"public interface {visitor_type}Visitor<T>")
    print("{")
    for type in types:
        print(f"    T Visit{type.className}{visitor_type}({type.className}{visitor_type} {type.className.lower()}, ExecutionContext context);")
    print("}")

def print_expression_ast(types: list[Type]):
    f = open("./codegen-disclaimer.txt", 'r')

    print(f.read())

    print()
    print("#nullable disable")
    print()
    print("using BDSM.Tokens;")
    print("using BDSM.ExecutionContexts;")
    print("using System.Collections.Generic;")
    print()
    print("namespace BDSM.Language;")
    print()
    print_visitor("Expression", types)
    print()
    print(f"public abstract class Expression")
    print("{")
    print("    public abstract T Accept<T>(ExpressionVisitor<T> visitor, ExecutionContext context);")
    print("}")
    for type in types:
        print()
        type.print()
    print()

def print_statement_ast(types: list[Type]):
    f = open("./codegen-disclaimer.txt", 'r')

    print(f.read())

    print()
    print("#nullable disable")
    print()
    print("using BDSM.Tokens;")
    print("using BDSM.ExecutionContexts;")
    print("using System.Collections.Generic;")
    print()
    print("namespace BDSM.Language;")
    print()
    print_visitor("Statement", types)
    print()
    print(f"public abstract class Statement")
    print("{")
    print("    public abstract T Accept<T>(StatementVisitor<T> visitor, ExecutionContext context);")
    print("}")
    for type in types:
        print()
        type.print()
    print()

def main():
    if len(sys.argv) == 1:
        print ("[!] Usage: python make-ast.py [E(xpression)|S(tatement)] > file.cs")
        return

    if str.lower(sys.argv[1]) == 'e':
        types = [
            Type("Expression", "Assign", "Token name, Expression val"),
            Type("Expression", "Binary", "Expression left, Token op, Expression right"),
            Type("Expression", "Grouping", "Expression expr"),
            Type("Expression", "Literal", "object val"),
            Type("Expression", "Logical", "Expression left, Token op, Expression right"),
            Type("Expression", "Set", "Expression obj, Token name, Expression val"),
            Type("Expression", "Unary", "Token op, Expression expr"),
            Type("Expression", "Call", "Expression callee, Token paren, List<Expression> arguments"),
            Type("Expression", "Get", "Expression instance, Token name"),
            Type("Expression", "Tuple", "Token paren, List<Expression> members"),
            Type("Expression", "Variable", "Token name"),
        ]
        print_expression_ast(types)

    elif str.lower(sys.argv[1]) == 's':
        types = [
            Type("Statement", "Import", "Token what"),
            Type("Statement", "Block", "List<Statement> statements"),
            Type("Statement", "Scene", "Token declName, List<VarStatement> vars, List<FunctionStatement> functions, List<RegionStatement> regions, List<PropStatement> props"),
            Type("Statement", "Region", "Token declName, List<FunctionStatement> functions"),
            Type("Statement", "Prop", "Token declName, Token name, List<VerbStatement> verbs"),
            Type("Statement", "Actor", "Token declName, List<Statement> verbs, List<VarStatement> vars, List<FunctionStatement> functions"),
            Type("Statement", "Verb", "Token name, Token item, List<Statement> statements"),
            Type("Statement", "Expression", "Expression expr"),
            Type("Statement", "Function", "Token name, List<Token> parameters, List<Statement> body"),
            Type("Statement", "Ifs", "Expression condition, Statement thenBranch, Statement elseBranch"),
            Type("Statement", "Print", "Expression expr"),
            Type("Statement", "Returns", "Token keyword, Expression value"),
            Type("Statement", "Async", "Statement body, Statement then"),
            Type("Statement", "Var", "Token name, Expression initializer"),
            Type("Statement", "Whiles", "Expression condition, Statement body"),
        ]
        print_statement_ast(types)

    else:
        print ("[!] Usage: python make-ast.py [E(xpression)|S(tatement)] > file.cs")
        return

if __name__ == '__main__':
    main()
