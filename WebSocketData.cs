using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using HitboxDashboard_WPF.Properties;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace HitboxDashboard_WPF
{
    public sealed class WebSocketData
    {
        private static WebSocket _websocket;
        public static Thread UserListThread;
        public static Thread BanListThread;
        private readonly StringBuilder _baseMessage = new StringBuilder("5:::{\"name\":\"message\",\"args\":");
        private readonly static String WSAddress = GetWsAddress();
        private readonly MainWindow hb;

        public WebSocketData(MainWindow hitbox)
        {
            _websocket = new WebSocket("ws://"+WSAddress+"/socket.io/1/websocket/" + GetWsConnId());

            _websocket.Opened += websocket_Opened;
            _websocket.MessageReceived += websocket_MessageReceived;
            _websocket.Error += websocket_Error;
            _websocket.Closed += websocket_Closed;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            hb = hitbox;
            _websocket.Open();
            _websocket.EnableAutoSendPing = false;
        }

        public event EventHandler<WebSocketMessageEventArgs> WebSocketMessage;

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (UserListThread != null)
                UserListThread.Abort();
        }

        public void timer_Tick(object sender, EventArgs e)
        {
            if (hb.tabControl.IsFocused)
            {
                GuiMessage(hb.Username, "SYSTEM: User List is focused, focus on chat to allow updating.", "000000");
                return;
            }
            SendUserListMessage();
            Debug.WriteLine("User List Sent.");
        }

        private static String GetWsConnId()
        {
            var fullConnId = API.Get("http://" + WSAddress + "/socket.io/1").ToString();
            return fullConnId.Substring(0, fullConnId.IndexOf(":", StringComparison.Ordinal));
        }

        private static String GetWsAddress()
        {
            using (var client = new WebClient())
            {
                return
                    JArray.Parse(client.DownloadString("http://www.hitbox.tv/api/chat/servers.json?redis=true"))
                        .First.SelectToken("server_ip")
                        .ToString();
            }
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            Trace.WriteLine("Connection has been closed.");
            GuiMessage(hb.Username, "SYSTEM: Chat connection closed. Restart application.", "000000");
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            Trace.WriteLine("Error: " + e.Exception.GetBaseException().Message);
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Trace.WriteLine(e.Message);
            if (e.Message.Equals("1::")) return;
            if (e.Message.Equals("2::"))
            {
                _websocket.Send("2::");
                return;
            }
            if (e.Message.StartsWith("5:::"))
            {
                var jsonObj = JObject.Parse(e.Message.Substring(4));
                var args = JObject.Parse(jsonObj.GetValue("args").First.ToString());
                var method = args.GetValue("method");
                var paramsObject = args.GetValue("params");

                switch (method.ToString())
                {
                    case "loginMsg":
                        GuiMessage(hb.Username, "SYSTEM: Successfully connected to server.", "000000");
                        SendUserListMessage();
                        break;

                    case "chatMsg":
                        GuiMessage(paramsObject["name"].ToString(), paramsObject["text"].ToString(),
                            paramsObject["nameColor"].ToString());
                        break;
                    case "userList":
                        UserListThread = new Thread(() => { hb.LoadUserData(paramsObject.ToString()); });
                        UserListThread.Start();
                        break;
                    case "banList":
                        BanListThread = new Thread(() => { hb.LoadBanList(paramsObject.ToString()); });
                        BanListThread.Start();
                        break;
                    default:
                        return;
                }
                //Debug.WriteLine(jsonObj);
                //Debug.WriteLine("jsonObj: {0}", jsonObj);
                //Debug.WriteLine("test: {0}", args);
                //Debug.WriteLine("method: {0}", method);
                //Debug.WriteLine("Params: {0}", paramsObject);
            }
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Trace.WriteLine("Connected");

            SendJoinChannelMessage();
        }

        public void GuiMessage(string username, string messasge, string color)
        {
            var wsMessage = new WebSocketMessageEventArgs {UserName = username, Message = messasge, Color = color};
            OnWebSocketMessageReceived(wsMessage);
        }

        private void SendJoinChannelMessage()
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"joinChannel\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() +
                      "\",\"name\":\"" + hb.Username + "\",\"token\":\"" + hb.AuthToken + "\"}}]}");
            _websocket.Send(sb.ToString());
        }

        private void SendUserListMessage()
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"getChannelUserList\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() +
                      "\"}}]}");
            _websocket.Send(sb.ToString());
        }

        private void OnWebSocketMessageReceived(WebSocketMessageEventArgs e)
        {
            if (WebSocketMessage != null)
                WebSocketMessage(this, e);
        }

        // Public Send Methods

        #region Public Send

        public void SendChat(string msg)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"chatMsg\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      hb.Username + "\",\"nameColor\":\"FA58F4\",\"text\":\"" + msg + "\"}}]}");
            Debug.WriteLine(sb.ToString());
            _websocket.Send(sb.ToString());
        }

        public void SendTimeout(string user)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"kickUser\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      user + "\",\"token\":\"" + hb.AuthToken + "\",\"timeout\":600}}]}");
            _websocket.Send(sb.ToString());
        }

        public void SendBan(string user)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"banUser\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      user + "\"}}]}");
            _websocket.Send(sb.ToString());
        }

        public void SendUnBan(string user)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"unbanUser\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      user + "\",\"token\":\"" + hb.AuthToken + "\"}}]}");
            _websocket.Send(sb.ToString());
        }

        public void SendMod(string user)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"makeMod\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      user + "\",\"token\":\"" + hb.AuthToken + "\"}}]}");
            _websocket.Send(sb.ToString());
        }

        public void SendUnMod(string user)
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"removeMod\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() + "\",\"name\":\"" +
                      user + "\",\"token\":\"" + hb.AuthToken + "\"}}]}");
            _websocket.Send(sb.ToString());
        }
        #endregion


    }

    public class WebSocketMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Color { get; set; }
    }
}