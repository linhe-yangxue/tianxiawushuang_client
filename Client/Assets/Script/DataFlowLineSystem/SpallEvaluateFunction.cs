using UnityEngine;
using System.Collections;
using System;

namespace LHC.PipeLineUIRefreshSyetem {

    public delegate object Evaluate(object leftValue, object rightValue);
    public delegate object UnitaryEvaluate(BaseNode baseNode);/*一元操作符可能在变量后面也可能在常数前面
                                                               * 而跟在符号前面的情况，例如"!=" 
                                                               * 这里将"!="视作一个独立的符号
                                                               * ！后面追加括号的情况
                                                               * 例如!(a>b&&c<d)暂不考虑*/

    public enum TextColor {
        GREEN,
        RED,
        BLACK,
        WHITE,
        GREY,
    }
    public static class EvaluateFunc {
       

        public static Evaluate GetSetTextColorFunc() {
            return (leftValue, rightValue) => {
                string colorStr = "";
                /*之后颜色字符串读表*/
                switch ((TextColor)rightValue) {
                    case TextColor.BLACK: colorStr = "000000"; break;
                    case TextColor.GREEN: colorStr = "33FF33"; break;
                    case TextColor.RED: colorStr = "FF0000"; break;
                    case TextColor.WHITE: colorStr = "FFFFFF"; break;
                    case TextColor.GREY: colorStr = "666666"; break;
                }
                return "[" + colorStr + "]" + leftValue.ToString() + "[-]";
            };
        }
        public static Evaluate GetFormatFunc() {
            return (leftValue, rightValue) => string.Format((string)leftValue, rightValue);
        }

        public static Evaluate GetOrFunc() {
            return (leftValue, rightValue) => (bool)leftValue || (bool)rightValue;
        }
        public static Evaluate GetAndFunc() {
            return (leftValue, rightValue) => (bool)leftValue && (bool)rightValue;
        }
        public static Evaluate GetEqualFunc() {
            return (leftValue, rightValue) => (int)leftValue == (int)rightValue;
        }

        public static Evaluate GetUnequalFunc() {
            return (leftValue, rightValue) => (int)leftValue != (int)rightValue;
        }

        public static Evaluate GetLessOrEqualFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue <= (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue <= (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue <= (int)rightValue;
                else return (int)leftValue <= (int)rightValue;
            };
        }

        public static Evaluate GetLessFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue < (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue < (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue < (int)rightValue;
                else return (int)leftValue < (int)rightValue;
            };
        }


        public static Evaluate GetGreaterOrEqualFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue >= (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue >= (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue >= (int)rightValue;
                else return (int)leftValue >= (int)rightValue;
            };
        }

        public static Evaluate GetGreaterFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue > (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue > (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue > (int)rightValue;
                else return (int)leftValue > (int)rightValue;
            };
        }


        public static Evaluate GetAddFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is string || rightValue is string) return leftValue.ToString() + rightValue.ToString();
                else {
                    if (leftValue is float && rightValue is float) return (float)leftValue + (float)rightValue;
                    else if (leftValue is int && rightValue is float) return (int)leftValue + (float)rightValue;
                    else if (leftValue is float && rightValue is int) return (float)leftValue + (int)rightValue;
                    else return (int)leftValue + (int)rightValue;    
                }
                
            };
        }

        public static Evaluate GetMinusFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue - (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue - (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue - (int)rightValue;
                else return (int)leftValue - (int)rightValue;
            };
        }

        public static Evaluate GetMultiplyFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue * (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue * (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue * (int)rightValue;
                else return (int)leftValue * (int)rightValue;
            };
        }

        public static Evaluate GetDivideFunc() {
            return (leftValue, rightValue) => {
                if (leftValue is float && rightValue is float) return (float)leftValue / (float)rightValue;
                else if (leftValue is int && rightValue is float) return (int)leftValue / (float)rightValue;
                else if (leftValue is float && rightValue is int) return (float)leftValue / (float)((int)rightValue);
                else return (float)((int)leftValue) / (int)(rightValue);
            };
        }

        public static Evaluate GetElseFunc() {
            return (leftValue, rightValue) => true;
        }
    }

    
}