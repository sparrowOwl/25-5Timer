using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;




namespace WpfApp_25to5Timer
{
    // 設定
    static class Settings
    {

        public const int SECONDS_PER_ROUND = 60;

        public const int TASK_TIME = 25;
        public const int REST_TIME = 5;
        public static int NowMinutesPerRound { get; set; } = TASK_TIME;

        public const int QUICK_JUDGMENT = 6;
        

        public static bool CanControlMessage { get; set; } = true;


        public static Dictionary<string, string> AnnouncementComment { get; private set; } = new Dictionary<string, string>() {
            { "残り3分" , "あと3分" },
            { "残り1分" , "あと1分!" },
            { "開始待ち" , "タスクが決まったら\n ▶ボタンを\nおしてね" },
            { "開始" , "はじまるよ～" },
            { "停止" , "ストップ" },
            { "一時停止" , "いちじていし" },
            { "終了" , "じかんだよ～" },
            { "過剰クリック" , "はやい！" },
            { "タイマー設定伺い" , "つぎはどうする？" },
            { "休憩終了伺い" , "休憩終わり？" },
            { "休憩開始" , "ごゆっくり～" },

        };

        public static Dictionary<string, string[]> BirdCommentAtClick { get; private set; } = new Dictionary<string, string[]>()
        {
            {"enaga"   ,  new string[]{
                "がんばれー" ,
                "ふぁいとー" ,
                "ぐっどらっく！" ,
                "おいしいごはんが\nまってるよー",
                "もうちょっと\nがんばってー",
                "もっと\nあつくなるんだー！",
                "いっしょに\nがんばろう！"}
            },
            {"sparrow" ,  new string[]{
                "情熱を\nときはなてー！" ,
                "好きこそ、無敵！",
                "熱いたましいを\n見せてー！",
                "勝利を\nつかみ取ってー！",
                "この一戦が\n人生だー！",
                "不撓不屈だー！"} 
            }
        };


        public static string[] BirdNames { get; private set; } = { "enaga", "sparrow" };

        public static double[][] BirdMovePositions { get; private set; } =
        {
            new double[]{ -45 ,  0 },
            new double[]{ -20 ,  5 },
            new double[]{   0 ,  0 },
            new double[]{   30 , -18}
        };

        public static Dictionary<String, System.Windows.Point> BirdRotatePoint { get; private set; } = new Dictionary<String, System.Windows.Point>()
        {
            { "body", new System.Windows.Point(0.5, 0.6)},
            { "reg", new System.Windows.Point(0.5, 0.55)},
            { "tail", new System.Windows.Point(0.5, 0.55)},
            { "head",new System.Windows.Point(0.5, 0.45)}
        };

        public static Dictionary<String, double> BirdCanvasSize { get; private set; } = new Dictionary<String, double> (){
            {"width",180 },
            {"height",180 }
        };

        // 鳥画像のrotateOriginに点を表示するか
        public const bool IS_BIRD_ROTATION_STANDARD_DISPLAY = false;



        // タスクウィンドウにある初期説明文
        public static string[] TaskWindowInitialDescription { get; private set; } = {
            "25分集中、5分休憩で 作業効率UP！",
            "【使い方】",
            "   こ の 説 明 を 消 し 、あなたのタスクを入力して、タイマーをスタート。",
            "   ",
            "【詳しい使い方】",
            "   ① こ の 説 明 を 消 す。",
            "　      ※画面上側の「全削除」をご活用ください",
            "   ② この欄にメモ帳感覚でタスクを入力。",
            "　    25分単位で、タスクを細切れにしてください。",
            "　      ※改行でタスクを区切ります",
            "　      ※先頭のタスクが「現在のタスク」です",
            "   ③ タスクを登録してください",
            "　      ※画面上側「タスクを確定」ボタンを押すか",
            "　      　画面右上の「×」ボタンで画面を閉じると登録",
            "   ④ 円形タイマー左下の「▶」ボタンを押してスタート",
            "   ⑤ まずは25分タスクに集中してください",
            "   　 時間になりましたら、画面中央に鳥が来てお知らせします。",
            "   　 ストレッチ、外の空気を吸う、何でも結構です。",
            "　　  リラックスしてください。",
            "　 ⑥ また25分集中、休憩　を繰り返します。",
            "　 　 ずっと集中しているよりも、効率UPが見込めます。",
            "  ",
            "【シマエナガもいいけれど雀も好きなへ】  ",
            "  　素早く連続で、鳥をクリックすると変身します。",
            "      ※シマエナガ ⇔ 雀",            
        };




