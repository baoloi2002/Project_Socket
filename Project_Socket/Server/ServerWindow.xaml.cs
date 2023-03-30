using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_Socket.Server
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public class QuizQuestion
    {
        public string question { get; set; }
        public string[] choices { get; set; }
        public int answer { get; set; }
    }

    public partial class ServerWindow : Window
    {

        private QuizQuestion[] LoadQuestions(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            QuizQuestion[] questions = JsonSerializer.Deserialize<QuizQuestion[]>(jsonString);
            return questions;
        }

        private void ShowQuiz( QuizQuestion quiz)
        {
            tbQuestion.Text = quiz.question;
            if (quiz.choices != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    btnAns0.Content = quiz.choices[0];
                    btnAns1.Content = quiz.choices[1];
                    btnAns2.Content = quiz.choices[2];
                    btnAns3.Content = quiz.choices[3];
                }
            }
        }

        private QuizQuestion[] quizList;
        public ServerWindow()
        {
            InitializeComponent();
            quizList = LoadQuestions("QuizList.json");

            ShowQuiz(quiz: quizList[0]);
        }
    }
}
