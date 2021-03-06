﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Scorpio.Compiler;
namespace Scorpio.Serialize {
    public class ScorpioMaker {
        private static byte LineFlag = byte.MaxValue;
        public static byte[] Serialize(String breviary, string data) {
            var tokens = new ScriptLexer(data, breviary).GetTokens();
            if (tokens.Count == 0) return new byte[0];
            int sourceLine = 0;
            byte[] ret = null;
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream)) {
                    writer.Write((byte)0);          //第一个字符写入一个null 以此判断文件是二进制文件还是字符串文件
                    writer.Write(tokens.Count);
                    for (int i = 0; i < tokens.Count; ++i) {
                        var token = tokens[i];
                        if (sourceLine != token.SourceLine) {
                            sourceLine = token.SourceLine;
                            writer.Write(LineFlag);
                            writer.Write(token.SourceLine);
                        }
                        writer.Write((byte)token.Type);
                        switch (token.Type) {
                        case TokenType.Boolean:
                            writer.Write((bool)token.Lexeme ? (byte)1 : (byte)0);
                            break;
                        case TokenType.String:
                        case TokenType.SimpleString:
                            Util.WriteString(writer, (string)token.Lexeme);
                            break;
                        case TokenType.Identifier:
                            Util.WriteString(writer, (string)token.Lexeme);
                            break;
                        case TokenType.Number:
                            if (token.Lexeme is double) {
                                writer.Write((byte)1);
                                writer.Write((double)token.Lexeme);
                            } else {
                                writer.Write((byte)2);
                                writer.Write((long)token.Lexeme);
                            }
                            break;
                        }
                    }
                    ret = stream.ToArray();
                }
            }
            return ret;
        }
        public static List<Token> Deserialize(byte[] data) {
            List<Token> tokens = new List<Token>();
            using (MemoryStream stream = new MemoryStream(data)) {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    reader.ReadByte();      //取出第一个null字符
                    int count = reader.ReadInt32();
                    int sourceLine = 0;
                    for (int i = 0; i < count; ++i) {
                        var flag = reader.ReadByte();
                        if (flag == LineFlag) {
                            sourceLine = reader.ReadInt32();
                            flag = reader.ReadByte();
                        }
                        var type = (TokenType)flag;
                        object value = null;
                        switch (type) {
                        case TokenType.Boolean:
                            value = (reader.ReadByte() == 1);
                            break;
                        case TokenType.String:
                        case TokenType.SimpleString:
                            value = Util.ReadString(reader);
                            break;
                        case TokenType.Identifier:
                            value = Util.ReadString(reader);
                            break;
                        case TokenType.Number:
                            if (reader.ReadByte() == 1)
                                value = reader.ReadDouble();
                            else
                                value = reader.ReadInt64();
                            break;
                        default:
                            value = type.ToString();
                            break;
                        }
                        tokens.Add(new Token(type, value, sourceLine - 1, 0));
                    }
                }
            }
            return tokens;
        }
        public static string DeserializeToString(byte[] data) {
            var builder = new StringBuilder();
            using (var stream = new MemoryStream(data)) {
                using (var reader = new BinaryReader(stream)) {
                    reader.ReadByte();      //取出第一个null字符
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; ++i) {
                        var flag = reader.ReadByte();
                        if (flag == LineFlag) {
                            var line = reader.ReadInt32();
                            flag = reader.ReadByte();
                            var sourceLine = builder.ToString().Split('\n').Length;
                            for (var j = sourceLine; j < line; ++j)
                                builder.Append('\n');
                        }
                        TokenType type = (TokenType)flag;
                        object value = null;
                        switch (type) {
                        case TokenType.Boolean:
                            value = (reader.ReadByte() == 1) ? "true" : "false";
                            break;
                        case TokenType.String:
                            value = "\"" + Util.ReadString(reader).Replace("\n", "\\n") + "\"";
                            break;
                        case TokenType.SimpleString:
                            value = "@\"" + Util.ReadString(reader) + "\"";
                            break;
                        case TokenType.Identifier:
                            value = Util.ReadString(reader);
                            break;
                        case TokenType.Number:
                            if (reader.ReadByte() == 1)
                                value = reader.ReadDouble();
                            else
                                value = reader.ReadInt64() + "L";
                            break;
                        default:
                            value = GetTokenString(type);
                            break;
                        }
                        builder.Append(value + " ");
                    }
                }
            }
            return builder.ToString();
        }
        private static string GetTokenString(TokenType type) {
            switch (type) {
            case TokenType.LeftBrace: return "{";
            case TokenType.RightBrace: return "}";
            case TokenType.LeftBracket: return "[";
            case TokenType.RightBracket: return "]";
            case TokenType.LeftPar: return "(";
            case TokenType.RightPar: return ")";

            case TokenType.Period: return ".";
            case TokenType.Comma: return ",";
            case TokenType.Colon: return ":";
            case TokenType.SemiColon: return ";";
            case TokenType.QuestionMark: return "?";
            case TokenType.Sharp: return "#";

            case TokenType.Plus: return "+";
            case TokenType.Increment: return "++";
            case TokenType.AssignPlus: return "+=";
            case TokenType.Minus: return "-";
            case TokenType.Decrement: return "--";
            case TokenType.AssignMinus: return "-=";
            case TokenType.Multiply: return "*";
            case TokenType.AssignMultiply: return "*=";
            case TokenType.Divide: return "/";
            case TokenType.AssignDivide: return "/=";
            case TokenType.Modulo: return "%";
            case TokenType.AssignModulo: return "%=";
            case TokenType.InclusiveOr: return "|";
            case TokenType.AssignInclusiveOr: return "|=";
            case TokenType.Or: return "||";
            case TokenType.Combine: return "&";
            case TokenType.AssignCombine: return "&=";
            case TokenType.And: return "&&";
            case TokenType.XOR: return "^";
            case TokenType.Negative: return "~";
            case TokenType.AssignXOR: return "^=";
            case TokenType.Shi: return "<<";
            case TokenType.AssignShi: return "<<=";
            case TokenType.Shr: return ">>";
            case TokenType.AssignShr: return ">>=";
            case TokenType.Not: return "!";
            case TokenType.Assign: return "=";
            case TokenType.Equal: return "==";
            case TokenType.NotEqual: return "!=";
            case TokenType.Greater: return ">";
            case TokenType.GreaterOrEqual: return ">=";
            case TokenType.Less: return "<";
            case TokenType.LessOrEqual: return "<=";

            case TokenType.Eval: return "eval";
            case TokenType.Var: return "var";
            case TokenType.Function: return "function";
            case TokenType.If: return "if";
            case TokenType.ElseIf: return "elif";
            case TokenType.Else: return "else";
            case TokenType.While: return "while";
            case TokenType.For: return "for";
            case TokenType.Foreach: return "foreach";
            case TokenType.In: return "in";
            case TokenType.Switch: return "switch";
            case TokenType.Case: return "case";
            case TokenType.Default: return "default";
            case TokenType.Try: return "try";
            case TokenType.Catch: return "catch";
            case TokenType.Throw: return "throw";
            case TokenType.Finally: return "finally";
            case TokenType.Continue: return "continue";
            case TokenType.Break: return "break";
            case TokenType.Return: return "return";
            case TokenType.Define: return "define";
            case TokenType.Ifndef: return "ifndef";
            case TokenType.Endif: return "endif";
            case TokenType.Null: return "null";
            case TokenType.Params: return "...";
            default: return "";
            }
        }
    }
}
