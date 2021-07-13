using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordTokenChecker_by_wDude
{
    class JsonDeserialize // Класс дессериализации Json, который приходит ответом от сервера
    {
        public string email { get; set; } // Почта профиля
        public string phone { get; set; } // Телефон профиля
        public bool verified { get; set; } // Верификация почты
        public int premium_type { get; set; } // Тип премиума, или же нитро классик, нитро фулл, без нитро

        
    }
}
