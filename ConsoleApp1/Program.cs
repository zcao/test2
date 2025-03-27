
using DynamicExpresso;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

var sample = new SampleClass();
sample.SampleMethod();
//var target = new Interpreter();
//target.SetVariable("a", "1");
// Create an interpreter instance
var interpreter = new Interpreter();

interpreter.SetFunction("pow", Math.Pow);

var result = interpreter.Eval<double>("pow(2, 3)"); // 2^3 = 8
var a = Regex.Replace("pow[1,2]", @"(\w+)\[(.*?)\]", "$1($2)");
Console.WriteLine(result);

public class SampleClass
{

    public void SampleMethod()
    {
        var a = new SampleClass();
        a.NotNull();
        a.Test("b");
        Console.WriteLine("This line will never be executed.");
    }

    public void Test(string a, [CallerArgumentExpression("a")]
        string argumentExpression = null)
    {

    }
}


public static class Ext
{
  
    public static T NotNull<T>(
   
        this T obj,
    string message = null,
    [CallerArgumentExpression("obj")]
        string argumentExpression = null)
    where T : class
    {
        if (obj == null)
        {
            throw new ArgumentException(
                message ?? $"Expected object of type '{typeof(T).FullName}' to be not null",
                message == null ? argumentExpression : null);
        }

        return obj;
    }
}