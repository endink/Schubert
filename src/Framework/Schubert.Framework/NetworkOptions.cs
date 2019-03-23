using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Schubert.Framework
{
    public class NetworkOptions
    {
        private int _datacenterId = -1;
        private LanOptions[] _lans = null;
        private string _currentIPAddress = null;


        /// <summary>
        /// 获取或设置 Snowflake 数据中心编号（取值范围 1~15）
        /// </summary>
        public int DataCenterId
        {
            get
            {
                this.TryGetIPAddress(out String address);
                return _datacenterId;
            }
        }

        public LanOptions[] Lans
        {
            get { return _lans ?? (_lans = new LanOptions[0]); }
            set
            {
                if (_lans != value)
                {
                    _lans = value;
                    _currentIPAddress = null;
                }
            }
        }

        /// <summary>
        /// 根据网络配置获取当前主机的 ip 地址。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool TryGetIPAddress(out string ipAddress)
        {
            if (_currentIPAddress == null)
            {
                lock (this)
                {
                    if (_currentIPAddress == null)
                    {
                        foreach (var lan in this.Lans)
                        {
                            if (lan.DataCenterId < 0)
                            {
                                continue;
                            }
                            string ip = lan.GetServerIPAddress();
                            if (!ip.IsNullOrWhiteSpace())
                            {
                                _currentIPAddress = ip;
                                _datacenterId = lan.DataCenterId;
                                break;
                            }
                        }
                        _currentIPAddress = _currentIPAddress ?? String.Empty;
                    }
                }
            }
            ipAddress = _currentIPAddress.IfNullOrWhiteSpace(String.Empty);
            return !String.IsNullOrWhiteSpace(ipAddress);
        }
    }

    public class LanOptions
    {
        private int _datacenterId = -1;
        private string _matchedAddress;
        private int? _computedServerNum;

        /// <summary>
        /// 获取或设置 Snowflake 数据中心编号（取值范围 1~15）
        /// </summary>
        public int DataCenterId
        {
            get { return _datacenterId; }
            set
            {
                if (_datacenterId != value)
                {
                    if (value < 1 || value > 15)
                    {
                        throw new ArgumentOutOfRangeException($"{nameof(LanOptions)} 仅支持 1 - 15 的数据中心配置。");
                    }
                    _datacenterId = value;
                }
            }
        }
        private readonly Dictionary<int, string> _ipMasks = null;

        public LanOptions()
        {
            _ipMasks = new Dictionary<int, string>();
        }
        private string GetIpMask(int index)
        {
            string mask = null;
            this._ipMasks.TryGetValue(index, out mask);
            return mask;
        }

        private void SetIpMask(int index, string mask)
        {
            this._ipMasks[index] = mask;
        }
        /// <summary>
        /// 获取或设置网络 1 的网段（例如 192.168.25.*）。
        /// </summary>
        public string LAN1IPMask
        {
            get { return GetIpMask(1); }
            set { SetIpMask(1, value); }
        }

        /// <summary>
        /// 获取或设置 网络2 的网段（例如 192.168.26.*）。
        /// </summary>
        public string LAN2IPMask
        {
            get { return GetIpMask(2); }
            set { SetIpMask(2, value); }
        }

        /// <summary>
        /// 获取或设置 网络3 的网段（例如 192.168.27.*）。
        /// </summary>
        public string LAN3IPMask
        {
            get { return GetIpMask(3); }
            set { SetIpMask(3, value); }
        }

        /// <summary>
        /// 获取或设置 网络4 的网段（例如 192.168.28.*）。
        /// </summary>
        public string LAN4IPMask
        {
            get { return GetIpMask(4); }
            set { SetIpMask(4, value); }
        }



        /// <summary>
        /// 计算当前服务器的 Ip 地址。
        /// </summary>
        private bool ComputeServerId(int lanIndex, out int serverId)
        {
            serverId = -1;
            string serverIP = this.MatchIPAddresses(lanIndex);
            if (!serverIP.IsNullOrWhiteSpace())
            {
                var ipSections = serverIP.Split('.');
                serverId = ((lanIndex - 1) << 8) | int.Parse(ipSections[3]);
            }
            return serverId != -1;
        }

        private String MatchIPAddresses(int lanIndex)
        {
            string ipMask = GetIpMask(lanIndex);
            string ipRegex = ipMask.IfNullOrWhiteSpace(String.Empty)?.Replace("*", @"(\d)+").Replace(".", @"\.");

            if (!ipRegex.IsNullOrWhiteSpace())
            {
                var addresses = SchubertUtility.GetLocalIPV4Addresses().Select(ad => ad.ToString());
                return addresses.FirstOrDefault(s => Regex.IsMatch(s, ipRegex)).IfNullOrEmpty(string.Empty);
            }
            return String.Empty;
        }

        /// <summary>
        /// 根据当前配置获取服务器 IP 地址。
        /// </summary>
        /// <returns></returns>
        public string GetServerIPAddress()
        {
            if (_matchedAddress != null)
            {
                return _matchedAddress;
            }
            for (int i = 1; i <= 4; i++)
            {
                string ip = this.MatchIPAddresses(i);
                if (!String.IsNullOrWhiteSpace(ip))
                {
                    _matchedAddress = ip;
                    return ip;
                }
            }
            _matchedAddress = String.Empty;
            return _matchedAddress;
        }


        /// <summary>
        /// 根据当前配置获取服务器 Id。
        /// </summary>
        /// <returns></returns>
        public int? GetServerId()
        {
            if (_computedServerNum.HasValue)
            {
                return _computedServerNum == -1 ? null : _computedServerNum;
            }
            for (int i = 1; i <= 4; i++)
            {
                int serverId;
                if (ComputeServerId(i, out serverId))
                {
                    _computedServerNum = serverId;
                    return serverId;
                }
            }
            _computedServerNum = -1;
            return null;
        }
    }
}