        // タスクリスト
        public static List<string> TaskList { get; set; } = new List<string>();

        // タスクを完了した回数
        public static int FinishTaskCount { get; set; } = 0;

        // タスクを完了した回数に対するコメント
        public static String[] CheeringOfCount { get; } =
        {
            "えらい！",
            "すてき！",
            "すごい！",
            "すばらしい！",
            "やったー！"
        };

        // タスクを完了した回数に応じた画像 
        public static String[] CheeringOfCountImage { get;} =
{
            "eggs_1.png",
            "eggs_2.png",
            "eggs_3.png",
            "eggs_4.png",
            "eggs_5.png",
            "eggs_6.png"
        };

        // タスクウィンドウがユーザーによって、非表示でなくCloseにされてしまったかどうか
        public static bool TaskWinClosed { get; set; } = false;




    };

    // タイマー残時間の円形プログレスバー
    class ProgresCircleControl
    {
        private double Progress { get; set; }
        public int Full = 0;
        public int Rest = 0;
        private double Radius { get; set; }
        private double Angle { get; set; }
        private double Size { get; set; }
        private System.Windows.Point StartPoint { get; set; }
        private double Margin { get; set; }
        private ArcSegment Arc { get; set; }
        private TextBlock IntTextBlock { get; set; }
        private PointAnimation PointAnm { get; set; } = new PointAnimation();

        public ProgresCircleControl(
            ArcSegment arc,
            System.Windows.Point parentStartPoint,
            TextBlock tb,
            int full
            )
        {
            Full = full;
            Rest = full;
            Arc = arc;
            Arc.Point = parentStartPoint;
            StartPoint = parentStartPoint;
            Radius = Arc.Size.Width;
            Size = parentStartPoint.X - parentStartPoint.Y;
            Margin = parentStartPoint.Y;
            IntTextBlock = tb;
            PointAnm.Duration = new Duration(TimeSpan.FromSeconds(1));
            PointAnm.RepeatBehavior = new RepeatBehavior(1);
            PointAnm.AutoReverse = false;
        }

        public  static Double ResAngle(double val)
        {
            double result = Math.Floor(val * 100) * 3.6;
            if (result <= 0) { return 0.1; }
            if (result >= 360) { return 360 - 0.1; }
            return result;
        }

        private System.Windows.Point ResCircleEndPoint(Double angle, int radius)
        {
            Double radian = Math.PI * (angle - 90d) / 180;
            Boolean leftSide = angle > 0.5 ? true : false;
            Double x = radius + radius * Math.Cos(radian) + this.Margin * (leftSide ? 1 : 2);
            Double y = radius + radius * Math.Sin(radian) + this.Margin * (leftSide ? 1 : 2); ;
            return new System.Windows.Point(x, y);
        }

        public bool Tick(int reduce)
        {
            Rest -= reduce;
            IntTextBlock.Text = "" + Rest;
            if (Rest < 1)
            {      
                Arc.Point = StartPoint;
                return true;
            }
            else
            {
                if (Rest == 0)
                {
                    Progress = 0;
                }
                else
                {
                    Progress = Rest * 1.0 / Full;
                }

                Arc.IsLargeArc = Progress > 0.5 ? true : false;
                Angle = ResAngle(Progress);
                Arc.Point = ResCircleEndPoint(Angle, (int)this.Radius);
                return false;
            }
        }
    }

    // キャラクターの画像が属するキャンバス
    class CanvasContainer
    {

