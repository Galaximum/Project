using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

namespace Test
{
    class MyPagerAdapter : PagerAdapter
    {
        LinearLayout[] pages;
        public MyPagerAdapter(Context context, params LinearLayout[] linears)
        {
            pages = linears;
        }
        public override int Count
        {
            get {
                return pages.Length;
            }
        }

        /// <summary>
        /// Метод,предназначенный для удаления страницек.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="position"></param>
        /// <param name="object"></param>
        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            var viewPager = container.JavaCast<ViewPager>();
            viewPager.RemoveView(@object as View);
        }

        /// <summary>
        /// Метод,который позволяет добавить свою страницу в  pageAdapter.
        /// </summary>
        /// <param name="collection">Коллекция объектов</param>
        /// <param name="position">Позиция</param>
        /// <returns></returns>
        [Obsolete]
        public override Java.Lang.Object InstantiateItem(View collection, int position)
        {
            View v = pages[position];
            ((ViewPager)collection).AddView(v, 0);
            return v;
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == @object;
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return null;
        }
    }
}