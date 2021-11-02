using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Syroot.BinaryData;

namespace BossTool
{
    public class BossContainer
    {
        private const uint BossContainerMagic = 0x626f7373; // 'boss'
        private const uint BossContainerCafeTypeMagic = 0x00020001;

        public byte[] Data
        {
            get;
            set;
        }
        
        public BossContainer(Stream stream, byte[] bossKey, byte[] hmacKey = null)
        {
            using BinaryDataReader reader = new BinaryDataReader(stream)
            {
                ByteOrder = ByteOrder.BigEndian
            };

            if (reader.ReadUInt32() != BossContainerMagic)
            {
                throw new Exception("Invalid BOSS container magic. Is this really a BOSS container?");
            }

            if (reader.ReadUInt32() != BossContainerCafeTypeMagic)
            {
                throw new Exception("Not a Cafe BOSS container.");
            }

            if (reader.ReadUInt16() != 0x0001)
            {
                throw new Exception("Unknown field not 0x0001.");
            }

            if (reader.ReadUInt16() != 0x0002)
            {
                throw new Exception("Hash type not SHA-256.");
            }

            byte[] nonce = new byte[12];
            reader.Read(nonce, 0, 12);
            
            byte[] decryptedData = new byte[stream.Length - 0x20];
            Data = new byte[decryptedData.Length - 0x20];
            reader.Seek(0x20, SeekOrigin.Begin);

            using AesCtrStream aesStream = new AesCtrStream(stream, bossKey, nonce);
            aesStream.Read(decryptedData, 0, decryptedData.Length);

            Array.Copy(decryptedData, 0x20, Data, 0, Data.Length);

            if (hmacKey != null && hmacKey.Length != 0)
            {
                HMACSHA256 hmac = new HMACSHA256()
                {
                    Key = hmacKey
                };

                byte[] expectedHash = new byte[0x20];
                byte[] computedHash = hmac.ComputeHash(Data);

                if (expectedHash.SequenceEqual(computedHash))
                {
                    throw new Exception("Invalid HMAC. The data may be corrupt.");
                }
            }
        }
    }
}
