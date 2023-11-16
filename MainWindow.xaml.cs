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
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●基本的な設定をまとめたクラス　●●●●●●●●●●●●●●●●●●●●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    static class Settings
    {
        // 鳥画像のrotateOriginに点を表示するか
        public const bool OriginShow = false;

        // 鳥の各コメント 各鳥共通
        public static Dictionary<string, string> Comment = new Dictionary<string, string>() {
            { "残り5分" , "あと5分" },
            { "残り1分" , "あと1分!" },
            { "開始" , "はじまるよ～" },
            { "停止" , "ストップ" },
            { "一時停止" , "いちじていし" },
            { "終了" , "じかんだよ～" },
            { "過剰クリック" , "はやい！" },
        };

        // ランダムコメント
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



        // タイマーの時間単位(1周何秒かけるか)
        public static int TimeUnit = 60;
        // タイマーの初期設定(何週か)
        public static int M_defo = 25;

        // 素早いクリック判定回数
        public static int QuickJudgment = 6;

        // 鳥の名前リスト　必ず2つ以上登録
        public static string[] BirdNames = { "enaga", "sparrow" };


        // 鳥位置移動先座標リスト      
        public static double[][] NewXYList =
        {
            new double[]{ -45 ,  0 },
            new double[]{ -20 ,  5 },
            new double[]{   0 ,  0 },
            new double[]{   0 , 55 }
        };

        // 鳥の回転基準
        public static Dictionary<String, System.Windows.Point> BirdRotatePoint = new Dictionary<String, System.Windows.Point>()
        {
            { "body", new System.Windows.Point(0.5, 0.6)},
            { "reg", new System.Windows.Point(0.5, 0.55)},
            { "tail", new System.Windows.Point(0.5, 0.55)},
            { "head",new System.Windows.Point(0.5, 0.45)}
        };

        // 鳥画像の幅と高さ
        public static Dictionary<String, double> BirdCanvasSize = new Dictionary<String, double> (){
            {"width",180 },
            {"height",180 }
        };
        
    };

    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●タイマーの周囲にある円弧についてのクラス　　　　　　　　　　　　　　●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    class ProgresCircleControl
    {

        // 進捗・達成のﾊﾟｰｾﾝﾄ
        private double Progress { get; set; }

        // 設定された時間
        public int Full = 0;

        // 残時間
        public int Rest = 0;

        // 円の半径
        private double Radius { get; set; }

        // 円の角度
        private double Angle { get; set; }

        // 円のサイズ
        private double Size { get; set; }

        // 円の初期スタート位置
        private System.Windows.Point StartPoint { get; set; }

        // 親要素に対する円のMargin(正方形のCanvas、Top)
        private double Margin { get; set; }

        // 円弧のセグメント　ArcSegment
        private ArcSegment Arc;

        // 残時間の表示先TextBlock
        private TextBlock textBlock;

        // pointAnimation 基礎
        private PointAnimation pAnm = new PointAnimation();


        // コンストラクタ―　-----------------------------------------
        public ProgresCircleControl(
            ArcSegment arc,
            System.Windows.Point parentStartPoint,
            TextBlock tb,
            int full
            )
        {

            // 設定された時間
            Full = full;

            // 残時間
            Rest = full;

            // 円弧そのもの
            Arc = arc;

            // 円弧の右端座標
            Arc.Point = parentStartPoint;

            // 円弧の左端座標
            StartPoint = parentStartPoint;


            // 円弧の半径
            Radius = Arc.Size.Width;

            // 円弧のサイズ
            Size = parentStartPoint.X - parentStartPoint.Y;



            // 親要素に対する円のMargin(正方形のCanvas、Top)
            Margin = parentStartPoint.Y;

            // 残時間の表示先TextBlock
            textBlock = tb;

            // アニメーション所要時間 = 1秒
            pAnm.Duration = new Duration(TimeSpan.FromSeconds(1));

            // アニメーションの繰り返し設定　= 1回だけ
            pAnm.RepeatBehavior = new RepeatBehavior(1);

            // アニメーションの繰り返し再生
            pAnm.AutoReverse = false;

        }


        // 残時間%を角度(Max360)へ変換する　----------------------------------------
        public Double ResAngle(double val)
        {
            // %を角度に直した数字 result
            double result = Math.Floor(val * 100) * 3.6;

            // もし result が0以下ならば、0だと表示がおかしくなるので0.1をreturn 
            if (result <= 0) { return 0.1; }

            // もし result が360以上ならば、360以上だと表示がおかしくなるので360ぎりぎりをreturn 
            if (result >= 360) { return 360 - 0.1; }

            return result;
        }

        // 円弧終了地点の座標を計算する　--------------------------------------------
        private System.Windows.Point ResCircleEndPoint(Double angle, int radius)
        {
            // 円中心の角度（中心角θ）
            Double radian = Math.PI * (angle - 90d) / 180;

            // 円弧が左側にまで及ぶか
            Boolean leftSide = angle > 0.5 ? true : false;

            // 円弧終了地点 X の座標
            Double x = radius + radius * Math.Cos(radian) + this.Margin * (leftSide ? 1 : 2);

            // 円弧終了地点 Y の座標
            Double y = radius + radius * Math.Sin(radian) + this.Margin * (leftSide ? 1 : 2); ;


            return new System.Windows.Point(x, y);
        }

        // Tick処理　------------------------------------------------------------------
        public bool Tick(int reduce)
        {
            // 残時間を、指定時間分減らす
            Rest -= reduce;

            // 残時間を、テキストブロックに表示
            textBlock.Text = "" + Rest;

            // もし残時間 0 ならばTrueを返す
            if (Rest < 1)
            {
                // 円弧右端の座標を初期位置に戻す                
                Arc.Point = StartPoint;

                return true;
            }
            else
            {
                // 残時間の％を算出
                if (Rest == 0)
                {
                    // 残時間0ならば、残％も0
                    Progress = 0;
                }
                else
                {
                    // 残時間1以上ならば、残％は 残 / MAX ％
                    Progress = Rest * 1.0 / Full;
                }

                // 円弧の大小を決定
                Arc.IsLargeArc = Progress > 0.5 ? true : false;

                // 残時間を角度へ変換
                Angle = ResAngle(Progress);

                // Pointの算出
                Arc.Point = ResCircleEndPoint(Angle, (int)this.Radius);
                

                return false;
            }



        }

    }

    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●鳥の各パーツを管理するキャンバスクラス　　　　　　　　　　　　　　　●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    class CanvasContainer
    {
        // キャンバスの名前 (検索用)
        public String Name = "";

        // キャンバスそのもの
        public Canvas Canvas = new Canvas();

        // キャンバスの親要素
        public String ParentName = "all";


        // コンストラクタ― 　------------------------------------------------------------
        public CanvasContainer(String name, String parentName, Double width, Double height, int zindex, System.Windows.Point RenderTransformOrigin, System.Windows.Media.Color color)
        {
            // キャンバスの名前 (検索用)
            Name = name;

            // 回転の対象にするためにキャンバスへ名前をつける
            Canvas.Name = name;

            // 親要素の名前
            ParentName = parentName;

            // キャンバスの横幅
            Canvas.Width = width;

            // キャンバスの高さ
            Canvas.Height = height;

            // 回転についてのプロパティ
            RotateTransform rt = new RotateTransform();

            // キャンバスのプロパティに回転プロパティを加える
            Canvas.RenderTransform = rt;

            // キャンバスの回転プロパティを初期化
            Canvas.RenderTransformOrigin = RenderTransformOrigin;

            // キャンバスの Zindex（重なり） を設定
            Canvas.SetZIndex(Canvas, zindex);



            // originShowがTrueならば　回転の中心に目印として円を描画
            // 鳥画像の回転中心位置を調節する際に使います。
            // 新規鳥を追加時にご利用ください。
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


    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●鳥クラス　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    class Character
    {



        // 鳥の名前
        public String characterName = Settings.BirdNames[0];

        // キャンバスのwidth
        private double canvasWidh;

        // キャンバスのheight
        private double canvasHeight;

        // 鳥の中央からの距離 X
        public double positionX = -10;

        // 鳥の中央からの距離 Y
        public double positionY = 0;

        // 鳥がモーションの変更を受け付ける状態かどうか
        public bool isCanMosyonChange= false;

        // 定数・回転アニメーションを繰り返すか
        private const bool _autoReverse = true;


        // キャンバス・全てのパーツ
        public Canvas AllCanvas = new Canvas();

        // キャンバス・リスト　{名前、CanvasContainer}
        public Dictionary<String, CanvasContainer> canvasDic = new Dictionary<String, CanvasContainer>()
        {
            {"all",new CanvasContainer("all","noParents",Settings.BirdCanvasSize["width"],Settings.BirdCanvasSize["height"],0,new System.Windows.Point(0.5, 0.5),System.Windows.Media.Color.FromArgb(0,0,0,0))}
        };

        // ストーリーボード辞書・全てのパーツ
        public Dictionary<string, Storyboard> StoryboardDic = new Dictionary<string, Storyboard>()
        {
            {"default",new Storyboard() },
        };


        // ストーリーボード状態・Start中かどうか
        public Dictionary<string, string> StoryboardIsMoveDic = new Dictionary<string, string>()
        {
            {"default","stop"},
        };


        // Image辞書 部位名ごとに管理（head,body等）
        public Dictionary<string, System.Windows.Controls.Image> imageDic = new Dictionary<string, System.Windows.Controls.Image>();


        // コンストラクター ----------------------------------------------------------
        public Character(string name, double width, double height)
        {

            // 鳥パーツ各キャンバス　横幅
            canvasWidh = width;

            // 鳥パーツ各キャンバス　高さ
            canvasHeight = height;

            // 鳥パーツキャンバスのすべてを収めるキャンバス　横幅
            canvasDic["all"].Canvas.Width = width;

            // 鳥パーツキャンバスのすべてを収めるキャンバス　高さ
            canvasDic["all"].Canvas.Height = height;

            // 全ての鳥を収める、描画用キャンバスに この鳥のcanvasDic["all"].Canvasを追加
            AllCanvas.Children.Add(canvasDic["all"].Canvas);


            // 鳥の名前
            characterName = name;

            // 基本的な鳥パーツをセット
            SetBirdImg(width, height);

            // 鳥アニメーションをセット
            setBirdAnimation(positionX, positionY);

        }

        // ストーリーボードの操作  --------------------------------------------------
        public void StoryControl(string storyName , string elemName , string move)
        {
            Debug.Print(storyName+"."+elemName+"=>"+move+"が指示されました 現在は"+ StoryboardIsMoveDic[storyName]);

            // StoryboardIsMoveDicに格納されている現在Moveと同じなので何もしない
            if (StoryboardIsMoveDic[storyName] == move){ return; }



            switch (move)
            {
                case "start":

                    //もし停止中なら普通に再開
                    if (StoryboardIsMoveDic[storyName] == "stop")
                    {
                        StoryboardDic[storyName].Begin(canvasDic[elemName].Canvas, true);
                        StoryboardIsMoveDic[storyName] = "start";
                    }
                    else
                    {
                        // 一時停止中 もしくは 再生中 なので、続きから再開                             
                        StoryboardDic[storyName].Resume(canvasDic[elemName].Canvas);
                        StoryboardIsMoveDic[storyName] = "start";

                    }
                    break;


                case "reStart":
                    // startと同様の処理
                    StoryboardDic[storyName].Begin(canvasDic[elemName].Canvas, true);
                    StoryboardIsMoveDic[storyName] = "start";
                    break;

                case "stop":

                    // 停止
                    StoryboardDic[storyName].Stop(canvasDic[elemName].Canvas);
                    StoryboardIsMoveDic[storyName] = "stop";

                    break;


                case "pause":

                    // もし停止中に一時停止指示が来ても何もしない
                    if (StoryboardIsMoveDic[storyName] == "stop"){ return; }
                                        
                    // 一時停止 
                    StoryboardDic[storyName].Pause(canvasDic[elemName].Canvas);
                    StoryboardIsMoveDic[storyName] = "pause";
                    
                    break;


                default:
                    MessageBox.Show("対処が決められていない move ！" + move);
                    Debug.Print("対処が決められていない move ！" + move);
                    break;
            }
        }





        // CanvasのDoubleアニメーションをストーリーボードに登録する  ----------------
        public void SetDoubleAnimation(
            string targetName,
            PropertyPath _PropertyPath,
            double from, double to, double duration,
            RepeatBehavior _RepeatBehavior,
            bool autoReverse = _autoReverse,
            string targetStoryboard = "defo"
         )

        {
            // アニメーションの宣言
            DoubleAnimation anm = new DoubleAnimation(
                from, to, new Duration(TimeSpan.FromSeconds(duration))
            );


            // 自動で反復か
            anm.AutoReverse = autoReverse;

            // 反復方法の設定            
            anm.RepeatBehavior = _RepeatBehavior;



            // ストーリーボードに、ターゲット名とアニメーションをセットで登録
            Storyboard.SetTargetName(anm, targetName);

            // セット登録したアニメーションが何を対象にしているか登録
            Storyboard.SetTargetProperty(anm, _PropertyPath);

            // ストーリーボード辞書に指定名が無ければ作っておく
            if (StoryboardDic.ContainsKey(targetStoryboard) == false)
            {

                StoryboardDic.Add(targetStoryboard,new Storyboard());
                Debug.Print("newStoryboard! : "+targetStoryboard);

                StoryboardIsMoveDic.Add(targetStoryboard,"stop");

            }
            // ストーリーボード辞書に追加
            StoryboardDic[targetStoryboard].Children.Add(anm);


            // ストーリーボードのアニメーションが終わったら StoryboardIsMoveDicを"Stop"にする
            StoryboardDic[targetStoryboard].Completed += (sender, e) =>
            {
                StoryboardIsMoveDic[targetStoryboard] = "stop";
            };

        }

        // 鳥の変身　イメージのソースを変更する----------------------------------------------
        public void changeImage(string name)
        {
            // 鳥名変更
            characterName = name;
            foreach (var item in imageDic)
            {
                imageDic[item.Key].Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + characterName + "_" + item.Key + ".png"));

            }
        }


        // アニメーション用のImageを作成する  -------------------------------------------
        public System.Windows.Controls.Image MakeImageInCanvas(String name, Double width, Double height)
        {
            var Image = new System.Windows.Controls.Image();
            Image.Width = width;
            Image.Height = height;
            Image.Stretch = Stretch.Fill;
            Image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + characterName + "_" + name + ".png"));
            return Image;
        }


        // キャンバスを作り画像を加える --------------------------------------------------
        public void SetImageToCanvas(String name, String  parentGroupName ,Double width, Double height,int zindex, System.Windows.Point rotateCener, System.Windows.Media.Color color)
        {




            // 親に指定したキャンバスが存在しているか調べる それが自分自身ならスキップ 
            if (canvasDic.ContainsKey(parentGroupName) == false && name != parentGroupName)
            {

                Debug.Print("親に指定されたキャンバス ["+ parentGroupName + "] が存在しません。zindexは0、rotateOriginは0.5,0.5で新規作成します ");

                // 無ければ作る zindexは0、rotateOriginは0.5,0.5で固定
                canvasDic.Add(parentGroupName, new CanvasContainer(parentGroupName, "all", canvasWidh, canvasHeight, 0, new System.Windows.Point(0.5, 0.5), System.Windows.Media.Color.FromArgb(255,255,0,0)));

                // キャンバスの名前を設定
                canvasDic[parentGroupName].Canvas.Name = parentGroupName;


                // 親はcanvasDic["all"]固定
                canvasDic["all"].Canvas.Children.Add(canvasDic[parentGroupName].Canvas);

            }

            // 自身のキャンバスを作成
            canvasDic.Add(name, new CanvasContainer(name, parentGroupName, canvasWidh, canvasHeight, zindex, rotateCener,color));

            // 自身のキャンバスの名前を設定
            canvasDic[name].Canvas.Name = name;

            // 親のcanvasDicにAdd　それが自分なら不要
            if (name != parentGroupName)
            {
               canvasDic[parentGroupName].Canvas.Children.Add(canvasDic[name].Canvas);
            }
           
            // Imageを作成
            var image = MakeImageInCanvas(name, canvasWidh, canvasHeight);
            imageDic.Add(name,image);

            // canvasDic[name].CanvasにImageを追加
            canvasDic[name].Canvas.Children.Add(imageDic[name]);

        }


        // 基本的な鳥パーツを一括でセット  ------------------------------------
        public void SetBirdImg( double width, double height)
        {
            SetImageToCanvas("body", "all", width, height,   0, Settings.BirdRotatePoint["body"], System.Windows.Media.Color.FromArgb(255, 255, 255, 0)  );
            SetImageToCanvas("reg",  "body", width, height, -1, Settings.BirdRotatePoint["reg"],  System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
            SetImageToCanvas("tail", "body", width, height, -2, Settings.BirdRotatePoint["tail"], System.Windows.Media.Color.FromArgb(255, 0, 255, 255));
            SetImageToCanvas("head", "body", width, height,  1, Settings.BirdRotatePoint["head"], System.Windows.Media.Color.FromArgb(255, 255, 0, 255));

        }


        // 鳥アニメーションの設定をセット （上書き式）
        public void setBirdAnimation(double _positionX, double _positionY)
        {

            // 旧位置から移動 Y
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY,_positionY, 0.5,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );


            // 旧位置から移動 X
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                positionX, _positionX, 0.25,
                new RepeatBehavior(1),
                false,
                "jumpToNewPosition"
            );

            // 旧位置から移動 かつ透明から可視へ
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


            // All 1回ジャンプ
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY, positionY - 10, 0.2,
                new RepeatBehavior(1),
                true,
                "jump"
            );

            // All 2回ジャンプ
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.TopProperty),
                positionY, positionY - 10, 0.1,
                new RepeatBehavior(2),
                true,
                "twoJump"
            );

            // All 左へ
            SetDoubleAnimation(
                "all",
                new PropertyPath(Canvas.LeftProperty),
                positionX, positionX - 30, 0.4,
                new RepeatBehavior(1),
                false,
                "toLeft"
            );
            Canvas.SetLeft(canvasDic["all"].Canvas, -15);



            // 以下は座標関係ない回転のアニメーションな為、基本位置の影響なし

            // head 首傾げ
            SetDoubleAnimation(
                "head",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                -20, 20, 4,
                RepeatBehavior.Forever,
                true
            );
            //canvasDic["head"].Canvas.RenderTransform = new RotateTransform(-20);

            // tail しっぽ振り 
            SetDoubleAnimation(
                "tail",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                30, -30, 4,
                RepeatBehavior.Forever,
                true
            );
            //canvasDic["tail"].Canvas.RenderTransform = new RotateTransform(30);


            // body 体傾げ
            SetDoubleAnimation(
                "body",
                new PropertyPath("(RenderTransform).(RotateTransform.Angle)"),
                10, -10, 6,
                RepeatBehavior.Forever,
                true
            );
            //canvasDic["body"].Canvas.RenderTransform = new RotateTransform(-10);


        }



    }

    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●吹き出しクラス　　　　　　　　　　　　　　　　　　　　　　　　　　　●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    class hukidasiAnimation
    {
        // フェードインのストーリーボード
        public Storyboard _StoryboardFeedIn = new Storyboard();

        // フェードアウトのストーリーボード
        public Storyboard _StoryboardFeedOut = new Storyboard();

        // コンストラクタ―
        public hukidasiAnimation()
        {

            // ストーリーボードに、ターゲット名とアニメーションをセットで登録
            Storyboard.SetTargetName(feedIn, "messageBack");
            Storyboard.SetTargetName(feedOut, "messageBack");


            // セット登録したアニメーションが何を対象にしているか登録
            Storyboard.SetTargetProperty(feedIn, new PropertyPath(Control.OpacityProperty));
            Storyboard.SetTargetProperty(feedOut, new PropertyPath(Control.OpacityProperty));


            // ストーリーボードに追加 
            _StoryboardFeedIn.Children.Add(feedIn);
            _StoryboardFeedOut.Children.Add(feedOut);

        }


        // フェードインアニメーション  ---------------------------------------------
        private DoubleAnimation feedIn = new DoubleAnimation()
        {
            From = 0.0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(2)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        // フェードアウトアニメーション  -------------------------------------------
        private DoubleAnimation feedOut = new DoubleAnimation()
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromSeconds(5)),
            RepeatBehavior = new RepeatBehavior(1),
            AutoReverse = false
        };

        // 停止アニメーション  -------------------------------------------
        public void Stop()
        {
             _StoryboardFeedOut.Stop();
             _StoryboardFeedIn.Stop();
        }


    }






    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    // ●メインウィンドウクラス　　　　　　　　　　　　　　　　　　　　　　　●
    // ●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●
    public partial class MainWindow : Window
    {
        // 鳥の宣言
        private Character bird;

        // 吹き出しアニメーションクラスの宣言
        private hukidasiAnimation hukidasiAnimation = new hukidasiAnimation();

        // 時間管理タイマー
        private DispatcherTimer _timer = new DispatcherTimer();

        // 素早いクリック判定タイマー 
        private DispatcherTimer tooClick_timer = new DispatcherTimer();

        // 素早いクリックカウント用　Int
        private int tooClickCount = 0;

        // 円(秒)
        private ProgresCircleControl circle_S;
        // 円(分)
        private ProgresCircleControl circle_M;


        // 読み込み後に実行される内容 -------------------------------

        public MainWindow()
        {
            InitializeComponent();


            ////////////////////////////////////
            // ウィンドウ
            ////////////////////////////////////

            //  ウィンドウをマウスのドラッグで移動できるようにする
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            ////////////////////////////////////
            // 鳥
            ////////////////////////////////////

            // 鳥の宣言
            bird = new Character(Settings.BirdNames[0], Settings.BirdCanvasSize["width"], Settings.BirdCanvasSize["height"]);


            // 鳥キャンバスをウィンドウに追加
            canvas_bird.Children.Add(bird.AllCanvas);

            foreach (var item in bird.canvasDic)
            {
                // MainWindowに鳥キャンバスの名前と要素を登録 必須！
                RegisterName(item.Key, bird.canvasDic[item.Key].Canvas);
            }

            // 鳥アニメーションをスタート
            startSwing();

            // 鳥アニメーション・jumpToNewPosition の移動が終わったら以下の処理を実行する
            bird.StoryboardDic["jumpToNewPosition"].Completed += (sender, e) =>
            {
                startSwing();
            };


            ////////////////////////////////////
            // 円弧
            ////////////////////////////////////

            // タイマーの周囲にある円弧の宣言
            circle_S = new ProgresCircleControl(timePath_S, timePath_S_figure.StartPoint, showTime_S, Settings.TimeUnit);
            circle_M = new ProgresCircleControl(timePath_M, timePath_M_figure.StartPoint, showTime_M, Settings.M_defo);


            ////////////////////////////////////
            // タイマー　・素早いクリック判定タイマー
            ////////////////////////////////////

            // 残時間が25分59秒設定になっているので訂正
            TimeSet(Settings.M_defo);

            // タイマーのインターバル設定
            _timer.Interval = new TimeSpan(0, 0, 1);

            // タイマーが一秒ごとに何をするか設定
            _timer.Tick += (sender, e) =>
            {
                // もし秒数(S)が0ならば分(M)のPointを動かす
                if (circle_S.Tick(1))
                {
                    circle_M.Tick(1);

                    // もしまだ分数(M)が1以上ならば、秒数を補充する
                    if (circle_M.Rest > 0)
                    {
                        circle_S.Rest = Settings.TimeUnit;

                        // 表示だけ訂正する
                        circle_S.Tick(0);

                        // 残時間が一定時間以下になったらアナウンス
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


                            // 分数(M)が0ならばタイマーを終了する
                            _timer.Stop();

                            // メッセージを表示
                            messageControl(true, Settings.Comment["終了"]);
                            Debug.Print("残時間"+ circle_M.Rest+":"+ circle_S.Rest);


                            //  ウィンドウを中央に表示させる
                            this.Top = SystemParameters.WorkArea.Height / 2 - this.Height / 2;
                            this.Left = SystemParameters.WorkArea.Width / 2 - this.Width / 2;


                    }
                }
            };

            // タイマー開始する
            TimerStart();


            // 素早いクリック判定タイマーのインターバル設定
            tooClick_timer.Interval = new TimeSpan(0, 0, 1);

            // 素早いクリック判定タイマー タイマーが一秒ごとに何をするか設定
            tooClick_timer.Tick += (sender, e) =>
            {

                // クリックカウントが、素早いクリック判定数字を上回ったならば変身・移動
                if (tooClickCount > Settings.QuickJudgment)
                {
                    Debug.Print("素早いクリック！");

                    // クリックカウント初期化
                    tooClickCount = 0;


                    // 鳥がモーションの変更を受け付けないようにする
                    bird.isCanMosyonChange = false;


                    // 鳥の現在位置を変更
                    Random r1 = new System.Random();
                    int index = r1.Next(0, Settings.NewXYList.Length);

                    // 鳥画像候補を選出（現在の画像以外）
                    string[] d = { };

                    // 無限ループ防止機能
                    if (Settings.BirdNames.Length <2) {
                        MessageBox.Show("エラー！鳥の名前は2つ以上登録してください！");
                        return;
                    }

                    // ランダムでどの鳥に変身するか決定
                    int birdIndex = 0;
                    while (true)
                    {
                        // ランダムで鳥名インデックスを決定
                        birdIndex = new System.Random().Next(0, Settings.BirdNames.Length);

                        // もし現在の鳥名と異なる鳥名ならば決定
                        if (bird.characterName != Settings.BirdNames[birdIndex])
                        {
                            break;
                        }
                    }

                    Debug.Print(" birdIndex"+ birdIndex);

                    // 鳥画像を変える
                    bird.changeImage(Settings.BirdNames[birdIndex]);

                    // 鳥アニメーションを新規座標に合わせて更新                    
                    bird.setBirdAnimation(Settings.NewXYList[index][0], Settings.NewXYList[index][1]);

                    // 鳥アニメーションスタート
                    bird.StoryControl("jumpToNewPosition", "all", "reStart");

                }
                else
                {
                    // 1tickごとに-3
                    tooClickCount-=3;

                    if (tooClickCount <0)
                    {
                        //カウント0以下で停止
                        tooClick_timer.Stop();

                        Debug.Print("素早いクリック判定停止！ 現在 " + tooClickCount);

                    }
                }

            };


            //  画面が閉じられるときに、其々のタイマーを停止させる
            this.Closing += (e, s) =>
            {
                _timer.Stop();
                tooClick_timer.Stop();

            };



        }

        // タイマーを初期化 ------------------------------------------
        private void TimeSet(int minute)
        {
            // 残時間再設定
            circle_S.Rest = 0;
            circle_M.Rest = minute;
            circle_M.Full = minute;
            Settings.M_defo = minute;

            // 表示を初期化
            circle_S.Tick(0);
            circle_M.Tick(0);
        }

        // タイマーを開始 --------------------------------------------
        private void TimerStart()
        {
            messageControl(false, Settings.Comment["開始"]);

            //  ウィンドウを画面右下に表示させる
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;

            // 残り時間が0ならば残時間再設定
            if (circle_S.Rest <= 0 && circle_M.Rest <= 0)
            {
                TimeSet(Settings.M_defo);
            }

            _timer.Start();
        }

        // メッセージの表示
        private void messageControl(bool show, string text)
        {
            // messageTextを変更
            messageText.Text = text;

            // 吹き出しアニメーションを一時停止しておく
            hukidasiAnimation.Stop();

            if (show)
            {
                // フェードイン
                hukidasiAnimation._StoryboardFeedIn.Begin(messageBack, true);

                // 吹き出しの表示・非表示            
                messageBack.Visibility = Visibility.Visible;               
            }
            else
            {
                // フェードアウト
                hukidasiAnimation._StoryboardFeedOut.Begin(messageBack, true);
            }
        }

        // スタートボタンを押したときの処理
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            // タイマー開始する 鳥アニメーションは初期位置から開始 
            TimerStart();

            // 鳥の揺動アニメーションを一時停止
            bird.StoryControl("defo", "all", "pause");

            // 鳥の2回ジャンプアニメーションを実行
            bird.StoryControl("twoJump", "all", "start");

            // 吹き出しメッセージ
            messageControl(false, Settings.Comment["開始"]);






            // twoJump の移動が終わったら以下の処理を実行
            bird.StoryboardDic["twoJump"].Completed += (sender, e) =>
            {                
                startSwing();

                // 実行が終わったら登録を消す
                bird.StoryboardDic["twoJump"].Completed -= (sender, e) =>
                {
                    startSwing();
                };
            };
        }

        // 25分ボタンを押したときの処理
        private void Set25min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(25);
        }

        // 5分ボタンを押したときの処理
        private void Set5min_Button_Click(object sender, RoutedEventArgs e)
        {
            TimeSet(5);
        }

        // 一時停止ボタンを押したときの処理
        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            // カウントダウンをストップ　鳥アニメーションは一時停止
            _timer.Stop();

            // 鳥アニメーションを一時停止
            bird.StoryControl("defo", "all", "pause");
            
            // 吹き出しメッセージ
            messageControl(true, Settings.Comment["一時停止"]);

        }

        // ストップボタンを押したときの処理
        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {

            // カウントダウンをストップ　鳥アニメーションはストップでなく一時停止
            _timer.Stop();

            // 吹き出しメッセージ
            bird.StoryControl("defo", "all", "pause");

            // 鳥アニメーションを停止
            messageControl(true, Settings.Comment["停止"]);

            // タイマー再設定
            TimeSet(Settings.M_defo);
        }


        // 鳥の揺動アニメーションを開始する
        private void startSwing()
        {
            Debug.Print("再スタート");

            // 鳥アニメーション開始
            bird.StoryControl("defo", "all", "start");

            // 鳥がモーションの変更を受け付けられるようにする
            bird.isCanMosyonChange = true;
        }

        // 鳥をクリックした時
        private void canvas_bird_MouseDown(object sender, MouseButtonEventArgs e)
        {
            

            // 鳥の1回ジャンプアニメーションを実行
            bird.StoryControl("jump", "all", "start");
            messageControl(false, Settings.RandomComment[bird.characterName][(new System.Random()).Next(0, Settings.RandomComment[bird.characterName].Length)]);





            // 素早いクリックカウントが0以下ならば、素早いクリック判定タイマーを起動
            if (tooClickCount<=0)
            {
                tooClick_timer.Start();
            }
            tooClickCount++;
            Debug.Print("clicked 現在" + (tooClickCount + 1));
        }
    }
}
