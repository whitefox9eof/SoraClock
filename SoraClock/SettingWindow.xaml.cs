using IWshRuntimeLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SoraClock
{
    public class ResetEventArgs : EventArgs
    {

    }
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        private MainWindow mainWindow = (MainWindow) App.Current.MainWindow;
        private MainSettings settings;
        private Dictionary<string, object> tmp = new Dictionary<string, object>();
        public delegate void ResetHandler(object sender, ResetEventArgs Args);
        public event ResetHandler ResetEvent;

        public SettingWindow()
        {
            InitializeComponent();
            settings = MainSettings.Default;
            foreach(SettingsPropertyValue spv in settings.PropertyValues)
            {
                tmp.Add(spv.Name, spv.PropertyValue);
            }

            // スタートアップ登録チェックボックス初期化
            if (System.IO.File.Exists(getShortcutPath()))
            {
                startupCheckBox.IsChecked = true;
            }
            timeFormatTextBox.Text = settings.TimeFormat;
            // ウィンドウの設定
            clockBackgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(settings.WindowBackgroundColor);
            opacitySlider.Value = (int)(settings.WindowOpacity * 100);
            windowBorderCheckBox.IsChecked = (settings.OutlineBordorThickness.Left > 0) ? true : false;
            // フォント
            ICollectionView view = CollectionViewSource.GetDefaultView(Fonts.SystemFontFamilies);
            new FontFilter(view, this.fontFamilyTextBox);
            clockForegroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(settings.ClockForegroundColor);
            fontFamilyListBox.SelectedItem = settings.FontFamily;
            fontStyleListBox.SelectedItem = settings.FontStyle;
            List<int> fontSizeList = new List<int>(20);
            for (int i = 4; i < 20 + 4; i++) fontSizeList.Add(i * 2);
            fontSizeListBox.ItemsSource = fontSizeList;
            fontSizeListBox.SelectedItem = settings.FontSize;
            // 音量
            voiceVolumeSlider.Value = settings.VoiceVolume;
        }
        /// <summary>
        /// スタートアップ登録
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 時刻表示のフォーマット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeFormatTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TextChangedイベントの時だけsettings=nullになることがありアプリ停止してしまうため
            if (settings != null)
            {
                settings.TimeFormat = timeFormatTextBox.Text;
                settings.Save();
            }
        }
        /// <summary>
        /// ウィンドウの不透明度を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            settings.WindowOpacity = e.NewValue / 100;
            settings.Save();
        }
        /// <summary>
        /// 背景色の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clockBackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            settings.WindowBackgroundColor = clockBackgroundColorPicker.SelectedColorText;
            settings.Save();
        }
        /// <summary>
        /// ウィンドウの枠線の有無
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowBorderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.OutlineBordorThickness = new Thickness(3);
            settings.InlineBordorThickness = new Thickness(1);
            settings.Save();
        }
        private void windowBorderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.OutlineBordorThickness = new Thickness(0);
            settings.InlineBordorThickness = new Thickness(0);
            settings.Save();
        }

        /// <summary>
        /// 時刻文字色の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clockForegroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            settings.ClockForegroundColor = clockForegroundColorPicker.SelectedColorText;
            settings.Save();
        }

        /// <summary>
        /// 音量の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void voiceVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (settings != null)
            {
                settings.VoiceVolume = (int)voiceVolumeSlider.Value;
                settings.Save();
            }
        }
        /// <summary>
        /// 音量テスト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void voiceTestButton_Click(object sender, RoutedEventArgs e)
        {
            settings.VoiceVolume = (int)voiceVolumeSlider.Value;
            settings.Save();
            SCTools.playVoice("time-" + DateTime.Now.Hour + ".wav");
        }
        /// <summary>
        /// フォントファミリー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fontFamilyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fontFamilyListBox.SelectedItem != null)
            {
                // フォントファミリーが変更されたらスタイルを1番目に設定
                settings.FontFamily = (FontFamily)fontFamilyListBox.SelectedItem;
                fontStyleListBox.ItemsSource = settings.FontFamily.FamilyTypefaces;
                FamilyTypeface ft = (FamilyTypeface)fontStyleListBox.Items[1];
                fontStyleListBox.SelectedItem = ft.Style;
                settings.FontStyle = ft.Style;
                settings.Save();
            }
        }
        /// <summary>
        /// フォントスタイル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fontStyleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontStyleListBox.SelectedItem != null)
            {
                // フォントファミリー変更時のスタイルリセットでnullになるため
                FamilyTypeface ft = (FamilyTypeface)fontStyleListBox.SelectedItem;
                settings.FontStyle = ft.Style;
                settings.Save();
            }
        }
        /// <summary>
        /// フォントサイズ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fontSizeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.FontSize = (int)fontSizeListBox.SelectedItem;
            settings.Save();
        }
        /// <summary>
        /// ウィンドウ右下にグリップマークを表示してリサイズ可能にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resizeButton_Click(object sender, RoutedEventArgs e)
        {
            resizeButton.Visibility = Visibility.Collapsed;
            exitResizeButton.Visibility = Visibility.Visible;
            mainWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
            string message = "ウィンドウサイズの変更が可能になりました。\nウィンドウ端をドラッグして変更してください。";
            MessageBox.Show(message);
            mainWindow.moveTopmost();
        }
        /// <summary>
        /// 設定値を初期化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void defaultButton_Click(object sender, RoutedEventArgs e)
        {
            settings.Reset();
            settings.Save();
            mainWindow.moveTopmost();
            // ウィンドウリサイズのみ初期化が適用されないため個別に初期化する
            // ※MainSettingへの保存には成功している
            mainWindow.Width = settings.Width;
            mainWindow.Height = settings.Height;
        }
        /// <summary>
        /// 時刻フォーマットのヘルプリンク
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formatHelpLink_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Documents.Hyperlink hyperlink = (System.Windows.Documents.Hyperlink)e.OriginalSource;
            string target = hyperlink.NavigateUri.OriginalString;
            try { Process.Start(target); }
            catch
            {
                target = target.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {target}") { CreateNoWindow = true });
            }
        }
        /// <summary>
        /// OKボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            tmp = null;
            Close();
        }
        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.ResizeMode = ResizeMode.NoResize;
            if (tmp == null)  return;
            // OKボタン以外で閉じたら設定値を設定前に戻す
            foreach (SettingsPropertyValue spv in settings.PropertyValues)
            {
                settings[spv.Name] = tmp[spv.Name];
                //Debug.WriteLine(spv.Name+":"+spv.PropertyValue);
            }
            settings.Save();
            mainWindow.Width = settings.Width;
            mainWindow.Height = settings.Height;
        }
        /// <summary>
        /// ウィンドウのサイズ変更を終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitResizeButton_Click(object sender, RoutedEventArgs e)
        {
            exitResizeButton.Visibility = Visibility.Collapsed;
            resizeButton.Visibility = Visibility.Visible;
            mainWindow.ResizeMode = ResizeMode.NoResize;
        }
    }
}
