using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using LitJson;

/// <summary>
/// 微信支付报文数据类
/// </summary>
public class WeChatPayData
{
    /// <summary>
    /// 微信支付报文数据包，符合微信对报文参数进行字典排序的签名要求
    /// </summary>
    private SortedDictionary<string, object> m_values;

    /// <summary>
    /// 内部数据项数量
    /// </summary>
    public int Count
    {
        get
        {
            return this.m_values.Count;
        }
    }

    public WeChatPayData()
    {
        this.m_values = new SortedDictionary<string, object>();
    }

    /// <summary>
    /// 设置某个字段的值
    /// </summary>
    /// <param name="key">字段名</param>
    /// <param name="value">字段值</param>
    public void SetValue(string key, object value)
    {
        m_values[key] = value;
    }

    /// <summary>
    /// 根据字段名获取某个字段的值
    /// </summary>
    /// <param name="key">字段名</param>
    /// <returns>key对应的字段值</returns>
    public object GetValue(string key)
    {
        object o = null;
        m_values.TryGetValue(key, out o);
        return o;
    }

    /// <summary>
    ///  判断某个字段是否已设置
    /// </summary>
    /// <param name="key">字段名</param>
    /// <returns>若字段key已被设置，则返回true，否则返回false</returns>
    public bool IsSet(string key)
    {
        object o = null;
        m_values.TryGetValue(key, out o);
        if (null != o)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 获取Dictionary
    /// </summary>
    /// <returns></returns>
    public SortedDictionary<string, object> GetValues()
    {
        return m_values;
    }

    /// <summary>
    /// 将Dictionary对象转换成微信支付所需的XML报文数据
    /// </summary>
    /// <returns></returns>
    public string ToXml()
    {
        //数据为空时不能转化为xml格式
        if (0 == m_values.Count)
        {
            return string.Empty;
        }

        string xml = "<xml>";
        foreach (KeyValuePair<string, object> pair in m_values)
        {
            //字段值不能为null，会影响后续流程
            if (pair.Value == null)
            {
                Log.Error(this.GetType().ToString(), pair.Key + "的键值为null");
                throw new Exception(pair.Key + "的键值为null");
            }

            if (pair.Value.GetType() == typeof(int))
            {
                xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
            }
            else if (pair.Value.GetType() == typeof(string))
            {
                xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
            }
            else//除了string和int类型不能含有其他数据类型
            {
                Log.Error(this.GetType().ToString(), "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + "> 参数数据类型错误，只能是string或int");
                throw new Exception("<" + pair.Key + ">" + pair.Value + "</" + pair.Key + "> 参数数据类型错误，只能是string或int");
            }
        }
        xml += "</xml>";
        return xml;
    }

    /// <summary>
    /// 将微信支付返回的XML报文数据转换成Dictionary对象
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public SortedDictionary<string, object> FromXml(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return null;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
        XmlNodeList nodes = xmlNode.ChildNodes;
        foreach (XmlNode xn in nodes)
        {
            XmlElement xe = (XmlElement)xn;
            m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WeChatPayData数据中
        }

        return m_values;

    }


    /// <summary>
    /// 按微信要求，生成报文签名所需的字符串，sign和空参数不参与签名
    /// </summary>
    /// <returns></returns>
    public string ToSignStr()
    {
        string buff = "";
        foreach (KeyValuePair<string, object> pair in m_values)
        {
            if (pair.Value == null)
            {
                Log.Error(this.GetType().ToString(), pair.Key + "的键值为null");
                throw new Exception(pair.Key + "的键值为null");
            }

            if (pair.Key != "sign" && pair.Value.ToString() != "")
            {
                buff += pair.Key + "=" + pair.Value + "&";
            }
        }
        buff = buff.Trim('&');
        return buff;
    }

    /// <summary>
    /// 生成微信浏览器调用JSAPI所需的json格式参数
    /// </summary>
    /// <returns></returns>
    public string ToJson()
    {
        string jsonStr = JsonMapper.ToJson(m_values);
        return jsonStr;
    }

    /// <summary>
    /// 格式化成能在Web页面上显示的结果（因为web页面上不能直接输出xml格式的字符串）
    /// </summary>
    /// <returns></returns>
    public string ToPrintStr()
    {
        string str = "";
        foreach (KeyValuePair<string, object> pair in m_values)
        {
            if (pair.Value == null)
            {
                Log.Error(this.GetType().ToString(), pair.Key + "的键值为null");
                throw new Exception(pair.Key + "的键值为null");
            }

            str += string.Format("{0}={1}<br>", pair.Key, pair.Value.ToString());
        }
        Log.Debug(this.GetType().ToString(), "Print in Web Page : " + str);
        return str;
    }


    /// <summary>
    /// 微信支付的接口签名算法：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=4_3
    /// </summary>
    /// <returns></returns>
    public string MakeSign()
    {
        //转url格式
        string str = ToSignStr();
        //在string后加入商户支付密钥
        str += "&key=" + Config.MCHKEY;
        //MD5加密
        var md5 = MD5.Create();
        var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        var sb = new StringBuilder();
        foreach (byte b in bs)
        {
            sb.Append(b.ToString("x2"));
        }
        //所有字符转为大写
        return sb.ToString().ToUpper();
    }

    /// <summary>
    /// sign签名验证
    /// </summary>
    /// <returns></returns>
    public bool CheckSign()
    {
        //如果没有设置签名，则跳过检测
        if (!IsSet("sign"))
        {
            throw new Exception("没有设置sign签名");
        }
        //如果设置了签名但是签名为空，则抛异常
        else if (GetValue("sign") == null || GetValue("sign").ToString() == "")
        {
            throw new Exception("sign签名为空");
        }

        //对比接收到的sign签名是否一致
        if (MakeSign() == GetValue("sign").ToString())
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// 生成随机数
    /// </summary>
    /// <returns></returns>
    public static string MakeNonceStr()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }

    /// <summary>
    /// 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
    /// </summary>
    /// <returns></returns>
    public static string MakeTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }

}