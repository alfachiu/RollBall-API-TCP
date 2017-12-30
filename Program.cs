using System;
using System.Threading;
using Hprose.Client;

/// <summary>
/// 滾球遊戲 TCP API
/// Version Date: 2017/12/30 下午 3:32:50
/// Authored: alfa@52farfar.com
/// 函式支援各種程式語言 Hprose 官方參考： http://hprose.com/
/// 請求服務的間隔時間為： 500~1000ms，最佳建議值為700ms。
/// 參數值及傳回值全部為字串格式：若值不是單一時為字串陣列。
/// 註解有星號"*"必須修改才能正確執行。
/// 撰寫程式前請確認代碼模式：
/// 1.腳本被動模式：用戶自行撰寫流程代碼。
/// 2.腳本自動模式：用戶不需要撰寫流程代碼。
/// </summary>
namespace RollBall_API_TCP
{
    /// <summary>
    /// (0)哈囉世界
    /// Hello(你的名字)
    /// </summary>
    /// <return>字串</return>
    public interface IHello
    {
        string Hello(string name);
    }

    /// <summary>
    /// (1)*驗證用戶及註冊
    /// 1.你必須以瀏覽器或是程式內至少執行一次此服務項目執行註冊。
    /// 2.服務器會在第一次服務時記錄資料。
    /// 3.請連絡服務器管理員啟用你的IP。
    /// Verify(["你的外網地址", "你的電郵地址", "你的名字", "你的公司"[, "備註",...]])
    /// </summary>
    /// <return>布林字串</return>
    public interface IVerify
    {
        string Verify(string[] args);
    }

    /// <summary>
    /// (2)讀取功能啟用的狀態
    /// 若發現未啟用時請連絡服務器管理員。
    /// ReadStateOfEnable("")
    /// </summary>
    /// <return>
    /// 布林字串[4]
    /// 1.[0]=裝置(球台)是否啟用"。
    /// 2.[1]=裝置(球台)是否錯誤，當裝置啟用但是發生錯誤時為"True"。
    /// 3.[2]=服務器是否啟用，若服務器啟用時為"True"，未啟用時為"Error"。
    /// 4.[3]=腳本被動模式
    ///     "True"->腳本被動模式：用戶需要自行撰寫代碼操作裝置動作。參考：(8)寫入命令
    ///     "False"->腳本自動模式：服務器自動執行腳本。
    /// </return>
    public interface IReadStateOfEnable
    {
        string[] ReadStateOfEnable(string arg = "");
    }

    /// <summary>
    /// (3)讀取腳本間隔時間秒鐘
    /// 腳本被動模式：用戶自行撰寫代碼，不需要執行此函式。注意："3.逾時"
    /// 腳本自動模式：用戶可以讀取參考用。
    /// 流程及名詞：
    ///      1.備妥：當球在球盤邊緣上方備妥至"丟球命令"的間隔時間。
    ///      2.滾球：當"丟球命令"後要等待多久才開始偵測球滾動狀態。
    ///      3.逾時：當球不滾動後未停格於球盤正確位置(錯誤)的逾時時間。
    ///              此時間由伺服器自動處理，預設30秒鐘。
    ///              當逾時會自動給錯誤答案。參考：(5)讀取操作流程->5.錯誤
    ///              當逾時錯誤發生時的撿球指令是使用"強制撿球"。
    ///      4.答案：當正確停格有了正確答案後要等待多久才執行撿球命令。
    /// ReadScriptRemain("")
    /// </summary>
    /// <return>
    /// 整數字串[4]
    /// 1.[0]=備妥
    /// 2.[1]=滾球
    /// 3.[2]=逾時
    /// 4.[3]=答案
    /// </return>
    public interface IReadScriptRemain
    {
        string[] ReadScriptRemain(string arg = "");
    }

    /// <summary>
    /// (4)讀取查詢最後動作
    /// ReadQueryLastOperate("")
    /// </summary>
    /// <return>
    /// 字串[2]
    /// 1.[0]="100"(備妥)、"101"(滾球)、"102"(答案)，請不要做為程式內流程判斷用。參考：(5)讀取操作流程
    /// 2.[1]="True"|"False"，是否忙碌中。
    ///     腳本被動模式：
    ///         若忙碌中執行"(8)寫入命令"將會無效果。參考：(8)寫入命令
    ///         你可以用來判斷是否執行命令成功，比如下達丟球命令後判斷是否為忙碌，若是表示已成功。
    /// </return>
    public interface IReadQueryLastOperate
    {
        string[] ReadQueryLastOperate(string arg = "");
    }

    /// <summary>
    /// (5)讀取操作流程
    /// 流程及名詞：
    ///      1.備妥：當為"True"時，你可以開始停止下注倒數計時，然後執行"丟球命令"。
    ///      2.滾球：當為"True"時，球有正常滾動，此值在執行"丟球命令"後可能需花費數秒鐘才會出現。
    ///      4.答案：當為"True"時，已經有答案，如果球未正確停格(錯誤)也是會有答案。
    ///      4.完成：當為"True"時，先判斷"5.錯誤"後才決定是否要執行"(6)讀取答案取得"，然後執行"撿球命令"。
    ///      5.錯誤：當為"True"時，如果球未正確停格時錯誤為"True"，此時的答案請用戶自行重新定義。
    ///      腳本被動模式：注意：發生錯誤後的撿球指令是使用"強制撿球"。
    /// 流程分界點：
    ///      流程會有兩個主要分界點。
    ///      1.備妥=True、滾球=True、答案=False、完成=False、錯誤=False
    ///      2.備妥=False、滾球=False、答案=True、完成=True、錯誤=True|False
    /// ReadFlowOfOperate("")
    /// </summary>
    /// <return>
    // return: 布林字串[5]
    /// 1.[0]=備妥
    /// 2.[0]=滾球
    /// 3.[0]=答案
    /// 4.[0]=完成
    /// 5.[0]=錯誤
    /// </return>
    public interface IReadFlowOfOperate
    {
        string[] ReadFlowOfOperate(string arg = "");
    }

