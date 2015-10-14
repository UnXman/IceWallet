namespace IceWallet.Implementations.Blockchains.LevelDB
{
    internal enum DataEntryPrefix : byte
    {
        /// <summary>
        /// 区块头表
        /// </summary>
        DATA_HeaderList = 0x00,

        /// <summary>
        /// 区块
        /// </summary>
        DATA_Block = 0x01,

        /// <summary>
        /// 交易
        /// </summary>
        DATA_Transaction = 0x02,

        /// <summary>
        /// 当前区块，区块链的当前状态（包括所有的索引、统计信息）由该区块以及所有的前置区块共同决定
        /// </summary>
        SYS_CurrentBlock = 0x40,

        /// <summary>
        /// 未花费索引
        /// </summary>
        IX_Unspent = 0x90,

        /// <summary>
        /// 数据库版本
        /// </summary>
        CFG_Version = 0xf0,

        /// <summary>
        /// 数据库是否初始化
        /// </summary>
        CFG_Initialized = 0xf1,
    }
}
