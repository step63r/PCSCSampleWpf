using MinatoProject.PCSCSampleWpf.Common;
using PCSC;
using PCSC.Iso7816;
using PCSC.Monitoring;

namespace MinatoProject.PCSCSampleWpf.Services.Interfaces
{
    /// <summary>
    /// PCSCデバイスサービス インターフェイス
    /// </summary>
    public interface IPCSCDeviceService : IDisposable
    {
        #region イベントハンドラ
        /// <summary>
        /// CardInsertedEvent
        /// </summary>
        event CardInsertedEvent OnCardInserted;
        /// <summary>
        /// CardRemovedEvent
        /// </summary>
        event CardRemovedEvent OnCardRemoved;
        /// <summary>
        /// CardInitializedEvent
        /// </summary>
        event CardInitializedEvent OnCardInitialized;
        /// <summary>
        /// StatusChangeEvent
        /// </summary>
        event StatusChangeEvent OnStatusChanged;
        /// <summary>
        /// MonitorExceptionEvent
        /// </summary>
        event MonitorExceptionEvent OnMonitorExceptionRaised;
        #endregion

        #region プロパティ
        /// <summary>
        /// デバイス一覧
        /// </summary>
        List<string> Readers { get; }
        #endregion

        #region メソッド
        /// <summary>
        /// デバイスのモニタリングを開始する
        /// </summary>
        /// <param name="readerName">デバイス名</param>
        void StartMonitor(string readerName);
        /// <summary>
        /// デバイスのモニタリングを終了する
        /// </summary>
        void CancelMonitor();
        /// <summary>
        /// デバイスのステータスを取得する
        /// </summary>
        /// <param name="readerName">デバイス名</param>
        /// <returns>デバイスステータス</returns>
        ReaderStatus GetStatus(string readerName);
        /// <summary>
        /// カードのデータを取得する
        /// </summary>
        /// <param name="readerName">デバイス名</param>
        /// <param name="P1">パラメータ1</param>
        /// <returns>APDUレスポンスオブジェクト</returns>
        ResponseApdu GetData(string readerName, ApduInsGetData P1);
        /// <summary>
        /// バイナリデータを読み取る
        /// </summary>
        /// <param name="readerName">デバイス名</param>
        /// <param name="msb">最上位ビット</param>
        /// <param name="lsb">最下位ビット</param>
        /// <param name="size">サイズ</param>
        /// <returns>APDUレスポンスオブジェクト</returns>
        ResponseApdu ReadBinary(string readerName, byte msb, byte lsb, int size);
        /// <summary>
        /// バイナリデータを書き込む
        /// </summary>
        /// <param name="readerName">デバイス名</param>
        /// <param name="msb">最上位ビット</param>
        /// <param name="lsb">最下位ビット</param>
        /// <param name="data">サイズ</param>
        /// <returns>APDUレスポンスオブジェクト</returns>
        ResponseApdu UpdateBinary(string readerName, byte msb, byte lsb, byte[] data);
        #endregion
    }
}
