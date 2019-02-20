using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Text;

public class ScriptInterpreter
{

    public char lineSeparator = ';';
    public List<char> skippers = new List<char>() { '_' };

    public class InterpreterException : System.Exception
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

    public class FormattedComparison
    {
        public IFormattedComparisonElement lefthandData;
        public IFormattedComparisonElement righthandData;
        public string oprtr;
    }

    public interface IFormattedComparisonElement
    {
        string Get();
        string GetLocalization(Localization localizer);
    }

    public class FormattedDataFunction : IFormattedComparisonElement
    {
        public string functionName;
        public string[] parameters = new string[] {};

        public string Get()
        {
            return functionName;
        }

        public string GetLocalization(Localization loc)
        {
            return loc.GetLineFromCategory("scriptAction", functionName,
                FormatArguments(parameters, loc)
            );
        }
    }

    public class FormattedEnvironmentVar : IFormattedComparisonElement
    {
        public string varName;

        public string Get()
        {
            return varName;
        }

        public string GetLocalization(Localization loc)
        {
            return loc.GetLineFromCategory("scriptAction", varName);
        }
    }

    public class FormattedInt : IFormattedComparisonElement
    {
        public int value;

        public string Get()
        {
            return value.ToString();
        }

        public string GetLocalization(Localization loc)
        {
            return Get();
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

    class ObjectInteger : Object
    {
        public int value;

        public ObjectInteger(int _value)
        {
            value = _value;
        }

        public int ToInt()
        {
            return value;
        }

        public override string ToString()
        {
            return ToInt().ToString();
        }
    }

    public ScriptInterpreter()
    {
        LoadActionFunctions();
        LoadDataFunctions();
    }

    ////////////////////////////////////////////////
    ///
    ///     MAIN EVENT CREATION FUNCTION
    /// 
    ///
    public EventManager.GameAction MakeEvent(string eventDeclaration, System.Action<string> errorEvent = null, bool noLogging = false)
    {

        List<EventManager.GameEffect> actions = new List<EventManager.GameEffect>();
        try {
            actions = MakeGameEffects(eventDeclaration);
        }
        catch (InterpreterException e) {
            if (!noLogging) Debug.LogWarning("Interpration failed :\n" + e.ToString());
            errorEvent.Invoke(e.Message);
            return null;
        }
        catch (System.Exception e) {
            if (!noLogging) Debug.LogWarning("Unknown interpreter error - check your script.\n" + e.ToString());
            errorEvent.Invoke("Unknown interpreter error - check your script.\n" + e.ToString());
            return null;
        }
        return new EventManager.GameAction(actions);
    }
    /// 
    /// 
    ////////////////////////////////////////////////

    static string SanitizeCode(string code)
    {
        return code.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("	", "");
    }

    public static void Execute(List<EventManager.GameEffect> fxs)
    {
        foreach (EventManager.GameEffect effect in fxs) {
            effect.GetAction().Invoke();
        }
    }

    public List<EventManager.GameEffect> MakeGameEffects(string eventScript)
    {
        Dictionary<string, Object> context = new Dictionary<string, Object>();
        List<EventManager.GameEffect> effects = InterpretBlock(SanitizeCode(eventScript), context, lineSeparator);

        return effects;
    }

    List<EventManager.GameEffect> InterpretBlock(string block, Dictionary<string, Object> context, char separator, bool executeImmediatly = false)
    {

        List<EventManager.GameEffect> actions = new List<EventManager.GameEffect>();

        // Manual split to avoid splitting control blocks
        List<string> lines = new List<string>();
        StringBuilder lineBuilder = new StringBuilder();
        bool isInsideControlStructure = false;

        // Parsing
        foreach (char character in block) {

            if (isInsideControlStructure) {
                lineBuilder.Append(character);
                if (character == ']') {
                    isInsideControlStructure = false;
                }
                continue;
            }

            if (character == '[') {
                lineBuilder.Append(character);
                isInsideControlStructure = true;
                continue;
            }

            // Not in control structure
            if (character == separator) {
                // Skipping zero length lines
                if (lineBuilder.Length > 0) {
                    string statement = lineBuilder.ToString();
                    lines.Add(statement);
                }
                lineBuilder = new StringBuilder();
                continue;
            }

            lineBuilder.Append(character);
        }

        // Interpretation
        foreach (string line in lines) {

            if (!CheckSyntax(line)) {
                Throw(line);
            }

            // Contains skipper char - don't read line
            if (skippers.Contains(line[0])) {
                continue;
            }

            List<EventManager.GameEffect> gameEffects = InterpretStatement(line, context);
            foreach (EventManager.GameEffect effect in gameEffects) {
                actions.Add(effect);
                if (executeImmediatly) {
                    effect.GetAction().Invoke();
                }
            }
        }

        return actions;
    }

    // Checks syntax is correct on statement
    public static bool CheckSyntax(string statement)
    {
        return
            CheckDelimiterCount(statement, '(', ')') &&
            CheckDelimiterCount(statement, '{', '}') &&
            CheckDelimiterCount(statement, '[', ']')
        ;
    }

    // Checks there is as much ) than there is (
    static bool CheckDelimiterCount(string statement, char blockOpener, char blockCloser)
    {
        return statement.Count(f => f == blockOpener) == statement.Count(f => f == blockCloser);
    }


    // Crashes the interpreter, throws an exception
    static void Throw(string info)
    {
        string msg = info;
        Logger.Error(msg);
        throw new InterpreterException(msg);
    }


    // Interprets list of statement separated by [separator]
    // Returns a list of game effects from that statement (statement could be a condition block)
    List<EventManager.GameEffect> InterpretStatement(string statement, Dictionary<string, Object> context)
    {
        List<EventManager.GameEffect> effects = new List<EventManager.GameEffect>();

        // Chance statement
        if (statement.StartsWith("[")) {
            return UnpackControlStructure(statement, context);
        }

        // Assignation statement
        if (statement.Contains("=")) {
            effects.Add(new EventManager.GameEffect(delegate { Assign(statement, context); }, ""));
        }

        else {
            // XCution statement
            effects.Add(ExecuteActionFunctionFromString(statement, context));
        }

        return effects;
    }

    // Interprets inside of a control structure
    List<EventManager.GameEffect> UnpackControlStructure(string statement, Dictionary<string, Object> context)
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

        List<EventManager.GameEffect> blockEffects = new List<EventManager.GameEffect>();

        // Chance statement
        if (explodedStatement[0].Contains("%")) {
            blockEffects = InterpretChanceStructure(explodedStatement, context);
        }

        // Comparison
        else {
            // Interpreting comparison
            blockEffects = InterpretComparison(explodedStatement, context);
        }

        return blockEffects;
    }

