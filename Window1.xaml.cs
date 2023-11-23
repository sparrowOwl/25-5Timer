using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace WpfApp_25to5Timer
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            // MainWindowのどこをドラッグしてもウィンドウを移動できるようにする
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width - 200;

            FlowDocument taskExplanationDocument = new FlowDocument();


            for (int i = 0; i < Settings.TaskWindowInitialDescription.Length; i++)
            {
                Paragraph p = new Paragraph(new Run(Settings.TaskWindowInitialDescription[i]));
                taskExplanationDocument.Blocks.Add(p);
            }
            taskDocument.Document = taskExplanationDocument;

            this.Closing += (sender, e) =>
            {
                Debug.Print("w1が閉じられたよ");
                Settings.TaskWinClosed = true;
            };

            // もしタスクリストの長さが０以上ならば、おそらくユーザーによって閉じられている
            // タスクリストから表示を復活させる
            if (Settings.TaskList.Count >0)
            {
                taskChange("");
            }

            topTaskRemoveButton.Opacity = 0.2;


        }






        private void taskClear()
        {

            Settings.TaskList.Clear();
            taskChange("");

        }

        private void taskChange(string processType)
        {


            Debug.Print("done " + processType);


            if (processType == "topTaskRemove")
            {
                if (Settings.TaskList.Count == 0) { return; }

                Settings.TaskList.RemoveAt(0);
                Settings.FinishTaskCount++;
            }
            else if(processType == "taskFromFlowDocument")
            {
                Settings.TaskList.Clear();

                TextRange TaskTextRange = new TextRange(taskDocument.Document.ContentStart, taskDocument.Document.ContentEnd);
                string[] taskTextArray = Regex.Split(TaskTextRange.Text, "\r|\n|\r\n");
                foreach (string item in taskTextArray)
                {
                    if (item == "") { continue; }
                    Settings.TaskList.Add(item);
                }
            }

            

            FlowDocument taskExplanationDocument = new FlowDocument();

            bool isNowTask = true;
            foreach (string task in Settings.TaskList)
            {
                Paragraph p = new Paragraph(new Run(task));
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
        }

        public void topTaskRemove()
        {
            taskChange("topTaskRemove");
        }

        private void taskCompleteAndHide(object sender, RoutedEventArgs e)
        {
            taskChange("taskFromFlowDocument");
            this.Hide();
        }

        private void taskComplete(object sender, RoutedEventArgs e)
        {
            taskChange("taskFromFlowDocument");

        }

        private void taskClear(object sender, RoutedEventArgs e)
        {
            taskClear();
        }

        private void topTaskRemove(object sender, RoutedEventArgs e)
        {
            
            topTaskRemove();

        }
    }
}
