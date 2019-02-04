using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class EventInterpreter
{

    public string lineSeparator = ";";

    class InterpreterException : System.Exception
    {
        public InterpreterException()
        {
        }

        public InterpreterException(string message)
            : base(message)
        {
        }

        public InterpreterException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

    class ObjectPosition : Object
    {
        public int x;
        public int y;
        public int z;

        public ObjectPosition(string data)
        {
            x = System.Convert.ToInt32(data.Split(',')[0]);
            y = System.Convert.ToInt32(data.Split(',')[1]);
            z = System.Convert.ToInt32(data.Split(',')[2]);
        }

        public ObjectPosition()
        {

        }

        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, z);
        }
    }

    public EventInterpreter()
    {
        LoadActionFunctions();
    }

    ////////////////////////////////////////////////
    ///
    ///     MAIN EVENT CREATION FUNCTION
    /// 
    ///
    public EventManager.GameEvent MakeEvent(int id, string eventDeclaration)
    {
        Dictionary<string, Object> context = new Dictionary<string, Object>();
        eventDeclaration = eventDeclaration.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("	", "");

        // Double separator (;;) for regular lines
        List<System.Action> actions = new List<System.Action>();
        try {
            actions = InterpretBlock(eventDeclaration, context, lineSeparator + lineSeparator);
        }
        catch (InterpreterException e) {
            GameManager.instance.eventManager.interpreterError.Invoke(e.Message);
        }
        catch(System.Exception e) {
            Debug.LogWarning(e);
            GameManager.instance.eventManager.interpreterError.Invoke("Unknown interpreter error - check your script.\n"+e.Message);
            return null;
        }
        return new EventManager.GameEvent(id, actions);
    }
    /// 
    /// 
    ////////////////////////////////////////////////



    List<System.Action> InterpretBlock(string block, Dictionary<string, Object> context, string separator, int depth = 0)
    {

        List<System.Action> actions = new List<System.Action>();
        List<string> lines = new List<string>(block.Split(new[] { separator }, System.StringSplitOptions.RemoveEmptyEntries));

        foreach (string line in lines) {

            if (!CheckSyntax(line)) {
                Throw(line);
            }
            actions.Add(InterpretStatement(line, context, depth));

        }

        return actions;
    }

    // Interprets list of statement separated by [separator]
    List<System.Action> InterpretBlock(string block, Dictionary<string, Object> context, char separator)
    {
        return InterpretBlock(block, context, separator.ToString());
    }

    // Checks syntax is correct on statement
    bool CheckSyntax(string statement)
    {
        return
            CheckDelimiterCount(statement, '(', ')') &&
            CheckDelimiterCount(statement, '{', '}') &&
            CheckDelimiterCount(statement, '[', ']')
        ;
    }

    // Checks there is as much ) than there is (
    bool CheckDelimiterCount(string statement, char blockOpener, char blockCloser)
    {
        return statement.Count(f => f == blockOpener) == statement.Count(f => f == blockCloser);
    }


    // Crashes the interpreter, throws an exception
    static void Throw(string info)
    {
        string msg = "Syntax error while parsing event : \n" + info;
        Logger.Error(msg);
        GameManager.instance.eventManager.interpreterError.Invoke(msg);
        throw new InterpreterException(msg);
    }

    // Returns an action based on the string statement
    System.Action InterpretStatement(string statement, Dictionary<string, Object> context, int depth = 0)
    {
        System.Action action = delegate { };


        // Chance statement
        if (statement.StartsWith("[")) {
            return UnpackControlStructure(statement, context, depth);
        }

        // Assignation statement
        if (statement.Contains("=")) {
            Assign(statement, context);
            return delegate { };
        }

        // XCution statement
        return ExecuteActionFunctionFromString(statement, context);

    }

    // Interprets inside of a control structure
    System.Action UnpackControlStructure(string statement, Dictionary<string, Object> context, int depth = 0)
    {
        string statementInside = statement.Remove(statement.Length - 1, 1).Remove(0, 1);

        if (!CheckDelimiterCount(statementInside, '(', ')')) {
            Throw("Wrong parenthesis count :\n" + statement);
        }
        string[] explodedStatement = statementInside.Split(new char[] { '{', '}' }, System.StringSplitOptions.None);

        if (explodedStatement.Length != 5) {
            Throw("Invalid control structure :\n" + statement);
        }

        if (!explodedStatement[2].Contains("ELSE")) {
            Throw("No ELSE statement in control structure :\n" + string.Join("\n", explodedStatement));
        }

        // Chances 0-1
        float amount = 0;
        try {
            amount = System.Convert.ToSingle(explodedStatement[0].Replace("%", "")) / 100;
        }
        catch (System.Exception e) {
            Throw("Invalid probability in control structure : \n" + explodedStatement[0]);
        }

        string chanceStatement = explodedStatement[1];
        string elseStatement = explodedStatement[3];

        return delegate {
            List<System.Action> blockActions = new List<System.Action>();
            float rnd = Random.value;
            if (rnd <= amount) {
                blockActions = InterpretBlock(chanceStatement, context, lineSeparator + depth.ToString(), depth + 1);
            }
            else {
                blockActions = InterpretBlock(elseStatement, context, lineSeparator + depth.ToString(), depth + 1);
            }
            foreach (System.Action action in blockActions) {
                action.Invoke();
            }
        };
    }

    // Assigns data to a variable in the context
    void Assign(string assignationStatement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = assignationStatement.Split('=');
        if (explodedStatement.Length <= 0 || explodedStatement.Length > 2) {
            Throw(assignationStatement);
        }
        string varName = explodedStatement[0];
        Object generated = ExecuteDataFunctionFromString(explodedStatement[1], context);

        context[varName] = generated;
    }

    // Fetches data from a function taken from a statement
    Object ExecuteDataFunctionFromString(string statement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = ExplodeFunction(statement);
        string funcName = explodedStatement[0];
        string content = explodedStatement[1];

        if (!dataFunctions.ContainsKey(funcName)) {
            Throw("Unsupported data function \"" + funcName + "\". Supported data functions: \n" + string.Join("\n",  GetDataFunctions().ToArray()));
        }

        return dataFunctions[funcName].Invoke(content, context);
    }


    // Returns function name and string content
    string[] ExplodeFunction(string statement)
    {
        if (!CheckDelimiterCount(statement, '(', ')')) {
            Throw(statement);
        }
        string[] explodedStatement = statement.Split('(');
        string functionName = explodedStatement[0].ToUpper();
        string content = "";
        try {
            content = explodedStatement[1].Remove(explodedStatement[1].Length - 1, 1);
        }
        catch (System.Exception e) {
            Throw("Invalid function call :\n" + statement);
        }

        return new string[] { functionName, content };
    }


    // Get specific string argument from argument list
    static string GetArgument(string arguments, string key)
    {
        foreach (string statement in arguments.Split(new char[] { ',', ')', '(' }, System.StringSplitOptions.RemoveEmptyEntries)) {
            string[] explodedStatement = statement.Split(':');
            if (explodedStatement[0] == key) {
                try {
                    return explodedStatement[1];
                }
                catch (System.Exception e) {
                    Throw("Invalid argument : " + statement);
                }
            }
        }
        Throw("Missing argument \"" + key + "\" in args \"" + arguments + "\"");
        return "";
    }

    // Add an argument to an argument list
    static string AddArgument(string arguments, string argument)
    {
        string[] args = arguments.Split(',');
        args[args.Length] = argument;
        return string.Join(",", args);
    }

    // Execute action from function 
    System.Action ExecuteActionFunctionFromString(string statement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = ExplodeFunction(statement);
        string funcName = explodedStatement[0];
        string arguments = explodedStatement[1];

        if (!actionFunctions.ContainsKey(funcName)) {
            Throw("Unsupported action function \"" + funcName.ToString() + "\". Supported action functions: \n" + string.Join("\n", GetActionFunctions().ToArray())+"");
        }

        return delegate { actionFunctions[funcName](arguments, context); };

    }

    // Returns list of supported action functions
    List<string> GetActionFunctions()
    {
        List<string> funcs = new List<string>();
        foreach (KeyValuePair<string, System.Action<string, Dictionary<string, Object>>> function in actionFunctions) {
            funcs.Add(function.Key.ToString());
        }
        return funcs;
    }

    // Returns list of supported data functions
    List<string> GetDataFunctions()
    {
        List<string> funcs = new List<string>();
        foreach (KeyValuePair<string, System.Func<string, Dictionary<string, Object>, Object>> function in dataFunctions) {
            funcs.Add(function.Key);
        }
        return funcs;
    }



    /// <summary>
    /// ACTION FUNCTIONS
    /// </summary>
    Dictionary<string, System.Action<string, Dictionary<string, Object>>> actionFunctions = new Dictionary<string, System.Action<string, Dictionary<string, Object>>>();
    void LoadActionFunctions()
    {
        actionFunctions.Clear();
        actionFunctions.Add(
            "INCREASE_ENERGY_CONSUMPTION_FOR_BUILDING", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n");
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                int amount = System.Convert.ToInt32(GetArgument(args, "amount"));

                ConsequencesManager.GenerateConsumptionModifier(block, amount, duration);
            }
        );
        actionFunctions.Add(
            "INCREASE_MOOD_FOR_POPULATION", (args, context) => {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                if (pop == null) {
                    List<string> pops = new List<string>();
                    foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                int amount = System.Convert.ToInt32(GetArgument(args, "amount"));

                ConsequencesManager.GenerateMoodModifier(pop, amount, duration);
            }
        );
        actionFunctions.Add(
            "INCREASE_FOOD_CONSUMPTION_FOR_POPULATION", (args, context) => {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                if (pop == null) {
                    List<string> pops = new List<string>();
                    foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                float amount = System.Convert.ToSingle(GetArgument(args, "amount"));

                ConsequencesManager.GenerateFoodConsumptionModifier(pop, amount, duration);
            }
        );
        actionFunctions.Add(
            "INCREASE_HOUSE_NOTATION", (args, context) => {
                House house = null;
                try {
                    house = (House)context[GetArgument(args, "house")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                float amount = System.Convert.ToSingle(GetArgument(args, "amount"));
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));

                ConsequencesManager.ChangeHouseNotation(house, amount, duration);
            }
        );
        actionFunctions.Add(
            "DECREASE_MOOD_FOR_POPULATION", (args, context) => {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                if (pop == null) {
                    List<string> pops = new List<string>();
                    foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                int amount = System.Convert.ToInt32(GetArgument(args, "amount"));

                ConsequencesManager.GenerateMoodModifier(pop, -amount, duration);

            }
        );
        actionFunctions.Add(
            "ADD_FLAG_MODIFIER_ON_BUILDING_FOR_DURATION", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                string flagModifier = GetArgument(args, "flagModifier");

                ConsequencesManager.ModifyFlag(block, flagModifier, duration);
            }
        );
        actionFunctions.Add(
            "ADD_FLAG_ON_BUILDING_FOR_DURATION", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                string flag = GetArgument(args, "flag");

                ConsequencesManager.GenerateTempFlag(block, flag, duration);
            }
        );
        actionFunctions.Add(
            "ADD_FLAG_ON_BUILDING", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                string flag = GetArgument(args, "flag");

                // TODO
                //ConsequencesManager.GenerateFlag(block, flag);
            }
        );
        actionFunctions.Add(
            "ADD_STATE_ON_BUILDING", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

                State state = (State)System.Enum.Parse(typeof(State), GetArgument(args, "state"));

                ConsequencesManager.AddState(block, state);
            }
        );
        actionFunctions.Add(
            "ADD_SETTLERS_TO_NEXT_WAVE", (args, context) => {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                if (pop == null) {
                    List<string> pops = new List<string>();
                    foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                }
                int amount = System.Convert.ToInt32(GetArgument(args, "amount"));

                ConsequencesManager.AddSettlerBonusForNextWave(pop, amount);
            }
        );
        actionFunctions.Add(
            "DESTROY_BUILDING", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                ConsequencesManager.DestroyBlock(block);
            }
        );
        actionFunctions.Add(
            "CHANGE_BUILDING_SCHEME", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

                BlockScheme scheme = null;
                try {
                    scheme = (BlockScheme)context[GetArgument(args, "scheme")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

                int id = System.Convert.ToInt32(GetArgument(args, "id"));
                BlockScheme into = GameManager.instance.library.GetBlockByID(id);

                // TODO
                // ConsequencesManager.ConvertBlock(block, into);
            }
        );
        actionFunctions.Add(
            "LAY_MULTIPLE_SCHEME_ON_POSITION", (args, context) => {
                BlockScheme scheme = null;
                try {
                    scheme = (BlockScheme)context[GetArgument(args, "scheme")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

                int amount = System.Convert.ToInt32(GetArgument(args, "amount"));
                Vector3Int position = new ObjectPosition(GetArgument(args, "position")).ToVector3Int();

                ConsequencesManager.SpawnBlocksAtLocation(amount, scheme.ID, position);
            }
        );
        actionFunctions.Add(
            "LAY_SCHEME_ON_POSITION", (args, context) => {
                AddArgument(args, "amount:1");
                actionFunctions["LAY_MULTIPLE_SCHEME_ON_POSITION"](args, context);
            }
        );
        actionFunctions.Add(
            "REMOVE_FLAG_FROM_BUILDING", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                string flag = GetArgument(args, "flag");

                ConsequencesManager.DestroyFlag(block, System.Type.GetType(flag));
            }
        );
        actionFunctions.Add(
            "REMOVE_FLAG_FROM_BUILDING_FOR_DURATION", (args, context) => {
                Block block = null;
                try {
                    block = (Block)context[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }
                int duration = System.Convert.ToInt32(GetArgument(args, "duration"));
                string flag = GetArgument(args, "flag");

                // TODO
                //ConsequencesManager.TempDestroyFlag(block, FlagReader.Get flag, duration);
            }
        );
    }

    /// <summary>
    /// DATA FUNCTIONS
    /// </summary>
    Dictionary<string, System.Func<string, Dictionary<string, Object>, Object>> dataFunctions = new Dictionary<string, System.Func<string, Dictionary<string, Object>, Object>>() {

       {  "RANDOM_BUILDING", (args, ctx) =>
           {
               Debug.Log(GetArgument(args, "id"));
               int id = System.Convert.ToInt32(GetArgument(args, "id"));
               Block block = ConsequencesManager.GetRandomBuildingOfId(id);
               if (block == null) {
                   Throw("Could not find any building of ID "+id.ToString());
               }
               return block;
           }
       },
       {   "RANDOM_HOUSE", (args, ctx) =>
           {
               Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
               if (pop == null){
                    List<string> pops = new List<string>();
                    foreach(Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args+"\nPick one from the following : "+string.Join(", ", pops.ToArray()));
                }
               House house = ConsequencesManager.GetRandomHouseOf(pop);
               if (house == null) {
                   Throw("Could not find house belonging to pop "+pop.codeName);
               }
               return house;
           }
       },
       {   "SCHEME", (args, ctx) =>
           {
               int id = System.Convert.ToInt32(GetArgument(args, "id"));
               BlockScheme scheme = GameManager.instance.library.GetBlockByID(id);
               if (scheme == null) {
                   Throw("Invalid scheme id :"+id.ToString());
               }
               return scheme;
           }
       },
       {   "BUILDING_FROM_HOUSE", (args, ctx) =>
           {
                House house = null;
                try {
                    house = (House)ctx[GetArgument(args, "house")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

               return house.block;
           }
       },
       {   "POSITION_FROM_BUILDING", (args, ctx) =>
           {
                Block block = null;
                try {
                    block = (Block)ctx[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

               return new ObjectPosition(){
                   x = block.gridCoordinates.x,
                   y = block.gridCoordinates.y,
                   z = block.gridCoordinates.z
                };
           }
       },
       {   "SCHEME_FROM_BUILDING", (args, ctx) =>
           {
                Block block = null;
                try {
                    block = (Block)ctx[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

               return block.scheme;
           }
       }

    };


}
