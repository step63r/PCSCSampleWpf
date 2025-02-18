using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;

namespace MinatoProject.PCSCSampleWpf.ViewModels
{
    /// <summary>
    /// ReaderPage.xamlのViewModelクラス
    /// </summary>
    public partial class ReaderPageViewModel : ViewModel
    {
        #region プロパティ
        /// <summary>
        /// ページタイトル
        /// </summary>
        [ObservableProperty]
        private string pageTitle = "Reader";

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
        /// ロガー
        /// </summary>
        private readonly ILogger<MainWindowViewModel> _logger;
        /// <summary>
        /// デバイスサービス
        /// </summary>
        private readonly IPCSCDeviceService _deviceService;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="deviceService">デバイスサービス</param>
        public ReaderPageViewModel(ILogger<MainWindowViewModel> logger, IPCSCDeviceService deviceService)
        {
            _logger = logger;
            _deviceService = deviceService;

            Readers = _deviceService.Readers;
            if (Readers.Count > 0)
            {
                SelectedReader = Readers.First();
            }
        }
        #endregion

        #region コマンド
        [RelayCommand]
        private void ReadCard()
        {
            _logger.LogInformation("start");

            ReadButtonEnabled = false;
            ReadButtonText = "Reading...";

            try
            {
                //using (var context = _contextFactory.Establish(SCardScope.System))
                //{
                //    using (var reader = context.ConnectReader(SelectedReader, SCardShareMode.Shared, SCardProtocol.Any))
                //    {
                //        var apdu = new CommandApdu(IsoCase.Case2Short, reader.Protocol)
                //        {
                //            CLA = 0xFF,
                //            Instruction = InstructionCode.GetData,
                //            P1 = 0x00,
                //            P2 = 0x00,
                //            Le = 0      // We don't know the ID tag size
                //        };

                //        using (reader.Transaction(SCardReaderDisposition.Leave))
                //        {
                //            var sendPci = SCardPCI.GetPci(reader.Protocol);
                //            var receivePci = new SCardPCI();    // IO returned protocol control information.

                //            var receiveBuffer = new byte[256];
                //            var command = apdu.ToArray();

                //            var bytesReceived = reader.Transmit(
                //                sendPci,                // Protocol Control Information (T0, T1 or Raw)
                //                command,                // command APDU
                //                command.Length, 
                //                receivePci,             // returning Protocol Control Information
                //                receiveBuffer,
                //                receiveBuffer.Length);  // data buffer

                //            var responseApdu =
                //                new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, reader.Protocol);

                //            ResultText = $"SW1: {responseApdu.SW1:X2}" + Environment.NewLine +
                //                $"SW2: {responseApdu.SW2:X2}" + Environment.NewLine +
                //                $"Uid: {(responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received")}";
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                ReadButtonEnabled = true;
                ReadButtonText = "Read";
            }

            _logger.LogInformation("end");
        }
        #endregion
    }
}
