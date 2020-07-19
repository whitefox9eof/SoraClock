﻿using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace SoraClock
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
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
            timeFormatTextBox.Text = settings.TimeFormat;
            // ウィンドウの設定
            clockBackgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(settings.WindowBackgroundColor);
            opacitySlider.Value = settings.WindowOpacity;
            windowBorderCheckBox.IsChecked = settings.WindowBorder;
            // フォント
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
        /// 背景の不透明度を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            settings.WindowOpacity = (int)e.NewValue;
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
            settings.WindowBorder = true;
            settings.Save();
        }

        private void windowBorderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.WindowBorder = false;
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
        /// 時刻表示のフォーマット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeFormatTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // TextChangedイベントの時だけsettings=nullになることがありアプリ停止してしまうため
            if(settings != null)
            {
                settings.TimeFormat = timeFormatTextBox.Text;
                settings.Save();
            }
        }

        /// <summary>
        /// 音量の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void voiceVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(settings != null)
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
            // フォントファミリーが変更されたらスタイルを1番目に設定
            settings.FontFamily = (FontFamily)fontFamilyListBox.SelectedItem;
            fontStyleListBox.ItemsSource = settings.FontFamily.FamilyTypefaces;
            FamilyTypeface ft = (FamilyTypeface)fontStyleListBox.Items[1];
            fontStyleListBox.SelectedItem = ft.Style;
            Debug.WriteLine("ちぇんじど"+ft.Style);
            settings.FontStyle = ft.Style;
            settings.Save();
        }
        /// <summary>
        /// フォントスタイル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fontStyleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fontStyleListBox.SelectedItem != null)
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
    }
}
