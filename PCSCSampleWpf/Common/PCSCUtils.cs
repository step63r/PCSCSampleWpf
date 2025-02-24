using PCSC;

namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// PCSCユーティリティクラス
    /// </summary>
    public static class PCSCUtils
    {
        /// <summary>
        /// PCSCステータスフラグを分解してリストにする
        /// </summary>
        /// <param name="state">PCSCステータスフラグ</param>
        /// <returns>リスト</returns>
        public static List<SCRState> SeparatePCSCState(SCRState state)
        {
            var ret = new List<SCRState>();

            if (state.HasFlag(SCRState.Unaware))
            {
                ret.Add(SCRState.Unaware);
            }

            if (state.HasFlag(SCRState.Ignore))
            {
                ret.Add(SCRState.Ignore);
            }

            if (state.HasFlag(SCRState.Changed))
            {
                ret.Add(SCRState.Changed);
            }

            if (state.HasFlag(SCRState.Unknown))
            {
                ret.Add(SCRState.Unknown);
            }

            if (state.HasFlag(SCRState.Unavailable))
            {
                ret.Add(SCRState.Unavailable);
            }

            if (state.HasFlag(SCRState.Empty))
            {
                ret.Add(SCRState.Empty);
            }

            if (state.HasFlag(SCRState.Present))
            {
                ret.Add(SCRState.Present);
            }

            if (state.HasFlag(SCRState.AtrMatch))
            {
                ret.Add(SCRState.AtrMatch);
            }

            if (state.HasFlag(SCRState.Exclusive))
            {
                ret.Add(SCRState.Exclusive);
            }

            if (state.HasFlag(SCRState.InUse))
            {
                ret.Add(SCRState.InUse);
            }

            if (state.HasFlag(SCRState.Mute))
            {
                ret.Add(SCRState.Mute);
            }

            if (state.HasFlag(SCRState.Unpowered))
            {
                ret.Add(SCRState.Unpowered);
            }

            return ret;
        }
    }
}
