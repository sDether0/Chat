using System;
using System.Collections.Generic;

namespace JsonClasses
{
    public interface IInputProvider
    {
         ISendible CreateInput(InputType? fType = null, SecondType? sType = null, List<HistoryMessage> history = null, List<user> users = null, List<SPath> root = null, params string[] args);
    }

    public interface IInfoProvider : IInputProvider
    {
         ISendible SuccessLogin { get; }
         ISendible SuccessSignin { get; }
         ISendible DeniedLogin { get; }
         ISendible DeniedSignin { get; }
         ISendible NeedLogin { get; }
         ISendible File { get; }
    }

    public class ClientInputProvider : IInputProvider
    {
        public ISendible CreateInput(InputType? fType = null, SecondType? sType = null, List<HistoryMessage> history = null, List<user> users = null, List<SPath> root = null, params string[] args)
        {
            if (sType == null) throw new Exception("Input type is null");
            Input input = new Input{ sInputType = (SecondType)sType };
            switch (sType)
            {
                case SecondType.Message:
                    input.nick = args[0];
                    input.text = args[1];
                    break;
                case SecondType.GetFile:
                    input.file = args[0];
                    break;
                case SecondType.Login:
                    input.login = args[0];
                    input.pass = args[1];
                    break;
                case SecondType.SignIn:
                    input.login = args[0];
                    input.pass = args[1];
                    input.nick = args[2];
                    break;
                case SecondType.Refresh:
                    break;
                case SecondType.GetRoot:
                    break;
                case SecondType.GetHistory:
                    break;
            }
            return input;
        }
    }
    public class ServerInputProvider : IInfoProvider
    {
        static readonly ISendible needLogin = new Info() { fInputType = InputType.NeedLogin };
        static readonly ISendible successLogin = new Info() { fInputType = InputType.SuccessLogin };
        static readonly ISendible successSignin = new Info() { fInputType = InputType.SuccessSignin };
        static readonly ISendible deniedLogin = new Info() { fInputType = InputType.DeniedLogin };
        static readonly ISendible deniedSignin = new Info() { fInputType = InputType.DeniedSignin };
        static readonly ISendible file = new Info() { fInputType = InputType.File };
        public ISendible CreateInput(InputType? fType = null, SecondType? sType = null, List<HistoryMessage> history = null, List<user> users = null, List<SPath> root = null, params string[] args)
        {
            if (fType == null) throw new Exception("Input type is null");
            ISendible sendible = null;
            switch (fType)
            {
                case InputType.Input:
                    throw new InvalidOperationException();
                case InputType.Message:
                    sendible = new Message(args[1], args[0]);
                    break;
                case InputType.History:
                    sendible = new History() { messages = history };
                    break;
                case InputType.Refresh:
                    sendible = new Refresh() { users = users };
                    break;
                case InputType.Root:
                    sendible = new Root() { paths = root };
                    break;
                case InputType.NeedLogin:
                case InputType.SuccessLogin:
                case InputType.SuccessSignin:
                case InputType.DeniedLogin:
                case InputType.DeniedSignin:
                case InputType.File:
                    sendible = new Info() { fInputType = (InputType)fType };
                    break;
            }
            return sendible;
        }
        public ISendible NeedLogin
        {
            get => needLogin;
        }
        public ISendible SuccessLogin
        {
            get => successLogin;
        }
        public ISendible DeniedLogin
        {
            get => deniedLogin;
        }
        public ISendible DeniedSignin
        {
            get => deniedSignin;
        }
        public ISendible SuccessSignin
        {
            get => successSignin;
        }
        public ISendible File
        {
            get => file;
        }

    }
}