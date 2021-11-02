using System;
using System.IO;
using System.Security.Cryptography;

namespace BossTool
{
    public class AesCtrStream : Stream
    {
        public override bool CanRead => _innerStream.CanRead;

        // TODO
        public override bool CanSeek => false;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => throw new NotImplementedException();
        }

        private readonly Stream _innerStream;
        private readonly AesManaged _aes;
        private readonly byte[] _key;
        private readonly byte[] _nonce;

        private uint _counter;
        private byte[] _block;
        private int _blockIndex;

        public AesCtrStream(Stream stream, byte[] key, byte[] nonce)
        {
            _innerStream = stream;
            
            _aes = new AesManaged()
            {
                Mode = CipherMode.ECB,
            };

            _key = key;
            _nonce = nonce;
            _counter = 0;
            _block = new byte[_aes.BlockSize / 8];
            _blockIndex = 0;
            
            UpdateBlock();
        }

        public new void Dispose()
        {
            _aes.Dispose();
        }

        private void UpdateBlock()
        {
            _counter++;
            _blockIndex = 0;

            byte[] counterBytes = BitConverter.GetBytes(_counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }
            
            byte[] nonceWithCounter = new byte[16];
            Array.Copy(_nonce, nonceWithCounter, 12);
            Array.Copy(counterBytes, 0, nonceWithCounter, 12, 4);

            ICryptoTransform transform = _aes.CreateEncryptor(_key, nonceWithCounter);
            transform.TransformBlock(nonceWithCounter, 0, 16, _block, 0);
        }
        
        public override void Flush()
        {
            _innerStream.Flush();
        }
        
        // TODO
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] readBytes = new byte[count];
            int actualCount = _innerStream.Read(readBytes, 0, count);

            for (int i = 0; i < actualCount; i++)
            {
                buffer[offset + i] = (byte)(readBytes[i] ^ _block[_blockIndex]);

                _blockIndex++;
                if (_blockIndex == 16)
                {
                    UpdateBlock();
                }
            }

            return actualCount;
        }
        
        // TODO
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}
