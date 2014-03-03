using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Windows;

namespace UartTester.Settings
{
    public class BackupManger
    {
        const string Backuppath = "Backup";
        const int BackupIntervalSec = 30;

        Settings.SettingManger DataStorage { get; set; }

        DispatcherTimer BackupTimer = new DispatcherTimer();

        public BackupManger(Settings.SettingManger dataStorage)
        {
            DataStorage = dataStorage;
            BackupTimer.Interval = new TimeSpan(0, 0, BackupIntervalSec);
            BackupTimer.Tick += BackupTimer_Tick;

            BackupTimer.Start();
            Application.Current.Exit += Current_Exit;
        }

        public void OpenBackUpIfAny()
        {
            TryLoad();
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == 0)
            {
                FileInfo backupFile = new FileInfo(Backuppath);
                if (backupFile.Exists)
                {
                    try
                    {
                        backupFile.Delete();
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void TryLoad()
        {
            FileInfo backupFile = new FileInfo(Backuppath);
            if (backupFile.Exists)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("There are back-up data, do you want to load", "Back up", MessageBoxButton.YesNo))
                {
                    List<Settings.SettingBase> backedupSettings = new List<Settings.SettingBase>();
                    DataStorage.Deserialize(Backuppath, backedupSettings);
                    DataStorage.Load(backedupSettings);
                }
            }
        }

        void BackupTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                DataStorage.Serilize(Backuppath, DataStorage.Settings);
            }
            catch
            {

            }
        }

    }
}
