using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using WMPLib;
using System.IO;
using System.Data.Sql;
using MyExeptions;


namespace Game_Dota2
{
    class GameLogic
    {
        // Герой пользователя.
        public Unit Hero;
        // Герой бота.
        public Unit bot;
        static Random rnd = new Random();
        // Строка с результатом одного раунда.
        string Result;



        internal GameLogic(string[] hero, string[] bot)
        {
            // Инициализация героев.
            Hero = new Unit(hero);
            this.bot = new Unit(bot);
        }
        /// <summary>
        /// Метод,вовращающий информацию о раунде.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Result;
        }
        /// <summary>
        /// Один из кейсов битвы-"Атака-Скрытие"
        /// </summary>
        /// <param name="ataker">Нападающий</param>
        /// <param name="defender">Защищающийся</param>
        private void AttackAndEscape(Unit ataker, Unit defender)
        {
            // Запись информации о бое.
            Result = $"{ataker.name} выбрал Атаку, а {defender.name} решил скрыться \n";
            // РАсчет очков для каждого героя.
            double pointsAttaker = ataker.Points();
            double pointsDefender = defender.Points();
            // Исход боя, в зависимости от набранных очков.
            if (pointsAttaker > pointsDefender)
                // Вызов метода атаки.
                Result += ataker.Attack(defender);
            else
                // Вызов метода, определающего, кто промохнулся.
                Result += ataker.Missed(defender);
        }
        /// <summary>
        /// Один из кейсов битвы-"Атака-Защита"
        /// </summary>
        /// <param name="ataker">Нападающий</param>
        /// <param name="defender">Защищающийсяparam>
        private void AttackAndDefend(Unit ataker, Unit defender)
        {
            // ЗАпись информации о бое.
            Result = $"{ataker.name} выбрал Атаку, а {defender.name} выбрал Защиту \n";
            // РАсчет очков для каждого героя.
            double pointsAttacker = ataker.Points();
            double pointsDefender = defender.Points();
            // Исход боя, в зависимости от набранных очков.
            if (pointsAttacker > pointsDefender)
                // Вызов метода атаки.
                Result += ataker.Attack(defender);
            else
                // Вызов метода, определающего, кто промохнулся.
                Result += ataker.Missed(defender);
        }
        /// <summary>
        /// Один из кейсов битвы-"Атака-Атака"
        /// </summary>
        /// <param name="attacker">Нападающий</param>
        /// <param name="defender">Защищающийся</param>
        private void AttackAndAttack(Unit attacker, Unit defender)
        {
            // ЗАпись информации о бое.
            Result = $"{attacker.name} выбрал Атаку и {defender.name} выбрал Атаку \n";
            // РАсчет очков для каждого героя.
            double pointsAttacker = attacker.Points();
            double pointsDefender = defender.Points();
            // Исход боя, в зависимости от набранных очков.
            if (pointsAttacker > pointsDefender)
                // Вызов метода атаки для нападающего.
                Result += attacker.Attack(defender);
            else
                // Вызов метода атаки для защитника.
                Result += defender.Attack(attacker);
        }
        /// <summary>
        /// Один из кейсов битвы-"Защита-Скрытие"
        /// </summary>
        /// <param name="ataker">Нападающий</param>
        /// <param name="defender">Защищающийся</param>
        private void DefendAndEscape(Unit ataker, Unit defender)
        {
            // ЗАпись информации о бое.
            Result = $"{ataker.name} выбрал Защиту, а {defender.name} попытался Скрыться \n"
                + $"За это {defender.name} теряет 1% здоровья ";
            // Вызов метода,который снимает 1% макс здоровья.
            defender.TaxeForEscape();
        }
        /// <summary>
        /// Один из кейсов битвы-"Защита-Защита"
        /// </summary>
        /// <param name="ataker">Нападающий</param>
        /// <param name="defender">Защищающийся</param>
        private void DefendAndDefend(Unit ataker, Unit defender)
        {
            // ЗАпись информации о бое.
            Result = $"{ataker.name} выбрал защиту и {defender.name} выбрал защиту \n";
            Result += "Произошло : Nothing\nА ты чего то ждал ?)";
        }
        /// <summary>
        /// Один из кейсов битвы-"Скрытие-Скрытие"
        /// </summary>
        /// <param name="ataker">Нападающий</param>
        /// <param name="defender">Защищающийся</param>
        private void EscapeAndEscape(Unit ataker, Unit defender)
        {
            // ЗАпись информации о бое.
            Result = $"{ataker.name} и {defender.name} решили Скрыться \n";
            // Вызов метода регенерации персонажа.
            Result += ataker.HPregeneration() + "\n";
            // Вызов метода регенерации персонажа.
            Result += defender.HPregeneration();
        }
        /// <summary>
        /// Метод, определяющий , какой кейс для данного раунда запустить.
        /// </summary>
        /// <param name="type">Выбор пользователя</param>
        /// <param name="You">Герой пользователя</param>
        /// <param name="Bot">Герой Бота</param>
        public void Battle(Type type, Unit You, Unit Bot)
        {
            // Если игрок выбрал Атаку.
            if (type == Type.Attack)
            {
                switch (rnd.Next(0, 3))
                {
                    case 0:
                        AttackAndEscape(You, Bot);
                        break;
                    case 1:
                        AttackAndDefend(You, Bot);
                        break;
                    case 2:
                        AttackAndAttack(You, Bot);
                        break;
                }
            }
            // Если игрок выбрал Защиту.
            if (type == Type.Defend)
            {
                switch (rnd.Next(0, 3))
                {
                    case 0:
                        DefendAndEscape(You, Bot);
                        break;
                    case 1:
                        DefendAndDefend(You, Bot);
                        break;
                    case 2:
                        AttackAndDefend(Bot, You);
                        break;
                }
            }
            // Если игрок выбрал побег.
            if (type == Type.Escape)
            {
                switch (rnd.Next(0, 3))
                {
                    case 0:
                        EscapeAndEscape(You, Bot);
                        break;
                    case 1:
                        DefendAndEscape(Bot, You);
                        break;
                    case 2:
                        AttackAndEscape(Bot, You);
                        break;
                }
            }
        }
    }
}
