using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;


namespace WpfApp_25to5Timer
{

    public partial class Window1 : Window
    {




        public Window1()
        {
            InitializeComponent();

            // MainWindowのどこをドラッグしてもウィンドウを移動できるようにする
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width - 200;

            FlowDocument taskExplanationDocument = new();


            for (int i = 0; i < Settings.TaskWindowInitialDescription.Length; i++)
            {
                Paragraph p = new (new Run(Settings.TaskWindowInitialDescription[i]));
                taskExplanationDocument.Blocks.Add(p);
            }
            taskDocument.Document = taskExplanationDocument;

            this.Closing += (sender, e) =>
            {
                Debug.Print("w1が閉じられたよ");
                Settings.TaskWinClosed = true;
            };



            // もしタスクリストの長さが０以上ならば、おそらくユーザーによってウィンドウが閉じられた。
            // タスクリストから表示を復活させる
            if (Settings.TaskList.Count >0)
            {
                TaskChange("show");
            }
            else
            {
                 ChangeTaskControlButtonOpacity();
            }



        }


        public void AddListItem(string str)
        {
            DateTime dt = DateTime.Now;
            string dateResult = dt.ToString("HH:mm:ss");
            
            var tb = new TextBlock();
            tb.Text = dateResult + " " + str;
            tb.TextWrapping = TextWrapping.Wrap;

            // 親要素の幅からスクロールバー分を引いたWidthに設定する
            tb.Width = messageListBox.Width - 25;
            messageListBox.Items.Add(tb);
            int lastIndex = messageListBox.Items.Count - 1;
            var lastItem = messageListBox.Items.GetItemAt(lastIndex);
            messageListBox.ScrollIntoView(lastItem);
        }


        public void ReadCsv(string csvName)
        {
            Settings.TaskList.Clear();
            StreamReader sr;



            try
            {
               sr = new StreamReader(csvName);
            }
            catch (FileNotFoundException ex)
            {
                Debug.Print("csvファイルを読み込みましたが、空でした。ReadCsv("+csvName+")を終了します");
                AddListItem("csvファイルを読み込みましたが、空でした");
                File.WriteAllText(csvName, "");

                return;
            }


            while (sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(",");
                Settings.TaskList.AddRange(values);
            }

            // 空行の削除
            List<string> nullCheckList = new ();
            foreach (string item in Settings.TaskList)
            {
                if (item == "") { continue; }
                nullCheckList.Add(item);
            }
            Settings.TaskList = nullCheckList;


            sr.Close();
            TaskChange("read");


        }

        public void SaveCsv(string csvName)
        {

            TaskChange("taskFromFlowDocument");

            // lastTask.csvにタスクを保存
            string taskStringForCSV = "";

            foreach (string task in Settings.TaskList)
            {
                taskStringForCSV += task + ",";
            }

            File.WriteAllText(csvName, taskStringForCSV);


            AddListItem("csvファイルを保存しました");

        }

        private void ChangeTaskControlButtonOpacity()
        {
            taskInputCompleteButton.Opacity = 0.2;
            if (Settings.TaskList.Count > 0)
            {
                topTaskRemoveButton.Opacity = 1;
            }
            else
            {
                topTaskRemoveButton.Opacity = 0.2;               
            }

            TextRange TaskTextRange = new (taskDocument.Document.ContentStart, taskDocument.Document.ContentEnd);
            if (TaskTextRange.Text == "")
            {
                taskTotalDeletionButton.Opacity = 0.2;
            }
            else
            {
                taskTotalDeletionButton.Opacity = 1;
            }
        }




