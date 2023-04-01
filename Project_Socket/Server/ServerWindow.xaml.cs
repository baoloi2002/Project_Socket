using MaterialDesignThemes.Wpf;
using SuperSimpleTcp;
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

    public partial class ServerWindow : Window
    {
        private QuizQuestion[] quizList;
        private List<Person> people;
        public ServerWindow()
        {
            InitializeComponent();
            StartServer();
            quizList = LoadQuestions("QuizList.json");
            ShowQuiz(quiz: quizList[30]);

            // Create a collection of Person objects
            people = new List<Person>
            {
                new Person { Name = "Alice", Icon = PackIconKind.Account, Order = -9 },
                new Person { Name = "Bob1", Icon = PackIconKind.AccountOff, Order = 8 },
                new Person { Name = "Bob2", Icon = PackIconKind.AccountOff, Order = 7 },
                new Person { Name = "Bob3", Icon = PackIconKind.AccountOff, Order = 6 },
                new Person { Name = "Bob4", Icon = PackIconKind.AccountOff, Order = 5 },
                new Person { Name = "Bob5", Icon = PackIconKind.AccountOff, Order = 4 },
                new Person { Name = "Bob6", Icon = PackIconKind.AccountOff, Order = 3 },
                new Person { Name = "Bob7", Icon = PackIconKind.AccountOff, Order = 2 },
                new Person { Name = "Bob8", Icon = PackIconKind.AccountOff, Order = 1 },
                new Person { Name = "Charlie", Icon = PackIconKind.Account, Order = -1 },
            };

            lstUsersView.ItemsSource = people;
        }

        private void StartServer()
        {
            
        }
        private void SortList()
        {
            // Sort the list
            List<Person> sortedList = people.OrderBy(p => p.Order).ToList();

            // Update the SortOrder property of each item
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Order = i;
            }

            // Set the sorted list as the ItemsSource of the ListView
            lstUsersView.ItemsSource = sortedList;
        }


        private QuizQuestion[] LoadQuestions(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            QuizQuestion[] questions = JsonSerializer.Deserialize<QuizQuestion[]>(jsonString);
            Random random = new Random();
            for(int i = questions.Length - 1; i > 0; i--)
            {
                int j = random.Next(i+1);
                QuizQuestion tmp = questions[j];
                questions[j] = questions[i];
                questions[i] = tmp;
            }
            return questions;
        }

        private void ShowQuiz(QuizQuestion quiz)
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
    }
}
