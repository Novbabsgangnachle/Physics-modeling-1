using System;
using System.Collections.Generic;
using System.Linq;
using LunarLander.Models;

namespace LunarLander.Services
{
    public class LunarLanderSimulator
    {
        // Константы
        private const double Gravity = 1.62; // м/с² - ускорение свободного падения на Луне
        private const double MassEmpty = 2150; // кг - масса корабля без топлива (включая пилота и скафандр)
        private const double InitialFuelMass = 150; // кг - начальная масса топлива
        private const double FuelConsumptionRate = 15; // кг/с - расход топлива двигателем
        private const double ExhaustVelocity = 3660; // м/с - скорость истечения продуктов сгорания двигателя
        private const double InitialHeight = 2300; // м - начальная высота
        private const double InitialVelocity = 20; // м/с - начальная вертикальная скорость (вниз положительная)
        private const double MaxLandingSpeed = 3; // м/с - максимальная допустимая скорость при посадке
        private const double DeltaTime = 0.01; // с - шаг по времени для симуляции
        
        public SimulationResult RunSimulation()
        {
            // Моделирование свободного падения
            var freeFallResult = SimulateFreeFall(InitialHeight, InitialVelocity, Gravity, DeltaTime);
            double[] freeFallTime = freeFallResult.Time;
            double[] freeFallHeight = freeFallResult.Height;
            double[] freeFallVelocity = freeFallResult.Velocity;

            bool engineStarted = false;
            double[] totalTime = null;
            double[] totalHeight = null;
            double[] totalVelocity = null;
            double[] totalAcceleration = null;
            double ignitionHeight = 0;
            double ignitionVelocity = 0;

            // Перебор высот свободного падения в обратном порядке для определения момента включения двигателя
            for (int idx = freeFallTime.Length - 1; idx >= 0; idx--)
            {
                if (freeFallHeight[idx] <= 0)
                    continue; // Пропустить точки, где высота уже достигла или ниже 0

                ignitionHeight = freeFallHeight[idx];
                ignitionVelocity = freeFallVelocity[idx];
                double initialMass = MassEmpty + InitialFuelMass;

                // Моделирование управляемого снижения с включенным двигателем
                var poweredDescentResult = SimulatePoweredDescent(
                    ignitionHeight,
                    ignitionVelocity,
                    initialMass,
                    Gravity,
                    ExhaustVelocity,
                    FuelConsumptionRate,
                    MaxLandingSpeed,
                    DeltaTime);

                double[] poweredDescentTime = poweredDescentResult.Time;
                double[] poweredDescentHeight = poweredDescentResult.Height;
                double[] poweredDescentVelocity = poweredDescentResult.Velocity;
                double[] poweredDescentAcceleration = poweredDescentResult.Acceleration;

                // Проверка, достигла ли конечная скорость допустимого значения
                if (poweredDescentVelocity.Last() <= MaxLandingSpeed)
                {
                    engineStarted = true;

                    // Объединение результатов свободного падения и управляемого снижения
                    totalTime = freeFallTime.Take(idx + 1)
                                            .Concat(poweredDescentTime.Skip(1).Select(tp => freeFallTime[idx] + tp))
                                            .ToArray();
                    totalHeight = freeFallHeight.Take(idx + 1)
                                                .Concat(poweredDescentHeight.Skip(1))
                                                .ToArray();
                    totalVelocity = freeFallVelocity.Take(idx + 1)
                                                    .Concat(poweredDescentVelocity.Skip(1))
                                                    .ToArray();
                    double[] freeFallAcceleration = Enumerable.Repeat(Gravity, idx + 1).ToArray();
                    totalAcceleration = freeFallAcceleration.Concat(poweredDescentAcceleration).ToArray();

                    break; // Выход из цикла после успешного включения двигателя
                }
            }

            var simulationResult = new SimulationResult();

            if (!engineStarted)
            {
                // Если не удалось найти подходящую высоту для включения двигателя
                simulationResult.IsSuccessful = false;
                simulationResult.Message = "Не удалось найти подходящую высоту включения двигателя для безопасной посадки.";
            }
            else
            {
                // Если двигатель успешно включен, сохраняем результаты
                simulationResult.IsSuccessful = true;
                simulationResult.LandingHeight = ignitionHeight;
                simulationResult.LandingVelocity = totalVelocity.Last();
                simulationResult.Time = totalTime;
                simulationResult.Height = totalHeight;
                simulationResult.Velocity = totalVelocity;
                simulationResult.Acceleration = totalAcceleration;
            }

            return simulationResult;
        }

