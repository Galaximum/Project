using Android.Gms.Maps.Model;
using Org.Json;
using System;
using System.Collections.Generic;

namespace Test
{
    public class DirectionsParser
    {
        /// <summary>
        /// Метод,предназначенный для расспарсивания информации,которая приходит в виде Json объекта.
        /// </summary>
        /// <param name="jObject">Информация о маршруте</param>
        /// <returns>Преобразованная информация о маршруте</returns>
        public List<List<Dictionary<string, string>>> Parse(JSONObject jObject)
        {
            List<List<Dictionary<string, string>>> routes = new List<List<Dictionary<string, string>>>();
            JSONArray jRoutes = null;
            JSONArray jLegs = null;
            JSONArray jSteps = null;
            try
            {
                //Получаем массив информации о маршрутах по ключу "routes".
                jRoutes = jObject.GetJSONArray("routes");
                for (int i = 0; i < jRoutes.Length(); i++)
                {
                    //Получаем массив с информацией о маршруте по ключу "legs".
                    jLegs = ((JSONObject)jRoutes.Get(i)).GetJSONArray("legs");
                    List<Dictionary<string, string>> path = new List<Dictionary<string, string>>();
                    //Loop for all legs
                    for (int j = 0; j < jLegs.Length(); j++)
                    {
                        // получаем массив отрезков из которых маршрут по ключу "steps".
                        jSteps = ((JSONObject)jLegs.Get(j)).GetJSONArray("steps");

                        for (int k = 0; k < jSteps.Length(); k++)
                        {
                            string polyline = "";
                            // Поулчаем каждую точку отрезка.
                            polyline = (string)((JSONObject)((JSONObject)jSteps.Get(k)).Get("polyline")).Get("points");
                            List<LatLng> list = DecodePolyline(polyline);

                            // Парсим каждую точку отрезка.
                            for (int l = 0; l < list.Count; l++)
                            {
                                Dictionary<string, string> hm = new Dictionary<string, string>();
                                hm.Add("lat", list[l].Latitude.ToString());
                                hm.Add("lon", list[l].Longitude.ToString());
                                path.Add(hm);
                            }
                        }
                        // Добавляем в лист наш маршурут.
                        routes.Add(path);
                    }
                }
            }
            catch (JSONException e)
            {
                Android.Util.Log.Error("Error", e.Message); 
            }
            catch (Exception e)
            {
                Android.Util.Log.Error("Error", e.Message);
            }
            return routes;
        }


        /// <summary>
        /// Метод, предназначенный для Расшифрования информации о маршрутах,путем алгоритма от Google.
        /// </summary>
        /// <param name="encoded">Зашифрованная строка</param>
        /// <returns>Лист с координатами всех точек,для постройки маршрута</returns>
        private List<LatLng> DecodePolyline(string encoded)
        {

            List<LatLng> poly = new List<LatLng>();
            int index = 0, len = encoded.Length;
            int lat = 0, lng = 0;

            while (index < len)
            {
                int b, shift = 0, result = 0;
                do
                {
                    b = encoded[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;

                shift = 0;
                result = 0;
                do
                {
                    b = encoded[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                LatLng p = new LatLng((((double)lat / 1E5)),
                        (((double)lng / 1E5)));
                poly.Add(p);
            }
            return poly;
        }
    }
}

