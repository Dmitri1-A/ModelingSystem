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

        private ObservableCollection<StateSimulationModel> simulationModels;

        private SimulationModel simulationModel = null;

        public MainWindow()
        {
            simulationModels = new ObservableCollection<StateSimulationModel>();

            InitializeComponent();

            lvInfo.ItemsSource = simulationModels;
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

            rectangleChannelMain.Fill = model.StateChannelMain switch
            {
                SimulationModel.StateChannel.Enabled => null,
                SimulationModel.StateChannel.Transfer => Brushes.GreenYellow,
                SimulationModel.StateChannel.Broken => Brushes.Red,
                SimulationModel.StateChannel.Disabled => null,
                _ => null,
            };

            rectangleChannelReserve.Fill = model.StateChannelReserve switch
            {
                SimulationModel.StateChannel.Enabled => null,
                SimulationModel.StateChannel.Transfer => Brushes.GreenYellow,
                SimulationModel.StateChannel.Disabled => Brushes.Gray,
                SimulationModel.StateChannel.Broken => null,
                _ => null,
            };

            stackMessages.Children.Clear();
            for (int i = 0; i < model.BufferSize; i++)
            {
                Rectangle mess = new Rectangle();
                mess.Height = 30;
                mess.Width = 40;
                mess.Fill = Brushes.Green;
                mess.Margin = new Thickness(0, 0, 0, 5);
                mess.HorizontalAlignment = HorizontalAlignment.Center;

                stackMessages.Children.Add(mess);
            }

            int countElem = stackMessagesSuccess.Children.Count;
            int countNewElem = model.CountMesTransferred - countElem;

            for (int i = 0; i < countNewElem; i++)
            {
                Label mess = new();
                mess.Height = 25;
                mess.Width = 40;
                mess.Background = Brushes.Green;
                mess.Margin = new Thickness(0, 0, 0, 5);
                mess.HorizontalAlignment = HorizontalAlignment.Stretch;
                mess.VerticalAlignment = VerticalAlignment.Stretch;
                mess.VerticalContentAlignment = VerticalAlignment.Center;
                mess.HorizontalContentAlignment = HorizontalAlignment.Center;
                mess.Content = (countElem++).ToString();
                mess.Foreground = Brushes.White;

                stackMessagesSuccess.Children.Add(mess);
            }

            simulationModels.Add(new StateSimulationModel(model.TimeModel,
                model.CountMesIntercept, model.CountInclusionReserveChannel,
                model.StateChannelMain, model.StateChannelReserve));
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
                simulationModel.TimeSpeed = (int)sliderSpeed.Maximum + 1 - (int)sliderSpeed.Value;

                Button_Start.IsEnabled = false;
                progressBar.Maximum = simulationModel.TimeEnd;
                progressBar.Minimum = 0;
                simulationModels.Clear();
                stackMessagesSuccess.Children.Clear();

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
                simulationModel.TimeSpeed = (int)sliderSpeed.Maximum + 1 - (int)e.NewValue;
        }
    }
}
