using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp_25to5Timer
{
    static class Settings
    {
        // 鳥画像のrotateOriginに点を表示するか
        public const bool OriginShow = false;
        public static Dictionary<string, string> Comment = new Dictionary<string, string>() {
            { "残り5分" , "あと5分" },
            { "残り1分" , "あと1分!" },
            { "開始" , "はじまるよ～" },
            { "停止" , "ストップ" },
            { "一時停止" , "いちじていし" },
            { "終了" , "じかんだよ～" }
        };

        public static Dictionary<string, string[]> RandomComment = new Dictionary<string, string[]>()
        {
            {"enaga"   ,  new string[]{
                "がんばれー" ,
                "ふぁいとー" ,
                "ぐっどらっく！" ,
                "おいしいごはんがまってるよー",
                "もうちょっとがんばってー",
                "もっとあつくなるんだー！",
                "いっしょにがんばろう！"}
            },
            {"sparrow" ,  new string[]{
                "情熱をときはなてー！" ,
                "好きこそ、無敵！",
                "熱いたましいを見せてー！",
                "勝利をつかみ取ってー！",
                "この一戦が人生だー！",
                "不撓不屈だー！"} 
            }
        };

        public static int TimeUnit = 60;
        public static int M_defo = 25;
        public static int QuickJudgment = 6;
        public static string[] BirdNames = { "enaga", "sparrow" };

        public static double[][] NewXYList =
        {
            new double[]{ -45 ,  0 },
            new double[]{ -20 ,  5 },
            new double[]{   0 ,  0 },
            new double[]{   0 , 55 }
        };

        public static Dictionary<String, System.Windows.Point> BirdRotatePoint = new Dictionary<String, System.Windows.Point>()
        {
            { "body", new System.Windows.Point(0.5, 0.6)},
            { "reg", new System.Windows.Point(0.5, 0.55)},
            { "tail", new System.Windows.Point(0.5, 0.55)},
            { "head",new System.Windows.Point(0.5, 0.45)}
        };

        public static Dictionary<String, double> BirdCanvasSize = new Dictionary<String, double> (){
            {"width",180 },
            {"height",180 }
        };
        
    };

    class ProgresCircleControl
    {
        private double Progress;
        public int Full = 0;
        public int Rest = 0;
        private double Radius;
        private double Angle;
        private double Size;
        private System.Windows.Point StartPoint;
        private double Margin;
        private ArcSegment Arc;
        private TextBlock textBlock;
        private PointAnimation pAnm = new PointAnimation();

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
            textBlock = tb;
            pAnm.Duration = new Duration(TimeSpan.FromSeconds(1));
            pAnm.RepeatBehavior = new RepeatBehavior(1);
            pAnm.AutoReverse = false;
        }

        public Double ResAngle(double val)
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
            textBlock.Text = "" + Rest;
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


    class CanvasContainer
    {

        public String Name = "";
        public Canvas Canvas = new Canvas();
        public String ParentName = "all";
        public CanvasContainer(String name, String parentName, Double width, Double height, int zindex, System.Windows.Point RenderTransformOrigin, System.Windows.Media.Color color)
        {
            Name = name;
            Canvas.Name = name;
            ParentName = parentName;
            Canvas.Width = width;
            Canvas.Height = height;
            RotateTransform rt = new RotateTransform();
            Canvas.RenderTransform = rt;
            Canvas.RenderTransformOrigin = RenderTransformOrigin;
            Canvas.SetZIndex(Canvas, zindex);

            if (Settings.OriginShow)
            {
                Ellipse myEllipse = new Ellipse();
                myEllipse.Height = 6;
                myEllipse.Width = 6;
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
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

    class Character
    {
        public String characterName = Settings.BirdNames[0];
        private double canvasWidh;
        private double canvasHeight;
        public double positionX = -10;
        public double positionY = 0;
        public bool isCanMosyonChange= false;
        private const bool _autoReverse = true;
        public Canvas AllCanvas = new Canvas();
        public Dictionary<String, CanvasContainer> canvasDic = new Dictionary<String, CanvasContainer>()
        {
            {"all",new CanvasContainer("all","noParents",Settings.BirdCanvasSize["width"],Settings.BirdCanvasSize["height"],0,new System.Windows.Point(0.5, 0.5),System.Windows.Media.Color.FromArgb(0,0,0,0))}
        };

        public Dictionary<string, Storyboard> StoryboardDic = new Dictionary<string, Storyboard>()
        {
            {"default",new Storyboard() },
        };

        public Dictionary<string, string> StoryboardIsMoveDic = new Dictionary<string, string>()
        {
            {"default","stop"},
        };

        public Dictionary<string, System.Windows.Controls.Image> imageDic = new Dictionary<string, System.Windows.Controls.Image>();

        public Character(string name, double width, double height)
        {
            canvasWidh = width;
            canvasHeight = height;
            canvasDic["all"].Canvas.Width = width;
            canvasDic["all"].Canvas.Height = height;
            AllCanvas.Children.Add(canvasDic["all"].Canvas);
            characterName = name;
            SetBirdImg(width, height);
            setBirdAnimation(positionX, positionY);
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
                        StoryboardDic[storyName].Begin(canvasDic[elemName].Canvas, true);
                        StoryboardIsMoveDic[storyName] = "start";
                    }
                    else
                    {                
                        StoryboardDic[storyName].Resume(canvasDic[elemName].Canvas);
                        StoryboardIsMoveDic[storyName] = "start";
                    }
                    break;
                case "reStart":
                    StoryboardDic[storyName].Begin(canvasDic[elemName].Canvas, true);
                    StoryboardIsMoveDic[storyName] = "start";
                    break;

                case "stop":
                    StoryboardDic[storyName].Stop(canvasDic[elemName].Canvas);
                    StoryboardIsMoveDic[storyName] = "stop";
                    break;
                case "pause":
                    if (StoryboardIsMoveDic[storyName] == "stop"){ return; }
                    StoryboardDic[storyName].Pause(canvasDic[elemName].Canvas);
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
            bool autoReverse = _autoReverse,
            string targetStoryboard = "defo"
         )

        {
            DoubleAnimation anm = new DoubleAnimation(
                from, to, new Duration(TimeSpan.FromSeconds(duration))
            );

            anm.AutoReverse = autoReverse;  
            anm.RepeatBehavior = _RepeatBehavior;
            Storyboard.SetTargetName(anm, targetName);
            Storyboard.SetTargetProperty(anm, _PropertyPath);

            if (StoryboardDic.ContainsKey(targetStoryboard) == false)
            {
                StoryboardDic.Add(targetStoryboard,new Storyboard());
                Debug.Print("newStoryboard! : "+targetStoryboard);
                StoryboardIsMoveDic.Add(targetStoryboard,"stop");
            }
            StoryboardDic[targetStoryboard].Children.Add(anm);
            StoryboardDic[targetStoryboard].Completed += (sender, e) =>
            {
                StoryboardIsMoveDic[targetStoryboard] = "stop";
            };
        }

        public void changeImage(string name)
        {
            characterName = name;
            foreach (var item in imageDic)
            {
                imageDic[item.Key].Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + characterName + "_" + item.Key + ".png"));
            }
        }

        public System.Windows.Controls.Image MakeImageInCanvas(String name, Double width, Double height)
        {
            var Image = new System.Windows.Controls.Image();
            Image.Width = width;
            Image.Height = height;
            Image.Stretch = Stretch.Fill;
            Image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + characterName + "_" + name + ".png"));
            return Image;
        }

        public void SetImageToCanvas(String name, String  parentGroupName ,Double width, Double height,int zindex, System.Windows.Point rotateCener, System.Windows.Media.Color color)
        {
            if (canvasDic.ContainsKey(parentGroupName) == false && name != parentGroupName)
            {
                Debug.Print("親に指定されたキャンバス ["+ parentGroupName + "] が存在しません。zindexは0、rotateOriginは0.5,0.5で新規作成します ");
                canvasDic.Add(parentGroupName, new CanvasContainer(parentGroupName, "all", canvasWidh, canvasHeight, 0, new System.Windows.Point(0.5, 0.5), System.Windows.Media.Color.FromArgb(255,255,0,0)));
                canvasDic[parentGroupName].Canvas.Name = parentGroupName;
                canvasDic["all"].Canvas.Children.Add(canvasDic[parentGroupName].Canvas);
            }

            canvasDic.Add(name, new CanvasContainer(name, parentGroupName, canvasWidh, canvasHeight, zindex, rotateCener,color));
            canvasDic[name].Canvas.Name = name;
            if (name != parentGroupName)
            {
               canvasDic[parentGroupName].Canvas.Children.Add(canvasDic[name].Canvas);
            }

            var image = MakeImageInCanvas(name, canvasWidh, canvasHeight);
            imageDic.Add(name,image);
            canvasDic[name].Canvas.Children.Add(imageDic[name]);

        }

        public void SetBirdImg( double width, double height)
        {
            SetImageToCanvas("body", "all", width, height,   0, Settings.BirdRotatePoint["body"], System.Windows.Media.Color.FromArgb(255, 255, 255, 0)  );
            SetImageToCanvas("reg",  "body", width, height, -1, Settings.BirdRotatePoint["reg"],  System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
            SetImageToCanvas("tail", "body", width, height, -2, Settings.BirdRotatePoint["tail"], System.Windows.Media.Color.FromArgb(255, 0, 255, 255));
            SetImageToCanvas("head", "body", width, height,  1, Settings.BirdRotatePoint["head"], System.Windows.Media.Color.FromArgb(255, 255, 0, 255));

        }

        public void setBirdAnimation(double _positionX, double _positionY)
        {
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY,_positionY, 0.5,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                positionX, _positionX, 0.25,
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

            positionX = _positionX;
            positionY = _positionY;

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY, positionY - 10, 0.2,
                new RepeatBehavior(1),
                true,
                "jump"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY, positionY - 10, 0.1,
                new RepeatBehavior(2),
                true,
                "twoJump"
            );

            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                positionX, positionX - 30, 0.4,
                new RepeatBehavior(1),
                false,
                "toLeft"
            );
            Canvas.SetLeft(canvasDic["all"].Canvas, -15);

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

    class hukidasiAnimation
    {

        public Storyboard _StoryboardFeedIn = new Storyboard();
        public Storyboard _StoryboardFeedOut = new Storyboard();
        public hukidasiAnimation()
        {
            Storyboard.SetTargetName(feedIn, "messageBack");
            Storyboard.SetTargetName(feedOut, "messageBack");
            Storyboard.SetTargetProperty(feedIn, new PropertyPath(Control.OpacityProperty));
            Storyboard.SetTargetProperty(feedOut, new PropertyPath(Control.OpacityProperty));
            _StoryboardFeedIn.Children.Add(feedIn);
            _StoryboardFeedOut.Children.Add(feedOut);

        }


        private DoubleAnimation feedIn = new DoubleAnimation()
        {
            From = 0.0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(2)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        private DoubleAnimation feedOut = new DoubleAnimation()
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromSeconds(5)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        public void Stop()
        {
             _StoryboardFeedOut.Stop();
             _StoryboardFeedIn.Stop();
        }
    }
    public partial class MainWindow : Window
    {
        private Character bird;
        private hukidasiAnimation hukidasiAnimation = new hukidasiAnimation();
        private DispatcherTimer _timer = new DispatcherTimer();
        private DispatcherTimer tooClick_timer = new DispatcherTimer();
        private int tooClickCount = 0;
        private ProgresCircleControl circle_S;
        private ProgresCircleControl circle_M;
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            bird = new Character(Settings.BirdNames[0], Settings.BirdCanvasSize["width"], Settings.BirdCanvasSize["height"]);
            canvas_bird.Children.Add(bird.AllCanvas);
            foreach (var item in bird.canvasDic)
            {
                RegisterName(item.Key, bird.canvasDic[item.Key].Canvas);
            }
            startSwing();

            bird.StoryboardDic["jumpToNewPosition"].Completed += (sender, e) =>
            {
                startSwing();
            };

            circle_S = new ProgresCircleControl(timePath_S, timePath_S_figure.StartPoint, showTime_S, Settings.TimeUnit);
            circle_M = new ProgresCircleControl(timePath_M, timePath_M_figure.StartPoint, showTime_M, Settings.M_defo);

            TimeSet(Settings.M_defo);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += (sender, e) =>
            {
                if (circle_S.Tick(1))
                {
                    circle_M.Tick(1);
                    if (circle_M.Rest > 0)
                    {
                        circle_S.Rest = Settings.TimeUnit;
                        circle_S.Tick(0);
                        if (circle_M.Rest == 4)
                        {
                            messageControl(false, Settings.Comment["残り5分"]);
                        }
                        else if(circle_M.Rest == 0)
                        {
                            messageControl(false, Settings.Comment["残り1分"]);
                        }

                    }
                    else
                    {
                            _timer.Stop();
                            messageControl(true, Settings.Comment["終了"]);
                            Debug.Print("残時間"+ circle_M.Rest+":"+ circle_S.Rest);
                            this.Top = SystemParameters.WorkArea.Height / 2 - this.Height / 2;
                            this.Left = SystemParameters.WorkArea.Width / 2 - this.Width / 2;
                    }
                }
            };


            TimerStart();
            tooClick_timer.Interval = new TimeSpan(0, 0, 1);
            tooClick_timer.Tick += (sender, e) =>
            {
                if (tooClickCount > Settings.QuickJudgment)
                {
                    Debug.Print("素早いクリック！");
                    tooClickCount = 0;
                    bird.isCanMosyonChange = false;
                    Random r1 = new System.Random();
                    int index = r1.Next(0, Settings.NewXYList.Length);
                    string[] d = { };

                    if (Settings.BirdNames.Length <2) {
                        MessageBox.Show("エラー！鳥の名前は2つ以上登録してください！");
                        return;
                    }

                    int birdIndex = 0;
                    while (true)
                    {
                        birdIndex = new System.Random().Next(0, Settings.BirdNames.Length);
                        if (bird.characterName != Settings.BirdNames[birdIndex])
                        {
                            break;
                        }
                    }

                    Debug.Print(" birdIndex"+ birdIndex);
                    bird.changeImage(Settings.BirdNames[birdIndex]);                
                    bird.setBirdAnimation(Settings.NewXYList[index][0], Settings.NewXYList[index][1]);
                    bird.StoryControl("jumpToNewPosition", "all", "reStart");

                }
                else
                {
                    tooClickCount-=3;
                    if (tooClickCount <0)
                    {
                        tooClick_timer.Stop();
                        Debug.Print("素早いクリック判定停止！ 現在 " + tooClickCount);
                    }
                }
            };

            this.Closing += (e, s) =>
            {
                _timer.Stop();
                tooClick_timer.Stop();
            };
        }

        private void TimeSet(int minute)
        {
            circle_S.Rest = 0;
            circle_M.Rest = minute;
            circle_M.Full = minute;
            Settings.M_defo = minute;
            circle_S.Tick(0);
            circle_M.Tick(0);
        }

        private void TimerStart()
        {
            messageControl(false, Settings.Comment["開始"]);
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            if (circle_S.Rest <= 0 && circle_M.Rest <= 0)
            {
                TimeSet(Settings.M_defo);
            }
            _timer.Start();
        }

        private void messageControl(bool show, string text)
        {
            messageText.Text = text;
            hukidasiAnimation.Stop();
            if (show)
            {
                hukidasiAnimation._StoryboardFeedIn.Begin(messageBack, true);       
                messageBack.Visibility = Visibility.Visible;               
            }
            else
            {
                hukidasiAnimation._StoryboardFeedOut.Begin(messageBack, true);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TimerStart();
            bird.StoryControl("defo", "all", "pause");
            bird.StoryControl("twoJump", "all", "start");
            messageControl(false, Settings.Comment["開始"]);
            bird.StoryboardDic["twoJump"].Completed += (sender, e) =>
            {                
                startSwing();
                bird.StoryboardDic["twoJump"].Completed -= (sender, e) =>
                {
                    startSwing();
                };
            };
        }

        private void Set25min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(25);
        }

        private void Set5min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(5);
        }

        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            bird.StoryControl("defo", "all", "pause");
            messageControl(true, Settings.Comment["一時停止"]);
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            bird.StoryControl("defo", "all", "pause");
            messageControl(true, Settings.Comment["停止"]);
            TimeSet(Settings.M_defo);
        }

        private void startSwing()
        {
            Debug.Print("再スタート");
            bird.StoryControl("defo", "all", "start");
            bird.isCanMosyonChange = true;
        }

        private void canvas_bird_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bird.StoryControl("jump", "all", "start");
            messageControl(false, Settings.RandomComment[bird.characterName][(new System.Random()).Next(0, Settings.RandomComment[bird.characterName].Length)]);
            if (tooClickCount<=0)
            {
                tooClick_timer.Start();
            }
            tooClickCount++;
            Debug.Print("clicked 現在" + (tooClickCount + 1));
        }
    }
}
