using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace Tetris
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine(@"program start");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var log = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

            log.Debug("Hello world!");


            var tetris = new Tetris();

            var gameplay = new GamePlay(tetris, 20, 10);
            new Thread(gameplay.Start).Start();
            new Thread(() =>
            {
                for (;;)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    Console.ReadLine();
                }
            }).Start();
            Application.Run(tetris);
        }
    }
}