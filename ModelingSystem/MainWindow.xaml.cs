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

        private SimulationModel simulationModel = null;

        public MainWindow()
        {
            Indicators = new ObservableCollection<Indicator>()
            { 
                new Indicator {InterruptedMessageN = 1, ReserveChannelN = 3, TimeN = 0 },
                new Indicator {InterruptedMessageN = 2, ReserveChannelN = 5, TimeN = 1 },
            };

            InitializeComponent();

            lvIndicators.ItemsSource = Indicators;
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
        /// Изменяет изображение на экране, в соответствии с имитационной моделью
        /// </summary>
        private void ChangeStateScene(SimulationModel model)
        {
            progressBar.Value = model.TimeModel;

            switch (model.StateChannelMain)
            {
                case SimulationModel.StateChannel.Enabled:
                    rectangleMainChannel.Fill = null;
                    break;
                case SimulationModel.StateChannel.Transfer:
                    rectangleMainChannel.Fill = Brushes.GreenYellow;
                    break;
                case SimulationModel.StateChannel.Broken:
                    rectangleMainChannel.Fill = Brushes.Red;
                    break;
                default:
                    rectangleMainChannel.Fill = null;
                    break;
            }
        }

        #region Обработчики событий

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            tokenSource?.Cancel();
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

                simulationModel = new SimulationModel(T, t1, t2, t3, t4, t5,
                    dispatcher: Dispatcher, action: ChangeStateScene);
                simulationModel.TimeSpeed = (int)sliderSpeed.Value;

                Button_Start.IsEnabled = false;
                progressBar.Maximum = simulationModel.TimeEnd;
                progressBar.Minimum = 0;

                tokenSource = new CancellationTokenSource();

                Task task = Task.Factory.StartNew(
                    () => simulationModel.RunSimulation(tokenSource.Token),
                    tokenSource.Token);

                try
                {
                    await task;
                }
                finally
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                    simulationModel = null;
                    Button_Start.IsEnabled = true;
                }
            }
        }
        #endregion

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != simulationModel)
                simulationModel.TimeSpeed = (int)e.NewValue;
        }
    }
}
