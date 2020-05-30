using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Gms.Maps.Model;


namespace Test
{
    public enum TypePlaces { Основное_Кладбище, Старый_Колумбарий, Новый_Колумбарий }
    public class Human
    {
        public string fullName { get; }
        public LatLng coordinatesForMap { get; }
        public int plot { get; }
        public int row { get; }
        public string place { get; }
        public TypePlaces typePlace { get; }


        public Human(string info)
        {
            try
            {
                // Строка с ФИО, данными об участке,месте,ряду и точных координатах.
                string[] generalInformation = info.Split(";");
                // Строка с ФИО, и данными о участке,месте,ряду.
                List<string> nameAndСoordinates = generalInformation[0].Split(" ").ToList();
                // Фамилию делаем в нижнем регистре.
                StringBuilder str = new StringBuilder(nameAndСoordinates[0].ToLower());
                // Делаем первую букву в верхнем регистре.
                str[0] = char.ToUpper(str[0]);
                nameAndСoordinates[0] = str.ToString();
                // Записываем отдельно информацию о месте,ряду,участке.
                string coordinates = nameAndСoordinates[nameAndСoordinates.Count - 1];
                // Удаляем ее из списка с ФИО.
                nameAndСoordinates.RemoveAt(nameAndСoordinates.Count - 1);
                // Записываем информацию о ФИО в поле fullName.
                fullName = string.Join(" ", nameAndСoordinates);
                //Парсим данные о рассположении.
                string[] coordinatesArray = coordinates.Split("-");
                // Если в значении номера участка содержится спец. символ [, то место расположено в Колумбарии.
                if (coordinatesArray[0].Contains('['))
                {
                    plot = int.Parse(coordinatesArray[0].Trim('[', ']'));
                    row = -1;
                    place = $"{coordinatesArray[1]}-{coordinatesArray[2]}";
                    // Если участок меньше 116, то метсо расположено в Старом Колумбарии.
                    if (plot < 116)
                        typePlace = TypePlaces.Старый_Колумбарий;
                    // Иначе в новом.
                    else
                        typePlace = TypePlaces.Новый_Колумбарий;
                }
                // Если не содержится спец символ, то место находится в одном из основных участков.
                else
                {
                    typePlace = TypePlaces.Основное_Кладбище;
                    plot = int.Parse(coordinatesArray[0]);
                    row = int.Parse(coordinatesArray[1].Replace("а", ""));
                    place = coordinatesArray[2];
                }
                // Если в базе данных нет коориднат, то вызываем собственный метод, по подсчету координат.
                if (generalInformation[1] == string.Empty)
                {
                    coordinatesForMap = GetCoordinates.GetCoordinatesFromInfo(typePlace, plot, row);
                }
                // Если есть,то парсим и записываем их в формате LatLng.
                else
                {
                    double[] coordinatesForMap = generalInformation[1].Split(",").Select(x => double.Parse(x.Replace('.', ','))).ToArray();
                    this.coordinatesForMap = new LatLng(coordinatesForMap[0], coordinatesForMap[1]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool operator >(Human first, Human second)
        {
            return first.fullName.CompareTo(second.fullName) == 1;
        }
        public static bool operator <(Human first, Human second)
        {
            return first.fullName.CompareTo(second.fullName) == -1;
        }

    }
}