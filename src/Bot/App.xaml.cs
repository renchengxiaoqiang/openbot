using BotLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Bot
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
            SessionEnding += App_SessionEnding;
            Exit += App_Exit;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_Exit(object sender, ExitEventArgs e)
        {

        }

        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {

        }

        void App_Startup(object sender, StartupEventArgs e)
        {

        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                Log.Error("出现UnhandledException");
                Log.Exception(e.Exception);
            }
            e.Handled = true;
        }
    }
}
