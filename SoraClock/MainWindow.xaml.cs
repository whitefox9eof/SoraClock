using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoraClock
{
    public static class SCTools : Object
    {
        /// <summary>
        /// voiceフォルダの音声を再生する
        /// </summary>
        /// <param name="file"></param>
        public static void playVoice(string file)
        {
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(@"resources\voice\" + file, UriKind.Relative));
            player.Volume = (double)MainSettings.Default.VoiceVolume / 100;
            Debug.WriteLine(player.Volume);
            player.Play();
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

        public MainWindow()
        {
            InitializeComponent();
            // タイマーの初期設定
            timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            timer.Interval = TimeSpan.FromSeconds(1); // 時計は1秒ごとに更新
            timer.Tick += Timer_Tick;
            currentHour = DateTime.Now.Hour;
            timer.Start();
            // ウィンドウの初期設定
            settings = MainSettings.Default;
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
            windowBackgroundColor.Opacity = settings.WindowOpacity == 0 ? 0.01 : settings.WindowOpacity;
            // 時刻初期化
            DateTime time = DateTime.Now;
            clockTextBlock.Text = time.ToString(settings.TimeFormat);

        }
        public void moveTopmost()
        {
            // 一度メインウィンドウを最前面にして戻す
            Topmost = true;
            Topmost = settings.Topmost;
        }

        /// <summary>
        /// 起動時ボイス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            // 再生を遅延させないと音声の頭が再生されない
            await System.Threading.Tasks.Task.Delay(1900);
            DateTime time = DateTime.Now;
            // 起動時のボイス
            if (time.Hour >= 4 && time.Hour <= 10)
            {
                SCTools.playVoice("luanch-0.wav");
            }
            else if (time.Hour >= 11 && time.Hour <= 17)
            {
                SCTools.playVoice("luanch-1.wav");
            }
            else
            {
                SCTools.playVoice("luanch-2.wav");
            }
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
                SCTools.playVoice("time-"+ nowDateTime.Hour + ".wav");
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
            // 終了時にウィンドウの位置を保存
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
            settings.Topmost = true;
            settings.Save();
            Topmost = true;
        }
        /// <summary>
        /// 最前面解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topmostMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.Topmost = false;
            settings.Save();
            Topmost = false;
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
        /// ウィンドウサイズ変更を終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 設定を保存
            settings.Width = Width;
            settings.Height = Height;
            settings.Save();
            Topmost = settings.Topmost;
            // リサイズモード変更時にWidthが小さくなる現象が発生するため再設定する
            ResizeMode = ResizeMode.NoResize;
            Width = settings.Width;
            Height = settings.Height;
        }

    }
}
