﻿using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion
    }
}
