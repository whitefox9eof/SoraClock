using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoraClock
{
    public static class SCTools : Object
    {
        public static SolidColorBrush stringToSolidColorBrush(string colorCode)
        {
            Color color = (Color)ColorConverter.ConvertFromString(colorCode);;
            return new SolidColorBrush(color);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainSettings settings;
        private DispatcherTimer timer;
        private int currentHour;
        private SoundPlayer player;

        public MainWindow()
        {
            InitializeComponent();
            // タイマーの初期設定
            timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            currentHour = DateTime.Now.Hour;
            timer.Start();
            // ウィンドウの初期設定
            settings = MainSettings.Default;
            Topmost = settings.Topmost;
            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + Width) < SystemParameters.VirtualScreenWidth)
            { Left = settings.WindowLeft; }
            else
            { Left = (SystemParameters.VirtualScreenWidth - Width) / 2; }
            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + Height) < SystemParameters.VirtualScreenHeight)
            { Top = settings.WindowTop; }
            else
            { Top = (SystemParameters.VirtualScreenHeight - Height) / 2; }
            // 色・不透明度
            windowBackgroundColor.Color = (Color)ColorConverter.ConvertFromString(settings.ClockBackgroundColor);
            windowBackgroundColor.Opacity = (double)settings.WindowOpacity / 100;
            // 枠線の初期化
            SolidColorBrush scb = SCTools.stringToSolidColorBrush(settings.ClockForegroundColor);
            outlineBorder.BorderBrush = (Brush)scb;
            inlineBorder.BorderBrush = outlineBorder.BorderBrush;
            // 時刻文字色の初期化
            clockTextBlock.Foreground = scb;
            clockTextBlock.Text = DateTime.Now.ToString(settings.TimeFormat);
        }

        /// <summary>
        /// 時刻更新イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime nowDateTime = DateTime.Now;
            clockTextBlock.Text = nowDateTime.ToString(settings.TimeFormat);
            if(nowDateTime.Hour != currentHour)
            {
                player = new SoundPlayer("resources/voice/time-" + nowDateTime.Hour + ".wav");
                player.Play();
                currentHour = nowDateTime.Hour;
            }
        }

        /// <summary>
        /// コンテキストメニュー終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 終了前にウィンドウ位置を保存
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.Save();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 最前面表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topmostMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            settings.Topmost = true;
            settings.Save();
        }
        /// <summary>
        /// 最前面解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topmostMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            settings.Topmost = false;
            settings.Save();
        }

        /// <summary>
        /// ドラッグでウィンドウ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        /// <summary>
        /// 設定画面表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow window = new SettingWindow();
            window.settingEvent += new SettingWindow.SettingEventHandler(SettingWindow_EventHandler);
            window.Closed += Window_Closed;
            window.Show();
           settingsMenuItem.IsEnabled = false;
        }
        /// <summary>
        /// 設定画面を閉じたらコンテキストメニューの[設定]を有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            settingsMenuItem.IsEnabled = true;
        }


        /// <summary>
        /// 設定画面での変更を反映させるためのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingWindow_EventHandler(object sender, SettingEventArgs e)
        {
            if (e.opacity >= 0)
            {
                windowBackgroundColor.Opacity = (double)e.opacity / 100;
            }
            if (e.clockBackgroundColor != null)
            {
                windowBackgroundColor.Color = (Color)e.clockBackgroundColor;
            }
            if(e.clockForegroundColor != null)
            {
                clockTextBlock.Foreground = new SolidColorBrush((Color)e.clockForegroundColor);
            }
            if(e.timeFormat != null)
            {
                clockTextBlock.Text = DateTime.Now.ToString(e.timeFormat);
            }
        }
    }
}
