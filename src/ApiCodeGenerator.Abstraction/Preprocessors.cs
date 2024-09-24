using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ApiCodeGenerator.Abstraction
{
    /// <summary>
    /// Перечень препроцессоров для различных типов документов.
    /// </summary>
    public class Preprocessors : ReadOnlyDictionary<Type, Delegate[]>
    {
        public Preprocessors(IDictionary<Type, Delegate[]> dictionary)
            : base(dictionary)
        {
        }
    }
}
