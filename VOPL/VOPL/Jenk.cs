using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOPL
{
    public class JenkHash
    {
        public JenkHashInputEncoding Encoding { get; set; }
        public string Text { get; set; }
        public int HashInt { get; set; }
        public uint HashUint { get; set; }
        public string HashHex { get; set; }

        public JenkHash(string text, JenkHashInputEncoding encoding)
        {
            Encoding = encoding;
            Text = text;
            HashUint = GenHash(text, encoding);
            HashInt = (int)HashUint;
            HashHex = "0x" + HashUint.ToString("X");
        }


        public static uint GenHash(string text, JenkHashInputEncoding encoding = JenkHashInputEncoding.ASCII)
        {
            uint h = 0;
            byte[] chars;

            switch (encoding)
            {
                default:
                case JenkHashInputEncoding.UTF8:
                    chars = UTF8Encoding.UTF8.GetBytes(text);
                    break;
                case JenkHashInputEncoding.ASCII:
                    chars = ASCIIEncoding.ASCII.GetBytes(text);
                    break;
            }

            for (uint i = 0; i < chars.Length; i++)
            {
                h += chars[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHash(byte[] data)
        {
            uint h = 0;
            for (uint i = 0; i < data.Length; i++)
            {
                h += data[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);
            return h;
        }

    }

    public enum JenkHashInputEncoding
    {
        UTF8 = 0,
        ASCII = 1,
    }

    public static class JenkIndex
    {
        public static Dictionary<uint, string> Index = new Dictionary<uint, string>();
        private static object syncRoot = new object();

        public static void Clear()
        {
            lock (syncRoot)
            {
                Index.Clear();
            }
        }

        public static bool Ensure(string str)
        {
            uint hash = JenkHash.GenHash(str);
            if (hash == 0) return true;
            lock (syncRoot)
            {
                if (!Index.ContainsKey(hash))
                {
                    Index.Add(hash, str);
                    return false;
                }
            }
            return true;
        }

        public static string GetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = hash.ToString();
                }
            }
            return res;
        }
        public static string TryGetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = string.Empty;
                }
            }
            return res;
        }

    }

}
