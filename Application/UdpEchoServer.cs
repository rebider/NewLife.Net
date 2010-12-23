﻿using System.Net;
using NewLife.Net.Sockets;
using NewLife.Net.Udp;

namespace NewLife.Net.Application
{
    /// <summary>
    /// Udp实现的Echo服务
    /// </summary>
    public class UdpEchoServer : UdpNetServer
    {
        /// <summary>
        /// 已重载。
        /// </summary>
        protected override void EnsureCreateServer()
        {
            Name = "Echo服务（UDP）";

            base.EnsureCreateServer();

            UdpServer svr = Server as UdpServer;
            // 允许同时处理多个数据包
            svr.NoDelay = true;
            // 使用线程池来处理事件
            svr.UseThreadPool = true;
        }

        /// <summary>
        /// 已重载。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnReceived(object sender, NetEventArgs e)
        {
            if (e.BytesTransferred > 1024)
            {
                WriteLog("{0}的数据包大于1k，抛弃！", e.RemoteEndPoint);
                return;
            }

            //WriteLog("{0} {1}", e.RemoteEndPoint, Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred));
            WriteLog("{0} [{1}] {2}", e.RemoteEndPoint, e.BytesTransferred, e.GetString());

            if ((e.RemoteEndPoint as IPEndPoint).Address != IPAddress.Any)
            {
                UdpServer us = sender as UdpServer;
                us.Send(e.Buffer, e.Offset, e.BytesTransferred, e.RemoteEndPoint);
                // 这里发送完成后不需要关闭Socket，因为这是UdpServer的Socket
            }
        }
    }
}