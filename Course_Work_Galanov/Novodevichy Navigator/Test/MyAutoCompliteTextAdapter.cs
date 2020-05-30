using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;


namespace Test
{
    class MyAdapterForText : ArrayAdapter<string>
    {
        Filter filter;
        int idResource;
        public MyAdapterForText(Context context, int resource, Dictionary<char, List<Human>> keysNames) : base(context, resource)
        {
            filter = new MyFilter(keysNames, this);
            idResource = resource;
        }


        /// <summary>
        /// Метод,предначначенный для создания View для Array адаптера для выпадающего списка.
        /// </summary>
        /// <param name="position">Позиция View в ArrayList</param>
        /// <param name="convertView">View</param>
        /// <param name="parent"></param>
        /// <returns>Соззданное View</returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Если View не было создано,создаем.
            if (convertView == null)
            {
                LayoutInflater inflater = LayoutInflater.From(this.Context);
                convertView = inflater.Inflate(idResource, parent, false);
            }
            // Получаем информацию о ФИО
            string name = GetItem(position);
            // Записывем ее в TextView
            TextView text = ((TextView)convertView.FindViewById(Resource.Id.textView1));
            text.Text = name;
            return convertView;
        }

        public override Filter Filter => filter;

        class MyFilter : Filter
        {
            Dictionary<char, List<Human>> fullNames;
            ArrayAdapter adapter;
            public MyFilter(Dictionary<char, List<Human>> names, ArrayAdapter adapter)
            {
                fullNames = names;
                this.adapter = adapter;
            }


            /// <summary>
            /// Метод,задающий фильтрацию объектов в выпадающем тексте,относительно ввода пользователя.
            /// </summary>
            /// <param name="constraint">Ввод пользователя</param>
            /// <returns>Результат</returns>
            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                FilterResults filterResults = new FilterResults();
                // Если ввод не пустая строка, то преобразуем его, и найдем объекты,которые начинаются с введенной информации.
                if (constraint != null)
                {
                    string vvod = constraint.ToString().ToUpper();
                    char burva = vvod.First();
                    string[] arr;
                    if (fullNames.ContainsKey(burva))
                    {
                        // Массив инфомрации,которая начинается с ввода пользователя.
                        arr = fullNames[burva].Select(x => x.fullName).Where(x => x.ToUpper().StartsWith(vvod)).ToArray();
                    }
                    else
                    {
                        arr = new string[0];
                    }
                    filterResults.Values = arr;
                    filterResults.Count = arr.Length;
                }
                return filterResults;
            }


            /// <summary>
            /// Метод,предназначенный для визуализации отфильтрованной информации.
            /// </summary>
            /// <param name="constraint">Ввод пользователя</param>
            /// <param name="results">Результат</param>
            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                if (results != null && results.Count > 0)
                {
                    // Очищяем адаптер от старой информации,потом загружаем новую,потом показываем пользователю.
                    List<string> filterList = results.Values.ToArray<string>().ToList();
                    adapter.Clear();
                    adapter.AddAll(filterList);
                    adapter.NotifyDataSetChanged();
                }
            }
        }
    }
}