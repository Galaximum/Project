using Android.Content;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;
using static Android.Gms.Maps.GoogleMap;


namespace Test
{
    class MyInfoWindowAdapter : Java.Lang.Object, IInfoWindowAdapter
    {
        Context context;
        string layoutInflaterService;


        public MyInfoWindowAdapter(Context context, string layoutInflaterService)
        {
            this.context = context;
            this.layoutInflaterService = layoutInflaterService;
        }


        /// <summary>
        /// Метод,срабатывающий в случае если  GetInfoWindow() возвращает null
        /// </summary>
        /// <param name="marker">маркер</param>
        /// <returns>View</returns>
        public View GetInfoContents(Marker marker)
        {
            return null;
        }


        /// <summary>
        /// Метод,кастомизирующий окно информации,которое появляется при нажатии на маркер.
        /// </summary>
        /// <param name="marker">Маркер</param>
        /// <returns>View Компонент</returns>
        public View GetInfoWindow(Marker marker)
        {
            // НАходим информацию о месте,привязанному к данному маркеру.
            Human selectedHuman = MainActivity.findHumanFromMarker[marker.Id];
            // Подключение элементов управления.
            ContextThemeWrapper wrapper = new ContextThemeWrapper(context, Resource.Style.AppTheme);
            LayoutInflater inflater = (LayoutInflater)wrapper.GetSystemService(layoutInflaterService);
            LinearLayout pageForMap = inflater.Inflate(Resource.Layout.SimpleMapMarker, null) as LinearLayout;
            ImageView second = pageForMap.FindViewById<ImageView>(Resource.Id.imageView2);
            ImageView first = pageForMap.FindViewById<ImageView>(Resource.Id.imageView3);
            ImageView infoIcon = pageForMap.FindViewById<ImageView>(Resource.Id.imageView1);
            TextView textFIO = pageForMap.FindViewById<TextView>(Resource.Id.FIO);
            TextView textInfo = pageForMap.FindViewById<TextView>(Resource.Id.TypePlaces);
            Button button = pageForMap.FindViewById<Button>(Resource.Id.button1);
            // Установка картинок для элементов управления.
            first.SetImageBitmap(MainActivity.bitmapFromVector(context, Resource.Drawable.rozaBlackRight, 4));
            second.SetImageBitmap(MainActivity.bitmapFromVector(context, Resource.Drawable.rozaBlackLeft, 4));
            infoIcon.SetImageBitmap(MainActivity.bitmapFromVector(context, Resource.Drawable.book, 2));
            pageForMap.SetBackgroundResource(Resource.Drawable.styleInfo);
            //Информация на кнопке,для построения маршрута.
            textFIO.Text = textInfo.Text = null;

            // Если место это Основное кладбище, то заполняем окно,соответствующей информацией.
            if (selectedHuman.typePlace == TypePlaces.Основное_Кладбище)
            {
                textFIO.Text = selectedHuman.fullName;
                textInfo.Text = $"{selectedHuman.typePlace.ToString().Replace("_", " ")}";
                textInfo.Text += $"\nУчасток:{selectedHuman.plot}";
                textInfo.Text += $"\nРяд:{selectedHuman.row}";
                textInfo.Text += $"\nМесто:{selectedHuman.place}";
            }
            // Аналогично, если место это Колумбарий.
            else
            {
                textFIO.Text = selectedHuman.fullName;
                textInfo.Text = $"{selectedHuman.typePlace.ToString().Replace("_", " ")}";
                textInfo.Text += $"\nСекция:{selectedHuman.plot}";
                textInfo.Text += $"\nМесто:{selectedHuman.place}";
            }
            return pageForMap;
        }
    }
}