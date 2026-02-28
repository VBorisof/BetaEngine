#nullable disable

using BDSM.Runtime;
using BDSM.Tokens;
using System;

namespace BDSM.Utilities;

internal static class EvaluationUtilities
{
    public static void CheckNumberOperand(Token op, object right)
    {
        if (right is double)
        {
            return;
        }

        throw new RuntimeError(op, "Operand must be a number.");
    }
    public static void CheckNumberOperands(Token op, object left, object right)
    {
        if (right is double && left is double)
        {
            return;
        }

        throw new RuntimeError(op, "Operands must be numbers.");
    }
    public static bool IsTruthy(object o)
    {
        if (o == null)
        {
            return false;
        }
        if (o is bool v)
        {
            return v;
        }
        return true;
    }

    public static bool IsEqual(object left, object right)
    {
        if (left == null && right == null)
        {
            return true;
        }
        if (left == null)
        {
            return true;
        }

        return left.Equals(right);
    }

    public static string Stringify(object o)
    {
        if (o == null)
        {
            return "nil";
        }

        if (o is double)
        {
            var str = o.ToString();
            if (str.EndsWith(".0", StringComparison.InvariantCultureIgnoreCase))
            {
                str = str[..^2];
            }
            return str;
        }
        return o.ToString();
    }
}