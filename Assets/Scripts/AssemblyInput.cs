using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssemblyInput : MonoBehaviour {
    public TMP_InputField asmInput;
    public TMP_Text inform;
    public static long MC1, MC2;
    // Start is called before the first frame update
    void Start() {
        MC1 = MC2 = -1;
    }

    // Update is called once per frame
    void Update() {

    }
    static string GetOperator(ref string next, ref int index) {
        while (next[index] == ' ' || next[index] == ',') {
            ++index;
        }
        int start = index;
        while (index < next.Length && next[index] != ',' && next[index] != ' ') {
            ++index;
        }
        return next.Substring(start, index - start);
    }

    static IDE.ErrorType BiasParse(ref string text, ref long bias, ref string reg) {
        int frontbracket = -1, backbracket = -1;
        for (int i = 0; i < text.Length && text[i] != ' '; ++i) {
            if (text[i] == '(') frontbracket = i;
            else if (text[i] == ')') backbracket = i;
        }
        if (frontbracket != -1) {
            if (backbracket == -1) return IDE.ErrorType.MissingBackBracket;
            if (frontbracket == 0) {
                bias = 0;
                reg = text.Substring(1, text.Length - 2);
                return IDE.ErrorType.Pass;
            }
            else {
                bool minus = (text[0] == '-');
                int system, start = minus ? 1 : 0;
                if (text[start] == '0') {
                    if (text[start + 1] == 'x') system = 16;
                    else system = 8;
                }
                else system = 10;
                try {
                    if (minus) {
                        bias = Convert.ToInt64(text.Substring(1, frontbracket - 1), system);
                        bias = -bias;
                    }
                    else bias = Convert.ToInt64(text.Substring(0, frontbracket), system);
                }
                catch {
                    return IDE.ErrorType.InvalidNumber;
                }
                reg = text.Substring(frontbracket + 1, backbracket - frontbracket - 1);
                return IDE.ErrorType.Pass;
            }

        }
        else return IDE.ErrorType.MissingFrontBracket;
    }
    static IDE.ErrorType ParseRegister(string text,out int reg) {
        if (text[0] != '%') {
            reg = -1;
            return IDE.ErrorType.SymbolPercent;
        }
        for (int i = 0; i < register.Length; ++i) if (text == register[i]) {
            reg = i;
            return IDE.ErrorType.Pass;
        }
        reg = -1;
        return IDE.ErrorType.InvalidRegister;
    }
    static string[] instruction = {"halt","nop","rrmovq","cmovle","cmovl","cmove","cmovne",
            "cmovge","cmovg","irmovq","rmmovq","mrmovq","addq","subq","andq","xorq",
            "jmp","jle","jl","je","jne","jge","jg","call","ret","pushq","popq","inc","dec"};
    public static string[] register = {"%rax","%rcx","%rdx","%rbx","%rsp","%rbp","%rsi","%rdi",
            "%r8","%r9","%r10","%r11","%r12","%r13","%r14" };
    public void CheckAsm(int caller = 0) {
        Dictionary<string, long> empty = new Dictionary<string, long>();
        byte errOP;
        IDE.ErrorType ret = ParseAsm(asmInput.text, out MC1, out MC2, out errOP, ref empty);
        if (ret == IDE.ErrorType.Pass) {
            string output = "Machine Code:";
            byte type = (byte)(MC1 & 0xF), subtype = (byte)((MC1 & 0xF0) >> 4), rA = (byte)((MC1 & 0xF00) >> 8), rB = (byte)((MC1 & 0xF000) >> 12);
            output += type.ToString("X") + subtype.ToString("X") + " ";
            if ((2 <= type && type <= 6) || 10 <= type) output += rA.ToString("X") + rB.ToString("X") + " ";
            if ((3 <= type && type <= 5) || (7 <= type && type <= 8)) {
                long query = MC2;
                for (int i = 1; i <= 8; ++i) {
                    output += (query & 0xFF).ToString("X2") + " ";
                    query >>= 8;
                }
            }
            inform.text = output;
            inform.color = Color.white;
        }
        else {
            inform.text = "Wrong Assembly! "+IDE.info[(int)ret];
            inform.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
    }
    public static IDE.ErrorType ParseAsm(string text, out long M1, out long M2, out byte errOP, ref Dictionary<string,long> label) {
        string lowText = text.Trim().ToLower();
        int type = -1, subtype = 0, rA = -1, rB = -1;
        long D = -1;
        string op1 = "", op2 = "";
        for (int i = 0; i < instruction.Length; ++i) {
            if (lowText.Length < instruction[i].Length) continue;
            if (lowText.Substring(0, instruction[i].Length) == instruction[i]) {
                type = i;
                break;
            }
        }
        if (type != -1) {
            int index = instruction[type].Length;
            switch (type) {
                case 0:
                case 1:
                case 24:
                    errOP = 0;
                    if ((type == 0 && lowText[4] == ' ') || (type != 0 && lowText[3] == ' ')) {
                        M1 = M2 = -1;
                        return IDE.ErrorType.IncorrectOperands;
                    }
                    if (lowText.Length != instruction[type].Length) {
                        M1 = M2 = -1;
                        return IDE.ErrorType.InvalidAssembly;
                    }

                    if (type == 24) type = 9;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 12:
                case 13:
                case 14:
                case 15:
                    if (lowText[index++] != ' ') {
                        M1 = M2 = -1; errOP = 0;
                        return IDE.ErrorType.InvalidAssembly;
                    }
                    op1 = GetOperator(ref lowText, ref index);
                    op2 = GetOperator(ref lowText, ref index);
                    IDE.ErrorType p1 = ParseRegister(op1, out rA), p2 = ParseRegister(op2, out rB);
                    if (p1 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 1;
                        return p1;
                    }else if(p2 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 2;
                        return p2;
                    }

                    if (type <= 8) {
                        subtype = type - 2;
                        type = 2;
                    }
                    else {
                        subtype = type - 12;
                        type = 6;
                    }
                    break;
                case 9:
                    if (lowText[index++] != ' ') {
                        M1 = M2 = -1; errOP = 0;
                        return IDE.ErrorType.InvalidAssembly;
                    }
                    if (lowText[index++] != '$') {
                        M1 = M2 = -1; errOP = 1;
                        return IDE.ErrorType.SymbolDoller;
                    }
                    op1 = GetOperator(ref lowText, ref index);
                    try {
                        if (op1[0] == '0') {
                            if (op1[1] == 'x') D = Convert.ToInt64(op1, 16);
                            else D = Convert.ToInt64(op1, 8);
                        }
                        else D = Convert.ToInt64(op1, 10);
                    }
                    catch {
                        M1 = M2 = -1; errOP = 1;
                        return IDE.ErrorType.InvalidNumber;
                    }
                    op2 = GetOperator(ref lowText, ref index);
                    p2 = ParseRegister(op2, out rB);
                    if (p2 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 2;
                        return p2;
                    }

                    type = 3;
                    rA = 15;
                    break;
                case 10:
                    if (lowText[index++] != ' ') {
                        M1 = M2 = -1; errOP = 0;
                        return IDE.ErrorType.InvalidAssembly;
                    }
                    op1 = GetOperator(ref lowText, ref index);
                    p1 = ParseRegister(op1, out rA);
                    if (p1 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 1;
                        return p1;
                    }
                    op2 = GetOperator(ref lowText, ref index);
                    string regstr = "";
                    IDE.ErrorType p3 = BiasParse(ref op2, ref D, ref regstr);
                    if (p3 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 2;
                        return p3;
                    }
                    p2 = ParseRegister(regstr, out rB);
                    if (p2 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 2;
                        return p2;
                    }

                    type = 4;
                    break;
                case 11:
                    if (lowText[index++] != ' ') {
                        M1 = M2 = -1; errOP = 0;
                        return IDE.ErrorType.InvalidAssembly;
                    }
                    op1 = GetOperator(ref lowText, ref index);
                    regstr = "";
                    p3 = BiasParse(ref op1, ref D, ref regstr);
                    if (p3 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 1;
                        return p3;
                    }
                    p1 = ParseRegister(regstr, out rB);
                    if (p1 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 1;
                        return p1;
                    }

                    op2 = GetOperator(ref lowText, ref index);
                    p2 = ParseRegister(op2, out rA);
                    if (p2 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 2;
                        return p2;
                    }
                    type = 5;
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                    op1 = GetOperator(ref lowText, ref index);
                    if (label.ContainsKey(op1)) D = label[op1];
                    else {
                        try {
                            if (op1[0] == '0') {
                                if (op1[1] == 'x') D = Convert.ToInt64(op1, 16);
                                else D = Convert.ToInt64(op1, 8);
                            }
                            else D = Convert.ToInt64(op1, 10);
                        }
                        catch {
                            M1 = M2 = -1; errOP = 1;
                            return IDE.ErrorType.InvalidDestination;
                        }
                    }
                    
                    if (type <= 22) {
                        subtype = type - 16;
                        type = 7;
                    }
                    else type = 8;
                    break;
                case 25:
                case 26:
                case 27:
                case 28:
                    op1 = GetOperator(ref lowText, ref index);
                    p1 = ParseRegister(op1, out rA);
                    if (p1 != IDE.ErrorType.Pass) {
                        M1 = M2 = -1; errOP = 1;
                        return p1;
                    }
                    if (type <= 26) type -= 15;
                    else type = 12;
                    rB = 15;
                    break;
            }
        }
        if (type == -1) {
            M1 = M2 = -1; errOP = 0;
            return IDE.ErrorType.OtherError;
        }
        else {
            M1 = type + (subtype << 4) + (rA << 8) + (rB << 12);
            M2 = D;
            errOP = 0;
            return IDE.ErrorType.Pass;
        }
    }
}