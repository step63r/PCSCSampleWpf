namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// GetDataのP1で渡すパラメータ
    /// </summary>
    public enum ApduInsGetData : byte
    {
        /// <summary>
        /// カードUIDの取得
        /// </summary>
        GetUid = 0x00,
        /// <summary>
        /// カードATS-HB/INF/PMmの取得
        /// </summary>
        GetPMm = 0x01,
        /// <summary>
        /// カード識別IDの取得
        /// </summary>
        GetCardID = 0xF0,
        /// <summary>
        /// カード名称の取得
        /// </summary>
        GetCardName = 0xF1,
        /// <summary>
        /// 通信速度の取得
        /// </summary>
        GetCardSpeed = 0xF2,
        /// <summary>
        /// カード種別の取得
        /// </summary>
        GetCardType = 0xF3,
        /// <summary>
        /// カード種別名称の取得
        /// </summary>
        GetCardTypeName = 0xF4,
        /// <summary>
        /// NFC-DEP Target通信状態、ATR_RESの取得
        /// </summary>
        NfcTargetState = 0xF9,
    }
}
