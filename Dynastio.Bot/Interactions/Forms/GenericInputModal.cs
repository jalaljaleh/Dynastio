using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Test.Interactions.Forms
{
    public class GenericInputModal<T1> : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput("1")]
        public T1 First { get; set; }
    }
    public class GenericInputModal<T1, T2> : GenericInputModal<T1>
    {
        [ModalTextInput("2")]
        public T2 Second { get; set; }
    }
    public class GenericInputModal<T1, T2, T3> : GenericInputModal<T1, T2>
    {
        [ModalTextInput("3")]
        public T3 Third { get; set; }
    }
    public class GenericInputModal<T1, T2, T3, T4> : GenericInputModal<T1, T2, T3>
    {
        [ModalTextInput("4")]
        public T4 Fourth { get; set; }
    }
    public class GenericInputModal<T1, T2, T3, T4, T5> : GenericInputModal<T1, T2, T3, T4>
    {
        [ModalTextInput("5")]
        public T5 Fifth { get; set; }
    }
}
