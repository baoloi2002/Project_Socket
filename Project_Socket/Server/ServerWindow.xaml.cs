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
        public string? Question { get; set; }
        public List<string>? Choices { get; set; }
        public int Answer { get; set; }
    }

    public class Quiz
    {
        public List<QuizQuestion>? Questions { get; set; }
    }
    public partial class ServerWindow : Window
    {

        private Quiz QuizLoad()
        {
            string filePath = "QuizList.json";
            string jsonString;
            if (File.Exists(filePath))
            {
                jsonString = File.ReadAllText(filePath);
                // Do something with the file contents
            }
            else
            {
                // File does not exist
                return null; // or throw an exception if appropriate
            }

            try
            {
                Quiz quiz = JsonSerializer.Deserialize<Quiz>(json: jsonString);
                return quiz;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deserialization
                // (e.g. invalid JSON format)
                Console.WriteLine($"Error deserializing Quiz: {ex.Message}");
                return null;
            }
        }

        private Quiz quizList;

        private void ShowQuiz( QuizQuestion quiz)
        {
            tbQuestion.Text = quiz.Question;
            if (quiz.Choices != null && quiz.Choices.Count >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    btnAns0.Content = quiz.Choices[0];
                    btnAns1.Content = quiz.Choices[1];
                    btnAns2.Content = quiz.Choices[2];
                    btnAns3.Content = quiz.Choices[3];
                }
            }
        }

        public ServerWindow()
        {
            InitializeComponent();
            quizList = QuizLoad();

            //ShowQuiz(quiz: quizList.Questions[0]);
        }
    }
}
