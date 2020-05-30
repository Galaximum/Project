using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;


namespace Test
{
    class MyAdapterForList : ArrayAdapter<Human>
    {
        int idResource;
        public MyAdapterForList(Context context, int resource, List<Human> humans) : base(context, resource, humans)
        {
            idResource = resource;
        }


        /// <summary>
        /// Метод,предначначенный для создания View для Array адаптера.
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
            string name = GetItem(position).fullName;
            // Записывем ее в TextView
            TextView text = ((TextView)convertView.FindViewById(Resource.Id.textView1));
            text.Text = name;
            return convertView;
        }
    }
}