using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private string[] colors = { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        private string[] secretCode;
        private List<Brush> ellipseColor = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.Orange, Brushes.White, Brushes.Green, Brushes.Blue };
        private string[] highscores = new string[15];
        private List<string> spelers = new List<string>();

        private int maxPogingen = 10;
        public int MaxPogingen
        {
            get => maxPogingen;
            set
            {
                maxPogingen = value;
                Title = $"Mastermind - Max Pogingen: {maxPogingen}";
            }
        }

        int attempts = 0;
        int countDown = 10;
        int totalScore = 100;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        // --------------------------- Game Initialization Methods ---------------------------

        private void InitializeGame()
        {
            Title = $"Mastermind - Beurt van: {spelers[0]}";
            Random number = new Random();
            secretCode = Enumerable.Range(0, 4)
                             .Select(_ => colors[number.Next(colors.Length)])
                             .ToArray();
            cheatCode.Text = string.Join(" ", secretCode);
            totalScore = 100;
            scoreLabel.Text = $"Speler: {spelers[0]} | Score: {totalScore} | Pogingen: {attempts}/{maxPogingen}";
            attempts = 0;
            countDown = 10;
            historyPanel.Children.Clear();
            ResetAllColors();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countDown--;

            if (countDown < 0)
            {
                timer.Stop();
                attempts++; // Zorg ervoor dat de poging omhoog gaat

                if (attempts >= maxPogingen)
                {
                    GameOver();
                    return;
                }

                MessageBox.Show("Poging kwijt!", "Tijd voorbij", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Update de titel en score om de nieuwe poging weer te geven
                UpdateTitle();
                UpdateScoreOnly();

                // Start een nieuwe poging met een nieuwe timer
                StartCountDown();
            }

            timerCounter.Text = countDown.ToString();
        }



        private void StartCountDown()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            countDown = 10; 
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }



        private void StopCountDown()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
        }



        private void GameOver()
        {
            string huidigeSpeler = spelers[0];
            string volgendeSpeler = spelers.Count > 1 ? spelers[1] : spelers[0];

            AddHighscore(huidigeSpeler, attempts, totalScore);

            MessageBoxResult result = MessageBox.Show(
                $"Game Over! De code was: {string.Join(" ", secretCode)}\n\nSpeler {huidigeSpeler}, wil je nog eens spelen?\nVolgende speler: {volgendeSpeler}",
                $"Game Over - Speler: {huidigeSpeler}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            spelers.Add(spelers[0]);
            spelers.RemoveAt(0);

            if (result == MessageBoxResult.Yes)
            {
                string nieuweSpeler = spelers[0];
                MessageBox.Show($"Speler {nieuweSpeler} is nu aan de beurt!", "Volgende Beurt", MessageBoxButton.OK, MessageBoxImage.Information);

                Title = $"Mastermind - Beurt van: {nieuweSpeler}";
                InitializeGame();
                StartCountDown();
            }
            else
            {
                MessageBox.Show("Het spel eindigt hier. Bedankt voor het spelen!", "Einde Spel", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }









        // --------------------------- New StartGame Method ---------------------------

        private void StartGame()
        {
            spelers.Clear();

            do
            {
                string naam = Microsoft.VisualBasic.Interaction.InputBox(
                    "Voer de naam van een speler in:",
                    "Nieuwe Speler",
                    "");

                if (string.IsNullOrWhiteSpace(naam))
                {
                    MessageBox.Show("Naam mag niet leeg zijn. Probeer opnieuw.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                spelers.Add(naam);

            } while (MessageBox.Show("Wil je nog een speler toevoegen?", "Meer Spelers", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);

            if (spelers.Count > 0)
            {
                MessageBox.Show($"Spelers: {string.Join(", ", spelers)}", "Spelerslijst", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Geen spelers toegevoegd. Het spel kan niet starten.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --------------------------- Player Interaction Methods ---------------------------

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };
            string[] selectedColors = ellipses.Select(e => GetColorName(e.Fill)).ToArray();

            if (selectedColors.Any(color => color == "Transparent"))
            {
                MessageBox.Show("Selecteer vier kleuren!", "Fout", MessageBoxButton.OK);
                return;
            }

            attempts++;
            UpdateTitle();

            if (attempts >= maxPogingen)
            {
                GameOver();
                return;
            }

            CheckGuess(selectedColors);
            UpdateScoreLabel(selectedColors);
            StartCountDown();
        }

        private void CheckGuess(string[] selectedColors)
        {
            int correctPosition = 0;
            int correctColor = 0;

            List<string> tempSecretCode = new List<string>(secretCode);
            List<string> tempPlayerGuess = new List<string>(selectedColors);

            List<Brush> feedbackBorders = new List<Brush>();

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] == tempSecretCode[i])
                {
                    correctPosition++;
                    feedbackBorders.Add(Brushes.DarkRed);
                    tempSecretCode[i] = null;
                    tempPlayerGuess[i] = null;
                }
                else
                {
                    feedbackBorders.Add(null);
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] != null && tempSecretCode.Contains(tempPlayerGuess[i]))
                {
                    int indexInSecretCode = tempSecretCode.IndexOf(tempPlayerGuess[i]);

                    if (indexInSecretCode >= 0)
                    {
                        feedbackBorders[i] = Brushes.Wheat;
                        tempSecretCode[indexInSecretCode] = null;
                    }
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (feedbackBorders[i] == null)
                {
                    feedbackBorders[i] = Brushes.Transparent;
                }
            }

            if (correctPosition == 4)
            {
                AddHighscore(spelers[0], attempts, totalScore);
                timer.Stop();

                
                spelers.Add(spelers[0]);
                spelers.RemoveAt(0);

                string nieuweSpeler = spelers[0];
                MessageBoxResult result = MessageBox.Show(
                    $"Proficiat! Speler {spelers[^1]} heeft de code gekraakt in {attempts} pogingen!\n\nSpeler {nieuweSpeler} is nu aan de beurt.\nDruk op ja om te starten?",
                    $"Speler {spelers[^1]} wint!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    InitializeGame();
                    StartCountDown();
                }
                else
                {
                    MessageBox.Show("Het spel eindigt hier. Bedankt voor het spelen!", "Einde Spel", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
            }
            AddAttemptToHistory(selectedColors, feedbackBorders);
        }

        private void AddAttemptToHistory(string[] selectedColors, List<Brush> feedbackBorders)
        {
            StackPanel attemptPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            for (int i = 0; i < selectedColors.Length; i++)
            {
                Ellipse colorBox = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = GetBrushFromColorName(selectedColors[i]),
                    StrokeThickness = 5,
                    Stroke = feedbackBorders[i]
                };
                attemptPanel.Children.Add(colorBox);
            }
            historyPanel.Children.Add(attemptPanel);
        }

        private void UpdateScoreLabel(string[] selectedColors)
        {
            int scorePenalty = 0;

            List<string> tempSecretCode = new List<string>(secretCode);
            List<string> tempPlayerGuess = new List<string>(selectedColors);

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] == tempSecretCode[i])
                {
                    tempSecretCode[i] = null;
                    tempPlayerGuess[i] = null;
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] != null && tempSecretCode.Contains(tempPlayerGuess[i]))
                {
                    scorePenalty += 1;
                    tempSecretCode[tempSecretCode.IndexOf(tempPlayerGuess[i])] = null;
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] != null)
                {
                    scorePenalty += 2;
                }
            }

            totalScore -= scorePenalty;
            if (totalScore < 0) totalScore = 0;
            scoreLabel.Text = $"Speler: {spelers[0]} | Score: {totalScore} | Pogingen: {attempts}/{maxPogingen}";
        }

        private void UpdateTitle()
        {
            this.Title = $"Mastermind - Beurt van: {spelers[0]} | Poging {attempts}/{maxPogingen}";
        }

        private void ResetAllColors()
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };

            foreach (Ellipse ellipse in ellipses)
            {
                ellipse.Fill = Brushes.Red;
                ellipse.Stroke = Brushes.Transparent;
            }
        }

        private string GetColorName(Brush brush)
        {
            if (brush == Brushes.Red) return "Red";
            if (brush == Brushes.Yellow) return "Yellow";
            if (brush == Brushes.Orange) return "Orange";
            if (brush == Brushes.White) return "White";
            if (brush == Brushes.Green) return "Green";
            if (brush == Brushes.Blue) return "Blue";
            return "Transparent";
        }

        private Brush GetBrushFromColorName(string colorName)
        {
            switch (colorName)
            {
                case "Red": return Brushes.Red;
                case "Yellow": return Brushes.Yellow;
                case "Orange": return Brushes.Orange;
                case "White": return Brushes.White;
                case "Green": return Brushes.Green;
                case "Blue": return Brushes.Blue;
                default: return Brushes.Transparent;
            }
        }

        private void CheatCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                if (cheatCode.Visibility == Visibility.Hidden)
                {
                    cheatCode.Visibility = Visibility.Visible;
                    Title = "Mastermind (DEBUG MODE)";
                }
                else
                {
                    cheatCode.Visibility = Visibility.Hidden;
                    Title = "Mastermind";
                }
            }
        }

        private void color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse clickedEllipse)
            {
                int currentColorIndex = ellipseColor.IndexOf(clickedEllipse.Fill);
                int nextColorIndex = (currentColorIndex + 1) % ellipseColor.Count;
                clickedEllipse.Fill = ellipseColor[nextColorIndex];
            }
        }

        private void StartNewGame_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            StartGame();

            if (spelers.Count > 0)
            {
                InitializeGame();
                StartCountDown();
            }
        }

        private void SortHighscores()
        {
            highscores = highscores
                .Where(h => h != null)
                .OrderByDescending(h => int.Parse(h.Split('-')[2].Trim().Split('/')[0]))
                .Concat(Enumerable.Repeat<string>(null, highscores.Length))
                .Take(highscores.Length)
                .ToArray();
        }

        private void AddHighscore(string spelerNaam, int pogingen, int score)
        {
            string nieuweScore = $"{spelerNaam} - {pogingen} pogingen - {score}/100";

            for (int i = 0; i < highscores.Length; i++)
            {
                if (highscores[i] == null)
                {
                    highscores[i] = nieuweScore;
                    SortHighscores();
                    return;
                }
            }

            highscores[highscores.Length - 1] = nieuweScore;
            SortHighscores();
        }

        private void HighScores_Click(object sender, RoutedEventArgs e)
        {
            if (highscores.All(h => h == null))
            {
                MessageBox.Show("Er zijn nog geen highscores!", "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string highscoreTekst = string.Join("\n", highscores.Where(h => h != null));
            MessageBox.Show(highscoreTekst, "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult hintType = MessageBox.Show(
                "Welke hint wil je kopen?\n" +
                "Ja: Een juiste kleur (-15 strafpunten)\n" +
                "Nee: Een juiste kleur op de juiste plaats (-25 strafpunten)",
                "Hint Kopen",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (hintType == MessageBoxResult.Cancel)
            {
               
                return;
            }

            if (hintType == MessageBoxResult.Yes)
            {
                List<string> beschikbareKleuren = secretCode.Distinct().ToList();
                Random random = new Random();
                string juisteKleur = beschikbareKleuren[random.Next(beschikbareKleuren.Count)];

                MessageBox.Show($"Een van de kleuren in de geheime code is: {juisteKleur}", "Hint: Juiste Kleur", MessageBoxButton.OK, MessageBoxImage.Information);

                totalScore -= 15;
            }
            else if (hintType == MessageBoxResult.No)
            {
                List<int> beschikbarePosities = Enumerable.Range(0, secretCode.Length).ToList();
                Random random = new Random();
                int juistePositie = beschikbarePosities[random.Next(beschikbarePosities.Count)];

                MessageBox.Show($"De kleur op positie {juistePositie + 1} is: {secretCode[juistePositie]}", "Hint: Juiste Kleur op de Juiste Plaats", MessageBoxButton.OK, MessageBoxImage.Information);

                totalScore -= 25;
            }

            if (totalScore < 0)
            {
                totalScore = 0;
            }

            UpdateScoreLabel(new string[0]);
        }

        private void UpdateScoreOnly()
        {
            scoreLabel.Text = $"Speler: {spelers[0]} | Score: {totalScore} | Pogingen: {attempts}/{maxPogingen}";
        }

    }
}
