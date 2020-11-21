using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    public class ServiceStackRedisOptions : IOptions<ServiceStackRedisOptions>
    {
        #region Redis配置

        /// <summary>
        ///     单机的地址，例如：127.0.0.1:6379（默认值）。如果你只用到一个Redis服务端，那么配置此项即可。
        /// </summary>
        public string SingleServer { get; set; } = "127.0.0.1:6379";

        /// <summary>
        ///     读写的地址，例如：{ "192.168.1.1:6379","123456@192.168.1.2:6379","123456@192.168.1.3:6379","123456@192.168.1.4:6379" }
        /// </summary>
        public string[] ReadWriteServers { get; set; }

        /// <summary>
        ///     只读地址，例如：{ "192.168.1.1:6379","123456@192.168.1.3:6379" }
        /// </summary>
        public string[] ReadOnlyServers { get; set; }

        /// <summary>
        ///     MaxWritePoolSize写的频率比读低。默认值 8
        /// </summary>
        public int MaxWritePoolSize { get; set; } = 8;

        /// <summary>
        ///     MaxReadPoolSize读的频繁比较多。默认值 12，Redis官方声明最大连接数为1W，但是连接数要控制。
        /// </summary>
        public int MaxReadPoolSize { get; set; } = 12;

        /// <summary>
        ///     连接最大的空闲时间。默认值 60，Redis官方默认是240
        /// </summary>
        public int IdleTimeOutSecs { get; set; } = 60;

        /// <summary>
        ///     连接超时时间，毫秒。默认值 6000
        /// </summary>
        public int ConnectTimeout { get; set; } = 6000;

        /// <summary>
        ///     数据发送超时时间，毫秒。默认值 6000
        /// </summary>
        public int SendTimeout { get; set; } = 6000;

        /// <summary>
        ///     数据接收超时时间，毫秒。默认值 6000
        /// </summary>
        public int ReceiveTimeout { get; set; } = 6000;

        /// <summary>
        ///     连接池取链接的超时时间，毫秒。默认值 6000
        /// </summary>
        public int PoolTimeout { get; set; } = 6000;

        /// <summary>
        ///     默认的数据库。默认值 0，Redis官方默认也是0
        /// </summary>
        public long DefaultDb { get; set; } = 0;

        #endregion

        ServiceStackRedisOptions IOptions<ServiceStackRedisOptions>.Value => this;
    }
}