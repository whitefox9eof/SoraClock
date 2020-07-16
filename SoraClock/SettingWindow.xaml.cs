using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace SoraClock
{
    public class SettingEventArgs : EventArgs
    {
        public int opacity;
    }
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public delegate void SettingEventHandler(object sender, SettingEventArgs e);
        public event SettingEventHandler settingEvent;

        private MainSettings settings;

        public SettingWindow()
        {
            InitializeComponent();
            // スタートアップ登録チェックボックス初期化
            if(System.IO.File.Exists(getShortcutPath()))
            {
                startupCheckBox.IsChecked = true;
            }
            settings = MainSettings.Default;
            opacitySlider.Value = settings.WindowOpacity;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string shortcutPath = getShortcutPath(); 
            string targetPath = Assembly.GetExecutingAssembly().Location;

            WshShell shell = new WshShell();
            WshShortcut shortcut = (WshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            //アイコンのパス
            shortcut.IconLocation = targetPath + ",0";
            // 引数
            //shortcut.Arguments = "/a /b /c";
            // 作業フォルダ
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            // 実行時のウィンドウの大きさ 1:通常 3:最大化 7:最小化
            shortcut.WindowStyle = 1;
            // コメント
            shortcut.Description = "そら時計スタートアップ登録";
            shortcut.Save();

            //後始末
            Marshal.FinalReleaseComObject(shortcut);
            Marshal.FinalReleaseComObject(shell);
        }

        private string getShortcutPath()
        {
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            shortcutPath = Path.Combine(shortcutPath, Assembly.GetExecutingAssembly().GetName() + ".lnk");
            return shortcutPath;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string FilePath = getShortcutPath();
            FileInfo fileInfo = new FileInfo(FilePath);
            if (fileInfo.Exists)
            {
                if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fileInfo.Attributes = FileAttributes.Normal;
                }
            }
            fileInfo.Delete();
        }

        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SettingEventArgs settingEventArgs = new SettingEventArgs();
            settingEventArgs.opacity = (int)opacitySlider.Value;
            if(settingEvent != null)
            {
                settingEvent(this, settingEventArgs);
            }
        }
    }
}
