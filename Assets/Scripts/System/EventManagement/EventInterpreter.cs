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

    // Checks syntax is correct on statement
    bool CheckSyntax(string statement)
    {
        return 
            CheckParenthesisCount(statement)
        ;
    }

    // Checks there is as much ) than there is (
    bool CheckParenthesisCount(string statement)
    {
        return Regex.Matches(statement, @"\(").Count == Regex.Matches(statement, @"/)").Count;
    }

    // Returns an action based on the string statement
    System.Action InterpretStatement(string statement, Dictionary<string, Object> context)
    {
        System.Action action = delegate { };

        // Assignation statement
        if (statement.Contains("=")) {
            Assign(statement, context);
            return delegate { };
        }

        // XCution statement
        return ExecuteActionFunctionFromString(statement, context);
    }

    // Assigns data to a variable in the context
    void Assign(string assignationStatement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = assignationStatement.Split('=');
        if (explodedStatement.Length <= 0 || explodedStatement.Length > 2) {
            Throw(assignationStatement);
        }
        string varName = explodedStatement[0];
        Object generated = ExecuteDataFunctionFromString(explodedStatement[1]);

        context[varName] = generated;
    }

    // Fetches data from a function taken from a statement
    Object ExecuteDataFunctionFromString(string statement)
    {
        string[] explodedStatement = ExplodeFunction(statement);
        string funcName = explodedStatement[0];
        string content = explodedStatement[1];

        return dataFunctions[funcName].Invoke(content);
    }

    // Returns function name and string content
    string[] ExplodeFunction(string statement)
    {
        if (!CheckParenthesisCount(statement)) {
            Throw(statement);
        }
        string[] explodedStatement = statement.Split('(');
        string functionName = explodedStatement[0].ToUpper();
        string content = explodedStatement[1].Remove(explodedStatement[1].Length - 1, 1);

        return new string[] { functionName, content };
    }

    // Crashes the interpreter, throws an exception
    static void Throw(string info)
    {
        Logger.Throw("Syntax error while parsing event : \n" + info);
        throw null;
    }

    // Get specific string argument from argument list
    static string GetArgument(string args, string key)
    {
        foreach(string statement in args.Split(',')) {
            string[] explodedStatement = statement.Split(':');
            if (explodedStatement[0] == key) {
                return explodedStatement[1];
            }
        }
        Throw("Missing argument \"" + key + "\" in args \"" + args + "\"");
        return "";
    }

    // Execute action from function 
    System.Action ExecuteActionFunctionFromString(string statement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = ExplodeFunction(statement);
        string funcName = explodedStatement[0];
        string arguments = explodedStatement[1];

        return delegate { actionFunctions[funcName](arguments)};
    }






    Dictionary<string, System.Action<string>> actionFunctions = new Dictionary<string, System.Action<string>>() {
        { "INCREASE_ENERGY_CONSUMPTION", (args) =>
            {

            }
        }
    };



    /// <summary>
    /// DATA FUNCTIONS
    /// </summary>
    Dictionary<string, System.Func<string, Object>> dataFunctions = new Dictionary<string, System.Func<string, Object>>() {

        {  "RANDOM_BUILDING", (args) => 
            {
                int id = System.Convert.ToInt32(GetArgument(args, "id"));
                return ConsequencesManager.GetRandomBuildingOfId(id);
            }
        },
        {   "RANDOM_HOUSE", (args) =>
            {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                return ConsequencesManager.GetRandomHouseOf(pop);
            }
        }

    };




}