    /// <summary>
    /// (6)讀取答案取得
    /// ReadGetAnswer("")
    /// </summary>
    /// <return>
    /// 字串[6]
    /// 1.[0]=答案號碼，"1"~"12"
    /// 2.[1]=答案X軸，"0"~"11"
    /// 3.[2]=答案Y軸，"0"~"11"
    /// 4.[3]=答案時間，格式為"yyyy/MM/dd HH:mm:ss zzz"，範例："2017/12/29 02:48:02 +08:00"
    /// 5.[4]=球最後的座標X，浮點數，此座標是做為強制撿球用，你不需要使用。
    /// 6.[5]=球最後的座標Y，浮點數，此座標是做為強制撿球用，你不需要使用。
    /// </return>
    public interface IReadGetAnswer
    {
        string[] ReadGetAnswer(string arg = "");
    }

    /// <summary>
    /// (7)寫入腳本間隔時間秒鐘
    /// 此為保留函式，你不需要執行此函式。參考：(3)讀取腳本間隔時間秒鐘
    /// WriteScriptRemain([備妥, 滾球, 逾時, 答案])
    /// </summary>
    /// <return>布林字串</return>
    public interface IWriteScriptRemain
    {
        string WriteScriptRemain(string[] args);
    }

    /// <summary>
    /// (8)寫入命令
    /// public enum SendType
    /// {
    ///    None = 0,           // 無，不需要用，若送出命令會清除服務器緩存的命令。
    ///    Drop = 1,           // 丟球
    ///    Pickup = 2,         // 撿球
    ///    Query = 3,          // 查詢，不需要用，服務器會定時自動查詢。
    ///    PickupForce = 9,    // 強制撿球
    /// }
    //
    /// 腳本被動模式：自行撰寫代碼，先"(5)讀取操作流程"，按照流程下指令及"(6)讀取答案取得"。
    ///      只用到三個命令：
    ///          丟球：當"備妥"後執行丟球命令。
    ///          撿球：當"答案取得"後執行撿球。
    ///          強制撿球：當"答案取得"發生"錯誤"後執行強制撿球。注意：發生錯誤時，你應該自行定義答案內容。
    /// 腳本自動模式：不需要執行此任何命令。
    ///
    /// WriteCommand("1")    // 丟球
    /// WriteCommand("2")    // 撿球
    /// WriteCommand("9")    // 強制撿球
    /// </summary>
    /// <return>布林字串</return>
    public interface IWriteCommand
    {
        string WriteCommand(string arg);
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            string server_ip = "192.168.1.100";     // *服務器地址，請連絡服務器管理員取得正確值。
            string server_port = "5288";            // *服務器端口，請連絡服務器管理員取得正確值。
            HproseClient _tcp = HproseClient.Create($"tcp://{server_ip}:{server_port}");
            // 函式登錄
            IHello hello = _tcp.UseService<IHello>();
            IVerify verify = _tcp.UseService<IVerify>();
            IReadStateOfEnable readStateOfEnable = _tcp.UseService<IReadStateOfEnable>();
            IReadScriptRemain readScriptRemain = _tcp.UseService<IReadScriptRemain>();
            IReadQueryLastOperate readQueryLastOperate = _tcp.UseService<IReadQueryLastOperate>();
            IReadFlowOfOperate readFlowOfOperate = _tcp.UseService<IReadFlowOfOperate>();
            IReadGetAnswer readGetAnswer = _tcp.UseService<IReadGetAnswer>();
            IWriteCommand writeCommand = _tcp.UseService<IWriteCommand>();
            // 函式測試
            string empty = "";
            Console.WriteLine("[TCP Client]");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{i.ToString()}.");
                Console.WriteLine($"Hello: { hello.Hello("World")} {i.ToString()}");
                Console.WriteLine($"Verify: {verify.Verify(new string[] { "114.35.45.13", "admin@52farfar.com", "阿管先生", "發財公司", "我在珠海市" })}");
                Console.WriteLine($"ReadStateOfEnable: {string.Join(", ", readStateOfEnable.ReadStateOfEnable(empty))}");
                Console.WriteLine($"ReadScriptRemain: {string.Join(", ", readScriptRemain.ReadScriptRemain(empty))}");
                Console.WriteLine($"ReadQueryLastOperate: {string.Join(", ", readQueryLastOperate.ReadQueryLastOperate(empty))}");
                Console.WriteLine($"ReadFlowOfOperate: {string.Join(", ", readFlowOfOperate.ReadFlowOfOperate(empty))}");
                Console.WriteLine($"ReadGetAnswer: {string.Join(", ", readGetAnswer.ReadGetAnswer(empty))}");
                Console.WriteLine($"WriteCommand: {writeCommand.WriteCommand("0")}");
                Thread.Sleep(700);  // 程式實做時你可以使用 Timer，最佳建議值為700ms。
            }
            Console.WriteLine("Ending, Press any key to exit!");
            Console.ReadKey();
        }
    }
}