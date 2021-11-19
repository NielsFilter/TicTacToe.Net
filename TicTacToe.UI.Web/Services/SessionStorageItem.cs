using System;

namespace TicTacToe.UI.Web.Services
{
    public class SessionStorageItem<T>
    {
        public DateTime TimeStamp { get; set; }
        public T Item { get; set; }
    }
}