        /// <summary>
        /// Моделирует фазу свободного падения лунолета.
        /// </summary>
        /// <param name="initialHeight">Начальная высота (м).</param>
        /// <param name="initialVelocity">Начальная вертикальная скорость (м/с).</param>
        /// <param name="gravity">Ускорение свободного падения (м/с²).</param>
        /// <param name="deltaTime">Шаг по времени (с).</param>
        /// <returns>Кортеж с массивами времени, высоты и скорости.</returns>
        private (double[] Time, double[] Height, double[] Velocity) SimulateFreeFall(double initialHeight, double initialVelocity, double gravity, double deltaTime)
        {
            var timeList = new List<double> { 0 };
            var heightList = new List<double> { initialHeight };
            var velocityList = new List<double> { initialVelocity };

            // Цикл моделирования свободного падения до достижения поверхности Луны
            while (heightList.Last() > 0)
            {
                double newVelocity = velocityList.Last() + gravity * deltaTime;
                double newHeight = heightList.Last() - velocityList.Last() * deltaTime - 0.5 * gravity * Math.Pow(deltaTime, 2);
                double newTime = timeList.Last() + deltaTime;

                timeList.Add(newTime);
                velocityList.Add(newVelocity);
                heightList.Add(newHeight > 0 ? newHeight : 0); // Обеспечить, чтобы высота не была отрицательной
            }

            return (timeList.ToArray(), heightList.ToArray(), velocityList.ToArray());
        }

        /// <summary>
        /// Моделирует фазу управляемого снижения лунолета с включенным двигателем.
        /// </summary>
        /// <param name="startHeight">Высота включения двигателя (м).</param>
        /// <param name="startVelocity">Скорость при включении двигателя (м/с).</param>
        /// <param name="initialMass">Начальная масса корабля с топливом (кг).</param>
        /// <param name="gravity">Ускорение свободного падения (м/с²).</param>
        /// <param name="exhaustVelocity">Скорость истечения продуктов сгорания двигателя (м/с).</param>
        /// <param name="fuelRate">Расход топлива двигателем (кг/с).</param>
        /// <param name="maxSpeed">Максимальная допустимая скорость при посадке (м/с).</param>
        /// <param name="deltaTime">Шаг по времени (с).</param>
        /// <returns>Кортеж с массивами времени, высоты, скорости и ускорения.</returns>
        private (double[] Time, double[] Height, double[] Velocity, double[] Acceleration) SimulatePoweredDescent(
            double startHeight,
            double startVelocity,
            double initialMass,
            double gravity,
            double exhaustVelocity,
            double fuelRate,
            double maxSpeed,
            double deltaTime)
        {
            var timeList = new List<double> { 0 };
            var heightList = new List<double> { startHeight };
            var velocityList = new List<double> { startVelocity };
            var accelerationList = new List<double>();

            double currentMass = initialMass;

            // Цикл моделирования управляемого снижения до достижения поверхности или необходимой скорости
            while (heightList.Last() > 0 && currentMass > MassEmpty)
            {
                // Расчет ускорения от двигателя
                double thrustAcceleration = -exhaustVelocity * fuelRate / currentMass;
                double totalAcceleration = thrustAcceleration + gravity;
                accelerationList.Add(totalAcceleration);

                // Обновление скорости и высоты
                double newVelocity = velocityList.Last() + totalAcceleration * deltaTime;
                double newHeight = heightList.Last() - velocityList.Last() * deltaTime - 0.5 * totalAcceleration * Math.Pow(deltaTime, 2);
                double newTime = timeList.Last() + deltaTime;

                timeList.Add(newTime);
                velocityList.Add(newVelocity);
                heightList.Add(newHeight > 0 ? newHeight : 0); // Обеспечить, чтобы высота не была отрицательной
                currentMass -= fuelRate * deltaTime;
            }

            return (timeList.ToArray(), heightList.ToArray(), velocityList.ToArray(), accelerationList.ToArray());
        }
    }
}
