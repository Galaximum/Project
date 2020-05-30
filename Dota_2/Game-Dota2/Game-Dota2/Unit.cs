using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyExeptions;


namespace Game_Dota2
{
    class Unit
    {
        public readonly string name;
        readonly int type;
        readonly int baseStr;
        readonly int baseAgi;
        readonly int baseInt;
        readonly int moveSpeed;
        readonly double baseArmor;
        readonly double minDmg;
        readonly double regeneration;
        double Health;
        double MaxHealth;
        static Random rnd = new Random();



        internal Unit(string[] stats)
        {
            int i = 0;
            name = stats[i++];
            type = int.Parse(stats[i++]);
            baseStr = int.Parse(stats[i++]);
            baseAgi = int.Parse(stats[i++]);
            baseInt = int.Parse(stats[i++]);
            moveSpeed = int.Parse(stats[i++]);
            baseArmor = double.Parse(stats[i++]);
            minDmg = int.Parse(stats[i++]);
            regeneration = double.Parse(stats[i++]);
            MaxHealth = 29 * baseStr;
            // Логика,определяющая, запись идет с новый игры, или с XML.
            if (stats.Length == i)
                Health = MaxHealth;
            else
                Health = double.Parse(stats[i]);
        }
        /// <summary>
        /// Метод,отдающий всю информацию об объекте класса.
        /// </summary>
        /// <returns></returns>
        public string[] FullInfo()
        {
            return new string[] { name, type.ToString(), baseStr.ToString(), baseAgi.ToString(),
                baseInt.ToString(), moveSpeed.ToString(), baseArmor.ToString(),
                minDmg.ToString(), regeneration.ToString(), Health.ToString() };
        }
        /// <summary>
        /// Метод,расчитывающий очки для героя.
        /// </summary>
        /// <returns></returns>
        public double Points()
        {
            return (minDmg * baseStr / 10 + baseArmor * baseAgi / 10) * rnd.NextDouble();
        }
        /// <summary>
        /// Метод атаки.
        /// </summary>
        /// <param name="enemy">Ссылка на противника</param>
        /// <returns>Сообщение об атаке</returns>
        public string Attack(Unit enemy)
        {
            // Вычисление урона.
            double damage = minDmg * baseStr / 20;
            if (enemy.Health > damage)
                enemy.Health -= damage;
            else
                enemy.Health = 0;
            return $"{name} Нанес {damage} Урона {enemy.name}";
        }
        /// <summary>
        /// Информация о прохоме героя по герою
        /// </summary>
        /// <param name="enemy">Ссылка на противника</param>
        /// <returns>Сообщение о промохе</returns>
        public string Missed(Unit enemy)
        {
            return $"{name} не попал по {enemy.name}";
        }
        /// <summary>
        /// Метод, отдающий процент хп персонажа.
        /// </summary>
        /// <returns></returns>
        public double HealtNow()
        {
            if (MaxHealth != 0)
            return 100 * (Health / MaxHealth);
            return 0;
        }
        /// <summary>
        /// Метод,возвращающий максимальное и нынешнее хп персонажа
        /// </summary>
        /// <returns>Строка с хп</returns>
        public string TostringHealth()
        {
            return $"{Health.ToString("#.##")}/{MaxHealth.ToString("#.##")}";
        }
        /// <summary>
        /// Метод,проверяющий,умер ли кто из героев.
        /// </summary>
        /// <param name="defender"></param>
        /// <returns></returns>
        public Unit checkHealth(Unit defender)
        {
            // Если никто не умер,возрат null
            if (Health == 0)
                return this;
            if (defender.Health == 0)
                return defender;
            return null;
        }
        /// <summary>
        /// Метод,вычитающий 1% из героя,который бежал с поле боя.
        /// </summary>
        public void TaxeForEscape()
        {
            // Расчет 1% макс хп.
            double xhmin = MaxHealth / 100;
            //Хп не может уменьшиться ниже 2.
            if (Health - xhmin >= 2)
                Health -= xhmin;
            else if (Health > 2)
                Health = 2;
        }
        /// <summary>
        /// Метод,расчитывающий хп,которое отрегенирирует себе герой
        /// </summary>
        /// <returns>Строка с информацией</returns>
        public string HPregeneration()
        {
            // Расчет хп регена.
            double hpRegen = 5 * regeneration;
            if (rnd.NextDouble() <= 0.2)
            {
                // Если хп+реген меньше макс хп, то брибавляем,
                // Иначе если хп меньше макс хп,делаем хп=макс.хп
                if (Health + hpRegen <= MaxHealth)
                {
                    Health += hpRegen;
                    return $"{name} восстановил {hpRegen} здоровья";

                }
                else if (Health < MaxHealth)
                    Health = MaxHealth;
                return $"{name} восстанвоил здоровье до максимума";
            }
            return $"{name} Не смог восстановить здоровье";
        }
    }
}