        public String Name { get; set; } = "";
        public Canvas Canvas { get; set; } = new Canvas();
        public String ParentName { get; set; } = "all";
        public CanvasContainer(String name, String parentName, Double width, Double height, int zindex, System.Windows.Point RenderTransformOrigin, Color color)
        {
            Name = name;
            Canvas.Name = name;
            ParentName = parentName;
            Canvas.Width = width;
            Canvas.Height = height;
            RotateTransform rt = new ();
            Canvas.RenderTransform = rt;
            Canvas.RenderTransformOrigin = RenderTransformOrigin;
            Canvas.SetZIndex(Canvas, zindex);

            if (Settings.IS_BIRD_ROTATION_STANDARD_DISPLAY)
            {
                Ellipse myEllipse = new ();
                myEllipse.Height = 6;
                myEllipse.Width = 6;
                SolidColorBrush mySolidColorBrush = new ();
                mySolidColorBrush.Color = color;
                myEllipse.Fill = mySolidColorBrush;
                myEllipse.StrokeThickness = 1;
                myEllipse.Stroke = Brushes.Black;
                Canvas.SetTop(myEllipse, height * RenderTransformOrigin.Y - 3);
                Canvas.SetLeft(myEllipse, width * RenderTransformOrigin.X - 3);
                Canvas.SetZIndex(myEllipse, zindex + 1);
                this.Canvas.Children.Add(myEllipse);
            }
        }
    }

    // キャラクター
    class Character
    {
        public String CharacterName { get; set; } = Settings.BirdNames[0];
        private double CanvasWidh { get; set; }
        private double CanvasHeight { get; set; }
        public double PositionX { get; set; } = -10;
        public double PositionY { get; set; } = 0;
        public bool IsCanMosyonChange { get; set; } = false;
        private const bool AUTO_REVERSE = true;
        public Canvas AllCanvas { get; set; } = new Canvas();
        public Dictionary<String, CanvasContainer> CanvasDic { get; set; } = new Dictionary<String, CanvasContainer>()
        {
            {"all",new CanvasContainer("all","noParents",Settings.BirdCanvasSize["width"],Settings.BirdCanvasSize["height"],0,new System.Windows.Point(0.5, 0.5),System.Windows.Media.Color.FromArgb(0,0,0,0))}
        };

        public Dictionary<string, Storyboard> StoryboardDic { get; set; } = new Dictionary<string, Storyboard>()
        {
            {"default",new Storyboard() },
        };

        public Dictionary<string, string> StoryboardIsMoveDic { get; set; } = new Dictionary<string, string>()
        {
            {"default","stop"},
        };

        public Dictionary<string, System.Windows.Controls.Image> ImageDic { get; set; } = new Dictionary<string, System.Windows.Controls.Image>();

        public Character(string name, double width, double height)
        {
            CanvasWidh = width;
            CanvasHeight = height;
            CanvasDic["all"].Canvas.Width = width;
            CanvasDic["all"].Canvas.Height = height;
            AllCanvas.Children.Add(CanvasDic["all"].Canvas);
            CharacterName = name;
            SetBirdImg();
            SetBirdAnimation(PositionX, PositionY);
        }

        public void StoryControl(string storyName , string elemName , string move)
        {
            Debug.Print(storyName+"."+elemName+"=>"+move+"が指示されました 現在は"+ StoryboardIsMoveDic[storyName]);
            if (StoryboardIsMoveDic[storyName] == move){ return; }
            switch (move)
            {
                case "start":
                    if (StoryboardIsMoveDic[storyName] == "stop")
                    {
                        StoryboardDic[storyName].Begin(CanvasDic[elemName].Canvas, true);
                        StoryboardIsMoveDic[storyName] = "start";
                    }
                    else
                    {                
                        StoryboardDic[storyName].Resume(CanvasDic[elemName].Canvas);
                        StoryboardIsMoveDic[storyName] = "start";
                    }
                    break;
                case "reStart":
                    StoryboardDic[storyName].Begin(CanvasDic[elemName].Canvas, true);
                    StoryboardIsMoveDic[storyName] = "start";
                    break;

                case "stop":
                    StoryboardDic[storyName].Stop(CanvasDic[elemName].Canvas);
                    StoryboardIsMoveDic[storyName] = "stop";
                    break;
                case "pause":
                    if (StoryboardIsMoveDic[storyName] == "stop"){ return; }
                    StoryboardDic[storyName].Pause(CanvasDic[elemName].Canvas);
                    StoryboardIsMoveDic[storyName] = "pause";
                    break;
                default:
                    MessageBox.Show("対処が決められていない move ！" + move);
                    Debug.Print("対処が決められていない move ！" + move);
                    break;
            }
        }

