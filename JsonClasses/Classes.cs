using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonClasses
{
    public enum InputType
    {
        Input,
        Message,
        History,
        Refresh,
        Root,
        NeedLogin,
        File,
        SuccessLogin,
        SuccessSignin,
        DeniedLogin,
        DeniedSignin
    }

    public enum SecondType
    {
        Message,
        GetFile,
        Login,
        SignIn, 
        Refresh,
        GetRoot,
        GetHistory
    }
    public static class conv
    {
        public static string ObJsStr(this object mess)
        {
            var jmess = JObject.FromObject(mess);
            var smess = jmess.ToString();
            return smess;
        }

        public static Input ParseInput(string str)
        {
            return JObject.Parse(str).ToObject<Input>();
        }
        public static ISendible Parse(string str)
        {
            var type = JObject.Parse(str).ToObject<ISendible>();
            ISendible mess = null;
            switch (type.fInputType)
            {
                case InputType.Input:
                    mess = ParseInput(str);
                    break;
                case InputType.Message:
                    mess = ParseMessage(str);
                    break;
                case InputType.History:
                    mess = ParseHistory(str);
                    break;
                case InputType.Refresh:
                    mess = ParseRefresh(str);
                    break;
                case InputType.Root:
                    mess = ParseRoot(str);
                    break;
                    
                default:
                    mess = type;
                    break;
            }

            return mess;
        }

        public static Message ParseMessage(string str)
        {
            return JObject.Parse(str).ToObject<Message>();
        }
        public static History ParseHistory(string str)
        {
            return JObject.Parse(str).ToObject<History>();
        }
        public static Refresh ParseRefresh(string str)
        {
            return JObject.Parse(str).ToObject<Refresh>();
        }
        public static Root ParseRoot(string str)
        {
            return JObject.Parse(str).ToObject<Root>();
        }
    }
    public class Input : ISendible
    {
        public Input() => fInputType = InputType.Input;
        public SecondType sInputType;
        public string nick;
        public string text;
        public string file;
        public string pass;
        public string login;
    }
    
    public class ISendible
    {
        public InputType fInputType;
    }
    public class Message : ISendible
    {
        
        public string text;
        public string nick;
        public Message(string text, string nick)
        {
            fInputType = InputType.Message;
            this.text = text;
            this.nick = nick;
        }
    }
    public class user
    {
        public string nick;
        public string text;
        public string login;
    }
    public class Info : ISendible
    {
        
    }
    public class HistoryMessage
    {
        public string login;
        public DateTime time;
        public string text;
        public bool self;
    }
    public class History : ISendible
    {
        public History()=>fInputType = InputType.History;
        public List<HistoryMessage> messages = new List<HistoryMessage>();

        public void Add(DateTime time, string login, string text, bool self)
        {
            messages.Add(new HistoryMessage() { login = login, time = time, text = text, self = self });
        }
    }
    public class Refresh : ISendible
    {
        public Refresh() => fInputType = InputType.Refresh; 
        public List<user> users = new List<user>();
        public void Add(string nick, string login)
        {
            users.Add(new user() { nick = nick, login = login, text = null });
        }
    }
    public class SPath
    {
        public SPath(string p, bool f)
        {
            path = p;
            file = f;
        }
        public string path;
        public bool file;
    }
    public class Root : ISendible
    {
        public Root()=> fInputType = InputType.Root;

        public List<SPath> paths = new List<SPath>();
    }
    
}
