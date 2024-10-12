using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MatematikOyunu
{
    public partial class Form1 : Form
    {
        private int currentLevel;
        private int currentQuestion = 1;
        private int correctAnswers = 0;
        private int wrongAnswers = 0;
        private int passCount = 0;
        private List<Question> questions;
        private Random random = new Random();
        private Timer gameTimer;
        private int remainingTime;

        private const string SaveFilePath = "game_progress.txt"; // Skor dosyası
        private int currentBlock = 1; // Hangi blokta olduğumuzu izlemek için
        private const int questionsPerBlock = 5; // Her blokta gösterilecek soru sayısı

        public Form1(int startingLevel = 1)
        {
            InitializeComponent();
            currentLevel = startingLevel; // Başlangıç seviyesini ayarla
            InitializeGame();
        }



        private void InitializeGame()
        {
            gameTimer = new Timer();
            gameTimer.Interval = 1000; // 1 saniye
            gameTimer.Tick += GameTimer_Tick;

            buttonStartGame.Click += ButtonStartGame_Click;
            buttonAnswer.Click += ButtonAnswer_Click;
            buttonPass.Click += ButtonPass_Click;
            buttonNextLevel.Click += buttonNextLevel_Click; // Yeni seviyeye geçiş için olay işleyici

            UpdateLevelDisplay();
            DisableGameControls();
        }

        private void ButtonStartGame_Click(object sender, EventArgs e)
        {
            StartNewLevel();
            EnableGameControls();
            buttonStartGame.Enabled = false;
        }

        private void StartNewLevel()
        {
            currentBlock = 1; // Yeni seviyede ilk bloktan başla
            correctAnswers = 0;

            passCount = 0;
            questions = GenerateQuestions();
            remainingTime = 300 + (currentLevel - 1) * 60;
            UpdateTimeDisplay();
            gameTimer.Start();
            DisplayQuestion(); // İlk blok sorularını göster
            UpdateLevelDisplay();
        }


        private List<Question> GenerateQuestions()
        {
            var questions = new List<Question>();
            for (int i = 0; i < 20; i++)
            {
                questions.Add(GenerateQuestion());
            }
            return questions;
        }

        private Question GenerateQuestion()
        {
            int a, b;
            string operation;

            if (currentLevel <= 2)
            {
                // İlk 2 seviyede sadece toplama ve çıkarma, 1-20 arası sayılar
                a = random.Next(1, 21);
                b = random.Next(1, 21);
                operation = random.Next(2) == 0 ? "+" : "-";
            }
            else if (currentLevel <= 4)
            {
                // Seviye 3 ve 4: 1-50 arası sayılar
                a = random.Next(1, 21);
                b = random.Next(1, 21);
                operation = GetRandomOperation();
            }
            else if (currentLevel <= 6)
            {
                // Seviye 5 ve 6: 1-50 arası sayılar tüm işlemler
                a = random.Next(1, 51);
                b = random.Next(1, 51);
                operation = GetRandomOperation();
            }
            else
            {
                // Seviye 7 ve sonrasında tüm işlemler, sayılar 1-100 arasında
                a = random.Next(1, 101);
                b = random.Next(1, 101);
                operation = GetRandomOperation();
            }

            int result = CalculateResult(a, b, operation);

            return new Question
            {
                Text = $"{a} {operation} {b} = ?",
                CorrectAnswer = result
            };
        }

        private string GetRandomOperation()
        {
            string[] operations = { "+", "-", "*", "/" };
            return operations[random.Next(operations.Length)];
        }

        private int CalculateResult(int a, int b, string operation)
        {
            switch (operation)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "*": return a * b;
                case "/": return b != 0 ? a / b : 0; // Sıfıra bölmeyi önle
                default: return 0;
            }
        }



        private void DisplayQuestion()
        {
            int startIndex = (currentBlock - 1) * questionsPerBlock;
            int endIndex = startIndex + questionsPerBlock;

            for (int i = startIndex; i < endIndex && i < questions.Count; i++)
            {
                // Ekrandaki her bir soruyu ilgili label ve textbox'a aktar
                if (i - startIndex < 7) // Ekranda 5 label ve textbox olduğundan emin olun
                {
                    Label questionLabel = this.Controls.Find($"label{i - startIndex + 1}", true).FirstOrDefault() as Label;
                    TextBox answerTextBox = this.Controls.Find($"textBox{i - startIndex + 1}", true).FirstOrDefault() as TextBox;


                    if (questionLabel != null && answerTextBox != null)
                    {
                        questionLabel.Text = questions[i].Text;
                        answerTextBox.Clear();
                    }
                }
            }
        }



        private void ButtonAnswer_Click(object sender, EventArgs e)
        {
            CheckAnswersForBlock();
            string dogrular = correctAnswers.ToString();
            string yanlislar = wrongAnswers.ToString();

            MessageBox.Show("Doğru: " + dogrular + " Yanlış: " + yanlislar);

        }

        private void Reset()
        {
            // Doğru ve yanlış cevap sayısını sıfırla
            correctAnswers = 0;
            wrongAnswers = 0;
            passCount = 0;
        }


        private void ButtonPass_Click(object sender, EventArgs e)
        {
            if (passCount < 1) // Sadece 1 defa pas geçme hakkı
            {
                passCount++;
                currentBlock++; // Mevcut bloğu atla
                if (currentBlock > 4)
                {
                    EndLevel();
                }
                else
                {
                    DisplayQuestion(); // Bir sonraki bloğa geç
                }
            }
            else
            {
                MessageBox.Show("Pas hakkınızı kullandınız.");
            }
        }

        private void CheckAnswersForBlock()
        {
            // Blok başlangıç ve bitiş indekslerini belirleyelim
            int startIndex = (currentBlock - 1) * questionsPerBlock;
            int endIndex = Math.Min(startIndex + questionsPerBlock, questions.Count); // 5 soruya kadar

            // TextBox'ları bir listeye alalım
            List<TextBox> textBoxList = new List<TextBox>
    {
        textBox1,
        textBox2,
        textBox3,
        textBox4,
        textBox5
    };

            // Bloktaki soruları kontrol et
            for (int i = 0; i < endIndex - startIndex; i++) // Sadece 5 soruya kadar kontrol edelim
            {
                // Kullanıcının cevabını al
                string userAnswer = textBoxList[i].Text;
                // Doğru cevabı al
                string correctAnswer = questions[startIndex + i].CorrectAnswer.ToString();

                // Kullanıcının cevabını ve doğru cevabı gösterelim (debug amaçlı)
                MessageBox.Show($"Soru: {questions[startIndex + i].Text}, Kullanıcı Cevabı: {userAnswer}, Doğru Cevap: {correctAnswer}");

                // Cevabı kontrol et
                if (userAnswer == correctAnswer)
                {
                    correctAnswers++;
                }
                else
                {
                    wrongAnswers++;
                }
            }

            // Blok tamamlandığında bir sonraki bloğa geç
            currentBlock++;

            // Eğer tüm bloklar tamamlandıysa seviyeyi bitir
            if (currentBlock > 4) // 4 blok bittiğinde seviyeyi bitir
            {
                EndLevel();
            }
            else
            {
                // Sonraki blok sorularını görüntüle
                DisplayQuestion();
            }
        }



        private void EndLevel()
        {
            gameTimer.Stop(); // Zamanlayıcıyı durdur

            // Doğru cevap sayısına göre seviyeyi geçme durumu
            if (correctAnswers < 11) // Seviye geçme koşulu burada değiştirilebilir
            {
                MessageBox.Show("Üzgünüm, bir sonraki seviyeye geçemediniz. Tekrar deneyin.");
                StartNewLevel(); // Kullanıcı başarısız olduysa aynı seviyeyi tekrar başlat
                Reset();
            }
            else
            {
                MessageBox.Show($"Seviye {currentLevel} tamamlandı!");
                UpdateStarsDisplay(); // Yıldızları güncelle
                buttonNextLevel.Enabled = true; // Yeni seviyeye geçiş için butonu etkinleştir
                currentLevel++; // Bir sonraki seviyeye geç
                StartNewLevel(); // Kullanıcı başarılıysa yeni seviyeye geç
                Reset();
            }


        }
        private int CalculateStars()
        {
            if (correctAnswers >= 19) return 3;
            if (correctAnswers >= 16) return 2;
            if (correctAnswers >= 11) return 1;
            return 0;
        }



        
            private void UpdateStarsDisplay()
            {

            // Yıldızları seviyeye göre güncelle
            for (int i = 1; i < currentLevel; i++)
            {
                label6.Text += "★"; // Her seviye için bir yıldız ekle
            }

            // Eğer currentLevel 1 ise ilk seviye geçildiği için 1 yıldız gösterelim
            if (currentLevel == 1)
            {
                label6.Text = "★";
            }

            // Font boyutu ve stilini ayarlayalım
            label6.Font = new Font("Arial", 24, FontStyle.Bold);
            label6.ForeColor = Color.Gold; // Yıldızları altın sarısı renginde gösterelim
        }

        

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            UpdateTimeDisplay();

            if (remainingTime <= 0)
            {
                EndLevel();
            }
        }

        private void UpdateTimeDisplay()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            labelTimeRemaining.Text = $"Kalan Süre: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            progressBarTime.Value = Math.Max(0, Math.Min(100, (remainingTime * 100) / (300 + (currentLevel - 1) * 60)));
        }

        private void UpdateLevelDisplay()
        {
            labelLevel.Text = $"Seviye: {currentLevel}";
        }

        private void EnableGameControls()
        {
            textBox2.Enabled = true;
            buttonAnswer.Enabled = true;
            buttonPass.Enabled = true;
        }

        private void DisableGameControls()
        {
            textBox2.Enabled = false;
            buttonAnswer.Enabled = false;
            buttonPass.Enabled = false;
        }

        private void SaveProgress()
        {
            File.WriteAllText(SaveFilePath, currentLevel.ToString());
        }

        private void LoadProgress()
        {
            if (File.Exists(SaveFilePath))
            {
                currentLevel = int.Parse(File.ReadAllText(SaveFilePath));
            }
        }



        private void buttonNextLevel_Click(object sender, EventArgs e)
        {
            StartNewLevel();
            buttonAnswer.Enabled = true;
        }

        
    }



    public class Question
        {
            public string Text { get; set; }
            public int CorrectAnswer { get; set; }
        }
    }
