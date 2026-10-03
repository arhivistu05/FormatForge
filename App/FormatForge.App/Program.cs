namespace FormatForge.App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TrySetFormatForgeWorkingDirectory();
            ConversionSettings startupSettings = ConversionSettings.Load();
            ApplicationConfiguration.Initialize();
            ThemeManager.SetApplicationColorMode(startupSettings.Theme);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception exception)
            {
                ReportFatalException(exception);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (IsTransientSplitContainerGdiFailure(e.Exception))
            {
                return;
            }

            ReportFatalException(e.Exception);
            Application.Exit();
        }

        private static bool IsTransientSplitContainerGdiFailure(Exception exception)
        {
            return exception is System.Runtime.InteropServices.ExternalException externalException &&
                   externalException.ErrorCode == unchecked((int)0x80004005) &&
                   exception.StackTrace?.Contains("SplitContainer.RepaintSplitterRect", StringComparison.Ordinal) == true;
        }

        private static void ReportFatalException(Exception exception)
        {
            string logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FormatForge",
                "Logs",
                "application.log");

            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath)!);
                System.IO.File.AppendAllText(
                    logPath,
                    $"[{DateTimeOffset.Now:O}] {exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
            }

            MessageBox.Show(
                "FormatForge encountered an unexpected error and must close.\n\n" +
                exception.Message + "\n\nDetails: " + logPath,
                "FormatForge Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void TrySetFormatForgeWorkingDirectory()
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(FormatForgePaths.Root);
            }
            catch
            {
            }
        }
    }
}
