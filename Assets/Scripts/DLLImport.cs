using System;
using System.Runtime.InteropServices;
using UnityEngine;
public static class DLLImport{
    [DllImport("y86_sim")]
    public static extern bool api_load_prog(string filename);
    //[DllImport("y86_sim181516")]
    //public static extern bool api_get_state(bool[] cc, int[] stat, long[] pc, long[] reg, byte[] mem);
    //[DllImport("y86_sim181516")]
    [DllImport("y86_sim")]
    public static extern bool api_step_exec(uint step);
    [DllImport("y86_sim")]
    public static extern bool api_imm_exec(long part1, long part2);
    [DllImport("y86_sim")]
    public static extern bool api_revoke(int step);

}

public class MyOpenFile {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;

    }
    public class LocalDialog {
        //链接指定系统函数       打开文件对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        public static bool GetOFN([In, Out] OpenFileName ofn) {
            return GetOpenFileName(ofn);//执行打开文件的操作
        }

        //链接指定系统函数        另存为对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
        public static bool GetSFN([In, Out] OpenFileName ofn) {
            return GetSaveFileName(ofn);//执行保存选中文件的操作
        }

    }
    public OpenFileName openFileName;
    public bool OpenDirectory(string type) {
        openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = type;
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "选择文件";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetOpenFileName(openFileName))//点击按钮
        {
            //TODO
            return true;
        }
        else return false;
    }
}
