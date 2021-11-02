using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BossTool
{
    class Program
    {
        private static byte[] _bossKeyHash = new byte[]
            { 0x52, 0x02, 0xCE, 0x50, 0x99, 0x23, 0x2C, 0x3D, 0x36, 0x5E, 0x28, 0x37, 0x97, 0x90, 0xA9, 0x19 };

        private static byte[] _hmacKeyHash = new byte[]
            { 0xB4, 0x48, 0x2F, 0xEF, 0x17, 0x7B, 0x01, 0x00, 0x09, 0x0C, 0xE0, 0xDB, 0xEB, 0x8C, 0xE9, 0x77 };

        static async Task Main(string[] args)
        {
            Command command = new RootCommand()
            {
                new Option<string>("--boss-key", () => null, "The BOSS encryption key."),
                new Option<string>("--boss-hmac-key", () => null, "The BOSS HMAC key."),
                new Argument<FileInfo>("input", "The file to decrypt."),
                new Argument<FileInfo>("output", "Where to save the decrypted file to.")
            };

            command.Handler = CommandHandler.Create(
                (string bossKey, string bossHmacKey, FileInfo input, FileInfo output) =>
                {
                    Run(bossKey, bossHmacKey, input, output);
                });

            await command.InvokeAsync(args);
        }

        private static void Run(string bossKeyString, string hmacKeyString, FileInfo info, FileInfo outInfo)
        {
            byte[] bossKey = null;
            byte[] hmacKey = null;

            if (bossKeyString != null)
            {
                try
                {
                    bossKey = ByteUtil.ByteArrayFromString(bossKeyString);
                }
                catch (Exception)
                {
                    Console.WriteLine("Malformed BOSS key.");
                    return;
                }
            }

            if (hmacKeyString != null)
            {
                try
                {
                    hmacKey = ByteUtil.ByteArrayFromString(hmacKeyString);
                }
                catch (Exception)
                {
                    Console.WriteLine("Malformed BOSS HMAC key.");
                    return;
                }
            }

            if (bossKey == null)
            {
                Console.WriteLine($"No keys specified. Pass --boss-key with the BOSS encryption key.");
                return;
            }

            using MD5 md5 = MD5.Create();

            if (bossKey.Length != 16 || !md5.ComputeHash(bossKey).SequenceEqual(_bossKeyHash))
            {
                Console.WriteLine("BOSS key is incorrect.");
                return;
            }

            if (hmacKey != null && hmacKey.Length != 0)
            {
                if (hmacKey.Length != 64 || !md5.ComputeHash(hmacKey).SequenceEqual(_hmacKeyHash))
                {
                    Console.WriteLine("BOSS HMAC key is incorrect.");
                    return;
                }
            }

            if (!info.Exists)
            {
                Console.WriteLine("The specified file does not exist.");
                return;
            }

            if (outInfo.Exists)
            {
                Console.WriteLine("The specified output file already exists.");
                return;
            }

            using FileStream stream = new FileStream(info.FullName, FileMode.Open);
            BossContainer bossContainer = new BossContainer(stream, bossKey, hmacKey);

            using FileStream outStream = new FileStream(outInfo.FullName, FileMode.CreateNew);
            outStream.Write(bossContainer.Data, 0, bossContainer.Data.Length);
 
            Console.WriteLine("Done.");
        }
    }
}