        public void SetDoubleAnimation(
            string targetName,
            PropertyPath _PropertyPath,
            double from, double to, double duration,
            RepeatBehavior _RepeatBehavior,
            bool autoReverse = AUTO_REVERSE,
            string targetStoryboard = "defo"
         )

        {

            DoubleAnimation anm = new (
                from, to, new Duration(TimeSpan.FromSeconds(duration))
            );

            anm.AutoReverse = autoReverse;  
            anm.RepeatBehavior = _RepeatBehavior;
            Storyboard.SetTargetName(anm, targetName);
            Storyboard.SetTargetProperty(anm, _PropertyPath);

            if (StoryboardDic.ContainsKey(targetStoryboard) == false)
            {

                StoryboardDic.Add(targetStoryboard,new Storyboard());
                StoryboardIsMoveDic.Add(targetStoryboard,"stop");

            }

            StoryboardDic[targetStoryboard].Children.Add(anm);
            StoryboardDic[targetStoryboard].Completed += (sender, e) =>
            {
                StoryboardIsMoveDic[targetStoryboard] = "stop";
            };

        }

        public void ChangeImage(string name)
        {
            CharacterName = name;
            foreach (var item in ImageDic)
            {
                ImageDic[item.Key].Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + CharacterName + "_" + item.Key + ".png"));
            }
        }

        public System.Windows.Controls.Image MakeImageInCanvas(String name, Double width, Double height)
        {
            var Image = new System.Windows.Controls.Image();
            Image.Width = width;
            Image.Height = height;
            Image.Stretch = Stretch.Fill;
            Image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + CharacterName + "_" + name + ".png"));
            return Image;
        }

        public void SetImageToCanvas(String name, String  parentGroupName ,int zindex, System.Windows.Point rotateCener, System.Windows.Media.Color color)
        {
            if (CanvasDic.ContainsKey(parentGroupName) == false && name != parentGroupName)
            {
                Debug.Print("親に指定されたキャンバス ["+ parentGroupName + "] が存在しません。zindexは0、rotateOriginは0.5,0.5で新規作成します ");
                CanvasDic.Add(parentGroupName, new CanvasContainer(parentGroupName, "all", CanvasWidh, CanvasHeight, 0, new System.Windows.Point(0.5, 0.5), System.Windows.Media.Color.FromArgb(255,255,0,0)));
                CanvasDic[parentGroupName].Canvas.Name = parentGroupName;
                CanvasDic["all"].Canvas.Children.Add(CanvasDic[parentGroupName].Canvas);
            }

            CanvasDic.Add(name, new CanvasContainer(name, parentGroupName, CanvasWidh, CanvasHeight, zindex, rotateCener,color));
            CanvasDic[name].Canvas.Name = name;
            if (name != parentGroupName)
            {
               CanvasDic[parentGroupName].Canvas.Children.Add(CanvasDic[name].Canvas);
            }

            var image = MakeImageInCanvas(name, CanvasWidh, CanvasHeight);
            ImageDic.Add(name,image);
            CanvasDic[name].Canvas.Children.Add(ImageDic[name]);

        }

        public void SetBirdImg( )
        {
            SetImageToCanvas("body", "all",   0, Settings.BirdRotatePoint["body"], System.Windows.Media.Color.FromArgb(255, 255, 255, 0)  );
            SetImageToCanvas("reg",  "body", -1, Settings.BirdRotatePoint["reg"],  System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
            SetImageToCanvas("tail", "body", -2, Settings.BirdRotatePoint["tail"], System.Windows.Media.Color.FromArgb(255, 0, 255, 255));
            SetImageToCanvas("head", "body",  1, Settings.BirdRotatePoint["head"], System.Windows.Media.Color.FromArgb(255, 255, 0, 255));

        }

        public void SetBirdAnimation(double _PositionX, double _PositionY)
        {
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                PositionY,_PositionY, 0.5,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                PositionX, _PositionX, 0.25,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.OpacityProperty),
                0.0, 1.0, 0.5,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );

            PositionX = _PositionX;
            PositionY = _PositionY;

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                PositionY, PositionY - 10, 0.2,
                new RepeatBehavior(1),
                true,
                "jump"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                PositionY, PositionY - 10, 0.1,
                new RepeatBehavior(2),
                true,
                "twoJump"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                PositionX, PositionX - 30, 0.4,
                new RepeatBehavior(1),
                false,
                "toLeft"
            );
            Canvas.SetLeft(CanvasDic["all"].Canvas, -15);

            SetDoubleAnimation(
                "head",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                -20, 20, 4,
                RepeatBehavior.Forever,
                true
            );

            SetDoubleAnimation(
                "tail",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                30, -30, 4,
                RepeatBehavior.Forever,
                true
            );

            SetDoubleAnimation(
                "body",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                10, -10, 6,
                RepeatBehavior.Forever,
                true
            );
        }
    }

    // 吹き出しアニメーション
    class HukidasiAnimation
    {

        public Storyboard StoryboardFeedIn { get; set; } = new Storyboard();
        public Storyboard StoryboardFeedOut { get; set; } = new Storyboard();
        public HukidasiAnimation()
        {
            Storyboard.SetTargetName(feedIn, "messageBack");
            Storyboard.SetTargetName(feedOut, "messageBack");
            Storyboard.SetTargetProperty(feedIn, new PropertyPath(Control.OpacityProperty));
            Storyboard.SetTargetProperty(feedOut, new PropertyPath(Control.OpacityProperty));
            StoryboardFeedIn.Children.Add(feedIn);
            StoryboardFeedOut.Children.Add(feedOut);

        }


        private readonly DoubleAnimation feedIn = new()
        {
            From = 0.0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(2)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        private readonly DoubleAnimation feedOut = new ()
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromSeconds(5)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        public void Stop()
        {
             StoryboardFeedOut.Stop();
             StoryboardFeedIn.Stop();
        }
    }





    public partial class MainWindow : Window
    {
        private Character bird;
        private HukidasiAnimation HukidasiAnimation = new ();

        private DispatcherTimer mainTimer = new ();
        private DispatcherTimer timerDetermineQuickClick = new ();
        private int quickClickCount = 0;

        private ProgresCircleControl secondsProgressCircle;
        private ProgresCircleControl minutesProgressCircle;


        public Window1 taskWin;





        public MainWindow()
        {


            InitializeComponent();


            // MainWindowのどこをドラッグしてもウィンドウを移動できるようにする
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            // 画面右下へ移動
            this.Top = SystemParameters.WorkArea.Height - this.MaxHeight;
            this.Left = SystemParameters.WorkArea.Width - this.Width;





            // タスクウィンドウの表示
            taskWin = new Window1();
            taskWin.Show();

            // 鳥の設定
            bird = new Character(Settings.BirdNames[0], Settings.BirdCanvasSize["width"], Settings.BirdCanvasSize["height"]);
            canvas_bird.Children.Add(bird.AllCanvas);
            foreach (var item in bird.CanvasDic)
            {
                RegisterName(item.Key, bird.CanvasDic[item.Key].Canvas);
            }
            StartSwing();

            bird.StoryboardDic["jumpToNewPosition"].Completed += (sender, e) =>
            {
                StartSwing();
            };


            // タイマーの設定
            secondsProgressCircle = new ProgresCircleControl(timePath_S, timePath_S_figure.StartPoint, showTime_S, Settings.SECONDS_PER_ROUND);
            minutesProgressCircle = new ProgresCircleControl(timePath_M, timePath_M_figure.StartPoint, showTime_M, Settings.TASK_TIME);



            TimeSet(Settings.TASK_TIME);
            MessageControl(true, Settings.AnnouncementComment["開始待ち"]);




            mainTimer.Interval = new TimeSpan(0, 0, 1);
            mainTimer.Tick += (sender, e) =>
            {
                if (secondsProgressCircle.Tick(1))
                {
                    minutesProgressCircle.Tick(1);
                    if (minutesProgressCircle.Rest > 0)
                    {
                        secondsProgressCircle.Rest = Settings.SECONDS_PER_ROUND-1;
                        secondsProgressCircle.Tick(0);
                        if (minutesProgressCircle.Rest == 2)
                        {
                            MessageControl(false, Settings.AnnouncementComment["残り3分"]);
                        }
                        else if(minutesProgressCircle.Rest == 0)
                        {
                            MessageControl(false, Settings.AnnouncementComment["残り1分"]);
                        }

                    }
                    else
                    {
                        mainTimer.Stop();


                        if (Settings.TaskList.Count > 0 && Settings.NowMinutesPerRound != Settings.REST_TIME)
                        {
                            AskTaskFinished();
                        }
                        else
                        {                            
                            AskNextTime();
                        }

                        this.Top = SystemParameters.WorkArea.Height / 2 - this.Height / 2;
                        this.Left = SystemParameters.WorkArea.Width / 2 - this.Width / 2 - 110; // この110は、タスクウィンドウに被ってしまう事への対策
                    }
                }
            };







            timerDetermineQuickClick.Interval = new TimeSpan(0, 0, 1);
            timerDetermineQuickClick.Tick += (sender, e) =>
            {
                if (quickClickCount > Settings.QUICK_JUDGMENT)
                {
                    Debug.Print("素早いクリック！");
                    quickClickCount = 0;
                    bird.IsCanMosyonChange = false;
                    Random r1 = new ();
                    int index = r1.Next(0, Settings.BirdMovePositions.Length);
                    string[] d = Array.Empty<string>();

                    if (Settings.BirdNames.Length <2) {
                        MessageBox.Show("エラー！鳥の名前は2つ以上登録してください！");
                        return;
                    }

                    int birdIndex = 0;
                    while (true)
                    {
                        birdIndex = new System.Random().Next(0, Settings.BirdNames.Length);
                        if (bird.CharacterName != Settings.BirdNames[birdIndex])
                        {
                            break;
                        }
                    }

                    bird.ChangeImage(Settings.BirdNames[birdIndex]);                
                    bird.SetBirdAnimation(Settings.BirdMovePositions[index][0], Settings.BirdMovePositions[index][1]);
                    bird.StoryControl("jumpToNewPosition", "all", "reStart");

                }
                else
                {
                    quickClickCount-=3;
                    if (quickClickCount <0)
                    {
                        timerDetermineQuickClick.Stop();
                        Debug.Print("素早いクリック判定停止！ 現在 " + quickClickCount);
                    }
                }
            };

            this.Closing += (e, s) =>
            {
                mainTimer.Stop();
                timerDetermineQuickClick.Stop();
                taskWin.Close();
            };
        }

        private void TimeSet(int minute)
        {
            secondsProgressCircle.Rest = 0;
            minutesProgressCircle.Rest = minute;
            minutesProgressCircle.Full = minute;
            Settings.NowMinutesPerRound = minute;
            secondsProgressCircle.Tick(0);
            minutesProgressCircle.Tick(0);


            timeAskGrid.Visibility = Visibility.Collapsed;
        }

        private void TimerStart()
        {
            Settings.CanControlMessage = true;

            restAskGrid.Visibility = Visibility.Collapsed;

            if (Settings.TaskWinClosed) { taskWin = new Window1(); }
            taskWin.Hide();


            if (Settings.NowMinutesPerRound == Settings.REST_TIME)
            {
                MessageControl(true, Settings.AnnouncementComment["休憩開始"]);
            }
            else if (Settings.TaskList.Count > 0)
            {
                MessageControl(false,Settings.TaskList[0] + "\n"+　 Settings.AnnouncementComment["開始"]);
            }
            else
            {
                MessageControl(false, Settings.AnnouncementComment["開始"]);
            }

            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;

            if (Settings.TaskWinClosed) { taskWin = new Window1(); }


            if (secondsProgressCircle.Rest <= 0 && minutesProgressCircle.Rest <= 0)
            {
                TimeSet(Settings.NowMinutesPerRound);
            }
            mainTimer.Start();
        }




        private void MessageControl(bool show, string text)
        {
            if (Settings.CanControlMessage == false) { Debug.Print("弾かれました　"+text); return; }
            messageText.Text = text;
            HukidasiAnimation.Stop();
            if (show)
            {
                HukidasiAnimation.StoryboardFeedIn.Begin(messageBack, true);       
                messageBack.Visibility = Visibility.Visible;               
            }
            else
            {
                HukidasiAnimation.StoryboardFeedOut.Begin(messageBack, true);
            }
        }


        private void AskTaskFinished()
        {
            taskAskGrid.Visibility = Visibility.Visible;
            timeAskGrid.Visibility = Visibility.Collapsed;


            MessageControl(true, Settings.AnnouncementComment["終了"]);

            if (Settings.TaskWinClosed) { taskWin = new Window1(); }
            taskWin.Show();
            taskWin.topTaskRemoveButton.Visibility = Visibility.Collapsed;

            Settings.CanControlMessage = false;
        }


        private void AskNextTime()
        {
            taskAskGrid.Visibility = Visibility.Collapsed;
            taskWin.topTaskRemoveButton.Visibility = Visibility.Visible;


            if (Settings.NowMinutesPerRound == Settings.REST_TIME)
            {
                MessageControl(true, Settings.AnnouncementComment["終了"] + "\n" + Settings.AnnouncementComment["休憩終了伺い"]);
                restAskGrid.Visibility = Visibility.Visible;

            }
            else
            {
                MessageControl(true,  Settings.AnnouncementComment["タイマー設定伺い"]);
                timeAskGrid.Visibility = Visibility.Visible;

            }


            Settings.CanControlMessage = false;
        }



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TimerStart();

            bird.StoryControl("defo", "all", "pause");
            bird.StoryControl("twoJump", "all", "start");


            bird.StoryboardDic["twoJump"].Completed += (sender, e) =>
            {
                StartSwing();

            };
        }

        private void Set25min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(Settings.TASK_TIME);
        }

        private void Set5min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(Settings.REST_TIME);
            
        }

        private void Set25minAndStart_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(Settings.TASK_TIME);
            StartButton_Click(sender, e);
        }

        private void Set5minAndStart_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(Settings.REST_TIME);
            StartButton_Click(sender, e);

        }


        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            mainTimer.Stop();
            bird.StoryControl("defo", "all", "pause");
            MessageControl(true, Settings.AnnouncementComment["一時停止"]);
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            mainTimer.Stop();
            bird.StoryControl("defo", "all", "pause");
            MessageControl(true, Settings.AnnouncementComment["停止"]);
            TimeSet(Settings.NowMinutesPerRound);
        }

        private void StartSwing()
        {
            Debug.Print("再スタート");
            bird.StoryControl("defo", "all", "start");
            bird.IsCanMosyonChange = true;
        }

        private void Canvas_bird_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bird.StoryControl("jump", "all", "start");
            MessageControl(false, Settings.BirdCommentAtClick[bird.CharacterName][(new System.Random()).Next(0, Settings.BirdCommentAtClick[bird.CharacterName].Length)]);
            if (quickClickCount<=0)
            {
                timerDetermineQuickClick.Start();
            }
            quickClickCount++;
            Debug.Print("clicked 現在" + (quickClickCount + 1));
        }

        private void TaskWindowShowButton_Click(object sender, RoutedEventArgs e)
        {

            if (Settings.TaskWinClosed) { taskWin = new Window1(); }
            taskWin.Show();

        }







        private void TaskIncomplete(object sender, RoutedEventArgs e)
        {
            Settings.CanControlMessage = true;
            AskNextTime();
        }

        private void TopTaskRemove(object sender, RoutedEventArgs e)
        {
            Settings.CanControlMessage = true;
            AskNextTime();
            taskWin.TopTaskRemove();
        }
        private void MoreRest_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.CanControlMessage = true;
            TimerStart();
        }
    }
}
