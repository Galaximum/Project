using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace Test
{
    class GetCoordinates
    {
        // Начальные и конечные координаты Участков.
        static readonly List<List<double>>[] coordinatesRegions =
        {
                  // Координаты для 1 участка.
              new List<List<double>>
              {
                new List<double>{ 55.724969, 37.554993 },
                new List<double>{ 55.724470, 37.556503 }
              },
                  // Координаты для 2 участка.
              new List<List<double>>
              {
                new List<double>{ 55.724930, 37.554938 },
                new List<double>{ 55.724446, 37.556484 }
              },
                  // Координаты для 3 участка.
              new List<List<double>>
              {
                new List<double>{ 55.724973, 37.554901 },
                new List<double>{ 55.725683, 37.552744 }
              },
                  // Координаты для 4 участка.
              new List<List<double>>
              {
                new List<double>{ 55.724952, 37.554873 },
                new List<double>{ 55.725663, 37.552648 }
              },
                  // Координаты для 5 участка.
              new List<List<double>>
              {
                new List<double>{ 55.723956, 37.555684 },
                new List<double>{ 55.724711, 37.553155 }
              },
                  // Координаты для 6 участка.
              new List<List<double>>
              {
                new List<double>{ 55.723886, 37.555611 },
                new List<double>{ 55.724667, 37.553106 }
              },
                  // Координаты для 7 участка.
              new List<List<double>>
              {
                new List<double>{ 55.724722, 37.553061 },
                new List<double>{  55.725069, 37.552096 }
              },
                  // Координаты для 8 участка.
              new List<List<double>>
              {
                new List<double>{ 55.723615, 37.555339 },
                new List<double>{ 55.724429, 37.552837 }
              },
                  // Координаты для 9 участка.
              new List<List<double>>
              {
                new List<double>{ 55.725722,37.552591  },
                new List<double>{ 55.725800, 37.552374 }
              },
                  // Координаты для 10 участка.
              new List<List<double>>
              {
                new List<double>{ 55.725702,37.552557  },
                new List<double>{ 55.725791, 37.552289 }
              },
                  // Координаты для 11 участка.
              new List<List<double>>
              {
                new List<double>{ 55.725341,  37.552158 },
                new List<double>{ 55.725480, 37.551748 }
              },
        };

        // Количество рядов на участках.
        static readonly int[] countOfRowsRegions =
        {
             // Количество рядов на 1 участке 
            40,
             // Количество рядов на 2 участке 
            41,
             // Количество рядов на 3 участке
            58,
             // Количество рядов на 4 участке 
            61,
             // Количество рядов на 5 участке 
            44,
             // Количество рядов на 6 участке 
            40,
             // Количество рядов на 7 участке 
            22,
             // Количество рядов на 8 участке 
            46,
             // Количество рядов на 9 участке 
            9,
             // Количество рядов на 10 участке 
            10,
             // Количество рядов на 11 участке 
            8
        };

        // Первый участок 41-47 ряды.
        // Отдельно вынесенны из за того, что они расположены по другому, относительно других рядов.
        static readonly Dictionary<int, LatLng> coordinatesRegion1Row41_47 = new Dictionary<int, LatLng>
        {
            {41, new LatLng(55.724726, 37.555994)},
            {42, new LatLng(55.724601, 37.556588)},
            {43, new LatLng(55.724645, 37.556625)},
            {44, new LatLng(55.724680, 37.556662)},
            {45, new LatLng(55.724718, 37.556679)},
            {46, new LatLng(55.724754, 37.556713)},
            {47, new LatLng(55.724779, 37.556733)},
        };

        // Участок 3. 59-65 ряды.
        // Отдельно вынесенны из за того, что они расположены по другому, относительно других рядов.
        static readonly Dictionary<int, LatLng> coordinatesRegion3Row59_65 = new Dictionary<int, LatLng>
        {
            {59, new LatLng(55.725301, 37.554152)},
            {60, new LatLng(55.725097, 37.554977)},
            {61, new LatLng(55.725668, 37.553307)},
            {62, new LatLng(55.725703, 37.553346)},
            {63, new LatLng(55.725731, 37.553386)},
            {64, new LatLng(55.725764, 37.553445)},
            {65, new LatLng(55.725799, 37.553468)},
        };

        static readonly List<List<double>> coordinatesOldColumbary = new List<List<double>>
        {
                new List<double>{ 55.723396, 37.554889 },
                new List<double>{ 55.723791, 37.553929 }
        };

        static readonly List<List<double>> coordinatesNewColumbary = new List<List<double>>
        {
                new List<double>{ 55.723791, 37.553929 },
                new List<double>{  55.723904, 37.553738 }
        };


        // Количество секция в старом и новом Колумбариях.
        static readonly int countOfSectionOldColumbary = 115;
        static readonly int countOfSectionNewColumbary = 37;


        /// <summary>
        /// Метод,предназначенный для высчитывания координат,того или иного места,поддающегося на вход.
        /// </summary>
        /// <param name="type">Тип места</param>
        /// <param name="info">Информация о месте</param>
        /// <returns>Координаты в формате LatLng</returns>
        public static LatLng GetCoordinatesFromInfo(TypePlaces type, params int[] info)
        {
            if (type == TypePlaces.Основное_Кладбище)
            {
                switch (info[0])
                {
                    case 1:
                        {
                            // Если ряд 40 или меньше, то считаем координаты, если нет, то забираем константные координаты.
                            if (info[1] <= 40)
                                return CalculatedCoordinates(info[1], countOfRowsRegions[info[0] - 1], coordinatesRegions[info[0] - 1]);
                            else
                                return coordinatesRegion1Row41_47[info[1]];
                        }
                    case 3:
                        {
                            // Если ряд 58 или меньше, то считаем координаты, если нет, то забираем константные координаты.
                            if (info[1] <= 58)
                                return CalculatedCoordinates(info[1], countOfRowsRegions[info[0] - 1], coordinatesRegions[info[0] - 1]);
                            else
                                return coordinatesRegion3Row59_65[info[1]];
                        }
                    case 2:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return CalculatedCoordinates(info[1], countOfRowsRegions[info[0] - 1], coordinatesRegions[info[0] - 1]);
                    default:
                        return null;
                }
            }
            // Если тип места это Старый Колумбарий, то считаем координаты с другими начальными\конечными точками.
            else if (type == TypePlaces.Старый_Колумбарий)
                return CalculatedCoordinates(info[0], countOfSectionOldColumbary, coordinatesOldColumbary );
            // Аналогично и для Нового Колумбария
            else
                return CalculatedCoordinates(info[0] - 115, countOfSectionNewColumbary, coordinatesNewColumbary);
        }


       /// <summary>
       /// Метод,расчитывающий координаты места
       /// </summary>
       /// <param name="row">Ряд</param>
       /// <param name="countRow">Количество рядов на учатске</param>
       /// <param name="coordinates">НАчальные\Конечные координаты участка</param>
       /// <returns>Координаты ряда в LatLng</returns>
        private static LatLng CalculatedCoordinates(int row, int countRow, List<List<double>> coordinates)
        {
            // Считаем дельту между конечной и начальной точками и множаем на коэффициент.
            // Коэффициент равен результату деления данного ряда, на все ряды на участке.
            double coordinateX = (coordinates[1][0] - coordinates[0][0]) * (row / (double)countRow) + coordinates[0][0];
            double coordinateY = (coordinates[1][1] - coordinates[0][1]) * (row / (double)countRow) + coordinates[0][1];
            return new LatLng(coordinateX, coordinateY);
        }
    }
}