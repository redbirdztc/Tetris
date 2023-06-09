using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace Tetris
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AllocConsole();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var log = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}").CreateLogger();


            var tetris = new Tetris();
            var gameplay = new GamePlay(log, tetris, 20, 10);
            new Thread(gameplay.Start).Start();

            Application.Run(tetris);

            log.Debug("Game ready.");

            FreeConsole();
        }
    }
}