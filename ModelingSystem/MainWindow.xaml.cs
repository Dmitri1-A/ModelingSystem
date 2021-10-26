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

        private ManualResetEvent _wait = null;

        private bool isPause = false;

        private ObservableCollection<SimulationModel> simulationModels;

        private SimulationModel simulationModel = null;

        public MainWindow()
        {
            simulationModels = new ObservableCollection<SimulationModel>();

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

            if (!int.TryParse(TextboxBufferSize.Text, out num))
            {
                message += "Размер буфера должен быть целым числом\n";
                bRes = false;
            }
            else
            {
                if (num < 0)
                {
                    message += "Размер буфера должен быть неотрицательным числом\n";
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
            double h = borderMessages.Height;
            h = model.BufferCapacity > 0 ? h / model.BufferCapacity : h;
            for (int i = 0; i < model.BufferSize; i++)
            {
                Rectangle mess = new Rectangle();
                mess.Height = h - 2;
                mess.Width = 40;
                mess.Fill = Brushes.Green;
                mess.Margin = new Thickness(0, 0, 0, 2);
                mess.HorizontalAlignment = HorizontalAlignment.Center;

                stackMessages.Children.Add(mess);
            }

            LabelCountMessageTransfered.Content = model.CountMesTransferred;

            simulationModels.Add(model);
        }

        #region Обработчики событий

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            _wait?.Set();
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
                int bufSize = int.Parse(TextboxBufferSize.Text);

                simulationModel = new SimulationModel(T, t1, t2, t3, t4, t5,
                    dispatcher: Dispatcher, action: ChangeStateScene, capacity: bufSize);
                simulationModel.TimeSpeed = (int)sliderSpeed.Maximum + 1 - (int)sliderSpeed.Value;

                Button_Start.IsEnabled = false;
                ButtonUpload.IsEnabled = false;
                Button_TogglePlay.IsEnabled = true;
                Button_TogglePlay.Content = "Стоп";
                progressBar.Maximum = simulationModel.TimeEnd;
                progressBar.Minimum = 0;
                simulationModels.Clear();
                isPause = false;

                _wait = new ManualResetEvent(true);

                tokenSource = new CancellationTokenSource();

                Task task = Task.Factory.StartNew(
                    () => simulationModel.RunSimulation(tokenSource.Token, _wait),
                    tokenSource.Token);

                try
                {
                    await task;
                }
                finally
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                    _wait.Dispose();
                    _wait = null;
                    simulationModel = null;
                    Button_Start.IsEnabled = true;
                    ButtonUpload.IsEnabled = true;
                    Button_TogglePlay.IsEnabled = false;
                    Button_TogglePlay.Content = "Стоп";
                    isPause = false;
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
            Action<FileInfo> uploadDataToExcel = (FileInfo fileInfo) =>
            {
                bool flag = true;

                try
                {
                    using (var package = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Протокол моделирования");

                        sheet.Cells[1, 1].Value = "t(c)";
                        sheet.Cells[1, 2].Value = "Основной канал";
                        sheet.Cells[1, 3].Value = "Запасной канал";
                        sheet.Cells[1, 4].Value = "Сообщений в буфере";
                        sheet.Cells[1, 5].Value = "Число прерванных сообщений";
                        sheet.Cells[1, 6].Value = "Количество включений запасного канала";
                        sheet.Cells[1, 7].Value = "Сообщений отброшено";
                        sheet.Cells[1, 8].Value = "Сообщение пришло";

                        int row = 2;
                        foreach (SimulationModel state in simulationModels)
                        {
                            sheet.Cells[row, 1].Value = state.TimeModel;
                            sheet.Cells[row, 2].Value = state.StateChannelMain;
                            sheet.Cells[row, 3].Value = state.StateChannelReserve;
                            sheet.Cells[row, 4].Value = state.BufferSize;
                            sheet.Cells[row, 5].Value = state.CountMesIntercept;
                            sheet.Cells[row, 6].Value = state.CountInclusionReserveChannel;
                            sheet.Cells[row, 7].Value = state.CountMesDiscarded;
                            sheet.Cells[row, 8].Value = state.MessageWasIn;
                            row++;
                        }

                        for (int i = 1; i <= 8; i++)
                        {
                            sheet.Column(i).AutoFit();
                        }

                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    flag = false;
                }

                if (!flag)
                    return;

                var resultMsg = MessageBox.Show("Открыть файл протокола моделирования?",
                    "Открыть?",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (MessageBoxResult.OK == resultMsg)
                {
                    var file = new FileInfo(fileInfo.FullName);

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

                        // Открыть файл xlsx
                        if (verbs.Contains(info.Verb))
                        {
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
            };

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
                bool flag = true;

                if (file.Exists)
                {
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
}

                if (flag)
                    uploadDataToExcel(file);
            }

            ButtonUpload.IsEnabled = true;
        }

        private void Button_Click_TogglePlay(object sender, RoutedEventArgs e)
        {
            // Если поток симуляции не заблокирован
            if (!isPause)
            {
                // Установить событие ManualResetEvent
                // в несигнальное состояние (блокирует поток)
                try
                {
                    _ = (_wait?.Reset());
                }
                catch (ObjectDisposedException)
                {
                    _ = MessageBox.Show("Не удалось остановить симуляцию",
                        "Программная ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Button_TogglePlay.Content = "Продолжить";
                    ButtonUpload.IsEnabled = true;
                }
            }
            else
            {
                try
                {
                    _ = (_wait?.Set());
                }
                catch (ObjectDisposedException)
                {
                    _ = MessageBox.Show("Не удалось продолжить симуляцию",
                        "Программная ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Button_TogglePlay.Content = "Стоп";
                    ButtonUpload.IsEnabled = false;
                }
            }

            isPause = !isPause;
        }
        #endregion
    }
}
