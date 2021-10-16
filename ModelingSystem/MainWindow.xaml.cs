using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModelingSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource tokenSource = null;
        private ObservableCollection<Indicator> Indicators;
        private int countMessage = 0;

        public MainWindow()
        {
            Indicators = new ObservableCollection<Indicator>()
            { 
                new Indicator {NumInterruptMessages = 1, NumSpareChannel = 3, NumTime = 0 },
                new Indicator {NumInterruptMessages = 2, NumSpareChannel = 5, NumTime = 1 },
            };

            InitializeComponent();

            lvIndicators.ItemsSource = Indicators;
        }

        /// <summary>
        /// Запускает симуляцию модели в отдельном потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (IsValidField())
            {
                int t1 = int.Parse(TextboxT1.Text);
                int t2 = int.Parse(TextboxT2.Text);
                int t3 = int.Parse(TextboxT3.Text);
                int t4 = int.Parse(TextboxT4.Text);
                int t5 = int.Parse(TextboxT5.Text);
                int T = int.Parse(TextboxT.Text);

                MessageCount.Content = "0";
                Button_Start.IsEnabled = false;

                tokenSource = new CancellationTokenSource();
                Task task = Task.Factory.StartNew(RunSimulation, tokenSource.Token);
                
                try
                {
                    await task;
                }
                catch (OperationCanceledException ex)
                {
                    MessageBox.Show($"{nameof(Exception)} thrown with message: {ex.Message}");
                }
                finally
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                    Button_Start.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Проводит валидацию полей
        /// </summary>
        /// <returns></returns>
        private bool IsValidField()
        {
            bool bRes = true;
            string message = "";
            int num;

            if (!int.TryParse(TextboxT1.Text, out num))
            {
                message += "Значение t1 должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение t1 должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (!int.TryParse(TextboxT2.Text, out num))
            {
                message += "Значение t2 должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение t2 должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (!int.TryParse(TextboxT3.Text, out num))
            {
                message += "Значение t3 должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение t3 должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (!int.TryParse(TextboxT4.Text, out num))
            {
                message += "Значение t4 должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение t4 должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (!int.TryParse(TextboxT5.Text, out num))
            {
                message += "Значение t5 должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение t5 должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (!int.TryParse(TextboxT.Text, out num))
            {
                message += "Значение T должно быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 1)
                {
                    message += "Значение T должно быть больше 0\n";
                    bRes = false;
                }
            }

            if (message.Length > 0)
            {
                MessageBox.Show(message, "Неверные данные", MessageBoxButton.OK);
            }

            return bRes;
        }

        /// <summary>
        /// Код имитационной модели
        /// </summary>
        private void RunSimulation()
        {
            CancellationToken token = tokenSource.Token;
            token.ThrowIfCancellationRequested();

            countMessage = 10;
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke((Action)ChangeStateScene);
            }
            else
                ChangeStateScene();

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            for  (int i = 0; i < 10; i++)
            {
                Thread.Sleep(200);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
            }

            countMessage = 0;

            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke((Action)ChangeStateScene);
            }
            else
                ChangeStateScene();
        }

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            tokenSource?.Cancel();
        }

        private void ChangeStateScene()
        {
            MessageCount.Content = countMessage;
        }
    }
}
