using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;

namespace MinatoProject.PCSCSampleWpf.ViewModels
{
    /// <summary>
    /// WriterPage.xamlのViewModelクラス
    /// </summary>
    public partial class WriterPageViewModel : ViewModel
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
        /// MSB
        /// </summary>
        [ObservableProperty]
        private int mostSignificantBit = 0x00;

        /// <summary>
        /// LSB
        /// </summary>
        [ObservableProperty]
        private int leastSignificantBit = 0x00;

        /// <summary>
        /// 書き込む対象のバイナリデータ（文字列）
        /// </summary>
        [ObservableProperty]
        private string inputData = string.Empty;

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
        public WriterPageViewModel(ILogger<MainWindowViewModel> logger, IPCSCDeviceService deviceService)
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
        /// <summary>
        /// バイナリデータを書き込む
        /// </summary>
        [RelayCommand]
        private void UpdateBinary()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                byte[] data = CreateBinaryData(InputData);

                // MIFAREではP1は0x00固定、Leは0x10固定
                var response = _deviceService.UpdateBinary(SelectedReader, Convert.ToByte(MostSignificantBit), Convert.ToByte(LeastSignificantBit), data);
                ResultText = $"[SW1={response.SW1:X2}, SW2=0x{response.SW2:X2}]";
            }
            catch (Exception ex)
            {
                ResultText = $"[ERROR]\n{ex.Message}\n-----\n{ex.StackTrace}";
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("end");
        }
        #endregion

        #region メンバメソッド
        /// <summary>
        /// 書き込む対象のバイナリデータを生成する
        /// </summary>
        /// <param name="input">文字列</param>
        /// <param name="split">分割用のキャラクター</param>
        /// <returns>バイト配列</returns>
        private static byte[] CreateBinaryData(string input, char split = '-')
        {
            List<byte> ret = [];

            foreach (string s in input.Split(split))
            {
                int n = int.Parse(s);
                ret.Add(Convert.ToByte(n));
            }

            return [.. ret];
        }
        #endregion
    }
}