    public bool InterpretComparison(string script)
    {
        script = SanitizeCode(script);
        Dictionary<string, Object> context = new Dictionary<string, Object>();
        string oprtr = GetOperator(script);
        int[] unpackedDatas = UnpackDatas(script, oprtr, context);

        return comparisons[oprtr].Invoke(unpackedDatas[0], unpackedDatas[1]);
    }

    public FormattedComparison FormatComparison(string script)
    {
        script = SanitizeCode(script);
        Dictionary<string, Object> context = new Dictionary<string, Object>();
        string oprtr = GetOperator(script);
        string[] explodedDatas = script.Split(new string[] { oprtr }, System.StringSplitOptions.RemoveEmptyEntries);
        IFormattedComparisonElement[] unpackedDatas = new IFormattedComparisonElement[2];

        // Interpreting functions
        for (int i = 0; i < explodedDatas.Length; i++) {
            //Env
            if(explodedDatas[i][0] == '$') {
                unpackedDatas[i] = new FormattedEnvironmentVar { varName = explodedDatas[i].Replace("$", "") };
            }
            // Function
            else if (dataFunctions.ContainsKey(GetFunctionName(explodedDatas[i]))){
                unpackedDatas[i] = new FormattedDataFunction {
                    functionName = GetFunctionName(explodedDatas[i]),
                    parameters = ExplodeFunction(explodedDatas[i])[1].Split(',')
                };
            }
            // Number
            else{
                unpackedDatas[i] = new FormattedInt { value = System.Convert.ToInt32(explodedDatas[i]) };
            }
        }

        return new FormattedComparison(){lefthandData=unpackedDatas[0], oprtr=oprtr, righthandData=unpackedDatas[1]};

    }

