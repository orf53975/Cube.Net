﻿/* ------------------------------------------------------------------------- */
///
/// Client.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cube.Extensions;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Ntp.Client
    ///
    /// <summary>
    /// NTP でサーバと通信するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Client
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(IPAddress ipaddr)
            : this(ipaddr.ToString(), DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(IPAddress ipaddr, int port)
            : this(ipaddr.ToString(), port) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(string hostNameOrAddress)
            : this(hostNameOrAddress, DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(string hostNameOrAddress, int port)
        {
            Host = Dns.GetHostEntry(hostNameOrAddress);
            Timeout = TimeSpan.FromSeconds(5);
            _endpoint = new IPEndPoint(Host.AddressList[0], port);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultPort
        /// 
        /// <summary>
        /// NTP 通信のデフォルトポート番号を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly int DefaultPort = 123;

        /* ----------------------------------------------------------------- */
        ///
        /// Host
        /// 
        /// <summary>
        /// 通信する NTP サーバのホスト情報を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IPHostEntry Host { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Port
        /// 
        /// <summary>
        /// 通信する NTP サーバのポート番号を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Port
        {
            get { return _endpoint.Port; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// NTP パケット受信時のタイムアウト時間を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Task<Packet> GetAsync()
        {
            return GetAsyncCore().Timeout(Timeout);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsyncCore
        /// 
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<Packet> GetAsyncCore()
        {
            await SendToAsync();
            return await ReceiveFromAsync();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SendToAsync
        /// 
        /// <summary>
        /// NTP サーバへパケットを送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Task<int> SendToAsync()
        {
            return Cube.TaskEx.Run<int>(() =>
            {
                var packet = new Ntp.Packet();
                return _socket.SendTo(packet.RawData, _endpoint);
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReceiveFromAsync
        /// 
        /// <summary>
        /// NTP サーバからパケットを受信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Task<Packet> ReceiveFromAsync()
        {
            return Cube.TaskEx.Run<Packet>(() =>
            {
                byte[] raw = new byte[48 + (32 + 128) / 8];
                EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                var bytes = _socket.ReceiveFrom(raw, ref endpoint);
                return new Packet(raw);
            });
        }

        #endregion

        #region Fields
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint _endpoint = null;
        #endregion
    }
}