using System.Text.RegularExpressions;
using Unity.VisualScripting;
using System;

static public class YoFileDecode
{
    static string[] opName = {"halt","nop","","irmovq","rmmovq","mrmovq","",
            "","call","ret","pushq","popq"}; 
    static public bool linein(string str, ref long pos, ref string ret) {
        Regex pat = new Regex(@"0x([0-9|a-f]{3}): ([0-9|a-f]*)");
        string[] result = pat.Split(str);
        if (result.Length == 1 || result[2] == " " || result[2].Length == 16) return false;
        pos = Convert.ToInt64(result[1], 16);
        byte[] data = new byte[10];
        for(int i = 0; i < result[2].Length; i+=2) {
            data[i >> 1] = (byte)(Convert.ToInt64(result[2].Substring(i, 2), 16));
        }
        ret = "0x" + pos.ToString("X3") + ": " + MachineCodeDecode(data);
        return true;
    }
    static public string MachineCodeDecode(byte[] data, bool HEX=true) {
        string result;
        long V = 0;
        switch ((data[0] & 0xF0)>>4) {
            case 0x0:
            case 0x1:
            case 0x9:
                return opName[(data[0] & 0xF0) >> 4];
            case 0x2:
                switch (data[0] & 0xF) {
                    case 0x0:
                        result = "rrmovq ";break;
                    case 0x1:
                        result = "cmovle ";break;
                    case 0x2:
                        result = "cmovl ";break;
                    case 0x3:
                        result = "cmove ";break;
                    case 0x4:
                        result = "cmovne ";break;
                    case 0x5:
                        result = "cmovge ";break;
                    case 0x6:
                        result = "cmovg ";break;
                    default: result = null;break;
                }
                result += AssemblyInput.register[(data[1] & 0xF0) >> 4] + ", " + AssemblyInput.register[data[1] & 0xF];
                return result;
            case 0x3:
                result = "irmovq $";
                if (HEX) result += "0x";
                for (int i = 2; i <= 9; ++i) V += data[i] << ((i - 2) << 8);
                return result + V.ToString(HEX ? "X" : "") + ", " + AssemblyInput.register[data[1] & 0xF];
            case 0x4:
                result = "rmmovq " + AssemblyInput.register[(data[1] & 0xF0) >> 4] + ", ";
                for (int i = 2; i <= 9; ++i) V += data[i] << ((i - 2) << 8);
                if (V != 0) result += V.ToString(HEX ? "X" : "");
                return result + "(" + AssemblyInput.register[data[1] & 0xF] + ")";
            case 0x5:
                result = "mrmovq ";
                for (int i = 2; i <= 9; ++i) V += data[i] << ((i - 2) << 8);
                if (V != 0) result += V.ToString(HEX ? "X" : "");
                result += "(" + AssemblyInput.register[data[1] & 0xF] + ")" + ", ";
                return result + AssemblyInput.register[(data[1] & 0xF0) >> 4];
            case 0x6:
                switch (data[0] & 0xF) {
                    case 0x0:
                        result = "addq ";break;
                    case 0x1:
                        result = "subq ";break;
                    case 0x2:
                        result = "andq ";break;
                    case 0x3:
                        result = "xorq ";break;
                    default: result = null; break;
                }
                return result + AssemblyInput.register[(data[1] & 0xF0) >> 4] + ", " + AssemblyInput.register[data[1] & 0xF];
            case 0x7:
                switch (data[0] & 0xF) {
                    case 0x0:
                        result = "jmp ";break;
                    case 0x1:
                        result = "jle ";break;
                    case 0x2:
                        result = "jl ";break;
                    case 0x3:
                        result = "je ";break;
                    case 0x4:
                        result = "jne ";break;
                    case 0x5:
                        result = "jge ";break;
                    case 0x6:
                        result = "jg ";break;
                    default: result = null; break;
                }
                for (int i = 1; i <= 8; ++i) V += data[i] << ((i - 2) << 8);
                return result + "0x" + V.ToString("X3");
            case 0x8:
                for (int i = 1; i <= 8; ++i) V += data[i] << ((i - 2) << 8);
                return "call " + "0x" + V.ToString("X3");
            case 0xA:
            case 0xB:
                return opName[(data[0] & 0xF0) >> 4] + " " + AssemblyInput.register[(data[1] & 0xF0) >> 4];
            default:
                return "Wrong Code!";
        }
    }
}
