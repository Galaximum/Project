using Android.Gms.Maps.Model;
using Android.OS;
using Android.Widget;
using Java.Util;
using Org.Json;
using System.Collections.Generic;

namespace Test
{
    public class TaskRequestDirections : AsyncTask<string, Java.Lang.Void, string>
    {
        MainActivity main;

        public TaskRequestDirections(MainActivity main)
        {
            this.main = main;
        }

        /// <summary>
        /// Метод,предназначенный для отображения маршрута на карте Google maps.
        /// </summary>
        /// <param name="result">Информация о маршруте.</param>
        protected override void OnPostExecute(string result)
        {
            base.OnPostExecute(result);
            //Parse Json 
            JSONObject jSONObject = null;
            List<List<Dictionary<string, string>>> routes = null;
            try
            {
                // Парс информации о маршруте, и получения ее в преобразованном ввиде.
                jSONObject = new JSONObject(result);
                DirectionsParser directionsParser = new DirectionsParser();
                routes = directionsParser.Parse(jSONObject);
            }
            catch (System.Exception e)
            {
                Android.Util.Log.Error("Error", e.Message);
            }

            // Получение всех точек маршрута и создание объектов типа Polyline.
            ArrayList points = null;
            PolylineOptions polylineOptions = new PolylineOptions();
            foreach (List<Dictionary<string, string>> path in routes)
            {
                points = new ArrayList();
                polylineOptions = new PolylineOptions();
                foreach (Dictionary<string, string> point in path)
                {
                    double lat = double.Parse(point["lat"]);
                    double lon = double.Parse(point["lon"]);
                    points.Add(new LatLng(lat, lon));
                }

                polylineOptions.AddAll(points);
                polylineOptions.Geodesic(true);
                polylineOptions.InvokeColor(MainActivity.colorForRoute);
                polylineOptions.InvokeWidth(MainActivity.widthRoute);
            }
            // Отображения маршрута на карте.
            if (polylineOptions != null)
            {
                main.map.AddPolyline(polylineOptions);
            }
            else
            {
                Toast.MakeText(main.ApplicationContext, "Direction not found", ToastLength.Short);
            }
        }

        /// <summary>
        /// Метод выполняющиеся сразу после созданния экземпляра данного класса,предназначен для получения информации о маршруте.
        /// </summary>
        /// <param name="strings">Url запрос</param>
        /// <returns>Информация о маршруте</returns>
        protected override string RunInBackground(params string[] strings)
        {
            string responseString = "";
            try
            {
                responseString = main.RequestDirection(strings[0]);
            }
            catch (System.Exception e)
            {
                Android.Util.Log.Error("Error", e.Message);

            }
            return responseString;
        }
    }
}