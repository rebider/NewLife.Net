﻿using System;
using System.Collections.Generic;
using System.Text;
using NewLife.Net.Sockets;
using System.IO.Ports;
using System.Net;
using NewLife.IO;
using System.IO;
using NewLife.Security;

namespace NewLife.Net.Application
{
    /// <summary>串口服务器。把收发数据映射到本地的指定串口</summary>
    public class SerialServer : NetServer
    {
        #region 属性
        private String _PortName = "COM1";
        /// <summary>串口名。默认COM1</summary>
        public String PortName { get { return _PortName; } set { _PortName = value; } }
        #endregion

        #region 构造
        /// <summary>实例化一个串口服务器</summary>
        public SerialServer() { Port = 24; }
        #endregion

        #region 业务
        /// <summary>接受连接时，对于Udp是收到数据时（同时触发OnReceived）</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnAccepted(object sender, NetEventArgs e)
        {
            base.OnAccepted(sender, e);

            var session = e.Socket as ISocketSession;
            using (var sp = new SerialPort(PortName))
            {
                sp.Open();
                ReadAndSend(sp, session, e.RemoteEndPoint);
            }
        }

        /// <summary>收到数据时</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnReceived(object sender, NetEventArgs e)
        {
            base.OnReceived(sender, e);

            var session = e.Socket as ISocketSession;
            using (var sp = new SerialPort(PortName))
            {
                sp.Open();
                if (e.BytesTransferred > 0)
                {
                    WriteLog("Net=>SerialPort: {0}", e.Buffer.ToHex(e.Offset, e.BytesTransferred));
                    sp.Write(e.Buffer, e.Offset, e.BytesTransferred);
                }

                ReadAndSend(sp, session, e.RemoteEndPoint);
            }
        }

        void ReadAndSend(SerialPort sp, ISocketSession session, EndPoint remote)
        {
            // 读取数据
            var ms = new MemoryStream();
            while (sp.BytesToRead > 0)
            {
                Int32 d = sp.ReadByte();
                if (d != -1) ms.WriteByte((Byte)d);
            }

            if (ms.Length > 0)
            {
                ms.Position = 0;
                WriteLog("SerialPort=>Net: {0}", ms.ReadBytes().ToHex());
                ms.Position = 0;
                session.Send(ms, remote);
            }
        }
        #endregion
    }
}