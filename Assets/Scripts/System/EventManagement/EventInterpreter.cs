using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class EventInterpreter {

	public EventManager.GameEvent MakeEvent(int id, string eventDeclaration)
    {
        Dictionary<string, Object> context = new Dictionary<string, Object>();

        List<System.Action> actions = new List<System.Action>();

        List<string> lines = new List<string>(eventDeclaration.Replace(" ", "").Replace("\n", "").Split(';'));
        foreach (string line in lines) {
            if (!CheckSyntax(line)) {
                Throw(line);
            }
            actions.Add(InterpretStatement(line, context));
        }

        return new EventManager.GameEvent(id, actions);
    }

    bool CheckSyntax(string statement)
    {
        return 
            CheckParenthesisCount(statement)
        ;
    }

    bool CheckParenthesisCount(string statement)
    {
        return Regex.Matches(statement, @"\(").Count == Regex.Matches(statement, @"/)").Count;
    }

    System.Action InterpretStatement(string statement, Dictionary<string, Object> context)
    {
        System.Action action = delegate { };
        if (statement.Contains("=")) {
            Assign(statement, context);
            return delegate { };
        }
        return delegate { };
    }

    void Assign(string assignationStatement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = assignationStatement.Split('=');
        if (explodedStatement.Length <= 0 || explodedStatement.Length > 2) {
            Throw(assignationStatement);
        }
        string varName = explodedStatement[0];
        Object generated = GenerateObject(explodedStatement[1]);
    }

    Object GenerateObject(string statement)
    {
        if (!CheckParenthesisCount(statement)) {
            Throw(statement);
        }
        string[] explodedStatement = statement.Split('(');
        string functionName = explodedStatement[0];
        string content = explodedStatement[1].Remove(explodedStatement[1].Length - 1, 1);

        return GetFunctionOutput(functionName, content);
    }

    Object GetFunctionOutput(string functionName, string content)
    {
        return functions[functionName.ToUpper()].Invoke(content);
    }

    void Throw(string info)
    {
        Logger.Throw("Syntax error while parsing event : \n" + info);
        throw null;
    }

    Dictionary<string, System.Func<string, Object>> functions = new Dictionary<string, System.Func<string, Object>>() {
        {  "RANDOM_BUILDING", (args) => 
            {

            }
        }

    };
}
