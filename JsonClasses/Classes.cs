using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonClasses
{
    public static class conv
    {
        public static string ObJsStr(this object mess)
        {
            var jmess = JObject.FromObject(mess);
            var smess = jmess.ToString();
            return smess;
        }
    }
    public class Input
    {
        public string type;
        public string nick;
        public string text;
        public string file;
        public string pass;
        public string login;
    }
    public abstract class ISender
    {

    }
    public class Message : ISender
    {
        public string type = "!Message";
        public string text;
        public string nick;
        public Message(string text, string nick)
        {
            this.text = text;
            this.nick = nick;
        }
    }
    public class user
    {
        public string nick;
        public string text;
    }
    public class History : ISender
    {
        public string type = "!History";
        public List<user> users = new List<user>();

        public void Add(string nick, string text)
        {
            users.Add(new user() { nick = nick, text = text });
        }
    }
    public class Refresh : ISender
    {
        public string type = "!Refresh";
        public List<user> users = new List<user>();
        public void Add(string nick)
        {
            users.Add(new user() { nick = nick, text = null });
        }
    }
    public class SPath
    {
        public SPath(string p,bool f)
        {
            path = p;
            file = f;
        }
        public string path;
        public bool file;
    }
    public class Root : ISender
    {
        public string type = "!Root";

        public List<SPath> paths = new List<SPath>();
    }
}
