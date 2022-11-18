using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class IDE
{
    TMP_InputField content;
    static string filename = "";
    public enum ErrorType {
        Pass,
        InvalidSpace,InvalidAssembler,InvalidAssembly,
        IncorrectOperands,InvalidNumber,BigNumber,
        InvalidRegister,InvalidImmediate,InvalidDestination,
        SymbolPercent,SymbolDoller,MissingFrontBracket,MissingBackBracket,

        OtherError
    }
    public static string[] info = { "Invalid space","Invalid assembler instruction","Invalid assembly instruction",
        "Incorrect number of operands","Invalid Number","Number too big",
        "Invalid register", "Invalid immediate value", "Invalid destination",
        "Registers must begin with \"%\"","Immediate values must begin with \"$\"","Missing bracket \"(\"","Missing bracket \")\"",
        "Invalid Statement"
    };
    static string[] assembler = { "file", "align", "ascii", "global", "quad" };
    Dictionary<string, long> label;
    public void check() {
        label = new Dictionary<string, long>();
        string[] text = content.text.Split('\n');
        //UNDONE: process /* */ #
    }
    ErrorType parse(string line,ref long addr,out string assembly) {
        line = line.Trim().ToLower();
        assembly = "";
        Regex pat = new Regex(@"[^:]*:");
        if (pat.IsMatch(line)) {
            string newlabel = line.Split(':')[0];
            label.Add(newlabel, addr);
            assembly = "0x" + addr.ToString("x3") + ":";
            line = line.Substring(newlabel.Length + 1).Trim();
        }
        if (line[0] == '.') {
            assembly = "0x" + addr.ToString("x3") + ":";
            string[] parts = line.Split(" ");
            string command = parts[0].Substring(1);
            int i = -1;
            for (i = 0; i < assembler.Length; ++i) {
                if (assembler[i] == command) break;
            }
            if (i == assembler.Length) {
                assembly = "";
                return ErrorType.InvalidAssembler;
            }
            switch (i) {
                case 0:
                    filename = parts[1];
                    assembly = "";
                    return ErrorType.Pass;
                case 1:
                    long len;
                    bool parse = long.TryParse(parts[1], out len);
                    assembly = "";
                    if ((!parse) || (len & (len - 1)) != 0) return ErrorType.InvalidNumber;
                    if (len > 128) return ErrorType.BigNumber;
                    if (addr % len != 0) addr += addr - addr % len;
                    return ErrorType.Pass;
                case 2:
                case 3:
                    return ErrorType.Pass;
                case 4:

                    return ErrorType.Pass;
            }
        }
        else {
            long MC1, MC2;
            byte errOP;
            ErrorType result = AssemblyInput.ParseAsm(line, out MC1, out MC2, out errOP, ref label);
            if (result == ErrorType.Pass) {
                assembly = "0x" + addr.ToString("x3") + ": ";
                switch (MC1 & 0xF) {
                    case 0: case 1: case 9:
                        for(int i = 1; i <= 2; ++i) {
                            assembly += (MC1 & 0xF).ToString("x1");
                            MC1 >>= 4;
                        }
                        addr += 1; break;
                    case 2: case 6: case 10: case 11: case 12:
                        for (int i = 1; i <= 4; ++i) {
                            assembly += (MC1 & 0xF).ToString("x1");
                            MC1 >>= 4;
                        }
                        addr += 2; break;
                    case 7: case 8:
                        for (int i = 1; i <= 2; ++i) {
                            assembly += (MC1 & 0xF).ToString("x1");
                            MC1 >>= 4;
                        }
                        for (int i = 1; i <= 8; ++i) {
                            assembly += (MC2 & 0xFF).ToString("x2");
                            MC1 >>= 8;
                        }
                        addr += 9; break;
                    case 3: case 4: case 5:
                        for (int i = 1; i <= 4; ++i) {
                            assembly += (MC1 & 0xF).ToString("x1");
                            MC1 >>= 4;
                        }
                        for (int i = 1; i <= 8; ++i) {
                            assembly += (MC2 & 0xFF).ToString("x2");
                            MC1 >>= 8;
                        }
                        addr += 10; break;
                }
                return ErrorType.Pass;
            }
            else return result;
        }
        return ErrorType.Pass;
    }

    private string StrReverse(string str) {
        string ans = "";
        for(int i=str.Length-1; i>=0; i--) ans += str[i];
        return ans;
    }
}
