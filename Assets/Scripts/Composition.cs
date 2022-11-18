using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Composition : MonoBehaviour
{
    //----------------*Register Panel*----------------
    public static string[] reg_name = { "rax", "rcx", "rdx", "rbx", "rsp", "rbp", "rsi", "rdi",
            "r8", "r9", "r10", "r11", "r12", "r13", "r14","rip(PC)","ZF", "SF", "OF", "STAT" };
    public GameObject cpuPanel;
    public RectTransform gprPanelTransform;
    public GridLayoutGroup gprPanelLayout;
    public RectTransform otherRegTransform;
    public GridLayoutGroup otherRegLayout;
    public static TMP_Text[] regtext;
    //-----------------*Memory Panel*-----------------
    const int row = 128;
    public RectTransform memContent;
    //Address Tag
    public RectTransform address;
    RectTransform[] newtag = new RectTransform[row];
    public RectTransform memValTemp;
    //Memory Value
    RectTransform[] memval = new RectTransform[1024];
    public static TMP_Text[] memtext = new TMP_Text[1024];
    
    // Start is called before the first frame update
    void Start() {
        //----------------*Register Panel*----------------
        gprPanelLayout.cellSize = new Vector2(gprPanelTransform.rect.width / 4.0f, gprPanelTransform.rect.height / 4.0f);
        otherRegLayout.cellSize = new Vector2(otherRegTransform.rect.width / 4.0f, otherRegTransform.rect.height);
        regtext = cpuPanel.GetComponentsInChildren<TMP_Text>();
        ResetRegPanel();
        //-----------------*Memory Panel*-----------------
        float wAddr = address.rect.width, hAddr = address.rect.height, pos = 0;
        float wVal = memValTemp.rect.width, hVal = memValTemp.rect.height;
        memContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hAddr * row);
        for(int i = 0; i < row; ++i) {
            newtag[i] = Instantiate(address);
            newtag[i].SetParent(memContent);
            newtag[i].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, pos, hAddr);
            pos += hAddr;
            TMP_Text[] text = newtag[i].GetComponentsInChildren<TMP_Text>();
            text[0].text = "0x"+(i<<2).ToString("X3");
        }
        pos = 0;
        for (int i = 0; i < 1024; ++i) {
            memval[i] = Instantiate(memValTemp);
            memval[i].SetParent(memContent);
            memval[i].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, wAddr + wVal * (i % 8), wVal);
            memval[i].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, pos, hVal);
            if((i&0b111)==0b111)pos += hVal;
            memtext[i] = memval[i].GetComponentsInChildren<TMP_Text>()[0];
            memtext[i].text = 0.ToString("X2");
        }
        Destroy(address.gameObject);
        Destroy(memValTemp.gameObject);
    }
    // Update is called once per frame
    void Update(){}

    private void ResetRegPanel() {
        for (int i = 0; i <= 47; ++i) {
            switch (i % 3) {
                case 0:
                    regtext[i].text = "%" + reg_name[i / 3];
                    break;
                case 1:
                    regtext[i].text = "0x0";
                    break;
                case 2:
                    regtext[i].text = "0";
                    break;
            }
        }
        regtext[48].text = "ZF";
        regtext[50].text = "SF";
        regtext[52].text = "OF";
        regtext[54].text = "STAT";
        regtext[49].text = regtext[51].text = regtext[53].text = regtext[55].text = "0";
    }
    private static void RegTextColor(ref TMP_Text old,string newText) {
        if (newText != old.text) old.color = new Color(1.0f, 0.4f, 0.4f);
        else old.color = new Color(1.0f, 1.0f, 1.0f);
        old.text = newText;
    }
    public static void RegisterPanelUpdate(ref OnButtonPressed.CPU cpu) {
        for (int i = 0; i <= 44; ++i) {
            int reg = i / 3;
            switch (i % 3) {
                case 1:
                    RegTextColor(ref regtext[i], "0x" + cpu.reg[reg].ToString("X"));
                    break;
                case 2:
                    RegTextColor(ref regtext[i], cpu.reg[reg].ToString());
                    break;
            }
        }
        RegTextColor(ref regtext[46], "0x" + cpu.PC.ToString("X3"));
        RegTextColor(ref regtext[47], cpu.PC.ToString());
        RegTextColor(ref regtext[49], cpu.CC.ZF.ToString());
        RegTextColor(ref regtext[51], cpu.CC.SF.ToString());
        RegTextColor(ref regtext[53], cpu.CC.OF.ToString());
        RegTextColor(ref regtext[55], cpu.stat.ToString());
        for (int i = 0; i < 1024; ++i) {
            RegTextColor(ref memtext[i], cpu.mem[i].ToString("X2"));
        }
    }
}
