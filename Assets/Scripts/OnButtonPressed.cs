using UnityEngine;
using TMPro;
using LitJson;
using System.IO;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class OnButtonPressed : MonoBehaviour {
    int runState, cmdPointer;
    public TMP_Text execute;
    public RectTransform progPointer;
    string asmFileName;
    string[] asmContent;
    CPU data;
    /*************History Panel*************/
    public RectTransform histPanel;
    public RectTransform histTemplate;
    RectTransform[] histItem = new RectTransform[1024];
    string[] history = new string[1024];
    int rear;
    /*************Program Panel*************/
    public RectTransform progPanel;
    public RectTransform progTemplate;
    public RectTransform nowInst;
    public Scrollbar barPosition;
    public RectTransform progViewport;
    RectTransform[] progItem = new RectTransform[512];
    string[] program = new string[1024];
    int[] instID = new int[1024];
    int pid;
    // Start is called before the first frame update
    void Start()
    {
        runState = 0;
        cmdPointer = 0;
        rear = 0;
        data = new CPU();
    }
    //[DllImport("api_test")]
    //public static extern void ret_array(long[] aa);
    public void OnButtonOpen() {
        if (runState == 0) {//choose a file
            MyOpenFile ofd = new MyOpenFile();
            if (ofd.OpenDirectory("Y86-64汇编文件|*.yo\0*.yo")) {
                asmFileName = ofd.openFileName.file;
            }
            DLLImport.api_load_prog(asmFileName);
            asmContent = File.ReadAllLines(asmFileName);
            for(int i = 0; i < asmContent.Length; ++i) {
                long position = 0;
                string cmd = "";
                if (!YoFileDecode.linein(asmContent[i], ref position, ref cmd)) continue;
                program[position] = cmd;
            }
            ProgramPanelBuild();
            barPosition.value = 1;
            ChangeState(1);
        }
        else {//continue
            //data = new CPU();
            //bool[] cc = new bool[3];
            //int[] stat = new int[1];
            //long[] pc = new long[1];
            bool ret1 = DLLImport.api_step_exec(1);
            //bool ret = DLLImport.api_get_state(cc,stat,pc,data.reg,data.mem);
            //data.CC.OF = cc[0];
            //data.CC.SF = cc[1];
            //data.CC.ZF = cc[2];
            //data.stat = stat[0];
            //data.PC = pc[0];
            if (ret1) {
                Debug.Log("Execute Successully");
            }
            else {
                Debug.Log("Execute Unsuccessfully");
                return;
            }
            history[rear++] = program[data.PC];
            UpdateHistPanel(true);
            ReadJson();
            Composition.RegisterPanelUpdate(ref data);

            float width = progTemplate.rect.width, height = progTemplate.rect.height;
            nowInst.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, height * instID[data.PC], height);
            nowInst.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
            float destination = progViewport.position.y;
            barPosition.value = 1;
            if(nowInst.position.y < destination) {
                barPosition.value = 0;
                if(nowInst.position.y > destination) {
                    float l = 0.0f, r = 1.0f;
                    while (r - l > 1e-3) {
                        float mid = (l + r) / 2;
                        barPosition.value = mid;
                        if (nowInst.position.y > destination) l = mid;
                        else r = mid;
                    }
                }
            }
        }
    }
    public void OnButtonRevoke() {
        DLLImport.api_revoke(1);
        --rear;
        ReadJson();
        UpdateHistPanel(false);
    }
    public void OnButtonReset() {
        ResetVal();
        ResetProgPanel();
        ResetHistPanel();
    }
    public void OnButtonExecute() {
        Debug.Log(AssemblyInput.MC1 + " " + AssemblyInput.MC2);
        /*
        if(!DLLImport.api_imm_exec(AssemblyInput.MC1, AssemblyInput.MC2)) {
            Debug.Log("Execute Unsuccessfully");
        }
        */
    }
    private void ChangeState(int val) {
        runState = val;
        if (val == 0) {
            execute.text = "Execute";
        }
        else {
            execute.text = "Continue";
        }
    }

    public class CPU {
        public struct Condition_Code {public bool OF, SF, ZF;}
        public Condition_Code CC;
        public int stat;
        public long PC;
        public long[] reg = new long[15];
        public byte[] mem = new byte[1024];
        /* AOK = 1,// Normal operation              HLT = 2,// Halt instruction encountered
           ADR = 3,// Invalid address encountered   INS = 4,// Invalid instruction encountered */
    }
    public void ReadJson() {
        string jsonfile = File.OpenText("crt_state.json").ReadToEnd();
        Debug.Log(jsonfile);
        JsonData json = JsonMapper.ToObject(jsonfile);
        data = new CPU();
        data.CC.OF = (int)json["CC"]["OF"] == 1;
        data.CC.SF = (int)json["CC"]["SF"] == 1;
        data.CC.ZF = (int)json["CC"]["ZF"] == 1;
        data.stat = (int)json["STAT"];
        data.PC = (long)json["PC"];
        for (int i = 0; i <= 14; ++i) {
            data.reg[i] = (long)json["REG"][Composition.reg_name[i]];
        }
        for(int i = 0; i <= 0x8000; i += 8) {
            string x=i.ToString();
            if (json["MEM"].ContainsKey(x)) {
                long val = (long)json["MEM"][x];
                for (int j = 0; j <= 7; ++j) {
                    data.mem[i + j] = (byte)(val & 0xFF);
                    val >>= 8;
                }
            }
        }
    }
    /// <summary>
    /// 重置所有面板，回到启动应用时的内存状态
    /// </summary>
    private void ResetVal() {
        asmFileName = null;
        System.Array.Clear(asmContent, 0, asmContent.Length);
        ChangeState(0);
        rear = 0;
        data = new CPU();
        cmdPointer = 0;
    }

    private void ResetProgPanel() {
        program = new string[0x1000];
        instID = new int[0x1000];
        progPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        pid = 0;
    }
    private void ProgramPanelBuild() {
        progTemplate.gameObject.SetActive(true);
        float width = progTemplate.rect.width, height = progTemplate.rect.height;
        for (int i = 0; i < 1024; ++i) {
            if (program[i] != null) {
                instID[i] = pid;
                progItem[pid] = Instantiate(progTemplate);
                progItem[pid].SetParent(progPanel);
                progItem[pid].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, height * pid, height);
                progItem[pid].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
                progItem[pid].GetComponentInChildren<TMP_Text>().text = program[i];
                ++pid;
            }
        }
        progPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pid);
        progTemplate.gameObject.SetActive(false);
    }
    private void ResetHistPanel() {
        histItem = new RectTransform[1024];
        histPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
    }
    private void UpdateHistPanel(bool Continue) {
        if (Continue) {
            histTemplate.gameObject.SetActive(true);
            int id = rear - 1;
            histTemplate.GetComponentInChildren<TMP_Text>().text = history[id];
            float width = histTemplate.rect.width, height = histTemplate.rect.height;
            histItem[id] = Instantiate(histTemplate);
            histItem[id].SetParent(histPanel);
            histItem[id].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, height * id, height);
            histItem[id].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
            histTemplate.gameObject.SetActive(false);
            histPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * id);
        }
        else {
            Destroy(histItem[rear].gameObject);
        }
    }
}
