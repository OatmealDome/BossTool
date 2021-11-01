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
        static async Task Main(string[] args)
        {
            Command command = new RootCommand()
            {
                new Option<string>("--boss-key", () => null, "The BOSS encryption key."),
                new Option<string>("--boss-hmac-key", () => null, "The BOSS HMAC key."),
                new Argument<FileInfo>("file", "The file to decrypt.")
            };

            command.Handler = CommandHandler.Create((string bossKey, string bossHmacKey, FileInfo file) =>
            {
                Run(bossKey, bossHmacKey, file);
            });

            await command.InvokeAsync(args);
        }

        private static void Run(string bossKeyString, string hmacKeyString, FileInfo info)
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
        }
    }
}
