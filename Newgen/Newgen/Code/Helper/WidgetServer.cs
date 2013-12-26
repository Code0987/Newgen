﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Newgen.Base.Communication
{
    internal class Server
    {
        #region WebServer

        Thread thread = null;
        private bool isalive = true;

        public string CurrentAddress;

        private TcpListener listener;

        public Dictionary<string, Func<string, string>> Commands { get; set; }

        internal bool IsAlive
        {
            get { return this.isalive; }
            set { this.isalive = value; }
        }

        public Server()
        {
            try
            {
                this.IsAlive = true;

                this.Commands = new Dictionary<string, Func<string, string>>();

                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), GetFreeTcpPort());
                listener.Start();

                this.CurrentAddress = listener.LocalEndpoint.ToString();

                thread = new Thread(new ThreadStart(Start));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Name = "Newgen Local Server";
                thread.Start();
            }
            catch (Exception ex) { throw ex; }
        }

        private void Start()
        {
            while (this.IsAlive)
            {
                Socket socket = this.listener.AcceptSocket();
                try
                {
                    if (socket.Connected)
                    {
                        Hashtable headers = new Hashtable();
                        string postdata = null;
                        string response = null;
                        byte[] response_bytes = null;
                        string path = null;
                        string ctype = "text/plain; charset=utf-8";

                        using (NetworkStream stream = new NetworkStream(socket))
                        {
                            headers = this.GetHeaders(stream);
                            path = ((string)headers["Path"]);

                            if (path.Contains("Widget:"))
                            {
                                try
                                {
                                    string cf = E.WidgetsRoot +
                                        System.Web.HttpUtility.UrlDecode(path.Split(':')[1].Split(';')[0]) + "\\" +
                                        System.Web.HttpUtility.UrlDecode(path.Split(':')[1].Split(';')[1]);

                                    if (cf.Contains(".html") || cf.Contains(".htm"))
                                    {
                                        ctype = "text/html; charset=utf-8";
                                        response = File.ReadAllText(cf);
                                        response = response.Replace("./", "./Widget:" + System.Web.HttpUtility.UrlDecode(path.Split(':')[1].Split(';')[0]) + ";");
                                        response = response.Replace("<head>", "<head>\n" + this.GetScript());
                                    }
                                    if (cf.Contains(".png") || cf.Contains(".jpg") || cf.Contains(".jpeg") || cf.Contains(".gif"))
                                    {
                                        ctype = "image/png";
                                        response = null;
                                        response_bytes = File.ReadAllBytes(cf);
                                    }
                                    if (cf.Contains(".js") || cf.Contains(".css"))
                                    {
                                        response = File.ReadAllText(cf);
                                    }
                                }
                                catch { response = "Error loading data."; }
                            }
                            else if (path.Contains(":"))
                            {
                                string cmd = System.Web.HttpUtility.UrlDecode(path.Split(':')[0].Remove(0, 1));
                                postdata = System.Web.HttpUtility.UrlDecode(path.Split(':')[1]);
                                if (postdata != null) response = this.GetCommandResult(cmd, postdata);
                            }
                            else
                            {
                                if (headers["Method"].ToString().ToUpper().Contains("POST") || headers["Method"].ToString().ToUpper().Contains("GET"))
                                    if (headers.ContainsKey("Content-Length"))
                                    {
                                        int cl = 0;
                                        cl = Convert.ToInt32(headers["Content-Length"]);
                                        byte[] buffer = new byte[cl];
                                        stream.ReadTimeout = 1000;
                                        try
                                        {
                                            stream.Read(buffer, 0, buffer.Length);
                                        }
                                        catch { }
                                        postdata = Encoding.ASCII.GetString(buffer);
                                        if (postdata != null) response = this.GetCommandResult(path.Remove(0, 1), postdata);
                                    }
                            }
                        }

                        if (!string.IsNullOrEmpty(response))
                        {
                            byte[] data = Encoding.ASCII.GetBytes(response);
                            this.WriteHeader((string)headers["Protocol"], ctype, data.Length, "200 OK", ref socket);
                            this.WriteData(data, ref socket);
                        }
                        else if (response_bytes != null)
                        {
                            this.WriteHeader((string)headers["Protocol"], ctype, response_bytes.Length, "200 OK", ref socket);
                            this.WriteData(response_bytes, ref socket);
                        }
                        else
                        {
                            this.WriteHeader((string)headers["Protocol"], ctype, 0, "404 Not Found", ref socket);
                        }

                        socket.Close();
                    }
                }
                catch { socket.Close(); }
                finally { socket.Dispose(); }
            }
        }

        private Hashtable GetHeaders(Stream rawdata)
        {
            var headers = new Hashtable();

            var reader = new StreamReader(rawdata);

            var protocolinfo = reader.ReadLine().Split(' ');
            headers.Add("Method", protocolinfo[0]);
            headers.Add("Path", protocolinfo[1]);
            headers.Add("Protocol", protocolinfo[2]);

            var hdr = reader.ReadLine();

            while (!string.IsNullOrEmpty(hdr))
            {
                var kp = hdr.Split(':');
                var value = kp[1].Trim();
                for(var i = 2; i < kp.Length; i++)
                    value += ':' + kp[i];
                headers.Add(kp[0].Trim(), value);

                hdr = reader.ReadLine();
            }

            return headers;
        }

        private void WriteHeader(string httpversion, string mimeheader, int totalbytes, string statuscode, ref Socket socket)
        {
            var buffer = new StringBuilder();

            if (mimeheader.Length == 0) 
                mimeheader = "text/plain; charset=utf-8";

            buffer.Append(httpversion + statuscode + "\r\n");
            buffer.Append("Server: Newgen Local Server\r\n");
            buffer.Append("Content-Type: " + mimeheader + "\r\n");
            buffer.Append("Accept-Ranges: bytes\r\n");
            buffer.Append("Content-Length: " + totalbytes + "\r\n");
            buffer.Append("Connection: Close\r\n");
            buffer.Append("Origin: http://" + socket.RemoteEndPoint.ToString() + "\r\n");
            buffer.Append("Access-Control-Allow-Origin: http://" + socket.RemoteEndPoint.ToString() + "\r\n");
            buffer.Append("Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n");
            buffer.Append("Access-Control-Allow-Headers: Content-Length, Content-type\r\n\r\n");
            
            var data = Encoding.ASCII.GetBytes(buffer.ToString());

            this.WriteData(data, ref socket);
        }

        private void WriteData(Byte[] data, ref Socket socket)
        {
            try
            {
                if (socket.Connected) 
                    socket.Send(data, data.Length, 0);
            }
            catch { }
        }

        private string GetCommandResult(string cmd, string input)
        {
            if (this.Commands.ContainsKey(cmd)) 
                return this.Commands[cmd].Invoke(input);

            return null;
        }

        private static int GetFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private string GetScript()
        {
            try
            {
                string lis = "INVALID";
                lis = (!App.IsProMode) ? "TRIAL" : "LICENSED";

                string script =
@"
<script type=""text/javascript"">
/*
Newgen Runtime Shared Data @ Widget API
---------------------------------------
© 2013 NS, Neeraj.
---------------------------------------
*/

newgen = new (function ()
{
    this.ServerLocation = function () { return 'http://##SL/';}
    this.LicenseStatus = function () { return '##LS';}
    this.Version = function () { return '##V';}
    this.CloseHub = function () { window.location = '$Newgen::CloseHub'; return true;}

    this.SendMessage = function (key, msg) { $Newgen.getResponse('SendMessage', key + ';' + msg, null); }
    this.ASD = function (wn, data, callback) { $Newgen.getResponse('ASD', wn + ';' + data, callback); }
    this.GSD = function (wn, callback) { $Newgen.getResponse('GSD', wn, callback); }
    this.CSD = function (wn, callback) { $Newgen.getResponse('CSD', wn, callback); }

    this.getResponse = function (rell, data, callback, type)
    {
        type = type || 'text';
        var result = null;
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open('GET', $Newgen.ServerLocation() + rell + ':' + data, true);
        if (type == 'xml') { xmlHttp.setRequestHeader('Content-type', 'text/xml; charset=utf-8'); }
        if (type == 'text') { xmlHttp.setRequestHeader('Content-type', 'text/plain; charset=utf-8'); }
        xmlHttp.onreadystatechange = function ()
        {
            if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            {result = xmlHttp.responseText; if(callback !=null)callback(result);}
        }
        xmlHttp.send();
    }

    return 'Newgen';
});
window.$Newgen = newgen;
</script>
";
                script = script.Replace("##SL", App.LocalServer.CurrentAddress);
                script = script.Replace("##LS", lis);
                script = script.Replace("##V", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

                return (script);
            }
            catch { return ""; }
        }

        #endregion WebServer

        #region Defaults

        public static void LoadDefaultCommands(ref Server server)
        {
            server.Commands.Add("SendMessage", (a) =>
            {
                try
                {
                    MessagingHelper.SendMessageToNewgen(a.Split(';')[0], a.Split(';')[1]);
                    return "true";
                }
                catch { return "false"; }
            });

            server.Commands.Add("ASD", (a) =>
            {
                try
                {
                    E.SetSharedLocalData(a.Split(';')[0], a.Split(';')[1]);

                    return "true";
                }
                catch { return "false"; }
            });

            server.Commands.Add("GSD", (a) =>
            {
                try { return E.GetSharedLocalData(a.Trim()); }
                catch { return "null"; }
            });

            server.Commands.Add("CSD", (a) =>
            {
                try { E.ClearSharedLocalData(a.Trim()); return "true"; }
                catch { return "false"; }
            });
        }

        #endregion Defaults
    }
}