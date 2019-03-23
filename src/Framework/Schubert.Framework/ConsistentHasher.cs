using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 一致性 Hash 随机分布，通常用于负载均衡。
    /// </summary>
    public class ConsistentHasher
    {
        private SortedList<long, string> _nodes = null;
        private int numReps = 160;
        
        /// <summary>
        /// 创建 <see cref="ConsistentHasher"/> 的实例。
        /// </summary>
        /// <param name="nodes">要进行计算的节点集合。</param>
        /// <param name="fillCountPerNode">填充虚拟节点数（当 <paramref name="nodes"/> 节点过少影响随机分布，应进行填充，最佳效果为 10000 节点，即当有 5 个节点时该参数为 200）。</param>
        public ConsistentHasher(IEnumerable<string> nodes, int fillCountPerNode =100)
        {
            _nodes = new SortedList<long, string>();

            numReps = fillCountPerNode;
            //对所有节点，生成nCopies个虚拟结点
            foreach (string node in nodes)
            {
                //每四个虚拟结点为一组
                for (int i = 0; i < numReps / 4; i++)
                {
                    //getKeyForNode方法为这组虚拟结点得到惟一名称 
                    byte[] digest = HashAlgorithm.ComputeMd5(node + i);
                    // Md5是一个16字节长度的数组，将16字节的数组每四个字节一组，分别对应一个虚拟结点，这就是为什么上面把虚拟结点四个划分一组的原因
                    for (int h = 0; h < 4; h++)
                    {
                        long m = HashAlgorithm.Hash(digest, h);
                        _nodes[m] = node;
                    }
                }
            }
        }

        public string GetPrimary(string k)
        {
            byte[] digest = HashAlgorithm.ComputeMd5(k);
            string rv = GetNodeForKey(HashAlgorithm.Hash(digest, 0));
            return rv;
        }

        /// <summary>
        /// 高性能寻址，切勿使用 foreach 和 linq 之类的，该算法降低时间复杂度。
        /// </summary>
        private string GetNodeForKey(long hash)
        {
            string rv;
            long key = hash;
            int pos = 0;
            if (!_nodes.ContainsKey(key))
            {
                int low, high, mid;
                low = 1;
                high = _nodes.Count - 1;
                while (low <= high)
                {
                    mid = (low + high) / 2;
                    if (key < _nodes.Keys[mid])
                    {
                        high = mid - 1;
                        pos = high;
                    }
                    else if (key > _nodes.Keys[mid])
                        low = mid + 1;
                }
            }

            rv = _nodes.Values[pos + 1].ToString();
            return rv;
        }


        private class HashAlgorithm
        {
            public static long Hash(byte[] digest, int nTime)
            {
                long rv = ((long)(digest[3 + nTime * 4] & 0xFF) << 24)
                        | ((long)(digest[2 + nTime * 4] & 0xFF) << 16)
                        | ((long)(digest[1 + nTime * 4] & 0xFF) << 8)
                        | ((long)digest[0 + nTime * 4] & 0xFF);

                return rv & 0xffffffffL; /* Truncate to 32-bits */
            }

            /**
             * Get the md5 of the given key.
             */
            public static byte[] ComputeMd5(string k)
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] buffer = md5.ComputeHash(Encoding.UTF8.GetBytes(k));
                    return buffer;
                }
            }
        }
    }
}