        private void TaskChange(string processType)
        {


            Debug.Print("done " + processType);


            if (processType == "TopTaskRemove")
            {
                if (Settings.TaskList.Count == 0) { return; }
                AddListItem("●タスク・"+ Settings.TaskList[0]+"を完了");
                Settings.TaskList.RemoveAt(0);
                Settings.FinishTaskCount++;
            }
            else if(processType == "taskFromFlowDocument")
            {

                AddListItem("一覧からタスクを取得");
                Settings.TaskList.Clear();
                TextRange TaskTextRange = new (taskDocument.Document.ContentStart, taskDocument.Document.ContentEnd);
                string[] taskTextArray = Regex.Split(TaskTextRange.Text, "\r|\n|\r\n");
                foreach (string item in taskTextArray)
                {
                    if (Regex.Replace(item, "^\\s+", "") == "") { continue; }
                    Settings.TaskList.Add(item);
                }

            }
            else if (processType == "taskClear")
            {
                AddListItem("タスクを初期化");
                Settings.TaskList.Clear();
            }
            else if (processType == "show")
            {
                AddListItem("タスクを表示しました");
            }
            else if (processType == "read")
            {
                AddListItem("csvファイルを読み込みました");
            }
            else
            {
                MessageBox.Show("ERROR TaskChange(string processType)　において想定外の引数　："+ processType);
            }




        



        FlowDocument taskExplanationDocument = new ();

            bool isNowTask = true;
            foreach (string task in Settings.TaskList)
            {
                Paragraph p = new (new Run(task));
                if (isNowTask)
                {
                    isNowTask = false;
                    p.Style = (Style)Resources["nowTask"];


                }
                taskExplanationDocument.Blocks.Add(p);
            }
            taskDocument.Document = taskExplanationDocument;
            if (Settings.TaskList.Count==0)
            {
                nowTaskTextBox.Text = "なし";
                topTaskRemoveButton.Opacity = 0.2;
            }
            else
            {
                nowTaskTextBox.Text = Regex.Replace(Settings.TaskList[0], "^\\s+", "");
                topTaskRemoveButton.Opacity = 1;

            }



            // タスクをこなした回数の更新
            finishTaskCountTextBlock.Text = Settings.FinishTaskCount.ToString();

            // タスクをこなした回数に応じた応援を更新
            if (Settings.FinishTaskCount == 0)
            {
                finishTaskCommentTextBlock.Text = "まだこれから";

            } else if (Settings.FinishTaskCount % 4 ==0)
            {
                finishTaskCommentTextBlock.Text = "４タスクのたび\n長めのリフレッシュ！";

            }else
            {
                string firstString = "";

                switch (Settings.FinishTaskCount / 10)
                {
                    case 0: 
                        break;
                    case 1: firstString = "調子がいい！\n";
                        break;
                    case 2: firstString = "はかどってる！\n";
                        break;
                    case 3:
                        firstString = "波にのってる！\n";
                        break;
                    case 4:
                        firstString = "すさまじい！\n";
                        break;

                    default:
                        firstString = "とてつもない！\n";
                        break;
                }
                
                finishTaskCommentTextBlock.Text = firstString + Settings.CheeringOfCount[Settings.FinishTaskCount % (Settings.CheeringOfCount.Length)]; ;
            }

            int eggLv = (Settings.FinishTaskCount / 4);
            if (eggLv>= Settings.CheeringOfCountImage.Length) { eggLv = Settings.CheeringOfCountImage.Length-1; }

            finishTaskCommentImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/" + Settings.CheeringOfCountImage[eggLv]));


            ChangeTaskControlButtonOpacity();
        }

        public void TopTaskRemove()
        {
            TaskChange("TopTaskRemove");
        }

        private void TaskCompleteAndHide(object sender, RoutedEventArgs e)
        {
            TaskChange("taskFromFlowDocument");
            this.Hide();
        }

        private void TaskComplete(object sender, RoutedEventArgs e)
        {
            if (taskInputCompleteButton.Opacity == 0.2) { return; }
            TaskChange("taskFromFlowDocument");

        }

        private void TaskClear(object sender, RoutedEventArgs e)
        {
            TaskChange("taskClear");
        }

        private void TopTaskRemove(object sender, RoutedEventArgs e)
        {
            
            TopTaskRemove();

        }

        private void ReadCsvButton_Click(object sender, RoutedEventArgs e)
        {
            ReadCsv("lastTask.csv");
        }
        private void SaveCsvButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCsv("lastTask.csv");
        }
        private void TaskDocument_TextChanged(object sender, TextChangedEventArgs e)
        {
            taskInputCompleteButton.Opacity = 1;
            taskTotalDeletionButton.Opacity = 1;
        }
    }
}
