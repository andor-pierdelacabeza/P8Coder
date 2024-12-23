using System;
using System.IO;
using System.Windows.Forms;

namespace P8Coder
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            CoderForm mainForm = new();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1])) mainForm.LoadProject(args[1]);

            Application.Run(mainForm);
        }
    }
}
