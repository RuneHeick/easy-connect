using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UartTester
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(ex.Message + Environment.NewLine);
                    if (ex.InnerException != null)
                        sb.Append(ex.InnerException.Message + Environment.NewLine);

                    sb.Append(Environment.NewLine + "StackTrace: " + Environment.NewLine + ex.StackTrace);
                    MessageBox.Show(sb.ToString(), "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception t)
            {
                Console.Write("Error");
            }
            finally
            {
                System.Environment.Exit(3);
            }
        }


    }
}
