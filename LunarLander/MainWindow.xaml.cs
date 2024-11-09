using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using LunarLander.Models;
using LunarLander.Services;

namespace LunarLander
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlotModel _heightPlotModel;
        public PlotModel HeightPlotModel
        {
            get => _heightPlotModel;
            set
            {
                _heightPlotModel = value;
                OnPropertyChanged(nameof(HeightPlotModel));
            }
        }

        private PlotModel _speedPlotModel;
        public PlotModel SpeedPlotModel
        {
            get => _speedPlotModel;
            set
            {
                _speedPlotModel = value;
                OnPropertyChanged(nameof(SpeedPlotModel));
            }
        }

        private PlotModel _accelerationPlotModel;

        public PlotModel AccelerationPlotModel
        {
            get => _accelerationPlotModel;
            set
            {
                _accelerationPlotModel = value;
                OnPropertyChanged(nameof(AccelerationPlotModel));
            }
        }

        private string _resultText;
        public string ResultText
        {
            get => _resultText;
            set
            {
                _resultText = value;
                OnPropertyChanged(nameof(ResultText));
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PerformSimulation(); // Автоматический запуск симуляции при старте приложения
        }

        /// <summary>
        /// Событие, возникающее при изменении свойства.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывает событие <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">Имя измененного свойства.</param>
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Выполняет симуляцию и обновляет графики и результаты.
        /// </summary>
        private void PerformSimulation()
        {
            var simulator = new LunarLanderSimulator();
            SimulationResult simulationResult = simulator.RunSimulation();

            if (!simulationResult.IsSuccessful)
            {
                ResultText = simulationResult.Message;
            }
            else
            {
                HeightPlotModel = CreatePlotModel(
                    title: "Высота",
                    xAxisTitle: "Время, с",
                    yAxisTitle: "Высота, м",
                    xValues: simulationResult.Time,
                    yValues: simulationResult.Height,
                    color: OxyColors.Blue);

                SpeedPlotModel = CreatePlotModel(
                    title: "Скорость",
                    xAxisTitle: "Время, с",
                    yAxisTitle: "Скорость, м/с",
                    xValues: simulationResult.Time,
                    yValues: simulationResult.Velocity,
                    color: OxyColors.Orange);

                AccelerationPlotModel = CreatePlotModel(
                    title: "Ускорение",
                    xAxisTitle: "Время, с",
                    yAxisTitle: "Ускорение, м/с²",
                    xValues: simulationResult.Time.Take(simulationResult.Time.Length - 1).ToArray(),
                    yValues: simulationResult.Acceleration,
                    color: OxyColors.Green);

                ResultText = $"Подходящая высота для включения двигателя: {simulationResult.LandingHeight:F2} м\n" +
                             $"Вертикальная скорость при посадке: {simulationResult.LandingVelocity:F2} м/с";
            }
        }
        
        private PlotModel CreatePlotModel(string title, string xAxisTitle, string yAxisTitle, double[] xValues, double[] yValues, OxyColor color)
        {
            var model = new PlotModel { Title = title };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = xAxisTitle,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = yAxisTitle,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            var series = new LineSeries
            {
                Title = title,
                Color = color,
                MarkerType = MarkerType.None,
                StrokeThickness = 2
            };

            for (int i = 0; i < xValues.Length; i++)
            {
                series.Points.Add(new DataPoint(xValues[i], yValues[i]));
            }

            model.Series.Add(series);
            return model;
        }


        private void RunSimulation_Click(object sender, RoutedEventArgs e)
        {
            PerformSimulation(); 
        }

        private void ResetSimulation_Click(object sender, RoutedEventArgs e)
        {
            HeightPlotModel = null;
            SpeedPlotModel = null;
            AccelerationPlotModel = null;
            ResultText = "Симуляция сброшена. Нажмите 'Запустить симуляцию' для начала.";
        }
    }
}
