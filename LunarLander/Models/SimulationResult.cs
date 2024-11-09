using System;

namespace LunarLander.Models
{
    /// <summary>
    /// Представляет результаты симуляции посадки лунолета.
    /// </summary>
    public class SimulationResult
    {
        /// <summary>
        /// Получает или задает массив временных точек симуляции.
        /// </summary>
        public double[] Time { get; set; }

        /// <summary>
        /// Получает или задает массив высот на каждой временной точке.
        /// </summary>
        public double[] Height { get; set; }

        /// <summary>
        /// Получает или задает массив вертикальных скоростей на каждой временной точке.
        /// </summary>
        public double[] Velocity { get; set; }

        /// <summary>
        /// Получает или задает массив ускорений на каждой временной точке.
        /// </summary>
        public double[] Acceleration { get; set; }

        /// <summary>
        /// Получает или задает высоту, на которой был включен двигатель.
        /// </summary>
        public double LandingHeight { get; set; }

        /// <summary>
        /// Получает или задает вертикальную скорость при посадке.
        /// </summary>
        public double LandingVelocity { get; set; }

        /// <summary>
        /// Получает или задает значение, указывающее на успешность симуляции.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Получает или задает сообщение, связанное с результатом симуляции.
        /// </summary>
        public string Message { get; set; }
    }
}