    List<EventManager.GameEffect> InterpretComparison(string[] explodedStatement, Dictionary<string, Object> context)
    {
        List<EventManager.GameEffect> blockEffects = new List<EventManager.GameEffect>();
        blockEffects.Add(
            new EventManager.GameEffect(
                // INSIDE EXEC
                delegate {
                    string oprtr = GetOperator(explodedStatement[0]);
                    int[] unpackedDatas = UnpackDatas(explodedStatement[0], oprtr, context);
                    string comparatedStatement = explodedStatement[1];
                    string elseStatement = explodedStatement[3];
                                  
                    if (comparisons[oprtr].Invoke(unpackedDatas[0], unpackedDatas[1])) {
                        InterpretBlock(comparatedStatement, context, lineSeparator, true);
                    }
                    else {
                        InterpretBlock(elseStatement, context, lineSeparator, true);
                    }                    
                },
                ""
            )
        );
        return blockEffects;
    }

    string GetOperator(string comparison)
    {
        string oprtr = string.Empty;
        foreach (string possibleOperator in comparisons.Keys) {
            if (comparison.Contains(possibleOperator.ToUpper())) {
                oprtr = possibleOperator;
                break;
            }
        }
        if (oprtr.Length <= 0) {
            Throw("Unknown operator :\n" + comparison);
        }
        return oprtr;
    }

    static string GetFunctionName(string functionCall)
    {
        return functionCall.Split('(')[0];
    }

    int[] UnpackDatas(string datas, string oprtr, Dictionary<string, Object> context)
    {
        string[] explodedDatas = datas.Split(new string[] { oprtr }, System.StringSplitOptions.RemoveEmptyEntries);
        int[] unpackedDatas = new int[2];

        // Interpreting functions
        for (int i = 0; i < explodedDatas.Length; i++) {
            if (explodedDatas[i][0] == '$') {
                unpackedDatas[i] = System.Convert.ToInt32(Environment.GetVariable(explodedDatas[i].Remove(0, 1)));
            }
            else if (dataFunctions.ContainsKey(GetFunctionName(explodedDatas[i]))) {
                unpackedDatas[i] = ((ObjectInteger)ExecuteDataFunctionFromString(explodedDatas[i], context)).ToInt();
            }
            else {
                unpackedDatas[i] = System.Convert.ToInt32(explodedDatas[i]);
            }
        }

        return unpackedDatas;
    }

