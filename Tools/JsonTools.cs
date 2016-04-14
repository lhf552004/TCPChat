using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace Json
{
    /// <summary>
    /// Json帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }
        private static JsonSerializer serializer = new JsonSerializer();
        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }
    }
    public class ObjectSerialization
    {
        private object _entity;

        /// <summary>
        /// 被序列化得实体对象
        /// </summary>
        public object Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        private string _jsonData;

        /// <summary>
        /// 被转化为json格式数据的对象
        /// </summary>
        public string JsonData
        {
            get { return _jsonData; }
            set { _jsonData = value; }
        }

        /// <summary>
        /// 无参数构造方法
        /// </summary>
        public ObjectSerialization()
        {
        }

        /// <summary>
        /// 有参数构造方法
        /// </summary>
        /// <param name="entity">要被序列化得实体对象</param>
        public ObjectSerialization(object entity)
        {
            this._entity = entity;
        }


        /// <summary>
        /// 序列化实体对象
        /// </summary>
        /// <returns></returns>
        public string EntityToJson()
        {
            var serializer = new DataContractJsonSerializer(Entity.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, Entity);
            byte[] myByte = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(myByte, 0, (int)ms.Length);
            string dataString = Encoding.UTF8.GetString(myByte);
            return dataString;
        }


        /// <summary>
        /// 序列化实体对象
        /// </summary>
        /// <param name="entity">要被序列化得实体对象</param>
        /// <returns></returns>
        public string EntityToJson(object entity)
        {
            this._entity = entity;
            return EntityToJson();
        }

        /// <summary>
        /// 将Json格式数据转换为对象
        /// </summary>
        /// <returns></returns>
        public T GetObjectJson<T>()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonData));
            var serializer = new DataContractJsonSerializer(typeof(T));
            T t = (T)serializer.ReadObject(ms);
            return t;
        }

        /// <summary>
        /// 将Json格式数据转换为对象
        /// </summary>
        /// <param name="jsonData">json数据格式</param>
        /// <returns></returns>
        public T GetObjectJson<T>(string jsonData)
        {
            this._jsonData = jsonData;
            return GetObjectJson<T>();
        }
    }
}