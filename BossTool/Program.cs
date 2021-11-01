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
        }
    }
}
