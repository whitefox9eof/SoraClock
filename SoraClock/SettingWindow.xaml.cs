using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace SoraClock
{
    public class SettingEventArgs : EventArgs
    {
        public int opacity = -1;
        public Color? clockBackgroundColor = null;
        public Color? clockForegroundColor = null;
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
            settings = MainSettings.Default;
            // スタートアップ登録チェックボックス初期化
            if (System.IO.File.Exists(getShortcutPath()))
            {
                startupCheckBox.IsChecked = true;
            }
            // 背景色
            clockBackgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(settings.ClockBackgroundColor);
            // 背景不透明度
            opacitySlider.Value = settings.WindowOpacity;
            // 時刻の文字色
            clockForegroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(settings.ClockForegroundColor); 
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
        /// <summary>
        /// スタートアップのパス
        /// </summary>
        /// <returns></returns>
        private static string getShortcutPath()
        {
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            shortcutPath = Path.Combine(shortcutPath, Assembly.GetExecutingAssembly().GetName() + ".lnk");
            return shortcutPath;
        }
        /// <summary>
        /// スタートアップを解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 背景の不透明度を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SettingEventArgs args = new SettingEventArgs();
            args.opacity = (int)e.NewValue;

            if (settingEvent != null)
            {
                settingEvent(this, args);
            }

            settings.WindowOpacity = args.opacity;
            settings.Save();
        }
        /// <summary>
        /// 背景色の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clockBackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SettingEventArgs args = new SettingEventArgs();
            args.clockBackgroundColor = (Color)e.NewValue;

            if (settingEvent != null)
            {
                settingEvent(this, args);
            }

            settings.ClockBackgroundColor = clockBackgroundColorPicker.SelectedColorText;
            settings.Save();
        }

        /// <summary>
        /// 時刻文字色の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clockForegroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SettingEventArgs settingEventArgs = new SettingEventArgs();
            settingEventArgs.clockForegroundColor = (Color)e.NewValue;
            if (settingEvent != null)
            {
                settingEvent(this, settingEventArgs);
            }

            settings.ClockForegroundColor = clockForegroundColorPicker.SelectedColorText;
            settings.Save();
        }
    }
}
