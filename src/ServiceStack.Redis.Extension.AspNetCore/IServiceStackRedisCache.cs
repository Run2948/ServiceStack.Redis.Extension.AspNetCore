using System;
using System.Collections;
using System.Collections.Generic;
using ServiceStack.Redis;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    public partial interface IServiceStackRedisCache
    {
        /// <summary>
        /// 获取Redis客户端连接对象，有连接池管理。
        /// </summary>
        /// <param name="isReadOnly">是否取只读连接。Get操作一般是读，Set操作一般是写</param>
        /// <returns></returns>
        RedisClient GetRedisClient(bool isReadOnly = false);

        #region String 操作

        void SetString(string key, string value, int expirySeconds = -1);

        void SetString(string key, string value, TimeSpan? expireIn);

        string GetString(string key);

        bool Remove(string key);

        void RemoveAll(string[] keys);

        #endregion

        #region Store-Object 操作

        /// <summary>
        /// 自动根据Id字段产生主键字符串，如果Id是int，没值，则默认就是0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        T Store<T>(T entity) where T : class, new();

        /// <summary>
        /// 可以对基础对象做StoreAll
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void StoreAll<T>(IEnumerable<T> entities) where T : class, new();

        /// <summary>
        /// 可以Store泛型对象，此对象一定要包含Id字段，否则会报错。
        /// e.g.RedisHelper.StoreObject(new {Id = 101,Name = "name_11" });
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        object StoreObject(object entity);

        T GetById<T>(object id) where T : class, new();

        /// <summary>
        /// 常用。
        /// 后台关联出表数据时，可以用此函数取出对应用户，然后对表数据刷上用户名。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        IList<T> GetByIds<T>(ICollection ids) where T : class, new();

        /// <summary>
        /// 常用。基础数据有时可以直接GetAll出来，然后对IList做过滤排序。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IList<T> GetAll<T>() where T : class, new();

        /// <summary>
        /// 后台修改基础数据，要清基础数据的缓存时，可以全清。也可以单笔清DeleteById。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void DeleteAll<T>() where T : class, new();

        void DeleteById<T>(object id) where T : class, new();

        /// <summary>
        /// 常用。后台关联出表数据时，可以用此函数取出对应用户，然后对表数据刷上用户名。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        void DeleteByIds<T>(ICollection ids) where T : class, new();

        /// <summary>
        /// 如果不存在key缓存，则添加，返回true。如果已经存在key缓存，则不作操作，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        ///  <param name="expirySeconds">过期秒数</param>
        /// <returns></returns>
        bool AddIfNotExist<T>(string key, T entity, int expirySeconds = -1);

        /// <summary>
        /// 如果不存在key缓存，则添加，返回true。如果已经存在key缓存，则不作操作，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        ///  <param name="expireIn">过期时间</param>
        /// <returns></returns>
        bool AddIfNotExist<T>(string key, T entity, TimeSpan? expireIn);

        T Get<T>(string key);

        void Set<T>(string key, T entity, int expirySeconds = -1);

        void Set<T>(string key, T entity, TimeSpan? expireIn);

        /// <summary>
        /// key如果不存在，则添加value，返回true；如果key已经存在，则不添加value，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <param name="expirySeconds">过期秒数</param>
        /// <returns></returns>
        bool SetIfNotExists<T>(string key, T entity, int expirySeconds = -1);

        /// <summary>
        /// key如果不存在，则添加value，返回true；如果key已经存在，则不添加value，返回false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <param name="expireIn">过期时间</param>
        /// <returns></returns>
        bool SetIfNotExists<T>(string key, T entity, TimeSpan? expireIn);

        #endregion

        #region Hash-Store 操作

        /// <summary>
        /// 将一个对象存入Hash。比如对象User有Id=1和Name="aa"属性，则生成的Hash存储是key为urn:user:1，
        /// 第一行field为Id，它的value是1，第二行field为Name，它的value是"aa"。
        /// </summary>
        void StoreAsHash<T>(T entity);

        /// <summary>
        /// 根据Id获取对象(存储的时候使用Hash)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetFromHash<T>(object id);

        /// <summary>
        /// 将一组键值对写入一个hash。比如Dictionary<string,string>(){{"d1","1"},{"d2","2"}};，则生成的Hash存储是key为参数hashId，
        /// 第一行field为d1，它的value是1，第二行field为d2，它的value是2。
        /// </summary>
        void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string, string>> keyValuePairs);

        /// <summary>
        /// 根据HashId 获取hash对象总数量
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        int GetHashCount(string hashId);

        /// <summary>
        /// 根据HashId 获取 hash keys
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        List<string> GetHashKeys(string hashId);

        /// <summary>
        /// 根据HashId 获取 hash values
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        List<string> GetHashValues(string hashId);

        /// <summary>
        /// 根据HashId 获取 hash 字典值
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        Dictionary<string, string> GetEntriesFromHash(string hashId);

        /// <summary>
        /// 指定Key 移除 hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        void RemoveEntryFromHash(string hashId, string key);

        /// <summary>
        /// 指定hashId key 存储Hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetEntryInHash(string hashId, string key, string value);

        /// <summary>
        /// 指定hashId key，如果不存在则存储Hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetEntryInHashIfNotExists(string hashId, string key, string value);

        /// <summary>
        /// 取一个函数的单个字段值时，用此函数。
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetValueFromHash(string hashId, string key);

        /// <summary>
        /// 取一个Hash的多个字段值时用此函数。
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<string> GetValuesFromHash(string hashId, params string[] keys);

        #endregion

        #region List 操作

        /// <summary>
        /// 添加value 到现有List
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddItemToList(string key, string value);

        /// <summary>
        /// 添加多个value 到现有List
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        void AddRangeToList(string key, List<string> values);

        List<string> GetAllItemsFromList(string key);

        void RemoveItemFromList(string listId, string value);

        /// <summary>
        /// 从队列取数据 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="timeoutSeconds">超时秒数</param>
        /// <returns></returns>
        string DequeueItemFromList(string listId, int timeoutSeconds = -1);

        /// <summary>
        /// 从队列取数据 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        string DequeueItemFromList(string listId, TimeSpan? timeOut = null);

        /// <summary>
        /// 存储数据到队列 先进先出
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        void EnqueueItemOnList(string listId, string value);

        #endregion

        #region Inc-Dec 操作

        long IncrementValue(string key);

        long IncrementValueBy(string key, int count);

        long DecrementValue(string key);

        long DecrementValueBy(string key, int count);

        #endregion

        #region Other 操作

        long AppendToValue(string key, string value);

        string GetRandomKey();

        List<string> GetValues(List<string> keys);

        List<T> GetValues<T>(List<string> keys);

        void RemoveByPattern(string pattern);

        /// <summary>
        /// 按正则条件删除。可以删除一张表前缀
        /// </summary>
        /// <param name="pattern"></param>
        void RemoveByRegex(string pattern);

        List<string> GetAllKeys();

        void SetAll(IEnumerable<string> keys, IEnumerable<string> values);

        void SetAll(Dictionary<string, string> map);

        bool Expire(string key, int seconds);

        bool ExpireAt(string key, long unixTime);

        bool ExpireEntryAt(string key, DateTime expireAt);

        bool ExpireEntryIn(string key, TimeSpan expireIn);

        #endregion
    }
}