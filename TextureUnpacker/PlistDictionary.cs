using System;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;

public class PlistDictionary : Dictionary<string, XmlNode>
{
	public PlistDictionary()
	{

	}

    public PlistDictionary(XmlNode root)
    {
        var children = root.ChildNodes;

        for (int i = 0; i < children.Count; ++i)
        {
            var kNode = children[i];
            if (kNode.Name == "key")
            {
                this[kNode.InnerText] = children[++i];
            }
        }
    }

    /// <summary>
    /// 获取bool值，默认值为false
    /// </summary>
    public bool GetBool(string key)
    {
        var node = GetNode(key);
        if (node == null) return false;

        return bool.Parse(string.IsNullOrEmpty(node.InnerText) ? node.Name : node.InnerText);
    }

    /// <summary>
    /// 获取int值，默认值为0
    /// </summary>
    public int GetInt(string key)
    {
        var node = GetNode(key);
        if (node == null) return 0;

        int.TryParse(node.InnerText, out int value);
        return value;
    }

    /// <summary>
    /// 获取string值，默认值为""
    /// </summary>
    public string GetString(string key)
    {
        var node = GetNode(key);
        return node == null ? "" : node.InnerText;
    }

    /// <summary>
    /// 获取Size值，默认值为Size.Empty
    /// </summary>
    public Size GetSize(string key)
    {
        var str = GetString(key);
        if (str == "") return Size.Empty;

        String[] slist = str.Replace('{', ' ').Replace('}', ' ').Split(',');
        return new Size(int.Parse(slist[0].Trim()), int.Parse(slist[1].Trim()));
    }

    public Point GetPoint(string key)
    {
        var str = GetString(key);
        if (str == "") return Point.Empty;

        String[] slist = str.Replace('{', ' ').Replace('}', ' ').Split(',');
        return new Point(int.Parse(slist[0].Trim()), int.Parse(slist[1].Trim()));
    }

    public Rectangle GetRectangle(string key)
    {
        var str = GetString(key);
        if (str == "") return Rectangle.Empty;

        String[] slist = str.Replace('{', ' ').Replace('}', ' ').Split(',');
        return new Rectangle(
            int.Parse(slist[0].Trim()),
            int.Parse(slist[1].Trim()),
            int.Parse(slist[2].Trim()),
            int.Parse(slist[3].Trim())
            );
    }

    public XmlNode GetNode(string key)
    {
        TryGetValue(key, out XmlNode node);
        return node;
    }

    public Rectangle ToRectangle()
    {
        return new Rectangle(
                        GetInt("x"),
                        GetInt("y"),
                        GetInt("width"),
                        GetInt("height")
                    );
    }

    public Point ToPoint(string x, string y)
    {
        return new Point(
                        GetInt(x),
                        GetInt(y)
                    );
    }

    public Size ToSize(string width, string height)
    {
        return new Size(
                        GetInt(width),
                        GetInt(height)
                    );
    }
}
