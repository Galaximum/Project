using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using Android.Gms.Maps;
using Android.Locations;
using Android.Gms.Maps.Model;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Util;
using Java.IO;
using Android.Graphics.Drawables;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Android.Views.InputMethods;
using Android.Support.V4.App;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Java.Net;
using Java.Lang;

namespace Test
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", Icon = "@drawable/IconApp", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public partial class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        // Константы связанные с Google maps.
        // Лимит маркеров на карте.
        const int countOfMarkers = 5;
        // Цвет маршрута.
        public const int colorForRoute = unchecked((int)0xff3498db);
        // Ширина маршрута.
        public const int widthRoute = 10;
        // Google ключ, для работы с сервисами Google.
        const string googleKey = "AIzaSyCRV1KvwVR6Thxw15_swg4wqA0dql1o7kw";
        //Константы для permissions
        const int fineLocationPermissionCode = 100;



        // Шаг нажатия на кнопку "Назад".
        int countOfBackPressed = 0;
        // Время первого нажатия на кнопку "Назад".
        DateTime firstBackPressed;

        // Словарь с ключем Id Маркера, и значением объекта класса Human.
        public static Dictionary<string, Human> findHumanFromMarker { get; } = new Dictionary<string, Human>();
        // Словарь с ключем объекта класса Human, и значением Marker.
        Dictionary<Human, Marker> findMarkerFromHuman = new Dictionary<Human, Marker>();
        // Словарь с ключем Char, и значением Лист объектов класса Human.
        Dictionary<char, List<Human>> parsedDataBase = new Dictionary<char, List<Human>>();

        // Карта.
        public GoogleMap map;
        // Странички.
        LinearLayout searchPage, mapPage, settingsPage;
        // Координата пользователя.
        LatLng myPosition;
        // Элемент управления, состоящий из страниц.
        ViewPager viewPager;

        // Текст для кнопок переключения между страницами.
        static string[] textForTabs =
        {
            "Поиск",
            "Карта",
            "Настройки"
        };

        // Иконки для панели навигации по страничкам.
        static int[] iconsForTabs =
        {
            Resource.Drawable.search,
            Resource.Drawable.mapIcon,
            Resource.Drawable.settings
        };

        // Иконки маркеров с цифрами.
        static int[] numberMarkersIcons =
        {
            Resource.Drawable.markerNumber1,
            Resource.Drawable.markerNumber2,
            Resource.Drawable.markerNumber3,
            Resource.Drawable.markerNumber4,
            Resource.Drawable.markerNumber5,
        };


        /// <summary>
        /// Метод,создающий приложение при его запуске.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //// Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            // разрешение на использование векторных изображений.
            AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
            ReadDataBase();
            CreatePages();
            CreateOptionsPageSettings();
            CreateOptionsPageMap();
            CreateOptionsPageSearch();
        }


        /// <summary>
        /// Метод, предназначенный для описания основной логики связанной со страницой "Search".
        /// </summary>
        private void CreateOptionsPageSearch()
        {
            // Инициализация элементов управления.
            AutoCompleteTextView autoCompleteTextView = (AutoCompleteTextView)searchPage.FindViewById(Resource.Id.autoCompleteTextView1);
            ImageButton image = searchPage.FindViewById<ImageButton>(Resource.Id.imageButton1);
            image.SetImageBitmap(bitmapFromVector(this, Resource.Drawable.sendIcon, 4));
            // Создаем адаптер для автозаполнения элемента AutoCompleteTextView.
            ArrayAdapter<string> adapter = new MyAdapterForText(this, Resource.Layout.simple_dropdown, parsedDataBase);
            autoCompleteTextView.Adapter = adapter;
            // Событие обработки ввода ФИО
            image.Click += delegate
            {
                // Считывание введенной информации.
                string name = autoCompleteTextView.Text;
                if (name != "")
                {
                    // Преобразование введенной информации.
                    name = string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => char.ToUpper(x[0]) + x.Remove(0, 1).ToLower()).ToArray());

                    // Если выбранное место для посищения не выходит за лимит,то добавляем его на карту.
                    if (findHumanFromMarker.Count < countOfMarkers)
                    {
                        

                        // Поиск введенных ФИО среди ФИО базы данных.
                        if (parsedDataBase.ContainsKey(name.First()) && parsedDataBase[name.First()].Select(x => x.fullName).Contains(name))
                        {
                            autoCompleteTextView.Text = null;
                            // Скрытие системной клавиатуры.
                            InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                            imm.HideSoftInputFromWindow(image.WindowToken, HideSoftInputFlags.NotAlways);
                            // Установка страницы с картой.
                            autoCompleteTextView.ClearFocus();
                            viewPager.CurrentItem = 1;
                            // Нахождение Выбранного человека.
                            Human selectedHuman = parsedDataBase[name.First()].Where(x => x.fullName == name).First();
                            // Если выбранное место, не выбиралось раннее, то добавляем его на карту.
                            if (!findMarkerFromHuman.ContainsKey(selectedHuman))
                            {
                                // Создание маркера.
                                Marker marker = map.AddMarker(new MarkerOptions().SetPosition(selectedHuman.coordinatesForMap));
                                marker.HideInfoWindow();
                                // Установка иконки маркера.
                                marker.SetIcon(BitmapDescriptorFactory.FromBitmap(bitmapFromVector(ApplicationContext, Resource.Drawable.mainMarker, 3)));
                                // Добавления нужных данных в словари.

                                findHumanFromMarker.Add(marker.Id, selectedHuman);
                                findMarkerFromHuman.Add(selectedHuman, marker);
                            }
                            // Если выбиралось раннее, то сообщаем об этом пользователю.
                            else
                                Toast.MakeText(this, "Вы уже выбрали данное место для посещения!", ToastLength.Long).Show();
                        }
                        // Если в базе данных не найдены ФИО, сообщаем об этом пользователю.
                        else
                        {
                            autoCompleteTextView.Text = null;
                            Toast.MakeText(this, $"Данное ФИО не найдено в базе данных!", ToastLength.Long).Show();
                        }
                    }
                    // Сообщаем пользователю о достигнутом лимите маркеров на карте.
                    else
                    {
                        autoCompleteTextView.Text = null;
                        Toast.MakeText(this, $"Вы не можете выбрать для посещения больше чем {countOfMarkers} мест!", ToastLength.Long).Show();
                    }
                }
                // Если была нажата кнопка, но при этом пользователь ничего не ввел, сообщаем ему об этом.
                else
                {
                    Toast.MakeText(this, "Сначала введите ФИО, потом нажмите на кнопку", ToastLength.Long).Show();
                }
            };
        }


        /// <summary>
        /// Метод, предназначенный для подключения Google maps к прилложению.
        /// </summary>
        private void CreateOptionsPageMap()
        {
            // Установка карты.
            var mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);
        }


        /// <summary>
        /// Метод, предназначенный для подключения трех основых страниц к приложению.
        /// </summary>
        private void CreatePages()
        {
            LinearLayout General = FindViewById<LinearLayout>(Resource.Id.General);
            // Находим страничку с картой.
            mapPage = General.FindViewById<LinearLayout>(Resource.Id.Map);
            General.RemoveView(mapPage);
            ContextThemeWrapper wrapper = new ContextThemeWrapper(this, Resource.Style.AppTheme);
            LayoutInflater inflater = (LayoutInflater)wrapper.GetSystemService(LayoutInflaterService);
            // Страничка с поиском.
            searchPage = inflater.Inflate(Resource.Layout.PageSearch, null) as LinearLayout;
            // Страничка с настройками.
            settingsPage = inflater.Inflate(Resource.Layout.PageSetting, null) as LinearLayout;
            // Привязка страничек к кнопкам переключения.
            viewPager = FindViewById<ViewPager>(Resource.Id.viewPager1);
            TabLayout tab = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            // Установка основных вкладок для приложения.
            viewPager.Adapter = new MyPagerAdapter(this, searchPage, mapPage, settingsPage);
            // Установка главной страницы для приложения.
            viewPager.CurrentItem = 1;

            // Привязка заголовков к страницам.
            tab.SetupWithViewPager(viewPager);

            // Установка изображений и текста, для отдельных "табов".
            for (int i = 0; i < iconsForTabs.Length; i++)
            {
                tab.GetTabAt(i).SetText(textForTabs[i]);
                Drawable icon = new BitmapDrawable(bitmapFromVector(ApplicationContext, iconsForTabs[i], 1));
                tab.GetTabAt(i).SetIcon(icon);
            }
        }


        /// <summary>
        /// Метод,проверяющий разрешения для приложения.
        /// </summary>
        /// <param name="permission">Разрешение</param>
        /// <param name="requestCode">Код разрешения</param>
        private void CheckPermissionFineLocation(string permission, int requestCode)
        {
            // Проверка, если разрешение не предоставлено
            if (ContextCompat.CheckSelfPermission(this, permission) == Permission.Denied)
                ActivityCompat.RequestPermissions(this, new string[] { permission }, requestCode);
            else
            {
                CheckPermissionMap();
            }
        }


        /// <summary>
        /// Метод,опредлеляющий, выло ли выданно разрешение пользователем.
        /// </summary>
        /// <param name="requestCode">Код разрешения</param>
        /// <param name="permissions">Разрешение</param>
        /// <param name="grantResults">выбор пользователя</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == fineLocationPermissionCode)
            {
                // Если разрешение одобрено, сообщаем об этом пользователю.
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    Toast.MakeText(this, "Доступ к данным о местоположении телефона разрешен!", ToastLength.Short).Show();
                    CheckPermissionMap();
                }
                else
                {
                    Toast.MakeText(this, "Доступ к данным о местоположении телефона запрещен!", ToastLength.Short).Show();
                }
            }
        }


        /// <summary>
        /// Метод,осуществлющий парсинг базы данных. 
        /// </summary>
        private void ReadDataBase()
        {
            // Поток к файлу даты из ресурсов.
            Stream stream = Resources.OpenRawResource(Resource.Raw.FullDataBase);
            // Поток для чтения файла.
            BufferedReader reader = new BufferedReader(new InputStreamReader(stream));
            string svcLine;
            // Чтение файла.
            reader.ReadLine();
            while ((svcLine = reader.ReadLine()) != null)
            {
                try
                {

                    char firstSymbol = svcLine.First();
                    // Если в словаре числится ключ, заполняем лист по данному ключу.
                    if (parsedDataBase.ContainsKey(firstSymbol))
                    {
                        parsedDataBase[firstSymbol].Add(new Human(svcLine));
                    }
                    // Если в словаре не числится ключ, то создаем в нем новый объект List<Human> и заполняем его.
                    else
                    {
                        parsedDataBase.Add(firstSymbol, new List<Human>());
                        parsedDataBase[firstSymbol].Add(new Human(svcLine));
                    }
                }
                catch (System.Exception)
                {
                    Log.Error("Pogramm can't parsed string", svcLine);
                }
            }
            // Закрытие потоков.
            reader.Close();
            stream.Close();
        }


        /// <summary>
        /// Метод,описывающий основную логику на странице "Setting".
        /// </summary>
        private void CreateOptionsPageSettings()
        {
            // Инициализация элементов управления.
            TextView textMapSetting = settingsPage.FindViewById<TextView>(Resource.Id.textMapSetting);
            // Добавления рисунка к тексту с настройками карты.
            Drawable arrowDown = new BitmapDrawable(bitmapFromVector(ApplicationContext, Resource.Drawable.downArrow, 2));
            textMapSetting.SetCompoundDrawablesWithIntrinsicBounds(null, null, arrowDown, null);
            LinearLayout mapStyles = settingsPage.FindViewById<LinearLayout>(Resource.Id.linearLayout2);

            // Событие клика по тексту о настройках карты.
            // Событие Скрывает\Показывает меню со всеми настройками карты,доступные пользователю.
            textMapSetting.Click +=
             (sender, e) =>
             {
                 if (mapStyles.Visibility == ViewStates.Visible)
                     mapStyles.Visibility = ViewStates.Gone;
                 else
                     mapStyles.Visibility = ViewStates.Visible;
             };

            // Инициализация каждого RadioButton.
            RadioButton light = settingsPage.FindViewById<RadioButton>(Resource.Id.StyleMapLight);
            RadioButton retro = settingsPage.FindViewById<RadioButton>(Resource.Id.styleMapRetro);
            RadioButton night = settingsPage.FindViewById<RadioButton>(Resource.Id.styleMapNight);
            // События, которые устанавливают новую тему для карты.
            light.Click += delegate { SetNewMapStyle(Resource.Raw.styleMapLight); };
            retro.Click += delegate { SetNewMapStyle(Resource.Raw.styleMapRetro); };
            night.Click += delegate { SetNewMapStyle(Resource.Raw.styleMapNightPlus); };
        }


        /// <summary>
        /// Метод,описывающий основную логику,связанную с Google maps.
        /// </summary>
        /// <param name="map">Google карта</param>
        public void OnMapReady(GoogleMap map)
        {
            this.map = map;
            CheckPermissionFineLocation(Manifest.Permission.AccessFineLocation, fineLocationPermissionCode);
            // Установка адаптера для меню,которое появляется при нажатии на маркер.
            map.SetInfoWindowAdapter(new MyInfoWindowAdapter(this, LayoutInflaterService));
            map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(55.724758, 37.554268), 16.5f));
            // Установка кнопок для карты: Зум; найти местоположение.
            map.UiSettings.ZoomControlsEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = true;

            // Событие, которое при долгом нажатии на карту очищяет ее.
            map.MapLongClick += (sender, e) =>
            {
                map.Clear();
                findHumanFromMarker.Clear();
                findMarkerFromHuman.Clear();
            };

            // Событие срабатывающие при нажатии на маркер.
            map.InfoWindowClick += (sender, e) =>
            {
                Marker ThisMarker = e.Marker;
                // Инициализация основных элементов управления.
                ContextThemeWrapper wrapper = new ContextThemeWrapper(this, Resource.Style.AppTheme);
                LayoutInflater inflater = (LayoutInflater)wrapper.GetSystemService(LayoutInflaterService);
                Android.App.AlertDialog.Builder dialogBuilder = new Android.App.AlertDialog.Builder(this);
                LinearLayout window = inflater.Inflate(Resource.Layout.layoutForDialog, null) as LinearLayout;
                dialogBuilder.SetView(window);
                RadioGroup group = window.FindViewById<RadioGroup>(Resource.Id.radioGroup1);
                RadioButton toThisMarker = window.FindViewById<RadioButton>(Resource.Id.radioButton1);
                RadioButton toAllMarkers = window.FindViewById<RadioButton>(Resource.Id.radioButton2);
                RadioButton toAllMarkersUserСhoice = window.FindViewById<RadioButton>(Resource.Id.radioButton3);
                ListView listView1 = window.FindViewById<ListView>(Resource.Id.listView1);
                ListView listView2 = window.FindViewById<ListView>(Resource.Id.listView2);
                LinearLayout listsForUserChoice = window.FindViewById<LinearLayout>(Resource.Id.listsWithHumans);
                // Подключения адаптеров для списков для выбора определенного порядка посещения.
                ArrayAdapter<Human> givenTheСhoice = new MyAdapterForList(this, Resource.Layout.simpleListItem, findMarkerFromHuman.Keys.ToList());
                ArrayAdapter<Human> userСhoice = new MyAdapterForList(this, Resource.Layout.simpleListItem, new List<Human>());
                listView1.Adapter = givenTheСhoice;
                listView2.Adapter = userСhoice;
                // Событие которое скрывает\показывает списки для выбора определенного порядка посещения мест.
                group.CheckedChange += (sender, e) =>
                {
                    if (e.CheckedId == toAllMarkersUserСhoice.Id)
                        listsForUserChoice.Visibility = ViewStates.Visible;
                    else
                        listsForUserChoice.Visibility = ViewStates.Gone;
                };

                // Событие,срабатывающие при нажатии на элемент списка "givenTheChoice".
                // Событие добавляет выбранный элемент в список "UserChoice" и удаляет его из старого списка.
                listView1.ItemClick += (sender, e) =>
                {

                    Human human = givenTheСhoice.GetItem(e.Position);
                    userСhoice.Add(human);
                    userСhoice.NotifyDataSetChanged();
                    givenTheСhoice.Remove(human);
                    givenTheСhoice.NotifyDataSetChanged();
                };

                // Событие,срабатывающие при нажатии на элемент списка "UserChoice".
                // Событие добавляет выбранный элемент в список "givenTheChoice" и удаляет его из старого списка.
                listView2.ItemClick += (sender, e) =>
                {
                    Human human = userСhoice.GetItem(e.Position);
                    givenTheСhoice.Add(human);
                    givenTheСhoice.NotifyDataSetChanged();
                    userСhoice.Remove(human);
                    userСhoice.NotifyDataSetChanged();
                };

                // Добавление кнопки для подтверждения построения маршрута.
                dialogBuilder.SetPositiveButton(Resource.String.textForPositiveButton, ((sender, e) =>
                {
                    // Если У пользователя включена геолокация, то строится маршут, иначе сообщается пользоваетлю об этом.
                    if (map.MyLocation == null)
                    {
                        CheckPermissionMap();
                    }
                    else
                    {
                        // Если пользователя выбрал построение маршрута к данному маркеру.
                        if (toThisMarker.Checked)
                        {
                            myPosition = new LatLng(map.MyLocation.Latitude, map.MyLocation.Longitude);
                            CreateRoute(myPosition, ThisMarker.Position);
                        }
                        // Если пользователь выбрал построение маршрута по оптимальному маршруту.
                        else if (toAllMarkers.Checked)
                        {
                            List<Human> selectedHumans = findMarkerFromHuman.Keys.ToList();
                            //логика по постройке оптимального маршрута
                            List<Human> minDistance = GetListMinDistance(selectedHumans);
                            CreateRouteFromListHuman(minDistance);

                        }
                        // Если пользователя выбрал построение относительно собственного порядка посещения.
                        else if (toAllMarkersUserСhoice.Checked)
                        {
                            // Маршрут строится, если пользователь выбрал все места для посещения, из раннее его выбранных.
                            if (listView2.Count == findMarkerFromHuman.Count)
                            {
                                List<Human> listWithHuman = new List<Human>();
                                // Получение выбранных полььзователем мест для посещения и запись их в лист.
                                for (int i = 0; i < userСhoice.Count; i++)
                                    listWithHuman.Add(userСhoice.GetItem(i));
                                CreateRouteFromListHuman(listWithHuman);
                            }
                            else
                            {
                                Toast.MakeText(this, "Вы не выбрали все места для посещения", ToastLength.Long).Show();
                            }
                        }
                        // Если не был выбран ни один из предложенных вариантов построения маршрута.
                        else
                        {
                            Toast.MakeText(this, "Вы не выбрали ни один из доступных методов прокладывания маршрута", ToastLength.Long).Show();
                        }
                    }
                }));

                // Кнопка отказа от построения маршрута.
                dialogBuilder.SetNegativeButton(Resource.String.textForNegativeButton, (sender, e) =>
                {
                    Toast.MakeText(this, "Вы отменили строительство маршрута.\nЕсли вы хотите очистить карту, сделайте долгое нажатие по ней!", ToastLength.Long).Show();
                });
                dialogBuilder.Show();
            };

        }


        /// <summary>
        /// Проверка на разрешение об использовании геоданных устройства приложением.
        /// Также проверка на включенный GPS модуль.
        /// </summary>
        private void CheckPermissionMap()
        {
            // Проверка наличия прав на использование геоданных устройства
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                map.MyLocationEnabled = true;
                LocationManager locationManager = (LocationManager)GetSystemService(LocationService);
                Criteria criteria = new Criteria();
                string provider = locationManager.GetBestProvider(criteria, true);
                // Проверка на включенный GPS модуль.
                if (provider == null)
                {
                    Toast.MakeText(this, "Пожалуйста, включите GPS модуль!", ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "Вы не можете строить маршруты, так как запретили доступ приложению к данным о местоположении телефона", ToastLength.Long).Show();
            }
        }


        /// <summary>
        /// Метод,строящий маршрут по n точкам,которые были в него загружены.
        /// </summary>
        /// <param name="Humans">Лист из n точек</param>
        private void CreateRouteFromListHuman(List<Human> Humans)
        {
            // Вычисление позиции пользователя.
            myPosition = new LatLng(map.MyLocation.Latitude, map.MyLocation.Longitude);
            // Построение маршрута от пользователя до первой точки.
            CreateRoute(myPosition, Humans[0].coordinatesForMap);
            // Установка новой картинки для маркера.
            findMarkerFromHuman[Humans[0]].SetIcon(BitmapDescriptorFactory.FromBitmap(bitmapFromVector(base.ApplicationContext, numberMarkersIcons[0], 2)));
            // Построение остальных маршрутов между оставшиемися точками.
            for (int i = 0; i < Humans.Count - 1; i++)
            {
                CreateRoute(Humans[i].coordinatesForMap, Humans[i + 1].coordinatesForMap);
                findMarkerFromHuman[Humans[i + 1]].SetIcon(BitmapDescriptorFactory.FromBitmap(bitmapFromVector(base.ApplicationContext, numberMarkersIcons[i + 1], 2)));
            }
        }


        /// <summary>
        /// Метод,который расчитывает оптимальный путь через n точек.
        /// </summary>
        /// <param name="selectedHumans">Лист с n точками</param>
        /// <returns>Лист с точками,рассположенными в опредленном порядке,для оптимального маршрута</returns>
        private List<Human> GetListMinDistance(List<Human> selectedHumans)
        {
            myPosition = new LatLng(map.MyLocation.Latitude, map.MyLocation.Longitude);
            Dictionary<float, List<Human>> keyValuePairs = new Dictionary<float, List<Human>>();
            // Получение списка со всеми возможными наборами из n точек.
            //   List<List<Human>> fullPermutations = Permutation(selectedHumans);
            List<List<Human>> fullPermutations = Permutations(selectedHumans);

            // Определение суммарного расстояния между точками. 
            string str = "";
            foreach (List<Human> item in fullPermutations)
            {
                float[] distanceBetweenPoints = new float[1];
                float result = 0.0f;
                Location.DistanceBetween(myPosition.Latitude, myPosition.Longitude, item[0].coordinatesForMap.Latitude, item[0].coordinatesForMap.Longitude, distanceBetweenPoints);
                result += distanceBetweenPoints[0];
                for (int i = 0; i < item.Count() - 1; i++)
                {
                    Location.DistanceBetween(item[i].coordinatesForMap.Latitude, item[i].coordinatesForMap.Longitude, item[i + 1].coordinatesForMap.Latitude, item[i + 1].coordinatesForMap.Longitude, distanceBetweenPoints);
                    result += distanceBetweenPoints[0];
                }
                str += $"{result}: " + string.Join(" ", item.Select(x => x.fullName)) + "\n";
                if (!keyValuePairs.ContainsKey(result))
                    keyValuePairs.Add(result, item);
            }
            // Нахождение списка с минимальным расстоянием.
            float minValue = keyValuePairs.Keys.Min<float>();
            return keyValuePairs[minValue];
        }

        /// <summary>
        /// Метод,возращает всевозможные комбинации перестановок n точек.
        /// </summary>
        /// <param name="ListElement">Лист с n точками</param>
        /// <returns>Лист со всеми комбинациями</returns>
        private List<List<Human>> Permutations(List<Human> elements)
        {
            List<List<Human>> result = new List<List<Human>>();
            // Упорядочение последовательности по лексикографическому сравнению ФИО.
            elements.Sort((x, y) => x.fullName.CompareTo(y.fullName));
            // Добавление первой последовательности в список с результатом.
            result.Add(elements);
            Human[] arr = elements.ToArray();
            // Цикл, находящий следующие перестановки последовательности, относительно предыдущей.
            while (NextSet(arr, arr.Length))
            {
                // Добавление новой перестановки в список с результатами.
                result.Add(((Human[])arr.Clone()).ToList());
            }
            return result;
        }

        /// <summary>
        /// Метод, генерирующий следующую перестановку последовательности.
        /// </summary>
        /// <param name="previousSequence">предыдущая перестановка</param>
        /// <param name="countOfElements">количетсво элементов перестановки</param>
        /// <returns>Возвращает true,если существует следующая перестановка,иначе false</returns>
        static bool NextSet(Human[] previousSequence, int countOfElements)
        {
            int j = countOfElements - 2;
            // поиск наибольшего j, для которого выполняется условие: a[j] < a[j+1].
            while (j != -1 && previousSequence[j] > previousSequence[j + 1]) j--;
            // Если такого j нет, то перестановок больше не существует.
            if (j == -1)
                return false;
            int l = countOfElements - 1;
            // Поиск такого l, для которого выполняется условие: a[l] > a[j]
            while (previousSequence[j] > previousSequence[l]) l--;
            Swap(previousSequence, j, l);
            // Записываем последовательность a[j+1],...,a[n-1] в обратном порядке.
            int k = j + 1, r = countOfElements - 1;
            while (k < r)
                Swap(previousSequence, k++, r--);
            return true;
        }

        /// <summary>
        /// Метод,меняющий элементы перестановки местами по указанным индексам.
        /// </summary>
        /// <param name="sequence">перестановка</param>
        /// <param name="firstIndex">индекс первого элемента</param>
        /// <param name="secondIndex">индекс второго элемента</param>
        static void Swap(Human[] sequence, int firstIndex, int secondIndex)
        {
            Human s = sequence[firstIndex];
            sequence[firstIndex] = sequence[secondIndex];
            sequence[secondIndex] = s;
        }


        /// <summary>
        /// Метод, создающий маршрут по координатам начальной и конечной точки.
        /// </summary>
        /// <param name="start">Начальная точка</param>
        /// <param name="end">Конечная точка</param>
        /// <param name="color">Цвет маршрута</param>
        private void CreateRoute(LatLng start, LatLng end)
        {
            // Получение Url запроса.
            string url = GetRequestUrl(start, end);
            // Создания Задания на построение маршрута.
            TaskRequestDirections taskRequestDirections = new TaskRequestDirections(this);
            taskRequestDirections.Execute(url);
        }


        /// <summary>
        /// Метод, создающий url запрос для сервисов Google maps.
        /// </summary>
        /// <param name="origin">Начальная точка для маршрута</param>
        /// <param name="dest">Конечная точка дял маршрута</param>
        /// <returns>Готовый Url запрос</returns>
        private string GetRequestUrl(LatLng origin, LatLng dest)
        {
            string str_org = $"origin={origin.Latitude.ToString().Replace(",", ".")},{origin.Longitude.ToString().Replace(",", ".")}";
            string str_dest = $"destination={dest.Latitude.ToString().Replace(",", ".")},{dest.Longitude.ToString().Replace(",", ".")}";
            string sensor = "sensor=false";
            string mode = "mode=walking";
            string key = $"key={googleKey}";
            string param = str_org + "&" + str_dest + "&" + sensor + "&" + mode + "&" + key;
            string output = "json";
            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + param;
            return url;
        }


        /// <summary>
        /// Метод, осуществляющий Http подключение по url запросу и считывающий информацию о маршруте.
        /// </summary>
        /// <param name="reqUrl">Url запрос</param>
        /// <returns>Информация о маршруте</returns>
        public string RequestDirection(string reqUrl)
        {
            string responseString = "";
            Stream inputStream = null;
            HttpURLConnection httpURLConnection = null;
            try
            {
                // Подключение и считывание информации о маршруте.
                URL url = new URL(reqUrl);
                httpURLConnection = (HttpURLConnection)url.OpenConnection();
                httpURLConnection.Connect();
                inputStream = httpURLConnection.InputStream;
                InputStreamReader inputStreamReader = new InputStreamReader(inputStream);
                BufferedReader bufferedReader = new BufferedReader(inputStreamReader);
                StringBuffer stringBuffer = new StringBuffer();
                string line = "";
                while ((line = bufferedReader.ReadLine()) != null)
                {
                    stringBuffer.Append(line);
                }
                responseString = stringBuffer.ToString();
                bufferedReader.Close();
                inputStreamReader.Close();
            }
            catch (System.Exception e)
            {
                Log.Error("Error", e.Message);
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                }
                httpURLConnection.Disconnect();
            }
            return responseString;
        }


        /// <summary>
        /// Метод,предназначенный для смены стиля карты.
        /// </summary>
        /// <param name="IDstyle">Идентификатор стиля</param>
        private void SetNewMapStyle(int IDstyle)
        {
            try
            {
                // Изменение стиля карты.
                bool success = map.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, IDstyle));
                if (!success)
                {
                    Log.Error("Error", "Style parsing failed.");
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                Log.Error("Error", e.Message);
            }
        }


        /// <summary>
        /// Метод,описывающий логику,при нажатии на системную кнопку Back
        /// </summary>
        public override void OnBackPressed()
        {
            // Если окно главное, то предупреждаем пользователя о том, что он хочет выйти.
            // Если он нажмет еще раз на кнопку "Назад" в течении 2 секунд, то приложения закроется.
            if (viewPager.CurrentItem == 1)
            {
                countOfBackPressed++;
                string str = "Повторите для выхода из приложения";
                Toast msg = Toast.MakeText(ApplicationContext, str, ToastLength.Short);
                if (countOfBackPressed == 1)
                {
                    firstBackPressed = DateTime.Now;
                    msg.Show();
                }
                if (countOfBackPressed == 2)
                {
                    // Если прошло меньше 2-ух секунд то выход из приложения.
                    if ((DateTime.Now - firstBackPressed) < TimeSpan.FromSeconds(2))
                    {
                        base.OnBackPressed();
                    }
                    else// Иначе, повторно выбрасываем сообщение пользователю.
                    {
                        countOfBackPressed = 0;
                        this.OnBackPressed();
                    }
                }
            }
            // Если окно, на котором былы нажата системная кнопка, не главное, то возращаем на главное.
            else
            {
                viewPager.CurrentItem = 1;
            }
        }


        /// <summary>
        /// Метод,позволяющий конвертировать векторные изображения в растровые.
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="vectorResId">Идентификатор вектора</param>
        /// <param name="multiplicity">Кратность</param>
        /// <returns>Растровое изображение</returns>
        public static Bitmap bitmapFromVector(Context context, int vectorResId, double multiplicity)
        {
            // Получение Drawable из XML файла с векторным изображением.
            Drawable vectorDrawable = ContextCompat.GetDrawable(context, vectorResId);
            // Определение размера.
            vectorDrawable.SetBounds(0, 0, (int)(vectorDrawable.IntrinsicWidth * multiplicity), (int)(vectorDrawable.IntrinsicHeight * multiplicity));
            // Создание пустого растрового изображения.
            Bitmap bitmap = Bitmap.CreateBitmap((int)(vectorDrawable.IntrinsicWidth * multiplicity), (int)(vectorDrawable.IntrinsicHeight * multiplicity), Bitmap.Config.Argb8888);
            // Перемещение векторного изображения на растровое.
            Canvas canvas = new Canvas(bitmap);
            vectorDrawable.Draw(canvas);
            return bitmap;
        }
    }
}