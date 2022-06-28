using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace LHC.PipeLineUIRefreshSyetem {

    public static class SpallDataBase<K> where K : struct {
        public static Dictionary<string, ValueType> publicDataDict { get;private set; }
        public static DataGroup<K> curDataGroup { get; set; }

        /// <summary>
        /// 重置临时数据dict
        /// </summary>
        /// <param name="tupleArr"></param>
        public static void ResetTempDataDict(params Tuple<string, ValueType>[] tupleArr) {
            publicDataDict = new Dictionary<string, ValueType>();
            tupleArr.Foreach(tuple => publicDataDict.Add(tuple.field1, tuple.field2));
        }
    }

    public abstract class BaseNode {
        public abstract object GetValue();
        
    }

    public class TextColorToken : BaseNode {
        public readonly TextColor colorType;

        public override object GetValue() {
            return colorType;
        }
        public TextColorToken(TextColor colorType) {
            this.colorType = colorType;
        }
    }
    public class NumToken : BaseNode{
        public readonly ValueType value;
        public override object GetValue() {
            return value;
        }

        public NumToken(ValueType value) {
            this.value = value;
        }
    }

    public class VarToken<K> : BaseNode where K:struct{
        public readonly string varName;
        public override object GetValue() {
            //by chenliang
            //begin

//             if (SpallDataBase<K>.publicDataDict!=null&& SpallDataBase<K>.publicDataDict.Keys.Contains(varName)) return SpallDataBase<K>.publicDataDict[varName];
//             else {
//                 K dataType = (K)Enum.Parse(typeof(K), varName);
//                 return SpallDataBase<K>.curDataGroup.GetValueType(dataType);
//             }
//----------------------
            //去除Linq相关
            if (SpallDataBase<K>.publicDataDict != null && SpallDataBase<K>.publicDataDict.ContainsKey(varName))
                return SpallDataBase<K>.publicDataDict[varName];
            else
            {
                K dataType = (K)Enum.Parse(typeof(K), varName);
                return SpallDataBase<K>.curDataGroup.GetValueType(dataType);
            }

            //end
        }
        public VarToken(string varName) {
            this.varName = varName;
        }
    }
    public class StringToken : BaseNode {
        public readonly string str;
        public StringToken(string str) {
            this.str = str;
        }
        public override object GetValue() {
            return str;
        }
    }
    
    public class OpToken : BaseNode {
        public readonly int evalPriority=-1;/*||&&:0 ><=:1 +-:2 乘除:3 #:3 $:3*/
        public readonly Evaluate evalFunc;
        public BaseNode leftNode = null;
        public BaseNode rightNode = null;
        public bool isElse=false;
        
        
        public override object GetValue() {
            if (isElse) return true;
            else {
                if (evalFunc == null) return leftNode.GetValue();
                else return evalFunc(leftNode.GetValue(), rightNode.GetValue());    
            }
        }

        public OpToken(Evaluate func, int evalPriority) {
            this.evalFunc = func;
            this.evalPriority = evalPriority;
        }

        public OpToken(bool isElse) {
            this.isElse = isElse;
        }
        public OpToken() { }
        public void AddOtherTree(OpToken newTree) {
            BaseNode _leftNode = this.leftNode;
            if (_leftNode is OpToken) {
                OpToken opTokenLeft = (OpToken)_leftNode;
                while (opTokenLeft.leftNode is OpToken && opTokenLeft.rightNode != null) {
                    opTokenLeft = (OpToken)opTokenLeft.leftNode;
                }
                if (opTokenLeft.rightNode == null) {
                    opTokenLeft.rightNode = newTree;
                    return;
                }      
            }
            OpToken opToken = this;
            while (opToken.rightNode != null) {
                opToken = (OpToken)opToken.rightNode;
            }
            opToken.rightNode = newTree;
        }
        public OpToken GetNewOpToken(int treeIndex, OpToken newOpToken) {
            OpToken resultAstTree;
            OpToken latestOpToken = SpallInterpreter.latestOpTokenList[treeIndex];
            if (latestOpToken.evalPriority == -1) {
                newOpToken.leftNode = latestOpToken.leftNode;
                resultAstTree = newOpToken;
                SpallInterpreter.latestOpTokenList[treeIndex] = resultAstTree;
            } else {
                if (newOpToken.evalPriority > latestOpToken.evalPriority) {
                    /*这里可能出现(5>4&&3<2)||3>2，这回导致第一个符号(||)会在数字(3)前出现，
                     * 那么新的数字就占据了左枝，而后来的符号(>)出现的时候左枝有数字但右枝没有，
                     * 所以这里左右两边都要判断*/

                    if (latestOpToken.rightNode != null) {
                        newOpToken.leftNode = latestOpToken.rightNode;
                        latestOpToken.rightNode = newOpToken;
                    } else {
                        newOpToken.leftNode = latestOpToken.leftNode;
                        latestOpToken.leftNode = newOpToken;
                    }
                    resultAstTree = this;
                    SpallInterpreter.latestOpTokenList[treeIndex] = newOpToken;
                } else {
                    /*可能会出现比最新枝优先级低，但是比顶层优先级高*/
                    if (newOpToken.evalPriority <= this.evalPriority) {
                        newOpToken.leftNode = this;
                        resultAstTree = newOpToken;
                        SpallInterpreter.latestOpTokenList[treeIndex] = resultAstTree;
                    } else {
                        newOpToken.leftNode = latestOpToken;
                        rightNode = newOpToken;
                        resultAstTree = this;
                        SpallInterpreter.latestOpTokenList[treeIndex] = newOpToken;
                    }
                }
            }
            return resultAstTree;
        }

    }
    public static class SpallInterpreter{
        static char[] opCharArr_1 = new char[6]{'+','-','*','/','#','$'};
        static char[] opCharArr_2 = new char[6] { '=', '>', '<', '&', '|', '!' };
        static char[] opCharArr_3 = new char[3] { '(', ')', '~' };
        public static List<OpToken> latestOpTokenList;
        public static List<OpToken> bracketsOpTokenList;
        public static void AddNumOrVar(int treeIndex, BaseNode newNode) {
            var latestOpToken = latestOpTokenList[treeIndex];
            if (latestOpToken.leftNode == null) latestOpToken.leftNode = newNode;
            else latestOpToken.rightNode = newNode;
        }
       

        public delegate bool CharCondition(char curChar);
        public class Parser {
            public int pointer { get; private set; }
            string source;
            public Parser(int pointer, string source) {
                this.pointer = pointer;
                this.source=source;
            }

            public string Analyse(CharCondition analyseCondition) {
                string token = "";
                while (isSmaller && analyseCondition(curChar)) {
                    token += curChar;
                    PointerForward();
                }
                PointerBack();
                return token;
            }
            public bool isSmaller{
                get{return pointer < source.Length;}
            }
            public char curChar {
                get {return source[pointer];}
            }

            public char nextChar {
                get {
                    if (pointer + 1 == source.Length) {
                        DEBUG.LogError("指针已经到达末尾");
                        return ' ';
                    }else return source[pointer+1]; 
                }
            }

            public char preChar {
                get {
                    if (pointer - 1 == -1) {
                        DEBUG.LogError("指针在开头");
                        return ' ';
                    } else return source[pointer - 1];
                }
            }

            public void PointerForward() {
                pointer++;
            }

            public void PointerBack() {
                pointer--;
            }
        }

        public static Dictionary<T, Tuple<OpToken, OpToken>[]> GetASTreeDict<T, K>(string source)
            where T : struct
            where K : struct {
            Dictionary<T, Tuple<OpToken, OpToken>[]> resultDict = new Dictionary<T, Tuple<OpToken, OpToken>[]>();
            //by chenliang
            //begin

//            source.Split('@').Where(str=>str.Length>0).Foreach(str => {
//-----------------------
            //去除Linq相关
            string[] tmpSourceSplit = source.Split('@');
            List<string> tmpListSourceSplit = new List<string>();
            for (int i = 0, count = tmpSourceSplit.Length; i < count; i++)
            {
                if (tmpSourceSplit[i].Length > 0)
                    tmpListSourceSplit.Add(tmpSourceSplit[i]);
            }
            tmpListSourceSplit.ForEach(str => {

            //end
                Parser parser = new Parser(0, str);
                CharCondition logicNameCond = curChar =>char.IsLetter(curChar) || char.IsNumber(curChar) || curChar == '_';
                //by chenliang
                //begin

//                T pipeLineType = (T)Enum.Parse(typeof(T), parser.Analyse(logicNameCond));
//                parser.PointerForward();
//                CharCondition emptyContCond = curChar => curChar == '\n' || curChar == ' ' || curChar == '\r';
//                parser.Analyse(emptyContCond);
//---------------------
                string tmpStrParserAnalyse = "";
                while (parser.isSmaller &&
                    (char.IsLetter(parser.curChar) || char.IsNumber(parser.curChar) || parser.curChar == '_'))
                {
                    tmpStrParserAnalyse += parser.curChar;
                    parser.PointerForward();
                }
                parser.PointerBack();
                T pipeLineType = (T)Enum.Parse(typeof(T), tmpStrParserAnalyse);
                parser.PointerForward();

                CharCondition emptyContCond = curChar => curChar == '\n' || curChar == ' ' || curChar == '\r';
                while (parser.isSmaller &&
                    (parser.curChar == '\n' || parser.curChar == ' ' || parser.curChar == '\r'))
                    parser.PointerForward();
                parser.PointerBack();

                //end

                string token_logic=str.Substring(parser.pointer);
                List<string> logicList=new List<string>();
                Func<string,bool> logicCondition=logic=>{
                    bool result=true;
                    if(logic.Length==0) result=false;
                    else if (logic == "\r" || logic == "\n") {
                        result = false;
                    }
                    else{
                        int emptyCount = 0;
                        for (int i = 0; i < logic.Length; i++) {
                        if(logic.Substring(i, 1)==" ")
                            emptyCount++;
                        }
                        result=!(emptyCount==logic.Length);
                    }
                    return result;
                };
                //by chenliang
                //begin

//                 var tupleArr = Regex.Split(token_logic, "\n", RegexOptions.IgnoreCase).Where(x => logicCondition(x)).
//                     Select( x=> {
//                     var arr = Regex.Split(x, " is ", RegexOptions.IgnoreCase);
//                     if (arr.Length > 2 || arr.Length == 0) DEBUG.LogError("语法错误");
//                     for (int i = 0; i < arr.Length; i++) {
//                         arr[i] = arr[i].Replace(" ", "");
//                         arr[i] = arr[i].Replace("\n", "");
//                         arr[i] = arr[i].Replace("\r", "");
//                     }
//                     if (arr.Length == 1) return new Tuple<OpToken, OpToken>(GetASTree<K>(arr[0]), null);
//                     else return new Tuple<OpToken, OpToken>(GetASTree<K>(arr[0]), GetASTree<K>(arr[1]));
//                 }).ToArray();
//-----------------------------
                //去除Linq相关
                string[] tmpTokenLogicSplit = Regex.Split(token_logic, "\n", RegexOptions.IgnoreCase);
                List<Tuple<OpToken, OpToken>> tmpListTokenLogic = new List<Tuple<OpToken, OpToken>>();
                for (int i = 0, count = tmpTokenLogicSplit.Length; i < count; i++)
                {
                    string tmpStrTokenLogic = tmpTokenLogicSplit[i];
                    if (logicCondition(tmpStrTokenLogic))
                    {
                        string[] tmpSplit = Regex.Split(tmpStrTokenLogic, " is ", RegexOptions.IgnoreCase);
                        if (tmpSplit.Length > 2 || tmpSplit.Length == 0)
                        {
                            DEBUG.LogError("语法错误");
                            continue;
                        }
                        for (int j = 0, jCount = tmpSplit.Length; j < jCount; j++)
                        {
                            tmpSplit[j] = tmpSplit[j].Replace(" ", "");
                            tmpSplit[j] = tmpSplit[j].Replace("\n", "");
                            tmpSplit[j] = tmpSplit[j].Replace("\r", "");
                        }
                        if (tmpSplit.Length == 1)
                            tmpListTokenLogic.Add(new Tuple<OpToken, OpToken>(GetASTree<K>(tmpSplit[0]), null));
                        else
                            tmpListTokenLogic.Add(new Tuple<OpToken, OpToken>(GetASTree<K>(tmpSplit[0]), GetASTree<K>(tmpSplit[1])));
                    }
                }
                Tuple<OpToken, OpToken>[] tupleArr = new Tuple<OpToken, OpToken>[tmpListTokenLogic.Count];
                for (int i = 0, count = tmpListTokenLogic.Count; i < count; i++)
                    tupleArr[i] = tmpListTokenLogic[i];

                //end
                
                resultDict.Add(pipeLineType, tupleArr);
            });
            //resultDict.Foreach(x => x.Value.Foreach(y => DEBUG.Log(x.Key + "_____" + y.field1 + "___is___" + y.field2)));
            return resultDict;
        }

        /*将字符串转化为语法树
         * 符号优先级高的的成为旧树的右枝（深度搜索直到找到空缺）
         * 符号优先级低的成为新树，旧树成为其左枝
         * 遇到"("则treeIndex++ ")"则treeIndex-- 
         * 不同treeIndex代表不同的树
         * 最后将所有的树整合在一起
         * treeIndex高的成为低的的空缺右枝*/
        public static OpToken GetASTree<K>(string source) where K: struct{
            //DEBUG.Log(source);
            List<OpToken> astTreeList = new List<OpToken>();
            latestOpTokenList = new List<OpToken>();
            bracketsOpTokenList = new List<OpToken>();
            astTreeList.Add(new OpToken());
            latestOpTokenList.Add(astTreeList[0]);
            Parser parser = new Parser(0, source);
            int treeIndex = 0;
            Action<OpToken> addList = opToken => {
                if (astTreeList.Count <= treeIndex) {
                    astTreeList.Add(opToken);
                    SpallInterpreter.latestOpTokenList.Add(opToken);
                } else astTreeList[treeIndex] = astTreeList[treeIndex].GetNewOpToken(treeIndex, opToken);
            };

            while (parser.isSmaller) {
                //DEBUG.Log(parser.curChar);                
                if (char.IsLetter(parser.curChar)) {
                    CharCondition cond = character =>
                        char.IsLetter(character) || char.IsNumber(character) || character == '_';
                    //by chenliang
                    //begin

//                    var token = parser.Analyse(cond);
//---------------------------
                    var token = "";
                    while (parser.isSmaller &&
                        (char.IsLetter(parser.curChar) || char.IsNumber(parser.curChar) || parser.curChar == '_'))
                    {
                        token += parser.curChar;
                        parser.PointerForward();
                    }
                    parser.PointerBack();

                    //end
                    var textColorEnum = Enum.GetValues(typeof(TextColor)).GetEnumerator();
                    bool isTextColor=false;
                    while (textColorEnum.MoveNext()) {
                        if (token == textColorEnum.Current.ToString()) {
                            SpallInterpreter.AddNumOrVar(treeIndex, new TextColorToken((TextColor)textColorEnum.Current));
                            isTextColor = true;
                            break;
                        }
                    }
                    if (!isTextColor) {
                        if (token == "else") addList(new OpToken(true)); 
                        else if (token == "empty") SpallInterpreter.AddNumOrVar(treeIndex, new StringToken(""));
                        else SpallInterpreter.AddNumOrVar(treeIndex, new VarToken<K>(token));    
                    }
                    
                } else if (char.IsNumber(parser.curChar)) {
                    CharCondition cond = character => character == '.' || char.IsNumber(character);
                    //by chenliang
                    //begin

//                    var token = parser.Analyse(cond);
//-----------------------
                    var token = "";
                    while (parser.isSmaller &&
                        (parser.curChar == '.' || char.IsNumber(parser.curChar)))
                    {
                        token += parser.curChar;
                        parser.PointerForward();
                    }
                    parser.PointerBack();

                    //end
                    int intValue;
                    if (int.TryParse(token, out intValue)) SpallInterpreter.AddNumOrVar(treeIndex, new NumToken(intValue));
                    else {
                        float floatValue;
                        if (float.TryParse(token, out floatValue)) SpallInterpreter.AddNumOrVar(treeIndex, new NumToken(floatValue));
                        else DEBUG.LogError("只支持int和float两种类型");
                    }
                } else{
                    switch (parser.curChar) {
                        case '+': addList(new OpToken(EvaluateFunc.GetAddFunc(), 2)); break;
                        case '-': addList(new OpToken(EvaluateFunc.GetMinusFunc(), 2)); break;
                        case '*': addList(new OpToken(EvaluateFunc.GetMultiplyFunc(), 3)); break;
                        case '/': addList(new OpToken(EvaluateFunc.GetDivideFunc(), 3)); break;
                        case '#': addList(new OpToken(EvaluateFunc.GetSetTextColorFunc(), 3)); break;
                        case '$': addList(new OpToken(EvaluateFunc.GetFormatFunc(), 3)); break;
                        case '>':
                            if (parser.nextChar == '=') {
                                addList(new OpToken(EvaluateFunc.GetGreaterOrEqualFunc(), 1));
                                parser.PointerForward();
                            } else addList(new OpToken(EvaluateFunc.GetGreaterFunc(), 1));
                            break;

                        case '<':
                            if (parser.nextChar == '=') {
                                addList(new OpToken(EvaluateFunc.GetLessOrEqualFunc(), 1));
                                parser.PointerForward();
                            } else addList(new OpToken(EvaluateFunc.GetLessFunc(), 1));
                            break;

                        case '=':
                            if (parser.nextChar == '=') {
                                addList(new OpToken(EvaluateFunc.GetEqualFunc(), 1));
                                parser.PointerForward();
                            } else {
                                //出现在变量赋值的情况中
                            }
                            break;

                        case '！':
                            if (parser.nextChar == '=') {
                                addList(new OpToken(EvaluateFunc.GetUnequalFunc(), 1));
                                parser.PointerForward();
                            } else {
                                //出现在对变量取反的情况中
                            }
                            break;

                        case '&':
                            if (parser.nextChar == '&') {
                                addList(new OpToken(EvaluateFunc.GetAndFunc(), 0));
                                parser.PointerForward();
                            } else DEBUG.LogError("请使用 &&");
                            break;

                        case '|':
                            if (parser.nextChar == '|') {
                                addList(new OpToken(EvaluateFunc.GetOrFunc(), 0));
                                parser.PointerForward();
                            } else DEBUG.LogError("请使用 ||");
                            break;
                        case '(':
                            treeIndex++;
                            if (astTreeList.Count <= treeIndex) {
                                astTreeList.Add(new OpToken());
                                SpallInterpreter.latestOpTokenList.Add(astTreeList[treeIndex]);
                            }
                            break;
                        case')':
                            treeIndex--;
                            break;
                        case '\"':
                            parser.PointerForward();
                            CharCondition cond = character => char.IsLetter(character) || char.IsNumber(character) || character == '_'
                                || character == '{' || character == '}' || character == '×';
                            //by chenliang
                            //begin

//                            var token = parser.Analyse(cond);
//-----------------------------------
                            var token = "";
                            while (parser.isSmaller &&
                                (char.IsLetter(parser.curChar) || char.IsNumber(parser.curChar) || parser.curChar == '_'
                                || parser.curChar == '{' || parser.curChar == '}' || parser.curChar == '×'))
                            {
                                token += parser.curChar;
                                parser.PointerForward();
                            }
                            parser.PointerBack();

                            //end
                            SpallInterpreter.AddNumOrVar(treeIndex, new StringToken(token));
                            break;
    
                        default:
                            DEBUG.LogError("出现非法字符");
                            break;
                    }
                } 
                parser.PointerForward();
            }
            
            if (astTreeList.Count > 1) {
                for (int i = astTreeList.Count - 1; i > 0; i--)
                    astTreeList[i - 1].AddOtherTree(astTreeList[i]);
            }
           
            //DEBUG.Log(astTreeList[0].GetValue());
            return astTreeList[0];
        }
    }



}
