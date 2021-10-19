using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Diagnostics;

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                ButtonUpload.IsEnabled = false;
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
                    ButtonUpload.IsEnabled = true;
                }
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != simulationModel)
                simulationModel.TimeSpeed = (int)sliderSpeed.Maximum + 1 - (int)e.NewValue;
        }

        private void Button_Click_Upload_Protocol_Simulation(object sender, RoutedEventArgs e)
        {
            if (simulationModels.Count < 1)
            {
                MessageBox.Show("Нет данных для выгрузки");
                return;
            }

            ButtonUpload.IsEnabled = false;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Протокол моделирования"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel|*.xlsx"; // Filter files by extension

            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                var file = new FileInfo(dlg.FileName);

                if (file.Exists)
                {
                    bool flag = true;

                    try
                    {
                        file.Delete();
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                        flag = false;
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        MessageBox.Show(ex.Message);
                        flag = false;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(ex.Message);
                        flag = false;
                    }

                    if (flag == false)
                        return;
                }

                try
                {
                    using (var package = new ExcelPackage(file))
                    {
                        var sheet = package.Workbook.Worksheets.Add("Протокол моделирования");

                        sheet.Cells[1, 1].Value = "t(c)";
                        sheet.Cells[1, 2].Value = "Состояние основного канала";
                        sheet.Cells[1, 3].Value = "Состояние запасного канала";
                        sheet.Cells[1, 4].Value = "Число прерванных сообщений";
                        sheet.Cells[1, 5].Value = "Количество включений запасного канала";

                        int row = 2;
                        foreach (StateSimulationModel state in simulationModels)
                        {
                            sheet.Cells[row, 1].Value = state.TimeModel;
                            sheet.Cells[row, 2].Value = state.StateChannelMain;
                            sheet.Cells[row, 3].Value = state.StateChannelReserve;
                            sheet.Cells[row, 4].Value = state.CountMesIntercept;
                            sheet.Cells[row, 5].Value = state.CountInclusionReserveChannel;
                            row++;
                        }

                        sheet.Column(1).AutoFit();
                        sheet.Column(2).AutoFit();
                        sheet.Column(3).AutoFit();
                        sheet.Column(4).AutoFit();
                        sheet.Column(5).AutoFit();

                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                file = new FileInfo(file.FullName);

                if (file.Exists)
                {
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.Verb = "Open";
                    info.UseShellExecute = true;
                    info.CreateNoWindow = true;
                    info.WindowStyle = ProcessWindowStyle.Maximized;
                    info.FileName = file.Name;
                    info.WorkingDirectory = file.DirectoryName;
                    var verbs = info.Verbs;

                    if (verbs.Contains(info.Verb)) {
                        try
                        {
                            Process.Start(info);
                        }
                        catch (InvalidOperationException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (ArgumentNullException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (PlatformNotSupportedException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }

            ButtonUpload.IsEnabled = true;
        }
        #endregion

    }
}
