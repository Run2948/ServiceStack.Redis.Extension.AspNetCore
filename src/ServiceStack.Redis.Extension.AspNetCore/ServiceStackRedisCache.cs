using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using ServiceStack.Redis;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    /// <summary>
    /// Redis操作类
    /// </summary>
    public partial class ServiceStackRedisCache : IServiceStackRedisCache
    {
        /// <summary>
        /// Redis连接池管理实例
        /// </summary>
        private readonly PooledRedisClientManager _pooledClientManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ServiceStackRedisCache(IOptions<ServiceStackRedisOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;

            if (options.ReadWriteServers == null && options.ReadOnlyServers == null &&
                string.IsNullOrEmpty(options.SingleServer))
            {
                throw new ArgumentNullException(nameof(options.SingleServer));
            }

            if (options.ReadWriteServers?.Length < 1 && options.ReadOnlyServers?.Length < 1 &&
                string.IsNullOrEmpty(options.SingleServer))
            {
                throw new ArgumentNullException(nameof(options.SingleServer));
            }

            var config = new RedisClientManagerConfig
            {
                AutoStart = true,
                MaxWritePoolSize = options.MaxWritePoolSize,
                MaxReadPoolSize = options.MaxReadPoolSize,
                DefaultDb = options.DefaultDb,
            };
            _pooledClientManager = new PooledRedisClientManager(
                options.ReadWriteServers ?? new[] { options.SingleServer }
                , options.ReadOnlyServers ?? new[] { options.SingleServer },
                config)
            {
                ConnectTimeout = options.ConnectTimeout,
                SocketSendTimeout = options.SendTimeout,
                SocketReceiveTimeout = options.ReceiveTimeout,
                IdleTimeOutSecs = options.IdleTimeOutSecs,
                PoolTimeout = options.PoolTimeout
            };
        }

        /// <summary>
        /// 获取Redis客户端连接对象，有连接池管理。
        /// </summary>
        /// <param name="isReadOnly">是否取只读连接。Get操作一般是读，Set操作一般是写</param>
        /// <returns></returns>
        public RedisClient GetRedisClient(bool isReadOnly = false)
        {
            RedisClient result;
            if (!isReadOnly)
            {
                //RedisClientManager.GetCacheClient()会返回一个新实例，而且只提供一小部分方法，它的作用是帮你判断是否用写实例还是读实例
                result = _pooledClientManager.GetClient() as RedisClient;
            }
            else
            {
                //如果你读写是两个做了主从复制的Redis服务端，那么要考虑主从复制是否有延迟。有一些读操作是否是即时的，需要在写实例中获取。
                result = _pooledClientManager.GetReadOnlyClient() as RedisClient;
            }
            //如果你的需求需要经常切换Redis数据库，则下一句可以用。否则一般都只用默认0数据库，集群是没有数据库的概念。
            //result.ChangeDb(Db);
            return result;
        }

        #region  String 操作

        public void SetString(string key, string value, int expirySeconds = -1)
        {
            using var redisClient = GetRedisClient();
            if (expirySeconds == -1)
            {
                redisClient.SetValue(key, value);
            }
            else
            {
                redisClient.SetValue(key, value, new TimeSpan(0, 0, 0, expirySeconds));
            }
        }

        public void SetString(string key, string value, TimeSpan? expireIn)
        {
            using var redisClient = GetRedisClient();
            if (expireIn == null)
            {
                redisClient.SetValue(key, value);
            }
            else
            {
                redisClient.SetValue(key, value, expireIn.Value);
            }
        }

        public string GetString(string key)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetValue(key);
            return val;
        }

        public bool Remove(string key)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.Remove(key);
            return val;
        }

        public void RemoveAll(string[] keys)
        {
            using var redisClient = GetRedisClient();
            redisClient.RemoveAll(keys);
        }

        #endregion

        #region Store-Object 操作

        /// <summary>
        /// 自动根据Id字段产生主键字符串，如果Id是int，没值，则默认就是0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Store<T>(T entity) where T : class, new()
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.Store(entity);
            return val;
        }

        /// <summary>
        /// 可以对基础对象做StoreAll
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void StoreAll<T>(IEnumerable<T> entities) where T : class, new()
        {
            using var redisClient = GetRedisClient();
            redisClient.StoreAll(entities);
        }

        /// <summary>
        /// 可以Store泛型对象，此对象一定要包含Id字段，否则会报错。
        /// e.g.RedisHelper.StoreObject(new {Id = 101,Name = "name_11" });
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object StoreObject(object entity)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.StoreObject(entity);
            return val;
        }

        public T GetById<T>(object id) where T : class, new()
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetById<T>(id);
            return val;
        }

        /// <summary>
        /// 常用。后台关联出表数据时，可以用此函数取出对应用户，然后对表数据刷上用户名。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IList<T> GetByIds<T>(ICollection ids) where T : class, new()
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetByIds<T>(ids);
            return val;
        }

        /// <summary>
        /// 常用。基础数据有时可以直接GetAll出来，然后对IList做过滤排序。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetAll<T>() where T : class, new()
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetAll<T>();
            return val;
        }

        /// <summary>
        /// 后台修改基础数据，要清基础数据的缓存时，可以全清。也可以单笔清DeleteById。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteAll<T>() where T : class, new()
        {
            using var redisClient = GetRedisClient();
            redisClient.DeleteAll<T>();
        }

        public void DeleteById<T>(object id) where T : class, new()
        {
            using var redisClient = GetRedisClient();
            redisClient.DeleteById<T>(id);
        }

        public void DeleteByIds<T>(ICollection ids) where T : class, new()
        {
            using var redisClient = GetRedisClient();
            redisClient.DeleteByIds<T>(ids);
        }

        /// <summary>
        /// 如果不存在key缓存，则添加，返回true。如果已经存在key缓存，则不作操作，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        ///  <param name="expirySeconds">过期秒数</param>
        /// <returns></returns>
        public bool AddIfNotExist<T>(string key, T entity, int expirySeconds = -1)
        {
            using var redisClient = GetRedisClient();
            return expirySeconds == -1 ? redisClient.Add(key, entity) : redisClient.Add(key, entity, new TimeSpan(0, 0, 0, expirySeconds));
        }

        /// <summary>
        /// 如果不存在key缓存，则添加，返回true。如果已经存在key缓存，则不作操作，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        ///  <param name="expireIn">过期时间</param>
        /// <returns></returns>
        public bool AddIfNotExist<T>(string key, T entity, TimeSpan? expireIn)
        {
            using var redisClient = GetRedisClient();
            return expireIn == null ? redisClient.Add(key, entity) : redisClient.Add(key, entity, expireIn.Value);
        }

        public T Get<T>(string key)
        {
            using var redisClient = GetRedisClient(true);
            return redisClient.Get<T>(key);
        }

        public void Set<T>(string key, T entity, int expirySeconds = -1)
        {
            using var redisClient = GetRedisClient();
            if (expirySeconds == -1)
            {
                redisClient.Set(key, entity);
            }
            else
            {
                redisClient.Set(key, entity, new TimeSpan(0, 0, 0, expirySeconds));
            }
        }

        public void Set<T>(string key, T entity, TimeSpan? expireIn)
        {
            using var redisClient = GetRedisClient();
            if (expireIn == null)
            {
                redisClient.Set(key, entity);
            }
            else
            {
                redisClient.Set(key, entity, expireIn.Value);
            }
        }

        /// <summary>
        /// key如果不存在，则添加value，返回true；如果key已经存在，则不添加value，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <param name="expirySeconds">过期秒数</param>
        /// <returns></returns>
        public bool SetIfNotExists<T>(string key, T entity, int expirySeconds = -1)
        {
            using var redisClient = GetRedisClient();
            return expirySeconds == -1 ? redisClient.Add(key, entity) : redisClient.Add(key, entity, new TimeSpan(0, 0, 0, expirySeconds));
        }

        /// <summary>
        /// key如果不存在，则添加value，返回true；如果key已经存在，则不添加value，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <param name="expireIn">过期时间</param>
        /// <returns></returns>
        public bool SetIfNotExists<T>(string key, T entity, TimeSpan? expireIn)
        {
            using var redisClient = GetRedisClient();
            return expireIn == null ? redisClient.Add(key, entity) : redisClient.Add(key, entity, expireIn.Value);
        }

        #endregion

        #region Hash-Store 操作

        /// <summary>
        /// 将一个对象存入Hash。比如对象User有Id=1和Name="aa"属性，则生成的Hash存储是key为urn:user:1，
        /// 第一行field为Id，它的value是1，第二行field为Name，它的value是"aa"。
        /// </summary>
        public void StoreAsHash<T>(T entity)
        {
            using var redisClient = GetRedisClient();
            redisClient.StoreAsHash(entity);
        }

        /// <summary>
        /// 根据Id获取对象(存储的时候使用Hash)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetFromHash<T>(object id)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetFromHash<T>(id);
            return val;
        }

        /// <summary>
        /// 将一组键值对写入一个hash。比如Dictionary<string,string>(){{"d1","1"},{"d2","2"}};，则生成的Hash存储是key为参数hashId，
        /// 第一行field为d1，它的value是1，第二行field为d2，它的value是2。
        /// </summary>
        public void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            using var redisClient = GetRedisClient();
            redisClient.SetRangeInHash(hashId, keyValuePairs);
        }

        /// <summary>
        /// 根据HashId 获取hash对象总数量
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public int GetHashCount(string hashId)
        {
            using var redisClient = GetRedisClient(true);
            return (int)redisClient.GetHashCount(hashId);
        }

        /// <summary>
        /// 根据HashId 获取 hash keys
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> GetHashKeys(string hashId)
        {
            using var redisClient = GetRedisClient(true);
            return redisClient.GetHashKeys(hashId);
        }

        /// <summary>
        /// 根据HashId 获取 hash values
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> GetHashValues(string hashId)
        {
            using var redisClient = GetRedisClient(true);
            return redisClient.GetHashValues(hashId);
        }

        /// <summary>
        /// 根据HashId 获取 hash 字典值
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetEntriesFromHash(string hashId)
        {
            using var redisClient = GetRedisClient(true);
            return redisClient.GetAllEntriesFromHash(hashId);
        }

        /// <summary>
        /// 指定Key 移除 hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        public void RemoveEntryFromHash(string hashId, string key)
        {
            using var redisClient = GetRedisClient();
            redisClient.RemoveEntryFromHash(hashId, key);
        }

        /// <summary>
        /// 指定hashId key 存储Hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetEntryInHash(string hashId, string key, string value)
        {
            using var redisClient = GetRedisClient();
            redisClient.SetEntryInHash(hashId, key, value);
        }

        /// <summary>
        /// 指定hashId key，如果不存在则存储Hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetEntryInHashIfNotExists(string hashId, string key, string value)
        {
            using var redisClient = GetRedisClient();
            redisClient.SetEntryInHashIfNotExists(hashId, key, value);
        }

        /// <summary>
        /// 取一个函数的单个字段值时，用此函数。
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueFromHash(string hashId, string key)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetValueFromHash(hashId, key);
            return val;
        }

        /// <summary>
        /// 取一个Hash的多个字段值时用此函数。
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> GetValuesFromHash(string hashId, params string[] keys)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetValuesFromHash(hashId, keys);
            return val;
        }

        #endregion

        #region List 操作

        /// <summary>
        /// 添加value 到现有List
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddItemToList(string key, string value)
        {
            using var redisClient = GetRedisClient();
            redisClient.AddItemToList(key, value);
        }

        /// <summary>
        /// 添加多个value 到现有List
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AddRangeToList(string key, List<string> values)
        {
            using var redisClient = GetRedisClient();
            redisClient.AddRangeToList(key, values);
        }

        public List<string> GetAllItemsFromList(string key)
        {
            using var redisClient = GetRedisClient(true);
            return redisClient.GetAllItemsFromList(key);
        }
        public void RemoveItemFromList(string listId, string value)
        {
            using var redisClient = GetRedisClient();
            redisClient.RemoveItemFromList(listId, value);
        }

        /// <summary>
        /// 从队列取数据 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="timeoutSeconds">超时秒数</param>
        /// <returns></returns>
        public string DequeueItemFromList(string listId, int timeoutSeconds = -1)
        {
            using var redisClient = GetRedisClient();
            return timeoutSeconds == -1 ? redisClient.DequeueItemFromList(listId) : redisClient.BlockingDequeueItemFromList(listId, new TimeSpan(0, 0, 0, timeoutSeconds));
        }

        /// <summary>
        /// 从队列取数据 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public string DequeueItemFromList(string listId, TimeSpan? timeOut = null)
        {
            using var redisClient = GetRedisClient();
            return timeOut == null ? redisClient.DequeueItemFromList(listId) : redisClient.BlockingDequeueItemFromList(listId, timeOut);
        }

        /// <summary>
        /// 存储数据到队列 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        public void EnqueueItemOnList(string listId, string value)
        {
            using var redisClient = GetRedisClient();
            redisClient.EnqueueItemOnList(listId, value);
        }

        #endregion

        #region Inc-Dec 操作

        public long IncrementValue(string key)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.IncrementValue(key);
            return val;
        }

        public long IncrementValueBy(string key, int count)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.IncrementValueBy(key, count);
            return val;
        }

        public long DecrementValue(string key)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.DecrementValue(key);
            return val;
        }

        public long DecrementValueBy(string key, int count)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.DecrementValueBy(key, count);
            return val;
        }

        #endregion

        #region Other 操作

        public long AppendToValue(string key, string value)
        {
            using var redisClient = GetRedisClient();
            var val = redisClient.AppendToValue(key, value);
            return val;
        }

        public string GetRandomKey()
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetRandomKey();
            return val;
        }

        public List<string> GetValues(List<string> keys)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetValues(keys);
            return val;
        }

        public List<T> GetValues<T>(List<string> keys)
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.GetValues<T>(keys);
            return val;
        }

        public void RemoveByPattern(string pattern)
        {
            using var redisClient = GetRedisClient();
            redisClient.RemoveByPattern(pattern);
        }

        /// <summary>
        /// 按正则条件删除。可以删除一张表前缀
        /// </summary>
        /// <param name="pattern"></param>
        public void RemoveByRegex(string pattern)
        {
            RemoveByPattern(pattern.Replace(".*", "*").Replace(".+", "?"));
        }

        public List<string> GetAllKeys()
        {
            using var redisClient = GetRedisClient(true);
            var val = redisClient.SearchKeys("*");
            return val;
        }

        public void SetAll(IEnumerable<string> keys, IEnumerable<string> values)
        {
            using var redisClient = GetRedisClient();
            redisClient.SetAll(keys, values);
        }

        public void SetAll(Dictionary<string, string> map)
        {
            using var redisClient = GetRedisClient();
            redisClient.SetAll(map);
        }

        public bool Expire(string key, int seconds)
        {
            using var redisClient = GetRedisClient();
            return redisClient.Expire(key, seconds);
        }

        public bool ExpireAt(string key, long unixTime)
        {
            using var redisClient = GetRedisClient();
            return redisClient.ExpireAt(key, unixTime);
        }

        public bool ExpireEntryAt(string key, DateTime expireAt)
        {
            using var redisClient = GetRedisClient();
            return redisClient.ExpireEntryAt(key, expireAt);
        }

        public bool ExpireEntryIn(string key, TimeSpan expireIn)
        {
            using var redisClient = GetRedisClient();
            return redisClient.ExpireEntryIn(key, expireIn);
        }

        #endregion
    }
}
