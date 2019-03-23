using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Net
{
    /// <summary>
    /// Represents a network endpoint as an host(name or IP address) and a port number.
    /// </summary>
    public class HostAndPort
    {
        private string m_Host = "";
        private int m_Port = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">The port number associated with the host. Value -1 means port not specified.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public HostAndPort(string host, int port)
        {
            Guard.ArgumentNullOrWhiteSpaceString(host, nameof(host));
            if (port < 0 || port > 65535)
            {
                throw new ArgumentException("端口号必须在 0 ~ 65535 之间。");
            }
            m_Host = host;
            m_Port = port;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="endPoint">Host IP end point.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>endPoint</b> is null reference.</exception>
        public HostAndPort(IPEndPoint endPoint)
        {
            Guard.ArgumentNotNull(endPoint, nameof(endPoint));
            m_Host = endPoint.Address.ToString();
            m_Port = endPoint.Port;
        }

        public static bool TryParse(string value, out HostAndPort hostAndPort)
        {
            hostAndPort = null;
            try
            {
                hostAndPort = Parse(value);
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
            }
            return hostAndPort != null;
        }
        
        public static HostAndPort Parse(string hostAndPortString)
        {
            Guard.ArgumentNullOrWhiteSpaceString(hostAndPortString, nameof(hostAndPortString));

            // We have host name with port.
            if (hostAndPortString.IndexOf(':') > -1)
            {
                string[] hostPort = hostAndPortString.Split(new char[] { ':' });
                try
                {
                    if (hostPort.Length == 2)
                    {
                        return new HostAndPort(hostPort[0], Convert.ToInt32(hostPort[1]));
                    }
                    else
                    {
                        throw new ArgumentException($"Argument '{nameof(hostAndPortString)}' has invalid value  ( value : {hostAndPortString} ).");
                    }
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Argument '{nameof(hostAndPortString)}' has invalid value ( value : {hostAndPortString} ).");
                }
            }
            // We have host name without port.
            else
            {
                throw new ArgumentException($"Argument '{nameof(hostAndPortString)}' has invalid value ( value : {hostAndPortString} ).");
            }
        }

        public static bool TryParseMultiple(String hosts, string spliter, out HostAndPort[] result)
        {
            Guard.ArgumentNullOrWhiteSpaceString(spliter, nameof(spliter));
            Guard.ArgumentNullOrWhiteSpaceString(hosts, nameof(hosts));

            result = null;
            try
            {
                result = ParseMultiple(hosts, spliter);
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
            }
            return result != null;
        }

        public static HostAndPort[] ParseMultiple(String hosts, string spliter = ",")
        {
            Guard.ArgumentNullOrWhiteSpaceString(spliter, nameof(spliter));
            Guard.ArgumentNullOrWhiteSpaceString(hosts,nameof(hosts));

            var splited = hosts.Split(new String[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length > 0)
            {
                HostAndPort[] hostResult = new HostAndPort[splited.Length];
                int index = 0;
                foreach (var s in splited)
                {
                    hostResult[index] = Parse(s);
                    index++;
                }
                return hostResult;
            }
            else
            {
                throw new ArgumentException($"Argument '{nameof(hosts)}' has invalid value ( value : {hosts} ).", nameof(hosts));
            }
        }
        


        public override string ToString()
        {
            if (m_Port == -1)
            {
                return m_Host;
            }
            else
            {
                return m_Host + ":" + m_Port.ToString();
            }
        }

        /// <summary>
        /// Gets if <b>Host</b> is IP address.
        /// </summary>
        public bool IsIPAddress
        {
            get { return m_Host.IsIPV4Address(); }
        }

        /// <summary>
        /// Gets host name or IP address.
        /// </summary>
        public string Host
        {
            get { return m_Host; }
        }

        /// <summary>
        /// Gets the port number of the endpoint. Value -1 means port not specified.
        /// </summary>
        public int Port
        {
            get { return m_Port; }
        }

    }
}
