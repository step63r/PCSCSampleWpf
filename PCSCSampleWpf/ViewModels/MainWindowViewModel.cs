using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCSC;
using PCSC.Iso7816;

namespace MinatoProject.PCSCSampleWpf.ViewModels
{
    /// <summary>
    /// MainWindow.xamlのViewModelクラス
    /// </summary>
    internal partial class MainWindowViewModel : ObservableObject
    {
        #region プロパティ
        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        [ObservableProperty]
        private string applicationTitle = "PCSC Sample WPF";

        /// <summary>
        /// デバイス一覧
        /// </summary>
        [ObservableProperty]
        private List<string> readers = [];

        /// <summary>
        /// 選択されたデバイス
        /// </summary>
        [ObservableProperty]
        private string selectedReader = string.Empty;

        /// <summary>
        /// Readボタンの文言
        /// </summary>
        [ObservableProperty]
        private string readButtonText = "Read";

        /// <summary>
        /// Readボタンの押下可否
        /// </summary>
        [ObservableProperty]
        private bool readButtonEnabled = true;

        /// <summary>
        /// 処理結果の文字列
        /// </summary>
        [ObservableProperty]
        private string resultText = string.Empty;
        #endregion

        #region メンバ変数
        /// <summary>
        /// IContextFactory
        /// </summary>
        private readonly IContextFactory _contextFactory = ContextFactory.Instance;
        #endregion
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            using (var context = _contextFactory.Establish(SCardScope.System))
            {
                readers = [.. context.GetReaders()];

                if (readers.Count > 0)
                {
                    selectedReader = readers.First();
                }
            }
        }

        #region コマンド
        /// <summary>
        /// カード情報を読み取る
        /// </summary>
        [RelayCommand]
        private void ReadCard()
        {
            ReadButtonEnabled = false;
            ReadButtonText = "Reading...";

            try
            {
                using (var context = _contextFactory.Establish(SCardScope.System))
                {
                    using (var reader = context.ConnectReader(SelectedReader, SCardShareMode.Shared, SCardProtocol.Any))
                    {
                        var apdu = new CommandApdu(IsoCase.Case2Short, reader.Protocol)
                        {
                            CLA = 0xFF,
                            Instruction = InstructionCode.GetData,
                            P1 = 0x00,
                            P2 = 0x00,
                            Le = 0      // We don't know the ID tag size
                        };

                        using (reader.Transaction(SCardReaderDisposition.Leave))
                        {
                            var sendPci = SCardPCI.GetPci(reader.Protocol);
                            var receivePci = new SCardPCI();    // IO returned protocol control information.

                            var receiveBuffer = new byte[256];
                            var command = apdu.ToArray();

                            var bytesReceived = reader.Transmit(
                                sendPci,                // Protocol Control Information (T0, T1 or Raw)
                                command,                // command APDU
                                command.Length, 
                                receivePci,             // returning Protocol Control Information
                                receiveBuffer,
                                receiveBuffer.Length);  // data buffer

                            var responseApdu =
                                new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, reader.Protocol);

                            ResultText = $"SW1: {responseApdu.SW1:X2}" + Environment.NewLine +
                                $"SW2: {responseApdu.SW2:X2}" + Environment.NewLine +
                                $"Uid: {(responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received")}";
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO: logging
                throw;
            }
            finally
            {
                ReadButtonEnabled = true;
                ReadButtonText = "Read";
            }
        }
        #endregion
    }
}