    List<EventManager.GameEffect> InterpretChanceStructure(string[] explodedStatement, Dictionary<string, Object> context)
    {
        // Chances 0-1
        float amount = 0;
        try {
            amount = System.Convert.ToSingle(explodedStatement[0].Replace("%", "")) / 100;
        }
        catch (System.Exception) {
            Throw("Invalid probability in chance structure : \n" + explodedStatement[0]);
        }

        string chanceStatement = explodedStatement[1];
        string elseStatement = explodedStatement[3];

        List<EventManager.GameEffect> chanceBlock = InterpretBlock(chanceStatement, context, lineSeparator);
        List<EventManager.GameEffect> elseBlock = InterpretBlock(elseStatement, context, lineSeparator);

        // Combine chance descriptions
        List<EventManager.GameEffect> blockEffects = new List<EventManager.GameEffect>();
        blockEffects.Add(
            new EventManager.GameEffect(delegate { },
                "chance",
                (100 * amount).ToString()
            )
        );
        // Chance
        foreach (EventManager.GameEffect fx in chanceBlock) {
            blockEffects.Add(
                new EventManager.GameEffect(delegate { }, fx.intention, fx.parameters) { ttColor = fx.ttColor }
            );
        }
        if (chanceBlock.Count <= 0) {
            blockEffects.Add(
                new EventManager.GameEffect(delegate { }, "none")
            );
        }

        blockEffects.Add(
            new EventManager.GameEffect(delegate { },
                "chance",
                (100 * (1 - amount)).ToString()
            )
        );
        // Else
        foreach (EventManager.GameEffect fx in elseBlock) {
            blockEffects.Add(
                new EventManager.GameEffect(delegate { }, fx.intention, fx.parameters) { ttColor = fx.ttColor }
            );
        }
        if (elseBlock.Count <= 0) {
            blockEffects.Add(
                new EventManager.GameEffect(delegate { }, "none")
            );
        }


        // Unique delegate that will roll the dice
        blockEffects[0].SetAction(
            delegate {
                if (Random.value < amount) {
                    foreach (EventManager.GameEffect fx in chanceBlock) {
                        fx.GetAction().Invoke();
                    }
                }
                else {
                    foreach (EventManager.GameEffect fx in elseBlock) {
                        fx.GetAction().Invoke();
                    }
                }
            }
        );

        return blockEffects;
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
            Throw("Unsupported data function \"" + funcName + "\". Supported data functions: \n" + string.Join("\n", GetDataFunctions().ToArray()));
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
        catch (System.Exception) {
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
                catch (System.Exception) {
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
        List<string> args = new List<string>(arguments.Split(','));
        args.Add(argument);
        return string.Join(",", args.ToArray());
    }

    // Execute action from function 
    EventManager.GameEffect ExecuteActionFunctionFromString(string statement, Dictionary<string, Object> context)
    {
        string[] explodedStatement = ExplodeFunction(statement);
        string funcName = explodedStatement[0];
        string arguments = explodedStatement[1];

        if (!actionFunctions.ContainsKey(funcName)) {
            Throw("Unsupported action function \"" + funcName.ToString() + "\". Supported action functions: \n" + string.Join("\n", GetActionFunctions().ToArray()) + "");
        }

        string[] locArguments = FormatArguments(arguments.Split(','));

        return
            new EventManager.GameEffect(
                delegate {
                    actionFunctions[funcName](arguments, context);
                },
                funcName,
                locArguments
            ) { ttColor = GetDataFunctionPreviewColor(funcName) }
        ;

    }

    Tooltip.informationType GetDataFunctionPreviewColor(string functionName)
    {
        switch (functionName) {
            default:
                return Tooltip.informationType.Neutral;

            case "INCREASE_ENERGY_CONSUMPTION_FOR_BUILDING":
            case "INCREASE_FOOD_CONSUMPTION_FOR_POPULATION":
            case "DECREASE_MOOD_FOR_POPULATION":
            case "DESTROY_BUILDING":
                return Tooltip.informationType.Negative;

            case "INCREASE_MOOD_FOR_POPULATION":
            case "DECREASE_FOOD_CONSUMPTION_FOR_POPULATION":
            case "INCREASE_HOUSE_NOTATION":
                return Tooltip.informationType.Positive;
        }
    }

    public string[] FormatArguments(string[] arguments)
    {
        return FormatArguments(arguments, GameManager.instance.localization);
    }

    public static string[] FormatArguments(string[] arguments, Localization loc)
    {

        List<string> argumentsDisplayOrder = new List<string> {
            "population",
            "duration",
            "amount",
            "scheme_id"
        };
        
        Dictionary<string, string> correspondance = new Dictionary<string, string>();

        foreach (string paramName in argumentsDisplayOrder) {
            correspondance[paramName] = string.Empty;
        }

        foreach (string arg in arguments) {
            if (arg.Length <= 0) {
                continue;
            }
            string paramName = arg.Split(':')[0];
            string paramValue = arg.Split(':')[1];
            string convertedArg = "";

            bool skip = false;
            switch (paramName) {
                case "population": convertedArg = loc.GetLineFromCategory("populationType", paramValue); break;
                case "amount": convertedArg = System.Convert.ToSingle(paramValue).ToString("+0;-#"); break;
                case "duration": convertedArg = System.Convert.ToSingle(paramValue).ToString("+0;-#"); break;
                case "scheme_id": convertedArg = loc.GetLineFromCategory("blockName", "block"+paramValue); break;
                default: skip = true; break;
            }

            if (skip) continue;

            correspondance[paramName] = convertedArg;
        }

        List<string> formatted = new List<string>();
        foreach (string paramName in argumentsDisplayOrder) {
            formatted.Add(correspondance[paramName]);
        }

        return formatted.ToArray();
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
                catch (System.Exception) {
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

                int eventId = ((ObjectInteger)context["_EVENT_ID"]).ToInt();

                ConsequencesManager.GenerateMoodModifier(pop, amount, duration, eventId);
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
            "DECREASE_FOOD_CONSUMPTION_FOR_POPULATION", (args, context) => {
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

                ConsequencesManager.GenerateFoodConsumptionModifier(pop, -amount, duration);
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

                int eventId = ((ObjectInteger)context["_EVENT_ID"]).ToInt();

                ConsequencesManager.GenerateMoodModifier(pop, -amount, duration, eventId);

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
                ConsequencesManager.GenerateFlag(block, flag);
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
                ConsequencesManager.ConvertBlock(block, into);
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
                Vector3Int position = ((ObjectPosition)context[GetArgument(args, "position")]).ToVector3Int();

                ConsequencesManager.SpawnBlocksAtLocation(amount, scheme.ID, new Vector2Int(position.x, position.z));
            }
        );
        actionFunctions.Add(
            "LAY_SCHEME_ON_POSITION", (args, context) => {
                args = AddArgument(args, "amount:1");
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
                ConsequencesManager.DestroyFlagTemporarily(block, System.Type.GetType(flag), duration);
            }
        );
        actionFunctions.Add(
            "GAME_OVER", (args, context) => {
                GameManager.instance.ExitToMenu();
            }
        );
        actionFunctions.Add(
            "TRIGGER_EVENT", (args, context) => {
                int eventId = System.Convert.ToInt32(GetArgument(args, "id"));
                GameManager.instance.eventManager.TriggerEvent(eventId);
            }
        );
        actionFunctions.Add(
            "DECLARE_EVENT", (args, context) => {
                int eventId = System.Convert.ToInt32(GetArgument(args, "id"));
                context["_EVENT_ID"] = new ObjectInteger(eventId);
            }
        );
        actionFunctions.Add(
            "DECLARE_ACHIEVEMENT", (args, context) => {
                int eventId = System.Convert.ToInt32(GetArgument(args, "id"));
                context["_ACHIEVEMENT_ID"] = new ObjectInteger(eventId);
            }
        );
        actionFunctions.Add(
            "ECHO", (args, context) => {
                Debug.Log("[SCRIPT ECHO] "+args);
            }
        );
        actionFunctions.Add(
            "UNLOCK_ACHIEVEMENT", (args, context) => {
                int achievementId = System.Convert.ToInt32(GetArgument(args, "id"));
                GameManager.instance.achievementManager.achiever.UnlockAchievement(achievementId);
            }
        );
    }

    /// <summary>
    /// DATA FUNCTIONS
    /// </summary>
    Dictionary<string, System.Func<string, Dictionary<string, Object>, Object>> dataFunctions = new Dictionary<string, System.Func<string, Dictionary<string, Object>, Object>>();
    void LoadDataFunctions()
    {
        dataFunctions.Clear();
        dataFunctions.Add(
            "RANDOM_BUILDING", (args, ctx) => {
                int id = System.Convert.ToInt32(GetArgument(args, "id"));
                Block block = ConsequencesManager.GetRandomBuildingOfId(id);
                if (block == null) {
                    Throw("Could not find any building of ID " + id.ToString());
                }
                return block;
            }
        );
        dataFunctions.Add(
            "BUILDING_COUNT", (args, ctx) => {
                int id = System.Convert.ToInt32(GetArgument(args, "scheme_id"));
                int count = GameManager.instance.systemManager.AllBlocks.FindAll(o => o.scheme.ID == id).Count;
                return new ObjectInteger(count);
            }
        );
        dataFunctions.Add(
             "MOOD", (args, ctx) => {
                 Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                 if (pop == null) {
                     List<string> pops = new List<string>();
                     foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                         pops.Add(existingPop.codeName);
                     }
                     Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                 }
                 return new ObjectInteger(Mathf.RoundToInt(GameManager.instance.populationManager.GetAverageMood(pop)));
             }
        );
        dataFunctions.Add(
             "CITIZEN_COUNT", (args, ctx) => {
                 Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                 if (pop == null) {
                     List<string> pops = new List<string>();
                     foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                         pops.Add(existingPop.codeName);
                     }
                     Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                 }
                 return new ObjectInteger(GameManager.instance.populationManager.populations[pop].citizens.Count);
             }
        );
        dataFunctions.Add(
            "RANDOM_HOUSE", (args, ctx) =>
            {
                Population pop = GameManager.instance.populationManager.GetPopulationByCodename(GetArgument(args, "population"));
                if (pop == null) {
                    List<string> pops = new List<string>();
                    foreach (Population existingPop in GameManager.instance.populationManager.populationTypeList) {
                        pops.Add(existingPop.codeName);
                    }
                    Throw("Invalid population name :\n " + args + "\nPick one from the following : " + string.Join(", ", pops.ToArray()));
                }
                House house = ConsequencesManager.GetRandomHouseOf(pop);
                if (house == null) {
                    Throw("Could not find house belonging to pop " + pop.codeName);
                }
                return house;
            }
        );
        dataFunctions.Add(
            "RANDOM_POSITION", (args, ctx) =>
            {
                Vector2Int coords2D = GameManager.instance.gridManagement.GetRandomCoordinates();
                Vector3Int coords = new Vector3Int(coords2D.x, GameManager.instance.gridManagement.minHeight, coords2D.y);

                return new ObjectPosition() {
                    x = coords.x,
                    y = coords.y,
                    z = coords.z
                };
            }
        );
        dataFunctions.Add(
            "SCHEME", (args, ctx) =>
            {
                int id = System.Convert.ToInt32(GetArgument(args, "id"));
                BlockScheme scheme = GameManager.instance.library.GetBlockByID(id);
                if (scheme == null) {
                    Throw("Invalid scheme id :" + id.ToString());
                }
                return scheme;
            }
        );
        dataFunctions.Add(
            "BUILDING_FROM_HOUSE", (args, ctx) =>
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
        );
        dataFunctions.Add(
            "POSITION_FROM_BUILDING", (args, ctx) =>
            {
                Block block = null;
                try {
                    block = (Block)ctx[GetArgument(args, "building")];
                }
                catch (System.Exception e) {
                    Throw("Impossible cast in " + args + "\n" + e.ToString());
                }

                return new ObjectPosition() {
                    x = block.gridCoordinates.x,
                    y = block.gridCoordinates.y,
                    z = block.gridCoordinates.z
                };
            }
        );
        dataFunctions.Add(
            "SCHEME_FROM_BUILDING", (args, ctx) =>
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
        );
    }

    /// <summary>
    /// COMPARISONS
    /// </summary>
    Dictionary<string, System.Func<int, int, bool>> comparisons = new Dictionary<string, System.Func<int, int, bool>>() {
        {
            "GREATER_THAN", (n1, n2)=>{
                return n1 > n2;
            }
        },
        {
            "LESS_THAN", (n1, n2)=>{
                return n1 < n2;
            }
        },
        {
            "GREATER_OR_EQUAL_THAN", (n1, n2)=>{
                return n1 >= n2;
            }
        },
        {
            "LESS_OR_EQUAL_THAN", (n1, n2)=>{
                return n1 <= n2;
            }
        },
        {
            "EQUAL", (n1, n2)=>{
                return n1 == n2;
            }
        }
    };
}